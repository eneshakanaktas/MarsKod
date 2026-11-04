@echo off
rem MarsKod: oyunu telefon boyutunda acar. Paket guncelse hemen acar; GitHub'da yeni is varsa ya da kod degistiyse once yeniden uretir.
rem Paket uretilirken Unity penceresi acik olmamali, Unity Hub acik ve giris yapilmis olmali.
cd /d "%~dp0"

rem 1) Kaydedilmis degisiklik yoksa main'e gec ve GitHub'daki yeni isleri cek. Varsa dokunma (yarim isi ezmeyelim).
git fetch origin >nul 2>&1
git diff --quiet HEAD
if errorlevel 1 goto atla_cek
git checkout main
if errorlevel 1 goto hata
git pull origin main
if errorlevel 1 goto hata
:atla_cek

rem 2) Paket guncel mi? Eksikse, koddan farkliysa ya da kod kaydedilmeden degistiyse yeniden uretilir.
set NEEDBUILD=
if not exist "Build\Win\MarsKod.exe" set NEEDBUILD=1
git diff --quiet HEAD -- Assets ProjectSettings
if errorlevel 1 set NEEDBUILD=1
for /f %%H in ('git rev-parse HEAD') do set HEADID=%%H
set BUILTID=
if exist "Build\Win\commit.txt" set /p BUILTID=<"Build\Win\commit.txt"
if not "%BUILTID%"=="%HEADID%" set NEEDBUILD=1
if not defined NEEDBUILD goto ac

rem 3) Eski paketi yedege al (uretim basarisiz olursa geri koyariz), yenisini uret.
if exist "Build\Win-eski" rmdir /s /q "Build\Win-eski"
if exist "Build\Win" move "Build\Win" "Build\Win-eski" >nul

rem Unity lisansi icin Unity Hub acik olmali (kapaliysa kendimiz aciyoruz).
tasklist /fi "imagename eq Unity Hub.exe" | find /i "Unity Hub.exe" >nul
if errorlevel 1 (
  echo Unity Hub aciliyor, 20 saniye bekleniyor...
  explorer.exe "shell:AppsFolder\UnityTechnologies.UnityHub_2vrhnee42bhxm!UnityHub"
  timeout /t 20 /nobreak >nul
)

rem Kurulu Unity 6 surumunu bul (6000.6.0f1, 6000.6.3f1 vb. hangisi varsa).
set UNITY=
for /d %%V in ("C:\Program Files\Unity\Hub\Editor\6000.6.*") do if exist "%%V\Editor\Unity.exe" set UNITY="%%V\Editor\Unity.exe"
if not defined UNITY (
  echo Unity 6 bulunamadi. Unity Hub'dan Unity 6000.6 surumunu kurun.
  pause
  exit /b 1
)
echo Paket uretiliyor, lutfen bekleyin. 10-20 dk surebilir, pencereyi kapatmayin...
%UNITY% -batchmode -quit -projectPath "%~dp0." -executeMethod OyunBuild.BuildWindows -logFile build.log

if not exist "Build\Win\MarsKod.exe" (
  echo Paket uretilemedi. Ayrinti: build.log. En sik sebep: Unity Hub'da hesaba giris yapilmamis. Hub'i ac, giris yap, tekrar dene.
  if exist "Build\Win-eski" (
    echo Eski paket geri konuyor...
    if exist "Build\Win" rmdir /s /q "Build\Win"
    move "Build\Win-eski" "Build\Win" >nul
    start "" "Build\Win\MarsKod.exe" -screen-width 450 -screen-height 975 -screen-fullscreen 0
  )
  pause
  exit /b 1
)
rem Paketin hangi kodla uretildigini yaz: sonraki acilista kod ayniysa bekletmeden acilir.
>"Build\Win\commit.txt" echo %HEADID%
if exist "Build\Win-eski" rmdir /s /q "Build\Win-eski"

:ac
rem 4) Oyunu ac.
start "" "Build\Win\MarsKod.exe" -screen-width 450 -screen-height 975 -screen-fullscreen 0
exit /b 0

:hata
echo Git hata verdi. Ekrandaki mesaji Claude'a goster.
pause
exit /b 1
