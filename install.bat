@echo off
setlocal enabledelayedexpansion

echo ===================================================
echo Pacman C - Installation and ROM Preparation Script
echo ===================================================
echo.

set "SCRIPT_DIR=%~dp0"

:: 1. Check for freeglut libraries and headers
echo Checking for required freeglut libraries and headers...
set "LIBS_MISSING=0"
if not exist "%SCRIPT_DIR%libs\freeglut.dll" set "LIBS_MISSING=1"
if not exist "%SCRIPT_DIR%libs\freeglut.lib" set "LIBS_MISSING=1"
if not exist "%SCRIPT_DIR%include\GL\glut.h" set "LIBS_MISSING=1"

if "%LIBS_MISSING%"=="1" (
    :: Try local fallback first
    if exist "D:\repos\cuda-samples\bin\win64\Release\freeglut.dll" (
        echo Found local FreeGLUT fallback in cuda-samples. Copying files...
        if not exist "%SCRIPT_DIR%libs" mkdir "%SCRIPT_DIR%libs"
        if not exist "%SCRIPT_DIR%include\GL" mkdir "%SCRIPT_DIR%include\GL"
        copy /y "D:\repos\cuda-samples\bin\win64\Release\freeglut.dll" "%SCRIPT_DIR%libs\" >nul
        copy /y "D:\repos\cuda-samples\Common\lib\x64\freeglut.lib" "%SCRIPT_DIR%libs\" >nul
        copy /y "D:\repos\cuda-samples\Common\lib\x64\freeglut.lib" "%SCRIPT_DIR%libs\glut32.lib" >nul
        copy /y "D:\repos\cuda-samples\Common\GL\*" "%SCRIPT_DIR%include\GL\" >nul
        echo FreeGLUT libraries and headers configured from local fallback.
    ) else (
        echo FreeGLUT libraries or headers not present. Downloading...
        
        :: Create directories if they don't exist
        if not exist "%SCRIPT_DIR%libs" mkdir "%SCRIPT_DIR%libs"
        if not exist "%SCRIPT_DIR%include\GL" mkdir "%SCRIPT_DIR%include\GL"
        
        :: Download FreeGLUT with fallback
        echo Downloading FreeGLUT from TransmissionZero...
        powershell -Command "try { Start-BitsTransfer -Source 'https://www.transmissionzero.co.uk/files/software/development/GL/freeglut-MSVC.zip' -Destination '%SCRIPT_DIR%freeglut-MSVC.zip' -ErrorAction Stop } catch { [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; Invoke-WebRequest -Uri 'https://www.transmissionzero.co.uk/files/software/development/GL/freeglut-MSVC.zip' -OutFile '%SCRIPT_DIR%freeglut-MSVC.zip' }"
        
        if not exist "%SCRIPT_DIR%freeglut-MSVC.zip" (
            echo Error: Failed to download freeglut-MSVC.zip
            exit /b 1
        )
        
        :: Extract FreeGLUT
        echo Extracting FreeGLUT...
        if exist "%SCRIPT_DIR%freeglut-MSVC" rd /s /q "%SCRIPT_DIR%freeglut-MSVC"
        powershell -Command "Expand-Archive -Path '%SCRIPT_DIR%freeglut-MSVC.zip' -DestinationPath '%SCRIPT_DIR%freeglut-MSVC' -Force"
        
        :: Copy files
        echo Copying library and header files...
        if exist "%SCRIPT_DIR%freeglut-MSVC\freeglut\bin\x64\freeglut.dll" (
            copy /y "%SCRIPT_DIR%freeglut-MSVC\freeglut\bin\x64\freeglut.dll" "%SCRIPT_DIR%libs\" >nul
        ) else (
            echo Error: freeglut.dll x64 not found in zip!
            exit /b 1
        )
        
        if exist "%SCRIPT_DIR%freeglut-MSVC\freeglut\lib\x64\freeglut.lib" (
            copy /y "%SCRIPT_DIR%freeglut-MSVC\freeglut\lib\x64\freeglut.lib" "%SCRIPT_DIR%libs\" >nul
            copy /y "%SCRIPT_DIR%freeglut-MSVC\freeglut\lib\x64\freeglut.lib" "%SCRIPT_DIR%libs\glut32.lib" >nul
        ) else (
            echo Error: freeglut.lib x64 not found in zip!
            exit /b 1
        )
        
        if exist "%SCRIPT_DIR%freeglut-MSVC\freeglut\include\GL" (
            copy /y "%SCRIPT_DIR%freeglut-MSVC\freeglut\include\GL\*" "%SCRIPT_DIR%include\GL\" >nul
        ) else (
            echo Error: freeglut headers not found in zip!
            exit /b 1
        )
        
        :: Cleanup
        echo Cleaning up temporary download files...
        del /f /q "%SCRIPT_DIR%freeglut-MSVC.zip"
        rd /s /q "%SCRIPT_DIR%freeglut-MSVC"
        echo FreeGLUT libraries and headers installed successfully!
    )
) else (
    echo FreeGLUT libraries and headers are already present.
)
echo.

:: Ensure game directory exists and contains the DLL
if not exist "%SCRIPT_DIR%game" mkdir "%SCRIPT_DIR%game"
if exist "%SCRIPT_DIR%libs\freeglut.dll" (
    copy /y "%SCRIPT_DIR%libs\freeglut.dll" "%SCRIPT_DIR%game\" >nul
)

:: 1.5 Check for OpenGL GL.h and GLU.h
echo Checking for OpenGL headers GL.h and GLU.h...
set "GL_HEADERS_MISSING=0"
if not exist "%SCRIPT_DIR%include\GL\gl.h" set "GL_HEADERS_MISSING=1"
if not exist "%SCRIPT_DIR%include\GL\glu.h" set "GL_HEADERS_MISSING=1"

if "%GL_HEADERS_MISSING%"=="1" (
    echo OpenGL headers missing. Downloading from MinGW-w64...
    if not exist "%SCRIPT_DIR%include\GL" mkdir "%SCRIPT_DIR%include\GL"
    powershell -Command "try { Start-BitsTransfer -Source 'https://raw.githubusercontent.com/mingw-w64/mingw-w64/master/mingw-w64-headers/include/GL/gl.h' -Destination '%SCRIPT_DIR%include\GL\gl.h' -ErrorAction Stop } catch { Invoke-WebRequest -Uri 'https://raw.githubusercontent.com/mingw-w64/mingw-w64/master/mingw-w64-headers/include/GL/gl.h' -OutFile '%SCRIPT_DIR%include\GL\gl.h' }"
    powershell -Command "try { Start-BitsTransfer -Source 'https://raw.githubusercontent.com/mingw-w64/mingw-w64/master/mingw-w64-headers/include/GL/glu.h' -Destination '%SCRIPT_DIR%include\GL\glu.h' -ErrorAction Stop } catch { Invoke-WebRequest -Uri 'https://raw.githubusercontent.com/mingw-w64/mingw-w64/master/mingw-w64-headers/include/GL/glu.h' -OutFile '%SCRIPT_DIR%include\GL\glu.h' }"
    if exist "%SCRIPT_DIR%include\GL\gl.h" (
        echo OpenGL headers downloaded successfully.
    ) else (
        echo Error: Failed to download OpenGL headers!
        exit /b 1
    )
) else (
    echo OpenGL headers are already present.
)
echo.

:: Patch OpenGL/FreeGLUT headers for compiler compatibility
echo Patching OpenGL and GLUT headers for compatibility...
powershell -ExecutionPolicy Bypass -File "%SCRIPT_DIR%patch_headers.ps1"
echo.

:: 2. Check for portable TCC compiler
echo Checking for portable Tiny C Compiler (TCC)...
set "COMPILER_MISSING=0"
if not exist "%SCRIPT_DIR%compiler\tcc.exe" set "COMPILER_MISSING=1"

if "%COMPILER_MISSING%"=="1" (
    echo Portable TCC compiler not present. Starting installation...
    
    echo Downloading TCC 0.9.27 from Savannah...
    powershell -Command "try { Start-BitsTransfer -Source 'https://download.savannah.gnu.org/releases/tinycc/tcc-0.9.27-win64-bin.zip' -Destination '%SCRIPT_DIR%tcc.zip' -ErrorAction Stop } catch { Invoke-WebRequest -Uri 'https://download.savannah.gnu.org/releases/tinycc/tcc-0.9.27-win64-bin.zip' -OutFile '%SCRIPT_DIR%tcc.zip' }"
    
    if not exist "%SCRIPT_DIR%tcc.zip" (
        echo Error: Failed to download tcc.zip
        exit /b 1
    )
    
    echo Extracting TCC...
    if exist "%SCRIPT_DIR%tcc_tmp" rd /s /q "%SCRIPT_DIR%tcc_tmp"
    powershell -Command "Expand-Archive -Path '%SCRIPT_DIR%tcc.zip' -DestinationPath '%SCRIPT_DIR%tcc_tmp' -Force"
    
    if not exist "%SCRIPT_DIR%tcc_tmp\tcc" (
        echo Error: Extraction failed!
        del /f /q "%SCRIPT_DIR%tcc.zip"
        exit /b 1
    )
    
    if exist "%SCRIPT_DIR%compiler" rd /s /q "%SCRIPT_DIR%compiler"
    if not exist "%SCRIPT_DIR%compiler" mkdir "%SCRIPT_DIR%compiler"
    xcopy /e /y /q "%SCRIPT_DIR%tcc_tmp\tcc\*" "%SCRIPT_DIR%compiler" >nul
    
    echo Cleaning up temporary download files...
    del /f /q "%SCRIPT_DIR%tcc.zip"
    rd /s /q "%SCRIPT_DIR%tcc_tmp"
    
    if exist "%SCRIPT_DIR%compiler\tcc.exe" (
        echo TCC compiler installed successfully in compiler/
    ) else (
        echo Error: Verification failed. compiler\tcc.exe not found!
        exit /b 1
    )
) else (
    echo Portable TCC compiler is already present.
)
echo.

:: 3. Run ROM conversion step
echo Preparing ROM header files...
set "ROM_DIR="
if exist "%SCRIPT_DIR%rom" set "ROM_DIR=%SCRIPT_DIR%rom"
if exist "%SCRIPT_DIR%roms" set "ROM_DIR=%SCRIPT_DIR%roms"

if defined ROM_DIR (
    pushd "%SCRIPT_DIR%"
    call convert_roms.bat
    popd
    if !errorlevel! neq 0 (
        echo Error: ROM conversion failed!
        exit /b !errorlevel!
    )
    echo ROM headers generated successfully in include/roms/
) else (
    echo Error: 'rom' or 'roms' directory not found! Please place your ROM files in the 'rom' or 'roms' directory.
    exit /b 1
)

echo.
echo ===================================================
echo Installation and setup completed successfully!
echo You can now build using build.bat
echo ===================================================
pause
