using System;
using System.Collections.Specialized;
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

namespace SCME.Agent
{
    /// <summary>Апдейтер</summary>
    internal class Updater
    {
        //Расположения файлов для обновления
        private const string ERROR_FILE = "UpdateError.txt";
        private const string GET_SCME_AGENT_VERSION_PATH = "Update/GetAgentVersion";
        private const string DOWNLOAD_SCME_AGENT_FILE_PATH = "Update/GetAgentFolder";
        private const string EQUAL_SOFTWARE_VERSION = "Update/EqualSoftwareVersion";
        private const string DOWNLOAD_SCME_UI_SERVICE_FILE_PATH = "Update/GetSoftwareFolder";
        private const string BACK_AGENT_DIRECTORY = "../BackAGENT";
        private const string BACK_UI_SERVICE_DIRECTORY = "../BackUiService";
        private readonly HttpClient Client = new HttpClient();
        private const int DELAY_MS = 1000;
        private const int COUNT_REPEAT = 3;

        private static void Directory_Move(string sourceDirName, string destDirName, bool copySubDirs) //Перемещение папок
        {
            //Корневая папка
            DirectoryInfo RootDirectory = new DirectoryInfo(sourceDirName);
            //Корневая папка не существует
            if (!RootDirectory.Exists)
                throw new DirectoryNotFoundException(string.Format("Каталог не существует или не найден: {0}", sourceDirName));
            //Подразделы корневой папки
            DirectoryInfo[] SubDirectories = RootDirectory.GetDirectories();
            if (!Directory.Exists(destDirName))
                Directory.CreateDirectory(destDirName);
            //Файлы корневой директории
            FileInfo[] Files = RootDirectory.GetFiles();
            //Перемещение всех файлов в папку назначения
            foreach (FileInfo File in Files)
                File.MoveTo(Path.Combine(destDirName, File.Name));
            //Рекурсивное копирование подразделов
            if (copySubDirs)
                foreach (DirectoryInfo directoryInfo in SubDirectories)
                    Directory_Move(directoryInfo.FullName, Path.Combine(destDirName, directoryInfo.Name), true);
        }

        /// <summary>Обновление агента</summary>
        /// <returns>Результат выполнения обновления</returns>
        public async Task<bool> UpdateAgent()
        {
            try
            {
                //Получение версии агента с сервера
                string ServerAgentVersionString = await Action_RepeatAsync(() => Client.GetStringAsync(new Uri(new Uri(Program.ConfigData.UpdateServerUrl), GET_SCME_AGENT_VERSION_PATH)));
                if (FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion == ServerAgentVersionString)
                    return false;
                //Получение новой версии агента
                byte[] NewVersionBytes = await Action_RepeatAsync(() => Client.GetByteArrayAsync(new Uri(new Uri(Program.ConfigData.UpdateServerUrl), DOWNLOAD_SCME_AGENT_FILE_PATH)));
                //Бэкап предыдущей версии
                if (Directory.Exists(BACK_AGENT_DIRECTORY))
                    Directory.Delete(BACK_AGENT_DIRECTORY, true);
                Directory_Move(Directory.GetCurrentDirectory(), BACK_AGENT_DIRECTORY, true);
                try
                {
                    //Архивация новой версии и перезапись конфигурационного json-файла
                    await using MemoryStream Stream = new MemoryStream(NewVersionBytes);
                    using ZipArchive Archive = new ZipArchive(Stream, ZipArchiveMode.Read, false);
                    Archive.ExtractToDirectory(Directory.GetCurrentDirectory(), true);
                    File.Copy(Path.Combine(BACK_AGENT_DIRECTORY, "appsettings.json"), "appsettings.json", true);
                }
                catch
                {
                    Directory_Move(BACK_AGENT_DIRECTORY, Directory.GetCurrentDirectory(), true);
                    throw;
                }
                return true;
            }
            catch (Exception ex)
            {
                File.WriteAllText(ERROR_FILE, ex.ToString());
                throw;
            }
        }

        /// <summary>Обновление UI</summary>
        /// <returns>Результат выполнения обновления</returns>
        public async Task<bool> UpdateUiService()
        {
            try
            {
                bool CurrentVersionNotFound = false;
                string MmeCode;
                string ServiceDirectory = Path.GetDirectoryName(Program.ConfigData.ServiceAppPath);
                string VersionFileName = Path.Combine(Path.GetDirectoryName(Program.ConfigData.UIAppPath), "Version.txt");
                UriBuilder UriBuilder = new UriBuilder(new Uri(new Uri(Program.ConfigData.UpdateServerUrl), EQUAL_SOFTWARE_VERSION));
                NameValueCollection Query = HttpUtility.ParseQueryString(UriBuilder.Query);
                //Проверка существования файлов службы и версии
                bool VariantOne = File.Exists(Program.ConfigData.ServiceAppPath);
                bool VariantTwo = File.Exists(VersionFileName);
                //Оба файла отсутствуют
                if (!VariantOne && !VariantTwo)
                {
                    MmeCode = Program.ConfigData.MMECode;
                    if (MessageBox.Show(string.Format(@"Текущая версия не найдена. Установить последнюю версию {0} ? {1}", MmeCode, Environment.NewLine), "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.No)
                        return false;
                    CurrentVersionNotFound = true;
                    Query["currentVersion"] = "0.0.0.0";
                }
                else
                {
                    XmlDocument XmlDocument = new XmlDocument();
                    XmlDocument.Load(Path.Combine(Path.GetDirectoryName(Program.ConfigData.ServiceAppPath), "SCME.UIServiceConfig.dll.config"));
                    MmeCode = XmlDocument.SelectNodes("configuration/applicationSettings/SCME.UIServiceConfig.Properties.Settings/setting").Cast<XmlNode>().Single(node => node.Attributes["name"].Value == "MMECode").InnerText;
                    //Отсутствует файл текущей версии
                    if (VariantTwo)
                        Query["currentVersion"] = File.ReadAllText(Path.Combine(VersionFileName));
                    else
                        Query["currentVersion"] = FileVersionInfo.GetVersionInfo(Program.ConfigData.UIAppPath).ProductVersion;

                }
                Query["mme"] = MmeCode;
                UriBuilder.Query = Query.ToString();
                //Получение версии с сервера
                string VersionsEqual = await Action_RepeatAsync(() => Client.GetStringAsync(UriBuilder.Uri));
                if (VersionsEqual == "null")
                {
                    MessageBox.Show(string.Format("{0} не найден на сервере", MmeCode));
                    return true;
                }
                //Версии совпадают
                if (Convert.ToBoolean(VersionsEqual))
                    return true;
                //Получение новой версии UI
                byte[] NewVersionBytes = await Action_RepeatAsync(() => Client.GetByteArrayAsync(new Uri(new Uri(Program.ConfigData.UpdateServerUrl), $"{DOWNLOAD_SCME_UI_SERVICE_FILE_PATH}?mme={MmeCode}")));
                if (!CurrentVersionNotFound)
                {
                    if (Directory.Exists(BACK_UI_SERVICE_DIRECTORY))
                        Directory.Delete(BACK_UI_SERVICE_DIRECTORY, true);
                    Directory_Move(ServiceDirectory, BACK_UI_SERVICE_DIRECTORY, true);
                }
                try
                {
                    //Архивация новой версии
                    await using MemoryStream Stream = new MemoryStream(NewVersionBytes);
                    using ZipArchive Archive = new ZipArchive(Stream, ZipArchiveMode.Read, false);
                    Archive.ExtractToDirectory(ServiceDirectory, true);
                }
                catch
                {
                    if (!CurrentVersionNotFound)
                        Directory_Move(BACK_UI_SERVICE_DIRECTORY, ServiceDirectory, true);
                    throw;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Ошибка");
                File.WriteAllText(ERROR_FILE, ex.ToString());
                throw;
            }
            return true;
        }

        private static async Task<T> Action_RepeatAsync<T>(Func<Task<T>> action, string exceptionMessage = null) //Попытка выполнения действия асинхронно
        {
            for (int i = 0; i < COUNT_REPEAT; i++)
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
    }
}