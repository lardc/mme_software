﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using SCME.Types;
using SCME.Types.BVT;
using SCME.Types.DataContracts;
using SCME.Types.Interfaces;
using SCME.Types.QrrTq;

namespace SCME.InterfaceImplementations
{
    public class SQLResultsServiceServer : IResultsService
    {
        private readonly SqlConnection _connection;
        private readonly Dictionary<string, long> _errors = new Dictionary<string, long>(64);
        private readonly Dictionary<string, long> _params= new Dictionary<string, long>(64);
        private readonly Dictionary<string, long> _tests= new Dictionary<string, long>(20);
        protected static readonly object MsLocker = new object();

        private SqlCommand _profileSelectCommand;
        private SqlCommand _testTypeIDSelectCommand;
        private SqlCommand _groupSelectCommand;
        private SqlCommand _groupInsertCommand;
        private SqlCommand _devErrInsertCommand;
        private SqlCommand _deviceSelectCmd;
        private SqlCommand _deviceDeleteCmd;
        private SqlCommand _devInsertCommand;
        private SqlCommand _selectGroupsCommand;
        private SqlCommand _devParamInsertCommand;
        private SqlCommand _selectDevicesCommand;
        private SqlCommand _selectDevErrCommand;
        private SqlCommand _selectDevParamsCommand;
        private SqlCommand _selectDevCondsCommand;
        private SqlCommand _selectDevNormCommand;
        private SqlCommand _selectDevClassCommand;
        private SqlCommand _devLookupPTTSelectCommand;
        private SqlCommand _devLookupProfileIdSelectCommand;
        private SqlCommand _selectDevRTClassCommand;

        public SQLResultsServiceServer(string databasePath)
        {
            _connection = new SqlConnection(databasePath);
            _connection.Open();
            PrepareQueries();
            PopulateDictionaries();
        }
        
        public SQLResultsServiceServer(SqlConnection sqlConnection)
        {
            _connection = sqlConnection;
            PrepareQueries();
            PopulateDictionaries();
        }

        private void PrepareQueries()
        {
            _profileSelectCommand = new SqlCommand(
                "SELECT P.[PROF_ID] FROM [dbo].[PROFILES] P WHERE P.[PROF_GUID] = @PROF_GUID", _connection);
            _profileSelectCommand.Parameters.Add("@PROF_GUID", SqlDbType.UniqueIdentifier);
            _profileSelectCommand.Prepare();

            _testTypeIDSelectCommand = new SqlCommand(
                "SELECT PTT.[PTT_ID] FROM [dbo].[PROF_TEST_TYPE] PTT WHERE PTT.[PROF_ID] = @PROF_ID AND PTT.[TEST_TYPE_ID] = @TEST_TYPE_ID AND PTT.[ORD] = @ORDER", _connection);
            _testTypeIDSelectCommand.Parameters.Add("@PROF_ID", SqlDbType.Int);
            _testTypeIDSelectCommand.Parameters.Add("@TEST_TYPE_ID", SqlDbType.Int);
            _testTypeIDSelectCommand.Parameters.Add("@ORDER", SqlDbType.Int);
            _testTypeIDSelectCommand.Prepare();

            _groupSelectCommand = new SqlCommand(
                "SELECT G.[GROUP_ID] FROM [dbo].[GROUPS] G WHERE G.[GROUP_NAME] = @GROUP_NAME", _connection);
            _groupSelectCommand.Parameters.Add("@GROUP_NAME", SqlDbType.NChar, 32);
            _groupSelectCommand.Prepare();

            _groupInsertCommand =
                new SqlCommand("INSERT INTO [dbo].[GROUPS](GROUP_NAME) OUTPUT INSERTED.GROUP_ID VALUES(@GROUP_NAME)",
                    _connection);
            _groupInsertCommand.Parameters.Add("@GROUP_NAME", SqlDbType.NChar, 32);
            _groupInsertCommand.Prepare();

            _devErrInsertCommand = new SqlCommand("INSERT INTO [dbo].[DEV_ERR](DEV_ID, ERR_ID) VALUES(@DEV_ID, @ERR_ID)",
                _connection);
            _devErrInsertCommand.Parameters.Add("@DEV_ID", SqlDbType.Int);
            _devErrInsertCommand.Parameters.Add("@ERR_ID", SqlDbType.Int);
            _devErrInsertCommand.Prepare();

            _deviceSelectCmd =
                new SqlCommand(
                    "SELECT D.[DEV_ID] FROM [dbo].[DEVICES] D WHERE D.[CODE] = @CODE AND D.[GROUP_ID] = @GROUP_ID AND D.[POS] = @POS AND D.[PROFILE_ID] = @PROF_ID",
                    _connection);
            _deviceSelectCmd.Parameters.Add("@CODE", SqlDbType.NVarChar, 64);
            _deviceSelectCmd.Parameters.Add("@GROUP_ID", SqlDbType.Int);
            _deviceSelectCmd.Parameters.Add("@POS", SqlDbType.Bit);
            _deviceSelectCmd.Parameters.Add("@PROF_ID", SqlDbType.UniqueIdentifier);
            _deviceSelectCmd.Prepare();

            _deviceDeleteCmd = new SqlCommand("DELETE FROM [dbo].[DEVICES] WHERE [DEV_ID] = @DEV_ID", _connection);
            _deviceDeleteCmd.Parameters.Add("@DEV_ID", SqlDbType.Int);
            _deviceDeleteCmd.Prepare();

            _devInsertCommand =
                new SqlCommand(
                    "INSERT INTO [dbo].[DEVICES](GROUP_ID, PROFILE_ID, CODE, SIL_N_1, SIL_N_2, TS, USR, POS, MME_CODE) OUTPUT INSERTED.DEV_ID VALUES(@GROUP_ID, @PROFILE_ID, @CODE, @SIL_N_1, @SIL_N_2, @TS, @USR, @POS, @MME_CODE)",
                    _connection);
            _devInsertCommand.Parameters.Add("@GROUP_ID", SqlDbType.Int);
            _devInsertCommand.Parameters.Add("@PROFILE_ID", SqlDbType.UniqueIdentifier);
            _devInsertCommand.Parameters.Add("@CODE", SqlDbType.NVarChar, 32);
            _devInsertCommand.Parameters.Add("@SIL_N_1", SqlDbType.NVarChar, 64);
            _devInsertCommand.Parameters.Add("@SIL_N_2", SqlDbType.NVarChar, 64);
            _devInsertCommand.Parameters.Add("@TS", SqlDbType.DateTime);
            _devInsertCommand.Parameters.Add("@USR", SqlDbType.VarChar, 32);
            _devInsertCommand.Parameters.Add("@POS", SqlDbType.Bit);
            _devInsertCommand.Parameters.Add("@MME_CODE", SqlDbType.NVarChar, 64);
            _devInsertCommand.Prepare();

            _selectGroupsCommand =
                new SqlCommand(
                    "SELECT DISTINCT G.[GROUP_NAME] FROM [dbo].[GROUPS] G, [dbo].[DEVICES] D WHERE G.[GROUP_ID] = D.[GROUP_ID] AND D.[TS] > @TS_FROM AND D.[TS] <= @TS_TO ORDER BY G.[GROUP_NAME] ASC",
                    _connection);
            _selectGroupsCommand.Parameters.Add("@TS_FROM", SqlDbType.DateTime);
            _selectGroupsCommand.Parameters.Add("@TS_TO", SqlDbType.DateTime);
            _selectGroupsCommand.Prepare();

            _devParamInsertCommand =
                new SqlCommand(
                    "INSERT INTO [dbo].[DEV_PARAM](DEV_ID, PARAM_ID, VALUE, TEST_TYPE_ID) VALUES(@DEV_ID, @PARAM_ID, @VALUE, @TEST_TYPE_ID)",
                    _connection);
            _devParamInsertCommand.Parameters.Add("@DEV_ID", SqlDbType.Int);
            _devParamInsertCommand.Parameters.Add("@PARAM_ID", SqlDbType.Int);
            _devParamInsertCommand.Parameters.Add("@VALUE", SqlDbType.Decimal);
            _devParamInsertCommand.Parameters["@VALUE"].Scale = 4;
            _devParamInsertCommand.Parameters["@VALUE"].Precision = 10;
            _devParamInsertCommand.Parameters.Add("@TEST_TYPE_ID", SqlDbType.Int);
            _devParamInsertCommand.Prepare();

            _selectDevicesCommand =
                new SqlCommand(
                    "SELECT D.[DEV_ID], D.[CODE], D.[SIL_N_1], D.[SIL_N_2], D.[TS], D.[USR], D.[POS], P.[PROF_NAME] FROM [dbo].[DEVICES] D, [dbo].[GROUPS] G, [dbo].[PROFILES] P WHERE D.[GROUP_ID] = G.[GROUP_ID] AND D.[PROFILE_ID] = P.[PROF_GUID] AND G.[GROUP_NAME] = @GROUP_NAME ORDER BY D.[CODE] ASC",
                    _connection);
            _selectDevicesCommand.Parameters.Add("@GROUP_NAME", SqlDbType.NChar, 32);
            _selectDevicesCommand.Prepare();

            _selectDevErrCommand =
                new SqlCommand(
                    "SELECT E.[ERR_CODE] FROM [dbo].[ERRORS] E, [dbo].[DEV_ERR] DE WHERE E.[ERR_ID] = DE.[ERR_ID] AND DE.[DEV_ID] = @DEV_ID",
                    _connection);
            _selectDevErrCommand.Parameters.Add("@DEV_ID", SqlDbType.Int);
            _selectDevErrCommand.Prepare();

            _selectDevParamsCommand =
                new SqlCommand(
                    "SELECT P.[PARAM_NAME] AS P_NID, P.[PARAM_NAME_LOCAL] AS P_NAME, DP.[VALUE] AS P_VALUE, P.[PARAM_IS_HIDE] AS IS_HIDE FROM [dbo].[PARAMS] P, [dbo].[DEV_PARAM] DP WHERE DP.[PARAM_ID] = P.[PARAM_ID] AND DP.[DEV_ID] = @DEV_ID ORDER BY P.[PARAM_ID] ASC",
                    _connection);
            _selectDevParamsCommand.Parameters.Add("@DEV_ID", SqlDbType.Int);
            _selectDevParamsCommand.Prepare();

            _selectDevCondsCommand =
                new SqlCommand(
                    "SELECT C.[COND_NAME] AS C_NID, C.[COND_NAME_LOCAL] AS C_NAME, PC.[VALUE] AS C_VALUE, C.[COND_IS_TECH] AS IS_TECH FROM [dbo].[CONDITIONS] C, [dbo].[PROF_COND] PC, [dbo].[DEVICES] D, [dbo].[PROFILES] P WHERE PC.[COND_ID] = C.[COND_ID] AND PC.[PROF_ID] = P.[PROF_ID] AND P.[PROF_GUID] = D.[PROFILE_ID] AND D.[DEV_ID] = @DEV_ID ORDER BY C.[COND_ID] ASC",
                    _connection);
            _selectDevCondsCommand.Parameters.Add("@DEV_ID", SqlDbType.Int);
            _selectDevCondsCommand.Prepare();

            _selectDevRTClassCommand =
                new SqlCommand(
                    "SELECT dbo.DeviceClass(z.DEV_ID, dbo.DeviceTypeByProfileName(@ProfName), z.PROF_ID, @RTProfName) AS DEVICECLASS" +
                    " FROM" +
                    " (" +
                    "SELECT MAX(D.DEV_ID) AS DEV_ID, D.TS, P.PROF_ID" +
                    " FROM DEVICES D" +
                    "  INNER JOIN PROFILES P ON (" +
                    "                            (D.PROFILE_ID=P.PROF_GUID) AND" +
                    "                            (P.PROF_NAME LIKE @ProfBody)" +
                    "                           )" +
                    " WHERE (D.CODE=@DevCode)" +
                    "  GROUP BY P.PROF_ID, D.TS) AS z order by z.TS desc", _connection);

            _selectDevClassCommand =
                new SqlCommand(
                    "SELECT dbo.DeviceClass(z.DEV_ID, dbo.DeviceTypeByProfileName(@ProfName), z.PROF_ID, @ProfName) AS DEVICECLASS" +
                    " FROM" +
                    " (" +
                    "SELECT MAX(D.DEV_ID) AS DEV_ID, D.TS, P.PROF_ID" +
                    " FROM DEVICES D" +
                    "  INNER JOIN PROFILES P ON (" +
                    "                            (D.PROFILE_ID=P.PROF_GUID) AND" +
                    "                            (P.PROF_NAME=@ProfName)" +
                    "                           )" +
                    " WHERE (D.CODE=@DevCode)" +
                    "  GROUP BY P.PROF_ID, D.TS) AS z order by z.TS desc", _connection);

            _selectDevClassCommand.Parameters.Add("@ProfName", SqlDbType.NVarChar, 32);
            _selectDevClassCommand.Parameters.Add("@DevCode", SqlDbType.NVarChar, 64);
            _selectDevClassCommand.Prepare();


            _selectDevRTClassCommand.Parameters.Add("@ProfName", SqlDbType.NVarChar, 32);
            _selectDevRTClassCommand.Parameters.Add("@RTProfName", SqlDbType.NVarChar, 32);
            _selectDevRTClassCommand.Parameters.Add("@ProfBody", SqlDbType.NVarChar, 32);
            _selectDevRTClassCommand.Parameters.Add("@DevCode", SqlDbType.NVarChar, 64);
            _selectDevRTClassCommand.Prepare();

            _selectDevNormCommand =
                new SqlCommand(
                    "SELECT P.[PARAM_NAME] AS P_NID, PP.[MIN_VAL] AS P_MIN, PP.[MAX_VAL] AS P_MAX FROM [dbo].[PARAMS] P, [dbo].[PROF_PARAM] PP, [dbo].[DEVICES] D, [dbo].[PROFILES] PR WHERE PP.[PARAM_ID] = P.[PARAM_ID] AND PP.[PROF_ID] = PR.[PROF_ID] AND PR.[PROF_GUID] = D.[PROFILE_ID] AND D.[DEV_ID] = @DEV_ID",
                    _connection);
            _selectDevNormCommand.Parameters.Add("@DEV_ID", SqlDbType.Int);
            _selectDevNormCommand.Prepare();

            _devLookupPTTSelectCommand =
                new SqlCommand(
                    "SELECT P.[PTT_ID] FROM [dbo].[PROF_TEST_TYPE] P WHERE P.[PROF_ID] = @PROF_ID AND P.[TEST_TYPE_ID] = @TEST_TYPE_ID AND P.[ORD] = @ORD",
                    _connection);
            _devLookupPTTSelectCommand.Parameters.Add("@PROF_ID", SqlDbType.Int);
            _devLookupPTTSelectCommand.Parameters.Add("@TEST_TYPE_ID", SqlDbType.Int);
            _devLookupPTTSelectCommand.Parameters.Add("@ORD", SqlDbType.Int);
            _devLookupPTTSelectCommand.Prepare();

            _devLookupProfileIdSelectCommand =
                new SqlCommand("SELECT P.[PROF_ID] FROM [dbo].[PROFILES] P WHERE P.[PROF_GUID] = @PROF_GUID", _connection);
            _devLookupProfileIdSelectCommand.Parameters.Add("@PROF_GUID", SqlDbType.UniqueIdentifier);
            _devLookupProfileIdSelectCommand.Prepare();
        }

        private void PopulateDictionaries()
        {
            _errors.Clear();
            _params.Clear();
            _tests.Clear();

            using (var paramCmd = _connection.CreateCommand())
            {
                paramCmd.CommandText = "SELECT E.[ERR_ID], RTRIM(E.[ERR_NAME]) FROM [dbo].[ERRORS] E";

                using (var reader = paramCmd.ExecuteReader())
                {
                    while (reader.Read())
                        _errors.Add(((string)reader[1]), (int)reader[0]);
                }
            }

            using (var paramCmd = _connection.CreateCommand())
            {
                paramCmd.CommandText = "SELECT P.[PARAM_ID], RTRIM(P.[PARAM_NAME]) FROM [dbo].[PARAMS] P";

                using (var reader = paramCmd.ExecuteReader())
                {
                    while (reader.Read())
                        _params.Add(((string)reader[1]), (int)reader[0]);
                }
            }

            using (var paramCmd = _connection.CreateCommand())
            {
                paramCmd.CommandText = "SELECT T.[TEST_TYPE_ID], T.[TEST_TYPE_NAME] FROM [dbo].[TEST_TYPE] T";

                using (var reader = paramCmd.ExecuteReader())
                {
                    while (reader.Read())
                        _tests.Add(((string)reader[1]), (int)reader[0]);
                }
            }
        }

        public List<DeviceLocalItem> GetUnsendedDevices()
        {
            return null;
        }

        public virtual void SetResultSended(long deviceId) { }

        #region THE ONLY USEFUL CODE

        public bool SaveResults(DeviceLocalItem localDevice)
        {
            var trans = _connection.BeginTransaction();

            try
            {
                var deviceId = InsertDevice(localDevice, trans);
                InsertErrors(localDevice.ErrorCodes, deviceId, trans);
                InsertParameters(localDevice.DeviceParameters, localDevice.ProfileKey, deviceId, trans);

                trans.Commit();
                return true;
            }
            catch (Exception)
            {
                trans.Rollback();
                return false;
            }
        }

        private long InsertDevice(DeviceLocalItem localItem, SqlTransaction trans)
        {
            var groupId = GetOrMakeGroupId(localItem.GroupName, trans);

            _deviceSelectCmd.Parameters["@CODE"].Value = localItem.Code;
            _deviceSelectCmd.Parameters["@GROUP_ID"].Value = groupId;
            _deviceSelectCmd.Parameters["@POS"].Value = (localItem.Position == 2);
            _deviceSelectCmd.Parameters["@PROF_ID"].Value = localItem.ProfileKey;
            _deviceSelectCmd.Transaction = trans;
            var devId = _deviceSelectCmd.ExecuteScalar();

            if (devId != null)
            {
                _deviceDeleteCmd.Transaction = trans;
                _deviceDeleteCmd.Parameters["@DEV_ID"].Value = devId;
                _deviceDeleteCmd.ExecuteNonQuery();
            }

            _devInsertCommand.Parameters["@GROUP_ID"].Value = groupId;
            _devInsertCommand.Parameters["@PROFILE_ID"].Value = localItem.ProfileKey;
            _devInsertCommand.Parameters["@CODE"].Value = localItem.Code;
            
            if (localItem.StructureOrd != null)
                _devInsertCommand.Parameters["@SIL_N_1"].Value = localItem.StructureOrd;
            else
                _devInsertCommand.Parameters["@SIL_N_1"].Value = DBNull.Value;
            
            if (localItem.StructureId != null)
                _devInsertCommand.Parameters["@SIL_N_2"].Value = localItem.StructureId;
            else
                _devInsertCommand.Parameters["@SIL_N_2"].Value = DBNull.Value;
            
            _devInsertCommand.Parameters["@TS"].Value = localItem.Timestamp;
            _devInsertCommand.Parameters["@USR"].Value = localItem.UserName;
            _devInsertCommand.Parameters["@POS"].Value = (localItem.Position == 2);
            _devInsertCommand.Parameters["@MME_CODE"].Value = localItem.MmeCode;
            _devInsertCommand.Transaction = trans;

            return (int)_devInsertCommand.ExecuteScalar();
        }

        private void InsertErrors(IEnumerable<long> errorCodes, long devId, SqlTransaction trans)
        {
            _devErrInsertCommand.Parameters["@DEV_ID"].Value = devId;
            _devErrInsertCommand.Transaction = trans;

            foreach (var errorCode in errorCodes)
            {
                _devErrInsertCommand.Parameters["@ERR_ID"].Value = errorCode;
                _devErrInsertCommand.ExecuteNonQuery();
            }
        }

        private void InsertParameters(Dictionary<TestTypeLocalItem, List<DeviceParametersLocalItem>> deviceParameters,Guid profileKey, long devId, SqlTransaction trans)
        {
            foreach(var i in deviceParameters)
            {
                var testTypeId = CalcTestTypeID(profileKey, i.Key.Name, i.Key.Order, trans);
                foreach(var j in i.Value)
                    InsertParameterValue(devId, j.Name, j.Value, testTypeId, trans);
            }
        }

        private int LookupProfileId(Guid profileGuid, SqlTransaction trans)
        {
            _devLookupProfileIdSelectCommand.Parameters["@PROF_GUID"].Value = profileGuid;
            _devLookupProfileIdSelectCommand.Transaction = trans;

            var res = _devLookupProfileIdSelectCommand.ExecuteScalar();

            if (res != null)
                return (int)res;

            return 0;
        }

        private int LookupPTT(int profileId, int testType, int ord, SqlTransaction trans)
        {
            _devLookupPTTSelectCommand.Parameters["@PROF_ID"].Value = profileId;
            _devLookupPTTSelectCommand.Parameters["@TEST_TYPE_ID"].Value = testType;
            _devLookupPTTSelectCommand.Parameters["@ORD"].Value = ord;
            _devLookupPTTSelectCommand.Transaction = trans;

            var res = _devLookupPTTSelectCommand.ExecuteScalar();

            if (res != null)
                return (int)res;

            return 0;
        }

        #endregion

        public void Dispose()
        {
            if (_connection != null && _connection.State == ConnectionState.Open)
                _connection.Close();
        }

        #region WriteResults

        public void WriteResults(ResultItem result, IEnumerable<string> errors)
        {
            try
            {
                if (_connection != null && _connection.State == ConnectionState.Open)
                {
                    //смотрим с чем мы имеем дело: либо с PSE, либо с PSD. сразу оба параметра result.PseJob и result.PsdJob заполнены быть не могут
                    if (!String.IsNullOrWhiteSpace(result.PsdJob) && !String.IsNullOrWhiteSpace(result.PseJob))
                        throw new ArgumentException(@"Only one of result.PsdJob or result.PseJob can be filled. In fact both parameters are filled.");

                    if (!String.IsNullOrWhiteSpace(result.PsdSerialNumber) && !String.IsNullOrWhiteSpace(result.PseNumber))
                        throw new ArgumentException(@"Only one of result.PsdSerialNumber or result.PseNumber can be filled. In fact both parameters are filled.");

                    string groupName = String.IsNullOrWhiteSpace(result.PsdJob) ? result.PseJob : result.PsdJob;
                    string code = String.IsNullOrWhiteSpace(result.PsdSerialNumber) ? result.PseNumber : result.PsdSerialNumber;

                    var trans = _connection.BeginTransaction();

                    try
                    {
                        var devId = InsertDevice(code, GetOrMakeGroupId(groupName, trans), result.ProfileKey, result, trans);
                        InsertErrors(errors, devId, trans);
                        InsertParameterValues(result, devId, trans);

                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new FaultException<FaultData>(new FaultData(){}, new FaultReason(ex.ToString()));
            }
        }

        private long GetProfileId(Guid profileKey, SqlTransaction trans)
        {
            _profileSelectCommand.Transaction = trans;
            _profileSelectCommand.Parameters["@PROF_GUID"].Value = profileKey;
            var possibleProfileId = _profileSelectCommand.ExecuteScalar();

            if (possibleProfileId == null)
                throw new ArgumentException(@"No such profile has been found", nameof(profileKey));

            return (int)possibleProfileId;
        }

        public int? ReadDeviceRTClass(string devCode, string profileName)
        {
            //считывает класс самого свежего изделия, имеющего номер devCode, измеренного по профилю с обозначением profileName
            //возвращает:
            //            null - по запрошенным devCode и profileName класс RT вычислить не возможно;
            //            -1 - ошибка в данной реализации: вместо нуля записей или единственной записи считано более одной записи;
            //            класс - целое положительное число - успешное вычисление класса

            lock (MsLocker)
            {
                if (_connection == null || _connection.State != ConnectionState.Open)
                    return null;

                _selectDevRTClassCommand.Parameters["@ProfName"].Value = profileName;
                _selectDevRTClassCommand.Parameters["@RTProfName"].Value = SCME.Types.Profiles.ProfileRoutines.MakeRT(profileName);
                _selectDevRTClassCommand.Parameters["@ProfBody"].Value = SCME.Types.Profiles.ProfileRoutines.ProfileRTBodyByProfileName(profileName).Replace('*', '%');  //string.Format("%{0}%", SCME.Types.Profiles.ProfileRoutines.ProfileRTBodyByProfileName(profileName));
                _selectDevRTClassCommand.Parameters["@DevCode"].Value = devCode;

                using (var reader = _selectDevRTClassCommand.ExecuteReader())
                {
                    int deviceClassFieldID = reader.GetOrdinal("DEVICECLASS");

                    if (reader.Read())
                    {
                        var res = reader.GetValue(deviceClassFieldID);
                        return (res == DBNull.Value) ? -1 : (int?)res;
                    }

                }
            }

            return null;
        }

        public int? ReadDeviceClass(string devCode, string profileName)
        {
            //чтение фактического класса изделия по принятым devCode, profileName
            //возвращает:
            //            null - по запрошенным devCode и profileName класс изделия вычислить не возможно;
            //            -1 - ошибка в данной реализации: вместо нуля записей или единственной записи считано более одной записи;
            //            класс - целое положительное число - успешное вычисление класса

            lock (MsLocker)
            {
                if (_connection == null || _connection.State != ConnectionState.Open)
                    return null;

                _selectDevClassCommand.Parameters["@ProfName"].Value = profileName;
                _selectDevClassCommand.Parameters["@DevCode"].Value = devCode;

                using (var reader = _selectDevClassCommand.ExecuteReader())
                {
                    int deviceClassFieldID = reader.GetOrdinal("DEVICECLASS");

                    if (reader.Read())
                    {
                        var res = reader.GetValue(deviceClassFieldID);
                        return (res == DBNull.Value) ? -1 : (int?)res;
                    }

                }
            }

            return null;
        }

        private long GetOrMakeGroupId(string groupName, SqlTransaction trans)
        {
            long groupId;

            _groupSelectCommand.Parameters["@GROUP_NAME"].Value = groupName;
            _groupSelectCommand.Transaction = trans;
            var possibleGroupId = _groupSelectCommand.ExecuteScalar();

            if (possibleGroupId == null)
            {
                _groupInsertCommand.Parameters["@GROUP_NAME"].Value = groupName;
                _groupInsertCommand.Transaction = trans;
                groupId = (int)_groupInsertCommand.ExecuteScalar();
            }
            else
                groupId = (int)possibleGroupId;

            return groupId;
        }

        private long CalcTestTypeID(Guid profileKey, string testTypeName, long order, SqlTransaction trans)
        {
            //вычисление идентификатора DEV_PARAM.TEST_TYPE_ID
            _testTypeIDSelectCommand.Transaction = trans;
            _testTypeIDSelectCommand.Parameters["@PROF_ID"].Value = GetProfileId(profileKey, trans);
            _testTypeIDSelectCommand.Parameters["@TEST_TYPE_ID"].Value = _tests[testTypeName];
            _testTypeIDSelectCommand.Parameters["@ORDER"].Value = order;

            var testTypeId = _testTypeIDSelectCommand.ExecuteScalar();

            if (testTypeId == null)
                throw new ArgumentException($@"No such PROF_TEST_TYPE.PTT_ID identifiers has been found for {testTypeName} test", nameof(profileKey));

            return (int) testTypeId;
        }
        

        private void InsertErrors(IEnumerable<string> errors, long devId, SqlTransaction trans)
        {
            _devErrInsertCommand.Parameters["@DEV_ID"].Value = devId;
            _devErrInsertCommand.Transaction = trans;
            
            foreach (var error in errors.Distinct())
            {
                _devErrInsertCommand.Parameters["@ERR_ID"].Value = _errors[error];
                _devErrInsertCommand.ExecuteNonQuery();
            }
        }

        protected virtual long InsertDevice(string code, long groupId, Guid profileId, ResultItem result, SqlTransaction trans)
        {
            _deviceSelectCmd.Transaction = trans;
            _deviceSelectCmd.Parameters["@CODE"].Value = code;
            _deviceSelectCmd.Parameters["@GROUP_ID"].Value = groupId;
            _deviceSelectCmd.Parameters["@POS"].Value = (result.Position == 2);
            _deviceSelectCmd.Parameters["@PROF_ID"].Value = profileId;

            var devId = _deviceSelectCmd.ExecuteScalar();

            if (devId != null)
            {
                _deviceDeleteCmd.Transaction = trans;
                _deviceDeleteCmd.Parameters["@DEV_ID"].Value = devId;
                _deviceDeleteCmd.ExecuteNonQuery();
            }

            _devInsertCommand.Transaction = trans;
            _devInsertCommand.Parameters["@GROUP_ID"].Value = groupId;
            _devInsertCommand.Parameters["@PROFILE_ID"].Value = profileId;
            _devInsertCommand.Parameters["@CODE"].Value = code;
            _devInsertCommand.Parameters["@SIL_N_1"].Value = DBNull.Value;
            _devInsertCommand.Parameters["@SIL_N_2"].Value = DBNull.Value;
            _devInsertCommand.Parameters["@TS"].Value = DateTime.Now;
            _devInsertCommand.Parameters["@USR"].Value = result.User;
            _devInsertCommand.Parameters["@POS"].Value = (result.Position == 2);
            _devInsertCommand.Parameters["@MME_CODE"].Value = result.MmeCode;

            return (int)_devInsertCommand.ExecuteScalar();
        }

        private void InsertParameterValues(ResultItem result, long devId, SqlTransaction trans)
        {
            InsertClampingParameterValues(result, devId, trans);
            InsertDvdtParameterValues(result, devId, trans);
            InsertGateParameterValues(result, devId, trans);
            InsertVtmParameterValues(result, devId, trans);
            InsertBvtParameterValues(result, devId, trans);
            InsertAtuParameterValues(result, devId, trans);
            InsertQrrTqParameterValues(result, devId, trans);
            InsertTOUParameterValues(result, devId, trans);
        }

        private void InsertClampingParameterValues(ResultItem result, long devId, SqlTransaction trans)
        {
            if (result.IsHeightMeasureEnabled)
            {
                const long orderClamp = 1L;
                var testTypeId = CalcTestTypeID(result.ProfileKey, "Clamping", orderClamp, trans);
                InsertParameterValue(devId, "IsHeightOk", result.IsHeightOk ? 1 : 0, testTypeId, trans);
            }
        }


        private void InsertGateParameterValues(ResultItem result, long devId, SqlTransaction trans)
        {
            for (var i = 0; i < result.GateTestParameters.Length; i++)
            {
                if (result.GateTestParameters[i].IsEnabled)
                {
                    var order = result.GateTestParameters[i].Order;
                    var testTypeId = CalcTestTypeID(result.ProfileKey, "Gate", order, trans);

                    InsertParameterValue(devId, "K", result.Gate[i].IsKelvinOk ? 1 : 0, testTypeId, trans);
                    InsertParameterValue(devId, "RG", result.Gate[i].Resistance, testTypeId, trans);
                    InsertParameterValue(devId, "IGT", result.Gate[i].IGT, testTypeId, trans);
                    InsertParameterValue(devId, "VGT", result.Gate[i].VGT, testTypeId, trans);

                    if (result.GateTestParameters[i].IsIhEnabled)
                        InsertParameterValue(devId, "IH", result.Gate[i].IH, testTypeId, trans);
                    if (result.GateTestParameters[i].IsIlEnabled)
                        InsertParameterValue(devId, "IL", result.Gate[i].IL, testTypeId, trans);
                }
            }
        }

        private void InsertDvdtParameterValues(ResultItem result, long devId, SqlTransaction trans)
        {
            for (var i = 0; i < result.DvdTestParameterses.Length; i++)
            {
                if (result.DvdTestParameterses[i].IsEnabled)
                {
                    var order = result.DvdTestParameterses[i].Order;
                    var testTypeId = CalcTestTypeID(result.ProfileKey, "Dvdt", order, trans);
                    InsertParameterValue(devId, "DVDT_OK", result.DVDT[i].Passed ? 1 : 0, testTypeId, trans);
                }
            }
        }

        private void InsertVtmParameterValues(ResultItem result, long devId, SqlTransaction trans)
        {
            for (var i = 0; i < result.VTMTestParameters.Length; i++)
            {
                if (result.VTMTestParameters[i].IsEnabled)
                {
                    var order = result.VTMTestParameters[i].Order;
                    var testTypeId = CalcTestTypeID(result.ProfileKey, "SL", order, trans);
                    InsertParameterValue(devId, "VTM", result.VTM[i].Voltage, testTypeId, trans);
                }
            }
        }

        private void InsertBvtParameterValues(ResultItem result, long devId, SqlTransaction trans)
        {
            for (var i = 0; i < result.BVTTestParameters.Length; i++)
            {
                if (result.BVTTestParameters[i].IsEnabled)
                {
                    var order = result.BVTTestParameters[i].Order;
                    var testTypeId = CalcTestTypeID(result.ProfileKey, "BVT", order, trans);
                    switch (result.BVTTestParameters[i].MeasurementMode)
                    {
                        case BVTMeasurementMode.ModeI:
                            switch (result.BVTTestParameters[i].TestType)
                            {
                                case BVTTestType.Both:
                                    InsertParameterValue(devId, "IDRM", result.BVT[i].IDRM, testTypeId, trans);
                                    InsertParameterValue(devId, "IRRM", result.BVT[i].IRRM, testTypeId, trans);
                                    break;
                                case BVTTestType.Direct:
                                    InsertParameterValue(devId, "IDRM", result.BVT[i].IDRM, testTypeId, trans);
                                    break;
                                case BVTTestType.Reverse:
                                    InsertParameterValue(devId, "IRRM", result.BVT[i].IRRM, testTypeId, trans);
                                    break;
                            }

                            break;
                        case BVTMeasurementMode.ModeV:
                            switch (result.BVTTestParameters[i].TestType)
                            {
                                case BVTTestType.Both:
                                    InsertParameterValue(devId, "VDRM", result.BVT[i].VDRM, testTypeId, trans);
                                    InsertParameterValue(devId, "VRRM", result.BVT[i].VRRM, testTypeId, trans);
                                    break;
                                case BVTTestType.Direct:
                                    InsertParameterValue(devId, "VDRM", result.BVT[i].VDRM, testTypeId, trans);
                                    break;
                                case BVTTestType.Reverse:
                                    InsertParameterValue(devId, "VRRM", result.BVT[i].VRRM, testTypeId, trans);
                                    break;
                            }

                            break;
                    }

                    if (result.BVTTestParameters[i].UseUdsmUrsm)
                    {
                        switch (result.BVTTestParameters[i].TestType)
                        {
                            case BVTTestType.Both:
                                InsertParameterValue(devId, "VDSM", result.BVT[i].VDSM, testTypeId, trans);
                                InsertParameterValue(devId, "IDSM", result.BVT[i].IDSM, testTypeId, trans);

                                InsertParameterValue(devId, "VRSM", result.BVT[i].VRSM, testTypeId, trans);
                                InsertParameterValue(devId, "IRSM", result.BVT[i].IRSM, testTypeId, trans);
                                break;

                            case BVTTestType.Direct:
                                InsertParameterValue(devId, "VDSM", result.BVT[i].VDSM, testTypeId, trans);
                                InsertParameterValue(devId, "IDSM", result.BVT[i].IDSM, testTypeId, trans);
                                break;

                            case BVTTestType.Reverse:
                                InsertParameterValue(devId, "VRSM", result.BVT[i].VRSM, testTypeId, trans);
                                InsertParameterValue(devId, "IRSM", result.BVT[i].IRSM, testTypeId, trans);
                                break;
                        }
                    }
                }
            }
        }

        private void InsertAtuParameterValues(ResultItem result, long devId, SqlTransaction trans)
        {
            for (var i = 0; i < result.ATUTestParameters.Length; i++)
            {
                if (result.ATUTestParameters[i].IsEnabled)
                {
                    var order = result.ATUTestParameters[i].Order;
                    var testTypeId = CalcTestTypeID(result.ProfileKey, "ATU", order, trans);
                    InsertParameterValue(devId, "UBR", result.ATU[i].UBR, testTypeId, trans);
                    InsertParameterValue(devId, "UPRSM", result.ATU[i].UPRSM, testTypeId, trans);
                    InsertParameterValue(devId, "IPRSM", result.ATU[i].IPRSM, testTypeId, trans);
                    InsertParameterValue(devId, "PRSM", result.ATU[i].PRSM, testTypeId, trans);
                }
            }
        }

        private void InsertQrrTqParameterValues(ResultItem result, long devId, SqlTransaction trans)
        {
            for (var i = 0; i < result.QrrTqTestParameters.Length; i++)
            {
                var order = result.QrrTqTestParameters[i].Order;
                var testTypeId = CalcTestTypeID(result.ProfileKey, "QrrTq", order, trans);
                if (result.QrrTqTestParameters[i].IsEnabled)
                {
                    switch (result.QrrTq[i].Mode)
                    {
                        case TMode.Qrr:
                            InsertParameterValue(devId, "IDC", result.QrrTq[i].Idc, testTypeId, trans);
                            InsertParameterValue(devId, "QRR", result.QrrTq[i].Qrr, testTypeId, trans);
                            InsertParameterValue(devId, "IRR", result.QrrTq[i].Irr, testTypeId, trans);
                            InsertParameterValue(devId, "TRR", result.QrrTq[i].Trr, testTypeId, trans);
                            InsertParameterValue(devId, "DCFactFallRate", result.QrrTq[i].DCFactFallRate, testTypeId, trans);
                            break;
                        case TMode.QrrTq:
                            InsertParameterValue(devId, "duD/dtcrit", result.QrrTq[i].OsvRate, testTypeId, trans);
                            InsertParameterValue(devId, "IDC", result.QrrTq[i].Idc, testTypeId, trans);
                            InsertParameterValue(devId, "DCFactFallRate", result.QrrTq[i].DCFactFallRate, testTypeId, trans);
                            InsertParameterValue(devId, "TQ", result.QrrTq[i].Tq, testTypeId, trans);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        private void InsertTOUParameterValues(ResultItem result, long devId, SqlTransaction trans)
        {
            for (var i = 0; i < result.TOUTestParameters.Length; i++)
            {
                var order = result.TOUTestParameters[i].Order;
                var testTypeId = CalcTestTypeID(result.ProfileKey, "TOU", order, trans);
                if (result.TOUTestParameters[i].IsEnabled)
                {
                    InsertParameterValue(devId, "TOU_TGD", result.TOU[i].TGD, testTypeId, trans);
                    InsertParameterValue(devId, "TOU_TGT", result.TOU[i].TGT, testTypeId, trans);
                }
            }
        }

        //private void InsertParameterValue(long device, string name, float value, Guid profileKey, string testTypeName, SqlTransaction trans)
        //{

        //    _devParamInsertCommand.Parameters["@DEV_ID"].Value = device;
        //    _devParamInsertCommand.Parameters["@PARAM_ID"].Value = _params[name];
        //    _devParamInsertCommand.Parameters["@VALUE"].Value = value;
        //    _devParamInsertCommand.Parameters["@TEST_TYPE_ID"].Value = CalcTestTypeID(profileKey, testTypeName, trans);
        //    _devParamInsertCommand.Transaction = trans;
        //    _devParamInsertCommand.ExecuteNonQuery();
        //}

        private void InsertParameterValue(long device, string name, float value, long testTypeId, SqlTransaction trans)
        {
            _devParamInsertCommand.Parameters["@DEV_ID"].Value = device;
            _devParamInsertCommand.Parameters["@PARAM_ID"].Value = _params[name];
            _devParamInsertCommand.Parameters["@VALUE"].Value = value;
            _devParamInsertCommand.Parameters["@TEST_TYPE_ID"].Value = testTypeId ;
            _devParamInsertCommand.Transaction = trans;
            _devParamInsertCommand.ExecuteNonQuery();
        }

        #endregion

        #region Read functions

        public List<string> GetGroups(DateTime? @from, DateTime? to)
        {
            lock (MsLocker)
            {
                var list = new List<string>();

                if (_connection != null && _connection.State == ConnectionState.Open)
                {
                    _selectGroupsCommand.Parameters["@TS_FROM"].Value = @from ?? DateTime.MinValue;
                    _selectGroupsCommand.Parameters["@TS_TO"].Value = to ?? DateTime.Now;

                    using (var reader = _selectGroupsCommand.ExecuteReader())
                    {
                        while (reader.Read())
                            list.Add(reader.GetString(0));
                    }
                }

                return list;
            }
        }

        public List<DeviceItem> GetDevices(string @group)
        {
            try
            {
                lock (MsLocker)
                {
                    var list = new List<DeviceItem>();

                    if (_connection != null && _connection.State == ConnectionState.Open)
                    {
                        _selectDevicesCommand.Parameters["@GROUP_NAME"].Value = @group;

                        using (var reader = _selectDevicesCommand.ExecuteReader())
                        {
                            var ordID = reader.GetOrdinal("DEV_ID");
                            var ordCode = reader.GetOrdinal("CODE");
                            var ordSn1 = reader.GetOrdinal("SIL_N_1");
                            var ordSn2 = reader.GetOrdinal("SIL_N_2");
                            var ordPos = reader.GetOrdinal("POS");
                            var ordUser = reader.GetOrdinal("USR");
                            var ordTs = reader.GetOrdinal("TS");
                            var ordProf = reader.GetOrdinal("PROF_NAME");

                            while (reader.Read())
                                list.Add(new DeviceItem
                                {
                                    InternalID = reader.GetInt32(ordID),
                                    Code = reader.GetString(ordCode),
                                    StructureOrd = ((reader.GetValue(ordSn1) as System.DBNull) == System.DBNull.Value) ? string.Empty : reader.GetString(ordSn1),
                                    StructureID = ((reader.GetValue(ordSn2) as System.DBNull) == System.DBNull.Value) ? string.Empty : reader.GetString(ordSn2),
                                    Position = reader.GetBoolean(ordPos) ? 2 : 1,
                                    User = reader.GetString(ordUser),
                                    Timestamp = reader.GetDateTime(ordTs),
                                    ProfileName = reader.GetString(ordProf)
                                });
                        }
                    }

                    return list;
                }
            }
            catch (Exception ex)
            {
                throw new System.ServiceModel.FaultException<FaultData>(new FaultData()
                {
                    Device = ComplexParts.Database,
                    TimeStamp = DateTime.Now,
                    Message = new Exception("ReadDevicesFromServer -> GetDevices not work", ex).ToString()
                });
            }
        }

        public List<int> ReadDeviceErrors(long internalId)
        {
            lock (MsLocker)
            {
                var list = new List<int>();

                if (_connection != null && _connection.State == ConnectionState.Open)
                {
                    _selectDevErrCommand.Parameters["@DEV_ID"].Value = internalId;

                    using (var reader = _selectDevErrCommand.ExecuteReader())
                    {
                        while (reader.Read())
                            list.Add((int)reader[0]);
                    }
                }

                return list;
            }
        }

        public List<ParameterItem> ReadDeviceParameters(long internalId)
        {
            lock (MsLocker)
            {
                var list = new List<ParameterItem>();

                if (_connection != null && _connection.State == ConnectionState.Open)
                {
                    _selectDevParamsCommand.Parameters["@DEV_ID"].Value = internalId;

                    using (var reader = _selectDevParamsCommand.ExecuteReader())
                    {
                        var ordNid = reader.GetOrdinal("P_NID");
                        var ordName = reader.GetOrdinal("P_NAME");
                        var ordValue = reader.GetOrdinal("P_VALUE");
                        var ordHide = reader.GetOrdinal("IS_HIDE");

                        while (reader.Read())
                            list.Add(new ParameterItem
                            {
                                Name = reader.GetString(ordNid),
                                NameLocal = reader.GetString(ordName),
                                Value = reader.GetFloat(ordValue),
                                IsHide = reader.GetBoolean(ordHide)
                            });
                    }
                }

                return list;
            }
        }

        public List<ConditionItem> ReadDeviceConditions(long internalId)
        {
            lock (MsLocker)
            {
                var list = new List<ConditionItem>();

                if (_connection != null && _connection.State == ConnectionState.Open)
                {
                    _selectDevCondsCommand.Parameters["@DEV_ID"].Value = internalId;

                    using (var reader = _selectDevCondsCommand.ExecuteReader())
                    {
                        var ordNid = reader.GetOrdinal("C_NID");
                        var ordName = reader.GetOrdinal("C_NAME");
                        var ordValue = reader.GetOrdinal("C_VALUE");
                        var ordIt = reader.GetOrdinal("IS_TECH");

                        while (reader.Read())
                            list.Add(new ConditionItem
                            {
                                Name = reader.GetString(ordNid),
                                NameLocal = reader.GetString(ordName),
                                Value = reader.GetString(ordValue),
                                IsTech = reader.GetBoolean(ordIt)
                            });
                    }
                }

                return list;
            }
        }

        public List<ParameterNormativeItem> ReadDeviceNormatives(long internalId)
        {
            lock (MsLocker)
            {
                var list = new List<ParameterNormativeItem>();

                if (_connection == null || _connection.State != ConnectionState.Open)
                    return list;

                _selectDevNormCommand.Parameters["@DEV_ID"].Value = internalId;

                using (var reader = _selectDevNormCommand.ExecuteReader())
                {
                    var ordNid = reader.GetOrdinal("P_NID");
                    var ordMin = reader.GetOrdinal("P_MIN");
                    var ordMax = reader.GetOrdinal("P_MAX");

                    while (reader.Read())
                        list.Add(new ParameterNormativeItem
                        {
                            Name = reader.GetString(ordNid),
                            Min = reader.IsDBNull(ordMin) ? (float?)null : reader.GetFloat(ordMin),
                            Max = reader.IsDBNull(ordMax) ? (float?)null : reader.GetFloat(ordMax)
                        });
                }

                return list;
            }
        }

        #endregion
    }
}
