using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace SCME.RemoteReportGenerator
{
    internal class MMERecord
    {
        internal string MMECode { get; set; }
        internal string IPAddress { get; set; }
        internal bool Use2PosTemplate { get; set; }

        public override string ToString()
        {
            return string.Format("{0}: IP - {1}, 2P - {2}", MMECode, IPAddress, Use2PosTemplate);
        }
    }

    internal class MMEDictionary
    {
        internal List<MMERecord> Records { get; private set; }

        internal MMEDictionary(string DocumentPath)
        {
            try
            {
                var doc = new XmlDocument();

                doc.Load(DocumentPath);
                Records = Load(doc);
            }
            catch (Exception)
            {
                Records = new List<MMERecord>();
                throw;
            }
        }

        private static List<MMERecord> Load(XmlDocument Document)
        {
            if (Document.DocumentElement == null)
                return new List<MMERecord>();

            return new List<MMERecord>(from XmlElement element in Document.DocumentElement
                select new MMERecord
                {
                    MMECode = element.Attributes[0].Value,
                    IPAddress = element.Attributes[1].Value,
                    Use2PosTemplate = Boolean.Parse(element.Attributes[2].Value)
                });
        }
    }
}