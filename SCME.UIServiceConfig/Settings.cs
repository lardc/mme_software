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
    public partial class Settings
    {
        public static void LoadSettings()
        {
            var settings = SCME.UIServiceConfig.Properties.Settings.Default;
            
            string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string path = Path.Combine(Path.GetDirectoryName(exePath), "SCME.UIServiceConfig.dll.config");
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(path);
            foreach(XmlElement i in  xmlDocument.SelectNodes(@"//applicationSettings/SCME.UIServiceConfig.Properties.Settings")[0])
            {
                string nameSetting = i.GetAttribute("name");
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
