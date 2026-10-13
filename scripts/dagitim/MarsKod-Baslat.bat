@echo off
rem MarsKod'u telefon boyutunda bir pencerede acar. Unity kurmaya gerek yoktur.
cd /d "%~dp0"
if not exist "MarsKod.exe" (
  echo MarsKod.exe bulunamadi. Zip dosyasini once "Tumunu ayikla" ile bir klasore acin.
  pause
  exit /b 1
)
start "" "MarsKod.exe" -screen-width 450 -screen-height 975 -screen-fullscreen 0
