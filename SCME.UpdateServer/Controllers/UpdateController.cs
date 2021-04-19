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
                ReturnFileInPars(ZipFileName);
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

        /// <summary>Получить расположение папки агента</summary>
        /// <returns>Расположение папки агента</returns>
        [HttpGet]
        public string EqualSoftwareVersion(string mme, string currentVersion)
        {
            var mmeParameter = Config.MmeParameters.SingleOrDefault(m => m.Name == mme);

            if (mmeParameter == null)
                return "null";

            var uiExeFileName = Path.Combine(Config.DataPathRoot, mmeParameter.Folder, Config.ScmeUIExeName);
            // ReSharper disable once AssignNullToNotNullAttribute
            var versionFileName = Path.Combine(uiExeFileName, Path.GetDirectoryName(uiExeFileName), "Version.txt");

            var variantTwo = System.IO.File.Exists(versionFileName);
            
            return variantTwo ? (currentVersion == System.IO.File.ReadAllText(versionFileName)).ToString().Trim() : (currentVersion == FileVersionInfo.GetVersionInfo(uiExeFileName).ProductVersion).ToString();
        }

        private void ReturnFileInPars(string fileName)
        {
            using var fileStream = System.IO.File.Open(fileName, FileMode.Open, FileAccess.Read);
            using var br = new BinaryReader(fileStream);
            var length =  fileStream.Length;
            var bytes = new byte[SIZE_PACKET];
            
            Response.ContentType = "application/octet-stream";

            Response.ContentLength = length;
            for (var i = 0; i < fileStream.Length; i += SIZE_PACKET)
            {
                var countReadBytes = Convert.ToInt32(i + SIZE_PACKET < length ? SIZE_PACKET : length - i);
                br.Read(bytes, 0, countReadBytes);
                Response.Body.WriteAsync(bytes, 0, countReadBytes).Wait();
            }
        }

        [HttpGet]
        public void GetSoftwareFolder(string mme)
        {
            var zipFileName = Guid.NewGuid().ToString();
            try
            {
                var mmeParameter = Config.MmeParameters.Single(m => m.Name == mme);
                var folderName = Path.Combine(Config.DataPathRoot, mmeParameter.Folder);

                FileStream fileStream;

                using (fileStream = System.IO.File.Open(zipFileName, FileMode.Create, FileAccess.ReadWrite))
                using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
                    foreach (var i in ZipAndXmlHelper.DirectorySearch(folderName))
                    {
                        var entryName = i.Substring(folderName.Length + 1);
                        if (Config.ScmeCommonConfigName == entryName || $@"UI\{Config.ScmeCommonConfigName}" == entryName)
                        {
                            var entry = archive.CreateEntry(entryName);
                            using var stream = entry.Open();
                            stream.Write(new ReadOnlySpan<byte>(ZipAndXmlHelper.GetChangedConfig(i, mmeParameter)));
                        }
                        else
                            archive.CreateEntryFromFile(i, entryName);
                    }

                ReturnFileInPars(zipFileName);
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
	                if (System.IO.File.Exists(zipFileName))
	                    System.IO.File.Delete(zipFileName);
				}
	            catch (Exception e)
	            {
	                Console.WriteLine(e);
	            }
			}

            GC.Collect(2, GCCollectionMode.Forced, true);
            GC.WaitForPendingFinalizers();
        }
    }
}