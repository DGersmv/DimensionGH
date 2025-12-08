# Инструкция по отладке плагина Grasshopper

## Как посмотреть ошибки в Grasshopper:

### Способ 1: Через меню Grasshopper
1. Откройте Grasshopper
2. Перейдите: **File → Special Folders → Components Folder**
   - Это откроет папку, где должны быть плагины
   - Проверьте, есть ли там файл `DimensionGH_Gh.gha`

### Способ 2: Через папку логов
1. Откройте проводник Windows
2. Перейдите в: `%APPDATA%\Grasshopper\Logs`
   - Или: `C:\Users\ВАШЕ_ИМЯ\AppData\Roaming\Grasshopper\Logs`
3. Найдите последние файлы `.log`
4. Откройте их в блокноте и ищите:
   - "DimensionGH"
   - "Error"
   - "Exception"
   - "Failed to load"

### Способ 3: Через консоль Rhino
1. Откройте Rhino
2. В командной строке введите: `GrasshopperDeveloperSettings`
3. Включите "Show loading messages"
4. Перезапустите Grasshopper
5. Смотрите сообщения в консоли Rhino

### Способ 4: Проверка через Windows Event Viewer
1. Откройте "Просмотр событий Windows" (Event Viewer)
2. Windows Logs → Application
3. Ищите ошибки, связанные с Grasshopper или .NET

## Проверка файла плагина:

1. Откройте: `%APPDATA%\Grasshopper\Libraries\`
2. Убедитесь, что файл называется: `DimensionGH_Gh.gha` (не `.dll`)
3. Проверьте размер файла (должен быть больше 0 байт)
4. Щелкните правой кнопкой → Свойства
   - Если есть "Разблокировать" - нажмите его

## Проверка зависимостей:

Плагин требует:
- **Newtonsoft.Json.dll** - должен быть в той же папке или в GAC
- **Grasshopper.dll** - из установки Rhino
- **RhinoCommon.dll** - из установки Rhino

Если Newtonsoft.Json не найден:
1. Скопируйте `Newtonsoft.Json.dll` из `packages\Newtonsoft.Json.13.0.3\lib\net45\`
2. В папку: `%APPDATA%\Grasshopper\Libraries\`

## Быстрая проверка:

Запустите в PowerShell:
```powershell
$file = "$env:APPDATA\Grasshopper\Libraries\DimensionGH_Gh.gha"
if (Test-Path $file) {
    Write-Host "Файл найден: $file" -ForegroundColor Green
    $info = Get-Item $file
    Write-Host "Размер: $($info.Length) байт"
    Write-Host "Дата: $($info.LastWriteTime)"
} else {
    Write-Host "Файл НЕ найден!" -ForegroundColor Red
}
```

## Если плагин все еще не загружается:

1. Удалите файл `DimensionGH_Gh.gha` из папки Libraries
2. Пересоберите проект в Visual Studio
3. Скопируйте файл заново
4. Полностью закройте Rhino (не только Grasshopper)
5. Запустите Rhino заново
6. Откройте Grasshopper

## Проверка версии Rhino:

Убедитесь, что используете Rhino 8 (как указано в путях к DLL в проекте)

## Проблема с несовместимостью версий SDK:

Если появляется ошибка "Component Library was build against a newer minor version of the Grasshopper SDK":

**Симптомы:**
- Ошибка при запуске Grasshopper
- Указывает на несоответствие версий SDK (например, Local SDK = 8.22, Referenced SDK = 8.25)

**Быстрое решение (попробуйте сначала):**
1. **Просто закройте диалог ошибки:**
   - Нажмите "Close" в диалоге ошибки
   - Grasshopper может продолжить работу несмотря на предупреждение
   - Ваш плагин DimensionGH_Gh должен работать нормально

2. **Проверьте настройки загрузки:**
   - В Rhino выполните: `_GrasshopperDeveloperSettings`
   - Или в Grasshopper: File → Preferences → Solver
   - Попробуйте изменить настройки загрузки проблемных компонентов

**Если ошибка блокирует работу:**

1. **Временно отключить проблемный компонент:**
   - Закройте Rhino полностью
   - Откройте: `C:\Program Files\Rhino 8\Plug-ins\Grasshopper\Components\`
   - Найдите проблемный `.gha` файл, который указан в сообщении об ошибке
   - Если файл существует, переименуйте его, добавив `.backup_disabled` в конец имени
   - Запустите Rhino снова
   - **Примечание:** Если файл не существует, просто закройте диалог ошибки - Grasshopper обычно продолжает работу

2. **Обновить Grasshopper:**
   - В Rhino: Help → Check for Updates
   - Убедитесь, что Grasshopper обновлен до версии, соответствующей компонентам
   - Если обновлений нет, переустановите Rhino 8 SR25 полностью

3. **Проверить версии DLL:**
   ```powershell
   (Get-Item "C:\Program Files\Rhino 8\Plug-ins\Grasshopper\Grasshopper.dll").VersionInfo.FileVersion
   ```
   Должно быть: `8.25.25328.11001` (если Rhino обновлен до SR25)


