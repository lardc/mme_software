using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace SCME.InterfaceImplementations
{
    public abstract class MigratorInserter
    {
        public string TableName { get; private set; }
        public string ColumnWhereName { get; private set; }
        public string InsertCommand { get; private set; }
        public abstract void Migrate(DbConnection _mConnection);

        public MigratorInserter(string tableName, string columnWhereName, string insertCommand)
        {
            TableName = tableName;
            ColumnWhereName = columnWhereName;
            InsertCommand = insertCommand;
        }
    }
}
