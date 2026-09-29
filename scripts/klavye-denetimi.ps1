# Bilgisayar klavyesi denetimi: oyunu acar, kod alanina Windows tus mesajlariyla yazar, sonucu log'dan okur.
# Unity klavye harflerini Windows'un pencereye gonderdigi mesajlardan (WM_KEYDOWN / WM_CHAR) okur; bu betik onlari gonderir.
# Kullanim: powershell -ExecutionPolicy Bypass -File scripts/klavye-denetimi.ps1 [-Klasor <goruntu klasoru>]
# Sonuc: "KLAVYE DENETIMI: TAMAM" (ya da FARKLI + yazilan kod). Goruntuler <klasor>'de (klavye.png dahil).
param([string]$Klasor = "$env:TEMP\marskod-klavye")

$ErrorActionPreference = "Stop"
$kok = Split-Path -Parent $PSScriptRoot
$exe = Join-Path $kok "oyun\Build\Win\MarsKod.exe"
if (-not (Test-Path $exe)) { throw "Once Windows paketi uretilmeli: $exe" }
New-Item -ItemType Directory -Force $Klasor | Out-Null
$hazir = Join-Path $Klasor "klavye-hazir.txt"
$bitti = Join-Path $Klasor "klavye-bitti.txt"
$log = Join-Path $Klasor "oyun.log"
foreach ($f in @($hazir, $bitti)) { if (Test-Path $f) { Remove-Item $f } }

Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class Tus {
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern bool PostMessageW(IntPtr h, uint msg, IntPtr w, IntPtr l);
}
"@
$WM_KEYDOWN = 0x100; $WM_KEYUP = 0x101; $WM_CHAR = 0x102

$oyun = Start-Process $exe -ArgumentList @("-screen-width", "450", "-screen-height", "975", "-screen-fullscreen", "0",
    "-logFile", "`"$log`"", "-shots", "`"$Klasor`"", "-klavyedenetimi") -PassThru

# Oyun koda tiklayip hazir dosyasini yazana kadar bekle
$son = (Get-Date).AddMinutes(4)
while (-not (Test-Path $hazir)) {
    if ((Get-Date) -gt $son -or $oyun.HasExited) { throw "Oyun klavye denetimine gelmedi" }
    Start-Sleep -Milliseconds 300
}
$oyun.Refresh()
$h = $oyun.MainWindowHandle

function Bas([int]$vk, [int]$ch = 0) {
    [Tus]::PostMessageW($h, $WM_KEYDOWN, [IntPtr]$vk, [IntPtr]1) | Out-Null
    if ($ch -ne 0) { [Tus]::PostMessageW($h, $WM_CHAR, [IntPtr]$ch, [IntPtr]1) | Out-Null }
    Start-Sleep -Milliseconds 60
    [Tus]::PostMessageW($h, $WM_KEYUP, [IntPtr]$vk, [IntPtr]0xC0000001) | Out-Null
    Start-Sleep -Milliseconds 60
}
function Yaz([string]$s) {
    foreach ($c in $s.ToCharArray()) {
        [Tus]::PostMessageW($h, $WM_CHAR, [IntPtr][int]$c, [IntPtr]1) | Out-Null
        Start-Sleep -Milliseconds 40
    }
}
$ENTER = 0x0D; $GERI = 0x08; $TAB = 0x09; $BAS = 0x24; $END = 0x23; $SOL = 0x25

# Oyunun bekledigi sira (Oyun.cs KeyboardCheck ile ayni)
Bas $ENTER 13
Yaz "for i in range(2):"
Bas $ENTER 13
Yaz "move(East))"
Bas $GERI 8
Bas $BAS
Bas $GERI 8
Bas $TAB 9
Bas $END
Bas $ENTER 13
Yaz ("# " + [char]0xE7 + [char]0x11F + [char]0x131 + [char]0x15F + " {x}") # "# çğış {x}" (dosya ASCII kalsin)
Bas $SOL
Yaz "y"
Set-Content -Path $bitti -Value "bitti"

$oyun.WaitForExit(240000) | Out-Null
Select-String -Path $log -Pattern "KLAVYE DENETIMI" | ForEach-Object { $_.Line }
