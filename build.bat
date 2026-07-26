@echo off
if not exist obj mkdir obj
if not exist "%~dp0game" mkdir "%~dp0game"

:: Ensure the DLL is in the game directory
if exist "%~dp0libs\freeglut.dll" copy /y "%~dp0libs\freeglut.dll" "%~dp0game\" >nul

if not exist "%~dp0compiler\tcc.exe" goto fallback

echo Building with portable Tiny C Compiler (TCC)...
"%~dp0compiler\tcc.exe" -O2 -I"%~dp0include" -I"%~dp0include\roms" pacman.c cpu.c video.c sound.c kbd.c harness.c -Llibs -lfreeglut -lopengl32 -lwinmm -o "%~dp0game\pacman.exe"
goto end

:fallback
echo Portable TCC compiler not found [compiler\tcc.exe is missing].
echo Falling back to MSVC compiler [cl.exe]...
echo.

if defined VCINSTALLDIR goto do_cl
if not exist "C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvarsall.bat" goto do_cl
call "C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvarsall.bat" x64

:do_cl
cl.exe /MD /O2 /Iinclude /Iinclude\roms pacman.c cpu.c video.c sound.c kbd.c harness.c /Foobj\ /link /LIBPATH:libs freeglut.lib opengl32.lib winmm.lib /DELAYLOAD:freeglut.dll delayimp.lib /out:"%~dp0game\pacman.exe"

:end
