# Установка DimensionGH_Gh плагина

## Требования

- Rhino 7 или 8
- Grasshopper
- Visual Studio 2019 или новее
- .NET Framework 4.8

## Шаги установки

### 1. Установка NuGet пакетов

Откройте проект в Visual Studio и восстановите NuGet пакеты:
- Правый клик на проекте → "Manage NuGet Packages"
- Или выполните: `nuget restore`

### 2. Настройка ссылок на библиотеки Rhino/Grasshopper

В файле `DimensionGH_Gh.csproj` обновите пути к библиотекам:

```xml
<Reference Include="Grasshopper">
  <HintPath>C:\Program Files\Rhino 8\System\Grasshopper.dll</HintPath>
  <!-- Или для Rhino 7: C:\Program Files\Rhinoceros 7\System\Grasshopper.dll -->
</Reference>
```

### 3. Сборка проекта

1. Выберите конфигурацию Release
2. Соберите проект (Build → Build Solution)
3. После сборки файл `.gha` автоматически скопируется в:
   `%AppData%\Grasshopper\Libraries\Dimension_Gh\`

### 4. Использование в Grasshopper

1. Запустите Grasshopper
2. Компоненты будут доступны в категории "Dimension Gh"
3. Укажите порт HTTP сервера Archicad (можно узнать из панели Dimension Gh в Archicad)

## Тестирование

1. Запустите Archicad с аддоном Dimension_Gh
2. Откройте панель Dimension Gh и узнайте порт (например, 19723)
3. В Grasshopper добавьте компонент "Dim_Connect"
4. Укажите порт и установите Ping = true
5. Должен появиться статус "Connected" и сообщение "Dimension_Gh alive"

