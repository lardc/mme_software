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
using SCME.dbViewer.ForSorting;
using System.ComponentModel;

namespace SCME.dbViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        //табельный номер, идентификатор аутентифицированного в данном приложении пользователя и битовая маска его разрешений
        public string FUserName = null;
        public long FUserID = -1;
        public long FPermissionsLo = 0;

        //идентификатор пользователя, выбранного для изменения его битовой маски разрешений. т.е. текущий пользователь FUserID есть администратор данного приложения, он выбрал пользователя FManagedUserID с целью изменения его прав
        public long FManagedUserID = -1;
        public long FManagedPermissionsLo = 0;

        /*
        private const int cDevID = 0;
        private const int cGroupName = 1;
        private const int cCode = 2;
        private const int cTsZeroTime = 4;
        private const int cUser = 6;
        private const int cDeviceType = 7;
        private const int cAverageCurrent = 8;
        private const int cСonstructive = 9;
        private const int cItem = 10;
        private const int cProfileID = 13;
        private const int cProfileName = 14;

        //временно отсутствуют
        private const int cDeviceClass = -1;
        private const int cEquipment = -1;
        private const int cStatus = -1;
        private const int cReason = -1;
        private const int cCodeOfNonMatch = -1;

        private List<DataTableParameters> listOfDeviceParameters = new List<DataTableParameters>();
        */

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

            dgDevices.UnFrozeMainFormHandler = this.UnFrozeMainForm;

            //dgDevices.CreateCalculatedFieldsHandler = CreateCalculatedFields;
            dgDevices.GetDeviceTypeHandler = this.DeviceType;
            dgDevices.GetCodeHandler = this.Code;
            dgDevices.GetGroupNameHandler = this.GroupName;
            dgDevices.GetProfileNameHandler = this.ProfileName;
            dgDevices.RefreshBottomRecordCountHandler = this.RefreshBottomRecordCount;
        }

        static void DispatcherUnhandledException(object Sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs E)
        {
            MessageBox.Show(E.Exception.ToString(), "Unhandled exception");
        }

        private static void CurrentDomainOnUnhandledException(object Sender, UnhandledExceptionEventArgs Args)
        {
            MessageBox.Show(Args.ExceptionObject.ToString(), "Unhandled exception");
        }

        private void UnFrozeMainForm()
        {
            this.IsEnabled = true;
        }

        private int DevID(object[] itemArray)
        {
            int index = this.dgDevices.dtData.Columns.IndexOf(Constants.DevID);
            return int.Parse(itemArray?[index].ToString());
        }

        private string ProfileID(object[] itemArray)
        {
            int index = this.dgDevices.dtData.Columns.IndexOf(Constants.ProfileID);
            return itemArray?[index].ToString();
        }

        private DateTime Ts(object[] itemArray)
        {
            int index = this.dgDevices.dtData.Columns.IndexOf(Constants.Ts);
            return DateTime.Parse(itemArray?[index].ToString());
        }

        private string GroupName(object[] itemArray)
        {
            int index = this.dgDevices.dtData.Columns.IndexOf(Constants.GroupName);
            return itemArray?[index].ToString();
        }

        private string Item(object[] itemArray)
        {
            int index = this.dgDevices.dtData.Columns.IndexOf(Constants.Item);
            return itemArray?[index].ToString();
        }

        private string Code(object[] itemArray)
        {
            int index = this.dgDevices.dtData.Columns.IndexOf(Constants.Code);
            return itemArray?[index].ToString();
        }

        private string ProfileName(object[] itemArray)
        {
            int index = this.dgDevices.dtData.Columns.IndexOf(Constants.ProfileName);
            return itemArray?[index].ToString();
        }

        private string DeviceType(object[] itemArray)
        {
            int index = this.dgDevices.dtData.Columns.IndexOf(Constants.DeviceType);
            return itemArray?[index].ToString();
        }

        private string Constructive(object[] itemArray)
        {
            int index = this.dgDevices.dtData.Columns.IndexOf(Constants.Сonstructive);
            return itemArray?[index].ToString();
        }

        private int? AverageCurrent(object[] itemArray)
        {
            int index = this.dgDevices.dtData.Columns.IndexOf(Constants.AverageCurrent);
            int? averageCurrent = itemArray?[index] as int?;

            return (averageCurrent == null) ? null : int.Parse(itemArray?[index].ToString()) as int?;
        }

        private int? DeviceClass(object[] itemArray)
        {
            int index = this.dgDevices.dtData.Columns.IndexOf(Constants.DeviceClass);
            int? deviceClass = itemArray?[index] as int?;

            return (deviceClass == null) ? null : int.Parse(itemArray?[index].ToString()) as int?;
        }

        private string Equipment(object[] itemArray)
        {
            int index = this.dgDevices.dtData.Columns.IndexOf(Constants.MmeCode);
            return itemArray?[index].ToString();
        }

        private string User(object[] itemArray)
        {
            int index = this.dgDevices.dtData.Columns.IndexOf(Constants.Usr);
            return itemArray?[index].ToString();
        }

        private string Status(object[] itemArray)
        {
            int index = this.dgDevices.dtData.Columns.IndexOf(Constants.Status);
            return itemArray?[index].ToString();
        }

        private string CodeOfNonMatch(object[] itemArray)
        {
            int index = this.dgDevices.dtData.Columns.IndexOf(Constants.CodeOfNonMatch);
            return itemArray?[index].ToString();
        }

        private string Reason(object[] itemArray)
        {
            int index = this.dgDevices.dtData.Columns.IndexOf(Constants.Reason);
            return itemArray?[index].ToString();
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

        private void CreateDeviceColumns()
        {
            dgDevices.ClearColumns();

            DataGridColumn column = dgDevices.NewColumn(TemperatureCondition.None, TemperatureCondition.None, Properties.Resources.DevID, Constants.DevID);  //0
            column.Visibility = Visibility.Collapsed;

            column = dgDevices.NewColumn(TemperatureCondition.None, TemperatureCondition.None, Properties.Resources.ProfileID, Constants.ProfileID);         //1
            column.Visibility = Visibility.Collapsed;

            dgDevices.NewColumn(TemperatureCondition.None, TemperatureCondition.None, Properties.Resources.GroupName, Constants.GroupName);                  //2
            dgDevices.NewColumn(TemperatureCondition.None, TemperatureCondition.None, Properties.Resources.Item, Constants.Item);                            //3
            dgDevices.NewColumn(TemperatureCondition.None, TemperatureCondition.None, Properties.Resources.SiType, Constants.SiType);                        //4
            dgDevices.NewColumn(TemperatureCondition.None, TemperatureCondition.None, Properties.Resources.SiOmnity, Constants.SiOmnity);                    //5
            dgDevices.NewColumn(TemperatureCondition.None, TemperatureCondition.None, Properties.Resources.Code, Constants.Code);                            //6

            //ширину столбца с датой/временем регистрации изделия в БД делаем так, чтобы было видно только дату - время пользователь смотрит редко и если ему это понадобится - он сам сделает столбец шире
            column = dgDevices.NewColumn(TemperatureCondition.None, TemperatureCondition.None, Properties.Resources.Ts, Constants.Ts);                       //7
            column.Width = 58;
            ((DataGridTextColumn)column).Binding.StringFormat = ("dd.MM.yy HH:mm:ss");

            //dgDevices.NewColumn(TemperatureCondition.None, TemperatureCondition.None, Properties.Resources.ProfileName, Constants.ProfileName);              //8
            dgDevices.NewColumn(TemperatureCondition.None, TemperatureCondition.None, Properties.Resources.DeviceType, Constants.DeviceType);                //9
            dgDevices.NewColumn(TemperatureCondition.None, TemperatureCondition.None, Properties.Resources.Constructive, Constants.Сonstructive);            //10
            dgDevices.NewColumn(TemperatureCondition.None, TemperatureCondition.None, Properties.Resources.AverageCurrent, Constants.AverageCurrent);        //11
            dgDevices.NewColumn(TemperatureCondition.None, TemperatureCondition.None, Properties.Resources.DeviceClass, Constants.DeviceClass);              //12
            //dgDevices.NewColumn(TemperatureCondition.None, TemperatureCondition.None, Properties.Resources.MmeCode, Constants.MmeCode);                      //13
            //dgDevices.NewColumn(TemperatureCondition.None, TemperatureCondition.None, Properties.Resources.Usr, Constants.Usr);                              //14
            dgDevices.NewColumn(TemperatureCondition.None, TemperatureCondition.None, Properties.Resources.Status, Constants.Status);                        //15
            dgDevices.NewColumn(TemperatureCondition.None, TemperatureCondition.None, Properties.Resources.CodeOfNonMatch, Constants.CodeOfNonMatch);        //16
            dgDevices.NewColumn(TemperatureCondition.None, TemperatureCondition.None, Properties.Resources.Reason, Constants.Reason);                        //17

            //на данный момент ещё не содан ни один столбец темепературного режима 1
            dgDevices.FirstCPColumnIndexInDataGrid1 = -1;
        }

        private void CreateCalculatedFields()
        {
            //создание вычисляемых полей, т.е. тех полей, значения которых не считываются из базы данных
            dgDevices?.dtData?.Columns.Add(Constants.CodeOfNonMatch);
        }

        private void LoadDevices()
        {
            //нас интересуют самые свежие результаты измерений
            string SqlText = "SELECT x.DEV_ID, x.GROUP_NAME, x.CODE, x.MME_CODE, x.TS, x.USR, x.DEVICETYPE, x.AVERAGECURRENT, x.СONSTRUCTIVE, x.ITEM, x.SITYPE, x.SIOMNITY, x.DEVICECLASS, x.STATUS, x.REASON, x.CODEOFNONMATCH, x.PROF_ID, x.PROF_NAME," +
                             "       (" +
                             "         SELECT T.TEST_TYPE_NAME AS Test, RTRIM(C.COND_NAME) AS Name, RTRIM(CAST(PC.VALUE AS VARCHAR(10))) AS Value" +
                             "         FROM PROF_COND PC" +
                             "          INNER JOIN PROF_TEST_TYPE PTT ON (PC.PROF_TESTTYPE_ID=PTT.PTT_ID)" +
                             "          INNER JOIN TEST_TYPE T ON (PTT.TEST_TYPE_ID=T.TEST_TYPE_ID)" +
                             "          INNER JOIN CONDITIONS C ON (PC.COND_ID=C.COND_ID)" +
                             "         WHERE (x.PROF_ID=PC.PROF_ID)" +
                             "         FOR XML AUTO, ROOT('CONDITIONS')" +
                             "       ) AS PROFCONDITIONS," +
                             "       (" +
                             "         SELECT TT.TEST_TYPE_NAME AS Test, RTRIM(P.PARAM_NAME) AS Name, ISNULL(P.PARAMUM, '') AS Um, CAST(DP.VALUE AS VARCHAR(10)) AS Value, CAST(PP.MIN_VAL AS VARCHAR(10)) AS NrmMin, CAST(PP.MAX_VAL AS VARCHAR(10)) AS NrmMax" +
                             "         FROM DEV_PARAM DP" +
                             "          INNER JOIN PROF_TEST_TYPE PTTD ON (DP.TEST_TYPE_ID=PTTD.PTT_ID)" +
                             "          INNER JOIN TEST_TYPE TT ON (PTTD.TEST_TYPE_ID=TT.TEST_TYPE_ID)" +
                             "          INNER JOIN PARAMS P ON (DP.PARAM_ID=P.PARAM_ID)" +
                             "          LEFT JOIN PROF_PARAM PP ON (" +
                             "                                      (DP.TEST_TYPE_ID=PP.PROF_TESTTYPE_ID) AND" +
                             "                                      (DP.PARAM_ID=PP.PARAM_ID)" +
                             "                                     )" +
                             "         WHERE (x.DEV_ID=DP.DEV_ID)" +
                             "         FOR XML AUTO, ROOT('PARAMETERS')" +
                             "       ) AS DEVICEPARAMETERS" +
                             " FROM" +
                             "      (" +
                             "        SELECT RTRIM(G.GROUP_NAME) AS GROUP_NAME, D.DEV_ID, D.CODE, D.MME_CODE, D.TS, D.USR, D.DEVICETYPE, D.AVERAGECURRENT, D.СONSTRUCTIVE, D.ITEM, D.SITYPE, D.SIOMNITY, D.DEVICECLASS, D.STATUS, D.REASON, D.CODEOFNONMATCH, P.PROF_ID, P.PROF_NAME, ROW_NUMBER() OVER(PARTITION BY D.GROUP_ID, D.CODE, D.MME_CODE, P.PROF_NAME ORDER BY D.DEV_ID DESC) AS RN" +
                             "        FROM DEVICES D" +
                             "         INNER JOIN PROFILES AS P ON (" +
                             "                                       (D.PROFILE_ID=P.PROF_GUID) AND" +
                             "                                       (ISNULL(P.IS_DELETED, 0)=0)" +
                             "                                     )" +
                             "         INNER JOIN GROUPS AS G ON (D.GROUP_ID=G.GROUP_ID)" +
                             "      ) x" +
                             " WHERE (x.RN=1)";

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

            SqlText += ((dateBeg != null) || (dateEnd != null) || (job != null) || (deviceType != null) || (constructive != null) || (averageCurrent != null) || (deviceClass != null) || (siType != null) || (profName != null) || (mmeCode != null) || (usr != null)) ? " AND" : string.Empty;

            string whereSection = string.Empty;

            if (dateBeg != null)
                whereSection = string.Format(" dbo.DateTimeToDateZeroTime(TS)>='{0}'", dateBeg);

            if (dateEnd != null)
            {
                if (whereSection != string.Empty)
                    whereSection += " AND";

                whereSection += string.Format(" dbo.DateTimeToDateZeroTime(TS)<='{0}'", dateEnd);
            }

            if (job != null)
            {
                if (whereSection != string.Empty)
                    whereSection += " AND";

                whereSection += string.Format(" GROUP_NAME='{0}'", job);
            }

            if (deviceType != null)
            {
                if (whereSection != string.Empty)
                    whereSection += " AND";

                whereSection += string.Format(" DEVICETYPE='{0}'", deviceType);
            }

            if (constructive != null)
            {
                if (whereSection != string.Empty)
                    whereSection += " AND";

                whereSection += string.Format(" СONSTRUCTIVE='{0}'", constructive);
            }

            if (averageCurrent != null)
            {
                if (whereSection != string.Empty)
                    whereSection += " AND";

                whereSection += string.Format(" AVERAGECURRENT='{0}'", averageCurrent);
            }

            if (deviceClass != null)
            {
                if (whereSection != string.Empty)
                    whereSection += " AND";

                whereSection += string.Format(" DEVICECLASS='{0}'", deviceClass);
            }

            if (siType != null)
            {
                if (whereSection != string.Empty)
                    whereSection += " AND";

                whereSection += string.Format(" SITYPE='{0}'", siType);
            }

            if (profName != null)
            {
                if (whereSection != string.Empty)
                    whereSection += " AND";

                whereSection += string.Format(" PROF_NAME='{0}'", profName);
            }

            if (mmeCode != null)
            {
                if (whereSection != string.Empty)
                    whereSection += " AND";

                whereSection += string.Format(" MME_CODE='{0}'", mmeCode);
            }

            if (usr != null)
            {
                if (whereSection != string.Empty)
                    whereSection += " AND";

                whereSection += string.Format(" USR='{0}'", usr);
            }

            if (whereSection != string.Empty)
                SqlText += whereSection;

            //чтобы пользователь не смог нажать кнопку или изменить фильтр - во время загрузки данных блокируем всю форму со всем её содержимым
            this.IsEnabled = false;

            //исполняется весьма долго и чтобы не получить исключительную ситуацию о ставшей очереди сообщений вызывает её принудительную обработку
            //эта обработка очереди сообщений позволяет пользователю во время загрузки данных делать что, что он не должен делать - нажимать кнопки, устанвливать фильтры и т.д. именно поэтому перед вызовом данной реализации вызывается this.IsEnabled
            dgDevices.ViewSqlResultByThread(SqlText);
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

        /*
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
        */


        public delegate void delegateRefreshBottomRecordCount();
        private void RefreshBottomRecordCount()
        {
            //вычисляем сколько записей стоит ниже текущей выбранной
            if (dgDevices.SelectedItem == null)
            {
                lbBottomRecordCount.Content = string.Empty;
            }
            else
            {
                int selectedRowNum = dgDevices.Items.IndexOf(dgDevices.SelectedItem) + 1;
                int bottomRecords = dgDevices.Items.Count - selectedRowNum;

                lbBottomRecordCount.Content = string.Format("({0})", bottomRecords.ToString());
            }
        }

        private void dgDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //вычисляем сколько записей стоит ниже текущей выбранной
            this.RefreshBottomRecordCount();
        }

        /*
        private DataTableParameters FindDataTableParametersByProfileID(List<DataTableParameters> listOfDeviceParameters, string profileID)
        {
            //ищет в принятом listOfDeviceParameters первую попавшуюся запись, имеющую профиль profileID
            var linqResults = listOfDeviceParameters.Where(fn => fn.ProfileID == profileID);

            return linqResults.FirstOrDefault();
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

        private void btRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadDevices();
        }

        private ReportData BuildReportInExcel(bool visibleAfterBuilding)
        {
            //построение отчёта в Excel
            DataView dv = this.dgDevices.ItemsSource as DataView;
            DataTable dt = dv.ToTable();

            if (dt.Rows.Count == 0)
            {
                MessageBox.Show(Properties.Resources.ReportCannotBeGenerated, Properties.Resources.NoData, MessageBoxButton.OK, MessageBoxImage.Information);
                return null;
            }

            ReportData reportData = new ReportData(dt, this.dgDevices);

            //для того, чтобы в отчёте была обеспечена уникальность шапки (любая шапка в формируемом отчёте должна быть уникальной) выполняем сортировку исходных данных для построения отчёта по ColumnsSignature, а внутри каждого уникального набора по коду ГП
            CustomComparer<object> customComparer = new CustomComparer<object>(ListSortDirection.Ascending);
            List<ReportRecord> sortedReportByDevices = reportData.OrderByDescending(x => x.ColumnsSignature).ThenBy(x => x.Code, customComparer).ToList<ReportRecord>();

            ReportData rep = new ReportData(sortedReportByDevices);

            //формируем отчёт            
            rep.ToExcel(this.dgDevices.connection, visibleAfterBuilding);

            return rep;
        }

        private void btReport_Click(object sender, RoutedEventArgs e)
        {
            BuildReportInExcel(true);
        }

        private void btReportPrint_Click(object sender, RoutedEventArgs e)
        {
            //string s = string.Format("{0:X2}{1:X2}{2:X2}", 150, 174, 226);

            //формируем отчёт в Excel и не показывая его пользователю сразу отправляем на печать           
            ReportData rep = BuildReportInExcel(false);
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

        private void mnuBeginSessionClick(object sender, RoutedEventArgs e)
        {
            //начать сеанс работы
            AuthenticationWindow auth = new AuthenticationWindow();

            if (auth.ShowModal(out this.FUserID, out this.FPermissionsLo) ?? false)
            {
                if (Routines.IsUserAdmin(this.FPermissionsLo))
                {
                    mnuSelectDCUser.Visibility = Visibility.Visible;
                    mnuBitCalculator.Visibility = Visibility.Visible;
                }
                else
                {
                    mnuSelectDCUser.Visibility = Visibility.Collapsed;
                    mnuBitCalculator.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void mnuCloseSessionClick(object sender, RoutedEventArgs e)
        {
            //завершить сеанс работы - забываем ранее авторизованного пользователя и его права
            this.FUserID = -1;
            this.FPermissionsLo = 0;

            mnuSelectDCUser.Visibility = Visibility.Collapsed;
            mnuBitCalculator.Visibility = Visibility.Collapsed;
        }

        private void mnuSelectDCUserClick(object sender, RoutedEventArgs e)
        {
            //выбор пользователя из списка пользователей DC
            if (Routines.IsUserAdmin(this.FPermissionsLo))
            {
                DCUsersList dcUsersList = new DCUsersList();

                if (dcUsersList.ShowModal() ?? false)
                {
                    BitCalculator bitCalc = new BitCalculator();
                    bitCalc.ShowModal(this.FManagedPermissionsLo);
                }
            }
        }

        private void mnuBitCalculatorClick(object sender, RoutedEventArgs e)
        {
            //управление правами пользователей доступно только пользователю, который является администратором этой системы
            if (Routines.IsUserAdmin(this.FPermissionsLo))
            {
                //имеем случай, когда пользователь, являющийся администратором управляет собственной битовой маской разрешений
                BitCalculator bitCalc = new BitCalculator();
                bitCalc.ShowModal(this.FPermissionsLo);
            }
        }
    }
}
