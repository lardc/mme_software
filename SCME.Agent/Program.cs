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

namespace SCME.Agent
{
    internal static class Program
    {
        private static Supervisor _supervisor;

        

        [STAThread]
        private static void Main()
        {
            MessageBox.Show(Application.ProductVersion);
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
            if (updater.Update().Result)
            {
                Process.Start(Assembly.GetExecutingAssembly().Location);
                return;
            }

            using (mutex)
            {
                _supervisor = new Supervisor();
                _supervisor.Start();

                Application.Run();
            }
        }
    }
}