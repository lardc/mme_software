using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using SCME.Types;
using SCME.Types.DataContracts;

namespace SCME.InterfaceImplementations
{
    public class SQLiteResultsServiceLocal : SQLiteResultsServiceServer
    {
        public SQLiteResultsServiceLocal(string databasePath) : base(databasePath) { }

        protected override long InsertDevice(ResultItem result, string code, long groupId, Guid profileId, SQLiteTransaction trans)
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

            var devInsertCommand = new SQLiteCommand("INSERT INTO DEVICES(DEV_ID, GROUP_ID, PROFILE_ID, CODE, SIL_N_1, SIL_N_2, TS, USR, POS, IsSendedToServer, MmeCode) VALUES(NULL, @GROUP_ID, @PROFILE_ID, @CODE, @SIL_N_1, @SIL_N_2, @TS, @USR, @POS, @IsSendedToServer, @MmeCode)", _connection);
            devInsertCommand.Parameters.Add("@GROUP_ID", DbType.Int64);
            devInsertCommand.Parameters.Add("@PROFILE_ID", DbType.Guid);
            devInsertCommand.Parameters.Add("@CODE", DbType.String);
            devInsertCommand.Parameters.Add("@SIL_N_1", DbType.String);
            devInsertCommand.Parameters.Add("@SIL_N_2", DbType.String);
            devInsertCommand.Parameters.Add("@TS", DbType.String);
            devInsertCommand.Parameters.Add("@USR", DbType.StringFixedLength);
            devInsertCommand.Parameters.Add("@POS", DbType.Boolean);
            devInsertCommand.Parameters.Add("@MmeCode", DbType.String);
            devInsertCommand.Parameters.Add("@IsSendedToServer", DbType.Boolean);
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
            devInsertCommand.Parameters["@IsSendedToServer"].Value = result.IsSentToServer;
            devInsertCommand.ExecuteNonQuery();

            return _connection.LastInsertRowId;
        }

        public override List<DeviceLocalItem> GetUnsendedDevices()
        {
            var unsendedDevices = GetDeviceLocalItems();

            int n = 0;
            foreach (var deviceLocalItem in unsendedDevices)
            {
                deviceLocalItem.ErrorCodes = GetDeviceErrors(deviceLocalItem.Id);
                deviceLocalItem.DeviceParameters = GetDeviceParameters(deviceLocalItem.ProfileKey, deviceLocalItem.Id);
                n++;
            }

            return unsendedDevices;
        }

        public override void SetResultSended(long deviceId)
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
                return;

            var updateDeviceCommand = new SQLiteCommand("UPDATE DEVICES SET IsSendedToServer = 1 WHERE DEV_ID = @DEV_ID", _connection);
            updateDeviceCommand.Parameters.Add("@DEV_ID", DbType.Int64);
            updateDeviceCommand.Prepare();
            updateDeviceCommand.Parameters["@DEV_ID"].Value = deviceId;
            updateDeviceCommand.ExecuteNonQuery();
        }

        private List<DeviceLocalItem> GetDeviceLocalItems()
        {
            lock (MsLocker)
            {
                var list = new List<DeviceLocalItem>();
                if (_connection == null || _connection.State != ConnectionState.Open) return list;

                var selectDevicesCommand =
                    new SQLiteCommand(
                        "SELECT D.*, G.GROUP_NAME FROM DEVICES D, GROUPS G WHERE D.GROUP_ID = G.GROUP_ID AND D.IsSendedToServer = 0 ORDER BY D.TS ASC",
                        _connection);

                using (var reader = selectDevicesCommand.ExecuteReader())
                {
                    var ordId = reader.GetOrdinal("DEV_ID");
                    var ordGroupName = reader.GetOrdinal("GROUP_NAME");
                    var ordProfileGuid = reader.GetOrdinal("PROFILE_ID");
                    var ordUser = reader.GetOrdinal("USR");
                    var ordMmeCode = reader.GetOrdinal("MmeCode");
                    var ordCode = reader.GetOrdinal("CODE");
                    var ordSn1 = reader.GetOrdinal("SIL_N_1");
                    var ordSn2 = reader.GetOrdinal("SIL_N_2");
                    var ordPos = reader.GetOrdinal("POS");
                    var ordTs = reader.GetOrdinal("TS");

                    while (reader.Read())
                        list.Add(new DeviceLocalItem
                        {
                            Id = reader.GetInt64(ordId),
                            GroupName = reader.GetString(ordGroupName),
                            ProfileKey = reader.GetGuid(ordProfileGuid),
                            UserName = reader.GetString(ordUser),
                            MmeCode = reader.GetString(ordMmeCode),
                            Code = reader.GetString(ordCode),
                            StructureOrd = reader.GetValue(ordSn1) == DBNull.Value ? null : reader.GetString(ordSn1) ,
                            StructureId = reader.GetValue(ordSn2) == DBNull.Value ? null : reader.GetString(ordSn2) ,
                            Position = reader.GetBoolean(ordPos) ? 2 : 1,
                            Timestamp = DateTime.Parse(reader.GetString(ordTs), CultureInfo.InvariantCulture)
                        });
                }

                return list;
            }
        }

        private IEnumerable<long> GetDeviceErrors(long deviceId)
        {
            lock (MsLocker)
            {
                var list = new List<long>();
                if (_connection == null || _connection.State != ConnectionState.Open) return list;
                var devErrorsSelectCommand = new SQLiteCommand("SELECT ER.ERR_CODE FROM ERRORS ER INNER JOIN DEV_ERR as DE ON DE.ERR_ID = ER.ERR_ID WHERE DE.DEV_ID = @DEV_ID", _connection);
                devErrorsSelectCommand.Parameters.Add("@DEV_ID", DbType.Int64);
                devErrorsSelectCommand.Prepare();
                devErrorsSelectCommand.Parameters["@DEV_ID"].Value = deviceId;
                using (var reader = devErrorsSelectCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(Convert.ToInt64(reader[0]));
                    }
                }

                return list;
            }
        }

        private Dictionary<TestTypeLocalItem, List<DeviceParametersLocalItem>> GetDeviceParameters(Guid profileKey, long devId)
        {
            lock (MsLocker)
            {
                var list = new List<(long TestTypeId, string Name, long Order)>();
                var result = new Dictionary<TestTypeLocalItem, List<DeviceParametersLocalItem>>();

                var select_PROF_TEST_TYPE = new SQLiteCommand("SELECT PTT.ID, TT.NAME, PTT.\"ORDER\" " +
                    "FROM PROF_TEST_TYPE as PTT " +
                    "INNER JOIN TEST_TYPE TT " +
                    "ON PTT.TEST_TYPE_ID = TT.ID " +
                    "WHERE PTT.PROF_ID = " +
                    "(SELECT PROF_ID FROM PROFILES WHERE PROF_GUID = @PROF_GUID ) " +
                    "ORDER by PTT.ID", _connection);
                select_PROF_TEST_TYPE.Parameters.AddWithValue("@PROF_GUID", profileKey);
                select_PROF_TEST_TYPE.Prepare();

                using (var reader = select_PROF_TEST_TYPE.ExecuteReader())
                    while (reader.Read())
                        list.Add((reader.GetInt64(0), reader.GetString(1), reader.GetInt64(2)));

                foreach (var i in list)
                {
                    var select_DEV_PARAM = new SQLiteCommand("SELECT VALUE, PARAM_NAME FROM DEV_PARAM AS DP INNER JOIN PARAMS P ON DP.PARAM_ID = P.PARAM_ID WHERE DP.TEST_TYPE_ID = @TEST_TYPE_ID AND DP.DEV_ID = @DEV_ID", _connection);
                    select_DEV_PARAM.Parameters.AddWithValue("@TEST_TYPE_ID", i.TestTypeId);
                    select_DEV_PARAM.Parameters.AddWithValue("@DEV_ID", devId);
                    select_DEV_PARAM.Prepare();
                    var j = result[new TestTypeLocalItem()
                    {
                        Name = i.Name,
                        Order = Convert.ToInt32(i.Order)

                    }] = new List<DeviceParametersLocalItem>();

                    using (var reader = select_DEV_PARAM.ExecuteReader())
                        while (reader.Read())
                            j.Add(new DeviceParametersLocalItem()
                            {
                                Name = reader.GetString(1),
                                Value = reader.GetFloat(0)
                            });
                }

                result = result.Where(m => m.Value.Count > 0).ToDictionary(m=> m.Key, m=> m.Value);

                return result;

                ////var devParamsSelectCommand = new SQLiteCommand("SELECT PARAM_ID, VALUE, TEST_TYPE_ID FROM DEV_PARAM  WHERE DEV_ID = @DEV_ID", _connection);
                //var devParamsSelectCommand = new SQLiteCommand("SELECT DP.PARAM_ID, DP.VALUE, DP.TEST_TYPE_ID, P.PARAM_NAME " +
                //                                               "FROM DEV_PARAM DP, PARAMS P " +
                //                                               "WHERE ( " +
                //                                               "       (DP.DEV_ID = @DEV_ID) AND " +
                //                                               "       (P.PARAM_ID = DP.PARAM_ID) " +
                //                                               "      )", _connection);

                //devParamsSelectCommand.Parameters.Add("@DEV_ID", DbType.Int64);
                //devParamsSelectCommand.Prepare();
                //devParamsSelectCommand.Parameters["@DEV_ID"].Value = deviceId;

                //using (var reader = devParamsSelectCommand.ExecuteReader())
                //{
                //    while (reader.Read())
                //    {
                //        list.Add(new DeviceParameterLocal
                //        {
                //            ParameterId = reader.GetInt64(0),
                //            Value = reader.GetFloat(1),
                //            TestTypeId = reader.GetInt64(2),
                //            Name = reader.GetString(3)
                //        });
                //    }
                //}

                //return list;
            }
        }
    }
}
