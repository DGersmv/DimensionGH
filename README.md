# Dimension GH

Grasshopper Dimension Bridge for Archicad 29

## Описание

Add-on для Archicad 29, который предоставляет мост между Grasshopper и Archicad для работы с размерами через JSON-протокол.

## Текущий статус

- ✅ Этап 1: Переименование и уникальные идентификаторы (завершено)
- ⏳ Этап 2: Сборка "пустого" Dimension_Gh
- ⏳ Этап 3: Выделение модуля моста (Bridge)
- ⏳ Этап 4: JSON-каркас команд (Ping, GetDimensions, CreateLinearDimension)
- ⏳ Этап 5: Подготовка к работе с размерами (API_DimensionType)

## Сборка

См. [BUILD_INSTRUCTIONS.md](BUILD_INSTRUCTIONS.md) для подробных инструкций по сборке проекта.

## Структура проекта

```
Dimension_Gh/
├── Src/              # Исходный код C++
├── RFIX/             # Ресурсы (нелокализуемые)
├── RFIX.win/         # Ресурсы для Windows
├── CMakeLists.txt    # Файл конфигурации CMake
└── BUILD_INSTRUCTIONS.md
```

## Лицензия

[Указать лицензию]

