using SCME.InterfaceImplementations.Common;
using System.Data.SQLite;

namespace SCME.InterfaceImplementations.NewImplement.SQLite
{
    public class SQLiteSaveProfilesServiceTest : SaveProfilesServiceTest<SQLiteCommand, SQLiteConnection>
    {
        public SQLiteSaveProfilesServiceTest(SQLiteConnection dbConnection) : base(dbConnection) {  }
    }
}
