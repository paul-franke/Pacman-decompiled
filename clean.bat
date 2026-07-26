@echo off
echo ===================================================
echo Pacman C - Cleanup Script
echo ===================================================
echo.

set "SCRIPT_DIR=%~dp0"

echo Cleaning object files and executables...
if exist "%SCRIPT_DIR%obj" rd /s /q "%SCRIPT_DIR%obj"
if exist "%SCRIPT_DIR%pacman.exe" del /f /q "%SCRIPT_DIR%pacman.exe"
if exist "%SCRIPT_DIR%test.exe" del /f /q "%SCRIPT_DIR%test.exe"
if exist "%SCRIPT_DIR%game" rd /s /q "%SCRIPT_DIR%game"

echo Cleaning generated ROM header files...
if exist "%SCRIPT_DIR%include\roms" rd /s /q "%SCRIPT_DIR%include\roms"

echo Cleaning downloaded libraries and GL headers...
if exist "%SCRIPT_DIR%freeglut.dll" del /f /q "%SCRIPT_DIR%freeglut.dll"
if exist "%SCRIPT_DIR%libs\freeglut.dll" del /f /q "%SCRIPT_DIR%libs\freeglut.dll"
if exist "%SCRIPT_DIR%libs\freeglut.lib" del /f /q "%SCRIPT_DIR%libs\freeglut.lib"
if exist "%SCRIPT_DIR%libs\glut32.lib" del /f /q "%SCRIPT_DIR%libs\glut32.lib"
if exist "%SCRIPT_DIR%include\GL" rd /s /q "%SCRIPT_DIR%include\GL"

echo Cleaning portable compiler...
if exist "%SCRIPT_DIR%compiler" rd /s /q "%SCRIPT_DIR%compiler"

echo.
echo ===================================================
echo Cleanup completed successfully!
echo ===================================================
pause
