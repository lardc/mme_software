using System;
using System.IO;
using System.ServiceModel;
using System.Threading;
using System.Windows.Forms;
using SCME.DatabaseBackup.Properties;
using SCME.Logger;
using SCME.Types;
using Timer = System.Threading.Timer;

namespace SCME.DatabaseBackup
{
    public partial class MainForm : Form
    {
        private const string SERVER_ENDPOINT_NAME = "SCME.Service.DBMaintenanceServer";

        private readonly Timer m_NetPingTimer;
        private DatabaseMaintenanceProxy m_DatabaseClient;
        private volatile bool m_IsServerConnected;

        public MainForm()
        {
            InitializeComponent();
        
            m_NetPingTimer = new Timer(TimerCallback, null, Timeout.Infinite, 1000);
        }

        private void TimerCallback(object State)
        {
            if (!m_IsServerConnected)
                return;

            try
            {
                m_DatabaseClient.Check();
            }
            catch (FaultException<FaultData>)
            {
            }
            catch (Exception)
            {
            }
        }

        private void BtnSaveFile_Click(object Sender, EventArgs E)
        {
            if (saveDatabaseDialog.ShowDialog() == DialogResult.OK)
                tbDBFile.Text = saveDatabaseDialog.FileName;
        }

        private void BtnConnect_Click(object Sender, EventArgs E)
        {
            if (!m_IsServerConnected)
            {
                try
                {
                    m_DatabaseClient = new DatabaseMaintenanceProxy(SERVER_ENDPOINT_NAME,
                                                                    String.Format(Settings.Default.ServerAddressTemplate,
                                                                                  tbAddress.Text));

                    m_DatabaseClient.Open();
                    m_IsServerConnected = true;
                    m_NetPingTimer.Change(1000, 1000);

                    tsslStatus.Text = @"Подключено";
                    btnConnect.Text = @"Отключиться";
                    btnBackup.Enabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, @"Ошибка соединения", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                try
                {
                    m_NetPingTimer.Change(Timeout.Infinite, 1000);
                    m_IsServerConnected = false;

                    m_DatabaseClient.Close();
                    m_DatabaseClient = null;

                    tsslStatus.Text = @"Отключено";
                    btnConnect.Text = @"Подключиться";
                    btnBackup.Enabled = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, @"Ошибка соединения", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnBackup_Click(object Sender, EventArgs E)
        {
            try
            {
                File.Copy(Settings.Default.DBTemplateName, tbDBFile.Text, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Ошибка копирования шаблона БД", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var journal = new ResultsJournal();
            journal.Open(tbDBFile.Text, Settings.Default.DBOptions);
            var tableNames = journal.GetTableNamesList();


        }
    }
}
