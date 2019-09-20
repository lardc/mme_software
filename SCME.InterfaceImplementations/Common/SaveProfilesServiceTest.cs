using SCME.Types.BaseTestParams;
using SCME.Types.Profiles;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SCME.InterfaceImplementations.Common
{
    public class SaveProfilesServiceTest<TDbCommand, TDbConnection> : ISaveProfileServiceTest
        where TDbCommand : DbCommand where TDbConnection : DbConnection
    {
        protected TDbConnection _Connection;

        protected virtual int ExecuteCommandWithId(DbCommand command)
        {
            return Convert.ToInt32(command.ExecuteScalar());
        }
        
        protected virtual string _ProfileInsertString =>
            "INSERT INTO[PROFILES] (PROF_NAME, PROF_GUID, PROF_TS, PROF_VERS) OUTPUT INSERTED.PROF_ID VALUES(@PROF_NAME, @PROF_GUID, @PROF_TS, @VERSION)";

        protected virtual string _ProfileConditionInsertString =>
            "INSERT INTO [PROF_COND](PROF_TESTTYPE_ID, PROF_ID, COND_ID, VALUE) VALUES(@PROF_TESTTYPE_ID, @PROF_ID, @COND_ID, @VALUE)";

        protected virtual string _ProfileParameterInsertString =>
            "INSERT INTO [PROF_PARAM](PROF_TESTTYPE_ID, PROF_ID, PARAM_ID, MIN_VAL, MAX_VAL) VALUES(@PROF_TESTTYPE_ID, @PROF_ID, @PARAM_ID, @MIN_VAL, @MAX_VAL)";

        protected virtual string _ProfileTestTypeInsertString =>
            "INSERT INTO [PROF_TEST_TYPE] (PROF_ID, TEST_TYPE_ID, [ORD]) OUTPUT INSERTED.PTT_ID VALUES (@PROF_ID, @TEST_TYPE_ID, @ORD)";

        protected virtual string _MMECodeToProfileInsertString =>
            "INSERT INTO [MME_CODES_TO_PROFILES] (MME_CODE_ID, PROFILE_ID) VALUES (" +
            "(SELECT MME_CODE_ID FROM MME_CODES WHERE MME_CODE = @MME_CODE), @PROFILE_ID)";

        protected virtual string _MMECodeToProfileDeleteString =>
            "DELETE FROM MME_CODES_TO_PROFILES WHERE PROFILE_ID = @PROFILE_ID";

        protected virtual string _LoadTestTypesString => "SELECT TEST_TYPE_ID, RTRIM(TEST_TYPE_NAME) FROM TEST_TYPE";
        protected virtual string _LoadConditionsString => "SELECT COND_ID, RTRIM(COND_NAME) FROM CONDITIONS";
        protected virtual string _LoadParametersString => "SELECT PARAM_ID, RTRIM(PARAM_NAME) FROM PARAMS";

        private Dictionary<string, int> _TestTypeIdByName = new Dictionary<string, int>();
        private Dictionary<string, int> _ConditionIdByName = new Dictionary<string, int>();
        private Dictionary<string, int> _ParameterIdByName = new Dictionary<string, int>();

        private TDbCommand _ProfileInsert;

        private TDbCommand _ProfileConditionInsert;
        private TDbCommand _ProfileTestTypeInsert;
        private TDbCommand _ProfileParameterInsert;

        private TDbCommand _MMECodeToProfileInsert;
        private TDbCommand _MMECodeToProfileDelete;

        private TDbCommand _LoadTestTypes;
        private TDbCommand _LoadConditions;
        private TDbCommand _LoadParameters;

        private DbTransaction _DbTransaction;

        private readonly ConstructorInfo _CommandConstructor =
            typeof(TDbCommand).GetConstructor(new Type[] {typeof(string), typeof(TDbConnection)});

        public SaveProfilesServiceTest(TDbConnection connection)
        {
            _Connection = connection;
            if (_Connection.State != ConnectionState.Open)
                _Connection.Open();

            PrepareQueries();
            LoadDictionary();
        }

        private TDbCommand CreateCommand(string commandString, List<TDbCommandParametr> parameters)
        {
            var command = _CommandConstructor.Invoke(new object[] {commandString, _Connection}) as TDbCommand;
            foreach (var i in parameters)
            {
                var parametr = command.CreateParameter();
                parametr.DbType = i.DbType;
                parametr.ParameterName = i.Name;
                if (i.Size != null)
                    parametr.Size = i.Size.Value;
                command.Parameters.Add(parametr);
            }

            command.Prepare();
            return command;
        }


        private void PrepareQueries()
        {
            _ProfileInsert = CreateCommand(_ProfileInsertString, new List<TDbCommandParametr>()
            {
                new TDbCommandParametr("@PROF_NAME", DbType.String, 32),
                new TDbCommandParametr("@PROF_GUID", DbType.Guid),
                new TDbCommandParametr("@PROF_TS", DbType.DateTime, 8),
                new TDbCommandParametr("@VERSION", DbType.Int32),
            });

            _ProfileConditionInsert = CreateCommand(_ProfileConditionInsertString, new List<TDbCommandParametr>()
            {
                new TDbCommandParametr("@PROF_TESTTYPE_ID", DbType.Int32),
                new TDbCommandParametr("@PROF_ID", DbType.Int32),
                new TDbCommandParametr("@COND_ID", DbType.Int32),
                new TDbCommandParametr("@VALUE", DbType.String, 16),
            });
            _ProfileTestTypeInsert = CreateCommand(_ProfileTestTypeInsertString, new List<TDbCommandParametr>()
            {
                new TDbCommandParametr("@PROF_ID", DbType.Int32),
                new TDbCommandParametr("@TEST_TYPE_ID", DbType.Int32),
                new TDbCommandParametr("@ORD", DbType.Int32),
            });
            _ProfileParameterInsert = CreateCommand(_ProfileParameterInsertString, new List<TDbCommandParametr>()
            {
                new TDbCommandParametr("@PROF_TESTTYPE_ID", DbType.Int32),
                new TDbCommandParametr("@PROF_ID", DbType.Int32),
                new TDbCommandParametr("@PARAM_ID", DbType.Int32),
                new TDbCommandParametr("@MIN_VAL", DbType.Single),
                new TDbCommandParametr("@MAX_VAL", DbType.Single),
            });

            _MMECodeToProfileInsert = CreateCommand(_MMECodeToProfileInsertString, new List<TDbCommandParametr>()
            {
                new TDbCommandParametr("@MME_CODE", DbType.String, 64),
                new TDbCommandParametr("@PROFILE_ID", DbType.Int32),
            });
            _MMECodeToProfileDelete = CreateCommand(_MMECodeToProfileDeleteString, new List<TDbCommandParametr>()
            {
                new TDbCommandParametr("@PROFILE_ID", DbType.Int32),
            });

            _LoadTestTypes = CreateCommand(_LoadTestTypesString, new List<TDbCommandParametr>());
            _LoadConditions = CreateCommand(_LoadConditionsString, new List<TDbCommandParametr>());
            _LoadParameters = CreateCommand(_LoadParametersString, new List<TDbCommandParametr>());
        }

        private void LoadDictionary()
        {
            using (var reader = _LoadTestTypes.ExecuteReader())
                foreach (DbDataRecord i in reader)
                    _TestTypeIdByName.Add(i.GetString(1), Convert.ToInt32(reader[0]));

            using (var reader = _LoadConditions.ExecuteReader())
                foreach (DbDataRecord i in reader)
                    _ConditionIdByName.Add(i.GetString(1), Convert.ToInt32(reader[0]));

            using (var reader = _LoadParameters.ExecuteReader())
                foreach (DbDataRecord i in reader)
                    _ParameterIdByName.Add(i.GetString(1), Convert.ToInt32(reader[0]));
        }

        private (string typeName, Dictionary<string, object> conditions, Dictionary<string, (object Min, object Max)>
            parameters) ClampingConditionsParameters(ProfileDeepData data)
        {
            var clampingCondtions = new Dictionary<string, object>()
            {
                {"CLAMP_Type", data.ClampingForce},
                {"CLAMP_Force", data.ParameterClamp},
                {"CLAMP_HeightMeasure", data.IsHeightMeasureEnabled},
                {"CLAMP_HeightValue", data.Height},
                {"CLAMP_Temperature", data.Temperature},
            };

            var clampingParameters = new Dictionary<string, (object Min, object Max)>();

            return ("Clamping", clampingCondtions, clampingParameters);
        }

        private (string typeName, Dictionary<string, object> conditions, Dictionary<string, (object Min, object Max)>
            parameters) ComutationConditionsParameters(ProfileDeepData data)
        {
            var clampingCondtions = new Dictionary<string, object>()
            {
                {"COMM_Type", data.ComutationType},
            };

            var clampingParameters = new Dictionary<string, (object Min, object Max)>();

            return ("Commutation", clampingCondtions, clampingParameters);
        }

        private (string typeName, Dictionary<string, object> conditions, Dictionary<string, (object Min, object Max)>
            parameters) TypeConditionsParameters(Types.Gate.TestParameters gate)
        {
            var gateCondtions = new Dictionary<string, object>()
            {
                {"Gate_En", gate.IsEnabled},
                {"Gate_EnableCurrent", gate.IsCurrentEnabled},
                {"Gate_IHEn", gate.IsIhEnabled},
                {"Gate_ILEn", gate.IsIlEnabled},
                {"Gate_EnableIHStrike", gate.IsIhStrikeCurrentEnabled},
            };

            var gateParameters = new Dictionary<string, (object Min, object Max)>
            {
                {"RG", (DBNull.Value, gate.Resistance)},
                {"IGT", (DBNull.Value, gate.IGT)},
                {"VGT", (DBNull.Value, gate.VGT)},
            };

            if (gate.IsIhEnabled)
                gateParameters.Add("IH", (DBNull.Value, gate.IH));
            if (gate.IsIlEnabled)
                gateParameters.Add("IL", (DBNull.Value, gate.IL));

            return ("Gate", gateCondtions, gateParameters);
        }

        private (string typeName, Dictionary<string, object> conditions, Dictionary<string, (object Min, object Max)>
            parameters) TypeConditionsParameters(Types.VTM.TestParameters sl)
        {
            var slCondition = new Dictionary<string, object>()
            {
                {"SL_En", sl.IsEnabled},
                {"SL_Type", sl.TestType},
                {"SL_FS", sl.UseFullScale},
                {"SL_N", sl.Count},
            };

            switch (sl.TestType)
            {
                case Types.VTM.VTMTestType.Ramp:
                    slCondition.Add("SL_ITM", sl.RampCurrent);
                    slCondition.Add("SL_Time", sl.RampTime);
                    slCondition.Add("SL_OpenEn", sl.IsRampOpeningEnabled);
                    slCondition.Add("SL_OpenI", sl.RampOpeningCurrent);
                    slCondition.Add("SL_TimeEx", sl.RampOpeningTime);
                    break;
                case Types.VTM.VTMTestType.Sinus:
                    slCondition.Add("SL_ITM", sl.SinusCurrent);
                    slCondition.Add("SL_Time", sl.SinusTime);
                    break;
                case Types.VTM.VTMTestType.Curve:
                    slCondition.Add("SL_ITM", sl.CurveCurrent);
                    slCondition.Add("SL_Time", sl.CurveTime);
                    slCondition.Add("SL_Factor", sl.CurveFactor);
                    slCondition.Add("SL_TimeEx", sl.CurveAddTime);
                    break;
            }

            var slParameters = new Dictionary<string, (object Min, object Max)>
            {
                {"VTM", (DBNull.Value, sl.VTM)}
            };

            return ("BVT", slCondition, slParameters);
        }

        private (string typeName, Dictionary<string, object> conditions, Dictionary<string, (object Min, object Max)>
            parameters) TypeConditionsParameters(Types.BVT.TestParameters bvt)
        {
            var bvtCondition = new Dictionary<string, object>()
            {
                {"BVT_En", bvt.IsEnabled},
                {"BVT_Type", bvt.TestType},
                {"BVT_I", bvt.CurrentLimit},
                {"BVT_RumpUp", bvt.RampUpVoltage},
                {"BVT_StartV", bvt.StartVoltage},
                {"BVT_F", bvt.VoltageFrequency},
                {"BVT_FD", bvt.FrequencyDivisor},
                {"BVT_Mode", bvt.MeasurementMode},
                {"BVT_PlateTime", bvt.PlateTime},
            };
            switch (bvt.TestType)
            {
                case Types.BVT.BVTTestType.Both:
                    bvtCondition.Add("BVT_VD", bvt.VoltageLimitD);
                    bvtCondition.Add("BVT_VR", bvt.VoltageLimitR);
                    break;
                case Types.BVT.BVTTestType.Direct:
                    bvtCondition.Add("BVT_VD", bvt.VoltageLimitD);
                    break;
                case Types.BVT.BVTTestType.Reverse:
                    bvtCondition.Add("BVT_VR", bvt.VoltageLimitR);
                    break;
            }

            var bvtParameters = new Dictionary<string, (object Min, object Max)>();
            if (bvt.MeasurementMode == Types.BVT.BVTMeasurementMode.ModeV)
            {
                bvtParameters.Add("VRRM", (bvt.VRRM, DBNull.Value));

                if (bvt.TestType != Types.BVT.BVTTestType.Reverse)
                    bvtParameters.Add("VDRM", (bvt.VDRM, DBNull.Value));
            }
            else
            {
                bvtParameters.Add("IRRM", (DBNull.Value, bvt.IRRM));

                if (bvt.TestType != Types.BVT.BVTTestType.Reverse)
                    bvtParameters.Add("IDRM", (DBNull.Value, bvt.IDRM));
            }

            return ("SL", bvtCondition, bvtParameters);
        }

        private (string typeName, Dictionary<string, object> conditions, Dictionary<string, (object Min, object Max)>
            parameters) TypeConditionsParameters(Types.dVdt.TestParameters dvdt)
        {
            var dvdtCondition = new Dictionary<string, object>()
            {
                {"DVDT_En", dvdt.IsEnabled},
                {"DVDT_Mode", dvdt.Mode},
            };
            switch (dvdt.Mode)
            {
                case Types.dVdt.DvdtMode.Confirmation:
                    dvdtCondition.Add("DVDT_Voltage", dvdt.Voltage);
                    dvdtCondition.Add("DVDT_VoltageRate", dvdt.VoltageRate);
                    dvdtCondition.Add("DVDT_ConfirmationCount", dvdt.ConfirmationCount);
                    break;
                case Types.dVdt.DvdtMode.Detection:
                    dvdtCondition.Add("DVDT_Voltage", dvdt.Voltage);
                    dvdtCondition.Add("DVDT_VoltageRateLimit", dvdt.VoltageRateLimit);
                    dvdtCondition.Add("DVDT_VoltageRateOffSet", dvdt.VoltageRateOffSet);
                    break;
            }

            var dvdtParameters = new Dictionary<string, (object Min, object Max)>();


            return ("Dvdt", dvdtCondition, dvdtParameters);
        }

        private (string typeName, Dictionary<string, object> conditions, Dictionary<string, (object Min, object Max)> 
            parameters) TypeConditionsParameters(Types.ATU.TestParameters atu)
        {
            var atuCondition = new Dictionary<string, object>()
            {
                {"ATU_En", atu.IsEnabled},
                {"ATU_PrePulseValue", atu.PrePulseValue},
                {"ATU_PowerValue", atu.PowerValue},
            };

            var atuParameters = new Dictionary<string, (object Min, object Max)>()
            {
                {"UBR", (atu.UBR, DBNull.Value)},
                {"UPRSM", (atu.UPRSM, DBNull.Value)},
                {"IPRSM", (atu.IPRSM, DBNull.Value)},
                {"PRSM", (atu.PRSM, DBNull.Value)},
            };

            return ("ATU", atuCondition, atuParameters);
        }

        private (string typeName, Dictionary<string, object> conditions, Dictionary<string, (object Min, object Max)>
            parameters) TypeConditionsParameters(Types.QrrTq.TestParameters qrrTq)
        {
            var qrrTqCondition = new Dictionary<string, object>()
            {
                {"QrrTq_En", qrrTq.IsEnabled},
                {"QrrTq_Mode", qrrTq.Mode},
                {"QrrTq_TrrMeasureBy9050Method", qrrTq.TrrMeasureBy9050Method},
                {"QrrTq_DirectCurrent", qrrTq.DirectCurrent},
                {"QrrTq_DCPulseWidth", qrrTq.DCPulseWidth},
                {"QrrTq_DCRiseRate", qrrTq.DCRiseRate},
                {"QrrTq_DCFallRate", (uint) qrrTq.DCFallRate},
                {"QrrTq_OffStateVoltage", qrrTq.OffStateVoltage},
                {"QrrTq_OsvRate", (uint) qrrTq.OsvRate},
            };

            var qrrTqParameters = new Dictionary<string, (object Min, object Max)>()
            {
                {"IDC", (qrrTq.Idc, DBNull.Value)},
                {"QRR", (qrrTq.Qrr, DBNull.Value)},
                {"IRR", (qrrTq.Irr, DBNull.Value)},
                {"TRR", (qrrTq.Trr, DBNull.Value)},
                {"DCFactFallRate", (qrrTq.DCFactFallRate, DBNull.Value)},
            };
            if (qrrTq.Mode == Types.QrrTq.TMode.QrrTq)
                qrrTqParameters.Add("TQ", (qrrTq.Tq, DBNull.Value));

            return ("QrrTq", qrrTqCondition, qrrTqParameters);
        }

        private (string typeName, Dictionary<string, object> conditions, Dictionary<string, (object Min, object Max)>
            parameters) TypeConditionsParameters(Types.RAC.TestParameters rac)
        {
            var racCondition = new Dictionary<string, object>()
            {
                {"RAC_En", rac.IsEnabled},
                {"RAC_ResVoltage", rac.ResVoltage},
            };

            var racParameters = new Dictionary<string, (object Min, object Max)>()
            {
                {"ResultR", (rac.ResultR, DBNull.Value)}
            };

            return ("RAC", racCondition, racParameters);
        }

        private (string typeName, Dictionary<string, object> conditions, Dictionary<string, (object Min, object Max)>
            parameters) TypeConditionsParameters(Types.TOU.TestParameters tou)
        {
            var touCondition = new Dictionary<string, object>()
            {
                {"TOU_En", tou.IsEnabled},
                {"TOU_ITM", tou.CurrentAmplitude},
            };

            var touParameters = new Dictionary<string, (object Min, object Max)>()
            {
                {"TOU_TGD", (tou.TGD, DBNull.Value)},
                {"TOU_TGT", (tou.TGT, DBNull.Value)}
            };

            return ("TOU", touCondition, touParameters);
        }

        private void DeleteProfile(MyProfile profile)
        {
            _MMECodeToProfileDelete.Parameters["@PROFILE_ID"].Value = profile.Id;
            _MMECodeToProfileDelete.Transaction = _DbTransaction;
            _MMECodeToProfileDelete.ExecuteScalar();
        }

        private int InsertProfile(MyProfile profile, string MMECode)
        {
            _ProfileInsert.Parameters["@PROF_NAME"].Value = profile.Name;
            _ProfileInsert.Parameters["@PROF_GUID"].Value = profile.Key;
            _ProfileInsert.Parameters["@VERSION"].Value = profile.Version;
            _ProfileInsert.Parameters["@PROF_TS"].Value = profile.Timestamp;

            _ProfileInsert.Transaction = _DbTransaction;
            int id = ExecuteCommandWithId(_ProfileInsert);

            _MMECodeToProfileInsert.Parameters["@PROFILE_ID"].Value = id;
            _MMECodeToProfileInsert.Parameters["@MME_CODE"].Value = MMECode;

            _MMECodeToProfileInsert.Transaction = _DbTransaction;
            _MMECodeToProfileInsert.ExecuteScalar();

            return id;
        }

        private int SaveProfile(MyProfile profile, string MMECode)
        {
            profile.Id = InsertProfile(profile, MMECode);

            InserterBaseTestParametersAndNormatives inserter = new InserterBaseTestParametersAndNormatives(profile.Id,
                _DbTransaction,
                _ProfileTestTypeInsert, _ProfileParameterInsert, _ProfileConditionInsert,
                _TestTypeIdByName, _ConditionIdByName, _ParameterIdByName);

            inserter.Order = 0;
            inserter.Insert(ClampingConditionsParameters(profile.ProfileDeepData));
            inserter.Insert(ComutationConditionsParameters(profile.ProfileDeepData));
            foreach (var i in profile.ProfileDeepData.TestParametersAndNormatives)
            {
                switch (i)
                {
                    case Types.Gate.TestParameters gate:
                        inserter.Insert(TypeConditionsParameters(gate));
                        break;
                    case Types.VTM.TestParameters sl:
                        inserter.Insert(TypeConditionsParameters(sl));
                        break;
                    case Types.BVT.TestParameters bvt:
                        inserter.Insert(TypeConditionsParameters(bvt));
                        break;
                    case Types.dVdt.TestParameters dvdt:
                        inserter.Insert(TypeConditionsParameters(dvdt));
                        break;
                    case Types.ATU.TestParameters atu:
                        inserter.Insert(TypeConditionsParameters(atu));
                        break;
                    case Types.QrrTq.TestParameters qrrTq:
                        inserter.Insert(TypeConditionsParameters(qrrTq));
                        break;
                    case Types.RAC.TestParameters rac:
                        inserter.Insert(TypeConditionsParameters(rac));
                        break;
                    case Types.TOU.TestParameters tou:
                        inserter.Insert(TypeConditionsParameters(tou));
                        break;
                    default:
                        throw new NotImplementedException("SaveProfile switch");
                }
            }

            return profile.Id;
        }

        public void RemoveProfile(MyProfile profile)
        {
            try
            {
                _DbTransaction = _Connection.BeginTransaction();
                DeleteProfile(profile);
                _DbTransaction.Commit();
            }
            catch (Exception)
            {
                _DbTransaction.Rollback();
                throw;
            }
        }

        public int InsertUpdateProfile(MyProfile oldProfile, MyProfile newProfile, string mmeCode)
        {
            try
            {
                _DbTransaction = _Connection.BeginTransaction();
                DeleteProfile(oldProfile);
                var id = SaveProfile(newProfile, mmeCode);
                _DbTransaction.Commit();
                return id;
            }

            catch
            {
                _DbTransaction.Rollback();
                throw;
            }
        }
    }
}