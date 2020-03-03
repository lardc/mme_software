using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using SCME.Agent.Properties;
// ReSharper disable InvertIf

namespace SCME.Agent
{
    internal class Supervisor
    {
        private readonly Process _pService, _pUserInterface;
        private bool _restartService = true;
        public bool NeedRestart = false;
        internal Supervisor()
        {
            var ico = Resources.TrayIconPE;
            var toolStripButton = new ToolStripButton()
            {
                Text = @"Exit"
            };
            toolStripButton.Click += (sender, args) => Application.Exit();
            var contextMenuStrip = new ContextMenuStrip();
            contextMenuStrip.Items.Add(toolStripButton);
            var notifyIcon = new NotifyIcon
            {
                Text = @"SCME.Agent",
                Icon = new Icon(ico, ico.Width, ico.Height),
                ContextMenuStrip = contextMenuStrip,
                Visible = true
            };


            _pService = new Process
            {
                StartInfo =
                {
                    FileName = Program.ConfigData.ServiceAppPath,
                    WorkingDirectory =
                        Path.GetDirectoryName(Program.ConfigData.ServiceAppPath) ??
                        Environment.CurrentDirectory,
                    ErrorDialog = false
                },
                EnableRaisingEvents = true
            };
            _pService.Exited += PServiceOnExited;

            if (Program.ConfigData.IsUserInterfaceEnabled)
            {
                _pUserInterface = new Process
                {
                    StartInfo =
                    {
                        FileName = Program.ConfigData.UIAppPath,
                        WorkingDirectory =
                            (Path.GetDirectoryName(Program.ConfigData.UIAppPath)) ??
                            Environment.CurrentDirectory,
                        ErrorDialog = false
                    },
                    EnableRaisingEvents = true
                };

                _pUserInterface.Exited += PUserInterfaceOnExited;
            }

        }

        private void PUserInterfaceOnExited(object sender, EventArgs e)
        {
            _restartService = false;
            _pService.Kill();
            _pService.WaitForExit();
            NeedRestart = true;
            Application.Exit();
        }

        private void PServiceOnExited(object sender, EventArgs e)
        {
            if(_restartService)
                StartProcess(_pService);
        }

        internal void Start()
        {
            StartProcess(_pService);

            if (Program.ConfigData.IsUserInterfaceEnabled)
                StartProcess(_pUserInterface);
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