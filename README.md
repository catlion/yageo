Тонкая прослойка к API геокодера Яндекс.Карт для .NET 4+/Mono. Написана на F#, совместимость с C# пока не протестирована.

Цели
====
Результат запроса к геокодеру Яндекса возвращает массу информации, извлечение из которой собственно координат несколько затруднена.
Также текущий формат ответа не вполне соответствует официальной документации.
Эта прослойка позволяет упростить разбор результата, возвращаемого геокодером.

Зависимости
====
[ServiceStack.Text](https://github.com/ServiceStack/ServiceStack.Text) (в Visual Studio подключить Nuget-пакет, в Mono скачать и добавить в зависимости ServiceStack.Text.dll)

Использование
====
* В F#-проекте достаточно включить файл [yageo.fs](https://github.com/catlion/yageo/blob/master/yageo/yageo.fs)
* В C#/VB-скачать и добавить проект [yageo](https://github.com/catlion/yageo/tree/master/yageo), желательно собрать в виде [модуля](http://msdn.microsoft.com/en-us/library/58scf68s.aspx) и включить его в свою сборку.

Примеры использования находятся в папке [examples](https://github.com/catlion/yageo/tree/master/examples).
