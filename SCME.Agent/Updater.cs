using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using System.Xml;
using SCME.Agent.Properties;
// ReSharper disable InvertIf

namespace SCME.Agent
{
    internal class Updater
    {
        private const string ERROR_FILE = "UpdateError.txt";
        private const string GET_SCME_AGENT_VERSION_PATH = "Update/GetAgentVersion";
        private const string DOWNLOAD_SCME_AGENT_FILE_PATH = "Update/GetAgentFolder";
        private const string EQUAL_SOFTWARE_VERSION = "Update/EqualSoftwareVersion";
        private const string DOWNLOAD_SCME_UI_SERVICE_FILE_PATH = "Update/GetSoftwareFolder";
        private const string BACK_AGENT_DIRECTORY = "../BackAGENT";
        private const string BACK_UI_SERVICE_DIRECTORY = "../BackUiService";

        private readonly HttpClient _client = new HttpClient();

        private const int DELAY_MS = 1000;
        private const int COUNT_REPEAT = 3;
        
        private static async Task<T> RepeatActionAsync<T>(Func<Task<T>> action, string exceptionMessage = null)
        {
            for (var i = 0; i < COUNT_REPEAT; i++)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex)
                {
                    if (i == COUNT_REPEAT - 1)
                        throw string.IsNullOrEmpty(exceptionMessage) ? ex : new Exception(exceptionMessage, ex);
                    Thread.Sleep(DELAY_MS);
                }
            }
            throw new NotImplementedException();
        }

        private static void DirectoryMove(string sourceDirName, string destDirName, bool copySubDirs)
        {
            var dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: "+ sourceDirName);

            var subDirectories = dir.GetDirectories();

            if (!Directory.Exists(destDirName))
                Directory.CreateDirectory(destDirName);

            var files = dir.GetFiles();
            foreach (var file in files)
                file.MoveTo(Path.Combine(destDirName, file.Name));

            if (copySubDirs)
                foreach (var directoryInfo in subDirectories)
                    DirectoryMove(directoryInfo.FullName, Path.Combine(destDirName, directoryInfo.Name), true);
        }

        private static void UnzipToCurrentDirectory(byte[] archiveBytes, string destinationDirectory)
        {
            using var memoryStream = new MemoryStream(archiveBytes);
            using var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Read, false);
            foreach (var entry in zipArchive.Entries)
                File.WriteAllBytes(Path.Combine(destinationDirectory, entry.Name), new BinaryReader(entry.Open()).ReadBytes((int)entry.Length));
        }


        public async Task<bool> UpdateAgent()
        {
            try
            {
                var serverAgentVersionString = await RepeatActionAsync(() => _client.GetStringAsync(new Uri(new Uri(Program.ConfigData.UpdateServerUrl), GET_SCME_AGENT_VERSION_PATH)));
                if (FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion == serverAgentVersionString)
                    return false;

                var newVersionBytes = await RepeatActionAsync(() => _client.GetByteArrayAsync(new Uri(new Uri(Program.ConfigData.UpdateServerUrl), DOWNLOAD_SCME_AGENT_FILE_PATH)));

                if(Directory.Exists(BACK_AGENT_DIRECTORY))
                    Directory.Delete(BACK_AGENT_DIRECTORY, true);

                DirectoryMove(Directory.GetCurrentDirectory(), BACK_AGENT_DIRECTORY,true);
                
                try
                {
                    UnzipToCurrentDirectory(newVersionBytes, Directory.GetCurrentDirectory());
                }
                catch
                {
                    DirectoryMove(BACK_AGENT_DIRECTORY, Directory.GetCurrentDirectory(),true);
                    throw;
                }

                return true;
            }
            catch (Exception ex)
            {
                File.WriteAllText(ERROR_FILE, ex.ToString());
                return false;
            }
        }

        public async Task UpdateUiService()
        {
            try
            {
                string uiServiceDirectory = Path.GetDirectoryName(Program.ConfigData.UIAppPath);

                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(Path.Combine(Path.GetDirectoryName(Program.ConfigData.UIAppPath), "SCME.UIServiceConfig.dll.config"));
                var mmeCode = xmlDocument.SelectNodes("configuration/applicationSettings/SCME.UIServiceConfig.Properties.Settings/setting").Cast<XmlNode>().Single(m=> m.Attributes["name"].Value == "MMECode").InnerText;

                var uriBuilder = new UriBuilder(new Uri(new Uri(Program.ConfigData.UpdateServerUrl), EQUAL_SOFTWARE_VERSION));
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                query["mme"] = mmeCode;
                query["currentVersion"] = FileVersionInfo.GetVersionInfo(Program.ConfigData.UIAppPath).ProductVersion;
                uriBuilder.Query = query.ToString();
                
                var versionEqual = await RepeatActionAsync(() => _client.GetStringAsync(uriBuilder.Uri));

                if (versionEqual == "null")
                {
                    MessageBox.Show($@"{mmeCode} не найден на сервере");
                    return;
                }

                if (Convert.ToBoolean(versionEqual))
                    return;
                
                var newVersionBytes = await RepeatActionAsync(() => _client.GetByteArrayAsync(new Uri(new Uri(Program.ConfigData.UpdateServerUrl), $"{DOWNLOAD_SCME_UI_SERVICE_FILE_PATH}?mme={mmeCode}")));

                 if(Directory.Exists(BACK_UI_SERVICE_DIRECTORY))
                    Directory.Delete(BACK_UI_SERVICE_DIRECTORY, true);

                DirectoryMove(uiServiceDirectory, BACK_UI_SERVICE_DIRECTORY,true);

                try
                {
                    UnzipToCurrentDirectory(newVersionBytes, uiServiceDirectory);
                }
                catch
                {
                    DirectoryMove(BACK_UI_SERVICE_DIRECTORY, uiServiceDirectory ,true);
                    throw;
                }
                

            }
            catch (Exception ex)
            {
                File.WriteAllText(ERROR_FILE, ex.ToString());
            }
        }

    }
}