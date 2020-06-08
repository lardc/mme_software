using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace SCME.dbViewer
{
    public class Mapper
    {
        private Dictionary<string, DataRow> dictionary;

        public Mapper()
        {
            this.dictionary = new Dictionary<string, DataRow>();
        }

        private string CalcKey(string code, string groupName, string profileBody)
        {
            return string.Concat(code, groupName, profileBody);
        }

        private DataRow RowByKey(string key)
        {
            DataRow row = null;

            return (this.dictionary.TryGetValue(key, out row)) ? row : null;
        }

        public DataRow Pop(string code, string groupName, string profileBody)
        {
            //по принятым code, groupName, profileBody ищет в себе row и возвращает её в качестве результата
            //если ничего не находит - возвращает null
            string key = this.CalcKey(code, groupName, profileBody);
            DataRow row = this.RowByKey(key);

            return row;
        }

        public void Push(string code, string groupName, string profileBody, DataRow row)
        {
            //создаёт место хранения принятой row (на момент вызова она должна быть создана) в себе под принятыми идентификаторами: code, groupName, profileBody
            if (row != null)
            {
                string key = this.CalcKey(code, groupName, profileBody);
                this.dictionary.Add(key, row);
            }
        }

        public void SetDeviceClassAndStatusToDbNull()
        {
            //принятый indexOfColumnRecordIsStorage есть индекс столбца "NameOfHiddenColumn"
            //идём по записям, у которых значение скрытого столбца "NameOfHiddenColumn" равно False и выставляем значения DbNull.Value в реквизитах DeviceClass и Status
            //данная реализация вызывается после того, как формирование данных в DataTable закончено, т.е. все записи не принявшие в себя данных от других записей лишаются значений DeviceClass (если DeviceClass не Fault) и Status
            string columnNameRecordIsStorage = Routines.NameOfRecordIsStorageColumn();

            foreach (KeyValuePair<string, DataRow> entry in this.dictionary)
            {
                DataRow row = entry.Value;

                if ((bool)row[columnNameRecordIsStorage] == false)
                {
                    //имеем дело с записью, в которой нет никаких данных, кроме своих собственных
                    row.BeginEdit();

                    try
                    {
                        row[Constants.DeviceClass] = DBNull.Value;

                        if (row[Constants.Status].ToString() != Constants.FaultSatatus)
                            row[Constants.Status] = DBNull.Value;
                    }

                    finally
                    {
                        row.EndEdit();
                    }
                }
            }
        }
    }
}
