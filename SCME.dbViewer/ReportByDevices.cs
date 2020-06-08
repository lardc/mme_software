using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCME.Types.Profiles;
using Excel = Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Excel;
using System.Collections;
using SCME.dbViewer.CustomControl;
using System.Globalization;

namespace SCME.dbViewer.ForParameters
{
    public class ReportRecord
    {
        public ReportRecord(ReportData owner, DataRow row, DataGridSqlResult dataGrid)
        {
            this.Owner = owner;
            this.Row = row;
            this.DataGrid = dataGrid;
        }

        public ReportData Owner { get; set; }
        public DataRow Row { get; set; }
        public DataGridSqlResult DataGrid { get; set; }

        public string ProfileName
        {
            get
            {
                int columnIndex = this.Row.Table.Columns.IndexOf(Constants.ProfileName);

                return this.Row[columnIndex].ToString();
            }
        }

        public string ProfileBody
        {
            get
            {
                return ProfileRoutines.ProfileBodyByProfileName(this.ProfileName);
            }
        }

        public string GroupName
        {
            get
            {
                int columnIndex = this.Row.Table.Columns.IndexOf(Constants.GroupName);

                return this.Row[columnIndex].ToString();
            }
        }

        public string Code
        {
            get
            {
                int columnIndex = this.Row.Table.Columns.IndexOf(Constants.Code);

                return this.Row[columnIndex].ToString();
            }
        }

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

        public string Item
        {
            get
            {
                int columnIndex = this.Row.Table.Columns.IndexOf(Constants.Item);

                return this.Row[columnIndex].ToString();
            }
        }

        public string MmeCode
        {
            get
            {
                int columnIndex = this.Row.Table.Columns.IndexOf(Constants.MmeCode);

                return this.Row[columnIndex].ToString();
            }
        }

        public string PairMmeCode
        {
            get
            {
                int columnIndex = this.Row.Table.Columns.IndexOf(Routines.NameOfHiddenColumn(Constants.MmeCode));

                return this.Row[columnIndex].ToString();
            }
        }

        public DateTime Ts
        {
            get
            {
                int columnIndex = this.Row.Table.Columns.IndexOf(Constants.Ts);

                return DateTime.Parse(this.Row[columnIndex].ToString());
            }
        }

        public DateTime PairTs
        {
            get
            {
                int columnIndex = this.Row.Table.Columns.IndexOf(Routines.NameOfHiddenColumn(Constants.Ts));

                return DateTime.Parse(this.Row[columnIndex].ToString());
            }
        }

        public int? DeviceClass
        {
            get
            {
                int columnIndex = this.Row.Table.Columns.IndexOf(Constants.DeviceClass);
                var value = this.Row[columnIndex];

                return (value == DBNull.Value) ? null : (int?)int.Parse(value.ToString());
            }
        }

        public string Status
        {
            get
            {
                int columnIndex = this.Row.Table.Columns.IndexOf(Constants.Status);

                return this.Row[columnIndex].ToString();
            }
        }

        public string CodeOfNonMatch
        {
            get
            {
                int columnIndex = this.Row.Table.Columns.IndexOf(Constants.CodeOfNonMatch);

                return this.Row[columnIndex].ToString();
            }

        }

        public string ColumnsSignature
        {
            get
            {
                //формируем список столбцов, в которых содержатся данные
                //начинаем с наименования ПЗ и тела профиля для возможности выполнения желаемой сортировки записей
                string result = string.Concat(this.GroupName, this.ProfileBody);

                //наименования столбцов реквизитов - идентификаторов (любой столбец не являющийся conditions/parameters) должны попасть в возвращаемый результат не зависимо от того, хранит он значение или оно равно DBNull.Value
                for (int i = 0; i <= this.Row.Table.Columns.Count - 1; i++)
                {
                    switch (i >= this.DataGrid.FirstCPColumnIndexInDataTable1)
                    {
                        case true:
                            //имеем дело с conditions/parameters - наименование столбца попадает в выходной результат только при условии наличия данных в этом столбце текущей записи
                            if (this.Row[i] != DBNull.Value)
                                result = string.Concat(result, this.Row.Table.Columns[i].ToString());
                            break;

                        default:
                            //имеем дело с простым реквизитом, наименование столбца должно попасть в возвращаемый результат не зависимо от того, хранит он значение или оно равно DBNull.Value
                            result = string.Concat(result, this.Row.Table.Columns[i].ToString());
                            break;
                    }
                }

                return result;
            }
        }

        private string TemperatureDelimeter
        {
            get
            {
                return "\n";
            }
        }

        private string ParseColumnName(string columnName, out string temperatureCondition)
        {
            //первые два символа принятого columnName всегда указывают на температурный режим
            //если принятый columnName не содержит описания температурного режима - данная реализация возвращает null
            const int cTemperatureConditionStart = 0;
            const int cTemperatureConditionCount = 2;

            temperatureCondition = columnName.Substring(cTemperatureConditionStart, cTemperatureConditionCount);

            //проверяем что мы считали значение температурного режима
            TemperatureCondition tc;
            if (Enum.TryParse(temperatureCondition, true, out tc) && (Enum.IsDefined(typeof(TemperatureCondition), temperatureCondition)))
            {
                //мы считали корректное описание температурного режима
                //имя условия/параметра стоит за разделителем Constants.cNameSeparator
                int startNameIndex = columnName.IndexOf(Constants.cNameSeparator);

                return (startNameIndex == -1) ? null : columnName.Substring(startNameIndex + Constants.cNameSeparator.Length);
            }
            else
            {
                //мы не смогли считать описание температурного режима
                temperatureCondition = null;

                return null;
            }
        }

        private string NrmByColumnName(string columnName)
        {
            //считывает описание норм для значения измеряемого параметра с именем columnName
            string result = null;

            string nameOfNrmMin = Routines.NameOfNrmMinParametersColumn(columnName);
            int indexOfNrmMin = this.Row.Table.Columns.IndexOf(nameOfNrmMin);
            if (indexOfNrmMin != -1)
            {
                string nrmMin = this.Row[indexOfNrmMin].ToString();
                result = (nrmMin == string.Empty) ? string.Empty : string.Concat(nrmMin, "<x");
            }

            if (result == null)
            {
                string nameOfNrmMax = Routines.NameOfNrmMaxParametersColumn(columnName);
                int indexOfNrmMax = this.Row.Table.Columns.IndexOf(nameOfNrmMax);
                if (indexOfNrmMax != -1)
                {
                    string nrmMax = this.Row[indexOfNrmMax].ToString();
                    result = (nrmMax == string.Empty) ? string.Empty : string.Concat("x≤", nrmMax);
                }
            }

            return result;
        }

        private bool PairExists()
        {
            //отвечает на вопрос о наличии набора данных Pair в принятой this.Row
            string nameOfPairDevID = Routines.NameOfHiddenColumn(Constants.DevID);
            int indexOfPairDevID = this.Row.Table.Columns.IndexOf(nameOfPairDevID);

            if (indexOfPairDevID != -1)
                return (this.Row[indexOfPairDevID] != DBNull.Value);

            return false;
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

        private Range range(Excel.Application exelApp, int rowNum, int columnNum)
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

        private Range range(Excel.Application exelApp, int rowNumBeg, int columnNumBeg, int rowNumEnd, int columnNumEnd)
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
            if ((exelApp != null) && (sheet != null))
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

        public void TopHeaderToExcel(Excel.Application exelApp, Excel.Worksheet sheet, ref int rowNum)
        {
            //выводим верхнюю часть заголовка
            if ((exelApp != null) && (sheet != null))
            {
                int column = 1;
                Range rng = null;

                rng = this.range(exelApp, rowNum, column, rowNum, column + 1);
                rng.Merge();
                rng.Value2 = "ПРОТОКОЛ ИСПЫТАНИЙ";
                rng.HorizontalAlignment = -4108;
                column += 2;

                //выводим тело профиля
                rng = this.range(exelApp, rowNum, column, rowNum, column + 3);
                rng.Merge();
                rng.Value2 = this.ProfileBody;
                rng.HorizontalAlignment = -4108;
                rowNum++;
                column = 1;

                rng = this.range(exelApp, rowNum, column, rowNum, column + 1);
                rng.Merge();
                rng.Value2 = "Код ТМЦ";
                rng.HorizontalAlignment = -4108;
                column += 2;

                rng = this.range(exelApp, rowNum, column, rowNum, column + 3);
                rng.Merge();
                rng.Value2 = this.Item;
                rng.HorizontalAlignment = -4108;
                rowNum++;
                column = 1;

                rng = this.range(exelApp, rowNum, column, rowNum, column + 1);
                rng.Merge();
                rng.Value2 = "№ ПЗ";
                rng.HorizontalAlignment = -4108;
                column += 2;

                rng = this.range(exelApp, rowNum, column, rowNum, column + 3);
                rng.Merge();
                rng.Value2 = this.GroupName;
                rng.HorizontalAlignment = -4108;
                rowNum++;
            }
        }

        public void Tc1HeaderToExcel(Excel.Application exelApp, Excel.Worksheet sheet, ref int rowNum, ref int column)
        {
            //вывод наименований столбцов условий и параметров изделия
            if ((exelApp != null) && (sheet != null))
            {
                Range rng = null;
                int columnsCount = 0;

                //идём по столбцам условий и параметров 1-го температурного режима
                for (int columnIndex = this.DataGrid.FirstCPColumnIndexInDataTable1; columnIndex <= this.DataGrid.LastCPColumnIndexInDataTable1; columnIndex++)
                {
                    if (this.Row[columnIndex] != DBNull.Value)
                    {
                        DataColumn col = this.Row.Table.Columns[columnIndex];

                        string temperatureCondition;
                        string trueName = this.ParseColumnName(col.ColumnName, out temperatureCondition);

                        if (trueName != null)
                        {
                            if ((this.Owner.FListOfBannedNamesForUseInReport.Use(trueName)) && (!trueName.Contains(Constants.HiddenMarker)))
                            {
                                //выводим температуру при которой выполнено измерение данного параметра
                                int columnIndexInExcel = column + columnsCount;

                                rng = this.range(exelApp, rowNum, columnIndexInExcel);
                                rng.Value2 = temperatureCondition;
                                rng.HorizontalAlignment = -4108;

                                //выводим имя параметра
                                rng = this.range(exelApp, rowNum + 1, columnIndexInExcel);
                                rng.Value2 = trueName;
                                rng.HorizontalAlignment = -4108;
                                rng.Font.Bold = true;

                                //выводим единицу измерения                                
                                string nameOfUnitMeasure = Routines.NameOfUnitMeasure(this.Row.Table.Columns[columnIndex].ColumnName);
                                int indexOfUnitMeasure = this.Row.Table.Columns.IndexOf(nameOfUnitMeasure);

                                if (indexOfUnitMeasure != -1)
                                {
                                    rng = this.range(exelApp, rowNum + 2, columnIndexInExcel);
                                    rng.Value2 = this.Row[indexOfUnitMeasure];
                                    rng.HorizontalAlignment = -4108;
                                }

                                //выводим норму
                                string nrmDescr = this.NrmByColumnName(this.Row.Table.Columns[columnIndex].ColumnName);

                                if (nrmDescr != null)
                                {
                                    rng = this.range(exelApp, rowNum + 3, columnIndexInExcel);
                                    rng.Value2 = nrmDescr;
                                    rng.HorizontalAlignment = -4108;
                                    rng.Font.Bold = true;
                                }

                                //считаем сколько мы вывели новых столбцов
                                columnsCount++;
                            }
                        }
                    }
                }

                rowNum += 3;
                column += columnsCount;
            }
        }

        public void HeaderToExcel(Excel.Application exelApp, Excel.Worksheet sheet, ref int rowNum, int column)
        {
            //вывод общего заголовка
            if ((exelApp != null) && (sheet != null))
            {
                Range rng = null;

                rng = this.range(exelApp, rowNum, column);
                rng.Value2 = "Норма";
                rng.HorizontalAlignment = -4108;
                rowNum++;
                column--;

                rng = this.range(exelApp, rowNum, column);
                rng.Value2 = "№ ППЭ";
                rng.HorizontalAlignment = -4108;
                column++;

                rng = this.range(exelApp, rowNum, column);
                rng.Value2 = "Класс";
                rng.HorizontalAlignment = -4108;
                column++;
            }
        }

        public void Tc2HeaderToExcel(Excel.Application exelApp, Excel.Worksheet sheet, int rowNum, ref int column)
        {
            //вывод наименований столбцов условий и параметров изделия второго температурного режима
            if ((exelApp != null) && (sheet != null) && (this.Row != null))
            {
                Range rng = null;
                int columnsCount = 0;

                //идём по столбцам второго температурного режима
                int firstIndexOfCP2 = this.DataGrid.LastCPColumnIndexInDataTable1 + 1;

                for (int columnIndex = firstIndexOfCP2; columnIndex <= this.Row.Table.Columns.Count - 1; columnIndex++)
                {
                    if (this.Row[columnIndex] != DBNull.Value)
                    {
                        DataColumn col = this.Row.Table.Columns[columnIndex];

                        string temperatureCondition;
                        string trueName = this.ParseColumnName(col.ColumnName, out temperatureCondition);

                        if (trueName != null)
                        {
                            if ((this.Row[columnIndex] != DBNull.Value) && (this.Owner.FListOfBannedNamesForUseInReport.Use(trueName)) && (!trueName.Contains(Constants.HiddenMarker)))
                            {
                                //выводим температуру при которой выполнено измерение данного параметра
                                int columnIndexInExcel = column + columnsCount;

                                rng = this.range(exelApp, rowNum, columnIndexInExcel);
                                rng.Value2 = temperatureCondition;
                                rng.HorizontalAlignment = -4108;

                                //выводим имя параметра
                                rng = this.range(exelApp, rowNum + 1, columnIndexInExcel);
                                rng.Value2 = trueName;
                                rng.HorizontalAlignment = -4108;
                                rng.Font.Bold = true;

                                //выводим единицу измерения                                
                                string nameOfUnitMeasure = Routines.NameOfUnitMeasure(this.Row.Table.Columns[columnIndex].ColumnName);
                                int indexOfUnitMeasure = this.Row.Table.Columns.IndexOf(nameOfUnitMeasure);

                                if (indexOfUnitMeasure != -1)
                                {
                                    rng = this.range(exelApp, rowNum + 2, columnIndexInExcel);
                                    rng.Value2 = this.Row[indexOfUnitMeasure];
                                    rng.HorizontalAlignment = -4108;
                                }

                                //выводим норму
                                string nrmDescr = this.NrmByColumnName(this.Row.Table.Columns[columnIndex].ColumnName);

                                if (nrmDescr != null)
                                {
                                    rng = this.range(exelApp, rowNum + 3, columnIndexInExcel);
                                    rng.Value2 = nrmDescr;
                                    rng.HorizontalAlignment = -4108;
                                    rng.Font.Bold = true;
                                }

                                //считаем сколько мы вывели новых столбцов
                                columnsCount++;
                            }
                        }
                    }
                }

                rowNum += 3;
                column += columnsCount;
            }
        }

        public void StatusHeaderToExcel(Excel.Application exelApp, Excel.Worksheet sheet, int rowNum, int column)
        {
            //вывод наименований столбцов статуса и причины
            if ((exelApp != null) && (sheet != null))
            {
                Range rng = null;

                rng = this.range(exelApp, rowNum, column);
                rng.Value2 = "Статус";
                rng.HorizontalAlignment = -4108;
                column++;

                rng = this.range(exelApp, rowNum, column);
                rng.Value2 = "Код НП";
                rng.HorizontalAlignment = -4108;
            }
        }

        public void IdentityToExcel(Excel.Application exelApp, Excel.Worksheet sheet, int rowNum, ref int column)
        {
            //вывод идентификационные данные
            if ((exelApp != null) && (sheet != null))
            {
                Range rng = null;

                //выводим идентификационные данные   
                rng = this.range(exelApp, rowNum, column);
                rng.NumberFormat = "0";
                rng.Value2 = this.CodeSimple;
                column++;

                rng = this.range(exelApp, rowNum, column);
                rng.NumberFormat = "0";
                rng.Value2 = this.DeviceClass;
                rng.HorizontalAlignment = -4108;
                column++;
            }
        }

        public void Tc1BodyToExcel(Excel.Application exelApp, Excel.Worksheet sheet, ListOfCalculatorsMinMax listOfCalculatorsMinMax, int rowNum, ref int column)
        {
            //вывод значений условий и параметров 1-го температурного режима
            if ((exelApp != null) && (sheet != null))
            {
                Range rng = null;
                int columnsCount = 0;

                for (int columnIndex = this.DataGrid.FirstCPColumnIndexInDataTable1; columnIndex <= this.DataGrid.LastCPColumnIndexInDataTable1; columnIndex++)
                {
                    string name = this.Row.Table.Columns[columnIndex].ColumnName;

                    string temperatureCondition;
                    string trueName = this.ParseColumnName(name, out temperatureCondition);

                    if ((this.Row[columnIndex] != DBNull.Value) && (this.Owner.FListOfBannedNamesForUseInReport.Use(trueName)) && (trueName != null) && (!trueName.Contains(Constants.HiddenMarker)))
                    {
                        //выводим значение параметра
                        string value = this.Row[columnIndex].ToString().TrimEnd();

                        if (value != string.Empty)
                        {
                            int columnIndexInExcel = column + columnsCount;

                            //вычисляем значения min/max для определённых в listOfCalculatorsMinMax параметров
                            string nameOfUnitMeasure = Routines.NameOfUnitMeasure(this.Row.Table.Columns[columnIndex].ColumnName);
                            int indexOfUnitMeasure = this.Row.Table.Columns.IndexOf(nameOfUnitMeasure);
                            string um = this.Row[indexOfUnitMeasure].ToString();
                            listOfCalculatorsMinMax.Calc(columnIndexInExcel, name, um, value);

                            rng = this.range(exelApp, rowNum, columnIndexInExcel);

                            bool isDouble;
                            int iValue;
                            double dValue;

                            if (Routines.IsInteger(value, out iValue, out isDouble, out dValue))
                            {
                                //имеем дело с Int
                                rng.NumberFormat = "0";
                                rng.Value2 = iValue;
                            }
                            else
                            {
                                if (isDouble)
                                {
                                    //имеем дело с Double
                                    rng.NumberFormat = "0.00";
                                    rng.Value2 = dValue;
                                }
                                else
                                {
                                    //имеем дело не с Int и не с Double - со строкой
                                    rng.NumberFormat = "@";
                                    rng.Value2 = value;
                                }
                            }

                            rng.HorizontalAlignment = -4108;

                            //проверяем входит ли выведенное значение параметра в норматив
                            string columnName = this.Row.Table.Columns[columnIndex].ColumnName;
                            if (this.DataGrid.IsInNrm(this.Row, columnName) == CheckNrmStatus.Defective)
                            {
                                //выведенное значение за пределами норм - красим его
                                rng.Interior.Pattern = 1; //xlSolid
                                rng.Interior.PatternColorIndex = -4105; //xlAutomatic
                                rng.Interior.Color = 255;
                                rng.Font.Bold = true;
                            }
                            else
                            {
                                //если выведенное значение принадлежит параметру из списка важных для пользователя параметров - красим его серым
                                if (this.Owner.FListOfImportantNamesInReport.Contains(columnName))
                                {
                                    rng.Interior.Pattern = 1; //xlSolid
                                    rng.Interior.PatternColorIndex = -4105; //xlAutomatic
                                    rng.Interior.Color = Int32.Parse("EEEEEE", NumberStyles.HexNumber); //#EEEEEE - цвет в 16-тиричном формате
                                }
                            }

                            columnsCount++;
                        }
                    }
                }

                column += columnsCount;
            }
        }

        public void Tc2BodyToExcel(Excel.Application exelApp, Excel.Worksheet sheet, ListOfCalculatorsMinMax listOfCalculatorsMinMax, int rowNum, ref int column)
        {
            //вывод значений условий и параметров 2-го температурного режима
            if ((exelApp != null) && (sheet != null))
            {
                Range rng = null;
                int columnsCount = 0;

                int firstCPColumnIndexInDataTable2 = this.DataGrid.LastCPColumnIndexInDataTable1 + 1;

                for (int columnIndex = firstCPColumnIndexInDataTable2; columnIndex <= this.Row.Table.Columns.Count - 1; columnIndex++)
                {
                    string name = this.Row.Table.Columns[columnIndex].ColumnName;

                    string temperatureCondition;
                    string trueName = this.ParseColumnName(name, out temperatureCondition);

                    if ((this.Row[columnIndex] != DBNull.Value) && (this.Owner.FListOfBannedNamesForUseInReport.Use(trueName)) && (trueName != null) && (!trueName.Contains(Constants.HiddenMarker)))
                    {
                        //выводим значение параметра
                        string value = this.Row[columnIndex].ToString().TrimEnd();

                        if (value != string.Empty)
                        {
                            int columnIndexInExcel = column + columnsCount;

                            //вычисляем значения min/max для определённых в listOfCalculatorsMinMax параметров
                            string nameOfUnitMeasure = Routines.NameOfUnitMeasure(this.Row.Table.Columns[columnIndex].ColumnName);
                            int indexOfUnitMeasure = this.Row.Table.Columns.IndexOf(nameOfUnitMeasure);
                            string um = this.Row[indexOfUnitMeasure].ToString();
                            listOfCalculatorsMinMax.Calc(columnIndexInExcel, name, um, value);

                            rng = this.range(exelApp, rowNum, columnIndexInExcel);

                            bool isDouble;
                            int iValue;
                            double dValue;

                            if (Routines.IsInteger(value, out iValue, out isDouble, out dValue))
                            {
                                //имеем дело с Int
                                rng.NumberFormat = "0";
                                rng.Value2 = iValue;
                            }
                            else
                            {
                                if (isDouble)
                                {
                                    //имеем дело с Double
                                    rng.NumberFormat = "0.00";
                                    rng.Value2 = dValue;
                                }
                                else
                                {
                                    //имеем дело не с Int и не с Double - со строкой
                                    rng.NumberFormat = "@";
                                    rng.Value2 = value;
                                }
                            }

                            rng.HorizontalAlignment = -4108;

                            //проверяем входит ли выведенное значение параметра в норматив
                            string columnName = this.Row.Table.Columns[columnIndex].ColumnName;
                            if (this.DataGrid.IsInNrm(this.Row, columnName) == CheckNrmStatus.Defective)
                            {
                                //выведенное значение за пределами норм - красим его
                                rng.Interior.Pattern = 1; //xlSolid
                                rng.Interior.PatternColorIndex = -4105; //xlAutomatic
                                rng.Interior.Color = 255;
                                rng.Font.Bold = true;
                            }
                            else
                            {
                                //если выведенное значение принадлежит параметру из списка важных для пользователя параметров - красим его серым
                                if (this.Owner.FListOfImportantNamesInReport.Contains(columnName))
                                {
                                    rng.Interior.Pattern = 1; //xlSolid
                                    rng.Interior.PatternColorIndex = -4105; //xlAutomatic
                                    rng.Interior.Color = Int32.Parse("EEEEEE", NumberStyles.HexNumber); //#EEEEEE - цвет в 16-тиричном формате
                                }
                            }

                            columnsCount++;
                        }
                    }
                }

                column += columnsCount;
            }
        }

        public int StatusToExcel(Excel.Application exelApp, Excel.Worksheet sheet, int rowNum, int column, ref bool? isStatusOK)
        {
            //считываем значение итогового статуса
            string resultStatus = this.Status;

            if (resultStatus == Constants.GoodSatatus)
                isStatusOK = true;
            else
                isStatusOK = (resultStatus == Constants.FaultSatatus) ? false : (bool?)null;

            //в this.CodeOfNonMatch имеем уже слитые в одну строку коды НП
            this.statusToExcel(exelApp, sheet, resultStatus, this.CodeOfNonMatch, rowNum, column);

            return (column + 1);
        }

        public int statusToExcel(Excel.Application exelApp, Excel.Worksheet sheet, string status, string codeOfNonMatch, int rowNum, int column)
        {
            //вывод значений столбцов статуса и кода НП
            if ((exelApp != null) && (sheet != null))
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
                int iCodeOfNonMatch;

                if (int.TryParse(codeOfNonMatch, out iCodeOfNonMatch))
                {
                    rng.NumberFormat = "0";
                    rng.Value2 = iCodeOfNonMatch;
                }
                else
                {
                    rng.NumberFormat = "@";
                    rng.Value2 = codeOfNonMatch;
                }

                rng.HorizontalAlignment = -4108;
            }

            return column;
        }

        public void QtyReleasedByGroupNameToExcel(Excel.Application exelApp, Excel.Worksheet sheet, ref int rowNum, string groupName, int? qtyReleased)
        {
            if ((exelApp != null) && (sheet != null))
            {
                Range rng = null;

                rng = this.range(exelApp, rowNum, 2);
                rng.Value2 = "Запущено";
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

        public void QtyOKFaultToExcel(Excel.Application exelApp, Excel.Worksheet sheet, int rowNum, int totalCount, int statusUnknownCount, int statusFaultCount, int statusOKCount)
        {
            if ((exelApp != null) && (sheet != null))
            {
                Range rng = null;

                rng = this.range(exelApp, rowNum, 2);
                rng.Value2 = "Кол-во";
                rng.HorizontalAlignment = -4108;

                rng = this.range(exelApp, rowNum, 3);
                rng.Value2 = totalCount;
                rng.NumberFormat = "0";
                rng.HorizontalAlignment = -4108;

                rng = this.range(exelApp, rowNum, 4);
                rng.Value2 = "шт.";
                rng.HorizontalAlignment = -4108;
                rowNum++;

                rng = this.range(exelApp, rowNum, 2);
                rng.Value2 = "Годных";
                rng.HorizontalAlignment = -4108;

                rng = this.range(exelApp, rowNum, 3);
                rng.Value2 = statusOKCount;
                rng.NumberFormat = "0";
                rng.HorizontalAlignment = -4108;

                rng = this.range(exelApp, rowNum, 4);
                rng.Value2 = "шт.";
                rng.HorizontalAlignment = -4108;
                rowNum++;

                rng = this.range(exelApp, rowNum, 2);
                rng.Value2 = "Fault";
                rng.HorizontalAlignment = -4108;

                rng = this.range(exelApp, rowNum, 3);
                rng.Value2 = statusFaultCount;
                rng.NumberFormat = "0";
                rng.HorizontalAlignment = -4108;

                rng = this.range(exelApp, rowNum, 4);
                rng.Value2 = "шт.";
                rng.HorizontalAlignment = -4108;
                rowNum++;

                rng = this.range(exelApp, rowNum, 2);
                rng.Value2 = "Неопределённых";
                rng.HorizontalAlignment = -4108;

                rng = this.range(exelApp, rowNum, 3);
                rng.Value2 = statusUnknownCount;
                rng.NumberFormat = "0";
                rng.HorizontalAlignment = -4108;

                rng = this.range(exelApp, rowNum, 4);
                rng.Value2 = "шт.";
                rng.HorizontalAlignment = -4108;
                rowNum++;

                rng = this.range(exelApp, rowNum, 2);
                rng.Value2 = "Сформир.";
                rng.HorizontalAlignment = -4108;

                rng = this.range(exelApp, rowNum, 3);
                rng.Value2 = Environment.UserName;
                rng.HorizontalAlignment = -4108;

                rng = this.range(exelApp, rowNum, 4);
                rng.Value2 = DateTime.Today.ToString("dd.MM.yyyy");
                rng.NumberFormat = "dd/mm/yyyy;@";
                rng.HorizontalAlignment = -4108;
            }
        }

        public void ListOfCalculatorsMinMaxToExcel(Excel.Application exelApp, Excel.Worksheet sheet, ref int rowNum, ListOfCalculatorsMinMax listOfCalculatorsMinMax)
        {
            //вывод вычисленных значений min/max из listOfCalculatorMinMax в Excel
            if ((exelApp != null) && (sheet != null) && (listOfCalculatorsMinMax != null))
            {
                Range rng = null;

                foreach (CalculatorMinMax calculator in listOfCalculatorsMinMax)
                {
                    int column = calculator.Column;

                    //если из calculator выводить нечего, то ничего не выводим
                    if (column != -1)
                    {
                        rng = this.range(exelApp, rowNum, column);
                        rng.Value2 = string.Format("{0}\n{1}", "min/max", calculator.Name);
                        rng.HorizontalAlignment = -4108;

                        //выводим единицу измерения
                        rng = this.range(exelApp, rowNum + 1, column);
                        rng.NumberFormat = "@";
                        rng.Value2 = calculator.Um;
                        rng.HorizontalAlignment = -4108;

                        //выводим значение минимума
                        rng = this.range(exelApp, rowNum + 2, column);
                        rng.NumberFormat = "0.00";
                        rng.Value2 = calculator.MinValue;
                        rng.HorizontalAlignment = -4108;

                        //выводим значение максимума
                        rng = this.range(exelApp, rowNum + 3, column);
                        rng.NumberFormat = "0.00";
                        rng.Value2 = calculator.MaxValue;
                        rng.HorizontalAlignment = -4108;
                    }
                }

                rowNum += 4;
            }
        }
    }

    public class ReportData : List<ReportRecord>
    {
        private Excel.Application exelApp = null;
        private Excel.Worksheet sheet = null;

        public ListOfBannedNamesForUseInReport FListOfBannedNamesForUseInReport = new ListOfBannedNamesForUseInReport();
        public ListOfImportantNamesInReport FListOfImportantNamesInReport = new ListOfImportantNamesInReport();

        public ReportData(System.Data.DataTable source, DataGridSqlResult dataGrid)
        {
            //запоминаем ссылки на каждую row из принятого source
            for (int i = 0; i <= source.Rows.Count - 1; i++)
            {
                ReportRecord record = new ReportRecord(this, source.Rows[i], dataGrid);
                this.Add(record);
            }
        }

        public ReportData(List<ReportRecord> source) : base(source)
        {
        }

        public int? QtyReleasedByGroupName(string groupName, SqlConnection connection)
        {
            //возвращает количество изделий, запущенных по ЗП groupName
            SqlCommand selectCommand = new SqlCommand("SELECT dbo.SL_Qty_ReleasedByJob(@Job)", connection);
            SqlParameter Job = new SqlParameter("@Job", SqlDbType.NVarChar);

            selectCommand.Parameters.Add("@Job", SqlDbType.NVarChar, 20);
            selectCommand.Parameters["@Job"].Value = groupName;

            if (!connection.State.Equals(ConnectionState.Open))
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
            this[0].QtyReleasedByGroupNameToExcel(exelApp, sheet, ref rowNum, groupName, qtyReleased);
        }

        public void QtyOKFaultToExcel(Excel.Application exelApp, Excel.Worksheet sheet, int rowNum, int totalCount, int statusUnknownCount, int statusFaultCount, int statusOKCount)
        {
            this[0].QtyOKFaultToExcel(exelApp, sheet, rowNum, totalCount, statusUnknownCount, statusFaultCount, statusOKCount);
        }

        public void ListOfCalculatorsMinMaxToExcel(Excel.Application exelApp, Excel.Worksheet sheet, ref int rowNum, ListOfCalculatorsMinMax listOfCalculatorsMinMax)
        {
            this[0].ListOfCalculatorsMinMaxToExcel(exelApp, sheet, ref rowNum, listOfCalculatorsMinMax);
        }

        public void ToExcel(SqlConnection connection, bool visibleAfterBuilding)
        {
            this.exelApp = new Microsoft.Office.Interop.Excel.Application();
            this.exelApp.Visible = false;

            try
            {
                this.exelApp.SheetsInNewWorkbook = 2;
                Excel.Workbook workBook = this.exelApp.Workbooks.Add(Type.Missing);
                this.exelApp.DisplayAlerts = false;
                this.sheet = (Excel.Worksheet)this.exelApp.Worksheets.get_Item(1);
                this.sheet.Name = "Протокол испытаний";

                //здесь будем хранить флаг о неизменности ПЗ во всём выведенном отчёте
                bool groupNameChanged = false;

                //здесь будем хранить предыдущий просмотренный в цикле GroupName
                string lastGroupName = null;

                string lastUsedColumnsSignature = string.Empty;

                //счётчики успешного и не успешного прохождения тестов
                int statusOKCount = 0;
                int statusFaultCount = 0;
                int statusUnknownCount = 0;
                int totalCount = 0;

                int lastUsedColumn = 0;

                int rowNum = 1;
                int rowNumBeg;
                int columnEnd = 0;

                //храним здесь сколько шапок было выведено
                int needHeaderCount = 0;

                ListOfCalculatorsMinMax listOfCalculatorsMinMax = new ListOfCalculatorsMinMax();

                foreach (ReportRecord p in this)
                {
                    string currentGroupName = p.GroupName;

                    if ((!groupNameChanged) && (lastGroupName != null))
                        groupNameChanged = (currentGroupName != lastGroupName);

                    int column = 1;

                    bool needHeader = ((lastUsedColumnsSignature == string.Empty) || (lastUsedColumnsSignature != p.ColumnsSignature));

                    lastGroupName = currentGroupName;

                    //выводим шапку если имеет место смена списка выведенных столбцов
                    int headerRowNum = rowNum + 2;
                    rowNumBeg = rowNum;

                    if (needHeader)
                    {
                        columnEnd = column + 2;

                        //выводим самую верхнюю часть шапки
                        p.TopHeaderToExcel(this.exelApp, this.sheet, ref rowNum);

                        //выводим шапку столбцов условий и измеренных параметров 1-го температурного режима
                        p.Tc1HeaderToExcel(this.exelApp, this.sheet, ref rowNum, ref columnEnd);

                        //выводим шапку идентификационных данных
                        p.HeaderToExcel(this.exelApp, this.sheet, ref rowNum, column + 1);

                        //выводим шапку условий и измеренных параметров 2-го температурного режима
                        p.Tc2HeaderToExcel(this.exelApp, this.sheet, rowNumBeg + 3, ref columnEnd);

                        //выводим шапку статуса
                        p.StatusHeaderToExcel(this.exelApp, this.sheet, rowNum, columnEnd);

                        needHeaderCount++;
                        rowNum++;
                    }

                    //выводим идентификационные данные
                    p.IdentityToExcel(this.exelApp, this.sheet, rowNum, ref column);

                    //выводим тело 1-го температурного режима
                    p.Tc1BodyToExcel(this.exelApp, this.sheet, listOfCalculatorsMinMax, rowNum, ref column);

                    //выводим тело pair
                    p.Tc2BodyToExcel(this.exelApp, this.sheet, listOfCalculatorsMinMax, rowNum, ref column);

                    //выводим статус
                    bool? isStatusOK = null;
                    lastUsedColumn = p.StatusToExcel(this.exelApp, this.sheet, rowNum, columnEnd, ref isStatusOK);

                    //формируем значения счётчиков неопределённого/успешного/не успешного прохождения тестов
                    if (isStatusOK == null)
                        statusUnknownCount++;
                    else
                    {
                        switch (isStatusOK)
                        {
                            case false:
                                statusFaultCount++;
                                break;

                            default:
                                statusOKCount++;
                                break;
                        }
                    }

                    //считаем сколько всего изделий просмотрено в цикле
                    totalCount++;

                    //обводим границы
                    p.SetBorders(this.exelApp, this.sheet, rowNumBeg, rowNum, lastUsedColumn);

                    //запоминаем набор столбцов, которые мы вывели
                    lastUsedColumnsSignature = p.ColumnsSignature;

                    rowNum++;
                }

                //если на протяжении всего цикла не зафиксировано изменение ПЗ - выводим кол-во запущенных ТМЦ по ПЗ. если же изменение ПЗ зафиксировано - в отчёте есть данные по разным ПЗ и запущенное кол-во не имеет смысла
                if (!groupNameChanged)
                {
                    int? qtyReleased = this.QtyReleasedByGroupName(lastGroupName, connection);

                    //выводим значения min/max для определённых в listOfCalculatorsMinMax параметров
                    if (needHeaderCount == 1)
                        this.ListOfCalculatorsMinMaxToExcel(this.exelApp, this.sheet, ref rowNum, listOfCalculatorsMinMax);

                    //выводим количество ТМЦ, запущенных по ПЗ
                    this.QtyReleasedByGroupNameToExcel(this.exelApp, this.sheet, ref rowNum, lastGroupName, qtyReleased);

                    //выводим кол-во годных/не годных
                    this.QtyOKFaultToExcel(this.exelApp, this.sheet, rowNum, totalCount, statusUnknownCount, statusFaultCount, statusOKCount);
                }

                //создаём нижний колонтитул
                this.sheet.PageSetup.RightFooter = "Лист &P Листов &N";

                this.sheet.UsedRange.EntireRow.AutoFit();
                this.sheet.UsedRange.EntireColumn.AutoFit();
                this.sheet.UsedRange.Font.Name = "Arial Narrow";

                //настраиваем вид печатного отчёта
                this.sheet.PageSetup.LeftMargin = 0;
                this.sheet.PageSetup.RightMargin = 0;
                this.sheet.PageSetup.TopMargin = 42;
                this.sheet.PageSetup.BottomMargin = 28;
                this.sheet.PageSetup.Orientation = Excel.XlPageOrientation.xlLandscape;
                this.sheet.PageSetup.PaperSize = Excel.XlPaperSize.xlPaperA4;
                this.sheet.PageSetup.Zoom = false;
                this.sheet.PageSetup.FitToPagesWide = 1;
                this.sheet.PageSetup.FitToPagesTall = false;
                this.sheet.PageSetup.ScaleWithDocHeaderFooter = true;
                this.sheet.PageSetup.AlignMarginsHeaderFooter = true;
            }
            finally
            {
                if (visibleAfterBuilding)
                {
                    this.exelApp.Visible = visibleAfterBuilding;
                    this.exelApp.WindowState = Microsoft.Office.Interop.Excel.XlWindowState.xlMaximized;
                }
            }
        }

        public void Print()
        {
            //печать уже сформированного отчёта
            this.sheet?.PrintOut(Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
        }
    }

    /*
    public class ReportData
    {
        public DataTableParameters RTData { get; set; }
        public DataTableParameters TMData { get; set; }

        public string ColumnsSignature
        {
            get
            {
                string result = string.Empty;

                switch (this.RTData == null)
                {
                    case true:
                        result = this.TMData?.ColumnsSignature;
                        break;

                    default:
                        result = (this.TMData == null) ? this.RTData?.ColumnsSignature : this.RTData?.ColumnsSignature + this.TMData?.ColumnsSignature;
                        break;
                }

                return result;
            }
        }

        public string Status
        {
            get
            {
                //чтобы вернуть OK надо, чтобы все тесты завершились с результатом OK. если хотя-бы один тест закончился с результатом Fault - вернём Fault
                string result = "Fault";

                string goodStatus = "OK";
                result = RTData?.Status;

                if (result == goodStatus)
                    result = RTData?.Status;

                return result;
            }
        }

        public string Reason
        {
            get
            {
                string result = string.Empty;

                //не формируем описания абсолютно всех проблем ибо пользователь их все читать точно не будет - ограничиваемся первой найденной проблемой
                result = RTData?.Reason;

                if (result == string.Empty)
                    result = TMData?.Reason;

                return result;
            }
        }

        public string Code
        {
            get
            {
                return (RTData == null) ? TMData.Code : RTData.Code;
            }
        }

        public void TopHeaderToExcel(Excel.Application exelApp, Excel.Worksheet sheet, ref int rowNum)
        {
            switch (this.RTData == null)
            {
                case true:
                    //TMData точно не null
                    this.TMData.TopHeaderToExcel(exelApp, sheet, ref rowNum);
                    break;

                default:
                    this.RTData.TopHeaderToExcel(exelApp, sheet, ref rowNum);
                    break;
            }
        }

        public void ColumnsHeaderToExcel(Excel.Application exelApp, Excel.Worksheet sheet, ref int rowNum, ref int column)
        {
            switch (this.RTData == null)
            {
                case true:
                    //TMData точно не null
                    this.TMData.ColumnsHeaderToExcel(exelApp, sheet, ref rowNum, ref column);
                    break;

                default:
                    this.RTData.ColumnsHeaderToExcel(exelApp, sheet, ref rowNum, ref column);
                    break;
            }
        }

        public void HeaderToExcel(Excel.Application exelApp, Excel.Worksheet sheet, ref int rowNum, int column, ref int columnEnd)
        {
            switch (this.RTData == null)
            {
                case true:
                    //TMData точно не null
                    this.TMData.HeaderToExcel(exelApp, sheet, ref rowNum, ref column, ref columnEnd);
                    break;

                default:
                    this.RTData.HeaderToExcel(exelApp, sheet, ref rowNum, ref column, ref columnEnd);
                    break;
            }
        }

        public void PairColumnsHeaderToExcel(Excel.Application exelApp, Excel.Worksheet sheet, int rowNum, ref int column)
        {
            if ((this.RTData != null) && (this.TMData != null))
                this.TMData.ColumnsHeaderToExcel(exelApp, sheet, ref rowNum, ref column);
        }

        public void PairHeaderToExcel(Excel.Application exelApp, Excel.Worksheet sheet, int rowNum, ref int column)
        {
            if ((this.RTData != null) && (this.TMData != null))
                this.TMData.PairHeaderToExcell(exelApp, sheet, rowNum, ref column);
        }


        public void StatusHeaderToExcel(Excel.Application exelApp, Excel.Worksheet sheet, int rowNum, int column)
        {
            switch (this.RTData == null)
            {
                case true:
                    //TMData точно не null
                    this.TMData.StatusHeaderToExcel(exelApp, sheet, rowNum, column);
                    break;

                default:
                    this.RTData.StatusHeaderToExcel(exelApp, sheet, rowNum, column);
                    break;
            }
        }

        public int StatusToExcel(Excel.Application exelApp, Excel.Worksheet sheet, int rowNum, int column, ref bool? isStatusOK)
        {
            //статус вычисляется по холодному и горячему измерениям
            //если хотя-бы одно измерение отсутствует - возвращаем неопределённый (пустое значение) статус
            //значение статуса "Ok" выводим только если для холодного и горячего измерений имеем статусы "Ok". если хотя-бы один из статусов не "Ok" - выводим "Fault"
            string rtStatus = this.RTData?.Status.Trim();
            string tmStatus = this.TMData?.Status.Trim();

            string status = string.Empty;

            //только если оба измерения завершились успешно - возвратим статус "OK"
            if ((rtStatus == "OK") && (tmStatus == "OK"))
                status = "OK";
            else
            {
                //если хотя-бы одно измерение завершилось не успешно - возвратим статус "Fault"
                if ((rtStatus == "Fault") || (tmStatus == "Fault"))
                    status = "Fault";
                else status = string.Empty;
            }

            if (status == "OK")
            {
                isStatusOK = true;
            }
            else
            {
                if (status == "Fault")
                {
                    isStatusOK = false;
                }
                else
                    isStatusOK = null;
            }

            string rtCodeOfNonMatch = (this.RTData?.CodeOfNonMatch == null) ? string.Empty : this.RTData?.CodeOfNonMatch;
            string tmCodeOfNonMatch = (this.TMData?.CodeOfNonMatch == null) ? string.Empty : this.TMData?.CodeOfNonMatch;

            string codeOfNonMatch = rtCodeOfNonMatch;

            if (tmCodeOfNonMatch != string.Empty)
            {
                if (codeOfNonMatch != string.Empty)
                    codeOfNonMatch += ", ";

                codeOfNonMatch += tmCodeOfNonMatch;
            }

            int result;

            switch (this.RTData == null)
            {
                case true:
                    //TMData точно не null
                    result = this.TMData.StatusToExcel(exelApp, sheet, status, codeOfNonMatch, rowNum, column);
                    break;

                default:
                    result = this.RTData.StatusToExcel(exelApp, sheet, status, codeOfNonMatch, rowNum, column);
                    break;
            }

            return result;
        }

        public void CounterToExcel(Excel.Application exelApp, Excel.Worksheet sheet, int counter, int rowNum)
        {
            switch (this.RTData == null)
            {
                case true:
                    //TMData точно не null
                    this.TMData.CounterToExcel(exelApp, sheet, counter, rowNum);
                    break;

                default:
                    this.RTData.CounterToExcel(exelApp, sheet, counter, rowNum);
                    break;
            }
        }

        public void ListOfCalculatorsMinMaxToExcel(Excel.Application exelApp, Excel.Worksheet sheet, int rowNum, ListOfCalculatorsMinMax listOfCalculatorsMinMax)
        {
            switch (this.RTData == null)
            {
                case true:
                    //TMData точно не null
                    this.TMData.ListOfCalculatorsMinMaxToExcel(exelApp, sheet, rowNum, listOfCalculatorsMinMax);
                    break;

                default:
                    this.RTData.ListOfCalculatorsMinMaxToExcel(exelApp, sheet, rowNum, listOfCalculatorsMinMax);
                    break;
            }
        }

        public void IdentityToExcel(Excel.Application exelApp, Excel.Worksheet sheet, int counter, int rowNum, ref int column)
        {
            switch (this.RTData == null)
            {
                case true:
                    //TMData точно не null
                    this.TMData.IdentityToExcel(exelApp, sheet, counter, rowNum, ref column);
                    break;

                default:
                    this.RTData.IdentityToExcel(exelApp, sheet, counter, rowNum, ref column);
                    break;
            }
        }

        public void EndIdentityToExcel(Excel.Application exelApp, Excel.Worksheet sheet, int rowNum, ref int column)
        {
            switch (this.RTData == null)
            {
                case true:
                    //TMData точно не null
                    this.TMData.EndIdentityToExcel(exelApp, sheet, rowNum, ref column);
                    break;

                default:
                    this.RTData.EndIdentityToExcel(exelApp, sheet, rowNum, ref column);
                    break;
            }

        }

        public void BodyToExcel(Excel.Application exelApp, Excel.Worksheet sheet, ListOfCalculatorsMinMax listOfCalculatorsMinMax, int rowNum, ref int column)
        {
            switch (this.RTData == null)
            {
                case true:
                    //TMData точно не null
                    this.TMData.BodyToExcel(exelApp, sheet, listOfCalculatorsMinMax, rowNum, ref column);
                    break;

                default:
                    this.RTData.BodyToExcel(exelApp, sheet, listOfCalculatorsMinMax, rowNum, ref column);
                    break;
            }
        }

        public void PairBodyToExcel(Excel.Application exelApp, Excel.Worksheet sheet, ListOfCalculatorsMinMax listOfCalculatorsMinMax, int rowNum, ref int column)
        {
            if ((this.RTData != null) && (this.TMData != null))
                this.TMData.BodyToExcel(exelApp, sheet, listOfCalculatorsMinMax, rowNum, ref column);
        }

        public void PairIdentityToExcel(Excel.Application exelApp, Excel.Worksheet sheet, int rowNum, int column)
        {
            if ((this.RTData != null) && (this.TMData != null))
                this.TMData.PairIdentityToExcel(exelApp, sheet, rowNum, column);
        }

        public void SetBorders(Excel.Application exelApp, Excel.Worksheet sheet, int rowNumBeg, int rowNumEnd, int columnEnd)
        {
            DataTableParameters dtp = (this.RTData == null) ? this.TMData : this.RTData;
            dtp?.SetBorders(exelApp, sheet, rowNumBeg, rowNumEnd, columnEnd);
        }

        public int? QtyReleasedByGroupName(string lastGroupName, SqlConnection connection)
        {
            int? result;

            switch (this.RTData == null)
            {
                case true:
                    //TMData точно не null
                    result = this.TMData.QtyReleasedByGroupName(lastGroupName, connection);
                    break;

                default:
                    result = this.RTData.QtyReleasedByGroupName(lastGroupName, connection);
                    break;
            }

            return result;
        }

        public void QtyReleasedByGroupNameToExcel(Excel.Application exelApp, Excel.Worksheet sheet, ref int rowNum, string groupName, int? qtyReleased)
        {
            //выводим кол-во запущенных по ЗП изделий
            switch (this.RTData == null)
            {
                case true:
                    //TMData точно не null
                    this.TMData.QtyReleasedByGroupNameToExcel(exelApp, sheet, ref rowNum, groupName, qtyReleased);
                    break;

                default:
                    this.RTData.QtyReleasedByGroupNameToExcel(exelApp, sheet, ref rowNum, groupName, qtyReleased);
                    break;
            }
        }

        public void QtyOKFaultToExcel(Excel.Application exelApp, Excel.Worksheet sheet, int rowNum, int totalCount, int statusUnknownCount, int statusFaultCount, int statusOKCount)
        {
            //выводим кол-во годных/не годных
            switch (this.RTData == null)
            {
                case true:
                    //TMData точно не null
                    this.TMData.QtyOKFaultToExcel(exelApp, sheet, rowNum, totalCount, statusUnknownCount, statusFaultCount, statusOKCount);
                    break;

                default:
                    this.RTData.QtyOKFaultToExcel(exelApp, sheet, rowNum, totalCount, statusUnknownCount, statusFaultCount, statusOKCount);
                    break;
            }
        }


        public void PaintRT(Excel.Application exelApp, Excel.Worksheet sheet, int rowNumBeg, int rowNumEnd, int colunmBeg, int columnEnd)
        {
            //красим холодное при наличии горячего
            if (this.TMData != null)
                this.RTData?.Paint(exelApp, sheet, rowNumBeg, rowNumEnd, colunmBeg, columnEnd, TemperatureCondition.RT);
        }

        public void PaintSingleRT(Excel.Application exelApp, Excel.Worksheet sheet, int rowNumBeg, int rowNumEnd, int colunmBeg, int columnEnd)
        {
            //красим холодное при отсутствии горячего
            if (this.TMData == null)
                this.RTData?.Paint(exelApp, sheet, rowNumBeg, rowNumEnd, colunmBeg, columnEnd, TemperatureCondition.RT);
        }

        public void PaintTM(Excel.Application exelApp, Excel.Worksheet sheet, int rowNumBeg, int rowNumEnd, int colunmBeg, int columnEnd)
        {
            //красим горячее в при наличии холодного
            if (this.RTData != null)
                this.TMData?.Paint(exelApp, sheet, rowNumBeg, rowNumEnd, colunmBeg, columnEnd, TemperatureCondition.TM);
        }

        public void PaintSingleTM(Excel.Application exelApp, Excel.Worksheet sheet, int rowNumBeg, int rowNumEnd, int colunmBeg, int columnEnd)
        {
            //красим горячее в при отсутствии холодного
            if (this.RTData == null)
                this.TMData?.Paint(exelApp, sheet, rowNumBeg, rowNumEnd, colunmBeg, columnEnd, TemperatureCondition.TM);
        }
    }
    */


    /*
    public class ReportByDevices : List<ReportData>
    {
        Excel.Application exelApp = null;
        Excel.Worksheet sheet = null;

        private string LastUsedColumnsSignature { get; set; }

        public ReportByDevices() : base()
        {
        }

        public ReportByDevices(List<ReportData> source) : base(source)
        {
        }

        public ReportData NewReportData()
        {
            ReportData result = new ReportData();
            this.Add(result);

            return result;
        }

        public void ToExcel(SqlConnection connection, bool visibleAfterBuilding)
        {
            this.exelApp = new Microsoft.Office.Interop.Excel.Application();
            this.exelApp.Visible = false;

            try
            {
                this.exelApp.SheetsInNewWorkbook = 2;
                Excel.Workbook workBook = this.exelApp.Workbooks.Add(Type.Missing);
                this.exelApp.DisplayAlerts = false;
                this.sheet = (Excel.Worksheet)this.exelApp.Worksheets.get_Item(1);
                this.sheet.Name = "Протокол испытаний";

                this.LastUsedColumnsSignature = string.Empty;
                int rowNum = 1;
                int columnEnd = 0;

                //счётчик выведенных записей
                int counter = 0;

                //здесь будем хранить флаг о неизменности ПЗ во всём выведенном отчёте
                bool groupNameChanged = false;

                //здесь будем хранить предыдущий просмотренный в цикле GroupName
                string lastGroupName = null;

                //счётчики успешного и не успешного прохождения тестов
                int statusOKCount = 0;
                int statusFaultCount = 0;
                int statusUnknownCount = 0;
                int totalCount = 0;

                int lastUsedColumn = 0;
                int rowNumBeg;

                //храним здесь сколько шапок было выведено
                int needHeaderCount = 0;

                ListOfCalculatorsMinMax listOfCalculatorsMinMax = new ListOfCalculatorsMinMax();

                foreach (ReportData p in this)
                {
                    string currentGroupName = (p.RTData == null) ? p.TMData.GroupName : p.RTData.GroupName;

                    if ((!groupNameChanged) && (lastGroupName != null))
                        groupNameChanged = (currentGroupName != lastGroupName);

                    lastGroupName = currentGroupName;

                    int column = 1;

                    bool needHeader = ((this.LastUsedColumnsSignature == string.Empty) || (this.LastUsedColumnsSignature != p.ColumnsSignature));

                    //выводим шапку если имеет место смена списка выведенных столбцов
                    int headerRowNum = rowNum + 2;
                    rowNumBeg = rowNum;

                    if (needHeader)
                    {
                        columnEnd = column + 3;

                        //выводим самую верхнюю часть шапки
                        p.TopHeaderToExcel(this.exelApp, this.sheet, ref rowNum);

                        //выводим шапку столбцов условий и измеренных параметров
                        p.ColumnsHeaderToExcel(this.exelApp, this.sheet, ref rowNum, ref columnEnd);

                        //выводим шапку идентификационных данных
                        p.HeaderToExcel(this.exelApp, this.sheet, ref rowNum, column + 2, ref columnEnd);

                        //выводим шапку Pair
                        p.PairColumnsHeaderToExcel(this.exelApp, this.sheet, rowNumBeg + 3, ref columnEnd);

                        //выводим шапку идентификационных данных Pair
                        p.PairHeaderToExcel(this.exelApp, this.sheet, rowNum, ref columnEnd);

                        //выводим шапку статуса
                        p.StatusHeaderToExcel(this.exelApp, this.sheet, rowNum, columnEnd);

                        needHeaderCount++;
                        rowNum++;
                    }

                    //выводим идентификационные данные
                    counter++;
                    p.IdentityToExcel(this.exelApp, this.sheet, counter, rowNum, ref column);

                    //выводим тело
                    p.BodyToExcel(this.exelApp, this.sheet, listOfCalculatorsMinMax, rowNum, ref column);

                    //выводим конечные идентификационные данные
                    p.EndIdentityToExcel(this.exelApp, this.sheet, rowNum, ref column);

                    //выводим тело pair
                    p.PairBodyToExcel(this.exelApp, this.sheet, listOfCalculatorsMinMax, rowNum, ref column);

                    //выводим идентификационные данные pair
                    p.PairIdentityToExcel(this.exelApp, this.sheet, rowNum, column);

                    //выводим статус
                    bool? isStatusOK = null;
                    lastUsedColumn = p.StatusToExcel(this.exelApp, this.sheet, rowNum, columnEnd, ref isStatusOK);

                    //формируем значения счётчиков неопределённого/успешного/не успешного прохождения тестов
                    if (isStatusOK == null)
                        statusUnknownCount++;
                    else
                    {
                        switch (isStatusOK)
                        {
                            case false:
                                statusFaultCount++;
                                break;

                            default:
                                statusOKCount++;
                                break;
                        }
                    }

                    //считаем сколько всего изделий просмотрено в цикле
                    totalCount++;

                    //обводим границы
                    p.SetBorders(this.exelApp, this.sheet, rowNumBeg, rowNum, lastUsedColumn);

                    //запоминаем набор столбцов, которые мы вывели
                    this.LastUsedColumnsSignature = p.ColumnsSignature;

                    rowNum++;
                }

                //получаем количество ТМЦ, запущенных по ПЗ
                ReportData rd = this[0];

                //если на протяжении всего цикла не зафиксировано изменение ПЗ - выводим кол-во запущенных ТМЦ по ПЗ. если же изменение ПЗ зафиксировано - в отчёте есть данные по разным ПЗ и запущенное кол-во не имеет смысла
                if (!groupNameChanged)
                {
                    int? qtyReleased = rd?.QtyReleasedByGroupName(lastGroupName, connection);

                    //выводим значения min/max для определённых в listOfCalculatorsMinMax параметров
                    if (needHeaderCount == 1)
                        rd?.ListOfCalculatorsMinMaxToExcel(this.exelApp, this.sheet, rowNum, listOfCalculatorsMinMax);

                    //выводим количество ТМЦ, запущенных по ПЗ
                    rd?.QtyReleasedByGroupNameToExcel(this.exelApp, this.sheet, ref rowNum, lastGroupName, qtyReleased);

                    //выводим кол-во годных/не годных
                    rd?.QtyOKFaultToExcel(this.exelApp, this.sheet, rowNum, totalCount, statusUnknownCount, statusFaultCount, statusOKCount);
                }

                //создаём нижний колонтитул
                this.sheet.PageSetup.RightFooter = "Лист &P Листов &N";

                this.sheet.UsedRange.EntireRow.AutoFit();
                this.sheet.UsedRange.EntireColumn.AutoFit();
                this.sheet.UsedRange.Font.Name = "Arial Narrow";

                //настраиваем вид печатного отчёта
                this.sheet.PageSetup.LeftMargin = 0;
                this.sheet.PageSetup.RightMargin = 0;
                this.sheet.PageSetup.TopMargin = 42;
                this.sheet.PageSetup.BottomMargin = 28;
                this.sheet.PageSetup.Orientation = Excel.XlPageOrientation.xlLandscape;
                this.sheet.PageSetup.PaperSize = Excel.XlPaperSize.xlPaperA4;
                this.sheet.PageSetup.Zoom = false;
                this.sheet.PageSetup.FitToPagesWide = 1;
                this.sheet.PageSetup.FitToPagesTall = false;
                this.sheet.PageSetup.ScaleWithDocHeaderFooter = true;
                this.sheet.PageSetup.AlignMarginsHeaderFooter = true;
            }

            finally
            {
                if (visibleAfterBuilding)
                {
                    this.exelApp.Visible = visibleAfterBuilding;
                    this.exelApp.WindowState = Microsoft.Office.Interop.Excel.XlWindowState.xlMaximized;
                }
            }
        }

        public void Print()
        {
            //печать уже сформированного отчёта
            this.sheet?.PrintOut(Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
        }
    }
    */

    public class CalculatorMinMax
    {
        private string FName = null;
        private string FUm = null;
        private int FColumn = -1;
        private double? FMinValue = null;
        private double? FMaxValue = null;

        public CalculatorMinMax(string name)
        {
            this.FName = name;
        }

        public string Name
        {
            get
            {
                return this.FName;
            }
        }

        public string Um
        {
            get
            {
                return this.FUm;
            }
        }

        public int Column
        {
            get
            {
                return this.FColumn;
            }
        }

        public double? MinValue
        {
            get
            {
                return this.FMinValue;
            }
        }

        public double? MaxValue
        {
            get
            {
                return this.FMaxValue;
            }
        }

        public void Calc(int column, string name, string um, string value)
        {
            if (name.Contains(this.FName))
            {
                double dValue;
                if (double.TryParse(value, out dValue))
                {
                    this.FMinValue = (this.FMinValue == null) ? dValue : Math.Min((double)this.FMinValue, dValue);
                    this.FMaxValue = (this.FMaxValue == null) ? dValue : Math.Max((double)this.FMaxValue, dValue);

                    //запоминаем номер столбца в отчёте Excel чтобы при выводе вычисленных min/max данных знать куда выводить вычисленные данные
                    if (this.FColumn == -1)
                        this.FColumn = column;

                    if (this.FUm == null)
                        this.FUm = um;
                }
            }
        }
    }

    public class ListOfCalculatorsMinMax : List<CalculatorMinMax>
    {
        public ListOfCalculatorsMinMax()
        {
            this.Add(new CalculatorMinMax("UTM"));
            this.Add(new CalculatorMinMax("IGT"));
            this.Add(new CalculatorMinMax("UGT"));
        }

        public void Calc(int column, string name, string um, string value)
        {
            foreach (CalculatorMinMax calculator in this)
                calculator.Calc(column, name, um, value);
        }
    }

    public class ListOfBannedNamesForUseInReport : List<string>
    {
        //тут храним список имён conditions и parameters которые не надо выводить в отчёт
        public ListOfBannedNamesForUseInReport()
        {
            this.Add("UBRmax");
            this.Add("UDSM");
            this.Add("URSM");
            this.Add("BVT_I");
            this.Add("ATU_PowerValue");
        }

        public bool Use(string name)
        {
            //true - разрешено использование данного имени conditions/parameters в отчёте
            //false - в отчёте данное имя conditions/parameters использовать нельзя
            return !this.Contains(name);
        }
    }

    public class ListOfImportantNamesInReport : List<string>
    {
        //тут храним список имён conditions и parameters которые важны для пользователя
        public ListOfImportantNamesInReport()
        {
            this.Add("RTUTM");
            this.Add("TMUTM");

            this.Add("RTUBO");
            this.Add("TMUBO");

            this.Add("RTUBR");
            this.Add("TMUBR");

            this.Add("RTIDRM");
            this.Add("TMIDRM");

            this.Add("RTIRRM");
            this.Add("TMIRRM");

            this.Add("RTIDSM");
            this.Add("TMIDSM");

            this.Add("RTIRSM");
            this.Add("TMIRSM");
        }
    }
}
