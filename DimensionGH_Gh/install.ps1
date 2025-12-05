# PowerShell script to install DimensionGH_Gh plugin to Grasshopper

$ErrorActionPreference = "Stop"

# Find DLL file
$dllPath = "bin\Debug\DimensionGH_Gh.dll"
if (-not (Test-Path $dllPath)) {
    $dllPath = "bin\Release\DimensionGH_Gh.dll"
}

if (-not (Test-Path $dllPath)) {
    Write-Host "Ошибка: файл DLL не найден. Сначала соберите проект в Visual Studio!" -ForegroundColor Red
    exit 1
}

# Target directory
$targetDir = "$env:APPDATA\Grasshopper\Libraries"
$targetFile = "$targetDir\DimensionGH_Gh.gha"

# Create target directory if it doesn't exist
if (-not (Test-Path $targetDir)) {
    New-Item -ItemType Directory -Path $targetDir | Out-Null
    Write-Host "Создана папка: $targetDir" -ForegroundColor Green
}

# Copy and rename
Copy-Item $dllPath $targetFile -Force
Write-Host "Плагин установлен:" -ForegroundColor Green
Write-Host "  Из: $dllPath" -ForegroundColor Gray
Write-Host "  В:  $targetFile" -ForegroundColor Gray
Write-Host ""
Write-Host "Перезапустите Grasshopper, чтобы увидеть категорию '227info'" -ForegroundColor Yellow

