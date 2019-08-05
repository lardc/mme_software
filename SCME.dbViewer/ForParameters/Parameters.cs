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

namespace SCME.dbViewer.ForParameters
{
    public class DataTableParameters : System.Data.DataTable
    {
        public int DevID { get; set; }
        public string ProfileName { get; set; }
        public string ProfileID { get; set; }
        public TemperatureCondition TemperatureCondition { get; set; }
        public string GroupName { get; set; }
        public string Code { get; set; }
        public string SilN1 { get; set; }
        public string SilN2 { get; set; }
        public string Equipment { get; set; }
        public string User { get; set; }

        public bool Used { get; set; }

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
                {
                    result += column.ColumnName;
                }

                return result;
            }
        }

        public DataTableParameters()
        {
            //всегда содержит две записи: первая хранит значение параметров, вторая - единицы измерения параметров
            DataRow row = this.NewRow();
            this.Rows.Add(row);

            row = this.NewRow();
            this.Rows.Add(row);
        }

        public int NewColumn(string name)
        {
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
            this.Columns.Add(column);

            return this.Columns.IndexOf(column);
        }

        public void Load(SqlConnection connection, int devID, string devType, TemperatureCondition temperatureCondition)
        {
            this.LoadConditions(connection, devID, devType, temperatureCondition);
            this.LoadMeasuredParameters(connection, devID);
        }

        public void Load(SqlConnection connection, int devID, string devType, TemperatureCondition temperatureCondition, string profileID, string profileName, DateTime tsZeroTime, string groupName, string item, string code, string constructive, int averageCurrent, int deviceClass, string equipment, string user, string status, string reason)
        {
            this.Load(connection, devID, devType, temperatureCondition);

            //запоминаем идентификационную информацию
            this.DevID = devID;
            this.ProfileName = profileName;
            this.ProfileID = profileID;
            this.TemperatureCondition = temperatureCondition;
            this.GroupName = groupName;
            this.Code = code;
            this.Equipment = equipment;
            this.User = user;
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
                        //тиристор
                        if (firstSimbol == "Т")
                        {
                            if (temperatureCondition == TemperatureCondition.RT)
                            {
                                //комнатная температура
                                //временно нет
                            }

                            if (temperatureCondition == TemperatureCondition.TM)
                            {
                                //горячее измерение
                                result.Add("BVT_VD");
                                result.Add("BVT_VR");
                            }
                        }

                        //диод
                        if (firstSimbol == "Д")
                        {
                            if (temperatureCondition == TemperatureCondition.RT)
                            {
                                //комнатная температура
                                //временно нет
                            }

                            if (temperatureCondition == TemperatureCondition.TM)
                            {
                                //горячее измерение
                                result.Add("BVT_VR");
                            }
                        }

                        break;

                    case TestParametersType.Gate:
                    case TestParametersType.Commutation:
                    case TestParametersType.Clamping:
                    case TestParametersType.Dvdt:
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
                {"BVT_VD", "В"},
                {"BVT_VR", "В"},
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
                {"BVT_VD", "BVT_VD"}, //IDRM
                {"BVT_VR", "BVT_VR"}, //IRRM 
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

        private void LoadConditions(SqlConnection connection, int devID, string deviceType, TemperatureCondition temperatureCondition)
        {
            //избирательная (в зависимости от типа теста) загрузка значений conditions для принятого devID
            string SQLText = "SELECT TT.TEST_TYPE_NAME, C.COND_NAME, PC.VALUE" +
                             " FROM PROF_COND AS PC" +
                             "  INNER JOIN PROFILES AS PR ON (PC.PROF_ID=PR.PROF_ID)" +
                             "  INNER JOIN DEVICES AS D ON ((D.PROFILE_ID=PR.PROF_GUID) AND" +
                             "                              (D.DEV_ID=@DevID))" +
                             "  INNER JOIN PROF_TEST_TYPE AS PTT ON (PC.PROF_TESTTYPE_ID=PTT.PTT_ID)" +
                             "  INNER JOIN TEST_TYPE AS TT ON (PTT.TEST_TYPE_ID=TT.TEST_TYPE_ID)" +
                             "  INNER JOIN CONDITIONS C ON (PC.COND_ID=C.COND_ID)" +
                             " ORDER BY PTT.PTT_ID";

            SqlCommand command = new SqlCommand(SQLText, connection);
            SqlParameter DevID = new SqlParameter("DevID", SqlDbType.Int);
            DevID.Value = devID;
            command.Parameters.Add(DevID);

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
                    TestParametersType testType;

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
                                //формируем столбец
                                int columnIndex = this.NewColumn(ConditionName(conditionName));

                                //запоминаем значение условия
                                index = reader.GetOrdinal("VALUE");
                                z = reader.GetValue(index);
                                string v = z.ToString();

                                double d;
                                switch (double.TryParse(v, out d))
                                {
                                    case true:
                                        string s = (Math.Abs(d % 1) <= (Double.Epsilon * 100)) ? ((int)d).ToString() : d.ToString("0.0");
                                        this.Rows[0][columnIndex] = s;
                                        break;

                                    default:
                                        this.Rows[0][columnIndex] = v;
                                        break;
                                }

                                //ед. измерения у условия в БД нет - используем свой словарь
                                this.Rows[1][columnIndex] = ConditionUnitMeasure(conditionName);
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

        private void LoadMeasuredParameters(SqlConnection connection, int devID)
        {
            //чтение значения данного параметра с его единицей измерения и температурой при которой было выполнено данное измерение
            string SQLText = "SELECT PC.VALUE AS TEMPERATURE, P.PARAM_NAME, P.PARAMUM, DP.VALUE" +
                             " FROM DEV_PARAM DP" +
                             "  INNER JOIN PARAMS AS P ON ((P.PARAM_ID=DP.PARAM_ID) AND" +
                             "                             NOT(P.PARAM_NAME='K'))" +
                             "  INNER JOIN DEVICES AS D ON (DP.DEV_ID=D.DEV_ID)" +
                             "  INNER JOIN PROFILES AS PR ON (D.PROFILE_ID=PR.PROF_GUID)" +
                             "  LEFT JOIN PROF_COND AS PC ON (PR.PROF_ID=PC.PROF_ID)" +
                             "  INNER JOIN CONDITIONS C ON ((PC.COND_ID=C.COND_ID) AND" +
                             "                              (C.COND_NAME='CLAMP_Temperature'))" +
                             " WHERE (DP.DEV_ID=@DevID)" +
                             " ORDER BY DP.DEV_PARAM_ID";

            SqlCommand command = new SqlCommand(SQLText, connection);

            SqlParameter DevID = new SqlParameter("DevID", SqlDbType.Int);
            DevID.Value = devID;
            command.Parameters.Add(DevID);

            if (connection.State != ConnectionState.Open)
                connection.Open();

            try
            {
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    //запоминаем наименование параметра
                    int index = reader.GetOrdinal("PARAM_NAME");
                    var z = reader.GetValue(index);
                    string paramName = z.ToString().TrimEnd();

                    //запоминаем при какой температуре производилось измерение
                    index = reader.GetOrdinal("TEMPERATURE");
                    z = reader.GetValue(index);
                    int t = int.Parse(z.ToString());
                    string tc = ((t > 25) ? t.ToString() + "°C" : "RT");

                    //формируем столбец
                    int columnIndex = this.NewColumn(string.Format("{0}{1}{2}", tc, this.TemperatureDelimeter, ParameterName(paramName)));

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
                                    string s = (Math.Abs(d % 1) <= (Double.Epsilon * 100)) ? ((int)d).ToString() : d.ToString("0.0");
                                    this.Rows[0][columnIndex] = s;
                                    break;

                                default:
                                    //выводим значение параметра в специальном формате вывода
                                    this.Rows[0][columnIndex] = d.ToString(format);
                                    break;
                            }
                            break;

                        default:
                            this.Rows[0][columnIndex] = v;
                            break;
                    }

                    //запоминаем единицу измерения параметра
                    index = reader.GetOrdinal("PARAMUM");
                    z = reader.GetValue(index);
                    string um = z.ToString();
                    this.Rows[1][columnIndex] = um;
                }
            }

            finally
            {
                connection.Close();
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
                range.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeRight].Weight = xlMedium;

                range.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeTop].LineStyle = xlContinuous;
                range.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeTop].ColorIndex = xlAutomatic;
                range.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeTop].Weight = xlMedium;

                range.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom].LineStyle = xlContinuous;
                range.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom].ColorIndex = xlAutomatic;
                range.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom].Weight = xlMedium;
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

        public void HeaderToExcel(Excel.Application exelApp, Excel.Worksheet sheet, int rowNum, ref int column)
        {
            //вывод общего заголовка
            if ((exelApp != null) || (sheet != null))
            {
                Range rng = null;

                rng = this.range(exelApp, rowNum, column);
                rng.Value2 = "№";
                rng.HorizontalAlignment = -4108;
                column++;

                rng = this.range(exelApp, rowNum, column);
                rng.Value2 = "№ ПЗ";
                rng.HorizontalAlignment = -4108;
                column++;

                rng = this.range(exelApp, rowNum, column);
                rng.Value2 = "Серийный номер";
                rng.HorizontalAlignment = -4108;
                column++;

                rng = this.range(exelApp, rowNum, column);
                rng.Value2 = "Партия ППЭ";
                rng.HorizontalAlignment = -4108;
                column++;

                rng = this.range(exelApp, rowNum, column);
                rng.Value2 = "Номер ППЭ";
                rng.HorizontalAlignment = -4108;
                column++;

                rng = this.range(exelApp, rowNum, column);
                rng.Value2 = "Стенд";
                rng.HorizontalAlignment = -4108;
                column++;

                rng = this.range(exelApp, rowNum, column);
                rng.Value2 = "Оператор";
                rng.HorizontalAlignment = -4108;
                column++;
            }
        }

        public void PairHeaderToExcel(Excel.Application exelApp, Excel.Worksheet sheet, int rowNum, int column)
        {
            //вывод заголовка для достроенных данных Pair
            //вывод общего заголовка
            if ((exelApp != null) || (sheet != null))
            {
                Range rng = null;
                rng = this.range(exelApp, rowNum, column);
                rng.Value2 = "Стенд";
                rng.HorizontalAlignment = -4108;
                column++;

                rng = this.range(exelApp, rowNum, column);
                rng.Value2 = "Оператор";
                rng.HorizontalAlignment = -4108;
                column++;
            }
        }

        public void IdentityToExcel(Excel.Application exelApp, Excel.Worksheet sheet, int number, int rowNum, ref int column)
        {
            //вывод строки с общими идентификационными данными
            //выводим порядковый номер
            if ((exelApp != null) || (sheet != null))
            {
                Range rng = null;
                rng = this.range(exelApp, rowNum, column);
                rng.Value2 = number.ToString();
                column++;

                //выводим идентификационные данные   
                rng = this.range(exelApp, rowNum, column);
                rng.NumberFormat = "@";
                rng.Value2 = this.GroupName;
                column++;

                rng = this.range(exelApp, rowNum, column);
                rng.NumberFormat = "@";
                rng.Value2 = this.Code;
                column++;

                rng = this.range(exelApp, rowNum, column);
                rng.NumberFormat = "@";
                rng.Value2 = this.SilN1;
                column++;

                rng = this.range(exelApp, rowNum, column);
                rng.NumberFormat = "@";
                rng.Value2 = this.SilN2;
                column++;

                rng = this.range(exelApp, rowNum, column);
                rng.NumberFormat = "@";
                rng.Value2 = this.Equipment;
                column++;

                rng = this.range(exelApp, rowNum, column);
                rng.NumberFormat = "@";
                rng.Value2 = this.User;
                column++;
            }
        }

        public void PairIdentityToExcel(Excel.Application exelApp, Excel.Worksheet sheet, int rowNum, ref int column)
        {
            //вывод строки с идентификационными данными Pair
            if ((exelApp != null) || (sheet != null))
            {
                Range rng = null;
                rng = this.range(exelApp, rowNum, column);
                rng.NumberFormat = "@";
                rng.Value2 = this.Equipment;
                column++;

                rng = this.range(exelApp, rowNum, column);
                rng.NumberFormat = "@";
                rng.Value2 = this.User;
                column++;
            }
        }

        public void ColumnsHeaderToExcel(Excel.Application exelApp, Excel.Worksheet sheet, ref int rowNum, int column)
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
                    rng.Value2 = this.Rows[1][columnIndex];
                    rng.HorizontalAlignment = -4108;
                }

                rowNum += 3;
            }
        }

        public void BodyToExcel(Excel.Application exelApp, Excel.Worksheet sheet, ref int rowNum, ref int column)
        {
            //вывод значений условий и параметров
            if ((exelApp != null) || (sheet != null))
            {
                Range rng = null;

                for (int columnIndex = 0; columnIndex <= this.Columns.Count - 1; columnIndex++)
                {
                    //выводим значение параметра
                    rng = this.range(exelApp, rowNum, column + columnIndex);
                    rng.Value2 = this.Rows[0][columnIndex];
                    rng.HorizontalAlignment = -4108;
                }

                rowNum++;
                column += this.Columns.Count;
            }
        }
    }

    public enum TemperatureCondition
    {
        None = 0,
        RT = 1,
        TM = 2
    }
}
