using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using SCME.dbViewer.ForFilters;
using System.Text.RegularExpressions;
using System.Collections;
using SCME.dbViewer.ForSorting;
using System.Xml.Linq;
using System.Xml;
using System.Globalization;
using SCME.dbViewer.ForParameters;
using SCME.Types.BaseTestParams;
using SCME.Types.Profiles;
using System.Runtime.Serialization;
using SCME.InterfaceImplementations;
using System.Collections.Concurrent;

namespace SCME.dbViewer.CustomControl
{
    /// <summary>
    /// Interaction logic for DataGridSqlResult.xaml
    /// </summary>
    public partial class DataGridSqlResult : DataGrid
    {
        private ConcurrentQueue<Action> queueManager = new ConcurrentQueue<Action>();
        private Cache cache = null;
        private DataGridColumnHeader lastHeaderClicked = null;
        private ListSortDirection lastSortedDirection = ListSortDirection.Ascending;
        private Button btFilterClicked = null;
        private ActiveFilters activeFilters;

        public SqlConnection connection = null;
        public DataTable dtData = null;

        public delegate void UnFrozeMainForm();
        public UnFrozeMainForm UnFrozeMainFormHandler { get; set; }

        /*
        public delegate void CreateCalculatedFields();
        public CreateCalculatedFields CreateCalculatedFieldsHandler { get; set; }
        public delegate void LoadPCNHandler(SqlConnection connection, object[] values);
        public LoadPCNHandler LoadPCN { get; set; }
        */

        public delegate string GetDeviceType(object[] values);
        public GetDeviceType GetDeviceTypeHandler { get; set; }

        public delegate string GetCode(object[] values);
        public GetCode GetCodeHandler { get; set; }

        public delegate string GetGroupName(object[] values);
        public GetGroupName GetGroupNameHandler { get; set; }

        public delegate string GetProfileName(object[] values);
        public GetProfileName GetProfileNameHandler { get; set; }

        public MainWindow.delegateRefreshBottomRecordCount RefreshBottomRecordCountHandler { get; set; }

        private const string cNewStringDelimeter = "\n";

        //здесь храним индекс первого столбца набора conditions/patameters первого температурного режима в списке столбцов DataTable
        public int FirstCPColumnIndexInDataTable1 { get; set; }

        //здесь храним индекс последнего столбца набора conditions/patameters первого температурного режима в списке столбцов DataTable
        public int LastCPColumnIndexInDataTable1 { get; set; }

        //число столбцов в DataTable и в DataGrid отличается - в DataGrid нет Hidden столбцов, которые есть в DataTable. индексы столбцов в DataGrid и DataTable не совпадают
        //здесь храним индекс первого столбца набора conditions/patameters первого температурного режима в списке столбцов DataGrid
        public int FirstCPColumnIndexInDataGrid1 { get; set; }

        //здесь храним индекс последнего столбца набора conditions/patameters первого температурного режима в списке столбцов DataGrid
        public int LastCPColumnIndexInDataGrid1 { get; set; }

        public DataGridSqlResult() : base()
        {
            InitializeComponent();

            this.Sorting += new DataGridSortingEventHandler(SortHandler);
            this.activeFilters = new ActiveFilters();
        }

        public void RTStyle()
        {
            this.RowStyle = (Style)FindResource("DataGridRowRT");
        }

        public void TMStyle()
        {
            this.ColumnHeaderStyle = (Style)FindResource("DataGridColumnHeaderCustomTM");
            this.RowStyle = (Style)FindResource("DataGridRowTM");
        }

        private void btFilter_Click(object sender, RoutedEventArgs e)
        {
            //выставляем флаг о прошедшем нажатии кнопки
            btFilterClicked = (Button)sender;
        }

        public void ClearColumns()
        {
            this.Columns.Clear();
            this.FirstCPColumnIndexInDataGrid1 = -1;
        }

        private void ClearDynamicColumns()
        {
            //удаление столбцов, которые были созданы в результате обработки данных из XML описания (полученного в качестве результата выполнения SQL запроса) и столбцов, созданных CreateNotImportantColumns
            //данная реализация вызывается фоновым потоком, поэтому просто так её вызывать нельзя
            queueManager.Enqueue(delegate
                                 {
                                     if (this.FirstCPColumnIndexInDataGrid1 != -1)
                                     {
                                         while ((this.Columns.Count - 1) >= this.FirstCPColumnIndexInDataGrid1)
                                             this.Columns.RemoveAt(this.FirstCPColumnIndexInDataGrid1);

                                         this.FirstCPColumnIndexInDataGrid1 = -1;
                                     }
                                 }
                                );
        }

        public DataGridColumn NewColumn(TemperatureCondition tc, TemperatureCondition temperatureCondition1, string header, string bindPath)
        {
            //создание нового столбца
            //tc - температурный режим которому должен принадлежать создаваемый в DataGrid столбец
            //temperatureCondition1 - фактический температурный режим 1
            //header - то как пользователь будет видеть название этого столбца
            //bindPath - столбец будет отображать данные столбца binding из доступного списка столбцов

            DataGridTextColumn textColumn = new DataGridTextColumn();
            textColumn.Header = header;
            textColumn.Binding = new Binding(bindPath);

            this.Columns.Add(textColumn);

            //если создаётся столбец не имеющий принадлежности к температурному режиму - не управляем установкой DisplayIndex - столбец всегда будет создан самым последним в списке уже созданных столбцов
            if (tc != TemperatureCondition.None)
            {
                //столбец принадлежит температурному режиму - устанавливаем ему индекс в списке столбцов
                if (temperatureCondition1 != TemperatureCondition.None)
                {
                    //столбец принадлежит температурному режиму 1 - ставим его самым последним в списке столбцов температурного режима 1. при этом все столбцы, стоящие за ним автоматически пересчитывают свои индексы
                    if (tc == temperatureCondition1)
                    {
                        if (this.FirstCPColumnIndexInDataGrid1 == -1)
                        {
                            this.FirstCPColumnIndexInDataGrid1 = this.Columns.IndexOf(textColumn);
                            this.LastCPColumnIndexInDataGrid1 = this.FirstCPColumnIndexInDataGrid1;
                        }
                        else
                        {
                            //раз мы создали новый столбец в наборе conditions/parameters температурного режима 1 - увеличиваем значение счётчика хранящего индекс последнего столбца набора conditions/parameters температурного режима 1
                            this.LastCPColumnIndexInDataGrid1++;

                            //двигаем созданный столбец в DataGrid
                            textColumn.DisplayIndex = this.LastCPColumnIndexInDataGrid1;
                        }
                    }
                }
            }

            return textColumn;
        }

        private void SetRowFilter()
        {
            var dv = this.ItemsSource as DataView;

            if (dv != null)
            {
                try
                {
                    dv.RowFilter = this.FiltersToString();
                }
                catch (Exception)
                {
                    MessageBox.Show("Введённое значение фильтра не соответствует типу фильтруемых данных.", "Filter error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    //MessageBox.Show(string.Format("При установке фильтра возникла исключительная ситуация:\r\n{0}", ex.ToString()), "Filter error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
        }

        private void CreateNotImportantColumns()
        {
            //создание не важных для пользователя столбцов - их создание выполняется в самом конце списка уже имеющихся на момент вызова столбцов
            //данная реализация вызывается фоновым потоком, поэтому просто так её вызывать нельзя
            queueManager.Enqueue(
                                  delegate
                                  {
                                      this.NewColumn(TemperatureCondition.None, TemperatureCondition.None, Properties.Resources.ProfileName, Constants.ProfileName);
                                      this.NewColumn(TemperatureCondition.None, TemperatureCondition.None, Properties.Resources.MmeCode, Constants.MmeCode);
                                      this.NewColumn(TemperatureCondition.None, TemperatureCondition.None, Properties.Resources.Usr, Constants.Usr);
                                  }
                                );
        }

        private void ViewSqlResult(string queryString)
        {
            //отображение результата выполнения запроса с текстом queryString           
            this.connection = DBConnections.Connection;

            this.connection.Open();

            try
            {
                //инициализируем места хранения начального и конечного индексов набора condition/parameters первого температурного режима
                this.FirstCPColumnIndexInDataTable1 = -1;
                this.LastCPColumnIndexInDataTable1 = -1;

                SqlCommand command = new SqlCommand(queryString, this.connection);
                command.CommandTimeout = 1000;
                SqlDataReader reader = command.ExecuteReader();
                DataTable columnsDT = null;
                object[] startRecordValues = null;
                int? devIDFromStartRecord = null;

                try
                {
                    this.dtData = CreateDataTable(reader);

                    //создаём DataTable чтобы собрать в ней список всех столбцов с корректными значениями типов для каждого столбца, хранящегося в XML 
                    columnsDT = CreateDataTable(reader);

                    //ищем первую попавшуюся запись, в которой сохранены результаты измерений при RT                    
                    devIDFromStartRecord = this.FindStartRecord(reader, out startRecordValues);
                }

                finally
                {
                    reader.Close();
                }

                this.FillDataTable(columnsDT, devIDFromStartRecord, startRecordValues, command);

                this.CreateNotImportantColumns();
            }

            finally
            {
                this.connection.Close();
            }
        }

        private int ColumnInDataTableByName(DataTable dtTable, string columnName)
        {
            return dtTable.Columns.IndexOf(columnName);
        }

        private TemperatureCondition TemperatureCondition1(TemperatureCondition tcByNewColumn)
        {
            //вычисляет температурный режим 1
            TemperatureCondition result = TemperatureCondition.None;

            if (this.FirstCPColumnIndexInDataTable1 == -1)
            {
                //ещё не создан ни один столбец температурного режима 1 - температурный режим 1 не определён, поэтому он должен быть определён по первому создаваемому столбцу
                result = tcByNewColumn;
            }
            else
            {
                //создан хотя-бы один столбец температурного режима 1 - считываем температурный режим 1
                if (this.dtData != null)
                {
                    DataColumn column = this.dtData.Columns[this.FirstCPColumnIndexInDataTable1];

                    result = ProfileRoutines.TemperatureConditionByProfileName(column.ColumnName);
                }
            }

            return result;
        }

        private int NewColumnInDataTable(DataTable dt, TemperatureCondition tc, TemperatureCondition temperatureCondition1, string columnName, Type columnType, bool unique, bool allowDBNull, bool autoIncrement)
        {
            //создание нового столбца в dtTable
            //в tc принимается температурный режим, которому должен принадлежать создаваемый столбец, в temperatureCondition1 - фактическое значение температурного режима 1
            //возвращает индекс созданного столбца
            if (dt == null)
                return -1;

            DataColumn column = new DataColumn(columnName, columnType);
            column.Unique = unique;
            column.AllowDBNull = allowDBNull;
            column.AutoIncrement = autoIncrement;
            column.DefaultValue = DBNull.Value;

            dt.Columns.Add(column);

            //если создаётся столбец не имеющий принадлежности к температурному режиму (это не видимый в интерфейсе столбец, пример - столбец Pair с именем профиля) - не управляем установкой Ordinal - столбец всегда будет создан самым последним в списке уже созданных столбцов
            //столбец принадлежит температурному режиму - устанавливаем ему индекс в списке столбцов
            if (tc != TemperatureCondition.None)
            {
                //столбец принадлежит температурному режиму
                if (temperatureCondition1 != TemperatureCondition.None)
                {
                    //столбец принадлежит температурному режиму 1 - ставим его самым последним в списке столбцов температурного режима 1. при этом все столбцы, стоящие за ним автоматически пересчитывают свои индексы
                    if (tc == temperatureCondition1)
                    {
                        if (this.FirstCPColumnIndexInDataTable1 == -1)
                        {
                            if (dt == this.dtData)
                            {
                                //данная реализация запускается и для принятого dt=this.dtData и для принятого dt=columnsDT. выставляем счётчики индексов только для случая dt=this.dtData
                                this.FirstCPColumnIndexInDataTable1 = dt.Columns.IndexOf(column);
                                this.LastCPColumnIndexInDataTable1 = this.FirstCPColumnIndexInDataTable1;
                            }
                        }
                        else
                        {
                            //раз мы создали новый столбец в наборе conditions/parameters температурного режима 1 - увеличиваем значение счётчика хранящего индекс последнего столбца набора conditions/parameters температурного режима 1
                            if (dt == this.dtData)
                            {
                                //данная реализация запускается и для принятого dt=this.dtData и для принятого dt=columnsDT. выставляем счётчики индексов только для случая dt=this.dtData
                                this.LastCPColumnIndexInDataTable1++;
                            }

                            column.SetOrdinal(this.LastCPColumnIndexInDataTable1);
                        }
                    }
                }
            }

            return dt.Columns.IndexOf(column);
        }

        private void NewColumnInDataGrid(TemperatureCondition tc, TemperatureCondition temperatureCondition1, string nameInDataTable, string nameInDataGrid)
        {
            //создание нового столбца в this
            queueManager.Enqueue(() => this.NewColumn(tc, temperatureCondition1, nameInDataGrid, nameInDataTable));
        }

        private DataTable CreateDataTable(SqlDataReader reader)
        {
            DataTable result = null;

            DataTable dtSchema = reader.GetSchemaTable();

            if (dtSchema != null)
            {
                result = new DataTable();

                //копируем все столбцы из dtSchema
                foreach (DataRow row in dtSchema.Rows)
                {
                    string nameInDataTable = System.Convert.ToString(row["ColumnName"]);

                    //смотрим какому температурному режиму принадлежит создаваемый столбец
                    TemperatureCondition tc = ProfileRoutines.TemperatureConditionByProfileName(nameInDataTable);

                    //смотрим с каким температурным режимом 1 мы имеем дело (может быть как RT, так и TM в зависимости от того какой встретится первым)
                    TemperatureCondition temperatureCondition1 = this.TemperatureCondition1(tc);

                    NewColumnInDataTable(result, tc, temperatureCondition1, nameInDataTable, (Type)(row["DataType"]), (bool)row["IsUnique"], (bool)row["AllowDBNull"], (bool)row["IsAutoIncrement"]);
                }
            }

            return result;
        }

        private DataTable SetTypeForData(DataTable table, DataTable columnsDT, int firstColumnIndex)
        {
            if (table.Columns.Count != columnsDT.Columns.Count)
                throw new Exception(string.Format("table.Columns.Count={0}, columnsDT.Columns.Count={1}. Ожидалось их равенство.", table.Columns.Count.ToString(), columnsDT.Columns.Count.ToString()));

            DataTable dtCloned = table.Clone();

            for (int i = firstColumnIndex; i < table.Columns.Count; i++)
                dtCloned.Columns[i].DataType = columnsDT.Columns[i].DataType;

            foreach (DataRow row in table.Rows)
                dtCloned.ImportRow(row);

            return dtCloned;
        }

        private int? FindStartRecord(SqlDataReader reader, out object[] values)
        {
            //ищет в принятом reader первую попавшуюся запись с температурным режимом измерения RT и в качестве результата возвращает значение поля DevID. если же во множестве записей reader нет ни одной с температурным режимом измерения RT (например из-за фильтров) - возвращает null

            //нужда в такой записи возникает по следуюшим причинам:
            //1. технологи хотят чтобы первым температурном режимом был именно RT (а не TM) - им так удобно;
            //2. столбцы DataTable dtData, хранящие имена conditions/parameters формируются динамически при чтении данных;
            //3. первая прочитанная запись из reader определяет каким будет температурный режим 1: если первая просмотренная запись будет нести набор RT conditions/parameters - то и первый температурный режим будет RT. если первая просмотренная запись будет нести набор TM conditions/parameters - то и первый температурный режим будет TM (см. реализацию NewColumnInDataTable)
            //4. казалось бы можно просто выполнить сортировку набора данных возвращаемых SQL сервером по наименованию профиля. при этом обход данных в reader был бы таким как нам надо - сначала записи с температурным режимом RT, потом с TM. но технологи частенько нарушают свои же правила по формированию имён профилей и вытащить из имени профиля обозначение температурного режима нельзя. пример такого профиля '0.332A.х.024.016.0290.TR211.x'. поэтому от использования сортировки отказался

            values = new object[reader.FieldCount];

            //XML полей в каждой записи два: сначала столбец с conditions, за ним столбец с parameters
            int indexOfXMLColumnConditions = this.dtData.Columns.IndexOf(Constants.XMLConditions);

            TemperatureCondition temperatureCondition = TemperatureCondition.None;
            string temperatureValue = string.Empty;

            XmlDocument xmlDoc = new XmlDocument();

            while (reader.Read())
            {
                reader.GetValues(values);
                object value = values[indexOfXMLColumnConditions];

                //нам надо прочитать описание температурного режима, оно есть в conditions, формат хранения XML
                //убеждаемся, что value в формате XML
                string sXML = ValueIsXml(value);

                if (sXML != null)
                {
                    xmlDoc.LoadXml(sXML);
                    XmlElement documentElement = xmlDoc.DocumentElement;

                    XMLValues subject = (documentElement.Name == "CONDITIONS") ? XMLValues.Conditions : (documentElement.Name == "PARAMETERS") ? XMLValues.Parameters : XMLValues.UnAssigned;

                    //считываем при какой температуре проводятся измерения. эта информация есть только в описании условий измерения                        
                    if (subject == XMLValues.Conditions)
                    {
                        temperatureCondition = TemperatureConditionFromXML(xmlDoc, out temperatureValue);

                        if (temperatureCondition == TemperatureCondition.RT)
                        {
                            //мы нашли запись с температурным режимом RT - извлекаем идентификатор DevID
                            int indexOfDevID = this.dtData.Columns.IndexOf(Constants.DevID);

                            return values[indexOfDevID] as int?;
                        }
                    }
                }
            }

            //мы просмотрели всё множество записей и не нашли нужную
            values = null;
            return null;
        }

        public void ViewSqlResultByThread(string queryString)
        {
            //выполнение реализации this.ViewSqlResultHandler в фоновом потоке, после выполнения которой система исполнит реализацию this.AfterFillDataTableHandler в главном потоке
            LongTimeRoutineWorker Worker = new LongTimeRoutineWorker(this.ViewSqlResultHandler, this.AfterFillDataTableHandler);

            Object[] args = { queryString };
            Worker.Run(args);
        }

        private void ViewSqlResultHandler(DoWorkEventArgs e)
        {
            //извлекаем из принятого workerParameters параметры, необходимые для вызова this.ViewSqlResult
            Object[] arg = e.Argument as Object[];
            string queryString = (string)arg[0];

            this.ViewSqlResult(queryString);
        }

        private void AfterFillDataTableHandler(string error)
        {
            //проверяем не завершилось ли исполнение протоковой функции ошибкой
            if (error == string.Empty)
            {
                //потоковая функция исполнена успешно - обрабатываем отложенную очередь вызовов
                Action act;

                while (queueManager.TryDequeue(out act))
                    act.Invoke();

                this.SetItemsSource(this.dtData);

                //загрузка данных завершилась - разблокируем форму
                this.UnFrozeMainFormHandler();
            }
            else MessageBox.Show(error, Properties.Resources.LoadDataFromDataBaseFault, MessageBoxButton.OK, MessageBoxImage.Exclamation);

        }

        private void FillDataTable(DataTable columnsDT, int? devIDFromStartRecord, object[] startRecordValues, SqlCommand command)
        {
            this.ClearDynamicColumns();

            //создаём кеш для выполнения группировки данных (для образования пар RT-TM)
            this.cache = new Cache();

            //запоминаем индекс будущего первого созданного столбца на основе данных XML - т.е. следуюшего за только что последним созданным столбцом
            int lastColumnIndex = columnsDT.Columns.Count;

            //если запись найдена - обрабатываем её
            if (devIDFromStartRecord != null)
                FillValues(columnsDT, startRecordValues, this.dtData);

            //создаём новый reader только потому что при выполнении this.FindStartRecord мы смещаемся во множестве записей (выполняя reader.Read()) без возможности вернутся к начальной записи 
            //создавая новый reader мы просматриваем всё множество записей, в том числе и найденную StartRecord
            SqlDataReader reader = command.ExecuteReader();
            object[] values = new object[reader.FieldCount];

            while (reader.Read())
            {
                reader.GetValues(values);
                bool needProcessing = true;

                if (devIDFromStartRecord != null)
                {
                    //смотрим на идентификатор DevID текущей записи, чтобы не допустить повторной обработки уже обработанной записи StartRecord
                    int indexOfDevID = this.dtData.Columns.IndexOf(Constants.DevID);
                    int currentDevID = (int)values[indexOfDevID];

                    needProcessing = (currentDevID != devIDFromStartRecord);
                }

                if (needProcessing)
                    FillValues(columnsDT, values, this.dtData);

                //прокачиваем очередь сообщений ибо загрузка данных может быть длительным процессом
                //если этого не делать - то при превышении времени исполнения 60 сек данной реализацией получим исключительную ситуацию
                //System.Windows.Forms.Application.DoEvents();
            }

            //все возможные пары RT-TM уже образованы, на момент вызова в this.cache остались только такие записи, которые не имеют пары - устанавливаем им значения реквизитов DeviceClass и Status равным DbNull
            this.cache.SetDeviceClassAndStatusToDbNull();

            //в columnsDT собран список столбцов с корректными типами столбцов. данный список столбцов появился в результате обработки данных в XML полях, полученных от SQL сервера. выполняем типизацию хранящихся в this.dtData значений начиная со столбца с индексом lastColumnIndex в соответствии с вычисленными типами столбцов
            this.dtData = SetTypeForData(this.dtData, columnsDT, (int)lastColumnIndex);
        }

        /*
        public void ViewSqlResultAsync(string SqlQuery)
        {
            SqlConnection connection = CreateConnection();
            connection.Open();

            SqlCommand cmd = new SqlCommand(SqlQuery, connection);

            cmd.BeginExecuteReader(new AsyncCallback(CallbackFunction), cmd);
        }

        private void CallbackFunction(IAsyncResult result)
        {
            SqlCommand cmd = (SqlCommand)result.AsyncState;
            SqlDataReader reader = cmd.EndExecuteReader(result);
            DataTable dtSchema = reader.GetSchemaTable();

            if (dtSchema != null)
            {
                this.dtData = new DataTable();

                foreach (DataRow row in dtSchema.Rows)
                {
                    string columnName = System.Convert.ToString(row["ColumnName"]);
                    DataColumn column = new DataColumn(columnName, (Type)(row["DataType"]));
                    column.Unique = (bool)row["IsUnique"];
                    column.AllowDBNull = (bool)row["AllowDBNull"];
                    column.AutoIncrement = (bool)row["IsAutoIncrement"];
                    dtData.Columns.Add(column);
                }

                this.CreateCalculatedFieldsHandler();

                Dispatcher.BeginInvoke(new delegateSetItemsSource(SetItemsSource), dtData);

                while (reader.Read())
                {
                    object[] values = new object[reader.FieldCount];
                    reader.GetValues(values);

                    Dispatcher.BeginInvoke(new delegateFillValues(FillValues), values, dtData);

                    LoadPCN(this.connection, values);
                }

                reader.Close();

                this.SetRowFilter();

                if (this.Items.Count > 0)
                    this.SelectedIndex = 0;
            }

            //if (cmd.Connection.State.Equals(ConnectionState.Open))
            //    cmd.Connection.Close();
        }
        */

        private Type TypeOfValue(string value, out string correctedValue)
        {
            //вычисляет тип принятого value по принципу: всё что не double - то string
            //не будем ограничивать пользователя в выборе системного разделителя дробной части от целой части - вернём в correctedValue корректное значение для любого значения системного разделителя дробной части от целой части
            double d;
            bool parsedAsDouble = double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out d);
            correctedValue = parsedAsDouble ? d.ToString() : value;

            return parsedAsDouble ? typeof(double) : typeof(string);
        }

        private bool ValueAsDouble(string value, out double correctedValue)
        {
            return double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out correctedValue);
        }

        private void StoreType(DataTable columnsDT, int columnIndex, Type columnDataType)
        {
            //выполняет сравнение принятого columnDataType с сохранённым ранее типом для столбца с индексом columnIndex. если в столбце columnIndex будет хотя бы одно значение с типом string - то тип всего столбца должент быть string
            DataColumn column = columnsDT.Columns[columnIndex];

            if (column != null)
            {
                Type storedDataType = column.DataType;

                if ((storedDataType != typeof(string)) && (columnDataType == typeof(string)))
                    column.DataType = columnDataType;
            }
        }

        [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
        private enum EType
        {
            [EnumMember]
            Unknown = 0,

            [EnumMember]
            Diode = 1,

            [EnumMember]
            Thyristor = 2
        }

        private EType ETypeByDeviceType(string deviceType)
        {
            EType result = EType.Unknown;

            //тиристорный тип: в deviceType должен присутствовать либо рус либо лат символ T
            if ((deviceType.IndexOf("Т", StringComparison.InvariantCultureIgnoreCase) != -1) || (deviceType.IndexOf("T", StringComparison.InvariantCultureIgnoreCase) != -1))
                result = EType.Thyristor;

            //диодный тип
            if (deviceType.IndexOf("Д", StringComparison.InvariantCultureIgnoreCase) != -1)
                result = EType.Diode;

            return result;
        }

        public List<string> ConditionNamesByDeviceType(TestParametersType testType, string deviceType, TemperatureCondition temperatureCondition)
        {
            //возвращает список условий, которые надо показывать пользователю
            List<string> result = null;

            if ((deviceType != null) && (deviceType != string.Empty))
            {
                switch (testType)
                {
                    case TestParametersType.StaticLoses:
                        result = new List<string>();
                        result.Add("SL_ITM"); //в БД это же условие используется как IFM
                        break;

                    case TestParametersType.Bvt:
                        //холодное измерение
                        if (temperatureCondition == TemperatureCondition.RT)
                        {
                            result = new List<string>();
                            result.Add("BVT_I");

                            EType eType = ETypeByDeviceType(deviceType);

                            if ((eType == EType.Diode) || (eType == EType.Thyristor))
                                result.Add("BVT_VR");
                        }

                        //горячее измерение
                        if (temperatureCondition == TemperatureCondition.TM)
                        {
                            EType eType = ETypeByDeviceType(deviceType);

                            //тиристорный тип
                            if (eType == EType.Thyristor)
                            {
                                result = new List<string>();
                                result.Add("BVT_VD");
                                result.Add("BVT_VR");
                            }

                            //диодный тип
                            if (eType == EType.Diode)
                            {
                                result = new List<string>();
                                result.Add("BVT_VR");
                            }
                        }
                        break;

                    case TestParametersType.Gate:
                    case TestParametersType.Commutation:
                        result = new List<string>();
                        break;

                    case TestParametersType.Clamping:
                        result = new List<string>();
                        //result.Add("CLAMP_Temperature");
                        break;

                    case TestParametersType.Dvdt:
                        result = new List<string>();
                        result.Add("DVDT_VoltageRate");
                        break;

                    case TestParametersType.ATU:
                        result = new List<string>();
                        break;

                    case TestParametersType.RAC:
                    case TestParametersType.IH:
                    case TestParametersType.RCC:
                    case TestParametersType.Sctu:
                    case TestParametersType.QrrTq:
                        result = new List<string>();
                        break;
                }
            }

            return result;
        }

        private List<string> MeasuredParametersByDeviceType(TestParametersType testType)
        {
            //возвращает список имён измеряемых параметров, которые надо показывать пользователю
            //если данная реализация возвращает null - значит никаких ограничений на имена измеряемых параметров для отображения пользователю нет - их надо показывать все
            List<string> result = null;

            switch (testType)
            {
                case TestParametersType.ATU:
                    result = new List<string>();
                    result.Add("PRSM");
                    break;

                case TestParametersType.Commutation:
                case TestParametersType.Clamping:
                    break;

                case TestParametersType.Gate:
                    result = new List<string>();
                    result.Add("RG");
                    result.Add("IL");
                    result.Add("IGT");
                    result.Add("VGT");
                    result.Add("IH");
                    break;

                case TestParametersType.StaticLoses:
                    result = new List<string>();
                    result.Add("VTM");
                    break;

                case TestParametersType.Bvt:
                    result = new List<string>();
                    result.Add("IDRM");
                    result.Add("IRRM");
                    result.Add("VDRM");
                    result.Add("VRRM");
                    result.Add("IDSM");
                    result.Add("IRSM");
                    result.Add("VDSM");
                    result.Add("VRSM");
                    break;

                case TestParametersType.Dvdt:
                case TestParametersType.RAC:
                case TestParametersType.IH:
                case TestParametersType.RCC:
                case TestParametersType.Sctu:
                    break;

                case TestParametersType.QrrTq:
                    result = new List<string>();
                    break;
            }

            return result;
        }

        private TemperatureCondition TemperatureConditionFromXML(XmlDocument xmlConditionDoc, out string value)
        {
            //извлечение значения условия с именем 'CLAMP_Temperature' теста 'Clamping' из XML описания условий xmlConditionDoc
            TemperatureCondition result = TemperatureCondition.None;

            XmlNode node = xmlConditionDoc.SelectSingleNode("//T[@Test='Clamping' and @Name='CLAMP_Temperature']");

            if (node == null)
            {
                //если профиль не содержит описания температуры - считаем, что измерения проводятся при комнатной температуре
                value = "RT";
                result = TemperatureCondition.RT;
            }
            else
            {
                string sTemperature = node.Attributes["Value"].Value;
                double dTemperature;
                value = string.Empty;

                if (double.TryParse(sTemperature, out dTemperature))
                {
                    value = string.Format("{0} °C", dTemperature.ToString());
                    result = (dTemperature > 25) ? TemperatureCondition.TM : TemperatureCondition.RT;
                }
            }

            return result;
        }

        private int GetColumnIndexInDataTable(string nameInDataTable, string nameInDataGrid, Type newColumnDataType, Type factColumnDataType, DataTable columnsDT, bool columnVisual)
        {
            int columnIndex = ColumnInDataTableByName(this.dtData, nameInDataTable);

            //столбец с именем name отсутствует в this.dtData - его надо создать
            if (columnIndex == -1)
            {
                //смотрим какому температурному режиму принадлежит создаваемый столбец
                TemperatureCondition tc = ProfileRoutines.TemperatureConditionByProfileName(nameInDataTable);

                //смотрим с каким температурным режимом 1 мы имеем дело (может быть как RT, так и TM в зависимости от того какой встретится первым при загрузке данных)
                TemperatureCondition temperatureCondition1 = this.TemperatureCondition1(tc);

                //создаём столбец в this.dtData всегда с типом columnDataType. создать сразу столбец с нужным типом можно только для случая чтения значений измеренных параметров, для случая conditions так делать нельзя т.к. нам не известны типы всех значений (они объявлены как string, но хранится в них могут как строки так и числа с плавающей запятой)
                NewColumnInDataTable(this.dtData, tc, temperatureCondition1, nameInDataTable, newColumnDataType, false, true, false);

                //создаём столбец в columnsDT с фактическим типом factColumnDataType. индекс созданного столбца в this.dtData и columnsDT одинаковый
                columnIndex = NewColumnInDataTable(columnsDT, tc, temperatureCondition1, nameInDataTable, factColumnDataType, false, true, false);

                //создаём столбец в DataGrid
                if (columnVisual)
                    NewColumnInDataGrid(tc, temperatureCondition1, nameInDataTable, nameInDataGrid);
            }

            //запоминаем/проверяем вычисленный тип значения value в columnsDT
            StoreType(columnsDT, columnIndex, factColumnDataType);

            return columnIndex;
        }

        private void ProcessingXmlData(DataTable columnsDT, XmlElement documentElement, XMLValues subject, TemperatureCondition temperatureCondition, string temperatureValue, string deviceType, DataRow dataRow)
        {
            //анализирует принятый текст xmlValue (который точно является XML текстом)
            //создаём столбцы в this и this.dtData из принятого описания в XML формате
            foreach (XmlNode child in documentElement.ChildNodes)
            {
                XmlAttributeCollection attributes = child.Attributes;

                if (attributes != null)
                {
                    string test = attributes["Test"].Value;

                    if (test == "SL")
                        test = "StaticLoses";

                    TestParametersType testType;

                    if (Enum.TryParse(test, true, out testType))
                    {
                        string name = attributes["Name"].Value;
                        string value = null;

                        Type factColumnDataType = null;
                        Type newColumnDataType = null;
                        double dValue;

                        bool need = false;
                        string columnNameInDataGrid = string.Empty;
                        string columnNameInDataTable = string.Empty;

                        string unitMeasure = null;

                        string nrmMin = null;
                        double? dnrmMin = null;

                        string nrmMax = null;
                        double? dnrmMax = null;

                        switch (subject)
                        {
                            case (XMLValues.Conditions):
                                //строим список условий, которые надо показать
                                List<string> conditions = ConditionNamesByDeviceType(testType, deviceType, temperatureCondition);

                                //если conditions=null - не показываем данное условие. если же conditions не null, то в нём должно присутствовать условие с именем name
                                need = ((conditions != null) && (conditions.IndexOf(name) != -1));

                                if (need)
                                {
                                    //описание условий хранятся как строки, в них может быть всё что угодно                                    
                                    value = attributes["Value"].Value;

                                    newColumnDataType = typeof(string);
                                    factColumnDataType = TypeOfValue(value, out value);

                                    //строим имя условия для отображения в DataGrid
                                    columnNameInDataGrid = string.Format("{0}{1}{2}", temperatureCondition.ToString(), cNewStringDelimeter, Dictionaries.ConditionName(temperatureCondition, name));

                                    //строим имя условия для использования в DataTable
                                    columnNameInDataTable = string.Format("{0}{1}", temperatureCondition.ToString(), Dictionaries.ConditionName(temperatureCondition, name));
                                }
                                break;

                            case (XMLValues.Parameters):
                                //строим список измеренных параметров, которые надо показать
                                List<string> measuredParameters = MeasuredParametersByDeviceType(testType);

                                //проверяем вхождение имени текущего измеренного параметра в список измеренных параметров, которые требуется показать для текущего типа теста. если measuredParameters=null - значит надо показать их все
                                need = ((measuredParameters == null) || (measuredParameters.IndexOf(name) != -1));

                                if (need)
                                {
                                    value = attributes["Value"].Value;

                                    //значения измеренных параметров - всегда числа с плавающей запятой. если это не так - ругаемся
                                    if (!ValueAsDouble(value, out dValue))
                                        throw new Exception(string.Format("При чтении значения измеренного параметра '{0}' из XML описания оказалось, что его значение '{1}' не преобразуется к типу Double.", name, value));

                                    value = dValue.ToString();
                                    newColumnDataType = typeof(double);
                                    factColumnDataType = newColumnDataType;

                                    //строим имя параметра для отображения в DataGrid
                                    columnNameInDataGrid = string.Format("{0}{1}{2}", temperatureCondition.ToString(), cNewStringDelimeter, Dictionaries.ParameterName(name));

                                    //строим имя параметра для использования в DataTable
                                    columnNameInDataTable = string.Format("{0}{1}", temperatureCondition.ToString(), Dictionaries.ParameterName(name));

                                    //считываем единицу измерения
                                    unitMeasure = attributes["Um"].Value;

                                    //считываем значения норм
                                    double d;

                                    XmlAttribute attribute = attributes["NrmMin"];
                                    if (attribute != null)
                                    {
                                        nrmMin = attribute.Value;
                                        //значения норм - всегда числа с плавающей запятой. если это не так - ругаемся
                                        if (!ValueAsDouble(nrmMin, out d))
                                            throw new Exception(string.Format("При чтении значения нормы (min) измеренного параметра '{0}' из XML описания оказалось, что оно '{1}' не преобразуется к типу Double.", name, nrmMin));

                                        dnrmMin = d;
                                    }

                                    attribute = attributes["NrmMax"];
                                    if (attribute != null)
                                    {
                                        nrmMax = attribute.Value;
                                        //значения норм - всегда числа с плавающей запятой. если это не так - ругаемся
                                        if (!ValueAsDouble(nrmMax, out d))
                                            throw new Exception(string.Format("При чтении значения нормы (max) измеренного параметра '{0}' из XML описания оказалось, что оно '{1}' не преобразуется к типу Double.", name, nrmMax));

                                        dnrmMax = d;
                                    }
                                }
                                break;

                            default:
                                throw new Exception(string.Format("Для принятого значения subject={0} обработка не предусмотрена.", subject.ToString()));
                        }

                        if (need)
                        {
                            //запоминаем значение condition/parameter
                            int columnIndex = GetColumnIndexInDataTable(columnNameInDataTable, columnNameInDataGrid, newColumnDataType, factColumnDataType, columnsDT, true);
                            dataRow[columnIndex] = value;

                            //запоминаем значение температуры                           
                            name = Routines.NameOfHiddenColumn(dataRow.Table.Columns[columnIndex].ColumnName);
                            int temperatureValuecolumnIndex = GetColumnIndexInDataTable(name, null, typeof(string), typeof(string), columnsDT, false);
                            dataRow[temperatureValuecolumnIndex] = temperatureValue;

                            //запоминаем значение единицы измерения
                            name = Routines.NameOfUnitMeasure(dataRow.Table.Columns[columnIndex].ColumnName);
                            int unitMeasureIndex = GetColumnIndexInDataTable(name, null, typeof(string), typeof(string), columnsDT, false);
                            dataRow[unitMeasureIndex] = unitMeasure;

                            //запоминаем значения норм min и max
                            if (dnrmMin != null)
                            {
                                name = Routines.NameOfNrmMinParametersColumn(dataRow.Table.Columns[columnIndex].ColumnName);
                                int nrmMinColumnIndex = GetColumnIndexInDataTable(name, null, typeof(double), typeof(double), columnsDT, false);
                                dataRow[nrmMinColumnIndex] = (double)dnrmMin;
                            }

                            if (dnrmMax != null)
                            {
                                name = Routines.NameOfNrmMaxParametersColumn(dataRow.Table.Columns[columnIndex].ColumnName);
                                int nrmMaxColumnIndex = GetColumnIndexInDataTable(name, null, typeof(double), typeof(double), columnsDT, false);
                                dataRow[nrmMaxColumnIndex] = (double)dnrmMax;
                            }
                        }
                    }
                }
            }
        }

        private string ValueIsXml(object value)
        {
            //отвечает на вопрос является ли принятый value Xml текстом
            //возвращает:
            //            null - принятый value не Xml текст
            //            not null - принятый value есть Xml текст
            string result = null;

            if (value != null)
            {
                string sValue = value.ToString();

                if (sValue != string.Empty)
                {
                    if (sValue.TrimStart().StartsWith("<"))
                        result = sValue;
                }
            }

            return result;
        }

        private void ProcessingNotXmlPairData(int index, DataRow destRow, object pairValue, DataTable columnsDT)
        {
            //запоминание pairValue в записи destRow по индексу index
            //для этого создаём не видимый в this столбец, чтение сохранённых в него данных будет реализовано через механизм hint's. тип создаваемого столбца - строка ибо для hint другого и не надо
            string columnName = destRow.Table.Columns[index].ColumnName;
            string pairColumnName = Routines.NameOfHiddenColumn(columnName);
            int columnIndex = GetColumnIndexInDataTable(pairColumnName, null, typeof(string), typeof(string), columnsDT, false);

            object minDeviceClass;

            switch (columnName)
            {
                case Constants.DeviceClass:
                    //вычисляем минимальное значение класса из двух возможных
                    int? deviceClass1 = (destRow[index] == DBNull.Value) ? null : destRow[index] as int?;
                    int? deviceClass2 = (pairValue == DBNull.Value) ? null : pairValue as int?;

                    minDeviceClass = Routines.Min(deviceClass1, deviceClass2);
                    minDeviceClass = minDeviceClass ?? DBNull.Value;

                    //пишем вычисленное значение minDeviceClass в destRow, а в HiddenColumn пишем исходные данные для вычисления этого минимума
                    destRow[index] = minDeviceClass;
                    destRow[columnIndex] = string.Format("min({0}, {1})", deviceClass1.ToString(), deviceClass2.ToString());
                    break;

                case Constants.CodeOfNonMatch:
                case Constants.Reason:
                    //объединяем значения реквизитов в одну строку
                    object value = destRow[index];

                    if ((value != DBNull.Value) && (pairValue != DBNull.Value))
                    {
                        destRow[index] = string.Concat(destRow[index], "; ");

                        //чтобы иметь возможность понять откуда получены отображаемые коды НП пишем исходные данные 
                        value = string.Concat(value.ToString(), " + ");
                    }

                    destRow[index] = string.Concat(destRow[index].ToString(), pairValue.ToString());
                    destRow[columnIndex] = string.Concat(value.ToString(), pairValue.ToString());
                    break;

                case Constants.Status:
                    //статус вычисляется по холодному и горячему измерениям
                    //если хотя-бы одно измерение отсутствует - возвращаем неопределённый (пустое значение) статус, т.е. если пара RT-TM не образовалась - статус имеет не определённое значение
                    //значение статуса "OK" выводим только если для холодного и горячего измерений имеем статусы "OK". если хотя-бы один из статусов не "OK" - выводим "Fault"
                    string status = destRow[index].ToString();
                    string pairStatus = pairValue.ToString();

                    //если хотя-бы одно измерение завершилось не успешно - возвратим статус "Fault"
                    string resultStatus = ((status == "Fault") || (pairStatus == "Fault")) ? "Fault" : string.Empty;

                    if ((resultStatus == string.Empty) && ((status != string.Empty) && (pairStatus != string.Empty)))
                    {
                        //только если оба измерения завершились успешно - возвратим статус "OK"
                        if ((status == "OK") && (pairStatus == "OK"))
                            resultStatus = "OK";
                    }

                    //пишем итоговый статус - пользователь будет его видеть в DataGrid
                    destRow[index] = resultStatus;

                    //запоминаем исходные данные, которые мы использовали при вычислении итогового статуса
                    destRow[columnIndex] = string.Format("ƒ({0}, {1})", status, pairStatus);
                    break;

                default:
                    destRow[columnIndex] = pairValue;
                    break;
            }
        }

        private void FillValuesToDataRow(DataTable columnsDT, DataRow dataRow, object[] values, bool isPair)
        {
            //isPair: значение true говорит о заливке в принятый dataRow данных пары. т.е. после заливки данных dataRow будет содержать результат объединения двух строк: данные из принятого dataRow объединятся с данными из принятого values
            //извлекаем тип изделия из текущей строки, представленной множеством полей values
            XmlDocument xmlDoc = new XmlDocument();
            string deviceType = this.GetDeviceTypeHandler(values);

            TemperatureCondition temperatureCondition = TemperatureCondition.None;
            string temperatureValue = string.Empty;

            for (int i = 0; i < values.Count(); i++)
            {
                object value = values[i];

                //проверяем представлено ли текущее значение поля в формате XML
                string sXML = ValueIsXml(value);

                switch (sXML == null)
                {
                    case true:
                        //обычное (не XML) значение
                        if (isPair)
                        {
                            //сохраняем реквизит пары value в dataRow
                            this.ProcessingNotXmlPairData(i, dataRow, value, columnsDT);
                        }
                        else
                            dataRow[i] = value;

                        break;

                    default:
                        //имеем дело с описанием множества значений либо условий измерений, либо значений параметров изделия в формате XML
                        xmlDoc.LoadXml(sXML);
                        XmlElement documentElement = xmlDoc.DocumentElement;

                        XMLValues subject = (documentElement.Name == "CONDITIONS") ? XMLValues.Conditions : (documentElement.Name == "PARAMETERS") ? XMLValues.Parameters : XMLValues.UnAssigned;

                        //считываем при какой температуре проводятся измерения. эта информация есть только в описании условий измерения                        
                        if (subject == XMLValues.Conditions)
                            temperatureCondition = TemperatureConditionFromXML(xmlDoc, out temperatureValue);

                        ProcessingXmlData(columnsDT, documentElement, subject, temperatureCondition, temperatureValue, deviceType, dataRow);
                        break;
                }
            }
        }

        private static String WildCardToRegular(string value)
        {
            return "^" + System.Text.RegularExpressions.Regex.Escape(value).Replace("\\*", ".*") + "$";
        }

        private delegate void delegateFillValues(DataTable columnsDT, object[] values, DataTable dtData);

        private void FillValues(DataTable columnsDT, object[] values, DataTable dtData)
        {
            //заливка данных values в dtData          
            string profileName = this.GetProfileNameHandler(values);
            string groupName = this.GetGroupNameHandler(values);
            string code = this.GetCodeHandler(values);

            string profileBody = string.Empty;
            string pairProfileBody = ProfileRoutines.PairProfileBodyByProfileName(profileName, out profileBody);

            //вычисляем запись куда надо сохранить принятые данные values
            DataRow pairRow = this.cache.Pop(code, groupName, pairProfileBody);

            switch (pairRow == null)
            {
                case (false):
                    //запись найдена - дописываем в неё всё из принятого values
                    pairRow.BeginEdit();
                    FillValuesToDataRow(columnsDT, pairRow, values, true);
                    pairRow.EndEdit();
                    break;

                default:
                    //запись не найдена - создаём новую
                    DataRow dataRow = dtData.NewRow();

                    dataRow.BeginEdit();
                    FillValuesToDataRow(columnsDT, dataRow, values, false); //dataRow.ItemArray = values;
                    dataRow.EndEdit();

                    dtData.Rows.Add(dataRow);

                    //запоминаем описание обработанной dataRow в быстром cache со своим собственным profileBody
                    try
                    {
                        this.cache.Push(code, groupName, profileBody, dataRow);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("{0}. code={1}, groupName={2}, profileBody={3}", string.Concat("При выполнении реализации this.cache.Push перехвачена исключительная ситуация. ", ex.ToString()), code, groupName, profileBody));
                    }
                    break;
            }
        }

        private delegate void delegateSetItemsSource(DataTable table);
        private void SetItemsSource(DataTable table)
        {
            if (table == null)
            {
                this.ItemsSource = null;
            }
            else
            {
                this.ItemsSource = table.AsDataView();

                //применяем набор фильтров, которые имели место до данной загрузки данных
                this.SetRowFilter();
            }
        }

        private Type DataTypeByColumn(DataGridTextColumn column, out string bindPath)
        {
            //возвращает тип данных, который отображается в столбце Column
            Type type = null;
            bindPath = null;

            if (column != null)
            {
                Binding b = (Binding)column.Binding;

                if (b != null)
                {
                    bindPath = b.Path.Path;
                    //type = dtData?.Columns[bindPath]?.DataType;

                    DataView dv = this.ItemsSource as DataView;
                    type = dv?.Table.Columns[bindPath]?.DataType;
                }
            }

            return type;
        }

        public string ColumnName(DataGridTextColumn column)
        {
            string result = null;

            if (column != null)
            {
                Binding b = (Binding)column.Binding;

                if (b != null)
                    result = b.Path.Path;
            }

            return result;
        }

        private void columnHeader_Click(object sender, RoutedEventArgs e)
        {
            var columnHeader = sender as System.Windows.Controls.Primitives.DataGridColumnHeader;

            if (btFilterClicked == null)
            {
                //вычисляем поле сортировки и его направление
                if (columnHeader != null)
                {
                    if (columnHeader == lastHeaderClicked)
                    {
                        if (lastSortedDirection == ListSortDirection.Ascending)
                        {
                            lastSortedDirection = ListSortDirection.Descending;
                        }
                        else lastSortedDirection = ListSortDirection.Ascending;
                    }
                    else lastSortedDirection = ListSortDirection.Ascending;

                    Path founded = null;

                    if (lastHeaderClicked != null)
                    {
                        founded = FindChild<Path>(lastHeaderClicked, "PathArrowUp");
                        if (founded != null)
                            founded.Visibility = Visibility.Collapsed;

                        founded = FindChild<Path>(lastHeaderClicked, "PathArrowDown");
                        if (founded != null)
                            founded.Visibility = Visibility.Collapsed;
                    }

                    lastHeaderClicked = columnHeader;

                    switch (lastSortedDirection)
                    {
                        case (ListSortDirection.Ascending):
                            founded = FindChild<Path>(lastHeaderClicked, "PathArrowUp");
                            break;

                        case (ListSortDirection.Descending):
                            founded = FindChild<Path>(lastHeaderClicked, "PathArrowDown");
                            break;

                        default:
                            founded = null;
                            break;
                    }

                    if (founded != null)
                        founded.Visibility = Visibility.Visible;
                }
            }
            else
            {
                //обработка нажатия кнопки фильтра
                Point position = btFilterClicked.PointToScreen(new Point(0d, 0d));
                position.Y += columnHeader.Height;

                //сбрасываем флаг о прошедшем нажатии кнопки фильтра
                btFilterClicked = null;

                DataGridTextColumn textColumn = (DataGridTextColumn)columnHeader.Column;

                string bindPath;
                Type filterType = DataTypeByColumn(textColumn, out bindPath);
                SetFilter(position, filterType, columnHeader.Content.ToString(), bindPath);
            }
        }

        void SortHandler(object sender, DataGridSortingEventArgs e)
        {
            DataGridColumn column = e.Column;

            switch (column.SortMemberPath)
            {
                case "CODE":
                case "GROUP_NAME":
                case "ITEM":
                    {
                        ListSortDirection sortDirection = (column.SortDirection == ListSortDirection.Ascending) ? ListSortDirection.Descending : ListSortDirection.Ascending;
                        SetItemsSource(null);

                        try
                        {
                            CustomComparer<object> customComparer = new CustomComparer<object>(sortDirection);
                            this.dtData = this.dtData.AsEnumerable().OrderBy(x => x.Field<object>(column.SortMemberPath), customComparer).CopyToDataTable();
                        }
                        finally
                        {
                            this.SetItemsSource(this.dtData);
                        }

                        column.SortDirection = sortDirection;
                        e.Handled = true;
                        break;
                    }
            }
        }

        private DataRowView SelectedRow()
        {
            DataRowView result = null;

            System.Collections.IList rows = this.SelectedItems;

            //различаем ситуации: выбрана строка и выбрана ячейка
            if (rows.Count > 0)
            {
                result = (DataRowView)rows[0];
            }
            else
            {
                IList<DataGridCellInfo> selCells = this.SelectedCells;

                if (selCells != null)
                {
                    if (selCells.Count != 0)
                    {
                        DataGridCellInfo cellInfo = this.SelectedCells[0];
                        FrameworkElement cellContent = cellInfo.Column.GetCellContent(cellInfo.Item);

                        result = (DataRowView)cellContent.DataContext;
                    }
                }
            }

            return result;
        }

        public object ValueFromSelectedRow(string bindPath) //SelectedText
        {
            //возвращает текст из выбранной пользователем в DataGrid строке
            DataRowView row = this.SelectedRow();
            object result = row?[bindPath];            

            return result;
        }

        private string FiltersToString()
        {
            string result = string.Empty;

            this.activeFilters.Correct();
            int index = 0;

            while ((this.activeFilters.Count > 0) && (index < this.activeFilters.Count))
            {
                FilterDescription f = this.activeFilters[index];

                if (result != string.Empty)
                    result += " AND ";

                //если f.value=null - тип значения нам не важен
                if (f.value == null)
                    result += string.Format("{0}{1}{2}", f.fieldName, f.comparisonCorrected, f.valueCorrected);
                else
                {
                    if (f.type == typeof(string))
                        result += string.Format("{0}{1}'{2}'", f.fieldName, f.comparisonCorrected, f.valueCorrected);

                    if (f.type == typeof(DateTime))
                    {
                        DateTime value;

                        //извлекаем значение даты из DataTime (значение времени отбрасываем). возможности обработки значения поля 'TS' каждой строки ограничены возможностями DataColumn.Expression. ничего лучше, чем рассмотреть DataTime как строку и вырезать из неё первые 10 символов на данный момент не придумал
                        if (DateTime.TryParse(f.value.ToString(), out value))
                            result += string.Format("Substring(Convert({0}, 'System.String'), 1, 10){1}'{2}'", f.fieldName, f.comparison, value.ToShortDateString());
                        else
                            this.activeFilters.Remove(f);
                    }

                    if (f.type == typeof(int))
                    {
                        int value;

                        if (int.TryParse(f.value.ToString(), out value))
                            result += string.Format("{0}{1}{2}", f.fieldName, f.comparison, value.ToString());
                        else
                            this.activeFilters.Remove(f);
                    }

                    if (f.type == typeof(double))
                    {
                        double value;

                        if (Routines.TryStringToDouble(f.value.ToString(), out value))
                            result += string.Format("{0}{1}{2}", f.fieldName, f.comparison, value.ToString().Replace(',', '.'));
                        else
                            this.activeFilters.Remove(f);
                    }
                }

                index++;
            }

            return result;
        }

        private void SetFilter(Point position, Type type, string tittlefieldName, string bindPath)
        {
            if (this.ItemsSource != null)
            {
                var dv = this.ItemsSource as DataView;

                if (dv != null)
                {
                    FilterDescription filter = new FilterDescription { type = type, tittlefieldName = tittlefieldName, fieldName = bindPath, comparison = "=", value = this.ValueFromSelectedRow(bindPath) };
                    this.activeFilters.Add(filter);

                    FiltersInput fmFiltersInput = new FiltersInput(this.activeFilters);                    

                    if (fmFiltersInput.Demonstrate(position) == true)
                        this.SetRowFilter();

                    if (this.Items.Count > 0)
                        this.SelectedIndex = 0;

                    if (this.RefreshBottomRecordCountHandler != null)
                        this.RefreshBottomRecordCountHandler();
                }
            }
        }

        /// <summary>
        /// Finds a Child of a given item in the visual tree. 
        /// </summary>
        /// <param name="parent">A direct parent of the queried item.</param>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="childName">x:Name or Name of child. </param>
        /// <returns>The first parent item that matches the submitted type parameter. 
        /// If not matching item can be found, 
        /// a null parent is being returned.</returns>
        public T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null)
                return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null)
                        break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        public CheckNrmStatus IsInNrm(DataRow row, string columnName)
        {
            //возвращает:
            //CheckNrmStatus.UnCheckable - проверка норм не имеет смысла
            //CheckNrmStatus.Good - значение в пределах нормы
            //CheckNrmStatus.Defective - значение вне нормы
            //CheckNrmStatus.NotSetted - нормы не установлены

            CheckNrmStatus result = CheckNrmStatus.UnCheckable;

            if (row != null)
            {
                //считываем проверяемое значение              
                int columnIndex = row.Table.Columns.IndexOf(columnName);

                if (columnIndex != -1)
                {
                    //нормы могут иметь столбцы с индексами начиная с this.FirstCPColumnIndexInDataTable1 и до упора
                    if (columnIndex >= this.FirstCPColumnIndexInDataTable1)
                    {
                        var value = row[columnIndex];

                        if (value != DBNull.Value)
                        {
                            //считываем значения нормы min
                            string nameOfNrmMinParametersColumn = Routines.NameOfNrmMinParametersColumn(columnName);
                            int columnNrmMinIndex = row.Table.Columns.IndexOf(nameOfNrmMinParametersColumn);

                            if (columnNrmMinIndex == -1)
                            {
                                result = CheckNrmStatus.NotSetted;
                            }
                            else
                            {
                                var nrmMin = row[columnNrmMinIndex];

                                if (nrmMin == DBNull.Value)
                                {
                                    result = CheckNrmStatus.NotSetted;
                                }
                                else
                                {
                                    double dValue = double.Parse(value.ToString());
                                    double dNrmMin = double.Parse(nrmMin.ToString());

                                    result = (dNrmMin < dValue) ? CheckNrmStatus.Good : CheckNrmStatus.Defective;

                                    if (result == CheckNrmStatus.Defective)
                                        return result;
                                }
                            }

                            //считываем значения нормы max
                            string nameOfNrmMaxParametersColumn = Routines.NameOfNrmMaxParametersColumn(columnName);
                            int columnNrmMaxIndex = row.Table.Columns.IndexOf(nameOfNrmMaxParametersColumn);

                            if (columnNrmMaxIndex == -1)
                            {
                                if (result == CheckNrmStatus.NotSetted)
                                    return CheckNrmStatus.NotSetted;
                            }
                            else
                            {
                                var nrmMax = row[columnNrmMaxIndex];

                                if (nrmMax == DBNull.Value)
                                {
                                    if (result == CheckNrmStatus.NotSetted)
                                        return CheckNrmStatus.NotSetted;
                                }
                                else
                                {
                                    double dValue = double.Parse(value.ToString());
                                    double dNrmMax = double.Parse(nrmMax.ToString());

                                    result = (dValue <= dNrmMax) ? CheckNrmStatus.Good : CheckNrmStatus.Defective;

                                    if (result == CheckNrmStatus.Defective)
                                        return result;
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }
    }

    public class FilterDescription
    {
        public Type type;
        public string tittlefieldName { get; set; }
        public string fieldName;
        public string comparison { get; set; }
        public string comparisonCorrected;
        public object value { get; set; }
        public string valueCorrected;

        public void Correct()
        {
            //корректировка описания фильтра
            if (this.value == null)
            {
                this.comparison = "=";
                this.comparisonCorrected = " IS ";
                this.valueCorrected = "NULL";
            }
            else
            {
                string newValue = this.value.ToString().Replace("*", "%");

                if (this.type == typeof(string))
                {
                    switch (newValue == this.value.ToString())
                    {
                        case true:
                            this.comparison = "=";
                            this.comparisonCorrected = "=";
                            this.valueCorrected = this.value.ToString();
                            break;

                        default:
                            this.comparisonCorrected = " LIKE ";
                            this.valueCorrected = newValue;
                            break;
                    }
                }
            }
        }
    }

    public class ActiveFilters : ObservableCollection<FilterDescription>
    {
        public bool FieldNameStoredMoreThanOnce(string fieldName)
        {
            //вычисляет сколько раз в this сохранено значений фильтров с принятым fieldName
            var linqResults = this.Where(fn => fn.fieldName == fieldName);

            return (linqResults.Count() > 1);
        }

        public void Correct()
        {
            for (int i = this.Count - 1; i >= 0; i--)
            {
                FilterDescription f = this.Items[i];

                string sValue = f.value.ToString();
                sValue = Regex.Replace(sValue, @"\s+", "");

                if (sValue == string.Empty)
                    f.value = null;

                f.Correct();
            }
        }
    }

    public class NrmToBrushMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            CheckNrmStatus inNrm = CheckNrmStatus.UnCheckable;

            DataGridCell cell = values[0] as DataGridCell;
            DataRow row = values[1] as DataRow;
            DataGridSqlResult dataGrid = values[2] as DataGridSqlResult;

            if (dataGrid != null)
            {
                if ((cell != null) && (row != null))
                {
                    DataGridTextColumn column = cell.Column as DataGridTextColumn;

                    if (column != null)
                    {
                        string columnName = dataGrid.ColumnName(column);
                        inNrm = dataGrid.IsInNrm(row, columnName);
                    }
                }

                switch (inNrm)
                {
                    case CheckNrmStatus.UnCheckable:
                        //проверять нечего
                        return DependencyProperty.UnsetValue;

                    case (CheckNrmStatus.Good):
                        //в норме
                        return dataGrid.FindResource("ValueInNrm");

                    case (CheckNrmStatus.Defective):
                        //за пределами нормы
                        return dataGrid.FindResource("ValueOutSideTheNorm");

                    case (CheckNrmStatus.NotSetted):
                        //норма не установлена
                        return dataGrid.FindResource("NrmNotSetted");

                    default:
                        return DependencyProperty.UnsetValue;
                }
            }

            return DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class ToolTipMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            const string noData = "Нет данных";

            DataGridCell cell = values[0] as DataGridCell;
            DataRow row = values[1] as DataRow;
            DataGridSqlResult dataGrid = values[2] as DataGridSqlResult;

            if ((cell != null) && (row != null) && (dataGrid != null))
            {
                DataGridTextColumn column = cell.Column as DataGridTextColumn;

                if (column != null)
                {
                    string columnName = dataGrid.ColumnName(column);

                    //по полученному имени столбца вычисляем имя скрытого столбца в row.Table
                    if (columnName != null)
                    {
                        //получаем имя столбца в row.Table, который хранит скрытые данные
                        columnName = Routines.NameOfHiddenColumn(columnName);

                        //получаем индекс столбца в таблице которая хранит скрытые данные, являющиеся результатом группировки
                        int columnIndex = row.Table.Columns.IndexOf(columnName);

                        if (columnIndex != -1)
                        {
                            var value = row[columnIndex];

                            return (value == DBNull.Value) ? noData : value;
                        }
                    }
                }
            }

            return noData;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class HeaderVisibilityMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //не будем показывать пользователю Header если количество записей отображаемых в DataGridSqlResult равно нулю и это не вызвано специфически установленными значениями фильтра
            //но если пользователь так настроил свои фильтры что это привело к пустому множеству в отображаемых данных - показывать header необходимо ибо пользователю при этом обязятельно должны быть доступны средства управления значениями фильтров
            int count = (int)values[0];
            DataGridSqlResult dg = values[1] as DataGridSqlResult;

            if (dg != null)
            {
                var dv = dg.ItemsSource as DataView;

                return ((dg.Items.Count == 0) && ((dv == null) || (dv.RowFilter == null) || (dv.RowFilter == string.Empty))) ? Visibility.Hidden : Visibility.Visible;
            }

            return Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public enum CheckNrmStatus
    {
        [EnumMember]
        UnCheckable = 0,

        [EnumMember]
        Good = 1,

        [EnumMember]
        Defective = 2,

        [EnumMember]
        NotSetted = 3
    }

    [Flags]
    public enum XMLValues
    {
        //не определённое значение описания в формате XML
        UnAssigned = 0x00,

        //описание в формате XML условий измерений
        Conditions = 0x01,

        //описание в формате XML измеренных значений параметров
        Parameters = 0x02
    }

    public static class Dictionaries
    {
        private static readonly Dictionary<string, string> RTConditionsNames;
        private static readonly Dictionary<string, string> TMConditionsNames;
        private static readonly Dictionary<string, string> ConditionsUnitMeasure;
        private static readonly Dictionary<string, string> ParametersName;
        private static readonly Dictionary<string, string> ParametersFormat;

        static Dictionaries()
        {
            //имена условий зависят от температурного режима. здесь хранятся соответствия имён условий базы данных именам условий RT, которые хочет видеть пользователь приложения
            RTConditionsNames = new Dictionary<string, string>()
            {
                {"CLAMP_Temperature", "T"},

                {"SL_ITM", "ITM"},

                {"BVT_VD", "UDRM"},
                {"BVT_VR", "UBRmax"},

                {"DVDT_VoltageRate", "DVDt"},

                {"QrrTq_DCFallRate", "dIdt"}
            };

            //имена условий зависят от температурного режима. здесь хранятся соответствия имён условий базы данных именам условий TM, которые хочет видеть пользователь приложения
            TMConditionsNames = new Dictionary<string, string>()
            {
                {"CLAMP_Temperature", "T"},

                {"SL_ITM", "ITM"},

                {"BVT_VD", "UDRM"},
                {"BVT_VR", "URRM"},

                {"DVDT_VoltageRate", "DVDt"},

                {"QrrTq_DCFallRate", "dIdt"}
            };

            //здесь храним значения единиц измерения условий
            ConditionsUnitMeasure = new Dictionary<string, string>()
            {
                {"SL_ITM", "А"},

                {"BVT_I", "мА"},
                {"BVT_VD", "В"},
                {"BVT_VR", "В"},

                {"DVDT_VoltageRate", "В/мкс"},

                {"QrrTq_DCFallRate", "А/мкс"}
            };

            //имена параметров не зависят от температурного режима. здесь хранятся соответсвия имён измеряемых параметров базы данных именам измеряемых параметров, которые хочет видеть пользователь приложения
            ParametersName = new Dictionary<string, string>()
            {
                {"VDRM", "UBO"},
                {"VRRM", "UBR"}
            };

            //здесь храним форматы отображения измеряемых параметров
            ParametersFormat = new Dictionary<string, string>()
            {
                {"VTM", "0.00"},
                {"VFM", "0.00"}
            };
        }

        public static string ConditionName(TemperatureCondition temperatureCondition, string conditionName)
        {
            Dictionary<string, string> dictionary = (temperatureCondition == TemperatureCondition.None) ? null : (temperatureCondition == TemperatureCondition.RT) ? RTConditionsNames : TMConditionsNames;

            if (dictionary == null)
                return conditionName;

            switch (dictionary.ContainsKey(conditionName))
            {
                case true:
                    return dictionary[conditionName];

                default:
                    return conditionName;
            }
        }

        public static string ConditionUnitMeasure(string conditionName)
        {
            switch (ConditionsUnitMeasure.ContainsKey(conditionName))
            {
                case true:
                    return ConditionsUnitMeasure[conditionName];

                default:
                    return null;
            }
        }

        public static string ParameterName(string parameterName)
        {
            string result;

            switch (ParametersName.ContainsKey(parameterName))
            {
                case true:
                    result = ParametersName[parameterName];
                    break;

                default:
                    result = parameterName;
                    break;
            }

            //если первый символ параметра начинается на V - заменяем его на U 
            if ((result != null) && (result.Substring(0, 1) == "V"))
                result = 'U' + result.Remove(0, 1);

            return result;
        }

        public static string ParameterFormat(string parameterName)
        {
            string result;

            switch (ParametersFormat.ContainsKey(parameterName))
            {
                case true:
                    result = ParametersFormat[parameterName];
                    break;

                default:
                    result = null;
                    break;
            }

            return result;
        }
    }
}
