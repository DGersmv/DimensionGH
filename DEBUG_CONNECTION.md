# Отладка подключения к Archicad

## Проблема: "Cannot connect to Archicad"

### Проверка 1: Запущен ли Archicad?
- Убедитесь, что Archicad 29 запущен
- Убедитесь, что аддон Dimension_Gh загружен (проверьте в меню Add-Ons)

### Проверка 2: Правильный ли порт?
- Используйте компонент Dim_Connect для получения порта
- Или проверьте порт в браузерной палитре Dimension_Gh
- Порт должен быть получен через команду GetPort, а не задан вручную

### Проверка 3: Зарегистрированы ли команды?
- Проверьте, что в Main.cpp зарегистрированы команды:
  - GetPortCommand ✅
  - PingCommand ✅
  - CreateLinearDimensionCommand ✅ (была закомментирована, теперь раскомментирована)

### Провлема 4: Формат запроса
- Grasshopper отправляет запрос через `API.ExecuteAddOnCommand`
- Формат: `{"Command": "API.ExecuteAddOnCommand", "Parameters": {...}}`
- Archicad должен автоматически обрабатывать HTTP запросы к этому endpoint

### Проверка 5: HTTP сервер Archicad
- Archicad автоматически запускает HTTP сервер на порту, полученном через `ACAPI_Command_GetHttpConnectionPort()`
- Сервер должен быть доступен на `http://127.0.0.1:{port}/`
- Endpoint: POST `/` с JSON телом

### Что проверить:
1. Пересоберите аддон после раскомментирования CreateLinearDimensionCommand
2. Перезапустите Archicad
3. Проверьте, что аддон загружен
4. Получите правильный порт через Dim_Connect
5. Попробуйте подключиться снова

### Альтернативный способ проверки:
Можно проверить подключение через браузер или curl:
```bash
curl -X POST http://127.0.0.1:{PORT}/ -H "Content-Type: application/json" -d "{\"Command\":\"API.ExecuteAddOnCommand\",\"Parameters\":{\"addOnCommandId\":{\"commandNamespace\":\"DimensionGh\",\"commandName\":\"Ping\"},\"addOnCommandParameters\":{}}}"
```


