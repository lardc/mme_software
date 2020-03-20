using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Runtime.Serialization;
using SCME.Types;
using SCME.Types.Database;
using SCME.Types.Profiles;

namespace SCME.InterfaceImplementations.Common.DbService
{
    public abstract partial class DbService<TDbCommand, TDbConnection> : IDbService where TDbCommand : DbCommand where TDbConnection : DbConnection
    {
        protected virtual string SelectAllTopProfileString => @"SELECT PROFILES.PROF_ID, PROFILES.PROF_NAME, PROFILES.PROF_GUID, LATEST_ORDERS.VERS, PROFILES.PROF_TS FROM
                                                                (SELECT PROF_NAME, MAX(PROF_VERS) AS VERS FROM PROFILES GROUP BY PROF_NAME) AS LATEST_ORDERS 
                                                                INNER JOIN PROFILES 
                                                                ON PROFILES.PROF_NAME = LATEST_ORDERS.PROF_NAME AND PROFILES.PROF_VERS = LATEST_ORDERS.VERS";

        protected virtual string CheckMmeCodeIsActiveString => $"SELECT COUNT (*) FROM MME_CODES WHERE MME_CODE = '{Constants.MME_CODE_IS_ACTIVE_NAME}'";

//        ALTER TABLE MME_CODES_TO_PROFILES 
//            ADD CONSTRAINT FK_MME_CODES_TO_PROFILES_MME_CODES
//        FOREIGN KEY (MME_CODE_ID) 
//        REFERENCES MME_CODES(MME_CODE_ID) 
//        ON DELETE CASCADE;

//        alter table MME_CODES_TO_PROFILES drop constraint FK_MME_CODES_TO_PROFILES_MME_CODES

        protected virtual string CheckConditionString => "SELECT COUNT (*) FROM CONDITIONS WHERE COND_NAME = @WHERE_PARAMETER";
        protected virtual string CheckParameterString => "SELECT COUNT (*) FROM PARAMS WHERE PARAM_NAME = @WHERE_PARAMETER";
        protected virtual string CheckTestTypeString => $"SELECT COUNT (*) FROM TEST_TYPE WHERE {DatabaseFieldTestTypeName} = @WHERE_PARAMETER";
        protected virtual string CheckErrorString => "SELECT COUNT (*) FROM ERRORS WHERE ERR_NAME = @WHERE_PARAMETER";

        protected virtual string MmeCodesByProfileString => @"SELECT MME_CODE FROM MME_CODES WHERE MME_CODE_ID IN 
                                                                (SELECT MME_CODE_ID FROM MME_CODES_TO_PROFILES WHERE PROFILE_ID = @PROFILE_ID)";

        protected virtual string InsertConditionString => "INSERT INTO CONDITIONS(COND_NAME, COND_NAME_LOCAL, COND_IS_TECH) VALUES(@COND_NAME, @COND_NAME_LOCAL, @COND_IS_TECH)";
        protected virtual string InsertParameterString => "INSERT INTO PARAMS(PARAM_NAME, PARAM_NAME_LOCAL, PARAM_IS_HIDE) VALUES(@PARAM_NAME, @PARAM_NAME_LOCAL, @PARAM_IS_HIDE)";
        protected virtual string InsertTestTypeString => "INSERT INTO TEST_TYPE(TEST_TYPE_ID, TEST_TYPE_NAME) VALUES(@ID, @NAME)";
        protected virtual string InsertErrorString => "INSERT INTO ERRORS(ERR_NAME, ERR_NAME_LOCAL, ERR_CODE) VALUES(@ERR_NAME, @ERR_NAME_LOCAL, @ERR_CODE)";

        protected virtual string GetFreeProfileNameString => @"SELECT TOP(1) PROF_ID FROM PROFILES ORDER BY PROF_ID DESC";
        protected virtual string ProfileNameExistsString => @"SELECT COUNT(*) FROM PROFILES WHERE PROF_NAME = @PROF_NAME";
        protected virtual string ChildSelectString => @"SELECT [PROF_ID], [PROF_NAME], [PROF_GUID], [PROF_VERS], [PROF_TS] FROM [PROFILES] WHERE [PROF_NAME] = @PROF_NAME AND PROF_ID <> @PROF_ID_EXCLUDE ORDER BY [PROF_TS] DESC";
        protected virtual string TestTypeSelectString => @"SELECT [PTT_ID], [TEST_TYPE_ID] FROM [PROF_TEST_TYPE] WHERE [PROF_ID] = @PROF_ID ORDER BY ORD";

        protected virtual string ProfilesByMmeSelectString => @"SELECT PROF_ID, PROF_NAME, PROF_GUID, PROF_VERS, PROF_TS FROM PROFILES WHERE PROF_ID IN
	            (SELECT PROFILE_ID FROM MME_CODES_TO_PROFILES WHERE MME_CODE_ID IN
		            (SELECT MME_CODE_ID FROM MME_CODES WHERE MME_CODE = @MME_CODE)) ORDER BY PROF_VERS DESC";

        protected virtual string OrderSelectString => @"SELECT [ORD] FROM [PROF_TEST_TYPE] WHERE [PTT_ID] = @TEST_TYPE_ID";
        protected virtual string ConditionSelectString => @"SELECT C.[COND_NAME], PC.[VALUE] FROM [PROF_COND] PC LEFT JOIN [CONDITIONS] C on C.[COND_ID] = PC.[COND_ID] WHERE PC.[PROF_TESTTYPE_ID] = @TEST_TYPE_ID";
        protected virtual string ParamSelectString => "SELECT P.[PARAM_NAME], PP.[MIN_VAL], PP.[MAX_VAL] FROM [PROF_PARAM] PP LEFT JOIN [PARAMS] P on P.[PARAM_ID] = PP.[PARAM_ID] WHERE PP.[PROF_TESTTYPE_ID] = @TEST_TYPE_ID";

        protected virtual string ProfileByNameByMmeMaxTimestampString => @"SELECT TOP 1 PROF_ID, PROF_NAME, PROF_GUID, PROF_VERS, PROF_TS FROM PROFILES WHERE PROF_NAME = @PROF_NAME AND PROF_ID IN (
                SELECT PROFILE_ID FROM MME_CODES_TO_PROFILES WHERE MME_CODE_ID IN 
                    (SELECT MME_CODE_ID FROM MME_CODES  WHERE MME_CODE = @MME_CODE) ) ORDER BY PROF_TS DESC";

        protected virtual string AllMmeCodesSelectString => @"SELECT MME_CODE, MME_CODE_ID FROM MME_CODES";

        protected virtual string ProfileInsertString =>
            "INSERT INTO[PROFILES] (PROF_NAME, PROF_GUID, PROF_TS, PROF_VERS) OUTPUT INSERTED.PROF_ID VALUES(@PROF_NAME, @PROF_GUID, @PROF_TS, @VERSION)";

        protected virtual string ProfileConditionInsertString => "INSERT INTO [PROF_COND](PROF_TESTTYPE_ID, PROF_ID, COND_ID, VALUE) VALUES(@PROF_TESTTYPE_ID, @PROF_ID, @COND_ID, @VALUE)";
        protected virtual string ProfileParameterInsertString => "INSERT INTO [PROF_PARAM](PROF_TESTTYPE_ID, PROF_ID, PARAM_ID, MIN_VAL, MAX_VAL) VALUES(@PROF_TESTTYPE_ID, @PROF_ID, @PARAM_ID, @MIN_VAL, @MAX_VAL)";
        protected virtual string ProfileTestTypeInsertString => "INSERT INTO [PROF_TEST_TYPE] (PROF_ID, TEST_TYPE_ID, [ORD]) OUTPUT INSERTED.PTT_ID VALUES (@PROF_ID, @TEST_TYPE_ID, @ORD)";
        protected virtual string InsertMmeCodeString => @"INSERT INTO MME_CODES (MME_CODE) OUTPUT INSERTED.MME_CODE_ID VALUES(@MME_CODE)";
        protected virtual string DeleteAllMmeCodeToProfileByMmeCodeString => @"DELETE FROM MME_CODES_TO_PROFILES WHERE MME_CODE_ID = (SELECT MME_CODE_ID FROM MME_CODES WHERE MME_CODE = @MME_CODE)";
        protected virtual string DeleteMmeCodeString => @"DELETE FROM MME_CODES WHERE @MME_CODE = MME_CODE";

        protected virtual string MmeCodeToProfileInsertString => "INSERT INTO [MME_CODES_TO_PROFILES] (MME_CODE_ID, PROFILE_ID) VALUES (" +
                                                                 "(SELECT MME_CODE_ID FROM MME_CODES WHERE MME_CODE = @MME_CODE), @PROFILE_ID)";

        protected virtual string MmeCodeToProfileDeleteString => @"DELETE FROM MME_CODES_TO_PROFILES WHERE PROFILE_ID = @PROFILE_ID AND MME_CODE_ID = 
            (SELECT MME_CODE_ID FROM MME_CODES WHERE MME_CODE = @MME_CODE)";

        protected virtual string LoadTestTypesString => "SELECT TEST_TYPE_ID, RTRIM(TEST_TYPE_NAME) FROM TEST_TYPE";
        protected virtual string LoadConditionsString => "SELECT COND_ID, RTRIM(COND_NAME) FROM CONDITIONS";
        protected virtual string LoadParametersString => "SELECT PARAM_ID, RTRIM(PARAM_NAME) FROM PARAMS";

        protected virtual string UpdateMmeCodesToProfilesString => "UPDATE MME_CODES_TO_PROFILES SET PROFILE_ID = @NEW_PROFILE_ID WHERE PROFILE_ID = @OLD_PROFILE_ID" ;

        private readonly Dictionary<string, int> _testTypeIdByName = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _conditionIdByName = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _parameterIdByName = new Dictionary<string, int>();

        private TDbCommand _mmeCodesByProfile;

        private TDbCommand _checkMmeCodeIsActive;

        private TDbCommand _checkCondition;
        private TDbCommand _checkParameter;
        private TDbCommand _checkTestType;
        private TDbCommand _checkError;

        private TDbCommand _insertCondition;
        private TDbCommand _insertParameter;
        private TDbCommand _insertTestType;
        private TDbCommand _insertError;

        private TDbCommand _profileInsert;

        private TDbCommand _getFreeProfileName;
        private TDbCommand _profileNameExists;

        private TDbCommand _profileConditionInsert;
        private TDbCommand _profileTestTypeInsert;
        private TDbCommand _profileParameterInsert;

        private TDbCommand _mmeCodeToProfileInsert;
        private TDbCommand _mmeCodeToProfileDelete;
        private TDbCommand _deleteAllMmeCodeToProfileByMmeCode;


        private TDbCommand _loadTestTypes;
        private TDbCommand _loadConditions;
        private TDbCommand _loadParameters;

        private TDbCommand _childSelect;
        private TDbCommand _testTypeSelect;
        private TDbCommand _profilesByMmeSelect;
        private TDbCommand _orderSelect;
        private TDbCommand _conditionSelect;
        private TDbCommand _paramSelect;
        private TDbCommand _selectAllTopProfile;

        private TDbCommand _profileByKeySelect;
        private TDbCommand _allMmeCodesSelect;

        private TDbCommand _deleteMmeCode;
        private TDbCommand _insertMmeCode;

        private TDbCommand _profileByNameByMmeMaxTimestamp;

        private TDbCommand _updateMmeCodesToProfile;

        private DbTransaction _dbTransaction;

        protected readonly TDbConnection Connection;

        private readonly ConstructorInfo _commandConstructor = typeof(TDbCommand).GetConstructor(new Type[] {typeof(string), typeof(TDbConnection)});

        private readonly Dictionary<string, List<MyProfile>> _cacheProfilesByMmeCode;
        private readonly Dictionary<int, ProfileCache> _cacheProfileById;

        private InserterBaseTestParametersAndNormatives _inserter;

        private bool _enableCache;

        public void ClearCacheByMmeCode(string mmeCode)
        {
            try
            {
                _cacheProfilesByMmeCode.TryGetValue(mmeCode, out var profiles);
                if (profiles == null)
                    return;

                foreach (var profile in profiles)
                    _cacheProfileById.Remove(profile.Id);

                _cacheProfilesByMmeCode.Remove(mmeCode);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        protected DbService(TDbConnection connection, bool enableCache = true)
        {
            _enableCache = enableCache;

            _cacheProfilesByMmeCode = new Dictionary<string, List<MyProfile>>();
            _cacheProfileById = new Dictionary<int, ProfileCache>();

            Connection = connection;
            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            PrepareQueries();
            LoadDictionary();
        }

        private TDbCommand CreateCommand(string commandString, IEnumerable<DbCommandParameter> parameters, DbTransaction dbTransaction = null)
        {
            var command = (TDbCommand) _commandConstructor.Invoke(new object[] {commandString, Connection});
            foreach (var i in parameters)
            {
                var parameter = command.CreateParameter();
                parameter.DbType = i.DbType;
                parameter.ParameterName = i.Name;
                if (i.Size != null)
                    parameter.Size = i.Size.Value;
                command.Parameters.Add(parameter);
            }

            command.Transaction = dbTransaction;
            command.Prepare();
            return command;
        }

        private void PrepareQueries()
        {
       
                _profileByNameByMmeMaxTimestamp = CreateCommand(ProfileByNameByMmeMaxTimestampString, new List<DbCommandParameter>()
                {
                    new DbCommandParameter("@PROF_NAME", DbType.String, 32),
                    new DbCommandParameter("@MME_CODE", DbType.String, 64),
                });

            
            _mmeCodesByProfile = CreateCommand(MmeCodesByProfileString, new List<DbCommandParameter>()
            {
                new DbCommandParameter("@PROFILE_ID", DbType.Int32),
            });

            var checkTDbCommandParameters = new List<DbCommandParameter>()
            {
                new DbCommandParameter("@WHERE_PARAMETER", DbType.String, 32)
            };

            _checkMmeCodeIsActive = CreateCommand(CheckMmeCodeIsActiveString, new List<DbCommandParameter>());

            _checkCondition = CreateCommand(CheckConditionString, checkTDbCommandParameters);
            _checkParameter = CreateCommand(CheckParameterString, checkTDbCommandParameters);
            _checkError = CreateCommand(CheckErrorString, checkTDbCommandParameters);
            _checkTestType = CreateCommand(CheckTestTypeString, checkTDbCommandParameters);

            _insertCondition = CreateCommand(InsertConditionString, new List<DbCommandParameter>()
            {
                new DbCommandParameter("@COND_NAME", DbType.String, 32),
                new DbCommandParameter("@COND_NAME_LOCAL", DbType.String, 64),
                new DbCommandParameter("@COND_IS_TECH", DbType.Boolean)
            });

            _insertTestType = CreateCommand(InsertTestTypeString, new List<DbCommandParameter>()
            {
                new DbCommandParameter("@ID", DbType.Int32),
                new DbCommandParameter("@NAME", DbType.String, 32),
            });

            _insertParameter = CreateCommand(InsertParameterString, new List<DbCommandParameter>()
            {
                new DbCommandParameter("@PARAM_NAME", DbType.String, 16),
                new DbCommandParameter("@PARAM_NAME_LOCAL", DbType.String, 64),
                new DbCommandParameter("@PARAM_IS_HIDE", DbType.Boolean)
            });

            _insertError = CreateCommand(InsertErrorString, new List<DbCommandParameter>()
            {
                new DbCommandParameter("@ERR_NAME", DbType.String, 20),
                new DbCommandParameter("@ERR_NAME_LOCAL", DbType.String, 32),
                new DbCommandParameter("@ERR_CODE", DbType.Int32)
            });


            _childSelect = CreateCommand(ChildSelectString, new List<DbCommandParameter>()
            {
                new DbCommandParameter("@PROF_NAME", DbType.String, 32),
                new DbCommandParameter("@PROF_ID_EXCLUDE", DbType.String, 32)
            });
            _testTypeSelect = CreateCommand(TestTypeSelectString, new List<DbCommandParameter>()
            {
                new DbCommandParameter("@PROF_ID", DbType.Int32)
            });

            //_AllTopProfilesSelect = CreateCommand(_AllTopProfilesSelectString, new List<DbCommandParameter>());
            _profilesByMmeSelect = CreateCommand(ProfilesByMmeSelectString, new List<DbCommandParameter>()
            {
                new DbCommandParameter("@MME_CODE", DbType.String, 64)
            });
            //_ProfilesByNameByMMESelect = CreateCommand(_ProfilesByNameByMMESelectString, new List<DbCommandParameter>()
            //{
            //    new DbCommandParameter("@PROF_NAME", DbType.String, 32 ),
            //    new DbCommandParameter("@MME_CODE", DbType.String, 64 )
            //});

            _orderSelect = CreateCommand(OrderSelectString, new List<DbCommandParameter>()
            {
                new DbCommandParameter("@TEST_TYPE_ID", DbType.Int32)
            });
            _conditionSelect = CreateCommand(ConditionSelectString, new List<DbCommandParameter>()
            {
                new DbCommandParameter("@TEST_TYPE_ID", DbType.Int32)
            });
            _paramSelect = CreateCommand(ParamSelectString, new List<DbCommandParameter>()
            {
                new DbCommandParameter("@TEST_TYPE_ID", DbType.Int32)
            });

            _allMmeCodesSelect = CreateCommand(AllMmeCodesSelectString, new List<DbCommandParameter>());

            _selectAllTopProfile = CreateCommand(SelectAllTopProfileString, new List<DbCommandParameter>());

            _getFreeProfileName = CreateCommand(GetFreeProfileNameString, new List<DbCommandParameter>());
            _profileNameExists = CreateCommand(ProfileNameExistsString, new List<DbCommandParameter>()
            {
                new DbCommandParameter("@PROF_NAME", DbType.String, 32),
            });

            _profileInsert = CreateCommand(ProfileInsertString, new List<DbCommandParameter>()
            {
                new DbCommandParameter("@PROF_NAME", DbType.String, 32),
                new DbCommandParameter("@PROF_GUID", DbType.Guid),
                new DbCommandParameter("@PROF_TS", DbType.DateTime, 8),
                new DbCommandParameter("@VERSION", DbType.Int32),
            });

            _profileConditionInsert = CreateCommand(ProfileConditionInsertString, new List<DbCommandParameter>()
            {
                new DbCommandParameter("@PROF_TESTTYPE_ID", DbType.Int32),
                new DbCommandParameter("@PROF_ID", DbType.Int32),
                new DbCommandParameter("@COND_ID", DbType.Int32),
                new DbCommandParameter("@VALUE", DbType.String, 16),
            });
            _profileTestTypeInsert = CreateCommand(ProfileTestTypeInsertString, new List<DbCommandParameter>()
            {
                new DbCommandParameter("@PROF_ID", DbType.Int32),
                new DbCommandParameter("@TEST_TYPE_ID", DbType.Int32),
                new DbCommandParameter("@ORD", DbType.Int32),
            });
            _profileParameterInsert = CreateCommand(ProfileParameterInsertString, new List<DbCommandParameter>()
            {
                new DbCommandParameter("@PROF_TESTTYPE_ID", DbType.Int32),
                new DbCommandParameter("@PROF_ID", DbType.Int32),
                new DbCommandParameter("@PARAM_ID", DbType.Int32),
                new DbCommandParameter("@MIN_VAL", DbType.Single),
                new DbCommandParameter("@MAX_VAL", DbType.Single),
            });

            _deleteMmeCode = CreateCommand(DeleteMmeCodeString, new List<DbCommandParameter>()
            {
                new DbCommandParameter("@MME_CODE", DbType.String, 64),
            });
            _insertMmeCode = CreateCommand(InsertMmeCodeString, new List<DbCommandParameter>()
            {
                new DbCommandParameter("@MME_CODE", DbType.String, 64),
            });

            _deleteAllMmeCodeToProfileByMmeCode = CreateCommand(DeleteAllMmeCodeToProfileByMmeCodeString, new List<DbCommandParameter>()
            {
                new DbCommandParameter("@MME_CODE", DbType.String, 64),
            });
            _mmeCodeToProfileInsert = CreateCommand(MmeCodeToProfileInsertString, new List<DbCommandParameter>()
            {
                new DbCommandParameter("@MME_CODE", DbType.String, 64),
                new DbCommandParameter("@PROFILE_ID", DbType.Int32),
            });
            _mmeCodeToProfileDelete = CreateCommand(MmeCodeToProfileDeleteString, new List<DbCommandParameter>()
            {
                new DbCommandParameter("@MME_CODE", DbType.String, 64),
                new DbCommandParameter("@PROFILE_ID", DbType.Int32),
            });
            
            _updateMmeCodesToProfile = CreateCommand(UpdateMmeCodesToProfilesString, new List<DbCommandParameter>()
            {
                new DbCommandParameter("@NEW_PROFILE_ID", DbType.Int32),
                new DbCommandParameter("@OLD_PROFILE_ID", DbType.Int32),
            });

            _loadTestTypes = CreateCommand(LoadTestTypesString, new List<DbCommandParameter>());
            _loadConditions = CreateCommand(LoadConditionsString, new List<DbCommandParameter>());
            _loadParameters = CreateCommand(LoadParametersString, new List<DbCommandParameter>());
        }
    }
}