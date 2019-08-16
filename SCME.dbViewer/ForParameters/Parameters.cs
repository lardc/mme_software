using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCME.Types.BaseTestParams;
using Excel = Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Excel;
using System.Globalization;
using SCME.Types.Profiles;

namespace SCME.dbViewer.ForParameters
{
    public class DataTableParameters : System.Data.DataTable
    {
        public int DevID { get; set; }
        public string DevType { get; set; }
        public string ProfileID { get; set; }
        public string ProfileName { get; set; }
        public string ProfileBody { get; set; }
        public TemperatureCondition TemperatureCondition { get; set; }
        public DateTime TsZeroTime { get; set; }
        public string GroupName { get; set; }
        public string Item { get; set; }
        public string Code { get; set; }

        public string CodeSimple
        {
            get
            {
                string result = this.Code;
                int posDelimeter = result.IndexOf("/");

                if (posDelimeter != -1)
                    result = result.Remove(posDelimeter);

                return result;
            }
        }

        public string Constructive { get; set; }
        public int? AverageCurrent { get; set; }
        public int? DeviceClass { get; set; }
        public string Equipment { get; set; }
        public string User { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }

        public ReportData Data { get; set; }

        //здесь будем хранить индексы начального и последнего столбца условий измерения Condition
        private int FIndexOfFirstColumnBvtCondition;
        private int FIndexOfLastColumnBvtCondition;

        private int FIndexOfFirstColumnDvdtCondition;
        private int FIndexOfLastColumnDvdtCondition;

        private int FIndexOfFirstColumnSLCondition;
        private int FIndexOfLastColumnSLCondition;

        private int FIndexOfFirstColumnGateCondition;
        private int FIndexOfLastColumnGateCondition;

        //здесь будем хранить индексы начального и последнего столбца измеренных параметров
        private int FIndexOfFirstColumnBvtMeasuredParameters;
        private int FIndexOfLastColumnBvtMeasuredParameters;

        private int FIndexOfFirstColumnDvdtMeasuredParameters;
        private int FIndexOfLastColumnDvdtMeasuredParameters;

        private int FIndexOfFirstColumnSLMeasuredParameters;
        private int FIndexOfLastColumnSLMeasuredParameters;

        private int FIndexOfFirstColumnGateMeasuredParameters;
        private int FIndexOfLastColumnGateMeasuredParameters;

        private const int RowIndexOfUnitMeasure = 0;
        private const int RowIndexOfNorm = 1;
        private const int RowIndexOfValue = 2;

        public DataTableParameters()
        {
            //всегда содержит три записи: первая хранит единицы измерения параметров, вторая нормы, а третья значения
            this.Rows.Add(this.NewRow());
            this.Rows.Add(this.NewRow());
            this.Rows.Add(this.NewRow());

            //до момента загрузки условий индексы столбцов первого и последнего условия не определены
            this.FIndexOfFirstColumnBvtCondition = -1;
            this.FIndexOfLastColumnBvtCondition = -1;

            this.FIndexOfFirstColumnDvdtCondition = -1;
            this.FIndexOfLastColumnDvdtCondition = -1;

            this.FIndexOfFirstColumnSLCondition = -1;
            this.FIndexOfLastColumnSLCondition = -1;

            this.FIndexOfFirstColumnGateCondition = -1;
            this.FIndexOfLastColumnGateCondition = -1;

            //до момента загрузки измеренных параметров индексы столбцов первого и последнего измеренного параметра не определены
            this.FIndexOfFirstColumnBvtMeasuredParameters = -1;
            this.FIndexOfLastColumnBvtMeasuredParameters = -1;

            this.FIndexOfFirstColumnDvdtMeasuredParameters = -1;
            this.FIndexOfLastColumnDvdtMeasuredParameters = -1;

            this.FIndexOfFirstColumnSLMeasuredParameters = -1;
            this.FIndexOfLastColumnSLMeasuredParameters = -1;

            this.FIndexOfFirstColumnGateMeasuredParameters = -1;
            this.FIndexOfLastColumnGateMeasuredParameters = -1;
        }

        public DataTableParameters(int devID, string deviceType, TemperatureCondition temperatureCondition, string profileID, string profileName, DateTime tsZeroTime, string groupName, string item, string code, string constructive, int? averageCurrent, int? deviceClass, string equipment, string user, string status, string reason) : this()
        {
            this.SetRequisites(devID, deviceType, temperatureCondition, profileID, profileName, tsZeroTime, groupName, item, code, constructive, averageCurrent, deviceClass, equipment, user, status, reason);
        }

        private string TemperatureDelimeter
        {
            get
            {
                return "\n";
            }
        }

        public string ColumnsSignature
        {
            get
            {
                string result = string.Empty;

                foreach (DataColumn column in this.Columns)
                    result += column.ColumnName;

                return result;
            }
        }

        private int NewColumn(string name, int id)
        {
            //в принятом ID - идентификатор данной сущности (для condition - COND_ID, для params - PARAM_ID). размещаем его в ExtendedProperties
            //все столбцы должны иметь уникальные имена
            string unicName;

            if (this.Columns.IndexOf(name) == -1)
            {
                unicName = name;
            }
            else
            {
                switch (name.Contains("(") && name.Contains(")"))
                {
                    //имя повторяется более 2-х раз
                    case true:
                        //извлекаем его количество повторений
                        char ch = '(';
                        int indexBeg = name.IndexOf(ch);
                        ch = ')';
                        int indexEnd = name.IndexOf(ch);
                        string sNum = name.Substring(indexBeg + 1, indexEnd - indexBeg - 1);
                        int num = int.Parse(sNum) + 1;
                        unicName = name.Substring(indexBeg) + num.ToString() + ")";
                        break;

                    //первое добавление номера к имени - второй по счёту повторяющийся столбец
                    default:
                        unicName = name + "(2)";
                        break;
                }
            }

            DataColumn column = new DataColumn(unicName);
            column.ExtendedProperties.Add("id", id);
            this.Columns.Add(column);

            return this.Columns.IndexOf(column);
        }

        public int NewConditionColumn(TestParametersType testType, string name, int id)
        {
            //создание столбца для хранения условия (condition)
            int result = NewColumn(name, id);

            //запоминаем индексы первого и последнего столбца, которые хранят описание условий. эти индексы хранятся для каждого типа теста
            switch (testType)
            {
                case TestParametersType.Bvt:
                    {
                        if (this.FIndexOfFirstColumnBvtCondition == -1)
                            this.FIndexOfFirstColumnBvtCondition = result;

                        this.FIndexOfLastColumnBvtCondition = result;
                        break;
                    }

                case TestParametersType.Dvdt:
                    {

                        if (this.FIndexOfFirstColumnDvdtCondition == -1)
                            this.FIndexOfFirstColumnDvdtCondition = result;

                        this.FIndexOfLastColumnDvdtCondition = result;
                        break;
                    }

                case TestParametersType.StaticLoses:
                    {
                        if (this.FIndexOfFirstColumnSLCondition == -1)
                            this.FIndexOfFirstColumnSLCondition = result;

                        this.FIndexOfLastColumnSLCondition = result;
                        break;
                    }

                case TestParametersType.Gate:
                    {

                        if (this.FIndexOfFirstColumnGateCondition == -1)
                            this.FIndexOfFirstColumnGateCondition = result;

                        this.FIndexOfLastColumnGateCondition = result;
                        break;
                    }
            }

            return result;
        }

        public int GetIndexOfMeasuredParametersByTestType(TestParametersType testType, out int indexOfLastColumnMeasuredParameters)
        {
            int result = -1;

            switch (testType)
            {
                case TestParametersType.Bvt:
                    {
                        result = this.FIndexOfFirstColumnBvtMeasuredParameters;
                        indexOfLastColumnMeasuredParameters = this.FIndexOfLastColumnBvtMeasuredParameters;
                        break;
                    }

                case TestParametersType.Dvdt:
                    {
                        result = this.FIndexOfFirstColumnDvdtMeasuredParameters;
                        indexOfLastColumnMeasuredParameters = this.FIndexOfLastColumnDvdtMeasuredParameters;
                        break;
                    }

                case TestParametersType.StaticLoses:
                    {
                        result = this.FIndexOfFirstColumnSLMeasuredParameters;
                        indexOfLastColumnMeasuredParameters = this.FIndexOfLastColumnSLMeasuredParameters;
                        break;
                    }

                case TestParametersType.Gate:
                    {
                        result = this.FIndexOfFirstColumnGateMeasuredParameters;
                        indexOfLastColumnMeasuredParameters = this.FIndexOfLastColumnGateMeasuredParameters;
                        break;
                    }

                default:
                    throw new Exception(string.Format("Для типа теста {0} обработка не предусмотрена.", testType.ToString()));
            }

            return result;
        }

        public void SetIndexOfMeasuredParametersByTestType(TestParametersType testType, int indexOfFirstColumnMeasuredParameters, int indexOfLastColumnMeasuredParameters)
        {
            switch (testType)
            {
                case TestParametersType.Bvt:
                    {
                        this.FIndexOfFirstColumnBvtMeasuredParameters = indexOfFirstColumnMeasuredParameters;
                        this.FIndexOfLastColumnBvtMeasuredParameters = indexOfLastColumnMeasuredParameters;
                        break;
                    }

                case TestParametersType.Dvdt:
                    {
                        this.FIndexOfFirstColumnDvdtMeasuredParameters = indexOfFirstColumnMeasuredParameters;
                        this.FIndexOfLastColumnDvdtMeasuredParameters = indexOfLastColumnMeasuredParameters;
                        break;
                    }

                case TestParametersType.StaticLoses:
                    {
                        this.FIndexOfFirstColumnSLMeasuredParameters = indexOfFirstColumnMeasuredParameters;
                        this.FIndexOfLastColumnSLMeasuredParameters = indexOfLastColumnMeasuredParameters;
                        break;
                    }

                case TestParametersType.Gate:
                    {
                        this.FIndexOfFirstColumnGateMeasuredParameters = indexOfFirstColumnMeasuredParameters;
                        this.FIndexOfLastColumnGateMeasuredParameters = indexOfLastColumnMeasuredParameters;
                        break;
                    }

                default:
                    throw new Exception(string.Format("Для типа теста {0} обработка не предусмотрена.", testType.ToString()));
            }
        }

        public int NewMeasuredParameterColumn(TestParametersType testType, string name, int id)
        {
            //создание столбца для хранения описания измеренного параметра (params)
            //запоминаем индекс первого столбца, который хранит описание самого первого измеренного параметра
            int indexOfLastColumnMeasuredParameters;
            int indexOfFirstColumnMeasuredParameters = this.GetIndexOfMeasuredParametersByTestType(testType, out indexOfLastColumnMeasuredParameters);

            int result = NewColumn(name, id);

            if (indexOfFirstColumnMeasuredParameters == -1)
                indexOfFirstColumnMeasuredParameters = result;

            //запоминаем индексы первого и последнего оспользованного столбца 
            this.SetIndexOfMeasuredParametersByTestType(testType, indexOfFirstColumnMeasuredParameters, result);

            return result;
        }

        public void SetRequisites(int devID, string deviceType, TemperatureCondition temperatureCondition, string profileID, string profileName, DateTime tsZeroTime, string groupName, string item, string code, string constructive, int? averageCurrent, int? deviceClass, string equipment, string user, string status, string reason)
        {
            //запоминаем идентификационную информацию
            this.DevID = devID;
            this.DevType = deviceType;
            this.TemperatureCondition = temperatureCondition;
            this.ProfileID = profileID;
            this.ProfileName = profileName;
            this.TsZeroTime = tsZeroTime;
            this.GroupName = groupName;
            this.Item = item;
            this.Code = code;
            this.Constructive = constructive;
            this.AverageCurrent = averageCurrent;
            this.DeviceClass = deviceClass;
            this.Equipment = equipment;
            this.User = user;
            this.Status = status;
            this.Reason = reason;
        }

        public void Load(SqlConnection connection, DataTableParameters sourceDataTableParameters, string profileID, int devID, string devType, TemperatureCondition temperatureCondition)
        {
            TestParametersType testParametersType=TestParametersType.Bvt;

            try
            {
                //Bvt
                this.LoadConditions(connection, sourceDataTableParameters, testParametersType, profileID, devType, temperatureCondition);
                this.LoadMeasuredParameters(connection, testParametersType, devID);
                this.LoadNormatives(connection, sourceDataTableParameters, testParametersType, profileID);

                //Dvdt
                testParametersType = TestParametersType.Dvdt;
                this.LoadConditions(connection, sourceDataTableParameters, testParametersType, profileID, devType, temperatureCondition);
                this.LoadMeasuredParameters(connection, testParametersType, devID);
                this.LoadNormatives(connection, sourceDataTableParameters, testParametersType, profileID);

                //SL
                testParametersType = TestParametersType.StaticLoses;
                this.LoadConditions(connection, sourceDataTableParameters, testParametersType, profileID, devType, temperatureCondition);
                this.LoadMeasuredParameters(connection, testParametersType, devID);
                this.LoadNormatives(connection, sourceDataTableParameters, testParametersType, profileID);

                //Gate
                testParametersType = TestParametersType.Gate;
                this.LoadConditions(connection, sourceDataTableParameters, testParametersType, profileID, devType, temperatureCondition);
                this.LoadMeasuredParameters(connection, testParametersType, devID);
                this.LoadNormatives(connection, sourceDataTableParameters, testParametersType, profileID);
            }
            catch
            {
                throw new Exception(string.Format("Тип теста {0},  profileID='{1}', devType={2}, temperatureCondition={3}", testParametersType.ToString(), profileID, devType, temperatureCondition.ToString()));
            }
        }

        private List<string> ConditionNamesByDeviceType(TestParametersType testType, string deviceType, TemperatureCondition temperatureCondition)
        {
            List<string> result = null;

            if ((deviceType != null) && (deviceType != string.Empty))
            {
                result = new List<string>();

                string firstSimbol = deviceType.Substring(0, 1).ToUpper();

                switch (testType)
                {
                    case TestParametersType.StaticLoses:
                        //тиристор, диод
                        if ((firstSimbol == "Т") || (firstSimbol == "Д"))
                        {
                            if (temperatureCondition == TemperatureCondition.RT)
                            {
                                //комнатная температура
                                result.Add("SL_ITM"); //в БД это же условие используется как IFM
                            }

                            if (temperatureCondition == TemperatureCondition.TM)
                            {
                                //горячее измерение
                                result.Add("SL_ITM"); //в БД это же условие используется как IFM
                            }
                        }
                        break;

                    case TestParametersType.Bvt:
                        if (temperatureCondition == TemperatureCondition.RT)
                            result.Add("BVT_I");

                        if (temperatureCondition == TemperatureCondition.TM)
                        {
                            //тиристор
                            if (firstSimbol == "Т")
                            {
                                //горячее измерение
                                result.Add("BVT_VD");
                                result.Add("BVT_VR");
                            }

                            //диод
                            if (firstSimbol == "Д")
                                result.Add("BVT_VR");
                        }

                        break;

                    case TestParametersType.Gate:
                    case TestParametersType.Commutation:
                    case TestParametersType.Clamping:

                    case TestParametersType.Dvdt:
                        result.Add("DVDT_VoltageRate");
                        break;

                    case TestParametersType.ATU:
                    case TestParametersType.RAC:
                    case TestParametersType.IH:
                    case TestParametersType.RCC:
                    case TestParametersType.Sctu:
                        break;

                    case TestParametersType.QrrTq:
                        if (firstSimbol == "Т")
                        {
                            //тиристор
                            if (temperatureCondition == TemperatureCondition.RT)
                            {
                                //комнатная температура
                                //измеряется только в TM
                            }

                            if (temperatureCondition == TemperatureCondition.TM)
                            {
                                //горячее измерение
                                result.Add("QrrTq_DCFallRate"); //скорость спада
                            }
                        }

                        if (firstSimbol == "Д")
                        {
                            //диод                        
                            if (temperatureCondition == TemperatureCondition.RT)
                            {
                                //комнатная температура
                                //измеряется только в TM
                            }

                            if (temperatureCondition == TemperatureCondition.TM)
                            {
                                //горячее измерение
                                result.Add("QrrTq_DCFallRate"); //скорость спада
                            }
                        }

                        break;
                }
            }

            return result;
        }

        private string ConditionUnitMeasure(string conditionName)
        {
            Dictionary<string, string> ConditionsUnitMeasure = new Dictionary<string, string>
            {
                {"SL_ITM", "А"},

                {"BVT_I", "мА"},
                {"BVT_VD", "В"},
                {"BVT_VR", "В"},

                {"DVDT_VoltageRate", "В/мкс"},

                {"QrrTq_DCFallRate", "А/мкс"}
            };

            switch (ConditionsUnitMeasure.ContainsKey(conditionName))
            {
                case true:
                    return ConditionsUnitMeasure[conditionName];

                default:
                    return string.Empty;
            }
        }

        private string ConditionName(string conditionName)
        {
            Dictionary<string, string> ConditionsName = new Dictionary<string, string>
            {
                {"SL_ITM", "ITM"},

                { "BVT_VD", "BVT_VD"}, //IDRM
                {"BVT_VR", "BVT_VR"}, //IRRM 

                {"DVDT_VoltageRate", "DvDt"},

                {"QrrTq_DCFallRate", "dIdT"}
            };

            switch (ConditionsName.ContainsKey(conditionName))
            {
                case true:
                    return ConditionsName[conditionName];

                default:
                    return conditionName;
            }
        }

        private bool TryParseTestParametersType(string strTestType, out TestParametersType testType)
        {
            switch (strTestType.ToUpper())
            {
                case "SL":
                    //почему-то именно по этому типу теста в базе данных и определении типа в TestParametersType имеется различие
                    testType = TestParametersType.StaticLoses;

                    return true;

                default:
                    return Enum.TryParse(strTestType, true, out testType);
            }
        }        

        private void CopyConditions(DataTableParameters source, TestParametersType testType)
        {
            //копирование описания условий из source в себя
            int indexOfFirstColumnCondition = -1;
            int indexOfLastColumnCondition = -1;

            switch (testType)
            {
                case TestParametersType.Bvt:
                    {
                        indexOfFirstColumnCondition = source.FIndexOfFirstColumnBvtCondition;
                        indexOfLastColumnCondition = source.FIndexOfLastColumnBvtCondition;
                        break;
                    }

                case TestParametersType.Dvdt:
                    {
                        indexOfFirstColumnCondition = source.FIndexOfFirstColumnDvdtCondition;
                        indexOfLastColumnCondition = source.FIndexOfLastColumnDvdtCondition;
                        break;
                    }

                case TestParametersType.StaticLoses:
                    {
                        indexOfFirstColumnCondition = source.FIndexOfFirstColumnSLCondition;
                        indexOfLastColumnCondition = source.FIndexOfLastColumnSLCondition;
                        break;
                    }

                case TestParametersType.Gate:
                    {
                        indexOfFirstColumnCondition = source.FIndexOfFirstColumnGateCondition;
                        indexOfLastColumnCondition = source.FIndexOfLastColumnGateCondition;
                        break;
                    }
            }

            //если (source.IndexOfFirstColumnCondition = -1) - значит копировать нечего
            if (indexOfFirstColumnCondition != -1)
            {
                for (int i = indexOfFirstColumnCondition; i <= indexOfLastColumnCondition; i++)
                {
                    //формируем столбец, который будет хранить описание условий
                    string conditionName = source.Columns[i].Caption;
                    int id = int.Parse(source.Columns[i].ExtendedProperties["id"].ToString());

                    int columnIndex = this.NewConditionColumn(testType, ConditionName(conditionName), id);

                    this.Rows[RowIndexOfUnitMeasure][columnIndex] = source.Rows[RowIndexOfUnitMeasure][i];
                    this.Rows[RowIndexOfValue][columnIndex] = source.Rows[RowIndexOfValue][i];
                }
            }
        }

        private void LoadConditionsFromDB(SqlConnection connection, TestParametersType testType, string profileID, string deviceType, TemperatureCondition temperatureCondition)
        {
            //избирательная (в зависимости от типа теста) загрузка значений conditions для принятого profileID
            string SQLText = "SELECT TT.TEST_TYPE_NAME, C.COND_ID, C.COND_NAME, PC.VALUE" +
                             " FROM PROF_COND AS PC" +
                             "  INNER JOIN PROFILES AS PR ON ((PC.PROF_ID=PR.PROF_ID) AND" +
                             "                                (PR.PROF_GUID=@ProfGuid))" +
                             "  INNER JOIN PROF_TEST_TYPE AS PTT ON (PC.PROF_TESTTYPE_ID=PTT.PTT_ID)" +
                             "  INNER JOIN TEST_TYPE AS TT ON ((PTT.TEST_TYPE_ID=TT.TEST_TYPE_ID) AND" +
                             "                                 (TT.TEST_TYPE_NAME=@TestTypeName))" +
                             "  INNER JOIN CONDITIONS C ON (PC.COND_ID=C.COND_ID)" +
                             " ORDER BY C.COND_ID";

            SqlCommand command = new SqlCommand(SQLText, connection);

            SqlParameter profGuid = new SqlParameter("ProfGuid", SqlDbType.UniqueIdentifier);
            profGuid.Value = new Guid(profileID);
            command.Parameters.Add(profGuid);

            SqlParameter testTypeName = new SqlParameter("TestTypeName", SqlDbType.NVarChar);
            testTypeName.Value = TestTypeName(testType.ToString());
            command.Parameters.Add(testTypeName);

            if (connection.State != ConnectionState.Open)
                connection.Open();

            try
            {
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    //запоминаем тип теста
                    int index = reader.GetOrdinal("TEST_TYPE_NAME");
                    var z = reader.GetValue(index);

                    if (TryParseTestParametersType(z.ToString(), out testType))
                    {
                        //получаем список условий, которые надо показать
                        List<string> conditions = ConditionNamesByDeviceType(testType, deviceType, temperatureCondition);

                        if ((conditions != null) && (conditions.Count > 0))
                        {
                            //запоминаем имя условия
                            index = reader.GetOrdinal("COND_NAME");
                            z = reader.GetValue(index);
                            string conditionName = z.ToString().TrimEnd();

                            //проверяем вхождение текущего условия в список условий, который требуется показать для текущего типа теста
                            if (conditions.IndexOf(conditionName) != -1)
                            {
                                index = reader.GetOrdinal("COND_ID");
                                z = reader.GetValue(index);
                                int id = int.Parse(z.ToString());

                                //формируем столбец, который будет хранить описание условий
                                int columnIndex = this.NewConditionColumn(testType, ConditionName(conditionName), id);

                                //ед. измерения у условия в БД нет - используем свой словарь
                                this.Rows[RowIndexOfUnitMeasure][columnIndex] = ConditionUnitMeasure(conditionName);

                                //запоминаем значение условия
                                index = reader.GetOrdinal("VALUE");
                                z = reader.GetValue(index);
                                string v = z.ToString();

                                double d;
                                switch (double.TryParse(v, out d))
                                {
                                    case true:
                                        string s = (Math.Abs(d % 1) <= (Double.Epsilon * 100)) ? ((int)d).ToString() : d.ToString("0.0");
                                        this.Rows[RowIndexOfValue][columnIndex] = s;
                                        break;

                                    default:
                                        this.Rows[RowIndexOfValue][columnIndex] = v;
                                        break;
                                }
                            }
                        }
                    }
                }
            }

            finally
            {
                connection.Close();
            }
        }

        private string TestTypeName(string testTypeName)
        {
            switch (testTypeName == "StaticLoses")
            {
                case true:
                    return "SL";

                default:
                    return testTypeName;
            }
        }

        private void LoadConditions(SqlConnection connection, DataTableParameters sourceDataTableParameters, TestParametersType testType, string profileID, string deviceType, TemperatureCondition temperatureCondition)
        {
            switch (sourceDataTableParameters == null)
            {
                case true:
                    //принятый sourceDataTableParameters есть null - добываем данные из базы данных
                    this.LoadConditionsFromDB(connection, testType, profileID, deviceType, temperatureCondition);
                    break;

                default:
                    //принятый sourceDataTableParameters не null - добываем данные из него, он выступает в качестве кеша
                    this.CopyConditions(sourceDataTableParameters, testType);
                    break;
            }
        }

        private string ParameterName(string parameterName)
        {
            Dictionary<string, string> ParametersName = new Dictionary<string, string>
            {
                {"VDRM", "UBO"},
                {"VRRM", "UBR"}
            };

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
            if (result.Substring(0, 1) == "V")
                result = 'U' + result.Remove(0, 1);

            return result;
        }

        private string ParameterFormat(string parameterName)
        {
            Dictionary<string, string> ParametersFormat = new Dictionary<string, string>
            {
                {"VTM", "0.00"},
                {"VFM", "0.00"}
            };

            string result;

            switch (ParametersFormat.ContainsKey(parameterName))
            {
                case true:
                    result = ParametersFormat[parameterName];
                    break;

                default:
                    result = string.Empty;
                    break;
            }

            return result;
        }

        private string DoubleToString(double value)
        {
            return (Math.Abs(value % 1) <= (Double.Epsilon * 100)) ? ((int)value).ToString() : value.ToString("0.0");
        }

        private string ObjectToString(object value)
        {
            string result = string.Empty;

            double d;
            if (double.TryParse(value.ToString(), out d))
                result = DoubleToString(d);

            return result;
        }

        private void LoadMeasuredParameters(SqlConnection connection, TestParametersType testType, int devID)
        {
            //построение списка измеренных параметров по принятому devID, с чтением: значения, единицы измерения и температуры при которой было выполнено данное измерение
            string SQLText = "SELECT PC.VALUE AS TEMPERATURE, P.PARAM_ID, P.PARAM_NAME, P.PARAMUM, DP.VALUE" +
                             " FROM DEV_PARAM DP" +
                             "  INNER JOIN PROF_TEST_TYPE AS PTT ON(DP.TEST_TYPE_ID=PTT.PTT_ID)" +
                             "  INNER JOIN TEST_TYPE AS TT ON ((PTT.TEST_TYPE_ID=TT.TEST_TYPE_ID) AND" +
                             "                                 (TT.TEST_TYPE_NAME=@TestTypeName))" +
                             "  INNER JOIN PARAMS AS P ON ((P.PARAM_ID=DP.PARAM_ID) AND" +
                             "                             NOT(P.PARAM_NAME='K'))" +
                             "  INNER JOIN DEVICES AS D ON ((DP.DEV_ID=D.DEV_ID) AND" +
                             "                              (D.DEV_ID=@DevID))" +
                             "  INNER JOIN PROFILES AS PR ON (D.PROFILE_ID=PR.PROF_GUID)" +
                             "  LEFT JOIN PROF_COND AS PC ON (PR.PROF_ID=PC.PROF_ID)" +
                             "  INNER JOIN CONDITIONS C ON ((PC.COND_ID=C.COND_ID) AND" +
                             "                              (C.COND_NAME='CLAMP_Temperature'))" +
                             " ORDER BY DP.DEV_PARAM_ID";

            SqlCommand command = new SqlCommand(SQLText, connection);

            SqlParameter DevID = new SqlParameter("DevID", SqlDbType.Int);
            DevID.Value = devID;
            command.Parameters.Add(DevID);

            SqlParameter testTypeName = new SqlParameter("TestTypeName", SqlDbType.NVarChar);
            testTypeName.Value = TestTypeName(testType.ToString());
            command.Parameters.Add(testTypeName);

            if (connection.State != ConnectionState.Open)
                connection.Open();

            try
            {
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    //запоминаем идентификатор параметра
                    int index = reader.GetOrdinal("PARAM_ID");
                    var z = reader.GetValue(index);
                    int id = int.Parse(z.ToString());

                    //запоминаем наименование параметра
                    index = reader.GetOrdinal("PARAM_NAME");
                    z = reader.GetValue(index);
                    string paramName = z.ToString().TrimEnd();

                    //запоминаем при какой температуре производилось измерение
                    index = reader.GetOrdinal("TEMPERATURE");
                    z = reader.GetValue(index);
                    int t = int.Parse(z.ToString());
                    string tc = ((t > 25) ? t.ToString() + "°C" : "RT");

                    //формируем столбец
                    int columnIndex = this.NewMeasuredParameterColumn(testType, string.Format("{0}{1}{2}", tc, this.TemperatureDelimeter, ParameterName(paramName)), id);

                    //запоминаем единицу измерения параметра
                    index = reader.GetOrdinal("PARAMUM");
                    z = reader.GetValue(index);
                    string um = z.ToString();
                    this.Rows[RowIndexOfUnitMeasure][columnIndex] = um;

                    //запоминаем значение параметра
                    index = reader.GetOrdinal("VALUE");
                    z = reader.GetValue(index);
                    string v = z.ToString();

                    double d;
                    switch (double.TryParse(v, out d))
                    {
                        case true:
                            //проверяем есть ли специальный формат вывода
                            string format = ParameterFormat(paramName);

                            switch (format == string.Empty)
                            {
                                case true:
                                    //для данного параметра не задан специальный формат вывода, выбираем оптимальный
                                    string s = DoubleToString(d);
                                    this.Rows[RowIndexOfValue][columnIndex] = s;
                                    break;

                                default:
                                    //выводим значение параметра в специальном формате вывода
                                    this.Rows[RowIndexOfValue][columnIndex] = d.ToString(format);
                                    break;
                            }
                            break;

                        default:
                            this.Rows[RowIndexOfValue][columnIndex] = v;
                            break;
                    }
                }
            }

            finally
            {
                connection.Close();
            }
        }

        private void CopyNormatives(DataTableParameters source, TestParametersType testType)
        {
            int indexOfLastColumnMeasuredParameters = -1;
            int indexOfFirstColumnMeasuredParameters = this.GetIndexOfMeasuredParametersByTestType(testType, out indexOfLastColumnMeasuredParameters);

            //копирование описания нормативов из source в себя. если (source.IndexOfFirstColumnMeasuredParameters=-1) - значит копировать нечего
            if (this.ProfileID != source.ProfileID)
                throw new InvalidOperationException("Вызван метод DataTableParameters.CopyNormatives для сущностей с разными профилями. Выполнение данного метода предполагает, что профили источника и приёмника имеют одинаковый идентификатор.");

            if (indexOfFirstColumnMeasuredParameters != -1)
            {
                for (int i = indexOfFirstColumnMeasuredParameters; i <= indexOfLastColumnMeasuredParameters; i++)
                {
                    //столбец, который хранит описание норм уже создан и имеет точно такой же индекс, что и столбец в source
                    this.Rows[RowIndexOfNorm][i] = source.Rows[RowIndexOfNorm][i];
                }
            }
        }

        private void LoadNormativesFromDB(SqlConnection connection, TestParametersType testType, string profileID)
        {
            //обходим все столбцы измеренных параметров (на момент вызова они должны быь созданы) и считываем из базы данных значения норм на данные параметры
            int indexOfLastColumnMeasuredParameters = -1;
            int indexOfFirstColumnMeasuredParameters = this.GetIndexOfMeasuredParametersByTestType(testType, out indexOfLastColumnMeasuredParameters);

            if (indexOfFirstColumnMeasuredParameters != -1)
            {
                string SQLText = "SELECT PP.MIN_VAL, PP.MAX_VAL" +
                                 " FROM PROF_PARAM PP" +
                                 "  INNER JOIN PROFILES P ON((PP.PROF_ID=P.PROF_ID) AND" +
                                 "                           (P.PROF_GUID=@ProfileID))" +
                                 " WHERE (PP.PARAM_ID=@ParamID)";

                SqlCommand command = new SqlCommand(SQLText, connection);

                SqlParameter ProfileID = new SqlParameter("ProfileID", SqlDbType.UniqueIdentifier);
                ProfileID.Value = new Guid(profileID);
                command.Parameters.Add(ProfileID);

                SqlParameter ParamID = new SqlParameter("ParamID", SqlDbType.Int);
                command.Parameters.Add(ParamID);

                if (connection.State != ConnectionState.Open)
                    connection.Open();

                try
                {
                    for (int i = indexOfFirstColumnMeasuredParameters; i <= indexOfLastColumnMeasuredParameters; i++)
                    {
                        //по принятому profileID и идентификатору параметра id читаем из базы данных значения норм min и max                  
                        DataColumn column = this.Columns[i];
                        int id = int.Parse(column.ExtendedProperties["id"].ToString());

                        ParamID.Value = id;
                        SqlDataReader reader = command.ExecuteReader();

                        try
                        {
                            while (reader.Read())
                            {
                                //запоминаем описание норм на параметр
                                int index = reader.GetOrdinal("MIN_VAL");
                                var z = reader.GetValue(index);
                                string nMin = (z == DBNull.Value ? null : ObjectToString(z));

                                index = reader.GetOrdinal("MAX_VAL");
                                z = reader.GetValue(index);
                                string nMax = (z == DBNull.Value ? null : ObjectToString(z));

                                string n = string.Empty;
                                if ((nMin != null) && (nMax != null))
                                    n = string.Format("({0}, {1})", nMin, nMax);
                                else
                                {
                                    if (nMin == null)
                                    {
                                        if (nMax != null)
                                            n = string.Format("<{0}", nMax);
                                    }
                                    else n = string.Format(">{0}", nMin);
                                }

                                this.Rows[RowIndexOfNorm][i] = n;
                            }
                        }
                        finally
                        {
                            reader.Close();
                        }
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        private void LoadNormatives(SqlConnection connection, DataTableParameters sourceDataTableParameters, TestParametersType testType, string profileID)
        {
            //загрузка значений норм для принятого профиля profileID
            switch (sourceDataTableParameters == null)
            {
                case true:
                    //принятый sourceDataTableParameters есть null - добываем данные из базы данных
                    this.LoadNormativesFromDB(connection, testType, profileID);
                    break;

                default:
                    //принятый sourceDataTableParameters не null - добываем данные из него, он выступает в качестве кеша
                    this.CopyNormatives(sourceDataTableParameters, testType);
                    break;
            }
        }

        private string RCColumnNumToA1ColumnNum(int columnNum)
        {
            //преобразование номера столбца ColumnNum из цифровой идентификации в строковую идентификацию
            int A1 = Convert.ToByte('A') - 1;  //номер "A" минус 1 (65 - 1 = 64)

            int AZ = Convert.ToByte('Z') - A1; //кол-во букв в англ. алфавите (90 - 64 = 26)
            int t, m;

            //номер колонки
            t = (int)(columnNum / AZ); //целая часть
            m = (columnNum % AZ);      //остаток

            if (m == 0)
                t--;

            char result = '\0';

            switch (t > 0)
            {
                case (true):
                    result = Convert.ToChar(A1 + t);
                    break;
            }

            switch (m)
            {
                case (0):
                    t = AZ;
                    break;

                default:
                    t = m;
                    break;
            }

            string Result = "";
            if (result != '\0')
                Result = Convert.ToString(result);

            return Result + Convert.ToString(Convert.ToChar(A1 + t));
        }

        public string xlRCtoA1(int rowNum, int columnNum)
        {
            //преобразование цифровой идентификации ячейки (по номеру столбца, номеру строки) в строковую идентификацию
            return "$" + RCColumnNumToA1ColumnNum(columnNum) + rowNum.ToString();
        }

        public Range range(Excel.Application exelApp, int rowNum, int columnNum)
        {
            switch (exelApp != null)
            {
                case true:
                    string addr = xlRCtoA1(rowNum, columnNum);

                    return exelApp.get_Range(addr, addr);

                default:
                    return null;
            }
        }

        public Range range(Excel.Application exelApp, int rowNumBeg, int columnNumBeg, int rowNumEnd, int columnNumEnd)
        {
            switch (exelApp != null)
            {
                case true:
                    string addrBeg = xlRCtoA1(rowNumBeg, columnNumBeg);
                    string addrEnd = xlRCtoA1(rowNumEnd, columnNumEnd);

                    return exelApp.get_Range(addrBeg, addrEnd);

                default:
                    return null;
            }
        }

        public void SetBorders(Excel.Application exelApp, Excel.Worksheet sheet, int rowNumBeg, int rowNumEnd, int columnEnd)
        {
            if ((exelApp != null) || (sheet != null))
            {
                const int xlContinuous = 1;
                const int xlAutomatic = -4105;
                const int xlThin = 2;
                const int xlMedium = -4138;

                //вычисляем адреса начала и конца выведенного блока данных по изделию
                string addrBeg = xlRCtoA1(rowNumBeg, 1);
                string addrEnd = xlRCtoA1(rowNumEnd, columnEnd);

                Range range = exelApp.get_Range(addrBeg, addrEnd);
                range.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlInsideVertical].LineStyle = xlContinuous;
                range.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlInsideVertical].ColorIndex = xlAutomatic;
                range.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlInsideVertical].Weight = xlThin;

                range.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlInsideHorizontal].LineStyle = xlContinuous;
                range.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlInsideHorizontal].ColorIndex = xlAutomatic;
                range.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlInsideHorizontal].Weight = xlThin;

                range.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeLeft].LineStyle = xlContinuous;
                range.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeLeft].ColorIndex = xlAutomatic;
                range.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeLeft].Weight = xlMedium;

                range.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeRight].LineStyle = xlContinuous;
                range.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeRight].ColorIndex = xlAutomatic;
                range.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeRight].Weight = xlThin;

                range.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeTop].LineStyle = xlContinuous;
                range.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeTop].ColorIndex = xlAutomatic;
                range.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeTop].Weight = xlThin;

                range.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom].LineStyle = xlContinuous;
                range.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom].ColorIndex = xlAutomatic;
                range.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom].Weight = xlThin;
            }
        }

        public int? QtyReleasedByGroupName(string groupName, SqlConnection connection)
        {
            //возвращает количество изделий, запущенных по ЗП GroupName
            SqlCommand selectCommand = new SqlCommand("SELECT dbo.SL_Qty_ReleasedByJob(@Job)", connection);
            SqlParameter Job = new SqlParameter("@Job", SqlDbType.NVarChar);

            selectCommand.Parameters.Add("@Job", SqlDbType.NVarChar, 20);
            selectCommand.Parameters["@Job"].Value = groupName;

            if (connection.State != ConnectionState.Open)
                connection.Open();

            selectCommand.Prepare();

            try
            {
                var res = selectCommand.ExecuteScalar();

                return (res == DBNull.Value) ? null : (int?)res;
            }

            finally
            {
                connection.Close();
            }
        }

        public void QtyReleasedByGroupNameToExcel(Excel.Application exelApp, Excel.Worksheet sheet, ref int rowNum, string groupName, int? qtyReleased)
        {
            if ((exelApp != null) || (sheet != null))
            {
                Range rng = null;

                rng = this.range(exelApp, rowNum, 2);
                rng.Value2 = "Общее кол-во";
                rng.HorizontalAlignment = -4108;

                rng = this.range(exelApp, rowNum, 3);
                rng.Value2 = qtyReleased.ToString();
                rng.HorizontalAlignment = -4108;

                rng = this.range(exelApp, rowNum, 4);
                rng.Value2 = "шт.";
                rng.HorizontalAlignment = -4108;
                rowNum++;
            }
        }

        public void QtyOKFaultToExcel(Excel.Application exelApp, Excel.Worksheet sheet, int rowNum, int statusFaultCounter, int statusOKCounter)
        {
            if ((exelApp != null) || (sheet != null))
            {
                Range rng = null;

                rng = this.range(exelApp, rowNum, 2);
                rng.Value2 = "Годных";
                rng.HorizontalAlignment = -4108;

                rng = this.range(exelApp, rowNum, 3);
                rng.Value2 = statusOKCounter.ToString();
                rng.HorizontalAlignment = -4108;

                rng = this.range(exelApp, rowNum, 4);
                rng.Value2 = "шт.";
                rng.HorizontalAlignment = -4108;
                rowNum++;

                rng = this.range(exelApp, rowNum, 2);
                rng.Value2 = "Fault";
                rng.HorizontalAlignment = -4108;

                rng = this.range(exelApp, rowNum, 3);
                rng.Value2 = statusFaultCounter.ToString();
                rng.HorizontalAlignment = -4108;

                rng = this.range(exelApp, rowNum, 4);
                rng.Value2 = "шт.";
                rng.HorizontalAlignment = -4108;
                rowNum++;

                rng = this.range(exelApp, rowNum, 2);
                rng.Value2 = "Сформирован";
                rng.HorizontalAlignment = -4108;

                rng = this.range(exelApp, rowNum, 3);
                rng.Value2 = Environment.UserName;
                rng.HorizontalAlignment = -4108;

                rng = this.range(exelApp, rowNum, 4);
                rng.Value2 = DateTime.Today.ToString("dd.MM.yyyy");
                rng.HorizontalAlignment = -4108;
            }
        }

        public void Paint(Excel.Application exelApp, Excel.Worksheet sheet, int rowNumBeg, int rowNumEnd, int colunmBeg, int columnEnd, TemperatureCondition temperatureCondition)
        {
            if ((exelApp != null) || (sheet != null))
            {
                const int xlSolid = 1;
                const int xlAutomatic = -4105;

                const int xlThemeColorAccent1 = 5;
                const int xlThemeColorAccent2 = 6;
                const int xlThemeColorDark1 = 1;

                int color;

                switch (temperatureCondition)
                {
                    case TemperatureCondition.RT:
                        color = xlThemeColorAccent1;
                        break;

                    case TemperatureCondition.TM:
                        color = xlThemeColorAccent2;
                        break;

                    default:
                        color = xlThemeColorDark1;
                        break;
                }

                //вычисляем адреса начала и конца выведенного блока данных по изделию
                string addrBeg = xlRCtoA1(rowNumBeg, colunmBeg);
                string addrEnd = xlRCtoA1(rowNumEnd, columnEnd);

                Range range = exelApp.get_Range(addrBeg, addrEnd);

                range.Interior.Pattern = xlSolid;
                range.Interior.PatternColorIndex = xlAutomatic;
                range.Interior.ThemeColor = color;
                range.Interior.TintAndShade = 0.799981688894314;
                range.Interior.PatternTintAndShade = 0;
            }
        }

        private string ParseColumnName(string ColumnName, out string temperatureCondition)
        {
            int pos = ColumnName.IndexOf(this.TemperatureDelimeter);

            switch (pos)
            {
                case -1:
                    //разделитель отсутствует
                    temperatureCondition = string.Empty;
                    return ColumnName;

                default:
                    temperatureCondition = ColumnName.Substring(0, pos);
                    return ColumnName.Remove(0, pos + this.TemperatureDelimeter.Length);
            }
        }

        public void HeaderToExcel(Excel.Application exelApp, Excel.Worksheet sheet, ref int rowNum, ref int column, ref int columnEnd)
        {
            //вывод общего заголовка
            if ((exelApp != null) || (sheet != null))
            {
                Range rng = null;

                rng = this.range(exelApp, rowNum, column);
                rng.Value2 = "Норма";
                rng.HorizontalAlignment = -4108;
                rowNum++;
                column -= 3;

                rng = this.range(exelApp, rowNum, column);
                rng.Value2 = "№";
                rng.HorizontalAlignment = -4108;
                column++;

                rng = this.range(exelApp, rowNum, column);
                rng.Value2 = "№ ПЗ";
                rng.HorizontalAlignment = -4108;
                column++;

                rng = this.range(exelApp, rowNum, column);
                rng.Value2 = "№ ППЭ";
                rng.HorizontalAlignment = -4108;
                column++;

                rng = this.range(exelApp, rowNum, column);
                rng.Value2 = "Класс";
                rng.HorizontalAlignment = -4108;
                column++;

                rng = this.range(exelApp, rowNum, columnEnd);
                rng.Value2 = "№ MME";
                rng.HorizontalAlignment = -4108;
                columnEnd++;

                rng = this.range(exelApp, rowNum, columnEnd);
                rng.Value2 = "Дата";
                rng.HorizontalAlignment = -4108;
                columnEnd++;
            }
        }

        public void StatusHeaderToExcel(Excel.Application exelApp, Excel.Worksheet sheet, int rowNum, int column)
        {
            //вывод наименований столбцов статуса и причины
            if ((exelApp != null) || (sheet != null))
            {
                Range rng = null;

                rng = this.range(exelApp, rowNum, column);
                rng.Value2 = "Статус";
                rng.HorizontalAlignment = -4108;
                column++;

                rng = this.range(exelApp, rowNum, column);
                rng.Value2 = "Код НП";
                rng.HorizontalAlignment = -4108;
                column++;

                rng = this.range(exelApp, rowNum, column);
                rng.Value2 = "Код ТМЦ";
                rng.HorizontalAlignment = -4108;
                rowNum -= 4;

                rng = this.range(exelApp, rowNum, column);
                rng.Value2 = "ПРОТОКОЛ ИСПЫТАНИЙ";
                rng.HorizontalAlignment = -4108;
                rng.Font.Bold = true;
                rowNum++;

                //выводим тело профиля с отрезанной температурной частью (символы с индексами 0, 1), символ 2 это пробел
                rng = this.range(exelApp, rowNum, column);
                rng.NumberFormat = "@";
                rng.Value2 = this.ProfileBody.Substring(3);
                rng.HorizontalAlignment = -4108;
            }
        }

        public int StatusToExcel(Excel.Application exelApp, Excel.Worksheet sheet, string status, int rowNum, int column)
        {
            //вывод значений столбцов статуса и кода НП
            if ((exelApp != null) || (sheet != null))
            {
                Range rng = null;

                //выводим статус
                rng = this.range(exelApp, rowNum, column);
                rng.NumberFormat = "@";
                rng.Value2 = status;
                rng.HorizontalAlignment = -4108;
                column++;

                //выводим код НП
                rng = this.range(exelApp, rowNum, column);
                rng.NumberFormat = "@";
                rng.Value2 = 0;
                rng.HorizontalAlignment = -4108;
                column++;

                //выводим код ТМЦ
                rng = this.range(exelApp, rowNum, column);
                rng.NumberFormat = "@";
                rng.Value2 = this.Item;
                rng.HorizontalAlignment = -4108;
            }

            return column;
        }

        public void CounterToExcel(Excel.Application exelApp, Excel.Worksheet sheet, int counter, int rowNum)
        {
            //вывод значения счётчика counter
            if ((exelApp != null) || (sheet != null))
            {
                Range rng = null;

                //выводим порядковый номер
                rng = this.range(exelApp, rowNum, 1);
                rng.NumberFormat = "@";
                rng.Value2 = counter;
            }
        }

        public void IdentityToExcel(Excel.Application exelApp, Excel.Worksheet sheet, int counter, int rowNum, ref int column)
        {
            //вывод идентификационные данные
            if ((exelApp != null) || (sheet != null))
            {
                Range rng = null;

                //выводим порядковый номер
                rng = this.range(exelApp, rowNum, column);
                rng.NumberFormat = "@";
                rng.Value2 = counter;
                column++;

                //выводим идентификационные данные   
                rng = this.range(exelApp, rowNum, column);
                rng.NumberFormat = "@";
                rng.Value2 = this.GroupName;
                column++;

                rng = this.range(exelApp, rowNum, column);
                rng.NumberFormat = "@";
                rng.Value2 = this.CodeSimple;
                column++;

                rng = this.range(exelApp, rowNum, column);
                rng.NumberFormat = "@";
                rng.Value2 = this.DeviceClass;
                rng.HorizontalAlignment = -4108;
                column++;
            }
        }

        public void EndIdentityToExcel(Excel.Application exelApp, Excel.Worksheet sheet, int rowNum, ref int column)
        {
            //вывод конечных идентификационных данных
            if ((exelApp != null) || (sheet != null))
            {
                Range rng = null;

                //выводим код MME
                rng = this.range(exelApp, rowNum, column);
                rng.NumberFormat = "@";
                rng.Value2 = this.Equipment;
                rng.HorizontalAlignment = -4108;
                column++;

                //выводим дату выполнения измерений
                rng = this.range(exelApp, rowNum, column);
                rng.NumberFormat = "d/mm/yyyy";
                rng.Value2 = this.TsZeroTime;
                rng.HorizontalAlignment = -4108;
                column++;
            }
        }

        public void PairIdentityToExcel(Excel.Application exelApp, Excel.Worksheet sheet, int rowNum, int column)
        {
            //вывод идентификационных данных Pair
            if ((exelApp != null) || (sheet != null))
                this.EndIdentityToExcel(exelApp, sheet, rowNum, ref column);
        }

        public void ColumnsHeaderToExcel(Excel.Application exelApp, Excel.Worksheet sheet, ref int rowNum, ref int column)
        {
            //вывод наименований столбцов условий и параметров изделия
            if ((exelApp != null) || (sheet != null))
            {
                Range rng = null;

                for (int columnIndex = 0; columnIndex <= this.Columns.Count - 1; columnIndex++)
                {
                    DataColumn Column = this.Columns[columnIndex];

                    string temperatureCondition;
                    string paramName = this.ParseColumnName(Column.ColumnName, out temperatureCondition);

                    //выводим температуру при которой выполнено измерение данного параметра
                    rng = this.range(exelApp, rowNum, column + columnIndex);
                    rng.Value2 = temperatureCondition;
                    rng.HorizontalAlignment = -4108;

                    //выводим имя параметра
                    rng = this.range(exelApp, rowNum + 1, column + columnIndex);
                    rng.Value2 = paramName;
                    rng.HorizontalAlignment = -4108;
                    rng.Font.Bold = true;

                    //выводим единицу измерения
                    rng = this.range(exelApp, rowNum + 2, column + columnIndex);
                    rng.Value2 = this.Rows[RowIndexOfUnitMeasure][columnIndex];
                    rng.HorizontalAlignment = -4108;

                    //выводим норму
                    rng = this.range(exelApp, rowNum + 3, column + columnIndex);
                    rng.Value2 = this.Rows[RowIndexOfNorm][columnIndex];
                    rng.HorizontalAlignment = -4108;
                    rng.Font.Bold = true;
                }

                rowNum += 3;
                column = column + this.Columns.Count;
            }
        }

        public void PairHeaderToExcell(Excel.Application exelApp, Excel.Worksheet sheet, int rowNum, ref int column)
        {
            if ((exelApp != null) || (sheet != null))
            {
                Range rng = null;

                rng = this.range(exelApp, rowNum, column);
                rng.Value2 = "№ MME";
                rng.HorizontalAlignment = -4108;
                column++;

                rng = this.range(exelApp, rowNum, column);
                rng.Value2 = "Дата";
                rng.HorizontalAlignment = -4108;
                column++;
            }
        }

        private bool IsValueInNorm(string norm, string value)
        {
            //сравнивает принятое значение value с принятым значением нормы norm
            //принятая norm - строка вида: >x, <x. где x есть double значение нормы
            bool result = false;

            double dValue = double.Parse(value);

            int pos = norm.IndexOf(">");
            if (pos != -1)
            {
                string sNorm = norm.Remove(pos, 1);
                double dNorm = double.Parse(sNorm, CultureInfo.InvariantCulture);

                result = (dValue > dNorm);
            }

            pos = norm.IndexOf("<");
            if (pos != -1)
            {
                string sNorm = norm.Remove(pos, 1);
                double dNorm = double.Parse(sNorm, CultureInfo.InvariantCulture);

                result = (dValue < dNorm);
            }

            return result;
        }

        public void BodyToExcel(Excel.Application exelApp, Excel.Worksheet sheet, int rowNum, ref int column)
        {
            //вывод значений условий и параметров
            if ((exelApp != null) || (sheet != null))
            {
                Range rng = null;

                for (int columnIndex = 0; columnIndex <= this.Columns.Count - 1; columnIndex++)
                {
                    //выводим значение параметра
                    string value = this.Rows[RowIndexOfValue][columnIndex].ToString();

                    if (value != string.Empty)
                    {
                        rng = this.range(exelApp, rowNum, column + columnIndex);
                        rng.Value2 = value;
                        rng.HorizontalAlignment = -4108;

                        string norm = this.Rows[RowIndexOfNorm][columnIndex].ToString();

                        if (norm != string.Empty)
                        {
                            //проверяем входит ли выведенное значение параметра в норматив
                            if (!IsValueInNorm(norm, value))
                            {
                                //выведенное значение за пределами норм - красим его
                                rng.Interior.Pattern = 1; //xlSolid
                                rng.Interior.PatternColorIndex = -4105; //xlAutomatic
                                rng.Interior.Color = 255;
                                rng.Font.Bold = true;
                            }
                        }
                    }
                }

                column += this.Columns.Count;
            }
        }
    }


}
