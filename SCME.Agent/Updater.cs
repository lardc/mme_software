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
        //Запросы к контроллеру UpdateServer
        private const string AgentVersionPath = "Update/GetAgentVersion";
        private const string AgentFolderPath = "Update/GetAgentFolder";
        private const string EqualSoftwareVersionPath = "Update/EqualSoftwareVersion";
        private const string UIServicePath = "Update/GetSoftwareFolder";
        private const string BackAgentPath = "../BackAGENT";
        private const string BackUIServicePath = "../BackUiService";
        private readonly HttpClient Client = new HttpClient();
        private const int Delay = 1000;
        private const int RepeatCount = 3;

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
                string ServerAgentVersionString = await Action_RepeatAsync(() => Client.GetStringAsync(new Uri(new Uri(Program.ConfigData.UpdateServerUrl), AgentVersionPath)));
                if (FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion == ServerAgentVersionString)
                    return false;
                //Получение новой версии агента
                byte[] NewVersionBytes = await Action_RepeatAsync(() => Client.GetByteArrayAsync(new Uri(new Uri(Program.ConfigData.UpdateServerUrl), AgentFolderPath)));
                //Бэкап предыдущей версии
                if (Directory.Exists(BackAgentPath))
                    Directory.Delete(BackAgentPath, true);
                Directory_Move(Directory.GetCurrentDirectory(), BackAgentPath, true);
                try
                {
                    //Архивация новой версии и перезапись конфигурационного json-файла
                    await using MemoryStream Stream = new MemoryStream(NewVersionBytes);
                    using ZipArchive Archive = new ZipArchive(Stream, ZipArchiveMode.Read, false);
                    Archive.ExtractToDirectory(Directory.GetCurrentDirectory(), true);
                    File.Copy(Path.Combine(BackAgentPath, "appsettings.json"), "appsettings.json", true);
                }
                catch (Exception ex)
                {
                    Directory_Move(BackAgentPath, Directory.GetCurrentDirectory(), true);
                    MessageBox.Show("Не удалось обновить SCME.Agent", "Ошибка");
                    //Запись логов ошибки
                    if (Directory.Exists("Logs"))
                    {
                        string ErrorMessage = string.Format("{0:dd.MM.yyyy HH:mm:ss} ERROR - Couldn't update SCME.Agent. Reason:\n{1}\n", DateTime.Now, ex);
                        File.AppendAllText(Program.LogFilePath, ErrorMessage);
                    }
                    throw;
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось обновить SCME.Agent", "Ошибка");
                //Запись логов ошибки
                if (Directory.Exists("Logs"))
                {
                    string ErrorMessage = string.Format("{0:dd.MM.yyyy HH:mm:ss} ERROR - Couldn't update SCME.Agent. Reason:\n{1}\n", DateTime.Now, ex);
                    File.AppendAllText(Program.LogFilePath, ErrorMessage);
                }
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
                UriBuilder UriBuilder = new UriBuilder(new Uri(new Uri(Program.ConfigData.UpdateServerUrl), EqualSoftwareVersionPath));
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
                byte[] NewVersionBytes = await Action_RepeatAsync(() => Client.GetByteArrayAsync(new Uri(new Uri(Program.ConfigData.UpdateServerUrl), $"{UIServicePath}?mme={MmeCode}")));
                if (!CurrentVersionNotFound)
                {
                    if (Directory.Exists(BackUIServicePath))
                        Directory.Delete(BackUIServicePath, true);
                    Directory_Move(ServiceDirectory, BackUIServicePath, true);
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
                        Directory_Move(BackUIServicePath, ServiceDirectory, true);
                    throw;
                }
                MessageBox.Show(string.Format("SCME.UIService обновлен до версии {0}", FileVersionInfo.GetVersionInfo(Program.ConfigData.UIAppPath).ProductVersion), "Обновление выполнено");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось обновить SCME.UIService", "Ошибка");
                //Запись логов ошибки
                if (Directory.Exists("Logs"))
                {
                    string ErrorMessage = string.Format("{0:dd.MM.yyyy HH:mm:ss} ERROR - Couldn't update SCME.UIService. Reason:\n{1}\n", DateTime.Now, ex);
                    File.AppendAllText(Program.LogFilePath, ErrorMessage);
                }
                throw;
            }
            return true;
        }

        private static async Task<T> Action_RepeatAsync<T>(Func<Task<T>> action, string exceptionMessage = null) //Попытка выполнения действия асинхронно
        {
            for (int i = 0; i < RepeatCount; i++)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex)
                {
                    if (i == RepeatCount - 1)
                        throw string.IsNullOrEmpty(exceptionMessage) ? ex : new Exception(exceptionMessage, ex);
                    Thread.Sleep(Delay);
                }
            }
            throw new NotImplementedException();
        }
    }
}