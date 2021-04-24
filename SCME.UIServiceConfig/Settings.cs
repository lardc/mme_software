using System;
using System.ComponentModel;
using System.IO;
using System.Xml;

namespace SCME.UIServiceConfig
{
    /// <summary>Конфигурационные настройки</summary>
    public static class Settings
    {
        /// <summary>Загрузка конфигурационных параметров</summary>
        /// <param name="loadParentConfig">Необходимость использовать родительские конфигурационные параметры</param>
        public static void LoadSettings(bool loadParentConfig = false)
        {
            Properties.Settings Settings = Properties.Settings.Default;
            //Расположение сборки
            string ExePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            //Расположение конфигурационных параметров
            string ConfigPath = Path.Combine((loadParentConfig ? Directory.GetParent(Path.GetDirectoryName(ExePath)).FullName : Path.GetDirectoryName(ExePath)) ?? throw new DirectoryNotFoundException(), "SCME.UIServiceConfig.dll.config");
            XmlDocument Document = new XmlDocument();
            Document.Load(ConfigPath);
            //Получение списка параметров
            foreach (XmlElement Element in Document.SelectNodes(@"//applicationSettings/SCME.UIServiceConfig.Properties.Settings")[0])
            {
                string NameSetting = Element.GetAttribute("name");
                string Value = Element.InnerText;
                Type TypeValue = Settings[NameSetting].GetType();
                TypeConverter Converter = TypeDescriptor.GetConverter(TypeValue);
                bool CanConvert = Converter.CanConvertFrom(typeof(string));
                //Конвертация не удалась
                if (!CanConvert)
                    throw new NotImplementedException();
                Settings[NameSetting] = Converter.ConvertFrom(Value);
            }
        }
    }
}
