@echo off
rem MarsKod Godot denemesini bilgisayarda açar (Godot 4.7 kurulu olmalı: winget install GodotEngine.GodotEngine)
cd /d "%~dp0"
godot --path .
if errorlevel 1 "%LOCALAPPDATA%\Microsoft\WinGet\Links\godot.exe" --path .
