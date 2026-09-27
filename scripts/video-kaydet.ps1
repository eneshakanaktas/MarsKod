# Unity taslağının ya da oyunun kısa videosunu çeker (gelişim süreci sayfası için).
# Oyunu ekran görüntüsü modunda (-shots) açar; bu mod "Çalıştır"a kendisi basar ve gösteri bitince kapanır.
# Pencere en öne alınır ve ekranın o bölgesi kaydedilir (Unity penceresinin içi doğrudan okunamıyor).
# Kayıt sırasında pencerenin önüne başka bir şey gelmemeli. Başta Unity logosu, sonda masaüstü görünür:
# ikisi de kesilmeli (masaüstünde kişisel bilgiler görünebilir).
# Gerekli: py -3.12 -m pip install --user imageio-ffmpeg
# Örnek: powershell -File scripts/video-kaydet.ps1 -Exe oyun\Build\Win\MarsKod.exe -Out ham.mp4 -Seconds 18
param([string]$Exe, [string]$Out, [double]$Seconds = 12.5)
Add-Type @"
using System; using System.Runtime.InteropServices;
public class W {
  [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
  [DllImport("user32.dll")] public static extern bool GetClientRect(IntPtr h, out RECT r);
  [DllImport("user32.dll")] public static extern bool ClientToScreen(IntPtr h, ref POINT p);
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
  [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr h, IntPtr after, int x, int y, int cx, int cy, uint flags);
  public struct RECT { public int L, T, R, B; }
  public struct POINT { public int X, Y; }
}
"@
[W]::SetProcessDPIAware() | Out-Null
$ff = & py -3.12 -c "import imageio_ffmpeg as f; print(f.get_ffmpeg_exe())"
$shots = Join-Path $env:TEMP ("shots-" + [guid]::NewGuid())
$p = Start-Process -FilePath $Exe -ArgumentList @("-screen-width", "450", "-screen-height", "975", "-screen-fullscreen", "0", "-shots", $shots) -PassThru
$h = [IntPtr]::Zero
for ($i = 0; $i -lt 200 -and $h -eq [IntPtr]::Zero; $i++) { Start-Sleep -Milliseconds 50; $p.Refresh(); $h = $p.MainWindowHandle }
# pencere son boyutuna gelene kadar bekle
$r = New-Object W+RECT
for ($i = 0; $i -lt 100; $i++) { [W]::GetClientRect($h, [ref]$r) | Out-Null; if (($r.B - $r.T) -gt 900) { break }; Start-Sleep -Milliseconds 30 }
Start-Sleep -Milliseconds 800
# en öne al (-1 = hep üstte) ve ekranın sol üstüne yakın taşı; boyut değişmez
[W]::SetWindowPos($h, [IntPtr](-1), 40, 10, 0, 0, 0x0001) | Out-Null
[W]::SetForegroundWindow($h) | Out-Null
Start-Sleep -Milliseconds 200
[W]::GetClientRect($h, [ref]$r) | Out-Null
$pt = New-Object W+POINT
[W]::ClientToScreen($h, [ref]$pt) | Out-Null
$w = [int](($r.R - $r.L) / 2) * 2; $hh = [int](($r.B - $r.T) / 2) * 2
& $ff -y -loglevel error -f gdigrab -framerate 30 -draw_mouse 0 -offset_x $pt.X -offset_y $pt.Y -video_size "${w}x${hh}" -t $Seconds -i desktop -c:v libx264 -preset veryfast -crf 16 -pix_fmt yuv420p $Out
Start-Sleep -Seconds 2
if (-not $p.HasExited) { Stop-Process -Id $p.Id }
"kaydedildi: $Out ($($pt.X),$($pt.Y) ${w}x${hh})"
