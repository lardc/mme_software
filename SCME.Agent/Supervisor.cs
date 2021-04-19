using SCME.Agent.Properties;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SCME.Agent
{
    internal class Supervisor
    {
        //Процессы службы и UI на рабочей станции
        private readonly Process PService, PUserInterface;
        private bool RestartService = true;
        public bool NeedsRestart = false;
        
        /// <summary>Инициализирует новый экземпляр класса Supervisor</summary>
        internal Supervisor()
        {
            TrayObject_Create();
            //Сервис на рабочей станции
            PService = new Process
            {
                StartInfo =
                {
                    FileName = Program.ConfigData.ServiceAppPath,
                    WorkingDirectory = Path.GetDirectoryName(Program.ConfigData.ServiceAppPath) ?? Environment.CurrentDirectory,
                    ErrorDialog = false
                },
                EnableRaisingEvents = true
            };
            PService.Exited += PService_Exited;
            //UI на рабочей станции
            if (Program.ConfigData.IsUserInterfaceEnabled)
            {
                PUserInterface = new Process
                {
                    StartInfo =
                    {
                        FileName = Program.ConfigData.UIAppPath,
                        WorkingDirectory = Path.GetDirectoryName(Program.ConfigData.UIAppPath) ??Environment.CurrentDirectory,
                        ErrorDialog = false
                    },
                    EnableRaisingEvents = true
                };
                PUserInterface.Exited += PUserInterface_Exited;
            }
        }

        internal void Start() //Запуск супервайзера
        {
            StartProcess(PService);
            //Запуск UI при необходимости
            if (Program.ConfigData.IsUserInterfaceEnabled)
                StartProcess(PUserInterface);
        }

        private void TrayObject_Create() //Создание объекта в трэе
        {
            Icon Icon = Resources.TrayIconPE;
            ToolStripButton ToolStripButton = new ToolStripButton()
            {
                Text = @"Exit"
            };
            ToolStripButton.Click += ToolStripButtonExit_Click;
            ContextMenuStrip ContextMenuStrip = new ContextMenuStrip();
            ContextMenuStrip.Items.Add(ToolStripButton);
            NotifyIcon NotifyIcon = new NotifyIcon
            {
                Text = "SCME.Agent",
                Icon = new Icon(Icon, Icon.Width, Icon.Height),
                ContextMenuStrip = ContextMenuStrip,
                Visible = true
            };
        }

        private void ToolStripButtonExit_Click(object sender, EventArgs e) //Закрытие супервайзера
        {
            PService.Exited -= PService_Exited;
            PUserInterface.Exited -= PUserInterface_Exited;
            PService.Kill();
            PService.WaitForExit();
            PUserInterface.Kill();
            PUserInterface.WaitForExit();
            Application.Exit();
        }

        private void PUserInterface_Exited(object sender, EventArgs e) //Выключение UI
        {
            RestartService = false;
            PService.Kill();
            PService.WaitForExit();
            NeedsRestart = true;
            Application.Exit();
        }

        private void PService_Exited(object sender, EventArgs e) //Выключение сервиса
        {
            if (RestartService)
                StartProcess(PService);
        }        

        private static void StartProcess(Process process) //Запуск процесса
        {
            try
            {
                //Проверка существования процесса
                Process[] ProcessesByName = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(process.StartInfo.FileName));
                //Процесс не запущен
                if (ProcessesByName.Length == 0)
                    process.Start();
            }
            catch (Exception ex)
            {
                string report = string.Format(Resources.Log_Message_Process_error, process.StartInfo.FileName, ex.Message);
                MessageBox.Show(report, Resources.Error_Caption_Supervisor_error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
        }
    }
}