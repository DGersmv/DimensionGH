# Dimension GH

Grasshopper Dimension Bridge for Archicad 27/28/29

## Описание

Add-on для Archicad, который предоставляет мост между Grasshopper и Archicad для работы с размерами через JSON-протокол.

## Поддерживаемые версии

- ✅ Archicad 27
- ✅ Archicad 28
- ✅ Archicad 29

## Компоненты проекта

### 1. Archicad Add-on (C++)

Плагин для Archicad, который предоставляет HTTP API для работы с размерами.

**Собранные плагины:**
- `Dist/DimensionGH-AC27.apx` - для Archicad 27
- `Dist/DimensionGH-AC28.apx` - для Archicad 28
- `Dist/DimensionGH-AC29.apx` - для Archicad 29

### 2. Grasshopper Plugin (C#)

Плагин для Grasshopper, который позволяет создавать и управлять размерами в Archicad.

**Файлы:**
- `Dist/DimensionGH_Gh.gha` - основной файл плагина
- `Dist/Newtonsoft.Json.dll` - обязательная зависимость

## Установка

### Archicad Add-on

1. Скопируйте соответствующий `.apx` файл из папки `Dist/`
2. В Archicad: Удобства → Удобства (Add-ons)
3. Найдите и установите `DimensionGH.apx`

### Grasshopper Plugin

См. [DimensionGH_Gh/INSTALLATION_FOR_USERS.md](DimensionGH_Gh/INSTALLATION_FOR_USERS.md) для подробных инструкций.

**Кратко:**
1. Скопируйте `DimensionGH_Gh.gha` в `%APPDATA%\Grasshopper\Libraries\`
2. **Обязательно** скопируйте `Newtonsoft.Json.dll` в ту же папку
3. Перезапустите Rhino и Grasshopper

## Сборка

### Archicad Add-on

См. [BUILD_INSTRUCTIONS.md](BUILD_INSTRUCTIONS.md) для подробных инструкций по сборке проекта.

**Быстрая сборка:**
```bash
cmake -S . -B build
cmake --build build --config Release
```

### Grasshopper Plugin

1. Откройте `DimensionGH_Gh/DimensionGH_Gh.sln` в Visual Studio
2. Восстановите NuGet пакеты
3. Соберите проект (Build → Build Solution)
4. Файлы автоматически скопируются в папку Grasshopper Libraries

## Структура проекта

```
DimensionGH/
├── Src/                    # Исходный код C++ (ArchiCAD Add-on)
├── RFIX/                   # Ресурсы (нелокализуемые)
├── RFIX.win/              # Ресурсы для Windows
├── DimensionGH_Gh/        # Исходный код C# (Grasshopper Plugin)
│   ├── Components/        # Компоненты Grasshopper
│   ├── Client/            # HTTP клиент
│   ├── Protocol/          # Протокол JSON
│   └── install.ps1        # Скрипт установки
├── Dist/                  # Собранные плагины
│   ├── DimensionGH-AC27.apx
│   ├── DimensionGH-AC28.apx
│   ├── DimensionGH-AC29.apx
│   ├── DimensionGH_Gh.gha
│   └── Newtonsoft.Json.dll
├── CMakeLists.txt         # Файл конфигурации CMake
└── BUILD_INSTRUCTIONS.md  # Инструкции по сборке
```

## Использование

1. Запустите Archicad с установленным аддоном DimensionGH
2. Откройте панель "Dimension Gh" в Archicad и узнайте порт HTTP сервера
3. В Grasshopper используйте компоненты из категории "Dimension Gh"
4. Укажите порт в компоненте "Dim_Connect" и установите Ping = true

## Текущий статус

- ✅ Сборка плагинов для Archicad 27/28/29
- ✅ Grasshopper плагин с компонентами для работы с размерами
- ✅ HTTP API для связи между Grasshopper и Archicad
- ✅ Автоматическая установка зависимостей

## Лицензия

[Указать лицензию]

