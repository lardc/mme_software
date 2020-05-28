using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Globalization;
using System.ServiceModel;
using SCME.InterfaceImplementations.Common;
using SCME.Types.DataContracts;

namespace SCME.InterfaceImplementations.NewImplement.SQLite
{
    public class SQLiteDbService : Common.DbService.DbService<SQLiteCommand, SQLiteConnection>
    {
        protected override string GetFreeProfileNameString => @"SELECT PROF_ID FROM PROFILES ORDER BY PROF_ID DESC LIMIT 1";
        protected override string TestTypeSelectString => @"SELECT [ID], [TEST_TYPE_ID] FROM [PROF_TEST_TYPE] WHERE [PROF_ID] = @PROF_ID";
        protected override string OrderSelectString => @"SELECT [ORDER] FROM [PROF_TEST_TYPE] WHERE [ID] = @TEST_TYPE_ID";
        protected override string LoadTestTypesString => "SELECT ID, NAME FROM TEST_TYPE";
        protected override string ProfileInsertString => "INSERT INTO PROFILES(PROF_ID, PROF_NAME, PROF_GUID, PROF_TS,PROF_VERS) VALUES (NULL, @PROF_NAME, @PROF_GUID, @PROF_TS,@VERSION)";
        protected override string ProfileTestTypeInsertString => "INSERT INTO PROF_TEST_TYPE (PROF_ID,TEST_TYPE_ID,[ORDER]) VALUES (@PROF_ID, @TEST_TYPE_ID, @ORD)";

        protected override string InsertTestTypeString => "INSERT INTO TEST_TYPE(ID, NAME) VALUES(@ID, @NAME)";

        protected override string InsertMmeCodeString => @"INSERT INTO MME_CODES (MME_CODE_ID, MME_CODE) VALUES (NULL, @MME_CODE)";

        protected override string ProfileByNameByMmeMaxTimestampString => @"SELECT PROF_ID, PROF_NAME, PROF_GUID, PROF_VERS, PROF_TS FROM PROFILES WHERE PROF_NAME = @PROF_NAME AND PROF_ID IN (
                SELECT PROFILE_ID FROM MME_CODES_TO_PROFILES WHERE MME_CODE_ID IN (SELECT MME_CODE_ID WHERE MME_CODE = @MME_CODE) ) ORDER BY PROF_TS DESC LIMIT 1";
        
        protected override string DatabaseFieldTestTypeName => "NAME";

        protected override int ExecuteCommandWithId(DbCommand command)
        {
            command.ExecuteNonQuery();
            return Convert.ToInt32(Connection.LastInsertRowId);
        }

        public readonly SQLiteResultsServiceLocal SqLiteResultsServiceLocal;
        public SQLiteDbService(SQLiteConnection connection) : base(connection)
        {
            SqLiteResultsServiceLocal = new SQLiteResultsServiceLocal(connection.ConnectionString);
        }
    }
}