#### Наблюдатель за Локальным модулем (Observer for Local Module)
версия модуля ```2.****```\, api версии 2\
Позволяет инициализировать, наблюдать за логами, получать статус.\
При первом старте программы, настройки инициализируются по умолчанию.\
Вам нужно поменять Auth, по умолчанию строка Dase64 (admin:admin), 
Токен, если вы собираитесь иницализировать модуль.
Настройки находятся в директории ~./config/obserberLm/srttings.json. (создаются при первом старте программы)\


```csharp
    internal class MySettings
    {
        /// <summary>
        /// Путь к API локального модуля. Например: http://localhost:5995/api/v2/
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Строка в формате Base64 для авторизации Basic по умолчанию стоит admin:admin
        /// </summary>
        public string Auth { get; set; }
        /// <summary>
        /// Токен для авторизации по API
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// Путь к папке логов
        /// </summary>
        public string FolderLog { get; set; }
        /// <summary>
        /// Количество строк в логах, 
        /// которые будут отображаться при открытии приложения, по умолчанию 100. 0 не допускается.
        /// </summary>
        public int Tail { get; set; } = 100;
    }
```
По умолчанию: (Auth: admin:admin в Base64) и Tail: 100)
```json
{
  "Url": "http://localhost:5995/api/v2/",
  "Auth": "YWRtaW46YWRtaW4="
  "Token": "97486293-646b-463e-8199-48c37e36d605",
  "FolderLog": "C:\\Program Files\\Regime\\var\\log" or "/var/log/regime"
  "Tail": 100
}
```
[GitHub](https://github.com/ionson100/observer_lm)