using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace SCME.UpdateServer
{
    public static class ZipAndXmlHelper
    {
        public static IEnumerable<string> DirectorySearch(string directory)
        {
            foreach (var f in Directory.GetFiles(directory))
                yield return f;
            
            foreach (var innerDirectory in Directory.GetDirectories(directory))
            foreach (var f in DirectorySearch(innerDirectory))
                yield return f;    
            
        }
        


        private static string PrintXml(string xml)
        {
            var result = "";

            var mStream = new MemoryStream();
            var writer = new XmlTextWriter(mStream, Encoding.Unicode);
            var document = new XmlDocument();

            try
            {
                document.LoadXml(xml);
                writer.Formatting = Formatting.Indented;

                document.WriteContentTo(writer);
                writer.Flush();
                mStream.Flush();

                mStream.Position = 0;

                var sReader = new StreamReader(mStream);

                var formattedXml = sReader.ReadToEnd();

                result = formattedXml;
            }
            catch (XmlException)
            {
                // Handle the exception
            }

            mStream.Close();
            writer.Close();

            return result;
        }

        public static byte[] GetChangedConfig(string sourceFileName, MmeParameter mmeParameter)
        {
            using var sr = new StreamReader(sourceFileName);
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(sr);
            
            var appSettings = xmlDocument.SelectNodes("configuration/applicationSettings/SCME.UIServiceConfig.Properties.Settings/setting").Cast<XmlNode>().ToList();

            var n = 0;
            foreach (var configurationSection in mmeParameter.Configs.GetChildren())
            {
                n++;
                var xmlNode = appSettings.SingleOrDefault(m => m.Attributes["name"].InnerText == configurationSection.Key);
                // ReSharper disable once InvertIf
                if (xmlNode != null)
                {
                    var newNode = xmlDocument.CreateElement("value");
                    newNode.InnerText = configurationSection.Value;
                    xmlNode.RemoveChild(xmlNode.ChildNodes[0]);
                    xmlNode.AppendChild(newNode);
                }
            }
            sr.Close();

            return Encoding.UTF8.GetBytes(PrintXml(xmlDocument.OuterXml));
        }
    }
}