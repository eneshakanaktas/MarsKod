@echo off
rem MarsKod: GitHub'daki EN GUNCEL oyunu (main dali) ceker, paketi yeniden uretir ve telefon boyutunda acar.
rem Unity penceresi aciksa paket uretilemez: once Unity'yi kapat.
cd /d "%~dp0"

rem 1) Kaydedilmemis degisiklik varsa dur (yanlislikla ezmeyelim).
git diff --quiet
if errorlevel 1 goto kirli
git diff --cached --quiet
if errorlevel 1 goto kirli

rem 2) main dalina gec ve guncelle.
git fetch origin
git checkout main
if errorlevel 1 goto hata
git pull origin main
if errorlevel 1 goto hata

rem 3) Eski paketi sil, yenisini uret.
if exist "Build\Win" rmdir /s /q "Build\Win"

rem Kurulu Unity 6 surumunu bul (6000.6.0f1, 6000.6.3f1 vb. hangisi varsa).
set UNITY=
for /d %%V in ("C:\Program Files\Unity\Hub\Editor\6000.6.*") do if exist "%%V\Editor\Unity.exe" set UNITY="%%V\Editor\Unity.exe"
if not defined UNITY (
  echo Unity 6 bulunamadi. Unity Hub'dan Unity 6000.6 surumunu kurun.
  pause
  exit /b 1
)
echo Paket uretiliyor, lutfen bekleyin (dal degisince ilk seferde 10-20 dk surebilir)...
%UNITY% -batchmode -quit -projectPath "%~dp0." -executeMethod OyunBuild.BuildWindows -logFile build.log

if not exist "Build\Win\MarsKod.exe" (
  echo Paket uretilemedi. Ayrinti: build.log  ^(Unity acik olabilir, kapatip tekrar dene^)
  pause
  exit /b 1
)

rem 4) Oyunu ac.
start "" "Build\Win\MarsKod.exe" -screen-width 450 -screen-height 975 -screen-fullscreen 0
exit /b 0

:kirli
echo Bu klasorde kaydedilmemis degisiklik var. Once commit et ya da Claude'a "kaydet" de.
pause
exit /b 1

:hata
echo Git hata verdi. Ekrandaki mesaji Claude'a goster.
pause
exit /b 1
