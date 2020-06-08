using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SCME.dbViewer
{
    public class Cache
    {
        private Dictionary<string, List<DataRow>> dictionary;

        public Cache()
        {
            this.dictionary = new Dictionary<string, List<DataRow>>();
        }

        private string CalcSculp(string code, string groupName, string profileBody)
        {
            return String.Concat(code, groupName, profileBody);
        }

        private List<DataRow> ListOfRows(string sculp)
        {
            List<DataRow> listOfRows = null;

            if (this.dictionary.TryGetValue(sculp, out listOfRows))
                return listOfRows;

            return null;
        }

        public void Push(string code, string groupName, string profileBody, DataRow row)
        {
            //поместить принятый row в кеш
            string sculp = this.CalcSculp(code, groupName, profileBody);
            List<DataRow> listOfRows = this.ListOfRows(sculp);

            switch (listOfRows == null)
            {
                case (true):
                    //ничего не найдено
                    listOfRows = new List<DataRow>();
                    listOfRows.Add(row);

                    this.dictionary.Add(sculp, listOfRows);
                    break;

                default:
                    //искомый sculp найден
                    listOfRows.Add(row);
                    break;
            }
        }

        public DataRow Pop(string code, string groupName, string profileBody)
        {
            //извлечь из кеша
            string sculp = this.CalcSculp(code, groupName, profileBody);
            List<DataRow> listOfRows = this.ListOfRows(sculp);

            if ((listOfRows != null) && (listOfRows.Count > 0))
            {
                DataRow row = listOfRows[0];

                //чтобы исключить возможность повторного использования row - удаляем найденную row из кеша
                listOfRows.Remove(row);

                return row;
            }

            return null;
        }

        public void SetDeviceClassAndStatusToDbNull()
        {
            //идём по всему списку и выставляем значения DbNull.Value в реквизитах DeviceClass и Status
            //данная реализация вызывается по окончанию формирования пар записей RT-TM, т.е. все записи не образовавшие пары лишаются значений DeviceClass (если DeviceClass не Fault) и Status
            foreach (KeyValuePair<string, List<DataRow>> entry in this.dictionary)
            {
                List<DataRow> listOfRows = entry.Value;

                foreach (DataRow row in listOfRows)
                {
                    row[Constants.DeviceClass] = DBNull.Value;

                    if (row[Constants.Status].ToString() != "Fault")
                        row[Constants.Status] = DBNull.Value;
                }
            }
        }
    }
}
