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

# Copy Newtonsoft.Json.dll if it exists
$jsonDllPath = Join-Path (Split-Path $dllPath) "Newtonsoft.Json.dll"
if (Test-Path $jsonDllPath) {
    $jsonTargetFile = "$targetDir\Newtonsoft.Json.dll"
    Copy-Item $jsonDllPath $jsonTargetFile -Force
    Write-Host "Newtonsoft.Json.dll скопирован:" -ForegroundColor Green
    Write-Host "  В:  $jsonTargetFile" -ForegroundColor Gray
} else {
    Write-Host "Предупреждение: Newtonsoft.Json.dll не найден в папке сборки!" -ForegroundColor Yellow
    Write-Host "  Убедитесь, что он скопирован вручную в: $targetDir" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Перезапустите Grasshopper, чтобы увидеть категорию 'Dimension Gh'" -ForegroundColor Yellow

