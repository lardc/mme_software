using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace SCME.UpdateServer
{
    public static class Program
    {
        //Текст ошибок, файлы с настройками
        private const string StartError = "CRITICAL_START_ERROR";
        private const string ValidateErrorFile = "VALIDATE_ERROR";
        private static FileStream AppSettingsLocker;
        private static string AppSetting = "appsettings.json";
        private static string AppSettingEditable = "appsettings.editable.json";

        public static void Main(string[] args)
        {
            Console.WriteLine(string.Format("Start {0}", Guid.NewGuid()));
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            File.Copy(AppSetting, AppSettingEditable, true);
            Task.Factory.StartNew(Settings_Check);
            try
            {
                HostBuilder_Create(args).Build().Run();
            }
            catch (Exception ex)
            {
                File.WriteAllText(StartError, ex.ToString());
            }
        }

        private static void Settings_Check() //Проверка файлов с настройками
        {
            while (true)
            {
                Thread.Sleep(1000);
                //Проверка существования файла с настройками
                if (!File.Exists(AppSettingEditable))
                {
                    File.WriteAllText(ValidateErrorFile, string.Format("{0} has been deleted", AppSettingEditable));
                    File.Copy(AppSetting, AppSettingEditable);
                    continue;
                }
                //Проверка хэша файлов с настройками (идентичные файлы)
                if (HashFile_Calculate(AppSettingEditable).SequenceEqual(HashFile_Calculate(AppSetting)))
                    continue;
                try
                {
                    //Замена файла с настройками
                    new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(AppSettingEditable, true, true).Build();
                    AppSettingsLocker.Close();
                    File.Copy(AppSettingEditable, AppSetting, true);
                    AppSettingsLocker = File.Open(AppSetting, FileMode.Open, FileAccess.Read, FileShare.Read);
                }
                catch (Exception e)
                {
                    File.WriteAllText(ValidateErrorFile, e.ToString());
                }
            }
        }

        private static byte[] HashFile_Calculate(string filename) //Подсчет хэша файла
        {
            using MD5 Md5 = MD5.Create();
            using FileStream stream = File.OpenRead(filename);
            return Md5.ComputeHash(stream);
        }

        private static IHostBuilder HostBuilder_Create(string[] args) //Создание хоста
        {
            AppSettingsLocker = File.Open(AppSetting, FileMode.Open, FileAccess.Read, FileShare.Read);
            IConfigurationRoot Config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(AppSetting, true, true).Build();
            return Host.CreateDefaultBuilder(args).ConfigureLogging(logging =>
            {
                logging.AddFilter("Microsoft", LogLevel.Warning);
                logging.ClearProviders();
                logging.AddConsole();
            }).ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseUrls(Config.GetValue<string>("HostUrl"));
                webBuilder.UseKestrel();
                webBuilder.UseStartup<Startup>();
            });
        }
    }
}
