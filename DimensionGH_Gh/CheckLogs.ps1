# Script to check Grasshopper logs for plugin loading errors

Write-Host "=== Проверка логов Grasshopper ===" -ForegroundColor Cyan
Write-Host ""

$logPath = "$env:APPDATA\Grasshopper\Logs"
$libraryPath = "$env:APPDATA\Grasshopper\Libraries\DimensionGH_Gh.gha"

Write-Host "1. Проверка файла плагина:" -ForegroundColor Yellow
if (Test-Path $libraryPath) {
    $fileInfo = Get-Item $libraryPath
    Write-Host "   ✓ Файл найден: $libraryPath" -ForegroundColor Green
    Write-Host "   Размер: $($fileInfo.Length) байт" -ForegroundColor Gray
    Write-Host "   Дата изменения: $($fileInfo.LastWriteTime)" -ForegroundColor Gray
} else {
    Write-Host "   ✗ Файл НЕ найден: $libraryPath" -ForegroundColor Red
}
Write-Host ""

Write-Host "2. Проверка логов Grasshopper:" -ForegroundColor Yellow
if (Test-Path $logPath) {
    Write-Host "   ✓ Папка логов найдена: $logPath" -ForegroundColor Green
    
    $logFiles = Get-ChildItem -Path $logPath -Filter "*.log" -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 5
    
    if ($logFiles) {
        Write-Host "   Найдено лог-файлов: $($logFiles.Count)" -ForegroundColor Gray
        Write-Host ""
        Write-Host "   Последние логи:" -ForegroundColor Cyan
        foreach ($log in $logFiles) {
            Write-Host "   - $($log.Name) ($($log.LastWriteTime))" -ForegroundColor Gray
            
            # Ищем ошибки связанные с DimensionGH
            $content = Get-Content $log.FullName -ErrorAction SilentlyContinue
            $errors = $content | Select-String -Pattern "DimensionGH|DimensionGh|Error|Exception" -CaseSensitive:$false
            
            if ($errors) {
                Write-Host "     ⚠ Найдены ошибки/упоминания:" -ForegroundColor Yellow
                $errors | Select-Object -First 3 | ForEach-Object {
                    Write-Host "       $($_.Line.Trim())" -ForegroundColor Red
                }
            }
        }
    } else {
        Write-Host "   ⚠ Лог-файлы не найдены" -ForegroundColor Yellow
    }
} else {
    Write-Host "   ⚠ Папка логов не найдена: $logPath" -ForegroundColor Yellow
}
Write-Host ""

Write-Host "3. Как посмотреть логи в Grasshopper:" -ForegroundColor Yellow
Write-Host "   - Откройте Grasshopper" -ForegroundColor White
Write-Host "   - File → Special Folders → User Object Folder" -ForegroundColor White
Write-Host "   - Или проверьте папку: $logPath" -ForegroundColor White
Write-Host ""

Write-Host "4. Проверка зависимостей:" -ForegroundColor Yellow
$dllPath = "bin\Debug\DimensionGH_Gh.dll"
if (-not (Test-Path $dllPath)) {
    $dllPath = "bin\Release\DimensionGH_Gh.dll"
}

if (Test-Path $dllPath) {
    Write-Host "   Проверка сборки..." -ForegroundColor Gray
    # Можно добавить проверку зависимостей через ILSpy или подобные инструменты
    Write-Host "   ✓ DLL найден: $dllPath" -ForegroundColor Green
} else {
    Write-Host "   ✗ DLL не найден. Соберите проект!" -ForegroundColor Red
}

