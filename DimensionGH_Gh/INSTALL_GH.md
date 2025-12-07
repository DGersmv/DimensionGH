# Установка плагина в Grasshopper

## Быстрая установка

1. **Соберите проект** в Visual Studio (Release или Debug)

2. **Скопируйте .dll в папку Grasshopper:**
   - Найти файл: `bin\Debug\DimensionGH_Gh.dll` (или `bin\Release\DimensionGH_Gh.dll`)
   - Скопировать в: `%AppData%\Grasshopper\Libraries\`
   - **Переименовать** в: `DimensionGH_Gh.gha`

3. **Перезапустите Grasshopper**

4. **Найдите компоненты:**
   - В Grasshopper откройте панель компонентов
   - Найдите категорию **"227info"**
   - Компонент **"Dim_Connect"** должен быть там

## Альтернативный способ (через PowerShell)

Запустите в PowerShell из папки проекта:

```powershell
$dllPath = "bin\Debug\DimensionGH_Gh.dll"
$targetDir = "$env:APPDATA\Grasshopper\Libraries"
$targetFile = "$targetDir\DimensionGH_Gh.gha"

if (Test-Path $dllPath) {
    if (-not (Test-Path $targetDir)) {
        New-Item -ItemType Directory -Path $targetDir
    }
    Copy-Item $dllPath $targetFile -Force
    Write-Host "Плагин скопирован в: $targetFile"
} else {
    Write-Host "Ошибка: файл $dllPath не найден. Сначала соберите проект!"
}
```

## Проверка установки

1. Откройте Grasshopper
2. В панели компонентов найдите категорию **"227info"**
3. Если категории нет - проверьте, что файл `.gha` находится в правильной папке
4. Если плагин не загружается - проверьте логи Grasshopper (File → Special Folders → User Object Folder)

## Устранение проблем

- **Плагин не появляется:** Убедитесь, что файл переименован в `.gha` (не `.dll`)
- **Ошибка загрузки:** Проверьте, что все зависимости (Newtonsoft.Json) доступны
- **Категория не видна:** Перезапустите Grasshopper после копирования файла

