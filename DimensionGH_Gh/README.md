# DimensionGH_Gh - Grasshopper Plugin

Grasshopper плагин для работы с размерами Archicad через Dimension_Gh add-on.

## Структура проекта

- `Protocol/` - классы протокола (JsonRequest, JsonResponse, DimensionDto)
- `Client/` - HTTP клиент для связи с Archicad
- `Components/` - компоненты Grasshopper
- `Properties/AssemblyInfo.cs` - информация о сборке

## Сборка

1. Открыть проект в Visual Studio
2. Убедиться, что ссылки на Grasshopper.dll, GH_IO.dll, RhinoCommon.dll настроены
3. Собрать проект
4. Скопировать .dll в %AppData%\Grasshopper\Libraries\Dimension_Gh\ и переименовать в .gha

## Использование

1. Запустить Archicad с аддоном Dimension_Gh
2. Узнать порт HTTP сервера из панели Dimension Gh в Archicad
3. В Grasshopper использовать компоненты DimensionGh с указанием порта

