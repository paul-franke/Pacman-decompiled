$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$romDir = Join-Path $scriptDir "rom"
if (-not (Test-Path $romDir)) {
    $romDir = Join-Path $scriptDir "roms"
}
$outDir = Join-Path $scriptDir "include\roms"

if (-not (Test-Path $romDir)) {
    Write-Error "Error: 'rom' or 'roms' directory not found!"
    exit 1
}

if (-not (Test-Path $outDir)) {
    New-Item -ItemType Directory -Force $outDir | Out-Null
}

$romFiles = @(
    'pacman.5e', 'pacman.5f', 'pacman.6e', 'pacman.6f', 'pacman.6h', 'pacman.6j',
    '82s123.7f', '82s126.4a', '82s126.1m', '82s126.3m'
)

foreach ($f in $romFiles) {
    $romPath = Join-Path $romDir $f
    if (-not (Test-Path $romPath)) {
        Write-Error "Error: Required ROM file $f not found in $romDir!"
        exit 1
    }
    
    $varName = 'rom_' + $f.Replace('.', '_')
    $bytes = [System.IO.File]::ReadAllBytes($romPath)
    $hexBytes = $bytes | ForEach-Object { '0x{0:x2}' -f $_ }
    $lines = @()
    for ($i = 0; $i -lt $hexBytes.Count; $i += 12) {
        $end = [Math]::Min($i + 11, $hexBytes.Count - 1)
        $chunk = ($hexBytes[$i..$end]) -join ', '
        if ($i + 12 -lt $hexBytes.Count) { $chunk = $chunk + ',' }
        $lines += '  ' + $chunk
    }
    $headerContent = 'unsigned char ' + $varName + '[] = {' + [Environment]::NewLine + ($lines -join [Environment]::NewLine) + [Environment]::NewLine + '};' + [Environment]::NewLine + 'unsigned int ' + $varName + '_len = ' + $bytes.Count + ';' + [Environment]::NewLine
    $headerPath = Join-Path $outDir ($f + '.h')
    [System.IO.File]::WriteAllText($headerPath, $headerContent)
    Write-Host "Generated $headerPath"
}
