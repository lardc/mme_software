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
        //Супервайзер и конфигурационные данные
        private static Supervisor Supervisor;
        public static ConfigData ConfigData;

        [STAThread]
        private static void Main()
        {
            //Добавление обработчика исключений
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            //Создание конфигурационного json-файла
            IConfigurationBuilder СonfigBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true);
            IConfigurationRoot Сonfiguration = СonfigBuilder.Build();
            ConfigData = Сonfiguration.GetSection(nameof(ConfigData)).Get<ConfigData>();
            //Режим отладки
            if (ConfigData.DebugUpdate)
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
            Mutex Mutex = null;
            bool MutexCreated = false;
            for (int i = 0; i < 3; ++i)
            {
                Mutex = new Mutex(true, @"Global\SCME.AGENT.Mutex", out MutexCreated);
                if (MutexCreated)
                    break;
                Thread.Sleep(500);
            }
            if (!MutexCreated)
            {
                Process.Start("explorer.exe");
                return;
            }
            try
            {
                //Обновление проектов
                Updater Updater = new Updater();
                bool AgentIsUpdated = Updater.UpdateAgent().Result;
                if (AgentIsUpdated)
                {
                    Process.Start(Path.ChangeExtension(Application.ExecutablePath, "exe"));
                    return;
                }
                if (!Updater.UpdateUiService().Result)
                    return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Возникла одна или несколько ошибок при обновлении ПО, попытка запуска. {0}{1}", Environment.NewLine, ex), "Ошибка");
            }
            //Перезапуск супервайзера
            using (Mutex)
            {
                Supervisor = new Supervisor();
                Supervisor.Start();
                Application.Run();
                Mutex.ReleaseMutex();
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