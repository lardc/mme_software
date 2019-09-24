using System;
using System.Data.Common;
using SCME.InterfaceImplementations.Common;
using System.Data.SQLite;
using System.Resources;

namespace SCME.InterfaceImplementations.NewImplement.SQLite
{
    public class SqLiteSaveProfilesService : SaveProfilesService<SQLiteCommand, SQLiteConnection>
    {
        protected override string _LoadTestTypesString => "SELECT ID, NAME FROM TEST_TYPE";
        protected override string _ProfileInsertString => "INSERT INTO PROFILES(PROF_ID, PROF_NAME, PROF_GUID, PROF_TS,PROF_VERS) VALUES (NULL, @PROF_NAME, @PROF_GUID, @PROF_TS,@VERSION)";
        protected override string _ProfileTestTypeInsertString => "INSERT INTO PROF_TEST_TYPE (PROF_ID,TEST_TYPE_ID,[ORDER]) VALUES (@PROF_ID, @TEST_TYPE_ID, @ORD)";

        public SqLiteSaveProfilesService(SQLiteConnection dbConnection) : base(dbConnection) {  }

        protected override int ExecuteCommandWithId(DbCommand command)
        {
            command.ExecuteScalar();
            return Convert.ToInt32(_Connection.LastInsertRowId);
        }
    }
}
