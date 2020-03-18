using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace SCME.UpdateServer
{
    public static class Program
    {
        private const string StartError = "CRITICAL_START_ERROR";
        private const string ValidateErrorFile = "VALIDATE_ERROR";
        private static FileStream _appSettingsLocker;
        private static string _appSetting = "appsettings.json";
        private static string _appSettingEditable = "appsettings.editable.json";
        public static void Main(string[] args)
        {
            Console.WriteLine($"Start {Guid.NewGuid()}");
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            
            File.Copy(_appSetting, _appSettingEditable, true);
            Task.Factory.StartNew(CheckSettings);
            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                File.WriteAllText(StartError, ex.ToString());
            }
            
        }

        private static void CheckSettings()
        {
            while (true)
            {
                Thread.Sleep(1000);
                if (!File.Exists(_appSettingEditable))
                {
                    File.WriteAllText(ValidateErrorFile, $"{_appSettingEditable} has been deleted");
                    File.Copy(_appSetting, _appSettingEditable);
                    continue;
                }

                if (HashFile(_appSettingEditable).SequenceEqual(HashFile(_appSetting))) 
                    continue;
                try
                {
                    new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(_appSettingEditable, true, true).Build();
                    _appSettingsLocker.Close();
                    File.Copy(_appSettingEditable, _appSetting, true);
                    _appSettingsLocker = File.Open(_appSetting, FileMode.Open, FileAccess.Read, FileShare.Read);
                }
                catch (Exception e)
                {
                    File.WriteAllText(ValidateErrorFile, e.ToString());
                }
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private static byte[] HashFile(string filename)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filename);
            return md5.ComputeHash(stream);
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            _appSettingsLocker = File.Open(_appSetting, FileMode.Open, FileAccess.Read, FileShare.Read);
            var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(_appSetting,  true, true).Build();
            
            return Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.AddFilter("Microsoft", LogLevel.Warning);
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls(config.GetValue<string>("HostUrl"));
                    webBuilder.UseKestrel();
                    webBuilder.UseStartup<Startup>();
                });
        }
    }
}
