using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
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
                {"IGT", (DBNull.Value, gate.IGT)},
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

            var dvdtParameters = new Dictionary<string, (object Min, object Max)>();


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
                {"PRSM", (atu.PRSM, DBNull.Value)},
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

        private (string typeName, Dictionary<string, object> conditions, Dictionary<string, (object Min, object Max)> parameters) TypeConditionsParameters(Types.RAC.TestParameters rac)
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
                var profiles =_cacheProfilesByMmeCode[string.Empty];
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
            if (profile == null) 
                return;
            
            if(profile.MmeCodes == null)
                profile.MmeCodes = new List<string>();
            else
                profile.MmeCodes.Remove(mmeCode);

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
            if (profile == null) 
                return;
            
            if(profile.MmeCodes == null)
                profile.MmeCodes = new List<string>();
            profile.MmeCodes.Add(mmeCode);
            
            _cacheProfilesByMmeCode.TryGetValue(mmeCode, out var profiles);
            profiles?.Add(profile.Profile);
        }

        public void InsertMmeCode(string mmeCode)
        {
            _insertMmeCode.Parameters["@MME_CODE"].Value = mmeCode;
            _insertMmeCode.ExecuteNonQuery();
        }
        
        private int InsertProfile(MyProfile profile, string mmeCode)
        {
            _profileInsert.Parameters["@PROF_NAME"].Value = profile.Name;
            _profileInsert.Parameters["@PROF_GUID"].Value = profile.Key;
            _profileInsert.Parameters["@VERSION"].Value = profile.Version;
            _profileInsert.Parameters["@PROF_TS"].Value = profile.Timestamp;

            _profileInsert.Transaction = _dbTransaction;
            var id = ExecuteCommandWithId(_profileInsert);

            InsertMmeCodeToProfile(id, mmeCode, _dbTransaction);

            return id;
        }

        private int SaveProfile(MyProfile profile, string mmeCode)
        {
            profile.Id = InsertProfile(profile, mmeCode);

            var inserter = new InserterBaseTestParametersAndNormatives(profile.Id,
                _dbTransaction,
                _profileTestTypeInsert, _profileParameterInsert, _profileConditionInsert,
                _testTypeIdByName, _conditionIdByName, _parameterIdByName, ExecuteCommandWithId) {Order = 0};

            inserter.Insert(ClampingConditionsParameters(profile.DeepData));
            inserter.Insert(ComutationConditionsParameters(profile.DeepData));
            foreach (var i in profile.DeepData.TestParametersAndNormatives)
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

        public void RemoveProfile(MyProfile profile, string mmeCode)
        {
            try
            {
                _dbTransaction = Connection.BeginTransaction();
                RemoveMmeCodeToProfile(profile.Id, mmeCode, _dbTransaction);
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
                if (oldProfile != null)
                {
                    RemoveMmeCodeToProfile(oldProfile.Id, mmeCode, _dbTransaction);
                    RemoveMmeCodeToProfile(oldProfile.Id, Constants.MME_CODE_IS_ACTIVE_NAME, _dbTransaction);
                }

                var id = SaveProfile(newProfile, mmeCode);
                InsertMmeCodeToProfile(id, Constants.MME_CODE_IS_ACTIVE_NAME,_dbTransaction);
                _dbTransaction.Commit();

                //If new version without rename then move children of old profile and old profile to children of new profile
                if (oldProfile != null)
                {
                    if (newProfile.Name.Equals(oldProfile.Name))
                    {
                        newProfile.Children.Add(oldProfile);
                        foreach (var i in oldProfile.Children)
                            newProfile.Children.Add(i);
                    }

                    oldProfile.Children.Clear();
                }

                _cacheProfileById[newProfile.Id] = new ProfileCache(newProfile){IsChildLoad = true, IsDeepLoad = true};
                _cacheProfilesByMmeCode[mmeCode].Add(newProfile);
                
                if (oldProfile == null)
                    _cacheProfileById[newProfile.Id].MmeCodes = new List<string>(){mmeCode};
                
                return id;
            }

            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                _dbTransaction.Rollback();
                throw;
            }
        }
    }
}