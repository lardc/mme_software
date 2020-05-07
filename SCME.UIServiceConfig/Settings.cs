using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace SCME.UIServiceConfig
{
    public static class Settings
    {
        public static void LoadSettings(bool loadParentConfig = false)
        {
            var settings = Properties.Settings.Default;
            
            var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var path = Path.Combine((loadParentConfig ? Directory.GetParent(Path.GetDirectoryName(exePath)).FullName : Path.GetDirectoryName(exePath)) ?? throw new DirectoryNotFoundException(), "SCME.UIServiceConfig.dll.config");
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(path);
            foreach(XmlElement i in  xmlDocument.SelectNodes(@"//applicationSettings/SCME.UIServiceConfig.Properties.Settings")[0])
            {
                var nameSetting = i.GetAttribute("name");
                var value = i.InnerText;
                
                var typeValue = settings[nameSetting].GetType();
                
                var converter = TypeDescriptor.GetConverter(typeValue);
                var canConvert = converter.CanConvertFrom(typeof(string));
                
                if(!canConvert)
                    throw new NotImplementedException();
                
                settings[nameSetting] = converter.ConvertFrom(value);
            }
        }
    }
}
