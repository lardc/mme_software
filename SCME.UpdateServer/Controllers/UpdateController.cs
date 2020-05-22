using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace SCME.UpdateServer.Controllers
{
    [Route("{controller}/{action}")]
    public class UpdateController : ControllerBase
    {
        private const int SIZE_PACKET = 1024 * 1024;
        private readonly UpdateDataConfig _config;

        public UpdateController(IOptionsSnapshot<UpdateDataConfig> config)
        {
            _config = config.Value;
        }

        public void Error()
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            System.IO.File.AppendAllText(Path.GetFullPath(Path.Combine(Startup.LOGS_DIRECTORY, $"{DateTime.Now:s}.txt".Replace(':', '-'))), $"{exceptionHandlerPathFeature.Path} {exceptionHandlerPathFeature.Error}");
        }

        [HttpGet]
        public string DebugParameter() => _config.DebugParameter;

        [HttpGet]
        public string GetAgentVersion() => FileVersionInfo.GetVersionInfo(Path.Combine(_config.DataPathRoot, _config.ScmeAgentFolderName, _config.ScmeAgentExeName)).ProductVersion;

        [HttpGet]
        public void GetAgentFolder()
        {
            var zipFileName = Guid.NewGuid().ToString();

            try
            {
                var folderName = Path.Combine(_config.DataPathRoot, _config.ScmeAgentFolderName);
                FileStream fileStream;

                using (fileStream = System.IO.File.Open(zipFileName, FileMode.Create, FileAccess.ReadWrite))
                using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
                    foreach (var i in ZipAndXmlHelper.DirectorySearch(folderName))
                        archive.CreateEntryFromFile(i, i.Substring(folderName.Length + 1));

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

        [HttpGet]
        public string EqualSoftwareVersion(string mme, string currentVersion)
        {
            var mmeParameter = _config.MmeParameters.SingleOrDefault(m => m.Name == mme);

            if (mmeParameter == null)
                return "null";

            var uiExeFileName = Path.Combine(_config.DataPathRoot, mmeParameter.Folder, _config.ScmeUIExeName);
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
                var mmeParameter = _config.MmeParameters.Single(m => m.Name == mme);
                var folderName = Path.Combine(_config.DataPathRoot, mmeParameter.Folder);

                FileStream fileStream;

                using (fileStream = System.IO.File.Open(zipFileName, FileMode.Create, FileAccess.ReadWrite))
                using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
                    foreach (var i in ZipAndXmlHelper.DirectorySearch(folderName))
                    {
                        var entryName = i.Substring(folderName.Length + 1);
                        if (_config.ScmeCommonConfigName == entryName || $@"UI\{_config.ScmeCommonConfigName}" == entryName)
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