using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace SCME.ProfileServer
{
    internal sealed class Configuration
    {
        internal string MMECode { get; set; }

        internal IList<Guid> ProfileKeyList { get; private set; }

        internal Configuration()
        {
            MMECode = "MME0";
            ProfileKeyList = new List<Guid>();
        }

        internal Configuration(string MMECode)
        {
            this.MMECode = MMECode;
            ProfileKeyList = new List<Guid>();
        }
    }

    public class ConfigurationList
    {
        private readonly string m_Path;
        private readonly XmlDocument m_Doc;

        internal ConfigurationList(string DocumentPath)
        {
            m_Path = DocumentPath;

            try
            {
                m_Doc = new XmlDocument();
                m_Doc.Load(DocumentPath);
            }
            catch (Exception ex)
            {
                m_Doc = null;
                throw new Exception(String.Format("Error while loading document: {0}", ex.Message));
            }

            Configurations = Load();
        }

        internal IList<Configuration> Configurations { get; private set; }

        private IList<Configuration> Load()
        {
            try
            {
                var root = new List<Configuration>();
                
                if (m_Doc.DocumentElement != null)
                    root.AddRange((from XmlElement element in m_Doc.DocumentElement select ParseElement(element)));

                return root;
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Error while parsing document: {0}", ex.Message));
            }
        }

        private static Configuration ParseElement(XmlNode Element)
        {
            try
            {
                var conf = new Configuration(Element.Attributes[0].Value);

                foreach (XmlNode record in Element.ChildNodes)
                    conf.ProfileKeyList.Add(Guid.Parse(record.FirstChild.Value));

                return conf;
            }
            catch (NullReferenceException ex)
            {
                throw new Exception(String.Format("Error during attribute loading: {0}", ex.Message));
            }
        }

        internal void Save()
        {
            if (m_Doc == null)
                return;
            
            if (m_Doc.DocumentElement != null)
                m_Doc.DocumentElement.RemoveAll();

            try
            {
                if (m_Doc.DocumentElement != null) 
                    foreach (var item in Configurations)
                        m_Doc.DocumentElement.AppendChild(SerializeItem(item));
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Error while serializing document: {0}", ex.Message));
            }

            try
            {
                m_Doc.Save(m_Path);
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Error while saving document: {0}", ex.Message));
            }
        }

        private XmlNode SerializeItem(Configuration Item)
        {
            var element = m_Doc.CreateNode(XmlNodeType.Element, "Configuration", "");

            try
            {
                // Add name
                var attr = m_Doc.CreateAttribute("Code");
                attr.Value = Item.MMECode;
                element.Attributes.Append(attr);

                foreach (var guid in Item.ProfileKeyList)
                {
                    var record = m_Doc.CreateNode(XmlNodeType.Element, "Record", "");
                    var recordContent = m_Doc.CreateNode(XmlNodeType.Text, "", "");

                    recordContent.Value = guid.ToString();
                    record.AppendChild(recordContent);

                    element.AppendChild(record);
                }
            }
            catch (NullReferenceException ex)
            {
                throw new Exception(String.Format("Error during attribute serialization: {0}", ex.Message));
            }

            return element;
        }
    }
}