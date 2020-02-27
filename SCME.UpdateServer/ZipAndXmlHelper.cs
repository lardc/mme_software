using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace SCME.UpdateServer
{
    public static class ZipAndXmlHelper
    {
        private static IEnumerable<string> DirectorySearch(string directory)
        {
            foreach (var f in Directory.GetFiles(directory))
                yield return f;
            
            foreach (var innerDirectory in Directory.GetDirectories(directory))
            foreach (var f in DirectorySearch(innerDirectory))
                yield return f;    
            
        }
        
        public static async Task<MemoryStream> GetZipStreamAsync(string folderName)
        {
            var memoryStream = new MemoryStream();

            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                foreach (var i in DirectorySearch(folderName))
                {
                    var entry = archive.CreateEntry(i.Substring(folderName.Length + 1));
                    await using var entryStream = entry.Open();
                    var bytes = await File.ReadAllBytesAsync(i);
                    await entryStream.WriteAsync(bytes, CancellationToken.None);
                }

            return memoryStream;
        }


        public  static async Task<byte[]> ReplaceConfig(ZipArchiveEntry zipArchiveEntry, MmeParameter mmeParameter)
        {
            await using var zipStream = zipArchiveEntry.Open();
            await using var memoryStream = new MemoryStream();
            await zipStream.CopyToAsync(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(memoryStream);

            var appSettings = xmlDocument.SelectNodes("configuration/applicationSettings/SCME.UIServiceConfig.Properties.Settings/setting").Cast<XmlNode>().ToList();
             
            if (appSettings == null)
                throw new Exception($"{nameof(ReplaceConfig)} {nameof(appSettings)} == null");

            foreach (var configurationSection in mmeParameter.Configs.GetChildren().SelectMany(m => m.GetChildren()))
            {
                var xmlNode = appSettings.SingleOrDefault(m => m.Attributes["name"].InnerText == configurationSection.Key);
                // ReSharper disable once InvertIf
                if (xmlNode != null)
                {
                    xmlNode.RemoveAll();
                    var newNode = xmlDocument.CreateElement("value");
                    newNode.InnerText = configurationSection.Value;
                    xmlNode.AppendChild(newNode);
                }
            }

            memoryStream.Position = 0;
            xmlDocument.Save(memoryStream);
            return memoryStream.ToArray();
        }
    }
}