using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace SCME.UpdateServer.Controllers
{
    [Route("{controller}/{action}")]
    public class UpdateController : ControllerBase
    {
        private const string SCME_AGENT_FOLDER_NAME = "SCME.Agent";
        private const string SCME_AGENT_EXE_NAME = "SCME.Agent.exe";

        private readonly MyConfig _config;

        public UpdateController(IOptions<MyConfig> config)
        {
            _config = config.Value;
        }

        public class InMemoryFile
        {
            public string FileName { get; set; }
            public byte[] Content { get; set; }
        }

        private async Task<byte[]> GetZipArchive(string folderName)
        {
            await using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, false))
                foreach (var i in Directory.GetFiles(folderName))
                {
                    var entry = archive.CreateEntry(Path.GetFileName(i));
                    await using var entryStream = entry.Open();
                    var bytes = await System.IO.File.ReadAllBytesAsync(i);
                    await entryStream.WriteAsync(bytes, CancellationToken.None);
                }

            return memoryStream.ToArray();
        }

        [HttpGet]
        public string GetAgentVersion() => FileVersionInfo.GetVersionInfo(Path.Combine(_config.DataPath, SCME_AGENT_FOLDER_NAME, SCME_AGENT_EXE_NAME)).ProductVersion;

        [HttpGet]
        public async Task<IActionResult> GetAgentFolder() => File(await GetZipArchive(Path.Combine(_config.DataPath, SCME_AGENT_FOLDER_NAME)), "application/octet-stream");
    }
}