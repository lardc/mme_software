using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace SCME.UpdateServer.Controllers
{
    [Route("{controller}/{action}")]
    public class UpdateController : ControllerBase
    {
        private readonly UpdateDataConfig _config;

        public UpdateController(IOptions<UpdateDataConfig> config)
        {
            _config = config.Value;
        }

        public class InMemoryFile
        {
            public string FileName { get; set; }
            public byte[] Content { get; set; }
        }

        [HttpGet]
        public string GetAgentVersion() => FileVersionInfo.GetVersionInfo(Path.Combine(_config.DataPathRoot, _config.ScmeAgentFolderName, _config.ScmeAgentExeName)).ProductVersion;

        [HttpGet]
        public async Task<IActionResult> GetAgentFolder() =>  File((await ZipAndXmlHelper.GetZipStreamAsync(Path.Combine(_config.DataPathRoot, _config.ScmeAgentFolderName))).ToArray(), "application/octet-stream");

        [HttpGet]
        public string EqualSoftwareVersion(string mme, string currentSoftwareFolderName) => (_config.MmeParameters.SingleOrDefault(m => m.Name == mme)?.Folder.Equals(currentSoftwareFolderName) ?? false).ToString();

        [HttpGet]
        public async Task<IActionResult> GetSoftwareFolder(string mme)
        {
            var mmeParameter = _config.MmeParameters.Single(m => m.Name == mme);

            var zipStreamAsync = await ZipAndXmlHelper.GetZipStreamAsync(Path.Combine(_config.DataPathRoot, mmeParameter.Folder));

            //Чтобы в MemoryStream записались изменения должен быть вызван метод Dispose для ZipArchive
            using (var zipArchive = new ZipArchive(zipStreamAsync, ZipArchiveMode.Update, true))
            {
                var oldEntry = zipArchive.Entries.Single(m => m.Name == _config.ScmeUiConfigName);

                var newEntryBytes = await ZipAndXmlHelper.ReplaceConfig("SCME.UI.Properties.Settings", oldEntry, mmeParameter);

                oldEntry.Delete();

                var modifiedEntry = zipArchive.CreateEntry(oldEntry.Name);
                await modifiedEntry.Open().WriteAsync(newEntryBytes);
            }

            zipStreamAsync.Position = 0;
            return File(zipStreamAsync, "application/octet-stream");
        }
    }
}