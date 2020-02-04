using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SCME.Agent
{
    class Updater
    {
        private const string GET_SCME_AGENT_VERSION_PATH = "Update/GetAgentVersion";
        private const string DOWNLOAD_SCME_AGENT_FILE_PATH = "Update/GetAgentFolder";
        private const string BACK_DIRECTORY = "Back";

        private readonly HttpClient _client = new HttpClient();

        private void RemoveBackFiles()
        {
            if (Directory.Exists(BACK_DIRECTORY))
                Directory.Delete(BACK_DIRECTORY, true);
        }

        private void MoveCurrentVersion()
        {
            Directory.CreateDirectory(BACK_DIRECTORY);

            foreach (var file in Directory.GetFiles(Directory.GetCurrentDirectory()))
                File.Move(file, Path.Combine(BACK_DIRECTORY, Path.GetFileName(file)));
        }

        private async Task DownloadNewVersionAsync()
        {
            var serverFolderBytes = await _client.GetByteArrayAsync(new Uri(new Uri(Properties.Settings.Default.UpdateServerUrl), DOWNLOAD_SCME_AGENT_FILE_PATH));

            using var memoryStream = new MemoryStream(serverFolderBytes);
            using var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Read, false);

            foreach (var entry in zipArchive.Entries)
                File.WriteAllBytes(entry.Name, new BinaryReader(entry.Open()).ReadBytes((int) entry.Length));
        }

        public async Task<bool> Update()
        {
            try
            {
                RemoveBackFiles();

                var serverAgentVersion = await _client.GetStringAsync(new Uri(new Uri(Properties.Settings.Default.UpdateServerUrl), GET_SCME_AGENT_VERSION_PATH));

                if (string.Compare(FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion, serverAgentVersion, StringComparison.InvariantCulture) >= 0)
                    return false;

                MoveCurrentVersion();
                await DownloadNewVersionAsync();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }
        }
    }
}