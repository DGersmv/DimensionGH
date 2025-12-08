# Скрипт для удаления/переименования CurveComponents.gha
# Требуются права администратора!

$filePath = "C:\Program Files\Rhino 8\Plug-ins\Grasshopper\Components\CurveComponents.gha"
$newName = "CurveComponents.gha.backup_disabled"

Write-Host "Проверяем файл..." -ForegroundColor Yellow

if (Test-Path $filePath) {
    Write-Host "Файл найден: $filePath" -ForegroundColor Green
    
    # Попытка 1: Переименовать (более безопасно, чем удалять)
    Write-Host "`nПопытка переименовать файл..." -ForegroundColor Yellow
    try {
        Rename-Item -Path $filePath -NewName $newName -Force
        Write-Host "✓ Файл успешно переименован в: $newName" -ForegroundColor Green
        Write-Host "Grasshopper больше не будет загружать этот файл." -ForegroundColor Green
    }
    catch {
        Write-Host "✗ Ошибка переименования: $_" -ForegroundColor Red
        Write-Host "`nПопробуйте:" -ForegroundColor Yellow
        Write-Host "1. Закрыть Rhino полностью (все окна)" -ForegroundColor White
        Write-Host "2. Закрыть все процессы Grasshopper" -ForegroundColor White
        Write-Host "3. Подождать 5 секунд" -ForegroundColor White
        Write-Host "4. Запустить этот скрипт снова от имени администратора" -ForegroundColor White
        
        # Попытка 2: Использовать move через cmd
        Write-Host "`nПопытка через Move-Item..." -ForegroundColor Yellow
        try {
            Move-Item -Path $filePath -Destination (Join-Path (Split-Path $filePath) $newName) -Force
            Write-Host "✓ Файл успешно переименован!" -ForegroundColor Green
        }
        catch {
            Write-Host "✗ Тоже не получилось. Файл занят другим процессом." -ForegroundColor Red
            Write-Host "`nАльтернативное решение:" -ForegroundColor Yellow
            Write-Host "Закройте Rhino и запустите вручную в командной строке (от имени администратора):" -ForegroundColor White
            Write-Host "ren `"$filePath`" `"$newName`"" -ForegroundColor Cyan
        }
    }
}
else {
    Write-Host "Файл не найден по пути: $filePath" -ForegroundColor Yellow
    Write-Host "Возможно, он уже удален или переименован." -ForegroundColor Yellow
    
    # Проверим, может быть он уже переименован
    $disabledPath = "C:\Program Files\Rhino 8\Plug-ins\Grasshopper\Components\CurveComponents.gha.backup_disabled"
    if (Test-Path $disabledPath) {
        Write-Host "`n✓ Файл уже переименован/отключен: $disabledPath" -ForegroundColor Green
    }
}

Write-Host "`nНажмите любую клавишу для выхода..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")


