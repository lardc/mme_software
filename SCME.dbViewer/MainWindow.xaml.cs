using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.Threading;
using System.Globalization;
using SCME.dbViewer.Properties;
using SCME.dbViewer.ForParameters;
using System.Data;
using System.Collections.ObjectModel;
using SCME.dbViewer.CustomControl;
using System.Windows.Controls.Primitives;
using System.Text.RegularExpressions;

namespace SCME.dbViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public const int cDevID = 0;
        public const int cProfileID = 1;
        public const int cTsZeroTime = 2;
        public const int cGroupName = 3;
        public const int cItem = 4;
        public const int cCode = 7;
        public const int cProfileName = 9;
        public const int cDeviceType = 10;
        public const int cСonstructive = 11;
        public const int cAverageCurrent = 12;
        public const int cDeviceClass = 13;
        public const int cEquipment = 14;
        public const int cUser = 15;
        public const int cStatus = 16;
        public const int cReason = 17;

        private SqlConnection Connection = null;

        public MainWindow()
        {
            Application.Current.DispatcherUnhandledException += DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

            try
            {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(Settings.Default.Localization);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Localization error");
            }

            InitializeComponent();

            CreateDeviceColumns();
            KeyPreview();
            Connection = CreateConnection();

            //чтобы скрыть пустые DataGrid сразу после запуска приложения
            DataViewModel vm = new DataViewModel();
            this.DataContext = vm;

            dgDevices.BuildData = BuildData;

            //применяем стили для раскраски DataGrid
            dgRTData.RTStyle();
            dgTMData.TMStyle();
        }

        static void DispatcherUnhandledException(object Sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs E)
        {
            MessageBox.Show(E.Exception.ToString(), "Unhandled exception");
        }

        private static void CurrentDomainOnUnhandledException(object Sender, UnhandledExceptionEventArgs Args)
        {
            MessageBox.Show(Args.ExceptionObject.ToString(), "Unhandled exception");
        }

        private int DevID(object[] itemArray)
        {
            return int.Parse(itemArray?[cDevID].ToString());
        }

        private string ProfileID(object[] itemArray)
        {
            return itemArray?[cProfileID].ToString();
        }

        private DateTime TsZeroTime(object[] itemArray)
        {
            return DateTime.Parse(itemArray?[cTsZeroTime].ToString());
        }

        private string GroupName(object[] itemArray)
        {
            return itemArray?[cGroupName].ToString();
        }

        private string Item(object[] itemArray)
        {
            return itemArray?[cItem].ToString();
        }

        private string Code(object[] itemArray)
        {
            return itemArray?[cCode].ToString();
        }

        private string ProfileName(object[] itemArray)
        {
            return itemArray?[cProfileName].ToString();
        }

        private string DeviceType(object[] itemArray)
        {
            return itemArray?[cDeviceType].ToString();
        }

        private string Constructive(object[] itemArray)
        {
            return itemArray?[cСonstructive].ToString();
        }

        private int AverageCurrent(object[] itemArray)
        {
            return int.Parse(itemArray?[cAverageCurrent].ToString());
        }

        private int DeviceClass(object[] itemArray)
        {
            return int.Parse(itemArray?[cDeviceClass].ToString());
        }

        private string Equipment(object[] itemArray)
        {
            return itemArray?[cEquipment].ToString();
        }

        private string User(object[] itemArray)
        {
            return itemArray?[cUser].ToString();
        }

        private string Status(object[] itemArray)
        {
            return itemArray?[cStatus].ToString();
        }

        private string Reason(object[] itemArray)
        {
            return itemArray?[cReason].ToString();
        }

        private SqlConnection CreateConnection()
        {
            string strCon = "server=192.168.0.134, 1444;uid=sa;pwd=Hpl1520; database=SCME_ResultsDB";

            return new SqlConnection(strCon);
        }

        public void KeyEventHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
                LoadDevices();

            if (sender is DatePicker)
            {
                switch (e.Key)
                {
                    case Key.Delete:
                    case Key.Back:
                    case Key.Escape:
                        DatePicker dt = (DatePicker)sender;
                        dt.SelectedDate = null;
                        break;

                    case Key.Enter:
                        LoadDevices();
                        break;
                }
            }
        }

        private void KeyPreview()
        {
            foreach (Control fe in FindVisualChildren<Control>(grdParent))
            {
                fe.KeyDown += new KeyEventHandler(KeyEventHandler);
            }
        }

        public IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private void CreateColumns()
        {
            CreateDeviceColumns();
        }

        private void CreateDeviceColumns()
        {
            dgDevices.ClearColumns();

            DataGridColumn column = dgDevices.NewColumn(Properties.Resources.DevID, "DEV_ID");  //0
            column.Visibility = Visibility.Collapsed;

            column = dgDevices.NewColumn(Properties.Resources.ProfileID, "PROFILE_ID");         //1
            column.Visibility = Visibility.Collapsed;

            column = dgDevices.NewColumn(Properties.Resources.TsZeroTime, "TSZEROTIME");        //2
            column.Visibility = Visibility.Collapsed;

            dgDevices.NewColumn(Properties.Resources.GroupName, "GROUP_NAME");                  //3
            dgDevices.NewColumn(Properties.Resources.Item, "ITEM");                             //4
            dgDevices.NewColumn(Properties.Resources.SiType, "SITYPE");                         //5
            dgDevices.NewColumn(Properties.Resources.SiOmnity, "SIOMNITY");                     //6
            dgDevices.NewColumn(Properties.Resources.Code, "CODE");                             //7
            dgDevices.NewColumn(Properties.Resources.Ts, "TS");                                 //8
            dgDevices.NewColumn(Properties.Resources.ProfName, "PROF_NAME");                    //9
            dgDevices.NewColumn(Properties.Resources.DeviceType, "DEVICETYPE");                 //10
            dgDevices.NewColumn(Properties.Resources.Constructive, "СONSTRUCTIVE");             //11
            dgDevices.NewColumn(Properties.Resources.AverageCurrent, "AVERAGECURRENT");         //12
            dgDevices.NewColumn(Properties.Resources.DeviceClass, "DEVICECLASS");               //13
            dgDevices.NewColumn(Properties.Resources.MmeCode, "MME_CODE");                      //14
            dgDevices.NewColumn(Properties.Resources.Usr, "USR");                               //15
            dgDevices.NewColumn(Properties.Resources.Status, "STATUS");                         //16
            dgDevices.NewColumn(Properties.Resources.Reason, "REASON");                         //17
        }

        private void LoadDevices()
        {
            string SqlText = "SELECT x.DEV_ID, x.PROFILE_ID, x.ITEM, x.SITYPE, x.SIOMNITY, x.TSZEROTIME, x.GROUP_NAME, x.CODE, x.TS, x.PROF_NAME, x.DEVICETYPE, x.СONSTRUCTIVE, x.AVERAGECURRENT, x.DEVICECLASS, x.MME_CODE, x.USR, x.STATUS, x.REASON" +
                              " FROM" +
                              " (" +
                                 "SELECT s.DEV_ID, s.PROFILE_ID, s.ITEM, dbo.SiType(s.ITEM) AS SITYPE, dbo.SiOmnity(s.ITEM) AS SIOMNITY, s.TSZEROTIME, s.GROUP_NAME, s.CODE, s.TS, s.PROF_NAME, s.DEVICETYPE, dbo.СonstructiveByProfileName(s.DEVICETYPE, s.PROF_NAME) AS СONSTRUCTIVE, dbo.AverageCurrent(s.DEVICETYPE, s.PROF_NAME) AS AVERAGECURRENT, dbo.DeviceClass(s.DEV_ID, s.DEVICETYPE, s.PROF_ID) AS DEVICECLASS, s.MME_CODE, s.USR, dbo.StrIsEmpty(s.REASON) AS STATUS, s.REASON" +
                                 " FROM" +
                                 " (" +
                                    "SELECT D.DEV_ID, D.PROFILE_ID, dbo.SL_ItemByJob(G.GROUP_NAME) AS ITEM, dbo.DateTimeToDateZeroTime(D.TS) AS TSZEROTIME, G.GROUP_NAME, D.CODE, D.TS, P.PROF_ID, P.PROF_NAME, dbo.DeviceTypeByProfileName(P.PROF_NAME) AS DEVICETYPE, MME_CODE, USR, dbo.IsAllTestsGood(D.DEV_ID, P.PROF_ID) AS REASON" +
                                    " FROM GROUPS G" +
                                    " INNER JOIN DEVICES AS D ON (G.GROUP_ID=D.GROUP_ID)" +
                                    " INNER JOIN PROFILES AS P ON (" +
                                    "                              (P.PROF_GUID=D.PROFILE_ID) AND" +
                                    "                              (ISNULL(P.IS_DELETED, 0)=0)" +
                                    "                             )" +
                                  " ) AS s" +
                              " ) AS x";

            string dateBeg = null;
            if (dpBegin.SelectedDate != null)
            {
                switch (dpBegin.CheckDate())
                {
                    case false:
                        MessageBox.Show(string.Format("Значение поля '{0}' не является датой, либо очень мало.", lbFromDate.Content.ToString()), "проверьте значение даты", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;

                    default:
                        dateBeg = dpBegin.SelectedDate.Value.Date.ToShortDateString();
                        break;
                }
            }

            string dateEnd = null;
            if (dpEnd.SelectedDate != null)
            {
                switch (dpEnd.CheckDate())
                {
                    case false:
                        MessageBox.Show(string.Format("Значение поля '{0}' не является датой, либо очень мало.", lbToDate.Content.ToString()), "проверьте значение даты", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;

                    default:
                        dateEnd = dpEnd.SelectedDate.Value.Date.ToShortDateString();
                        break;
                }
            }

            string job = null;
            if (tb_Job.Text.Trim() != string.Empty)
                job = tb_Job.Text.Trim();

            string deviceType = null;
            if (cmb_DeviceType.SelectedItem != null)
            {
                ComboBoxItem cbi = (ComboBoxItem)cmb_DeviceType.SelectedItem;
                deviceType = cbi.Content.ToString();
            }

            string constructive = null;
            if (tb_Сonstructive.Text.Trim() != string.Empty)
                constructive = tb_Сonstructive.Text.Trim();

            string averageCurrent = null;
            if (tb_AverageCurrent.Text.Trim() != string.Empty)
                averageCurrent = tb_AverageCurrent.Text.Trim();

            string deviceClass = null;
            if (tb_DeviceClass.Text.Trim() != string.Empty)
                deviceClass = tb_DeviceClass.Text.Trim();

            byte? siType = null;
            if (cmb_SiType.SelectedItem != null)
            {
                ComboBoxItem cbi = (ComboBoxItem)cmb_SiType.SelectedItem;
                siType = (byte)cbi.Tag;
            }

            string profName = null;
            if (tb_ProfName.Text.Trim() != string.Empty)
                profName = tb_ProfName.Text.Trim();

            string mmeCode = null;
            if (tb_MmeCode.Text.Trim() != string.Empty)
                mmeCode = tb_MmeCode.Text.Trim();

            string usr = null;
            if (tb_Usr.Text.Trim() != string.Empty)
                usr = tb_Usr.Text.Trim();

            SqlText += ((dateBeg != null) || (dateEnd != null) || (job != null) || (deviceType != null) || (constructive != null) || (averageCurrent != null) || (deviceClass != null) || (siType != null) || (profName != null) || (mmeCode != null) || (usr != null)) ? " WHERE" : "";

            string whereSection = string.Empty;

            if (dateBeg != null)
                whereSection = string.Format(" x.TSZEROTIME>='{0}'", dateBeg);

            if (dateEnd != null)
            {
                if (whereSection != string.Empty)
                    whereSection += " AND";

                whereSection += string.Format(" x.TSZEROTIME<='{0}'", dateEnd);
            }

            if (job != null)
            {
                if (whereSection != string.Empty)
                    whereSection += " AND";

                whereSection += string.Format(" x.GROUP_NAME='{0}'", job);
            }

            if (deviceType != null)
            {
                if (whereSection != string.Empty)
                    whereSection += " AND";

                whereSection += string.Format(" x.DEVICETYPE='{0}'", deviceType);
            }

            if (constructive != null)
            {
                if (whereSection != string.Empty)
                    whereSection += " AND";

                whereSection += string.Format(" x.СONSTRUCTIVE='{0}'", constructive);
            }

            if (averageCurrent != null)
            {
                if (whereSection != string.Empty)
                    whereSection += " AND";

                whereSection += string.Format(" x.AVERAGECURRENT='{0}'", averageCurrent);
            }

            if (deviceClass != null)
            {
                if (whereSection != string.Empty)
                    whereSection += " AND";

                whereSection += string.Format(" x.DEVICECLASS='{0}'", deviceClass);
            }

            if (siType != null)
            {
                if (whereSection != string.Empty)
                    whereSection += " AND";

                whereSection += string.Format(" x.SITYPE='{0}'", siType);
            }

            if (profName != null)
            {
                if (whereSection != string.Empty)
                    whereSection += " AND";

                whereSection += string.Format(" x.PROF_NAME='{0}'", profName);
            }

            if (mmeCode != null)
            {
                if (whereSection != string.Empty)
                    whereSection += " AND";

                whereSection += string.Format(" x.MME_CODE='{0}'", mmeCode);
            }

            if (usr != null)
            {
                if (whereSection != string.Empty)
                    whereSection += " AND";

                whereSection += string.Format(" x.USR='{0}'", usr);
            }

            if (whereSection != string.Empty)
                SqlText += whereSection;

            dgDevices.ViewSqlResult(Connection, SqlText);
        }

        private void dpBeg_CalendarClosed(object sender, RoutedEventArgs e)
        {
            LoadDevices();
        }

        private void dpEnd_CalendarClosed(object sender, RoutedEventArgs e)
        {
            LoadDevices();
        }

        private void tb_Job_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;

            if (tb.Text.Trim() != string.Empty)
                LoadDevices();
        }

        private void cmb_DeviceType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = (ComboBox)sender;

            if (cmb.SelectedItem != null)
                LoadDevices();
        }

        private void tb_Сonstructive_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;

            if (tb.Text.Trim() != string.Empty)
                LoadDevices();
        }

        private void tb_AverageCurrent_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;

            if (tb.Text.Trim() != string.Empty)
                LoadDevices();
        }

        private void tb_DeviceClass_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;

            if (tb.Text.Trim() != string.Empty)
                LoadDevices();
        }

        private void cmb_SiType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = (ComboBox)sender;

            if (cmb.SelectedItem != null)
                LoadDevices();
        }

        private void tb_ProfName_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;

            if (tb.Text.Trim() != string.Empty)
                LoadDevices();
        }

        private void tb_MmeCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;

            if (tb.Text.Trim() != string.Empty)
                LoadDevices();
        }

        private void tb_Usr_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;

            if (tb.Text.Trim() != string.Empty)
                LoadDevices();
        }

        private TemperatureCondition TemperatureConditionByProfileName(string profileName)
        {
            TemperatureCondition result = TemperatureCondition.None;

            if (profileName.Trim() != string.Empty)
            {
                string profileNameUpper = profileName.ToUpper();

                result = profileNameUpper.Contains("RT") ? TemperatureCondition.RT : profileNameUpper.Contains("TM") ? TemperatureCondition.TM : TemperatureCondition.None;
            }

            return result;
        }

        private string PairProfileNameByProfileName(string profileName, out TemperatureCondition temperatureCondition)
        {
            //вычисление пары для profileName
            string result = profileName;

            switch (TemperatureConditionByProfileName(result))
            {
                case TemperatureCondition.RT:
                    //меняем в profName RT на TM   
                    result = Regex.Replace(result, "RT", "TM", RegexOptions.IgnoreCase);
                    temperatureCondition = TemperatureCondition.TM;
                    break;

                case TemperatureCondition.TM:
                    //меняем TM на RT
                    result = Regex.Replace(result, "TM", "RT", RegexOptions.IgnoreCase);
                    temperatureCondition = TemperatureCondition.RT;
                    break;

                default:
                    result = string.Empty;
                    temperatureCondition = TemperatureCondition.None;
                    break;
            }

            return result;
        }

        private bool CalcPairData(string profName, string GroupName, string Code, out string ProfileName, out int devID, out TemperatureCondition temperatureCondition)
        {
            ProfileName = this.PairProfileNameByProfileName(profName, out temperatureCondition);
            string profileName = ProfileName;

            DataView dv = (DataView)dgDevices.ItemsSource;

            //ищем в dv первую попавшуюся запись с вычисленным кодом профиля profileName, номером ПЗ GroupName и номером ГП (ППЭ) Code
            var results = from DataRowView rowView in dv
                          where (
                                 (rowView.Row.Field<string>("PROF_NAME").ToUpper() == profileName.ToUpper()) &&
                                 (rowView.Row.Field<string>("GROUP_NAME") == GroupName) &&
                                 (rowView.Row.Field<string>("CODE") == Code)
                                )
                          select rowView.Row;

            DataRow row = results.FirstOrDefault();

            switch (row == null)
            {
                case true:
                    //ничего не нашли
                    devID = -1;
                    return false;

                default:
                    devID = row.Field<int>("DEV_ID");
                    return true;
            }
        }

        private void BuildData()
        {
            DataRowView row = dgDevices.SelectedItem as DataRowView;

            if (row != null)
            {
                object[] itemArray = row.Row.ItemArray;

                int selDevID = DevID(itemArray);
                string selDevType = DeviceType(itemArray);
                TemperatureCondition selTemperatureCondition = TemperatureConditionByProfileName(ProfileName(itemArray));

                string pairProfileName;
                int pairDevID;
                TemperatureCondition pairTemperatureCondition;

                bool foundedPair = CalcPairData(ProfileName(itemArray), GroupName(itemArray), Code(itemArray), out pairProfileName, out pairDevID, out pairTemperatureCondition);

                DataViewModel vm = null;
                switch (selTemperatureCondition)
                {
                    case TemperatureCondition.RT:
                        lbRTProfileName.Content = ProfileName(itemArray);
                        vm = new DataViewModel(selDevID, selDevType, selTemperatureCondition, pairDevID, selDevType, pairTemperatureCondition);
                        lbTMProfileName.Content = foundedPair ? pairProfileName : string.Empty;
                        break;

                    case TemperatureCondition.TM:
                        lbTMProfileName.Content = ProfileName(itemArray);
                        vm = new DataViewModel(pairDevID, selDevType, pairTemperatureCondition, selDevID, selDevType, selTemperatureCondition);
                        lbRTProfileName.Content = foundedPair ? pairProfileName : string.Empty;
                        break;

                    case TemperatureCondition.None:
                        vm = new DataViewModel();
                        break;
                }

                this.DataContext = vm;

                //вычисляем сколько записей стоит ниже текущей выбранной
                int selectedRowNum = dgDevices.Items.IndexOf(dgDevices.SelectedItem) + 1;
                int bottomRecords = dgDevices.Items.Count - selectedRowNum;
                lbBottomRecordCount.Content = string.Format("({0})", bottomRecords.ToString());
            }
        }

        private void dgDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BuildData();
        }

        private List<DataTableParameters> ListOfDeviceParameters()
        {
            //формирует список списков параметров по текущему отображаемому списку изделий            
            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            List<DataTableParameters> result = new List<DataTableParameters>();

            try
            {
                foreach (System.Data.DataRowView drv in dgDevices.ItemsSource)
                {
                    object[] itemArray = drv.Row.ItemArray;

                    int devID = DevID(itemArray);
                    string profileID = ProfileID(itemArray);
                    DateTime tsZeroTime = TsZeroTime(itemArray);
                    string groupName = GroupName(itemArray);
                    string item = Item(itemArray);
                    string code = Code(itemArray);
                    string profileName = ProfileName(itemArray);
                    string deviceType = DeviceType(itemArray);
                    string constructive = Constructive(itemArray);
                    int averageCurrent = AverageCurrent(itemArray);
                    int deviceClass = DeviceClass(itemArray);
                    string equipment = Equipment(itemArray);
                    string user = User(itemArray);
                    string status = Status(itemArray);
                    string reason = Reason(itemArray);
                    TemperatureCondition temperatureCondition = TemperatureConditionByProfileName(profileName);
                    
                    DataTableParameters p = new DataTableParameters();

                    result.Add(p);
                    p.Load(Connection, devID, deviceType, temperatureCondition, profileID, profileName, tsZeroTime, groupName, item, code, constructive, averageCurrent, deviceClass, equipment, user, status, reason);
                }
            }

            finally
            {
                Connection.Close();
            }

            return result;
        }

        private DataTableParameters FindPair(string profName, string SilN1, string SilN2, List<DataTableParameters> listOfDeviceParameters)
        {
            //ищет пару в listOfDeviceParameters для принятых profName, SilN1, SilN2
            DataTableParameters result = null;

            string ProfileName;
            int devID;
            TemperatureCondition temperatureCondition;

            if (this.CalcPairData(profName, SilN1, SilN2, out ProfileName, out devID, out temperatureCondition))
            {
                //по вычисленным значениям полей разыскиваем в listOfDeviceParameters первый попавшийся ещё не использованный device
                var results = from DataTableParameters dtp in listOfDeviceParameters
                              where (
                                     (dtp.DevID == devID) &&
                                     (dtp.Used == false)
                                    )
                              select dtp;

                result = results.FirstOrDefault();
            }

            return result;
        }

        private ReportData FindData(string profName, string SilN1, string SilN2)
        {
            //ищет DataTableParameters пару в списке записей для принятых profName, SilN1, SilN2
            TemperatureCondition temperatureCondition;
            string PairProfileName = this.PairProfileNameByProfileName(profName, out temperatureCondition);

            DataView dv = (DataView)dgDevices.ItemsSource;

            //ищем в dv первую попавшуюся запись с вычисленным кодом профиля PairProfileName, номеру партии SilN1, номеру ППЭ SilN2
            var results = from DataRowView rowView in dv
                          where (
                                 (rowView.Row.Field<string>("PROF_NAME").ToUpper() == PairProfileName.ToUpper()) &&
                                 (rowView.Row.Field<string>("SIL_N_1") == SilN1) &&
                                 (rowView.Row.Field<string>("SIL_N_2") == SilN2)
                                )
                          select rowView.Row;

            DataRow row = results.FirstOrDefault();

            switch (row == null)
            {
                case true:
                    //ничего не нашли
                    return null;

                default:
                    ReportData result = row.Field<ReportData>("PAIR");
                    return result;
            }
        }

        private ReportByDevices GroupData(List<DataTableParameters> listOfDeviceParameters)
        {
            ReportByDevices result = new ReportByDevices();

            //просматриваем список listOfDeviceParameters и метим использованные списки параметров чтобы не использовать их повторно
            foreach (DataTableParameters dtp in listOfDeviceParameters)
            {
                if ((dtp.Used == false) && (dtp.TemperatureCondition != TemperatureCondition.None))
                {
                    //данный device разрешён для использования
                    ReportData reportData = result.NewReportData();

                    dtp.Used = true;

                    //ищем пару для использованного device
                    DataTableParameters pair = this.FindPair(dtp.ProfileName, dtp.SilN1, dtp.SilN2, listOfDeviceParameters);

                    //проверяем это горячее или холодное измерение
                    switch (dtp.TemperatureCondition)
                    {
                        case TemperatureCondition.RT:
                            reportData.RTData = dtp;

                            if (pair != null)
                            {
                                reportData.TMData = pair;
                                pair.Used = true;
                            }
                            break;

                        case TemperatureCondition.TM:
                            if (pair != null)
                            {
                                reportData.RTData = pair;
                                pair.Used = true;
                            }

                            reportData.TMData = dtp;
                            break;
                    }
                }
            }

            //данные сгруппированы
            return result;
        }

        private void btReport_Click(object sender, RoutedEventArgs e)
        {
            if ((dgDevices.ItemsSource == null) || (dgDevices.Items.Count == 0))
            {
                MessageBox.Show(Properties.Resources.ReportCannotBeGenerated, Properties.Resources.NoData, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            //получаем исходый список, который необходимо сгруппировать по номеру ПЗ и номеру ППЭ
            List<DataTableParameters> listOfDeviceParameters = ListOfDeviceParameters();

            ReportByDevices report = GroupData(listOfDeviceParameters);
            report.ToExcel();
        }

        private void dgDevices_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void cmb_DeviceType_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Delete) || (e.Key == Key.Back))
            {
                ComboBox cb = (ComboBox)sender;
                cb.SelectedItem = null;
                LoadDevices();
            }
        }

        private void cmb_SiType_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Delete) || (e.Key == Key.Back))
            {
                ComboBox cb = (ComboBox)sender;
                cb.SelectedItem = null;
                LoadDevices();
            }
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            LoadDevices();
        }
    }

    public class DataViewModel
    {
        public DataTableParameters RT { get; set; }
        public DataTableParameters TM { get; set; }

        public DataViewModel(int RTdevID, string RTdevType, TemperatureCondition RTtemperatureCondition, int TMdevID, string TMdevType, TemperatureCondition TMtemperatureCondition) : base()
        {
            SqlConnection Connection = CreateConnection();

            this.RT = new DataTableParameters();
            this.RT.Load(Connection, RTdevID, RTdevType, RTtemperatureCondition);

            this.TM = new DataTableParameters();
            this.TM.Load(Connection, TMdevID, TMdevType, TMtemperatureCondition);
        }

        public DataViewModel() : base()
        {
            //пустая модель отображения
            this.RT = new DataTableParameters();
            this.TM = new DataTableParameters();
        }

        private SqlConnection CreateConnection()
        {
            string strCon = "server=192.168.0.134, 1444;uid=sa;pwd=Hpl1520; database=SCME_ResultsDB";

            return new SqlConnection(strCon);
        }
    }
}
