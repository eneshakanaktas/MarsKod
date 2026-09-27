@echo off
rem MarsKod oyununu bilgisayarda telefon boyutunda acar. Paket yoksa once uretir (birkac dakika).
cd /d "%~dp0"
rem Kurulu Unity 6 surumunu bul (6000.6.0f1, 6000.6.3f1 vb. hangisi varsa).
set UNITY=
for /d %%V in ("C:\Program Files\Unity\Hub\Editor\6000.6.*") do if exist "%%V\Editor\Unity.exe" set UNITY="%%V\Editor\Unity.exe"
if not exist "Build\Win\MarsKod.exe" (
  if not defined UNITY (
    echo Unity 6 bulunamadi. Unity Hub'dan Unity 6000.6 surumunu kurun.
    pause
    exit /b 1
  )
  echo Paket uretiliyor, lutfen bekleyin...
  %UNITY% -batchmode -quit -projectPath "%~dp0." -executeMethod OyunBuild.BuildWindows -logFile build.log
)
if not exist "Build\Win\MarsKod.exe" (
  echo Paket uretilemedi. Ayrinti: build.log
  pause
  exit /b 1
)
start "" "Build\Win\MarsKod.exe" -screen-width 450 -screen-height 975 -screen-fullscreen 0
