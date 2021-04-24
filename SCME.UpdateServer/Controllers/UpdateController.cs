using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace SCME.UpdateServer.Controllers
{
    //Стандартный маршрут
    [Route("{controller}/{action}")]

    public class UpdateController : ControllerBase
    {
        //Размер пакета передачи и конфигурационный файл
        private const int SIZE_PACKET = 1024 * 1024;
        private readonly UpdateDataConfig Config;

        /// <summary>Инициализирует новый экземпляр класса UpdateController</summary>
        /// <param name="config">Конфигурация сервиса</param>
        public UpdateController(IOptionsSnapshot<UpdateDataConfig> config)
        {
            Config = config.Value;
        }

        /// <summary>Возникла ошибка при выполнении</summary>
        public void Error()
        {
            IExceptionHandlerPathFeature ExceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            System.IO.File.AppendAllText(Path.GetFullPath(Path.Combine(Startup.LOGS_DIRECTORY, $"{DateTime.Now:s}.txt".Replace(':', '-'))), $"{ExceptionHandlerPathFeature.Path} {ExceptionHandlerPathFeature.Error}");
        }

        /// <summary>Получить параметр Debug</summary>
        /// <returns>Debug-параметр</returns>
        [HttpGet]
        public string DebugParameter()
        {
            return Config.DebugParameter;
        }

        /// <summary>Получить версию агента</summary>
        /// <returns>Версия агента</returns>
        [HttpGet]
        public string GetAgentVersion()
        {
            return FileVersionInfo.GetVersionInfo(Path.Combine(Config.DataPathRoot, Config.ScmeAgentFolderName, Config.ScmeAgentExeName)).ProductVersion;
        }

        /// <summary>Получить расположение папки агента</summary>
        /// <returns>Расположение папки агента</returns>
        [HttpGet]
        public void GetAgentFolder()
        {
            string ZipFileName = Guid.NewGuid().ToString();
            try
            {
                string FolderName = Path.Combine(Config.DataPathRoot, Config.ScmeAgentFolderName);
                using (FileStream Stream = System.IO.File.Open(ZipFileName, FileMode.Create, FileAccess.ReadWrite))
                using (ZipArchive Archive = new ZipArchive(Stream, ZipArchiveMode.Create, true))
                    foreach (string obj in ZipAndXmlHelper.DirectorySearch(FolderName))
                        Archive.CreateEntryFromFile(obj, obj.Substring(FolderName.Length + 1));
                ReturnFileInParts(ZipFileName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                try
                {
                    System.IO.File.Delete(ZipFileName);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            GC.Collect(2, GCCollectionMode.Forced, true);
            GC.WaitForPendingFinalizers();
        }

        /// <summary>Получить текущую версию на сервере</summary>
        /// <param name="mme">Mme-код</param>
        /// <param name="currentVersion">Текущая версия на станции</param>
        /// <returns>Текущая версию на сервере</returns>
        [HttpGet]
        public string EqualSoftwareVersion(string mme, string currentVersion)
        {
            MmeParameter MmeParameter = Config.MmeParameters.SingleOrDefault(m => m.Name == mme);
            if (MmeParameter == null)
                return "null";
            string UiExeFileName = Path.Combine(Config.DataPathRoot, MmeParameter.Folder, Config.ScmeUIExeName);
            string VersionFileName = Path.Combine(UiExeFileName, Path.GetDirectoryName(UiExeFileName), "Version.txt");
            //Файл с версией существует
            bool VariantTwo = System.IO.File.Exists(VersionFileName);
            //Возвращение версии либо из файла, либо из версии сборки
            return VariantTwo ? (currentVersion == System.IO.File.ReadAllText(VersionFileName)).ToString().Trim() : (currentVersion == FileVersionInfo.GetVersionInfo(UiExeFileName).ProductVersion).ToString();
        }

        /// <summary>Получить архив папки с проектом</summary>
        /// <param name="mme">Mme-код</param>
        [HttpGet]
        public void GetSoftwareFolder(string mme)
        {
            string ZipFileName = Guid.NewGuid().ToString();
            try
            {
                MmeParameter MmeParameter = Config.MmeParameters.Single(m => m.Name == mme);
                //Получение соответствуещего расположения папки
                string FolderName = Path.Combine(Config.DataPathRoot, MmeParameter.Folder);
                FileStream Stream;
                //Архивация папки
                using (Stream = System.IO.File.Open(ZipFileName, FileMode.Create, FileAccess.ReadWrite))
                using (ZipArchive Archive = new ZipArchive(Stream, ZipArchiveMode.Create, true))
                    foreach (string Element in ZipAndXmlHelper.DirectorySearch(FolderName))
                    {
                        string EntryName = Element.Substring(FolderName.Length + 1);
                        if (Config.ScmeCommonConfigName == EntryName || $@"UI\{Config.ScmeCommonConfigName}" == EntryName)
                        {
                            ZipArchiveEntry Entry = Archive.CreateEntry(EntryName);
                            using Stream NewStream = Entry.Open();
                            NewStream.Write(new ReadOnlySpan<byte>(ZipAndXmlHelper.GetChangedConfig(Element, MmeParameter)));
                        }
                        else
                            Archive.CreateEntryFromFile(Element, EntryName);
                    }
                //Передача архива по частям
                ReturnFileInParts(ZipFileName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                try
                {
                    //Удаление архива
                    System.IO.File.Delete(ZipFileName);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            //Вызов GC
            GC.Collect(2, GCCollectionMode.Forced, true);
            GC.WaitForPendingFinalizers();
        }

        /// <summary>Получить файл частями</summary>
        /// <param name="fileName">Имя файла</param>
        private void ReturnFileInParts(string fileName)
        {
            using FileStream Stream = System.IO.File.Open(fileName, FileMode.Open, FileAccess.Read);
            using BinaryReader Reader = new BinaryReader(Stream);
            long Length = Stream.Length;
            byte[] Bytes = new byte[SIZE_PACKET];
            Response.ContentType = "application/octet-stream";
            Response.ContentLength = Length;
            for (int i = 0; i < Stream.Length; i += SIZE_PACKET)
            {
                int CountReadBytes = Convert.ToInt32(i + SIZE_PACKET < Length ? SIZE_PACKET : Length - i);
                Reader.Read(Bytes, 0, CountReadBytes);
                Response.Body.WriteAsync(Bytes, 0, CountReadBytes).Wait();
            }
        }
    }
}