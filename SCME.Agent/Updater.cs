using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SCME.Agent.Properties;

namespace SCME.Agent
{
    class Updater
    {
        private const string ERROR_FILE = "UpdateError.txt";
        private const string GET_SCME_AGENT_VERSION_PATH = "Update/GetAgentVersion";
        private const string DOWNLOAD_SCME_AGENT_FILE_PATH = "Update/GetAgentFolder";
        private const string EQUAL_SOFTWARE_VERSION = "Update/EqualSoftwareVersion";
        private const string DOWNLOAD_SCME_UI_SERVICE_FILE_PATH = "Update/GetSoftwareFolder";
        private const string BACK_AGENT_DIRECTORY = "BackAGENT";
        private const string BACK_UI_SERVICE_DIRECTORY = "BackUiService";

        private readonly HttpClient _client = new HttpClient();

        private const int DELAY_MS = 1000;
        private const int COUNT_REPEAT = 3;

        public Updater()
        {
            
        }
        
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
                    if(i == COUNT_REPEAT - 1)
                        throw string.IsNullOrEmpty(exceptionMessage) ? ex : new Exception(exceptionMessage, ex);
                    Thread.Sleep(DELAY_MS);
                }
            }
            throw new NotImplementedException();
        }
        

 

        public async Task UpdateAgent()
        {
            try
            {
                var serverAgentVersionString =  await RepeatActionAsync(() => _client.GetStringAsync(new Uri(new Uri(Properties.Settings.Default.UpdateServerUrl), GET_SCME_AGENT_VERSION_PATH)));
                if (string.Compare(FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion, serverAgentVersionString, StringComparison.InvariantCulture) >= 0)
                    return;

                var newVersionBytes = await RepeatActionAsync(() => _client.GetByteArrayAsync(new Uri(new Uri(Properties.Settings.Default.UpdateServerUrl), DOWNLOAD_SCME_AGENT_FILE_PATH)));
                
                if (Directory.Exists(BACK_AGENT_DIRECTORY))
                    Directory.Delete(BACK_AGENT_DIRECTORY, true);
                
                if (Directory.Exists(BACK_AGENT_DIRECTORY))
                    Directory.CreateDirectory(BACK_AGENT_DIRECTORY);
                foreach (var file in Directory.GetFiles(Directory.GetCurrentDirectory()))
                    File.Move(file, Path.Combine(BACK_AGENT_DIRECTORY, Path.GetFileName(file)));
                
                try
                {
                    using var memoryStream = new MemoryStream(newVersionBytes);
                    using var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Read, false);
                    foreach (var entry in zipArchive.Entries)
                        File.WriteAllBytes(entry.Name, new BinaryReader(entry.Open()).ReadBytes((int) entry.Length));
                }
                catch (Exception)
                {
                    foreach (var file in Directory.GetFiles(BACK_AGENT_DIRECTORY))
                        File.Move(file, Path.Combine(Directory.GetCurrentDirectory(), Path.GetFileName(file)));
                    throw;
                }
                
                Process.Start(Assembly.GetExecutingAssembly().Location);
                Environment.Exit(Environment.ExitCode);
            }
            catch (Exception ex)
            {
               File.WriteAllText(ERROR_FILE, ex.ToString());
            }
        }

        public async Task UpdateUiService()
        {
            try
            {
                if( await RepeatActionAsync(() => _client.GetStringAsync(new Uri(new Uri(Properties.Settings.Default.UpdateServerUrl), EQUAL_SOFTWARE_VERSION))) == @"True")
                    return;
                
                var newVersionBytes = await RepeatActionAsync(() => _client.GetByteArrayAsync(new Uri(new Uri(Properties.Settings.Default.UpdateServerUrl), DOWNLOAD_SCME_UI_SERVICE_FILE_PATH)));
                
                if (Directory.Exists(BACK_UI_SERVICE_DIRECTORY))
                    Directory.Delete(BACK_UI_SERVICE_DIRECTORY, true);
                
                
                
                try
                {
                    UnpackZipBytes(newVersionBytes);
                }
                catch (Exception)
                {
                    RetreatCurrentVersion(BACK_UI_SERVICE_DIRECTORY);
                    throw;
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
    }
}