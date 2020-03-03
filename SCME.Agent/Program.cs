using System;
using System.Diagnostics;
using System.Drawing.Text;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Security.Policy;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;

namespace SCME.Agent
{
    
    internal static class Program
    {
        private static Supervisor _supervisor;
        public static ConfigData ConfigData;
        
        [STAThread]
        private static void Main()
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            var configuration = builder.Build();
            ConfigData = configuration.GetSection(nameof(ConfigData)).Get<ConfigData>();
            
            if(ConfigData.DebugUpdate)
            {
                try
                {
                    File.WriteAllText("Version info.txt", FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion);
                }
                catch(Exception ex)
                {
                    File.WriteAllText("Debug update.txt", ex.ToString());
                }
            }

            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new NullReferenceException());
            
            //MessageBox.Show(Application.ProductVersion);
            Mutex mutex = null;
            var mutexCreated = false;

            for (var i = 0; i < 3; ++i)
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

            var updater = new Updater();
            var agentIsUpdated = updater.UpdateAgent().Result;
            if (agentIsUpdated)
            {
                Process.Start(Path.ChangeExtension(Application.ExecutablePath, "exe"));
                return;
            }

            updater.UpdateUiService().Wait();

            using (mutex)
            {
                _supervisor = new Supervisor();
                _supervisor.Start();

                Application.Run();
                mutex.ReleaseMutex();
                if(_supervisor.NeedRestart)
                    Process.Start(Path.ChangeExtension(Application.ExecutablePath, "exe"));
            }
        }
    }
}