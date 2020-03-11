using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.ServiceModel;
using SCME.Types;
using SCME.Types.BVT;
using SCME.Types.DataContracts;
using SCME.Types.Interfaces;

namespace SCME.InterfaceImplementations
{
    public class SQLiteResultsServiceServer : IResultsService
    {
        protected readonly SQLiteConnection _connection;
        private readonly Dictionary<string, long> _errors;
        private readonly Dictionary<string, long> _params;
        private readonly Dictionary<string, long> _tests;
        protected static readonly object MsLocker = new object();

        private SQLiteCommand _devLookupPTTSelectCommand;
        private SQLiteCommand _devLookupProfileIdSelectCommand;

        public SQLiteResultsServiceServer(string databasePath)
        {
            _connection = new SQLiteConnection(databasePath, false);
            _errors = new Dictionary<string, long>(64);
            _params = new Dictionary<string, long>(64);
            _tests = new Dictionary<string, long>(20);
            _connection.Open();

            PopulateDictionaries();

              _devLookupPTTSelectCommand =
                new SQLiteCommand(
                    "SELECT P.[ID] FROM [PROF_TEST_TYPE] P WHERE P.[PROF_ID] = @PROF_ID AND P.[TEST_TYPE_ID] = @TEST_TYPE_ID AND P.[ORDER] = @ORD",
                    _connection);
            _devLookupPTTSelectCommand.Parameters.Add("@PROF_ID", DbType.Int32);
            _devLookupPTTSelectCommand.Parameters.Add("@TEST_TYPE_ID", DbType.Int32);
            _devLookupPTTSelectCommand.Parameters.Add("@ORD", DbType.Int32);
            _devLookupPTTSelectCommand.Prepare();

            _devLookupProfileIdSelectCommand =
                new SQLiteCommand("SELECT P.[PROF_ID] FROM [PROFILES] P WHERE P.[PROF_GUID] = @PROF_GUID", _connection);
            _devLookupProfileIdSelectCommand.Parameters.Add("@PROF_GUID", DbType.Guid);
            _devLookupProfileIdSelectCommand.Prepare();
        }

        private void PopulateDictionaries()
        {
            _errors.Clear();
            _params.Clear();
            _tests.Clear();

            using (var paramCmd = _connection.CreateCommand())
            {
                paramCmd.CommandText = "SELECT E.ERR_ID, E.ERR_NAME FROM ERRORS E";

                using (var reader = paramCmd.ExecuteReader())
                {
                    while (reader.Read())
                        _errors.Add((string)reader[1], (long)reader[0]);
                }
            }

            using (var paramCmd = _connection.CreateCommand())
            {
                paramCmd.CommandText = "SELECT P.PARAM_ID, P.PARAM_NAME FROM PARAMS P";

                using (var reader = paramCmd.ExecuteReader())
                {
                    while (reader.Read())
                        _params.Add((string)reader[1], (long)reader[0]);
                }
            }

            using (var paramCmd = _connection.CreateCommand())
            {
                paramCmd.CommandText = "SELECT T.ID, T.NAME FROM TEST_TYPE T";

                using (var reader = paramCmd.ExecuteReader())
                {
                    while (reader.Read())
                        _tests.Add(((string)reader[1]), Convert.ToInt32(reader[0]));
                }
            }
        }
        
        public int? ReadDeviceRTClass(string devCode, string profName)
        {
            //нет смысла вычислять RT класс по данным одной локально расположенной базы данных
            return null;
        }

        public int? ReadDeviceClass(string devCode, string profName)
        {
            //нет смысла вычислять класс по данным одной локально расположенной базы данных
            return null;
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
                        var devId = InsertDevice(result, code, GetOrMakeGroupId(groupName), result.ProfileKey, trans);
                        InsertErrors(errors, devId, trans);
                        InsertParameterValues(result, devId, trans);
                        

                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
             catch (Exception ex)
            {
                throw new FaultException(ex.ToString());
            }
        }

          private int LookupProfileId(Guid profileGuid, SQLiteTransaction trans)
        {
            _devLookupProfileIdSelectCommand.Parameters["@PROF_GUID"].Value = profileGuid;
            _devLookupProfileIdSelectCommand.Transaction = trans;

            var res = _devLookupProfileIdSelectCommand.ExecuteScalar();

            if (res != null)
                return Convert.ToInt32(res);

            return 0;
        }

        private int LookupPTT(int profileId, int testType, int ord, SQLiteTransaction trans)
        {
            _devLookupPTTSelectCommand.Parameters["@PROF_ID"].Value = profileId;
            _devLookupPTTSelectCommand.Parameters["@TEST_TYPE_ID"].Value = testType;
            _devLookupPTTSelectCommand.Parameters["@ORD"].Value = ord;
            _devLookupPTTSelectCommand.Transaction = trans;

            var res = _devLookupPTTSelectCommand.ExecuteScalar();

            if (res != null)
                return Convert.ToInt32(res);

            return 0;
        }

        private long GetProfileId(Guid profileKey)
        {
            var profileSelectCommand = new SQLiteCommand("SELECT P.PROF_ID FROM PROFILES P WHERE P.PROF_GUID = @PROF_GUID", _connection);
            profileSelectCommand.Parameters.Add("@PROF_GUID", DbType.Guid);
            profileSelectCommand.Prepare();
            profileSelectCommand.Parameters["@PROF_GUID"].Value = profileKey;
            var possibleProfileId = profileSelectCommand.ExecuteScalar();

            if (possibleProfileId == null)
                throw new ArgumentException(@"No such baseTestParametersAndNormatives has been found", "profileKey");

            return (long)possibleProfileId;
        }

        private long GetOrMakeGroupId(string groupName)
        {
            long groupId;

            var groupSelectCommand = new SQLiteCommand("SELECT G.GROUP_ID FROM GROUPS G WHERE G.GROUP_NAME = @GROUP_NAME", _connection);
            groupSelectCommand.Parameters.Add("@GROUP_NAME", DbType.StringFixedLength);
            groupSelectCommand.Prepare();

            groupSelectCommand.Parameters["@GROUP_NAME"].Value = groupName;
            var possibleGroupId = groupSelectCommand.ExecuteScalar();

            if (possibleGroupId == null)
            {
                var groupInsertCommand = new SQLiteCommand("INSERT INTO GROUPS(GROUP_ID, GROUP_NAME) VALUES(NULL, @GROUP_NAME)", _connection);
                groupInsertCommand.Parameters.Add("@GROUP_NAME", DbType.StringFixedLength);
                groupInsertCommand.Prepare();
                groupInsertCommand.Parameters["@GROUP_NAME"].Value = groupName;
                groupInsertCommand.ExecuteNonQuery();
                groupId = _connection.LastInsertRowId;
            }
            else
                groupId = (long)possibleGroupId;

            return groupId;
        }

        private void InsertErrors(IEnumerable<string> errors, long devId, SQLiteTransaction trans)
        {
            var devErrInsertCommand = new SQLiteCommand("INSERT INTO DEV_ERR(DEV_ID, ERR_ID) VALUES(@DEV_ID, @ERR_ID)", _connection);
            devErrInsertCommand.Parameters.Add("@DEV_ID", DbType.Int64);
            devErrInsertCommand.Parameters.Add("@ERR_ID", DbType.Int64);
            devErrInsertCommand.Prepare();
            devErrInsertCommand.Transaction = trans;
            devErrInsertCommand.Parameters["@DEV_ID"].Value = devId;
            foreach (var error in errors)
            {
                devErrInsertCommand.Parameters["@ERR_ID"].Value = _errors[error];
                devErrInsertCommand.ExecuteNonQuery();
            }
        }

        protected virtual long InsertDevice(ResultItem result, string code, long groupId, Guid profileId, SQLiteTransaction trans)
        {
            var deviceSelectCmd = new SQLiteCommand("SELECT D.DEV_ID FROM DEVICES D WHERE D.CODE = @CODE AND D.GROUP_ID = @GROUP_ID AND D.POS = @POS AND D.PROFILE_ID = @PROF_ID", _connection);
            deviceSelectCmd.Parameters.Add("@CODE", DbType.String);
            deviceSelectCmd.Parameters.Add("@GROUP_ID", DbType.Int64);
            deviceSelectCmd.Parameters.Add("@POS", DbType.Boolean);
            deviceSelectCmd.Parameters.Add("@PROF_ID", DbType.Guid);
            deviceSelectCmd.Prepare();
            deviceSelectCmd.Parameters["@CODE"].Value = code;
            deviceSelectCmd.Parameters["@GROUP_ID"].Value = groupId;
            deviceSelectCmd.Parameters["@POS"].Value = (result.Position == 2);
            deviceSelectCmd.Parameters["@PROF_ID"].Value = profileId;
            deviceSelectCmd.Transaction = trans;
            var devId = deviceSelectCmd.ExecuteScalar();

            if (devId != null)
            {
                var devParamDeleteCmd = new SQLiteCommand("DELETE FROM DEV_PARAM WHERE DEV_ID = @DEV_ID", _connection);
                devParamDeleteCmd.Parameters.Add("@DEV_ID", DbType.Int64);
                devParamDeleteCmd.Prepare();
                devParamDeleteCmd.Parameters["@DEV_ID"].Value = devId;
                devParamDeleteCmd.ExecuteNonQuery();

                var devErrDeleteCommand = new SQLiteCommand("DELETE FROM DEV_ERR WHERE DEV_ID = @DEV_ID", _connection);
                devErrDeleteCommand.Parameters.Add("@DEV_ID", DbType.Int64);
                devErrDeleteCommand.Prepare();
                devErrDeleteCommand.Parameters["@DEV_ID"].Value = devId;
                devErrDeleteCommand.ExecuteNonQuery();
                
                var deviceDeleteCmd = new SQLiteCommand("DELETE FROM DEVICES WHERE DEV_ID = @DEV_ID", _connection);
                deviceDeleteCmd.Parameters.Add("@DEV_ID", DbType.Int64);
                deviceDeleteCmd.Prepare();
                deviceDeleteCmd.Parameters["@DEV_ID"].Value = devId;
                deviceDeleteCmd.ExecuteNonQuery();
            }

            var devInsertCommand = new SQLiteCommand("INSERT INTO DEVICES(DEV_ID, GROUP_ID, PROFILE_ID, CODE, SIL_N_1, SIL_N_2, TS, USR, POS, IsSendedToServer, MmeCode) VALUES(NULL, @GROUP_ID, @PROFILE_ID, @CODE, @SIL_N_1, @SIL_N_2, @TS, @USR, @POS, 1, @MmeCode)", _connection);
            devInsertCommand.Parameters.Add("@GROUP_ID", DbType.Int64);
            devInsertCommand.Parameters.Add("@PROFILE_ID", DbType.Guid);
            devInsertCommand.Parameters.Add("@CODE", DbType.String);
            devInsertCommand.Parameters.Add("@SIL_N_1", DbType.String);
            devInsertCommand.Parameters.Add("@SIL_N_2", DbType.String);
            devInsertCommand.Parameters.Add("@TS", DbType.String);
            devInsertCommand.Parameters.Add("@USR", DbType.StringFixedLength);
            devInsertCommand.Parameters.Add("@POS", DbType.Boolean);
            devInsertCommand.Parameters.Add("@MmeCode", DbType.String);
            devInsertCommand.Prepare();

            devInsertCommand.Parameters["@GROUP_ID"].Value = groupId;
            devInsertCommand.Parameters["@PROFILE_ID"].Value = profileId;
            devInsertCommand.Parameters["@CODE"].Value = code;
            devInsertCommand.Parameters["@SIL_N_1"].Value = DBNull.Value;
            devInsertCommand.Parameters["@SIL_N_2"].Value = DBNull.Value;
            devInsertCommand.Parameters["@TS"].Value = DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
            devInsertCommand.Parameters["@USR"].Value = result.User;
            devInsertCommand.Parameters["@POS"].Value = (result.Position == 2);
            devInsertCommand.Parameters["@MmeCode"].Value = result.MmeCode;
            devInsertCommand.ExecuteNonQuery();

            return _connection.LastInsertRowId;
        }

        private void InsertParameterValues(ResultItem result, long devId, SQLiteTransaction trans)
        {
            if (result.IsHeightMeasureEnabled)
                InsertParameterValue(devId, "IsHeightOk", result.IsHeightOk ? 1 : 0, result.ProfileKey, 
                    LookupPTT(LookupProfileId(result.ProfileKey, trans), Convert.ToInt32(_tests["Clamping"]), 1, trans), 1, trans);

            InsertDvdtParameterValues(result, devId, trans);
            InsertGateParameterValues(result, devId, trans);
            InsertVtmParameterValues(result, devId, trans);
            InsertBvtParameterValues(result, devId, trans);
            InsertAtuParameterValues(result, devId, trans);
            InsertQrrTqParameterValues(result, devId, trans);
            InsertTOUParameterValues(result, devId, trans);
        }

        private void InsertDvdtParameterValues(ResultItem result, long devId, SQLiteTransaction trans)
        {
            for (var i = 0; i < result.DvdTestParameterses.Length; i++)
            {
                var order = result.DvdTestParameterses[i].Order;
                InsertParameterValue(devId, "DVDT_OK", result.DVDT[i].Passed ? 1 : 0, result.ProfileKey, result.DVDT[i].TestTypeId , order, trans);
            }
        }

        private void InsertGateParameterValues(ResultItem result, long devId, SQLiteTransaction trans)
        {
            for (var i = 0; i < result.GateTestParameters.Length; i++)
            {
                var order = result.GateTestParameters[i].Order;
                InsertParameterValue(devId, "K", result.Gate[i].IsKelvinOk ? 1 : 0, result.ProfileKey, result.Gate[i].TestTypeId, order, trans);
                InsertParameterValue(devId, "RG", result.Gate[i].Resistance, result.ProfileKey, result.Gate[i].TestTypeId, order, trans);
                InsertParameterValue(devId, "IGT", result.Gate[i].IGT, result.ProfileKey, result.Gate[i].TestTypeId, order, trans);
                InsertParameterValue(devId, "VGT", result.Gate[i].VGT, result.ProfileKey, result.Gate[i].TestTypeId, order, trans);

                if (result.GateTestParameters[i].IsIhEnabled)
                    InsertParameterValue(devId, "IH", result.Gate[i].IH, result.ProfileKey, result.Gate[i].TestTypeId, order, trans);
                if (result.GateTestParameters[i].IsIlEnabled)
                    InsertParameterValue(devId, "IL", result.Gate[i].IL, result.ProfileKey, result.Gate[i].TestTypeId, order, trans);
            }
        }

        private void InsertVtmParameterValues(ResultItem result, long devId, SQLiteTransaction trans)
        {
            for (int i = 0; i < result.VTMTestParameters.Length; i++)
            {
                var order = result.VTMTestParameters[i].Order;
                if (result.VTMTestParameters[i].IsEnabled)
                    InsertParameterValue(devId, "VTM", result.VTM[i].Voltage, result.ProfileKey, result.VTM[i].TestTypeId, order, trans);
            }
        }

        private void InsertBvtParameterValues(ResultItem result, long devId, SQLiteTransaction trans)
        {
            for (int i = 0; i < result.BVTTestParameters.Length; i++)
            {
                var order = result.BVTTestParameters[i].Order;
                if (result.BVTTestParameters[i].IsEnabled)
                {
                    switch (result.BVTTestParameters[i].MeasurementMode)
                    {
                        case BVTMeasurementMode.ModeI:
                            switch (result.BVTTestParameters[i].TestType)
                            {
                                case BVTTestType.Both:
                                    InsertParameterValue(devId, "IDRM", result.BVT[i].IDRM, result.ProfileKey, result.BVT[i].TestTypeId, order, trans);
                                    InsertParameterValue(devId, "IRRM", result.BVT[i].IRRM, result.ProfileKey, result.BVT[i].TestTypeId, order, trans);
                                    break;
                                case BVTTestType.Direct:
                                    InsertParameterValue(devId, "IDRM", result.BVT[i].IDRM, result.ProfileKey, result.BVT[i].TestTypeId, order, trans);
                                    break;
                                case BVTTestType.Reverse:
                                    InsertParameterValue(devId, "IRRM", result.BVT[i].IRRM, result.ProfileKey, result.BVT[i].TestTypeId, order, trans);
                                    break;
                            }
                            break;
                        case BVTMeasurementMode.ModeV:
                            switch (result.BVTTestParameters[i].TestType)
                            {
                                case BVTTestType.Both:
                                    InsertParameterValue(devId, "VDRM", result.BVT[i].VDRM, result.ProfileKey, result.BVT[i].TestTypeId, order, trans);
                                    InsertParameterValue(devId, "VRRM", result.BVT[i].VRRM, result.ProfileKey, result.BVT[i].TestTypeId, order, trans);
                                    break;
                                case BVTTestType.Direct:
                                    InsertParameterValue(devId, "VDRM", result.BVT[i].VDRM, result.ProfileKey, result.BVT[i].TestTypeId, order, trans);
                                    break;
                                case BVTTestType.Reverse:
                                    InsertParameterValue(devId, "VRRM", result.BVT[i].VRRM, result.ProfileKey, result.BVT[i].TestTypeId, order, trans);
                                    break;
                            }
                            break;
                    }
                                       
                    if (result.BVTTestParameters[i].UseUdsmUrsm)
                    {
                        switch (result.BVTTestParameters[i].TestType)
                        {
                            case BVTTestType.Both:
                                InsertParameterValue(devId, "VDSM", result.BVT[i].VDRM, result.ProfileKey, result.BVT[i].TestTypeId, order, trans);
                                InsertParameterValue(devId, "IDSM", result.BVT[i].IDRM, result.ProfileKey, result.BVT[i].TestTypeId, order, trans);

                                InsertParameterValue(devId, "VRSM", result.BVT[i].VRRM, result.ProfileKey, result.BVT[i].TestTypeId, order, trans);
                                InsertParameterValue(devId, "IRSM", result.BVT[i].IRRM, result.ProfileKey, result.BVT[i].TestTypeId, order, trans);
                                break;

                            case BVTTestType.Direct:
                                InsertParameterValue(devId, "VDSM", result.BVT[i].VDRM, result.ProfileKey, result.BVT[i].TestTypeId, order, trans);
                                InsertParameterValue(devId, "IDSM", result.BVT[i].IDRM, result.ProfileKey, result.BVT[i].TestTypeId, order, trans);
                                break;

                            case BVTTestType.Reverse:
                                InsertParameterValue(devId, "VRSM", result.BVT[i].VRRM, result.ProfileKey, result.BVT[i].TestTypeId, order, trans);
                                InsertParameterValue(devId, "IRSM", result.BVT[i].IRRM, result.ProfileKey, result.BVT[i].TestTypeId, order, trans);
                                break;
                        }
                    }
                }
            }
        }

        private void InsertAtuParameterValues(ResultItem result, long devId, SQLiteTransaction trans)
        {
            for (int i = 0; i < result.ATUTestParameters.Length; i++)
            {
                var order = result.ATUTestParameters[i].Order;
                if (result.ATUTestParameters[i].IsEnabled)
                {
                    InsertParameterValue(devId, "UBR", result.ATU[i].UBR, result.ProfileKey, result.ATU[i].TestTypeId, order, trans);
                    InsertParameterValue(devId, "UPRSM", result.ATU[i].UPRSM, result.ProfileKey, result.ATU[i].TestTypeId, order, trans);
                    InsertParameterValue(devId, "IPRSM", result.ATU[i].IPRSM, result.ProfileKey,  result.ATU[i].TestTypeId, order, trans);
                    InsertParameterValue(devId, "PRSM", result.ATU[i].PRSM, result.ProfileKey, result.ATU[i].TestTypeId, order, trans);
                }
            }
        }

        private void InsertQrrTqParameterValues(ResultItem result, long devId, SQLiteTransaction trans)
        {
            for (int i = 0; i < result.QrrTqTestParameters.Length; i++)
            {
                var order = result.QrrTqTestParameters[i].Order;
                if (result.QrrTqTestParameters[i].IsEnabled)
                {
                    InsertParameterValue(devId, "IDC", result.QrrTq[i].Idc, result.ProfileKey, result.QrrTq[i].TestTypeId, order, trans);
                    InsertParameterValue(devId, "QRR", result.QrrTq[i].Qrr, result.ProfileKey, result.QrrTq[i].TestTypeId, order, trans);
                    InsertParameterValue(devId, "IRR", result.QrrTq[i].Irr, result.ProfileKey, result.QrrTq[i].TestTypeId, order, trans);
                    InsertParameterValue(devId, "TRR", result.QrrTq[i].Trr, result.ProfileKey, result.QrrTq[i].TestTypeId, order, trans);
                    InsertParameterValue(devId, "DCFactFallRate", result.QrrTq[i].DCFactFallRate, result.ProfileKey, result.QrrTq[i].TestTypeId, order, trans);
                    InsertParameterValue(devId, "TQ", result.QrrTq[i].Tq, result.ProfileKey, result.QrrTq[i].TestTypeId,  order, trans);
                }
            }
        }

        private void InsertTOUParameterValues(ResultItem result, long devId, SQLiteTransaction trans)
        {
            for (int i = 0; i < result.TOUTestParameters.Length; i++)
            {
                var order = result.TOUTestParameters[i].Order;
                if (result.TOUTestParameters[i].IsEnabled)
                {
                    InsertParameterValue(devId, "TOU_TGD", result.TOU[i].TGD, result.ProfileKey, result.TOU[i].TestTypeId, order, trans);
                    InsertParameterValue(devId, "TOU_TGT", result.TOU[i].TGT, result.ProfileKey, result.TOU[i].TestTypeId, order, trans);
                }
            }
        }

        //private void InsertParameterValue(long device, string name, float value, long testTypeId)
        //{
        //    var devParamInsertCommand = new SQLiteCommand("INSERT INTO DEV_PARAM(DEV_ID, PARAM_ID, VALUE,TEST_TYPE_ID) VALUES(@DEV_ID, @PARAM_ID, @VALUE,@TEST_TYPE_ID)", _connection);
        //    devParamInsertCommand.Parameters.Add("@DEV_ID", DbType.Int64);
        //    devParamInsertCommand.Parameters.Add("@PARAM_ID", DbType.Int64);
        //    devParamInsertCommand.Parameters.Add("@VALUE", DbType.Single);
        //    devParamInsertCommand.Parameters.Add("@TEST_TYPE_ID", DbType.Int64);
        //    devParamInsertCommand.Prepare();
        //    devParamInsertCommand.Parameters["@DEV_ID"].Value = device;
        //    devParamInsertCommand.Parameters["@PARAM_ID"].Value = _params[name];
        //    devParamInsertCommand.Parameters["@VALUE"].Value = value;
        //    devParamInsertCommand.Parameters["@TEST_TYPE_ID"].Value = testTypeId;
        //    devParamInsertCommand.ExecuteNonQuery();
        //}

          private void InsertParameterValue(long device, string name, float value, Guid profileKey, long testTypeId, int order, SQLiteTransaction trans)
          {
              var devParamInsertCommand = new SQLiteCommand("INSERT INTO DEV_PARAM(DEV_ID, PARAM_ID, VALUE,TEST_TYPE_ID) VALUES(@DEV_ID, @PARAM_ID, @VALUE,@TEST_TYPE_ID)", _connection);
              devParamInsertCommand.Parameters.Add("@DEV_ID", DbType.Int64);
              devParamInsertCommand.Parameters.Add("@PARAM_ID", DbType.Int64);
              devParamInsertCommand.Parameters.Add("@VALUE", DbType.Single);
              devParamInsertCommand.Parameters.Add("@TEST_TYPE_ID", DbType.Int64);
              devParamInsertCommand.Prepare();
              devParamInsertCommand.Parameters["@DEV_ID"].Value = device;
              devParamInsertCommand.Parameters["@PARAM_ID"].Value = _params[name];
              devParamInsertCommand.Parameters["@VALUE"].Value = value;
              devParamInsertCommand.Parameters["@TEST_TYPE_ID"].Value = testTypeId ;
              devParamInsertCommand.Transaction = trans;
              devParamInsertCommand.ExecuteNonQuery();
          }

        #endregion

        public List<string> GetGroups(DateTime? @from, DateTime? to)
        {
            lock (MsLocker)
            {
                var list = new List<string>();

                if (_connection != null && _connection.State == ConnectionState.Open)
                {
                    var selectGroupsCommand = new SQLiteCommand("SELECT DISTINCT G.GROUP_NAME FROM GROUPS G, DEVICES D WHERE G.GROUP_ID = D.GROUP_ID AND D.TS > @TS_FROM AND D.TS <= @TS_TO ORDER BY G.GROUP_NAME ASC", _connection);
                    selectGroupsCommand.Parameters.Add("@TS_FROM", DbType.String);
                    selectGroupsCommand.Parameters.Add("@TS_TO", DbType.String);
                    selectGroupsCommand.Prepare();

                    selectGroupsCommand.Parameters["@TS_FROM"].Value = @from.HasValue
                        ? @from.Value.ToString(@"yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)
                        : DateTime.MinValue.ToString(@"yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                    selectGroupsCommand.Parameters["@TS_TO"].Value = to.HasValue
                        ? to.Value.ToString(@"yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)
                        : DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);


                    using (var reader = selectGroupsCommand.ExecuteReader())
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
            lock (MsLocker)
            {
                var list = new List<DeviceItem>();

                if (_connection != null && _connection.State == ConnectionState.Open)
                {
                    var selectDevicesCommand = new SQLiteCommand("SELECT D.DEV_ID, D.CODE, D.SIL_N_1, D.SIL_N_2, D.TS, D.USR, D.POS, P.PROF_NAME FROM DEVICES D, GROUPS G, PROFILES P WHERE D.GROUP_ID = G.GROUP_ID AND D.PROFILE_ID = P.PROF_GUID AND G.GROUP_NAME = @GROUP_NAME ORDER BY D.CODE ASC", _connection);
                    selectDevicesCommand.Parameters.Add("@GROUP_NAME", DbType.StringFixedLength);
                    selectDevicesCommand.Prepare();

                    selectDevicesCommand.Parameters["@GROUP_NAME"].Value = @group;

                    using (var reader = selectDevicesCommand.ExecuteReader())
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
                                InternalID = reader.GetInt64(ordID),
                                Code = reader.GetString(ordCode),
                                StructureOrd = reader.GetString(ordSn1),
                                StructureID = reader.GetString(ordSn2),
                                Position = reader.GetBoolean(ordPos) ? 2 : 1,
                                User = reader.GetString(ordUser),
                                Timestamp = DateTime.Parse(reader.GetString(ordTs), CultureInfo.InvariantCulture),
                                ProfileName = reader.GetString(ordProf)
                            });
                    }
                }

                return list;
            }
        }

        public List<int> ReadDeviceErrors(long internalId)
        {
            lock (MsLocker)
            {
                var list = new List<int>();

                if (_connection != null && _connection.State == ConnectionState.Open)
                {
                    var selectDevErrCommand = new SQLiteCommand("SELECT E.ERR_CODE FROM ERRORS E, DEV_ERR DE WHERE E.ERR_ID = DE.ERR_ID AND DE.DEV_ID = @DEV_ID", _connection);
                    selectDevErrCommand.Parameters.Add("@DEV_ID", DbType.Int64);
                    selectDevErrCommand.Prepare();
                    selectDevErrCommand.Parameters["@DEV_ID"].Value = internalId;

                    using (var reader = selectDevErrCommand.ExecuteReader())
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
                    var selectDevParamsCommand = new SQLiteCommand("SELECT P.PARAM_NAME AS P_NID, P.PARAM_NAME_LOCAL AS P_NAME, DP.VALUE AS P_VALUE, P.PARAM_IS_HIDE AS IS_HIDE FROM PARAMS P, DEV_PARAM DP WHERE DP.PARAM_ID = P.PARAM_ID AND DP.DEV_ID = @DEV_ID ORDER BY P.PARAM_ID ASC", _connection);
                    selectDevParamsCommand.Parameters.Add("@DEV_ID", DbType.Int64);
                    selectDevParamsCommand.Prepare();

                    selectDevParamsCommand.Parameters["@DEV_ID"].Value = internalId;

                    using (var reader = selectDevParamsCommand.ExecuteReader())
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
                    var selectDevCondsCommand = new SQLiteCommand("SELECT C.COND_NAME AS C_NID, C.COND_NAME_LOCAL AS C_NAME, PC.VALUE AS C_VALUE, C.COND_IS_TECH AS IS_TECH FROM CONDITIONS C, PROF_COND PC, DEVICES D, PROFILES P WHERE PC.COND_ID = C.COND_ID AND PC.PROF_ID = P.PROF_ID AND P.PROF_GUID = D.PROFILE_ID AND D.DEV_ID = @DEV_ID ORDER BY C.COND_ID ASC", _connection);

                    selectDevCondsCommand.Parameters.Add("@DEV_ID", DbType.Int64);
                    selectDevCondsCommand.Prepare();
                    selectDevCondsCommand.Parameters["@DEV_ID"].Value = internalId;

                    using (var reader = selectDevCondsCommand.ExecuteReader())
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

                if (_connection == null || _connection.State != ConnectionState.Open) return list;
                var selectDevNormCommand = new SQLiteCommand("SELECT P.PARAM_NAME AS P_NID, PP.MIN_VAL AS P_MIN, PP.MAX_VAL AS P_MAX FROM PARAMS P, PROF_PARAM PP, DEVICES D, PROFILES PR WHERE PP.PARAM_ID = P.PARAM_ID AND PP.PROF_ID = PR.PROF_ID AND PR.PROF_GUID = D.PROFILE_ID AND D.DEV_ID = @DEV_ID", _connection);
                selectDevNormCommand.Parameters.Add("@DEV_ID", DbType.Int64);
                selectDevNormCommand.Prepare();

                selectDevNormCommand.Parameters["@DEV_ID"].Value = internalId;

                using (var reader = selectDevNormCommand.ExecuteReader())
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

        public virtual List<DeviceLocalItem> GetUnsendedDevices()
        {
            return new List<DeviceLocalItem>();
        }

        public virtual void SetResultSended(long deviceId) { }

        public bool SaveResults(DeviceLocalItem localDevice)
        {
            try
            {
                var deviceId = InsertDevice(localDevice);
                InsertErrors(localDevice.ErrorCodes, deviceId);
                InsertParameters(localDevice.DeviceParameters, deviceId);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private long InsertDevice(DeviceLocalItem localItem)
        {
            var groupId = GetOrMakeGroupId(localItem.GroupName);

            var devInsertCommand = new SQLiteCommand("INSERT INTO DEVICES(DEV_ID, GROUP_ID, PROFILE_ID, CODE, SIL_N_1, SIL_N_2, TS, USR, POS, IsSendedToServer, MmeCode) VALUES(NULL, @GROUP_ID, @PROFILE_ID, @CODE, @SIL_N_1, @SIL_N_2, @TS, @USR, @POS, 1, @MmeCode)", _connection);
            devInsertCommand.Parameters.Add("@GROUP_ID", DbType.Int64);
            devInsertCommand.Parameters.Add("@PROFILE_ID", DbType.Guid);
            devInsertCommand.Parameters.Add("@CODE", DbType.String);
            devInsertCommand.Parameters.Add("@SIL_N_1", DbType.String);
            devInsertCommand.Parameters.Add("@SIL_N_2", DbType.String);
            devInsertCommand.Parameters.Add("@TS", DbType.String);
            devInsertCommand.Parameters.Add("@USR", DbType.StringFixedLength);
            devInsertCommand.Parameters.Add("@POS", DbType.Boolean);
            devInsertCommand.Parameters.Add("@MmeCode", DbType.String);
            devInsertCommand.Prepare();

            devInsertCommand.Parameters["@GROUP_ID"].Value = groupId;
            devInsertCommand.Parameters["@PROFILE_ID"].Value = localItem.ProfileKey;
            devInsertCommand.Parameters["@CODE"].Value = localItem.Code;
            devInsertCommand.Parameters["@SIL_N_1"].Value = localItem.StructureOrd;
            devInsertCommand.Parameters["@SIL_N_2"].Value = localItem.StructureId;
            devInsertCommand.Parameters["@TS"].Value = DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
            devInsertCommand.Parameters["@USR"].Value = localItem.UserName;
            devInsertCommand.Parameters["@POS"].Value = (localItem.Position == 2);
            devInsertCommand.Parameters["@MmeCode"].Value = localItem.MmeCode;
            devInsertCommand.ExecuteNonQuery();

            return _connection.LastInsertRowId;
        }

        private void InsertErrors(IEnumerable<long> errorCodes, long devId)
        {
            var devErrorInsertCommand = new SQLiteCommand("INSERT INTO DEV_ERR(DEV_ID, ERR_ID) VALUES(@DEV_ID, @ERR_ID)", _connection);
            devErrorInsertCommand.Parameters.Add("@DEV_ID", DbType.Int64);
            devErrorInsertCommand.Parameters.Add("@ERR_ID", DbType.Int64);
            devErrorInsertCommand.Prepare();
            devErrorInsertCommand.Parameters["@DEV_ID"].Value = devId;
            foreach (var errorCode in errorCodes)
            {
                devErrorInsertCommand.Parameters["@ERR_ID"].Value = errorCode;
                devErrorInsertCommand.ExecuteNonQuery();
            }
        }

        private void InsertParameters(IEnumerable<DeviceParameterLocal> deviceParameters, long devId)
        {
            var devParamInsertCommand = new SQLiteCommand("INSERT INTO DEV_PARAM(DEV_ID,PARAM_ID, VALUE, TEST_TYPE_ID) VALUES(@DEV_ID, @PARAM_ID, @VALUE, @TEST_TYPE_ID)", _connection);
            devParamInsertCommand.Parameters.Add("@DEV_ID", DbType.Int64);
            devParamInsertCommand.Parameters.Add("@PARAM_ID", DbType.Int64);
            devParamInsertCommand.Parameters.Add("@VALUE", DbType.Single);
            devParamInsertCommand.Parameters.Add("@TEST_TYPE_ID", DbType.Int64);
            devParamInsertCommand.Prepare();
            devParamInsertCommand.Parameters["@DEV_ID"].Value = devId;
            foreach (var devParam in deviceParameters)
            {
                devParamInsertCommand.Parameters["@PARAM_ID"].Value = devParam.ParameterId;
                devParamInsertCommand.Parameters["@VALUE"].Value = devParam.Value;
                devParamInsertCommand.Parameters["@TEST_TYPE_ID"].Value = devParam.TestTypeId;
                devParamInsertCommand.ExecuteNonQuery();
            }
        }

        public void Dispose()
        {
            if (_connection != null && _connection.State == ConnectionState.Open)
                _connection.Close();
        }
    }
}
