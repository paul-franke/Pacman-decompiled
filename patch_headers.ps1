# patch_headers.ps1
# Patches third-party OpenGL headers to wrap MSVC-specific pragma directives for portability.

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

$GlutHeader = Join-Path $ScriptDir "include\GL\glut.h"
$FreeGlutHeader = Join-Path $ScriptDir "include\GL\freeglut_std.h"

if (Test-Path $GlutHeader) {
    Write-Host "Patching glut.h..."
    $content = [System.IO.File]::ReadAllText($GlutHeader)
    
    # Define targets and replacements
    $target1 = '#pragma comment (lib, "winmm.lib")     /* link with Windows MultiMedia lib */'
    $repl1   = "#ifdef _MSC_VER`r`n#pragma comment (lib, `"winmm.lib`")     /* link with Windows MultiMedia lib */"
    
    $target2 = '#pragma comment (lib, "glut32.lib")    /* link with Win32 GLUT lib */'
    $repl2   = "#pragma comment (lib, `"glut32.lib`")    /* link with Win32 GLUT lib */`r`n#endif"
    
    $content = $content.Replace($target1, $repl1)
    $content = $content.Replace($target2, $repl2)
    
    [System.IO.File]::WriteAllText($GlutHeader, $content)
}

if (Test-Path $FreeGlutHeader) {
    Write-Host "Patching freeglut_std.h..."
    $content = [System.IO.File]::ReadAllText($FreeGlutHeader)
    
    $target1 = '#    pragma comment (lib, "freeglut_static.lib")    /* link with Win32 static freeglut lib */'
    $repl1   = "#    ifdef _MSC_VER`r`n#        pragma comment (lib, `"freeglut_static.lib`")    /* link with Win32 static freeglut lib */`r`n#    endif"
    
    $target2 = '#   pragma comment (lib, "freeglut.lib")    /* link with Win32 freeglut lib */'
    $repl2   = "#   ifdef _MSC_VER`r`n#       pragma comment (lib, `"freeglut.lib`")    /* link with Win32 freeglut lib */`r`n#   endif"
    
    $target3 = '#pragma comment (lib, "winmm.lib")       /* link with Windows MultiMedia lib */'
    $repl3   = "#ifdef _MSC_VER`r`n#pragma comment (lib, `"winmm.lib`")       /* link with Windows MultiMedia lib */"
    
    $target4 = '#pragma comment (lib, "glu32.lib")       /* link with OpenGL Utility lib */'
    $repl4   = "#pragma comment (lib, `"glu32.lib`")       /* link with OpenGL Utility lib */`r`n#endif"
    
    $content = $content.Replace($target1, $repl1)
    $content = $content.Replace($target2, $repl2)
    $content = $content.Replace($target3, $repl3)
    $content = $content.Replace($target4, $repl4)
    
    [System.IO.File]::WriteAllText($FreeGlutHeader, $content)
}
