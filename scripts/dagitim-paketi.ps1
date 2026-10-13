# Windows paketini (oyun/Build/Win) Unity'siz acilabilen tek bir zip'e cevirir.
# Once paket uretilmis olmali (oyun/calistir.bat ya da OyunBuild.BuildWindows).
# Cikti: oyun/Build/MarsKod-Windows.zip
$kok = Split-Path $PSScriptRoot -Parent
$paket = Join-Path $kok 'oyun\Build\Win'
$zip = Join-Path $kok 'oyun\Build\MarsKod-Windows.zip'
$gecici = Join-Path $kok 'oyun\Build\MarsKod'
if (-not (Test-Path (Join-Path $paket 'MarsKod.exe'))) { throw "Paket yok: once oyun\calistir.bat ile uretin." }
if (Test-Path $gecici) { Remove-Item $gecici -Recurse -Force }
if (Test-Path $zip) { Remove-Item $zip -Force }
Copy-Item $paket $gecici -Recurse
# Unity'nin "dagitma" uyarili yedek klasoru oyuncuya gerekmez.
Get-ChildItem $gecici -Directory -Filter '*BackUpThisFolder*' | Remove-Item -Recurse -Force
Copy-Item (Join-Path $PSScriptRoot 'dagitim\MarsKod-Baslat.bat') $gecici
Compress-Archive -Path $gecici -DestinationPath $zip
Remove-Item $gecici -Recurse -Force
"{0}  ({1:N1} MB)" -f $zip, ((Get-Item $zip).Length / 1MB)
