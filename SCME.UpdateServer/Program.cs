using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace SCME.UpdateServer
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) 
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false).Build(); 

            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls(config.GetValue<string>("HostUrl"));
                    webBuilder.UseKestrel();
                    webBuilder.UseStartup<Startup>();
                });
        }
    }
}
