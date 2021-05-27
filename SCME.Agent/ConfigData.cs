namespace SCME.Agent
{
    /// <summary>Конфигурационные данные агента</summary>
    public class ConfigData
    {
        /// <summary>Расположение файла службы</summary>
        public string ServiceAppPath
        {
            get; set;
        }

        /// <summary>Расположение файла UI</summary>
        public string UIAppPath
        {
            get; set;
        }

        /// <summary>Расположение файла Proxy</summary>
        public string ProxyAppPath
        {
            get; set;
        }

        /// <summary>Активация Proxy</summary>
        public bool IsProxyEnabled
        {
            get; set;
        }

        /// <summary>Задержка инициализации Proxy</summary>
        public int ProxyInitDelayMs
        {
            get; set;
        }

        /// <summary>Отладка процесса обновления</summary>
        public bool DebugUpdate
        {
            get; set;
        }

        /// <summary>Url сервера обновления</summary>
        public string UpdateServerUrl
        {
            get; set;
        }

        /// <summary>Доступность UI</summary>
        public bool IsUserInterfaceEnabled
        {
            get; set;
        }

        /// <summary>MME-код</summary>
        public string MMECode
        {
            get; set;
        }
    }
}