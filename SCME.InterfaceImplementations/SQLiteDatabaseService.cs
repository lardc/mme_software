using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using SCME.Types;
using SCME.Types.DatabaseServer;
using SCME.Types.Interfaces;
using SCME.Types.Profiles;

namespace SCME.InterfaceImplementations
{
    public class SQLiteDatabaseService : IDatabaseService
    {
        private readonly IProfilesService _profilesService;

        #region Properties
        public const string InsertConditionCmdTemplate =
            "INSERT INTO CONDITIONS(COND_ID, COND_NAME, COND_NAME_LOCAL, COND_IS_TECH) VALUES(NULL, @COND_NAME, @COND_NAME_LOCAL, @COND_IS_TECH)";

        public const string InsertParamCmdTemplate =
            "INSERT INTO PARAMS(PARAM_ID, PARAM_NAME, PARAM_NAME_LOCAL, PARAM_IS_HIDE) VALUES(NULL, @PARAM_NAME, @PARAM_NAME_LOCAL, @PARAM_IS_HIDE)";

        public const string InsertTestTypeCmdTemplate =
          "INSERT INTO TEST_TYPE(ID, NAME) VALUES(@ID, @NAME)";

        public const string InsertErrorCmdTemplate =
            "INSERT INTO ERRORS(ERR_ID, ERR_NAME, ERR_NAME_LOCAL, ERR_CODE) VALUES(NULL, @ERR_NAME, @ERR_NAME_LOCAL, @ERR_CODE)";

        private readonly string[] _mDbTablesList =
            {
                "CONDITIONS", "DEVICES", "GROUPS", "PARAMS", "PROFILES", "DEV_PARAM"
                , "PROF_COND", "PROF_PARAM", "ERRORS", "DEV_ERR", "TEST_TYPE","PROF_TEST_TYPE","MME_CODES","MME_CODES_TO_PROFILES"
            };

        public static readonly Tuple<string, string, bool>[] _ConditionsList = SQLDatabaseService._ConditionsList;

        public static readonly Tuple<string, string, bool>[] _ParamsList = SQLDatabaseService._ParamsList;

        public static readonly Tuple<string, string, int>[] _DefectCodes = SQLDatabaseService._DefectCodes;

        public static readonly Tuple<int, string>[] _TestTypes = SQLDatabaseService._TestTypes;

        private readonly string[] _MmeCodes = SQLDatabaseService._mmeCodes;

    private readonly SQLiteConnection _Connection;
    #endregion

    public SQLiteDatabaseService(string databasePath, string dbSettings = "synchronous=Full;journal mode=Truncate;failifmissing=True")
    {
        _Connection =
            new SQLiteConnection(
                $"data source={databasePath};{dbSettings}", false);
        _profilesService = new SQLiteProfilesService($"data source={databasePath};{dbSettings}");
    }

    public void ImportProfiles(string filePath)
    {
        var dictionary = new ProfileDictionary(filePath);
        var profileList = dictionary.PlainCollection.ToList();
        var profileItems = new List<ProfileItem>(profileList.Count);
        foreach (var profile in profileList)
        {
            var profileItem = new ProfileItem
            {
                ProfileName = profile.Name,
                ProfileKey = profile.Key,
                ProfileTS = profile.Timestamp,
                GateTestParameters = new List<Types.Gate.TestParameters>(),
                VTMTestParameters = new List<Types.SL.TestParameters>(),
                BVTTestParameters = new List<Types.BVT.TestParameters>(),
                DvDTestParameterses = new List<Types.dVdt.TestParameters>(),
                ATUTestParameters = new List<Types.ATU.TestParameters>(),
                QrrTqTestParameters = new List<Types.QrrTq.TestParameters>(),
                RACTestParameters = new List<Types.RAC.TestParameters>(),
                TOUTestParameters = new List<Types.TOU.TestParameters>(),
                CommTestParameters = profile.ParametersComm,
                IsHeightMeasureEnabled = profile.IsHeightMeasureEnabled,
                ParametersClamp = profile.ParametersClamp,
                Height = profile.Height,
                Temperature = profile.Temperature
            };
            foreach (var baseTestParametersAndNormativese in profile.TestParametersAndNormatives)
            {
                var gate = baseTestParametersAndNormativese as Types.Gate.TestParameters;
                if (gate != null)
                {
                    profileItem.GateTestParameters.Add(gate);
                    continue;
                }
                var sl = baseTestParametersAndNormativese as Types.SL.TestParameters;
                if (sl != null)
                {
                    profileItem.VTMTestParameters.Add(sl);
                    continue;
                }
                var bvt = baseTestParametersAndNormativese as Types.BVT.TestParameters;
                if (bvt != null)
                    profileItem.BVTTestParameters.Add(bvt);

                var dvdt = baseTestParametersAndNormativese as Types.dVdt.TestParameters;
                if (dvdt != null)
                    profileItem.DvDTestParameterses.Add(dvdt);

                var atu = baseTestParametersAndNormativese as Types.ATU.TestParameters;
                if (atu != null)
                    profileItem.ATUTestParameters.Add(atu);

                var qrrTq = baseTestParametersAndNormativese as Types.QrrTq.TestParameters;
                if (qrrTq != null)
                    profileItem.QrrTqTestParameters.Add(qrrTq);

                var rac = baseTestParametersAndNormativese as Types.RAC.TestParameters;
                if (rac != null)
                    profileItem.RACTestParameters.Add(rac);

                var tou = baseTestParametersAndNormativese as Types.TOU.TestParameters;
                if (tou != null)
                    profileItem.TOUTestParameters.Add(tou);
                }
            profileItems.Add(profileItem);

        }
        if (State == ConnectionState.Open)
            _profilesService.SaveProfiles(profileItems);
    }

    public void Open()
    {
        _Connection.Open();
    }

    public void Close()
    {
        if (_Connection.State == ConnectionState.Open)
            _Connection.Close();
    }

    public ConnectionState State
    {
        get { return _Connection.State; }
        set { throw new NotImplementedException(); }
    }

    public void ResetContent()
    {
        if (_Connection.State == ConnectionState.Open)
        {
            var trans = _Connection.BeginTransaction();

            try
            {
                using (var deleteCmd = _Connection.CreateCommand())
                {
                    foreach (var table in _mDbTablesList)
                    {
                        deleteCmd.CommandText = $"DELETE FROM {table}";
                        deleteCmd.ExecuteNonQuery();
                    }
                }

                using (var insertCmd = _Connection.CreateCommand())
                {
                    insertCmd.CommandText = InsertConditionCmdTemplate;
                    insertCmd.Parameters.Add("@COND_NAME", DbType.AnsiStringFixedLength);
                    insertCmd.Parameters.Add("@COND_NAME_LOCAL", DbType.String);
                    insertCmd.Parameters.Add("@COND_IS_TECH", DbType.Boolean);
                    insertCmd.Prepare();

                    foreach (var condition in _ConditionsList)
                    {
                        insertCmd.Parameters["@COND_NAME"].Value = condition.Item1;
                        insertCmd.Parameters["@COND_NAME_LOCAL"].Value = condition.Item2;
                        insertCmd.Parameters["@COND_IS_TECH"].Value = condition.Item3;

                        insertCmd.ExecuteNonQuery();
                    }
                }

                using (var insertCmd = _Connection.CreateCommand())
                {
                    insertCmd.CommandText = InsertTestTypeCmdTemplate;
                    insertCmd.Parameters.Add("@ID", DbType.Int32);
                    insertCmd.Parameters.Add("@NAME", DbType.String);
                    insertCmd.Prepare();

                    foreach (var testType in _TestTypes)
                    {
                        insertCmd.Parameters["@ID"].Value = testType.Item1;
                        insertCmd.Parameters["@NAME"].Value = testType.Item2;

                        insertCmd.ExecuteNonQuery();
                    }
                }

                using (var insertCmd = _Connection.CreateCommand())
                {
                    insertCmd.CommandText = InsertParamCmdTemplate;
                    insertCmd.Parameters.Add("@PARAM_NAME", DbType.AnsiStringFixedLength);
                    insertCmd.Parameters.Add("@PARAM_NAME_LOCAL", DbType.String);
                    insertCmd.Parameters.Add("@PARAM_IS_HIDE", DbType.Boolean);
                    insertCmd.Prepare();

                    foreach (var param in _ParamsList)
                    {
                        insertCmd.Parameters["@PARAM_NAME"].Value = param.Item1;
                        insertCmd.Parameters["@PARAM_NAME_LOCAL"].Value = param.Item2;
                        insertCmd.Parameters["@PARAM_IS_HIDE"].Value = param.Item3;

                        insertCmd.ExecuteNonQuery();
                    }
                }

                using (var insertCmd = _Connection.CreateCommand())
                {
                    insertCmd.CommandText = InsertErrorCmdTemplate;
                    insertCmd.Parameters.Add("@ERR_NAME", DbType.AnsiStringFixedLength);
                    insertCmd.Parameters.Add("@ERR_NAME_LOCAL", DbType.String);
                    insertCmd.Parameters.Add("@ERR_CODE", DbType.Int32);
                    insertCmd.Prepare();

                    foreach (var err in _DefectCodes)
                    {
                        insertCmd.Parameters["@ERR_NAME"].Value = err.Item1;
                        insertCmd.Parameters["@ERR_NAME_LOCAL"].Value = err.Item2;
                        insertCmd.Parameters["@ERR_CODE"].Value = err.Item3;

                        insertCmd.ExecuteNonQuery();
                    }
                }

                using (var insertCmd = _Connection.CreateCommand())
                {
                    insertCmd.CommandText = "INSERT INTO MME_CODES (MME_CODE) VALUES (@MME_CODE)";
                    insertCmd.Parameters.Add("@MME_CODE", DbType.String);
                    insertCmd.Prepare();

                    foreach (var code in _MmeCodes)
                    {
                        insertCmd.Parameters["@MME_CODE"].Value = code;
                        insertCmd.ExecuteNonQuery();
                    }
                }

                trans.Commit();
            }
            catch (Exception)
            {
                trans.Rollback();
                throw;
            }
        }
    }

    public static readonly MigratorInserter[] MigrationSet =
    {
            new MigratorInserterTemplate<Tuple<int,string>, SQLiteParameterCollection>("TEST_TYPE", "NAME", InsertTestTypeCmdTemplate, _TestTypes,
                (c, i) =>
                {

                    i.Add("@ID", DbType.Int32);
                    i.Add("@NAME", DbType.String, 32);

                    c.Add("@WHERE_PARAMETR", DbType.String, 32);
                },
                (c, o) =>
                {
                    c["@WHERE_PARAMETR"].Value = o.Item2;
                },
                (i, o) =>
                {
                    i["@ID"].Value = o.Item1;
                    i["@NAME"].Value = o.Item2;
                }),

             new MigratorInserterTemplate<Tuple<string,string, bool>, SQLiteParameterCollection>("PARAMS", "PARAM_NAME", InsertParamCmdTemplate, _ParamsList,
                (c, i) =>
                {

                    i.Add("@PARAM_NAME", DbType.String, 16);
                    i.Add("@PARAM_NAME_LOCAL", DbType.String, 64);
                    i.Add("@PARAM_IS_HIDE", DbType.Boolean);

                    c.Add("@WHERE_PARAMETR", DbType.String, 16);
                },
                (c, o) =>
                {
                    c["@WHERE_PARAMETR"].Value = o.Item1;
                },
                (i, o) =>
                {
                    i["@PARAM_NAME"].Value = o.Item1;
                    i["@PARAM_NAME_LOCAL"].Value = o.Item2;
                    i["@PARAM_IS_HIDE"].Value = o.Item3;
                }),

              new MigratorInserterTemplate<Tuple<string,string, bool>, SQLiteParameterCollection>("CONDITIONS", "COND_NAME", InsertConditionCmdTemplate, _ConditionsList,
                (c, i) =>
                {

                    i.Add("@COND_NAME", DbType.String, 32);
                    i.Add("@COND_NAME_LOCAL", DbType.String, 64);
                    i.Add("@COND_IS_TECH", DbType.Boolean);

                    c.Add("@WHERE_PARAMETR", DbType.String, 32);
                },
                (c, o) =>
                {
                    c["@WHERE_PARAMETR"].Value = o.Item1;
                },
                (i, o) =>
                {
                    i["@COND_NAME"].Value = o.Item1;
                    i["@COND_NAME_LOCAL"].Value = o.Item2;
                    i["@COND_IS_TECH"].Value = o.Item3;
                }),


              new MigratorInserterTemplate<Tuple<string,string, int>, SQLiteParameterCollection>("ERRORS", "ERR_NAME", InsertErrorCmdTemplate, _DefectCodes,
                (c, i) =>
                {

                    i.Add("@ERR_NAME", DbType.String, 20);
                    i.Add("@ERR_NAME_LOCAL", DbType.String, 32);
                    i.Add("@ERR_CODE", DbType.Int32);

                    c.Add("@WHERE_PARAMETR", DbType.String, 20);
                },
                (c, o) =>
                {
                    c["@WHERE_PARAMETR"].Value = o.Item1;
                },
                (i, o) =>
                {
                    i["@ERR_NAME"].Value = o.Item1;
                    i["@ERR_NAME_LOCAL"].Value = o.Item2;
                    i["@ERR_CODE"].Value = o.Item3;
                }),

        };

        
        public void Migrate()
        {
            if (_Connection.State == ConnectionState.Open)
            {
                foreach (var i in MigrationSet)
                    i.Migrate(_Connection);
            }
        }
    }
}