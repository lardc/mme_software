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
using SCME.Types.Profiles;

namespace SCME.dbViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public const int cDevID = 0;
        public const int cProfileID = 1;
        public const int cTsZeroTime = 5;
        public const int cGroupName = 6;
        public const int cItem = 2;
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
        public const int cCodeOfNonMatch = 18;

        private SqlConnection connection = null;
        private List<DataTableParameters> listOfDeviceParameters = null;
        private ReportByDevices reportByDevices = null;

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
            connection = CreateConnection();

            //чтобы скрыть пустые DataGrid сразу после запуска приложения
            this.DataContext = new DataViewModel();

            dgDevices.CreateCalculatedFields = CreateCalculatedFields;
            dgDevices.ReBuildData = ReBuildData;

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

        private int? AverageCurrent(object[] itemArray)
        {
            int? averageCurrent = itemArray?[cAverageCurrent] as int?;
            return (averageCurrent == null) ? null : int.Parse(itemArray?[cAverageCurrent].ToString()) as int?;
        }

        private int? DeviceClass(object[] itemArray)
        {
            int? deviceClass = itemArray?[cDeviceClass] as int?;
            return (deviceClass == null) ? null : int.Parse(itemArray?[cDeviceClass].ToString()) as int?;
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

        private string CodeOfNonMatch(object[] itemArray)
        {
            return itemArray?[cCodeOfNonMatch].ToString();
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
            dgDevices.NewColumn(Properties.Resources.CodeOfNonMatch, "CODEOFNONMATCH");         //17
            dgDevices.NewColumn(Properties.Resources.Reason, "REASON");                         //18
        }

        private void CreateCalculatedFields()
        {
            //создание вычисляемых полей, т.е. тех полей, значения которых не считываются из базы данных
            dgDevices?.dataTable?.Columns.Add("CODEOFNONMATCH");
        }

        private void ReBuildData()
        {
            //формирование исходных данных для отображения списка условий, параметров и построения протокола испытаний
            this.RefreshBottomRecordCount();
            this.listOfDeviceParameters = ListOfDeviceParameters();
            this.reportByDevices = GroupData(listOfDeviceParameters);
        }

        private void LoadDevices()
        {
            /*
            string SqlText = "SELECT x.DEV_ID, x.PROFILE_ID, x.ITEM, x.SITYPE, x.SIOMNITY, x.TSZEROTIME, x.GROUP_NAME, x.CODE, x.TS, x.PROF_NAME, x.DEVICETYPE, x.СONSTRUCTIVE, x.AVERAGECURRENT, x.DEVICECLASS, x.MME_CODE, x.USR, x.STATUS, x.REASON" +
                              " FROM" +
                              " (" +
                                 "SELECT s.DEV_ID, s.PROFILE_ID, s.ITEM, dbo.SiType(s.ITEM) AS SITYPE, dbo.SiOmnity(s.ITEM) AS SIOMNITY, s.TSZEROTIME, s.GROUP_NAME, s.CODE, s.TS, s.PROF_NAME, s.DEVICETYPE, dbo.СonstructiveByProfileName(s.DEVICETYPE, s.PROF_NAME) AS СONSTRUCTIVE, dbo.AverageCurrent(s.DEVICETYPE, s.PROF_NAME) AS AVERAGECURRENT, dbo.DeviceClass(s.DEV_ID, s.DEVICETYPE, s.PROF_ID) AS DEVICECLASS, s.MME_CODE, s.USR, dbo.StrIsEmpty(s.REASON) AS STATUS, s.REASON" +
                                 " FROM" +
                                 " (" +
                                    "SELECT D.DEV_ID, D.PROFILE_ID, dbo.SL_ItemByJob(G.GROUP_NAME) AS ITEM, dbo.DateTimeToDateZeroTime(D.TS) AS TSZEROTIME, RTRIM(G.GROUP_NAME) AS GROUP_NAME, D.CODE, D.TS, P.PROF_ID, P.PROF_NAME, dbo.DeviceTypeByProfileName(P.PROF_NAME) AS DEVICETYPE, MME_CODE, USR, dbo.IsAllTestsGood(D.DEV_ID, P.PROF_ID) AS REASON" +
                                    " FROM" +
                                    " (" +
                                       "SELECT MAX(DG.DEV_ID) AS DEV_ID" +
                                       " FROM DEVICES DG" +
                                       "  INNER JOIN PROFILES AS P ON (DG.PROFILE_ID=P.PROF_GUID)" +
                                       " GROUP BY P.PROF_NAME, DG.GROUP_ID, DG.CODE, DG.MME_CODE" +
                                    " ) AS z" +
                                    " INNER JOIN DEVICES AS D ON (D.DEV_ID=z.DEV_ID)" +
                                    " INNER JOIN GROUPS G ON(G.GROUP_ID=D.GROUP_ID)" +
                                    " INNER JOIN PROFILES AS P ON (" +
                                    "                              (P.PROF_GUID=D.PROFILE_ID) AND" +
                                    "                              (ISNULL(P.IS_DELETED, 0)=0)" +
                                    "                             )" +
                                 " ) AS s" +
                              " ) AS x";
            */
            //нас интересуют самые свежие результаты измерений
            string SqlText = "SELECT x.DEV_ID, x.PROFILE_ID, x.ITEM, x.SITYPE, x.SIOMNITY, x.TSZEROTIME, x.GROUP_NAME, x.CODE, x.TS, x.PROF_NAME, x.DEVICETYPE, x.СONSTRUCTIVE, x.AVERAGECURRENT, x.DEVICECLASS, x.MME_CODE, x.USR, x.STATUS, x.REASON" +
                             " FROM" +
                             " (" +
                                "SELECT s.DEV_ID, s.PROFILE_ID, s.ITEM, dbo.SiType(s.ITEM) AS SITYPE, dbo.SiOmnity(s.ITEM) AS SIOMNITY, s.TSZEROTIME, s.GROUP_NAME, s.CODE, s.TS, s.PROF_NAME, s.DEVICETYPE, dbo.СonstructiveByProfileName(s.DEVICETYPE, s.PROF_NAME) AS СONSTRUCTIVE, dbo.AverageCurrent(s.DEVICETYPE, s.PROF_NAME) AS AVERAGECURRENT, dbo.DeviceClass(s.DEV_ID, s.DEVICETYPE, s.PROF_ID, s.PROF_NAME) AS DEVICECLASS, s.MME_CODE, s.USR, dbo.StrIsEmpty(s.REASON) AS STATUS, s.REASON" +
                                " FROM" +
                                " (" +
                                   "SELECT z.DEV_ID, D.PROFILE_ID, dbo.SL_ItemByJob(G.GROUP_NAME) AS ITEM, dbo.DateTimeToDateZeroTime(D.TS) AS TSZEROTIME, RTRIM(G.GROUP_NAME) AS GROUP_NAME, z.CODE, D.TS, P.PROF_ID, z.PROF_NAME, dbo.DeviceTypeByProfileName(z.PROF_NAME) AS DEVICETYPE, z.MME_CODE, USR, dbo.IsAllTestsGood(z.DEV_ID, P.PROF_ID) AS REASON" +
                                   " FROM" +
                                   " (" +
                                      "SELECT MAX(DG.DEV_ID) AS DEV_ID, DG.CODE, DG.MME_CODE, PG.PROF_NAME" +
                                      " FROM DEVICES DG" +
                                      "  INNER JOIN PROFILES AS PG ON (" +
                                      "                                (DG.PROFILE_ID=PG.PROF_GUID) AND" +
                                      "                                (ISNULL(PG.IS_DELETED, 0)=0)" +
                                      "                               )" +
                                      " GROUP BY PG.PROF_NAME, DG.GROUP_ID, DG.CODE, DG.MME_CODE" +
                                   " ) AS z" +
                                   " INNER JOIN DEVICES AS D ON (D.DEV_ID=z.DEV_ID)" +
                                   " INNER JOIN GROUPS G ON (G.GROUP_ID=D.GROUP_ID)" +
                                   " INNER JOIN PROFILES AS P ON (P.PROF_GUID=D.PROFILE_ID)" +
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

            SqlText += ((dateBeg != null) || (dateEnd != null) || (job != null) || (deviceType != null) || (constructive != null) || (averageCurrent != null) || (deviceClass != null) || (siType != null) || (profName != null) || (mmeCode != null) || (usr != null)) ? " WHERE" : string.Empty;

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

            dgDevices.ViewSqlResult(connection, SqlText);
            RefreshConditionsAndParameters();
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

        private static String WildCardToRegular(string value)
        {
            return "^" + System.Text.RegularExpressions.Regex.Escape(value).Replace("\\*", ".*") + "$";
        }

        private DataTableParameters CalcPairData(List<DataTableParameters> listOfDeviceParameters, string profileBody, string GroupName, string Code)
        {
            //GroupName есть номер ПЗ, Code есть порядковый номер, а их сочетание это серийный номер. это справедливо как для PSE, так и для PSD 
            //ищем в listOfDeviceParameters первую попавшуюся не использованную запись с Data == null по вычисленному телу профиля profileName, номеру ПЗ GroupName и номеру ГП (ППЭ) Code
            var results = from DataTableParameters dtp in listOfDeviceParameters
                          where (
                                 (dtp.Code == Code) &&
                                 (dtp.GroupName == GroupName) &&
                                 (dtp.Data == null) &&
                                 (System.Text.RegularExpressions.Regex.IsMatch(dtp.ProfileName.ToUpper(), WildCardToRegular(profileBody)))  //(dtp.ProfileName.ToUpper().Contains(profileBody))                                 
                                )
                          select dtp;

            DataTableParameters result = results.FirstOrDefault();

            return result;
        }

        private int IndexOfDevID(List<DataTableParameters> listOfDeviceParameters, int DevID)
        {
            //вычисляет индекс записи в списке listOfDeviceParameters, с идентификатором DevID. в списке listOfDeviceParameters может быть только одна такая запись
            var results = from DataTableParameters dtp in listOfDeviceParameters
                          where (
                                 (dtp.DevID == DevID)
                                )
                          select dtp;

            if (results.Count() != 1)
                throw new Exception(string.Format("MainWindow.IndexOfDevID. Для DevID={0} найдено записей: {1}. Ожидалась одна запись.", DevID, results.Count()));

            DataTableParameters result = results.FirstOrDefault();

            return listOfDeviceParameters.IndexOf(result);
        }

        private void RefreshConditionsAndParameters()
        {
            //извлекаем из this.listOfDeviceParameters данные, которые имеют тот же Dev_ID, что и выбранная запись
            int index = dgDevices.SelectedIndex;

            if (index != -1)
            {
                if (this.listOfDeviceParameters != null)
                {
                    //ищем в listOfDeviceParameters выбранный Dev_ID
                    var drv = dgDevices.Items[index] as System.Data.DataRowView;

                    if (drv != null)
                    {
                        var itemArray = drv.Row.ItemArray;

                        if (itemArray != null)
                        {
                            int devID = DevID(itemArray);
                            index = this.IndexOfDevID(this.listOfDeviceParameters, devID);

                            DataTableParameters selData = this.listOfDeviceParameters[index];

                            if (selData != null)
                            {
                                DataViewModel vm = null;
                                TemperatureCondition selTemperatureCondition = TemperatureConditionByProfileName(selData.ProfileName);

                                switch (selTemperatureCondition)
                                {
                                    case TemperatureCondition.RT:
                                        lbRTProfileName.Content = selData.ProfileName;
                                        lbTMProfileName.Content = (selData.Data == null) ? string.Empty : ((selData.Data.TMData == null) ? string.Empty : selData.Data.TMData.ProfileName);
                                        vm = new DataViewModel(selData.Data.RTData, selData.Data.TMData);
                                        break;

                                    case TemperatureCondition.TM:
                                        lbTMProfileName.Content = selData.ProfileName;
                                        lbRTProfileName.Content = (selData.Data == null) ? string.Empty : ((selData.Data.RTData == null) ? string.Empty : selData.Data.RTData.ProfileName);
                                        vm = new DataViewModel(selData.Data.RTData, selData.Data.TMData);
                                        break;

                                    default:
                                        vm = new DataViewModel();
                                        break;
                                }

                                this.DataContext = vm;
                            }
                        }
                    }
                }
            }
        }

        private void RefreshBottomRecordCount()
        {
            //вычисляем сколько записей стоит ниже текущей выбранной
            int selectedRowNum = dgDevices.Items.IndexOf(dgDevices.SelectedItem) + 1;
            int bottomRecords = dgDevices.Items.Count - selectedRowNum;
            lbBottomRecordCount.Content = string.Format("({0})", bottomRecords.ToString());
        }

        private void dgDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //вычисляем сколько записей стоит ниже текущей выбранной
            this.RefreshBottomRecordCount();

            RefreshConditionsAndParameters();
        }

        private DataTableParameters FindDataTableParametersByProfileID(List<DataTableParameters> listOfDeviceParameters, string profileID)
        {
            //ищет в принятом listOfDeviceParameters первую попавшуюся запись, имеющую профиль profileID
            var linqResults = listOfDeviceParameters.Where(fn => fn.ProfileID == profileID);

            return linqResults.FirstOrDefault();
        }

        private List<DataTableParameters> ListOfDeviceParameters()
        {
            //формирует список списков параметров по текущему отображаемому списку изделий            
            if (connection.State != ConnectionState.Open)
                connection.Open();

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
                    int? averageCurrent = AverageCurrent(itemArray);
                    int? deviceClass = DeviceClass(itemArray);
                    string equipment = Equipment(itemArray);
                    string user = User(itemArray);
                    string status = Status(itemArray);
                    string reason = Reason(itemArray);
                    string codeOfNonMatch = CodeOfNonMatch(itemArray);
                    TemperatureCondition temperatureCondition = TemperatureConditionByProfileName(profileName);

                    //conditions для одного и того же профиля одинаковы, пробуем найти в result запись с профилем profileID, чтобы получить из неё conditions без обращения к базе данных - используем result как кеш
                    DataTableParameters dataTableParameters = FindDataTableParametersByProfileID(result, profileID);

                    DataTableParameters p = new DataTableParameters(drv.Row, devID, deviceType, temperatureCondition, profileID, profileName, tsZeroTime, groupName, item, code, constructive, averageCurrent, deviceClass, equipment, user, status, codeOfNonMatch, reason);
                    result.Add(p);

                    p.Load(connection, dataTableParameters, profileID, devID, deviceType, temperatureCondition);
                }
            }

            finally
            {
                connection.Close();
            }

            return result;
        }
        /*
        private ReportData FindData(string profName, string SilN1, string SilN2)
        {
            //ищет DataTableParameters пару в списке записей для принятых profName, SilN1, SilN2          
            string PairProfileName = this.PairProfileBodyByProfileName(profName);

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
        */

        private int? CalcClass(int? value1, int? value2)
        {
            if ((value1 == null) || (value2 == null))
            {
                //если хотя-бы одно из принятых значенией равно null, то и результат null 
                return null;
            }
            else
            {
                //оба принятых значения не null
                return Math.Min((int)value1, (int)value2);
            }
        }

        private ReportByDevices GroupData(List<DataTableParameters> listOfDeviceParameters)
        {
            ReportByDevices result = new ReportByDevices();

            List<DataTableParameters> sortedListOfDeviceParameters = listOfDeviceParameters.OrderBy(x => x.Code).ThenBy(x => x.GroupName).ThenBy(x => x.ProfileName).ToList();

            //просматриваем список sortedListOfDeviceParameters, формируем пары измерений (RT-TM) и метим использованные списки параметров, чтобы не использовать их повторно
            foreach (DataTableParameters dtp in sortedListOfDeviceParameters)
            {
                if ((dtp.Data == null) && (dtp.TemperatureCondition != TemperatureCondition.None))
                {
                    //ищем пару для текущего device
                    dtp.ProfileBody = ProfileRoutines.PairProfileBodyByProfileName(dtp.ProfileName);
                    DataTableParameters pair = this.CalcPairData(listOfDeviceParameters, dtp.ProfileBody, dtp.GroupName, dtp.Code);

                    if (pair != null)
                    {
                        //мы нашли пару - запоминаем в ней тело профиля, по которому мы её нашли
                        pair.ProfileBody = dtp.ProfileBody;

                        //вычисляем значение класса как минимальное из двух значений
                        int? minClass = CalcClass(dtp.DeviceClass, pair.DeviceClass);

                        //запоминаем вычисленное значение класса в построенной паре. это ни как не изменяет отображаемые в форме данные
                        dtp.DeviceClass = minClass;
                        pair.DeviceClass = minClass;
                    }

                    //данный device разрешён для использования (не задействован в ранее построенных парах)
                    ReportData reportData = result.NewReportData();

                    //проверяем это горячее или холодное измерение
                    switch (dtp.TemperatureCondition)
                    {
                        case TemperatureCondition.RT:
                            dtp.Data = reportData;

                            reportData.RTData = dtp;
                            reportData.TMData = pair;

                            if (pair != null)
                                pair.Data = reportData;

                            break;

                        case TemperatureCondition.TM:
                            dtp.Data = reportData;

                            reportData.TMData = dtp;
                            reportData.RTData = pair;

                            if (pair != null)
                                pair.Data = reportData;

                            break;
                    }
                }
            }

            //данные сгруппированы
            return result;
        }

        private void btRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadDevices();
        }

        private ReportByDevices BuildReportInExcel(bool visibleAfterBuilding)
        {
            //построение отчёта в Excel
            if ((this.reportByDevices == null) || (this.reportByDevices.Count == 0))
            {
                MessageBox.Show(Properties.Resources.ReportCannotBeGenerated, Properties.Resources.NoData, MessageBoxButton.OK, MessageBoxImage.Information);
                return null;
            }

            //исходные данные, необходимые для построения протокола испытаний всегда в актуальном состоянии, нет никакой необходимости строить их заново            
            //для того, чтобы в отчёте была обеспечена уникальность шапки (любая шапка в формируемом отчёте должна быть уникальной) выполняем сортировку исходных данных для построения отчёта по ColumnsSignature
            List<ReportData> sortedReportByDevices = this.reportByDevices.OrderByDescending(x => x.ColumnsSignature).ToList<ReportData>();
            ReportByDevices rep = new ReportByDevices(sortedReportByDevices);

            //формируем отчёт
            rep.ToExcel(this.connection, visibleAfterBuilding);

            return rep;
        }

        private void btReport_Click(object sender, RoutedEventArgs e)
        {
            BuildReportInExcel(true);
        }

        private void btReportPrint_Click(object sender, RoutedEventArgs e)
        {
            //формируем отчёт в Excel и не показывая его пользователю сразу отправляем на печать
            ReportByDevices rep = BuildReportInExcel(false);
            rep?.Print();
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
            if (e.Key == Key.F5)
                LoadDevices();
        }
    }

    public class DataViewModel
    {
        public DataTableParameters RT { get; set; }
        public DataTableParameters TM { get; set; }

        public DataViewModel(DataTableParameters rt, DataTableParameters tm)
        {
            this.RT = (rt == null) ? new DataTableParameters() : rt;
            this.TM = (tm == null) ? new DataTableParameters() : tm;
        }

        public DataViewModel()
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
