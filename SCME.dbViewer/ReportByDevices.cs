using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCME.Types.Profiles;
using Excel = Microsoft.Office.Interop.Excel;

namespace SCME.dbViewer.ForParameters
{
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

            if ((rtStatus != null) && (tmStatus != null))
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
            int result;

            switch (this.RTData == null)
            {
                case true:
                    //TMData точно не null
                    result = this.TMData.StatusToExcel(exelApp, sheet, status, rowNum, column);
                    break;

                default:
                    result = this.RTData.StatusToExcel(exelApp, sheet, status, rowNum, column);
                    break;
            }

            return result;
        }

        /*
        public void NotPairHeaderToExcel(Excel.Application exelApp, Excel.Worksheet sheet, int rowNum, int column)
        {
            //вывод всех шапок того, что хранится в NotPairData
            int rowNumBeg;
            foreach (DataTableParameters np in this.NotPairData)
            {
                rowNumBeg = rowNum;
                np.ColumnsHeaderToExcel(exelApp, sheet, ref rowNumBeg, column);
                column += np.Columns.Count;
            }
        }

        public void NotPairBodyToExcel(Excel.Application exelApp, Excel.Worksheet sheet, int row, ref int column)
        {
            //вывод всех тел того, что хранится в NotPairData
            int rowNum = row;
            int columnNum = column;

            foreach (DataTableParameters np in this.NotPairData)
            {
                np.BodyToExcel(exelApp, sheet, ref rowNum, ref columnNum);
            }

            column = columnNum;
        }
        */

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

        public void BodyToExcel(Excel.Application exelApp, Excel.Worksheet sheet, int rowNum, ref int column)
        {
            switch (this.RTData == null)
            {
                case true:
                    //TMData точно не null
                    this.TMData.BodyToExcel(exelApp, sheet, rowNum, ref column);
                    break;

                default:
                    this.RTData.BodyToExcel(exelApp, sheet, rowNum, ref column);
                    break;
            }
        }

        public void PairBodyToExcel(Excel.Application exelApp, Excel.Worksheet sheet, int rowNum, ref int column)
        {
            if ((this.RTData != null) && (this.TMData != null))
                this.TMData.BodyToExcel(exelApp, sheet, rowNum, ref column);
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

        public void QtyOKFaultToExcel(Excel.Application exelApp, Excel.Worksheet sheet, int rowNum, int statusFaultCounter, int statusOKCounter)
        {
            //выводим кол-во годных/не годных
            switch (this.RTData == null)
            {
                case true:
                    //TMData точно не null
                    this.TMData.QtyOKFaultToExcel(exelApp, sheet, rowNum, statusFaultCounter, statusOKCounter);
                    break;

                default:
                    this.RTData.QtyOKFaultToExcel(exelApp, sheet, rowNum, statusFaultCounter, statusOKCounter);
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

                //здесь будем хранить флаг о неизменности ЗП во всём выведенном отчёте
                bool groupNameChanged = false;

                //здесь будем хранить предыдущий просмотренный в цикле GroupName
                string lastGroupName = null;

                //счётчики успешного и не успешного прохождения тестов
                int statusOKCounter = 0;
                int statusFaultCounter = 0;

                int lastUsedColumn = 0;
                int rowNumBeg;

                foreach (ReportData p in this)
                {
                    string currentGroupName = p.RTData == null ? p.TMData.GroupName : p.RTData.GroupName;

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
                        columnEnd = column + 4;

                        //выводим шапку столбцов условий и измеренных параметров
                        p.ColumnsHeaderToExcel(this.exelApp, this.sheet, ref rowNum, ref columnEnd);

                        //выводим шапку идентификационных данных
                        p.HeaderToExcel(this.exelApp, this.sheet, ref rowNum, column + 3, ref columnEnd);

                        //выводим шапку Pair
                        p.PairColumnsHeaderToExcel(this.exelApp, this.sheet, rowNumBeg, ref columnEnd);

                        //выводим шапку идентификационных данные Pair
                        p.PairHeaderToExcel(this.exelApp, this.sheet, rowNum, ref columnEnd);

                        //выводим шапку статуса
                        p.StatusHeaderToExcel(this.exelApp, this.sheet, rowNum, columnEnd);

                        rowNum++;
                    }

                    //выводим идентификационные данные
                    counter++;
                    p.IdentityToExcel(this.exelApp, this.sheet, counter, rowNum, ref column);

                    //выводим тело
                    p.BodyToExcel(this.exelApp, this.sheet, rowNum, ref column);

                    //выводим конечные идентификационные данные
                    p.EndIdentityToExcel(this.exelApp, this.sheet, rowNum, ref column);

                    //выводим тело pair
                    p.PairBodyToExcel(this.exelApp, this.sheet, rowNum, ref column);

                    //выводим идентификационные данные pair
                    p.PairIdentityToExcel(this.exelApp, this.sheet, rowNum, column);

                    //выводим статус
                    bool? isStatusOK = null;
                    lastUsedColumn = p.StatusToExcel(this.exelApp, this.sheet, rowNum, columnEnd, ref isStatusOK);

                    //формируем значения счётчиков успешного/не успешного прохождения тестов
                    if (isStatusOK != null)
                    {
                        switch (isStatusOK)
                        {
                            case false:
                                statusFaultCounter++;
                                break;

                            default:
                                statusOKCounter++;
                                break;
                        }
                    }

                    //обводим границы
                    p.SetBorders(this.exelApp, this.sheet, rowNumBeg, rowNum, lastUsedColumn);

                    //запоминаем набор столбцов, которые мы вывели
                    this.LastUsedColumnsSignature = p.ColumnsSignature;

                    rowNum++;
                }

                //получаем количество ТМЦ, запущенных по ЗП
                ReportData rd = this[0];

                //если на протяжении всего цикла не зафиксировано изменение ПЗ - выводим кол-во запущенных ТМЦ по ПЗ. если же изменение ПЗ зафиксировано - в отчёте есть данные по разным ПЗ и запущенное кол-во не имеет смысла
                if (!groupNameChanged)
                {
                    int? qtyReleased = rd?.QtyReleasedByGroupName(lastGroupName, connection);

                    if (qtyReleased != null)
                    {
                        //если количество выведенных записей меньше, чем qtyReleased - добиваем сформированный отчёт пустыми строками
                        int emptyRecordsCount = (int)qtyReleased - counter;

                        if (emptyRecordsCount > 0)
                        {
                            rowNumBeg = rowNum;

                            for (int i = 0; i <= emptyRecordsCount - 1; i++)
                            {
                                counter++;
                                rd?.CounterToExcel(this.exelApp, this.sheet, counter, rowNum);
                                rowNum++;
                            }

                            //обводим границы
                            rd?.SetBorders(this.exelApp, this.sheet, rowNumBeg, rowNum - 1, lastUsedColumn);
                        }
                    }

                    //выводим количество ТМЦ, запущенных по ЗП
                    rd?.QtyReleasedByGroupNameToExcel(this.exelApp, this.sheet, ref rowNum, lastGroupName, qtyReleased);
                }

                //выводим кол-во годных/не годных
                rd?.QtyOKFaultToExcel(this.exelApp, this.sheet, rowNum, statusFaultCounter, statusOKCounter);

                //создаём нижний колонтитул
                this.sheet.PageSetup.RightFooter = "Лист &P Листов &N";

                this.sheet.UsedRange.EntireRow.AutoFit();
                this.sheet.UsedRange.EntireColumn.AutoFit();

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
}
