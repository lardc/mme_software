using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                switch (this.RTData == null)
                {
                    case true:
                        return this.TMData.ColumnsSignature;

                    default:
                        //RTData
                        switch (this.TMData == null)
                        {
                            case true:
                                return this.RTData.ColumnsSignature;

                            default:
                                //RTData и TMData
                                return this.RTData.ColumnsSignature + this.TMData.ColumnsSignature;
                        }
                }
            }
        }

        public void HeaderToExcel(Excel.Application exelApp, Excel.Worksheet sheet, int number, int rowNum, int column)
        {
            switch (this.RTData == null)
            {
                case true:
                    //TMData точно не null
                    this.TMData.HeaderToExcel(exelApp, sheet, rowNum, ref column);
                    break;

                default:
                    this.RTData.HeaderToExcel(exelApp, sheet, rowNum, ref column);
                    break;
            }
        }

        public void IdentityToExcel(Excel.Application exelApp, Excel.Worksheet sheet, int number, int rowNum, ref int column)
        {
            switch (this.RTData == null)
            {
                case true:
                    //TMData точно не null
                    this.TMData.IdentityToExcel(exelApp, sheet, number, rowNum, ref column);
                    break;

                default:
                    this.RTData.IdentityToExcel(exelApp, sheet, number, rowNum, ref column);
                    break;
            }
        }

        public void ColumnsHeaderToExcel(Excel.Application exelApp, Excel.Worksheet sheet, ref int rowNum, int column)
        {
            switch (this.RTData == null)
            {
                case true:
                    //TMData точно не null
                    this.TMData.ColumnsHeaderToExcel(exelApp, sheet, ref rowNum, column);
                    break;

                default:
                    this.RTData.ColumnsHeaderToExcel(exelApp, sheet, ref rowNum, column);
                    break;
            }
        }

        public void BodyToExcel(Excel.Application exelApp, Excel.Worksheet sheet, ref int rowNum, ref int column)
        {
            switch (this.RTData == null)
            {
                case true:
                    //TMData точно не null
                    this.TMData.BodyToExcel(exelApp, sheet, ref rowNum, ref column);
                    break;

                default:
                    this.RTData.BodyToExcel(exelApp, sheet, ref rowNum, ref column);
                    break;
            }
        }

        public void PairHeaderToExcel(Excel.Application exelApp, Excel.Worksheet sheet, int rowNum, int column)
        {
            if ((this.RTData != null) && (this.TMData != null))
                this.TMData.PairHeaderToExcel(exelApp, sheet, rowNum, column);
        }

        public void PairIdentityToExcel(Excel.Application exelApp, Excel.Worksheet sheet, int rowNum, ref int column)
        {
            if ((this.RTData != null) && (this.TMData != null))
                this.TMData.PairIdentityToExcel(exelApp, sheet, rowNum, ref column);
        }

        public void PairColumnsHeaderToExcel(Excel.Application exelApp, Excel.Worksheet sheet, ref int rowNum, int column)
        {
            if ((this.RTData != null) && (this.TMData != null))
                this.TMData.ColumnsHeaderToExcel(exelApp, sheet, ref rowNum, column);
        }

        public void PairBodyToExcel(Excel.Application exelApp, Excel.Worksheet sheet, int rowNum, ref int column)
        {
            if ((this.RTData != null) && (this.TMData != null))
                this.TMData.BodyToExcel(exelApp, sheet, ref rowNum, ref column);
        }

        public void SetBorders(Excel.Application exelApp, Excel.Worksheet sheet, int rowNumBeg, int rowNumEnd, int columnEnd)
        {
            DataTableParameters dtp = (this.RTData == null) ? this.TMData : this.RTData;
            dtp?.SetBorders(exelApp, sheet, rowNumBeg, rowNumEnd, columnEnd);
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
        private string LastUsedColumnsSignature { get; set; }

        public ReportData NewReportData()
        {
            ReportData result = new ReportData();
            this.Add(result);

            return result;
        }

        public void ToExcel()
        {
            Excel.Application exelApp = new Microsoft.Office.Interop.Excel.Application();
            exelApp.Visible = false;

            try
            {
                exelApp.SheetsInNewWorkbook = 2;
                Excel.Workbook workBook = exelApp.Workbooks.Add(Type.Missing);
                exelApp.DisplayAlerts = false;
                Excel.Worksheet sheet = (Excel.Worksheet)exelApp.Worksheets.get_Item(1);
                sheet.Name = "Отчёт по данным КИПП СПП";

                this.LastUsedColumnsSignature = string.Empty;
                int number = 1;
                int rowNum = 1;

                foreach (ReportData p in this)
                {
                    int column = 1;

                    bool needHeader = ((this.LastUsedColumnsSignature == string.Empty) || (this.LastUsedColumnsSignature != p.ColumnsSignature));

                    //выводим шапку если имеет место смена списка столбцов
                    int headerRowNum = rowNum;
                    int RowNumBeg = rowNum;
                    if (needHeader)
                        p.HeaderToExcel(exelApp, sheet, number, headerRowNum + 1, column);

                    if (needHeader)
                        p.ColumnsHeaderToExcel(exelApp, sheet, ref rowNum, column + 7);

                    //выводим тело
                    int BodyRowNum = rowNum;
                    int BodyColumn = column + 7;
                    p.BodyToExcel(exelApp, sheet, ref rowNum, ref BodyColumn);
                    int lastBodyColumn = BodyColumn - 1;

                    //выводим идентификационные данные
                    p.IdentityToExcel(exelApp, sheet, number, BodyRowNum, ref column);

                    //выводим шапку Pair
                    if (needHeader)
                        p.PairHeaderToExcel(exelApp, sheet, headerRowNum + 1, BodyColumn);

                    //выводим идентификационные данные Pair
                    int FirstPairBodyColumn = BodyColumn;
                    p.PairIdentityToExcel(exelApp, sheet, BodyRowNum, ref BodyColumn);

                    if (needHeader)
                        p.PairColumnsHeaderToExcel(exelApp, sheet, ref headerRowNum, BodyColumn);

                    //выводим тело Pair
                    p.PairBodyToExcel(exelApp, sheet, BodyRowNum, ref BodyColumn);

                    //обводим границы
                    p.SetBorders(exelApp, sheet, RowNumBeg, BodyRowNum, BodyColumn - 1);

                    //раскрашиваем
                    p.PaintRT(exelApp, sheet, RowNumBeg, BodyRowNum, 6, lastBodyColumn);
                    p.PaintSingleRT(exelApp, sheet, RowNumBeg, BodyRowNum, 6, lastBodyColumn);

                    p.PaintTM(exelApp, sheet, RowNumBeg, BodyRowNum, FirstPairBodyColumn, BodyColumn - 1);
                    p.PaintSingleTM(exelApp, sheet, RowNumBeg, BodyRowNum, 6, lastBodyColumn);

                    this.LastUsedColumnsSignature = p.ColumnsSignature;
                    number++;
                }

                sheet.UsedRange.EntireRow.AutoFit();
                sheet.UsedRange.EntireColumn.AutoFit();
            }

            finally
            {
                exelApp.Visible = true;
            }
        }
    }
}
