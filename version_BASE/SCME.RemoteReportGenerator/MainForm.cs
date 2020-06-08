using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Windows.Forms;
using SCME.ExcelPrinting;
using SCME.RemoteReportGenerator.Properties;
using SCME.Types;
using Timer = System.Threading.Timer;

namespace SCME.RemoteReportGenerator
{
    public partial class MainForm : Form
    {
        private const string DATABASE_SERVER_ENDPOINT_NAME = "SCME.Service.DatabaseServer";

        private readonly Timer m_NetPingTimer;
        private readonly List<DeviceItemWithParams> m_DeviceItemsWithParams;
        private readonly ExcelPrinter m_ExcelPrinter;
        private readonly MMEDictionary m_Dictionary;
        private DatabaseCommunicationProxy m_DatabaseClient;
        private volatile bool m_IsServerConnected;

        public MainForm()
        {
            InitializeComponent();

            m_DeviceItemsWithParams = new List<DeviceItemWithParams>();
            m_ExcelPrinter = new ExcelPrinter();

            m_Dictionary = new MMEDictionary(Settings.Default.MMEList);

            foreach (var arg in m_Dictionary.Records)
                cbMMEList.Items.Add(arg.MMECode);

            m_NetPingTimer = new Timer(TimerCallback, null, Timeout.Infinite, 10000);
            cbCustomer.SelectedIndex = 0;

            dtpFrom.Value = DateTime.Now.AddMonths(-1);
            dtpTo.Value = DateTime.Now.AddDays(1);
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

        private void BtnConnect_Click(object Sender, EventArgs E)
        {
            if (!m_IsServerConnected)
            {
                if (cbMMEList.SelectedIndex < 0)
                    return;

                var address = m_Dictionary.Records.Where(Arg => Arg.MMECode == (string) cbMMEList.SelectedItem).Select(Arg => Arg.IPAddress).First();
                
                try
                {
                    m_DatabaseClient = new DatabaseCommunicationProxy(DATABASE_SERVER_ENDPOINT_NAME,
                                                                      String.Format(Settings.Default.ServerAddressTemplate,
                                                                                    address));

                    m_DatabaseClient.Open();
                    m_IsServerConnected = true;
                    m_NetPingTimer.Change(1000, 1000);

                    tsslStatus.Text = @"Подключено";
                    btnConnect.Text = @"Отключиться";
                    btnSave.Enabled = true;
                    btnPrint.Enabled = true;
                    btnQuery.Enabled = true;
                    cbMMEList.Enabled = false;
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
                    btnSave.Enabled = false;
                    btnPrint.Enabled = false;
                    btnQuery.Enabled = false;
                    cbMMEList.Enabled = true;

                    lbGroups.Items.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, @"Ошибка соединения", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnQuery_Click(object Sender, EventArgs E)
        {
            if (m_IsServerConnected)
            {

                try
                {
                    var groups = m_DatabaseClient.ReadGroups(dtpFrom.Value, dtpTo.Value);

                    lbGroups.Items.Clear();

                    foreach (var group in groups)
                        lbGroups.Items.Add(group);

                    lbGroups.SelectedIndex = -1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, @"Ошибка соединения", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LbGroups_SelectedIndexChanged(object Sender, EventArgs E)
        {
            if (m_IsServerConnected)
            {
                if (lbGroups.SelectedIndex != -1)
                {
                    m_DeviceItemsWithParams.Clear();
                    dgvData.Rows.Clear();

                    try
                    {
                        var devices = m_DatabaseClient.ReadDevices(lbGroups.SelectedItem.ToString());

                        foreach (var devInfo in from deviceItem in devices
                                                let error =
                                                    m_DatabaseClient.ReadDeviceErrors(deviceItem.InternalID).FirstOrDefault()
                                                let devParams =
                                                    m_DatabaseClient.ReadDeviceParameters(deviceItem.InternalID)
                                                orderby deviceItem.Code, deviceItem.Position
                                                select new DeviceItemWithParams
                                                    {
                                                        GeneralInfo = deviceItem,
                                                        Parameters = devParams,
                                                        DefectCode = error
                                                    })
                                                    
                        {
                            AddRowToTable(devInfo);
                            m_DeviceItemsWithParams.Add(devInfo);
                        }

                        var firstDevice = devices.FirstOrDefault();
                        if (firstDevice != null)
                            tbDeviceType.Text = firstDevice.ProfileName;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, @"Ошибка соединения", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    dgvData.Rows.Clear();
                    m_DeviceItemsWithParams.Clear();
                }
            }
        }

        private void AddRowToTable(DeviceItemWithParams DevInfo)
        {
            var rowIndex = dgvData.Rows.Add();

            dgvData[0, rowIndex].Value = DevInfo.GeneralInfo.Code;
            dgvData[1, rowIndex].Value = DevInfo.GeneralInfo.Position;
            dgvData[2, rowIndex].Value = (DevInfo.DefectCode == 0) ? "QC" : DevInfo.DefectCode.ToString(CultureInfo.InvariantCulture);
            dgvData[3, rowIndex].Value = DevInfo.Parameters.Any(Item => Item.Name == "RG")
                                             ? DevInfo.Parameters.First(Item => Item.Name == "RG").Value.ToString(CultureInfo.InvariantCulture)
                                             : "-";
            dgvData[4, rowIndex].Value = DevInfo.Parameters.Any(Item => Item.Name == "IGT")
                                             ? DevInfo.Parameters.First(Item => Item.Name == "IGT").Value.ToString(CultureInfo.InvariantCulture)
                                             : "-";
            dgvData[5, rowIndex].Value = DevInfo.Parameters.Any(Item => Item.Name == "VGT")
                                             ? DevInfo.Parameters.First(Item => Item.Name == "VGT").Value.ToString(CultureInfo.InvariantCulture)
                                             : "-";
            dgvData[6, rowIndex].Value = DevInfo.Parameters.Any(Item => Item.Name == "IH")
                                             ? DevInfo.Parameters.First(Item => Item.Name == "IH").Value.ToString(CultureInfo.InvariantCulture)
                                             : "-";
            dgvData[7, rowIndex].Value = DevInfo.Parameters.Any(Item => Item.Name == "IL")
                                             ? DevInfo.Parameters.First(Item => Item.Name == "IL").Value.ToString(CultureInfo.InvariantCulture)
                                             : "-";
            dgvData[8, rowIndex].Value = DevInfo.Parameters.Any(Item => Item.Name == "VTM")
                                             ? DevInfo.Parameters.First(Item => Item.Name == "VTM").Value.ToString(CultureInfo.InvariantCulture)
                                             : "-";
            dgvData[9, rowIndex].Value = DevInfo.Parameters.Any(Item => Item.Name == "VRRM")
                                             ? DevInfo.Parameters.First(Item => Item.Name == "VRRM").Value.ToString(CultureInfo.InvariantCulture)
                                             : "-";
            dgvData[10, rowIndex].Value = DevInfo.Parameters.Any(Item => Item.Name == "VDRM")
                                             ? DevInfo.Parameters.First(Item => Item.Name == "VDRM").Value.ToString(CultureInfo.InvariantCulture)
                                             : "-";

            dgvData[11, rowIndex].Value = DevInfo.GeneralInfo.StructureOrd;
            dgvData[12, rowIndex].Value = DevInfo.GeneralInfo.StructureID;
        }

        private void BtnPrintSave_Click(object Sender, EventArgs E)
        {
            var save = ((string) ((Control) Sender).Tag) == "S";

            if (printDialog.ShowDialog() != DialogResult.OK)
                return;

            var printerName = printDialog.PrinterSettings.PrinterName;
            var copyCount = printDialog.PrinterSettings.Copies;

            if (m_IsServerConnected)
            {
                var record = m_Dictionary.Records.FirstOrDefault(Arg => Arg.MMECode == (string) cbMMEList.SelectedItem);
                
                if (m_DeviceItemsWithParams.Count > 0 && record != null)
                {
                    m_ExcelPrinter.TemplateXlsFilePath = record.Use2PosTemplate
                        ? m_ExcelPrinter.TemplateXlsFilePath =
                            Path.Combine(Application.StartupPath, Settings.Default.TemplateModDev)
                        : Path.Combine(Application.StartupPath, Settings.Default.TemplateTabDev);

                    m_ExcelPrinter.Copies = copyCount;
                    m_ExcelPrinter.PrinterName = printerName;

                    if (save && sdfReport.ShowDialog() == DialogResult.OK)
                    {
                        m_ExcelPrinter.SaveToFile = true;
                        m_ExcelPrinter.SaveXlsFilePath = sdfReport.FileName;
                    }
                    else
                        m_ExcelPrinter.SaveToFile = false;

                    List<ConditionItem> conditions;
                    List<ParameterNormativeItem> normatives;

                    try
                    {
                        conditions =
                            m_DatabaseClient.ReadDeviceConditions(m_DeviceItemsWithParams[0].GeneralInfo.InternalID);

                        normatives =
                            m_DatabaseClient.ReadDeviceNormatives(m_DeviceItemsWithParams[0].GeneralInfo.InternalID);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, @"Ошибка соединения", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    try
                    {
                        m_ExcelPrinter.CreateXlsReport(record.Use2PosTemplate,
                            new ReportInfo
                                {
                                    CustomerName = cbCustomer.SelectedItem.ToString(),
                                    GroupName = lbGroups.SelectedItem.ToString(),
                                    ModuleType = tbDeviceType.Text,
                                    Conditions = conditions,
                                    Normatives = normatives
                                }, m_DeviceItemsWithParams, cbCustomer.SelectedIndex == 0, save);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, @"Ошибка генерации отчета", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void MainForm_FormClosed(object Sender, FormClosedEventArgs E)
        {
            Settings.Default.Save();
        }
    }
}
