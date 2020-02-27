using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace SCME.UpdateServer
{
    public static class Program
    {
        private const string START_ERROR = "CRITICAL_START_ERROR";
        private static FileStream appSettingsLocker;
        public static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                File.WriteAllText(START_ERROR, ex.ToString());
            }
            
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            appSettingsLocker = File.Open("appsettings.json", FileMode.Open, FileAccess.Read, FileShare.Read);
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false).Build();
            
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
