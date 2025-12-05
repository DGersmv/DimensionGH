ПЛАН ДЛЯ CURSOR — Grasshopper-плагин DimensionGH_Gh (порт 22700)
ЭТАП 1. Создаём проект .gha

Создать новый C#-проект:

Тип: Class Library (.NET Framework) (4.8 или то, что у тебя обычно для GH).

Имя проекта: DimensionGH_Gh (или аналогичное, но везде одно и то же).

Пространство имён: DimensionGhGh.

Подключить ссылки на библиотеки Grasshopper / Rhino:

Добавить references:

Grasshopper.dll

GH_IO.dll

RhinoCommon.dll
(Путь взять из установленного Rhino/Grasshopper.)

Настроить сборку в .gha:

В AssemblyInfo прописать:

AssemblyTitle("Dimension Gh")

AssemblyDescription("Dimension Gh – Archicad dimensions bridge")

В post-build event:

Копировать собранный .dll в %AppData%\Grasshopper\Libraries\Dimension_Gh\.

Переименовывать .dll в .gha (можно прямо в post-build).

ЭТАП 2. Общий протокол и клиент

Цель: один клиент, который стучится в наш C++-аддон по адресу
http://127.0.0.1:22700/ (порт по умолчанию 22700).

Создать папку Protocol и в ней три файла:

JsonRequest.cs

Поля:

string Command

JObject Payload (или аналог для JSON).

JsonResponse.cs

Поля:

bool Ok

string Error

JObject Result

DimensionDto.cs

Поля (в точности под C++-часть):

string Guid

string Type (например, "linear")

Point3d[] Points (Rhino.Geometry.Point3d)

string Layer

string Text

string Style

Создать класс DimensionGhClient (например, в Client/DimensionGhClient.cs):

Поля/свойства:

string Host (по умолчанию "127.0.0.1")

int Port (по умолчанию 22700)

TimeSpan Timeout (например, 1–3 сек.)

Метод:

JsonResponse Send(JsonRequest request);


Внутри:

Формирует URL: http://{Host}:{Port}/

Делает POST с application/json

Сериализует JsonRequest → JSON

Читает ответ, десериализует → JsonResponse

При сетевой ошибке / таймауте:

Возвращает JsonResponse { Ok = false, Error = "..." }

Важно: порт по умолчанию 22700. Возможность переопределить через вход компонента.

ЭТАП 3. Базовый компонент “подключение / лампа”

Компонент Dim_Connect — статус соединения + зелёная лампа.

Создать файл Components/DimConnectComponent.cs:

Наследование: GH_Component.

Категория: "Dimension Gh"

Subcategory: "Connection" (или "Core")

Входы:

Port — int, optional, default = 22700.

Ping — bool (или bool/trigger).

Выходы:

Connected — bool.

Message — string.

Логика SolveInstance:

Прочитать Port и Ping.

Если Ping == false → ничего не делать, Connected = false, Message = "Idle" (по желанию).

Если Ping == true:

Создать JsonRequest:

{ "command": "Ping", "payload": {} }


Вызвать DimensionGhClient.Send(...).

По response.Ok выставить:

Connected = true/false

Message = response.Result["message"] или response.Error.

Иконка + цветовой статус:

Переопределить Icon и/или DrawViewportWires / DrawForeground (если нужно).

Внутреннее поле состояния (enum):

Unknown, Connected, Error.

Цвет лампы:

Серый — Unknown

Зелёный — Connected

Красный — Error

ЭТАП 4. Компонент Dim_GetDimensions

Получаем размеры из Archicad и превращаем их в геометрию GH.

Создать файл Components/DimGetDimensionsComponent.cs:

Наследование: GH_Component.

Категория: "Dimension Gh"

Subcategory: "Dimensions"

Входы:

Port — int, optional, default 22700.

FilterLayer — string, optional (пусто = все).

Run — bool (trigger).

Выходы:

Curves — List<Curve> (Rhino.Geometry.Curve).

Texts — List<string>.

Layers — List<string>.

Guids — List<string>.

(опционально) Raw — список сырых JSON/DTO для отладки.

Логика SolveInstance:

Если Run == false → ничего не делаем.

Если Run == true:

Собрать JsonRequest:

{
  "command": "GetDimensions",
  "payload": { "filterLayer": "<строка или пусто>" }
}


Вызвать DimensionGhClient.Send().

Если Ok == false → выдать пустые списки + warning/RuntimeMessage с Error.

Если Ok == true:

Распарсить result.dimensions:

Для каждого элемента:

Собрать PolylineCurve/NurbsCurve из Points.

Сохранить Text, Layer, Guid.

Выдать списки на соответствующие выходы.

ЭТАП 5. Компонент Dim_SendDimensions

Отправка кривых из GH → создание/обновление размеров в Archicad.

Создать файл Components/DimSendDimensionsComponent.cs:

Наследование: GH_Component.

Категория: "Dimension Gh"

Subcategory: "Dimensions"

Входы:

Port — int, optional, default 22700.

Curves — List<Curve> — геометрия GH.

Text — string (либо список, либо одиночный; можно сделать Tree-поддержку позже).

Layer — string (слой для новых размеров).

Style — string (имя стиля размерной).

Run — bool (trigger).

Выходы:

Guids — List<string> — созданных/обновлённых размеров.

Status — string.

Логика SolveInstance:

Если Run == false → ничего.

Если Run == true:

Пройти по входным Curves.

На каждую кривую → сэмплировать точки (например, вершины полилинии).

Преобразовать Point3d → DTO {x,y,z}.

Собрать массив dimensions:

{
  "command": "CreateLinearDimension",
  "payload": {
    "dimensions": [
      {
        "points": [...],
        "layer": "<Layer>",
        "style": "<Style>",
        "textOverride": "<Text или пусто>"
      },
      ...
    ]
  }
}


Вызвать Send.

Если Ok == false → Status = Error, Guids пустой список, RuntimeMessage.

Если Ok == true → разобрать result.guids и заполнить выход.

ЭТАП 6. Общие мелочи / UX

Категория/иконки:

Для всех компонентов использовать:

Category: "Dimension Gh"

Нарисовать простые SVG/битмапы для иконок:

Dim_Connect — лампочка.

Dim_GetDimensions — стрелка из AC в GH.

Dim_SendDimensions — стрелка из GH в AC.

Обработка ошибок и сообщения:

В каждом компоненте:

если нет связи → AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "...").

если ответ пустой → Info/Warning.

Это сильно упростит отладку.

Конфигурация порта:

Везде по умолчанию использовать 22700.

Внутри компонентов, если Port не подключён → брать default 22700.

В DimensionGhClient конструктор тоже должен оставлять 22700 как default.

ЭТАП 7. Финальная связка с C++-аддоном

(На стороне Cursor тоже можно закрепить как отдельный шаг.)

Следить, чтобы протокол совпадал с C++-частью:

Команда "Ping":

C++ возвращает {"ok":true, "result":{"message":"Dimension_Gh alive"}}

Команда "GetDimensions":

C++ возвращает {"ok":true, "result":{"dimensions":[...]}}

Команда "CreateLinearDimension":

C++ принимает payload.dimensions[]

Возвращает {"ok":true, "result":{"guids":["...", ...]}}

Тестовый сценарий:

Запустить Archicad с аддоном Dimension_Gh (порт 22700).

В GH:

положить Dim_Connect, нажать Ping → зелёная лампа.

положить Dim_GetDimensions → увидеть кривые размеров.

нарисовать кривые/линии в GH → Dim_SendDimensions → увидеть новые размеры в Archicad.