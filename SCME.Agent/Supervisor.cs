using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using SCME.Agent.Properties;

namespace SCME.Agent
{
    internal class Supervisor
    {
        private readonly NotifyIcon _trayIcon;
        private readonly Process _pService, _pUserInterface, _pProxy;

        internal Supervisor()
        {
            var ico = Resources.TrayIconPE;
            _trayIcon = new NotifyIcon
                {
                    Text = @"SCME.Agent",
                    Icon = new Icon(ico, ico.Width, ico.Height),
                    ContextMenu = new ContextMenu(new[] {new MenuItem(@"Exit", OnExit)}),
                    Visible = true
                };

            _pProxy = new Process
                {
                    StartInfo =
                        {
                            FileName = Settings.Default.ProxyAppPath,
                            WorkingDirectory =
                                Path.GetDirectoryName(Settings.Default.ProxyAppPath) ??
                                Environment.CurrentDirectory,
                            ErrorDialog = false
                        },
                    EnableRaisingEvents = true
                };
            _pProxy.Exited += PExited;

            _pService = new Process
                {
                    StartInfo =
                        {
                            FileName = Settings.Default.ServiceAppPath,
                            WorkingDirectory =
                                Path.GetDirectoryName(Settings.Default.ServiceAppPath) ??
                                Environment.CurrentDirectory,
                            ErrorDialog = false
                        },
                    EnableRaisingEvents = true
                };
            _pService.Exited += PExited;

            if (Settings.Default.IsUserInterfaceEnabled)
            {
                _pUserInterface = new Process
                {
                    StartInfo =
                    {
                        FileName = Settings.Default.UIAppPath,
                        WorkingDirectory =
                            (Path.GetDirectoryName(Settings.Default.UIAppPath)) ??
                            Environment.CurrentDirectory,
                        ErrorDialog = false
                    },
                    EnableRaisingEvents = true
                };

                _pUserInterface.Exited += PExited;
            }

            Application.ApplicationExit += Application_ApplicationExit;
        }

        internal void Start()
        {
            if (Settings.Default.IsProxyEnabled)
            {
                StartProcess(_pProxy);
                Thread.Sleep(Settings.Default.ProxyInitDelayMs);
            }
            
            StartProcess(_pService);
            
            if (Settings.Default.IsUserInterfaceEnabled)
                StartProcess(_pUserInterface);
        }

        private static void PExited(object sender, EventArgs e)
        {
            var p = sender as Process;
            
            StartProcess(p);
        }

        private void OnExit(object sender, EventArgs e)
        {
            _trayIcon.Visible = false;
            Application.Exit();
        }

        private void Application_ApplicationExit(object sender, EventArgs e)
        {
            _trayIcon.Visible = false;
        }

        private static void StartProcess(Process p)
        {
            try
            {
                var processesByName = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(p.StartInfo.FileName));

                if (processesByName.Length == 0)
                    p.Start();
            }
            catch (Exception ex)
            {
                var str = string.Format(Resources.Log_Message_Process_error, p.StartInfo.FileName, ex.Message);

                MessageBox.Show(str, Resources.Error_Caption_Supervisor_error, MessageBoxButtons.OK,
                                MessageBoxIcon.Error);

                Environment.Exit(1);
            }
        }
    }
}