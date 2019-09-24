using System.Data.SQLite;
using SCME.InterfaceImplementations.Common;

namespace SCME.InterfaceImplementations.NewImplement.SQLite
{
    public class SQLiteDbService : Common.DbService.DbService<SQLiteCommand, SQLiteConnection>
    {
        protected override string TestTypeSelectString => @"SELECT [ID], [TEST_TYPE_ID] FROM [PROF_TEST_TYPE] WHERE [PROF_ID] = @PROF_ID";
        protected override string OrderSelectString => @"SELECT [ORDER] FROM [PROF_TEST_TYPE] WHERE [ID] = @TEST_TYPE_ID";
        protected override string LoadTestTypesString => "SELECT ID, NAME FROM TEST_TYPE";
        protected override string ProfileInsertString => "INSERT INTO PROFILES(PROF_ID, PROF_NAME, PROF_GUID, PROF_TS,PROF_VERS) VALUES (NULL, @PROF_NAME, @PROF_GUID, @PROF_TS,@VERSION)";
        protected override string ProfileTestTypeInsertString => "INSERT INTO PROF_TEST_TYPE (PROF_ID,TEST_TYPE_ID,[ORDER]) VALUES (@PROF_ID, @TEST_TYPE_ID, @ORD)";
        
        public SQLiteDbService(SQLiteConnection connection) : base(connection)
        {
        }
    }
}