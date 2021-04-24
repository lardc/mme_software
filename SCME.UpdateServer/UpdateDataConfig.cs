using Microsoft.Extensions.Configuration;

namespace SCME.UpdateServer
{
    /// <summary>Параметры Mme</summary>
    public class MmeParameter
    {
        /// <summary>Наименование</summary>
        public string Name
        {
            get; set;
        }

        /// <summary>Расположение</summary>
        public string Folder
        {
            get; set;
        }

        /// <summary>Конфигурация</summary>
        public IConfigurationSection Configs
        {
            get; set;
        }
    }

    /// <summary>Параметры Mme</summary>
    public class UpdateDataConfig
    {
        /// <summary>Debug-параметр</summary>
        public string DebugParameter
        {
            get; set;
        }

        /// <summary>Корневой каталог</summary>
        public string DataPathRoot
        {
            get; set;
        }

        /// <summary>Расположение папки агента</summary>
        public string ScmeAgentFolderName
        {
            get; set;
        }

        /// <summary>Имя файла агента</summary>
        public string ScmeAgentExeName
        {
            get; set;
        }

        /// <summary>Имя файла UI</summary>
        public string ScmeUIExeName
        {
            get; set;
        }

        /// <summary>Имя файла общих настроек</summary>
        public string ScmeCommonConfigName
        {
            get; set;
        }

        /// <summary>Параметры Mme</summary>
        public MmeParameter[] MmeParameters
        {
            get; set;
        }
    }
}