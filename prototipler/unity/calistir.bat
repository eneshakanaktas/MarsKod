@echo off
rem unitytaslak1'i bilgisayarda telefon boyutunda acar. Paket yoksa once uretir (birkac dakika).
cd /d "%~dp0"
set UNITY="C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe"
if not exist "Build\Win\MarsKod.exe" (
  echo Paket uretiliyor, lutfen bekleyin...
  %UNITY% -batchmode -quit -projectPath "%~dp0." -executeMethod TaslakBuild.BuildWindows -logFile build.log
)
start "" "Build\Win\MarsKod.exe" -screen-width 450 -screen-height 975 -screen-fullscreen 0
