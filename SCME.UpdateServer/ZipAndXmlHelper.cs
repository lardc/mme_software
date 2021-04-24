using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace SCME.UpdateServer
{
    public static class ZipAndXmlHelper
    {
        /// <summary>Рекурсивное составление дерева директории</summary>
        /// <param name="directory">Директория для поиска</param>
        /// <returns>Все файлы и папки директории</returns>
        public static IEnumerable<string> DirectorySearch(string directory)
        {
            //Поиск всех файлов
            foreach (string File in Directory.GetFiles(directory))
                yield return File;
            //Получение всех директорий и поиск вложенных файлов
            foreach (string InnerDirectory in Directory.GetDirectories(directory))
                foreach (string File in DirectorySearch(InnerDirectory))
                    yield return File;
        }

        /// <summary>Получение изменений в конфигурации</summary>
        /// <param name="sourceFileName">Файл конфигурации</param>
        /// <param name="mmeParameter">Mme-код</param>
        /// <returns></returns>
        public static byte[] GetChangedConfig(string sourceFileName, MmeParameter mmeParameter)
        {
            using StreamReader Reader = new StreamReader(sourceFileName);
            XmlDocument Document = new XmlDocument();
            Document.Load(Reader);
            List<XmlNode> AppSettings = Document.SelectNodes("configuration/applicationSettings/SCME.UIServiceConfig.Properties.Settings/setting").Cast<XmlNode>().ToList();
            int i = 0;
            foreach (IConfigurationSection ConfigurationSection in mmeParameter.Configs.GetChildren())
            {
                i++;
                XmlNode XmlNode = AppSettings.SingleOrDefault(m => m.Attributes["name"].InnerText == ConfigurationSection.Key);
                if (XmlNode != null)
                {
                    XmlElement NewNode = Document.CreateElement("value");
                    NewNode.InnerText = ConfigurationSection.Value;
                    XmlNode.RemoveChild(XmlNode.ChildNodes[0]);
                    XmlNode.AppendChild(NewNode);
                }
            }
            Reader.Close();
            return Encoding.UTF8.GetBytes(Xml_Print(Document.OuterXml));
        }

        private static string Xml_Print(string xml) //Получение данных из xml
        {
            string Result = "";
            MemoryStream Stream = new MemoryStream();
            XmlTextWriter Writer = new XmlTextWriter(Stream, Encoding.Unicode);
            XmlDocument Document = new XmlDocument();
            try
            {
                Document.LoadXml(xml);
                Writer.Formatting = Formatting.Indented;
                Document.WriteContentTo(Writer);
                Writer.Flush();
                Stream.Flush();
                Stream.Position = 0;
                StreamReader Reader = new StreamReader(Stream);
                string FormattedXml = Reader.ReadToEnd();
                Result = FormattedXml;
            }
            catch { }
            Stream.Close();
            Writer.Close();
            return Result;
        }
    }
}