using SCME.DatabaseServer.Properties;
using System;
using System.Configuration.Install;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.ServiceProcess;
using System.Windows.Forms;

namespace SCME.DatabaseServer
{
    static class Program
    {
        //Иконка в трэе
        private static NotifyIcon NotifyIcon;

        private static void Main(string[] args)
        {
            //Добавление обработчика исключений
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            if (!Environment.UserInteractive)
            {
                ServiceBase.Run(new DatabaseService());
                return;
            }
            //Конкатинация параметров
            string Parameter = string.Concat(args);
            switch (Parameter)
            {
                case "--install":
                    Process_StartElevated("--installElevated");
                    break;
                case "--uninstall":
                    Process_StartElevated("--uninstallElevated");
                    break;
                case "--installElevated":
                    ManagedInstallerClass.InstallHelper(new[] { Assembly.GetExecutingAssembly().Location });
                    break;
                case "--uninstallElevated":
                    ManagedInstallerClass.InstallHelper(new[] { "/u", Assembly.GetExecutingAssembly().Location });
                    break;
                default:
                    Process_StartAsApplication();
                    break;
            }
        }

        private static void Process_StartElevated(string args) //Запуск процесса
        {
            ProcessStartInfo Info = new ProcessStartInfo(Assembly.GetEntryAssembly().Location, args)
            {
                Verb = @"runas"
            };
            using (Process Process = new Process())
            {
                Process.StartInfo = Info;
                Process.Start();
            }
        }

        private static void Process_StartAsApplication() //Запуск как приложение
        {
            SystemHost.StartService();
            TrayObject_Create();
            Application.ApplicationExit += Application_Exit;
            Application.Run();
        }

        private static void TrayObject_Create() //Создание объекта в трэе
        {
            Icon Icon = Resources.ServiceIcon;
            NotifyIcon = new NotifyIcon
            {
                Text = string.Format(@"SCME.DatabaseServer: {0}:{1}", SystemHost.GetHost(), SystemHost.GetPort()),
                Icon = new Icon(Icon, Icon.Width, Icon.Height),
                ContextMenu = new ContextMenu(new[]
                {
                    new MenuItem("Выход", NotifyIconButtonExit_Click)
                }),
                Visible = true
            };
        }

        static void Application_Exit(object sender, EventArgs e) //Остановка службы
        {
            SystemHost.StopService();
        }

        private static void NotifyIconButtonExit_Click(object sender, EventArgs e) //Закрытие приложения
        {
            NotifyIcon.Visible = false;
            Application.Exit();
        }

        private static void CurrentDomain_UnhandledException(object Sender, UnhandledExceptionEventArgs e) //Обработчик исключений
        {
            SystemHost.LogCriticalErrorMessage((Exception)e.ExceptionObject);
        }
    }
}
