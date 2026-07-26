@echo off
echo Converting ROM files to C headers...
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0convert_roms.ps1"
if %errorlevel% neq 0 (
    echo Error: ROM conversion failed!
    exit /b %errorlevel%
)
echo ROM conversion completed!
