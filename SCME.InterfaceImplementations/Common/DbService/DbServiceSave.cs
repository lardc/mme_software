using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using SCME.Types;
using SCME.Types.Profiles;

namespace SCME.InterfaceImplementations.Common.DbService
{
    public abstract partial class DbService<TDbCommand, TDbConnection> where TDbCommand : DbCommand where TDbConnection : DbConnection
    {
        protected virtual int ExecuteCommandWithId(DbCommand command)
        {
            return Convert.ToInt32(command.ExecuteScalar());
        }

        private void LoadDictionary()
        {
            _testTypeIdByName.Clear();
            _conditionIdByName.Clear();
            _parameterIdByName.Clear();
            using (var reader = _loadTestTypes.ExecuteReader())
                foreach (DbDataRecord i in reader)
                    _testTypeIdByName.Add(i.GetString(1), Convert.ToInt32((object) reader[0]));

            using (var reader = _loadConditions.ExecuteReader())
                foreach (DbDataRecord i in reader)
                    _conditionIdByName.Add(i.GetString(1), Convert.ToInt32((object) reader[0]));

            using (var reader = _loadParameters.ExecuteReader())
                foreach (DbDataRecord i in reader)
                    _parameterIdByName.Add(i.GetString(1), Convert.ToInt32((object) reader[0]));
        }

        private (string typeName, Dictionary<string, object> conditions, Dictionary<string, (object Min, object Max)> parameters) ClampingConditionsParameters(ProfileDeepData data)
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

        private (string typeName, Dictionary<string, object> conditions, Dictionary<string, (object Min, object Max)> parameters) ComutationConditionsParameters(ProfileDeepData data)
        {
            var clampingCondtions = new Dictionary<string, object>()
            {
                {"COMM_Type", data.CommutationType},
            };

            var clampingParameters = new Dictionary<string, (object Min, object Max)>();

            return ("Commutation", clampingCondtions, clampingParameters);
        }

        private (string typeName, Dictionary<string, object> conditions, Dictionary<string, (object Min, object Max)> parameters) TypeConditionsParameters(Types.Gate.TestParameters gate)
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
                {"IGT", (gate.MinIGT, gate.IGT)},
                {"VGT", (DBNull.Value, gate.VGT)},
            };

            if (gate.IsIhEnabled)
                gateParameters.Add("IH", (DBNull.Value, gate.IH));
            if (gate.IsIlEnabled)
                gateParameters.Add("IL", (DBNull.Value, gate.IL));

            return ("Gate", gateCondtions, gateParameters);
        }

        private (string typeName, Dictionary<string, object> conditions, Dictionary<string, (object Min, object Max)> parameters) TypeConditionsParameters(Types.VTM.TestParameters sl)
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

            return ("SL", slCondition, slParameters);
        }

        private (string typeName, Dictionary<string, object> conditions, Dictionary<string, (object Min, object Max)> parameters) TypeConditionsParameters(Types.BVT.TestParameters bvt)
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
                {"BVT_UseUdsmUrsm", bvt.UseUdsmUrsm},
                {"BVT_PulseFrequency", bvt.PulseFrequency},
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

            if (bvt.UseUdsmUrsm)
            {
                foreach (var i in new Dictionary<string, object>()
                {
                    {"BVT_UdsmUrsm_Type", bvt.UdsmUrsmTestType},
                    {"BVT_UdsmUrsm_I", bvt.UdsmUrsmCurrentLimit},
                    {"BVT_UdsmUrsm_RumpUp", bvt.UdsmUrsmRampUpVoltage},
                    {"BVT_UdsmUrsm_StartV", bvt.UdsmUrsmStartVoltage},
                    {"BVT_UdsmUrsm_F", bvt.UdsmUrsmVoltageFrequency},
                    {"BVT_UdsmUrsm_FD", bvt.UdsmUrsmFrequencyDivisor},
                    {"BVT_UdsmUrsm_PlateTime", bvt.UdsmUrsmPlateTime},
                    {"BVT_UdsmUrsm_PulseFrequency", bvt.UdsmUrsmPulseFrequency},
                })
                    bvtCondition.Add(i.Key, i.Value);

                switch (bvt.UdsmUrsmTestType)
                {
                    case Types.BVT.BVTTestType.Both:
                        bvtCondition.Add("VDSM", bvt.UdsmUrsmVoltageLimitD);
                        bvtCondition.Add("VRSM", bvt.UdsmUrsmVoltageLimitR);
                        break;
                    case Types.BVT.BVTTestType.Direct:
                        bvtCondition.Add("VDSM", bvt.UdsmUrsmVoltageLimitD);
                        break;
                    case Types.BVT.BVTTestType.Reverse:
                        bvtCondition.Add("VRSM", bvt.UdsmUrsmVoltageLimitR);
                        break;
                }

                bvtParameters.Add("IRSM", (DBNull.Value, bvt.IRSM));

                if (bvt.UdsmUrsmTestType != Types.BVT.BVTTestType.Reverse)
                    bvtParameters.Add("IDSM", (DBNull.Value, bvt.IDSM));
            }

            return ("BVT", bvtCondition, bvtParameters);
        }

        private (string typeName, Dictionary<string, object> conditions, Dictionary<string, (object Min, object Max)> parameters) TypeConditionsParameters(Types.dVdt.TestParameters dvdt)
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

            var dvdtParameters = new Dictionary<string, (object Min, object Max)>
            {
                { "DVDT_OK", (DBNull.Value,true)}
            };


            return ("Dvdt", dvdtCondition, dvdtParameters);
        }

        private (string typeName, Dictionary<string, object> conditions, Dictionary<string, (object Min, object Max)> parameters) TypeConditionsParameters(Types.ATU.TestParameters atu)
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
                {"PRSM", (atu.PRSM_Min, atu.PRSM_Max)},
            };

            return ("ATU", atuCondition, atuParameters);
        }

        private (string typeName, Dictionary<string, object> conditions, Dictionary<string, (object Min, object Max)> parameters) TypeConditionsParameters(Types.QrrTq.TestParameters qrrTq)
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

        private (string typeName, Dictionary<string, object> conditions, Dictionary<string, (object Min, object Max)> parameters) TypeConditionsParameters(Types.TOU.TestParameters tou)
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

        public void RemoveMmeCode(string mmeCode)
        {
            _dbTransaction = Connection.BeginTransaction();
            try
            {
                _deleteAllMmeCodeToProfileByMmeCode.Parameters["@MME_CODE"].Value = mmeCode;
                _deleteMmeCode.Parameters["@MME_CODE"].Value = mmeCode;

                _deleteAllMmeCodeToProfileByMmeCode.Transaction = _dbTransaction;
                _deleteMmeCode.Transaction = _dbTransaction;

                _deleteAllMmeCodeToProfileByMmeCode.ExecuteNonQuery();
                _deleteMmeCode.ExecuteNonQuery();

                _dbTransaction.Commit();


                //Only mmeCode = empty string 
                var profiles = _cacheProfilesByMmeCode[string.Empty];
                foreach (var i in profiles)
                {
                    _cacheProfileById.TryGetValue(i.Id, out var profile);
                    profile?.MmeCodes?.Remove(mmeCode);
                }
            }
            catch (Exception e)
            {
                _dbTransaction.Rollback();
                Debug.WriteLine(e);
                throw;
            }
        }

        public void RemoveMmeCodeToProfile(int profileId, string mmeCode, DbTransaction dbTransaction = null)
        {
            if (mmeCode != string.Empty)
            {
                _mmeCodeToProfileDelete.Parameters["@PROFILE_ID"].Value = profileId;
                _mmeCodeToProfileDelete.Parameters["@MME_CODE"].Value = mmeCode;
                _mmeCodeToProfileDelete.Transaction = dbTransaction;
                _mmeCodeToProfileDelete.ExecuteNonQuery();
            }

            _cacheProfileById.TryGetValue(profileId, out var profile);
            profile?.MmeCodes?.Remove(mmeCode);

            _cacheProfilesByMmeCode.TryGetValue(mmeCode, out var profiles);
            profiles?.Remove(_cacheProfileById[profileId].Profile);
        }

        public void InsertMmeCodeToProfile(int profileId, string mmeCode, DbTransaction dbTransaction = null)
        {
            if (mmeCode != string.Empty)
            {
                _mmeCodeToProfileInsert.Parameters["@PROFILE_ID"].Value = profileId;
                _mmeCodeToProfileInsert.Parameters["@MME_CODE"].Value = mmeCode;
                _mmeCodeToProfileInsert.Transaction = dbTransaction;
                _mmeCodeToProfileInsert.ExecuteNonQuery();
            }

            _cacheProfileById.TryGetValue(profileId, out var profile);
            profile?.MmeCodes?.Add(mmeCode);

            // ReSharper disable once InvertIf
            if (profile != null)
            {
                _cacheProfilesByMmeCode.TryGetValue(mmeCode, out var profiles);
                profiles?.Add(profile.Profile);
            }
        }

        public void UpdateMmeCodesToProfile(int oldProfileId, int newProfileId)
        {
            _updateMmeCodesToProfile.Parameters["@NEW_PROFILE_ID"].Value = newProfileId;
            _updateMmeCodesToProfile.Parameters["@OLD_PROFILE_ID"].Value = oldProfileId;
            _updateMmeCodesToProfile.Transaction = _dbTransaction;
            _updateMmeCodesToProfile.ExecuteNonQuery();
        }

        public void InsertMmeCode(string mmeCode)
        {
            _insertMmeCode.Parameters["@MME_CODE"].Value = mmeCode; 
            _insertMmeCode.ExecuteNonQuery();
        }

        private int InsertProfile(MyProfile profile)
        {
            _profileInsert.Parameters["@PROF_NAME"].Value = profile.Name;
            _profileInsert.Parameters["@PROF_GUID"].Value = profile.Key;
            _profileInsert.Parameters["@VERSION"].Value = profile.Version;
            _profileInsert.Parameters["@PROF_TS"].Value = profile.Timestamp;

            _profileInsert.Transaction = _dbTransaction;
            var id = ExecuteCommandWithId(_profileInsert);

            return id;
        }
        
        private int SaveProfile(MyProfile profile)
        {
            profile.Id = InsertProfile(profile);

            _inserter = new InserterBaseTestParametersAndNormatives(profile.Id,
                _dbTransaction,
                _profileTestTypeInsert, _profileParameterInsert, _profileConditionInsert,
                _testTypeIdByName, _conditionIdByName, _parameterIdByName, ExecuteCommandWithId) {Order = 0};

            _inserter.Insert(ClampingConditionsParameters(profile.DeepData));
            _inserter.Insert(ComutationConditionsParameters(profile.DeepData));
            foreach (var i in profile.DeepData.TestParametersAndNormatives)
            {
                switch (i)
                {
                    case Types.Gate.TestParameters gate:
                        _inserter.Insert(TypeConditionsParameters(gate));
                        break;
                    case Types.VTM.TestParameters sl:
                        _inserter.Insert(TypeConditionsParameters(sl));
                        break;
                    case Types.BVT.TestParameters bvt:
                        _inserter.Insert(TypeConditionsParameters(bvt));
                        break;
                    case Types.dVdt.TestParameters dvdt:
                        _inserter.Insert(TypeConditionsParameters(dvdt));
                        break;
                    case Types.ATU.TestParameters atu:
                        _inserter.Insert(TypeConditionsParameters(atu));
                        break;
                    case Types.QrrTq.TestParameters qrrTq:
                        _inserter.Insert(TypeConditionsParameters(qrrTq));
                        break;
                    case Types.TOU.TestParameters tou:
                        _inserter.Insert(TypeConditionsParameters(tou));
                        break;
                    default:
                        throw new NotImplementedException("SaveProfile switch");
                }
            }

            return profile.Id;
        }

        public void RemoveProfile(MyProfile profile, string mmeCode)
        {
            try
            {
                _dbTransaction = Connection.BeginTransaction();
                RemoveMmeCodeToProfile(profile.Id, mmeCode, _dbTransaction);
                _cacheProfileById.Remove(profile.Id);
                _dbTransaction.Commit();

//                _cacheProfileByKey.Remove(profile.Key);
//                _cacheProfilesByMmeCode[mmeCode].Remove(profile);
            }
            catch (Exception)
            {
                _dbTransaction.Rollback();
                throw;
            }
        }

        public int InsertUpdateProfile(MyProfile oldProfile, MyProfile newProfile, string mmeCode)
        {
            try
            {
                _dbTransaction = Connection.BeginTransaction();
                var newProfileId = SaveProfile(newProfile);
                List<string> mmeCodes = oldProfile != null ? GetMmeCodesByProfile(oldProfile.Id, _dbTransaction) : new List<string>(){ mmeCode, Constants.MME_CODE_IS_ACTIVE_NAME};

                if(oldProfile == null)
                    foreach (var i in mmeCodes)
                        InsertMmeCodeToProfile(newProfileId, i, _dbTransaction);
                else
                    UpdateMmeCodesToProfile(oldProfile.Id, newProfileId);

                _dbTransaction.Commit();

                if(oldProfile != null)
                {
                    foreach(var i in _cacheProfilesByMmeCode)
                            i.Value.RemoveAll(m=> m.Key == oldProfile.Key);

                    if(_cacheProfileById.ContainsKey(oldProfile.Id))
                        _cacheProfileById.Remove(oldProfile.Id);
                }

                 foreach(var i in mmeCodes)
                    if(_cacheProfilesByMmeCode.ContainsKey(i))
                        _cacheProfilesByMmeCode[i].Add(newProfile);

                 _cacheProfileById[newProfile.Id] = new ProfileCache(newProfile) {IsChildLoad = true, IsDeepLoad = true, MmeCodes = mmeCodes};


                return newProfileId;
            }

            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                _dbTransaction.Rollback();
                throw;
            }
        }
    }
}