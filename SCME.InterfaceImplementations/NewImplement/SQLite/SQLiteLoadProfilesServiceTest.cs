﻿using SCME.InterfaceImplementations.Common;
using System.Data.SQLite;

namespace SCME.InterfaceImplementations.NewImplement.SQLite
{
    public class SQLiteLoadProfilesServiceTest : LoadProfilesServiceTest<SQLiteCommand, SQLiteConnection>
    {
        protected override string _TestTypeSelectString => @"SELECT [ID], [TEST_TYPE_ID] FROM [PROF_TEST_TYPE] WHERE [PROF_ID] = @PROF_ID";
        protected override string _OrderSelectString => @"SELECT [ORDER] FROM [PROF_TEST_TYPE] WHERE [ID] = @TEST_TYPE_ID";
        public SQLiteLoadProfilesServiceTest(SQLiteConnection dbConnection) : base(dbConnection) {  }
    }
}