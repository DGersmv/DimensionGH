# Быстрая проверка - Почему меню не появляется

## Шаг 1: Проверьте файл плагина

Откройте проводник и перейдите в:
```
%APPDATA%\Grasshopper\Libraries
```

Должен быть файл: **DimensionGH_Gh.gha**

Если файла нет или он называется `.dll` вместо `.gha` - это проблема!

## Шаг 2: Проверьте логи Grasshopper

### В Grasshopper:
1. **File → Special Folders → Components Folder**
   - Откроется папка с плагинами
   - Проверьте, есть ли там `DimensionGH_Gh.gha`

### В Windows:
1. Откройте: `%APPDATA%\Grasshopper\Logs`
2. Найдите последний файл `.log`
3. Откройте его и ищите:
   - "DimensionGH"
   - "Error"
   - "Exception"
   - "Failed"

## Шаг 3: Проверьте зависимости

В папке `%APPDATA%\Grasshopper\Libraries\` должны быть:
- `DimensionGH_Gh.gha` ✓
- `Newtonsoft.Json.dll` (если его нет - скопируйте из `packages\Newtonsoft.Json.13.0.3\lib\net45\`)

## Шаг 4: Проверьте блокировку файла

1. Щелкните правой кнопкой на `DimensionGH_Gh.gha`
2. **Свойства**
3. Если внизу есть "Разблокировать" - нажмите его
4. **ОК**

## Шаг 5: Перезапустите правильно

1. **Полностью закройте Rhino** (не только Grasshopper!)
2. Подождите 5 секунд
3. Запустите Rhino заново
4. Откройте Grasshopper
5. Проверьте меню

## Шаг 6: Если все еще не работает

Попробуйте:
1. Удалите `DimensionGH_Gh.gha` из папки Libraries
2. Пересоберите проект в Visual Studio
3. Скопируйте файл заново
4. Перезапустите Rhino

## Где искать ошибки:

1. **В Grasshopper:** File → Special Folders → Components Folder
2. **В Windows:** %APPDATA%\Grasshopper\Logs
3. **В Rhino:** Включите "Show loading messages" через команду `GrasshopperDeveloperSettings`



