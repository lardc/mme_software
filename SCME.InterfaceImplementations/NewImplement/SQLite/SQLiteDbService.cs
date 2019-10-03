using System;
using System.Data.Common;
using System.Data.SQLite;
using System.ServiceModel;
using SCME.InterfaceImplementations.Common;

namespace SCME.InterfaceImplementations.NewImplement.SQLite
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Single,
        Namespace = "http://proton-electrotex.com/SCME")]
    public class SQLiteDbService : Common.DbService.DbService<SQLiteCommand, SQLiteConnection>
    {
        protected override string GetFreeProfileNameString => @"SELECT PROF_ID FROM PROFILES ORDER BY PROF_ID DESC LIMIT 1";
        protected override string TestTypeSelectString => @"SELECT [ID], [TEST_TYPE_ID] FROM [PROF_TEST_TYPE] WHERE [PROF_ID] = @PROF_ID";
        protected override string OrderSelectString => @"SELECT [ORDER] FROM [PROF_TEST_TYPE] WHERE [ID] = @TEST_TYPE_ID";
        protected override string LoadTestTypesString => "SELECT ID, NAME FROM TEST_TYPE";
        protected override string ProfileInsertString => "INSERT INTO PROFILES(PROF_ID, PROF_NAME, PROF_GUID, PROF_TS,PROF_VERS) VALUES (NULL, @PROF_NAME, @PROF_GUID, @PROF_TS,@VERSION)";
        protected override string ProfileTestTypeInsertString => "INSERT INTO PROF_TEST_TYPE (PROF_ID,TEST_TYPE_ID,[ORDER]) VALUES (@PROF_ID, @TEST_TYPE_ID, @ORD)";

        protected override string InsertTestTypeString => "INSERT INTO TEST_TYPE(ID, NAME) VALUES(@ID, @NAME)";
        
        protected override string DatabaseFieldTestTypeName => "NAME";

        protected override int ExecuteCommandWithId(DbCommand command)
        {
            command.ExecuteNonQuery();
            return Convert.ToInt32(_connection.LastInsertRowId);
        }

        public SQLiteDbService(SQLiteConnection connection) : base(connection)
        {
        }
    }
}