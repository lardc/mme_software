using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace SCME.UpdateServer
{
    /// <summary>Инициализатор RESTful-сервиса</summary>
    public class Startup
    {
        public const string LOGS_DIRECTORY = "Logs";

        public Startup(IConfiguration configuration) //Запуск RESTful-сервиса
        {
            if (!Directory.Exists(LOGS_DIRECTORY))
                Directory.CreateDirectory(LOGS_DIRECTORY);
            Configuration = configuration;
        }

        /// <summary>Конфигурация</summary>
        private IConfiguration Configuration
        {
            get;
        }

        public void ConfigureServices(IServiceCollection services) //Настройка RESTful-сервиса
        {
            services.AddControllers();
            services.AddOptions();
            services.Configure<UpdateDataConfig>(Configuration.GetSection("UpdateDataConfig"));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) //Настройка маршрутов и конечных точек
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            app.UseExceptionHandler("/Update/Error");
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
