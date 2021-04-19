using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace SCME.Agent
{

    internal static class Program
    {
        private static Supervisor Supervisor;
        public static ConfigData ConfigData;

        [STAThread]
        private static void Main()
        {
            //Добавление обработчика исключений
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            //Создание конфигурационного json-файла
            IConfigurationBuilder configBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            var configuration = configBuilder.Build();
            ConfigData = configuration.GetSection(nameof(ConfigData)).Get<ConfigData>();
            //Режим отладки
            if(ConfigData.DebugUpdate)
                try
                {
                    File.WriteAllText("Version info.txt", FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion);
                }
                catch (Exception ex)
                {
                    File.WriteAllText("Debug update.txt", ex.ToString());
                }
            //Корневая директория
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new NullReferenceException());
            //Синхронизация процессов обновления и перезапуска проектов
            Mutex mutex = null;
            bool mutexCreated = false;
            for (int i = 0; i < 3; ++i)
            {
                mutex = new Mutex(true, @"Global\SCME.AGENT.Mutex", out mutexCreated);
                if (mutexCreated)
                    break;
                Thread.Sleep(500);
            }
            if (!mutexCreated)
            {
                Process.Start("explorer.exe");
                return;
            }
            try
            {
                //Обновление проектов
                Updater updater = new Updater();
                bool agentIsUpdated = updater.UpdateAgent().Result;
                if (agentIsUpdated)
                {
                    Process.Start(Path.ChangeExtension(Application.ExecutablePath, "exe"));
                    return;
                }
                if (!updater.UpdateUiService().Result)
                    return;
            }
            catch(Exception ex)
            {
                MessageBox.Show(string.Format("Возникла одна или несколько ошибок при обновлении ПО, попытка запуска. {0}{1}", Environment.NewLine, ex), "Ошибка");
            }
            //Перезапуск супервайзера
            using (mutex)
            {
                Supervisor = new Supervisor();
                Supervisor.Start();
                Application.Run();
                mutex.ReleaseMutex();
                if (Supervisor.NeedsRestart)
                    Process.Start(Path.ChangeExtension(Application.ExecutablePath, "exe"));
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) //Обработчик исключений
        {
            File.WriteAllText("CRITICAL ERROR.txt", e.ExceptionObject.ToString());
        }
    }
}