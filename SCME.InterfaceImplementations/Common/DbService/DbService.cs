using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using SCME.Types.Profiles;

namespace SCME.InterfaceImplementations.Common.DbService
{
    public abstract partial class DbService<TDbCommand, TDbConnection> : IDbService where TDbCommand : DbCommand where TDbConnection : DbConnection
    {
        protected virtual string ChildsSelectString => @"SELECT [PROF_ID], [PROF_NAME], [PROF_GUID], [PROF_VERS], [PROF_TS] FROM [PROFILES] WHERE [PROF_NAME] = @PROF_NAME AND PROF_ID <> @PROF_ID_EXCLUDE ORDER BY [PROF_TS] DESC";
        protected virtual string TestTypeSelectString => @"SELECT [PTT_ID], [TEST_TYPE_ID] FROM [PROF_TEST_TYPE] WHERE [PROF_ID] = @PROF_ID";
        protected virtual string ProfilesByMmeSelectString => @"SELECT PROF_ID, PROF_NAME, PROF_GUID, PROF_VERS, PROF_TS FROM PROFILES WHERE PROF_ID IN
	            (SELECT PROFILE_ID FROM MME_CODES_TO_PROFILES WHERE MME_CODE_ID IN
		            (SELECT MME_CODE_ID FROM MME_CODES WHERE MME_CODE = @MME_CODE)) ORDER BY PROF_TS DESC";
        protected virtual string OrderSelectString => @"SELECT [ORD] FROM [PROF_TEST_TYPE] WHERE [PTT_ID] = @TEST_TYPE_ID";
        protected virtual string ConditionSelectString => @"SELECT C.[COND_NAME], PC.[VALUE] FROM [PROF_COND] PC LEFT JOIN [CONDITIONS] C on C.[COND_ID] = PC.[COND_ID] WHERE PC.[PROF_TESTTYPE_ID] = @TEST_TYPE_ID";
        protected virtual string ParamSelectString => "SELECT P.[PARAM_NAME], PP.[MIN_VAL], PP.[MAX_VAL] FROM [PROF_PARAM] PP LEFT JOIN [PARAMS] P on P.[PARAM_ID] = PP.[PARAM_ID] WHERE PP.[PROF_TESTTYPE_ID] = @TEST_TYPE_ID";
        protected virtual string ProfileByKeySelectString => @"SELECT PROF_ID, PROF_NAME, PROF_GUID, PROF_VERS, PROF_TS FROM PROFILES WHERE PROF_GUID = @PROF_GUID";
        protected virtual string AllMmeCodesSelectString => @"SELECT MME_CODE, MME_CODE_ID FROM MME_CODES";
        protected virtual string ProfileInsertString =>
            "INSERT INTO[PROFILES] (PROF_NAME, PROF_GUID, PROF_TS, PROF_VERS) OUTPUT INSERTED.PROF_ID VALUES(@PROF_NAME, @PROF_GUID, @PROF_TS, @VERSION)";
        protected virtual string ProfileConditionInsertString => "INSERT INTO [PROF_COND](PROF_TESTTYPE_ID, PROF_ID, COND_ID, VALUE) VALUES(@PROF_TESTTYPE_ID, @PROF_ID, @COND_ID, @VALUE)";
        protected virtual string ProfileParameterInsertString => "INSERT INTO [PROF_PARAM](PROF_TESTTYPE_ID, PROF_ID, PARAM_ID, MIN_VAL, MAX_VAL) VALUES(@PROF_TESTTYPE_ID, @PROF_ID, @PARAM_ID, @MIN_VAL, @MAX_VAL)";
        protected virtual string ProfileTestTypeInsertString => "INSERT INTO [PROF_TEST_TYPE] (PROF_ID, TEST_TYPE_ID, [ORD]) OUTPUT INSERTED.PTT_ID VALUES (@PROF_ID, @TEST_TYPE_ID, @ORD)";
        protected virtual string MmeCodeToProfileInsertString => "INSERT INTO [MME_CODES_TO_PROFILES] (MME_CODE_ID, PROFILE_ID) VALUES (" +
            "(SELECT MME_CODE_ID FROM MME_CODES WHERE MME_CODE = @MME_CODE), @PROFILE_ID)";
        protected virtual string MmeCodeToProfileDeleteString => "DELETE FROM MME_CODES_TO_PROFILES WHERE PROFILE_ID = @PROFILE_ID";
        protected virtual string LoadTestTypesString => "SELECT TEST_TYPE_ID, RTRIM(TEST_TYPE_NAME) FROM TEST_TYPE";
        protected virtual string LoadConditionsString => "SELECT COND_ID, RTRIM(COND_NAME) FROM CONDITIONS";
        protected virtual string LoadParametersString => "SELECT PARAM_ID, RTRIM(PARAM_NAME) FROM PARAMS";

        private readonly Dictionary<string, int> _testTypeIdByName = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _conditionIdByName = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _parameterIdByName = new Dictionary<string, int>();

        private TDbCommand _profileInsert;

        private TDbCommand _profileConditionInsert;
        private TDbCommand _profileTestTypeInsert;
        private TDbCommand _profileParameterInsert;

        private TDbCommand _mmeCodeToProfileInsert;
        private TDbCommand _mmeCodeToProfileDelete;

        private TDbCommand _loadTestTypes;
        private TDbCommand _loadConditions;
        private TDbCommand _loadParameters;
        
        private TDbCommand _childSelect;
        private TDbCommand _testTypeSelect;
        private TDbCommand _profilesByMmeSelect;
        private TDbCommand _orderSelect;
        private TDbCommand _conditionSelect;
        private TDbCommand _paramSelect;

        private TDbCommand _profileByKeySelect;
        private TDbCommand _allMmeCodesSelect;
        
        private DbTransaction _dbTransaction;

        private readonly TDbConnection _connection;

        private readonly ConstructorInfo _commandConstructor = typeof(TDbCommand).GetConstructor(new Type[] {typeof(string), typeof(TDbConnection)});

        private readonly Dictionary<string, List<MyProfile>> _cacheProfilesByMmeCode;
        private readonly Dictionary<Guid, (MyProfile Profile, bool IsDeepLoad, bool IsChildLoad)> _cacheProfileByKey;
        
        protected DbService(TDbConnection connection)
        {
            _connection = connection;
            if (_connection.State != ConnectionState.Open)
                _connection.Open();

            PrepareQueries();
            LoadDictionary();
            _cacheProfilesByMmeCode = new Dictionary<string, List<MyProfile>>();
            _cacheProfileByKey = new Dictionary<Guid, (MyProfile Profile, bool IsDeepLoad, bool IsChildLoad)>();
        }

        private TDbCommand CreateCommand(string commandString, List<TDbCommandParametr> parameters)
        {
            var command = (TDbCommand)_commandConstructor.Invoke(new object[] {commandString, _connection});
            foreach (var i in parameters)
            {
                var parameter = command.CreateParameter();
                parameter.DbType = i.DbType;
                parameter.ParameterName = i.Name;
                if (i.Size != null)
                    parameter.Size = i.Size.Value;
                command.Parameters.Add(parameter);
            }

            command.Prepare();
            return command;
        }

        private void PrepareQueries()
        {
            _childSelect = CreateCommand(ChildsSelectString, new List<TDbCommandParametr>()
            {
                new TDbCommandParametr("@PROF_NAME", DbType.String, 32),
                new TDbCommandParametr("@PROF_ID_EXCLUDE", DbType.String, 32)
            });
            _testTypeSelect = CreateCommand(TestTypeSelectString, new List<TDbCommandParametr>()
            {
                new TDbCommandParametr("@PROF_ID", DbType.Int32)
            });

            //_AllTopProfilesSelect = CreateCommand(_AllTopProfilesSelectString, new List<TDbCommandParametr>());
            _profilesByMmeSelect = CreateCommand(ProfilesByMmeSelectString, new List<TDbCommandParametr>()
            {
                new TDbCommandParametr("@MME_CODE", DbType.String, 64)
            });
            //_ProfilesByNameByMMESelect = CreateCommand(_ProfilesByNameByMMESelectString, new List<TDbCommandParametr>()
            //{
            //    new TDbCommandParametr("@PROF_NAME", DbType.String, 32 ),
            //    new TDbCommandParametr("@MME_CODE", DbType.String, 64 )
            //});

            _orderSelect = CreateCommand(OrderSelectString, new List<TDbCommandParametr>()
            {
                new TDbCommandParametr("@TEST_TYPE_ID", DbType.Int32)
            });
            _conditionSelect = CreateCommand(ConditionSelectString, new List<TDbCommandParametr>()
            {
                new TDbCommandParametr("@TEST_TYPE_ID", DbType.Int32)
            });
            _paramSelect = CreateCommand(ParamSelectString, new List<TDbCommandParametr>()
            {
                new TDbCommandParametr("@TEST_TYPE_ID", DbType.Int32)
            });

            _profileByKeySelect = CreateCommand(ProfileByKeySelectString, new List<TDbCommandParametr>()
            {
                new TDbCommandParametr("@PROF_GUID", DbType.Guid)
            });

            _allMmeCodesSelect = CreateCommand(AllMmeCodesSelectString, new List<TDbCommandParametr>());

            _profileInsert = CreateCommand(ProfileInsertString, new List<TDbCommandParametr>()
            {
                new TDbCommandParametr("@PROF_NAME", DbType.String, 32),
                new TDbCommandParametr("@PROF_GUID", DbType.Guid),
                new TDbCommandParametr("@PROF_TS", DbType.DateTime, 8),
                new TDbCommandParametr("@VERSION", DbType.Int32),
            });

            _profileConditionInsert = CreateCommand(ProfileConditionInsertString, new List<TDbCommandParametr>()
            {
                new TDbCommandParametr("@PROF_TESTTYPE_ID", DbType.Int32),
                new TDbCommandParametr("@PROF_ID", DbType.Int32),
                new TDbCommandParametr("@COND_ID", DbType.Int32),
                new TDbCommandParametr("@VALUE", DbType.String, 16),
            });
            _profileTestTypeInsert = CreateCommand(ProfileTestTypeInsertString, new List<TDbCommandParametr>()
            {
                new TDbCommandParametr("@PROF_ID", DbType.Int32),
                new TDbCommandParametr("@TEST_TYPE_ID", DbType.Int32),
                new TDbCommandParametr("@ORD", DbType.Int32),
            });
            _profileParameterInsert = CreateCommand(ProfileParameterInsertString, new List<TDbCommandParametr>()
            {
                new TDbCommandParametr("@PROF_TESTTYPE_ID", DbType.Int32),
                new TDbCommandParametr("@PROF_ID", DbType.Int32),
                new TDbCommandParametr("@PARAM_ID", DbType.Int32),
                new TDbCommandParametr("@MIN_VAL", DbType.Single),
                new TDbCommandParametr("@MAX_VAL", DbType.Single),
            });

            _mmeCodeToProfileInsert = CreateCommand(MmeCodeToProfileInsertString, new List<TDbCommandParametr>()
            {
                new TDbCommandParametr("@MME_CODE", DbType.String, 64),
                new TDbCommandParametr("@PROFILE_ID", DbType.Int32),
            });
            _mmeCodeToProfileDelete = CreateCommand(MmeCodeToProfileDeleteString, new List<TDbCommandParametr>()
            {
                new TDbCommandParametr("@PROFILE_ID", DbType.Int32),
            });

            _loadTestTypes = CreateCommand(LoadTestTypesString, new List<TDbCommandParametr>());
            _loadConditions = CreateCommand(LoadConditionsString, new List<TDbCommandParametr>());
            _loadParameters = CreateCommand(LoadParametersString, new List<TDbCommandParametr>());
        }
        
    }
}