using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using SCME.Types;
using SCME.Types.BaseTestParams;
using SCME.Types.BVT;
using SCME.Types.Commutation;
using SCME.Types.dVdt;
using SCME.Types.Profiles;
using SCME.Types.VTM;

namespace SCME.InterfaceImplementations.Common.DbService
{
    public abstract partial class DbService<TDbCommand, TDbConnection> where TDbCommand : DbCommand where TDbConnection : DbConnection
    {
        public (MyProfile profile, bool IsInMmeCode) GetTopProfileByName(string mmeCode, string name)
        {
            try
            {
                _profileByNameByMmeMaxTimestamp.Parameters["@MME_CODE"].Value = mmeCode;
                _profileByNameByMmeMaxTimestamp.Parameters["@PROF_NAME"].Value = name;
                using var reader = _profileByNameByMmeMaxTimestamp.ExecuteReader();
                var isRead = reader.Read();
                return !isRead ? (null, false) : (new MyProfile(reader.GetInt32(0), reader.GetString(1), reader.GetGuid(2), reader.GetInt32(3), reader.GetDateTime(4)), true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public Dictionary<string, int> GetMmeCodes()
        {
            var mmeCodes = new Dictionary<string, int>();

            using (var reader = _allMmeCodesSelect.ExecuteReader())
                while (reader.Read())
                    mmeCodes.Add(reader.GetString(0), reader.GetInt32(1));

            return mmeCodes;
        }


        public List<MyProfile> GetProfilesDeepByMmeCode(string mmeCode)
        {
            var res = GetProfilesSuperficially(mmeCode).Select(m =>
            {
                m.DeepData = LoadProfileDeepData(m);
                return m;
            }).ToList();

            _cacheProfilesByMmeCode[mmeCode] = res;
//            foreach (var i in res.Where(i => _cacheProfileById.ContainsKey(i.Id)))
//                _cacheProfileById[i.Id] = new ProfileCache(i) {IsChildLoad = true};

            return res;
        }


        public List<MyProfile> GetProfilesSuperficially(string mmeCode, string name = null)
        {
            try
            {
                var profiles = new List<MyProfile>();
                DbCommand profileSelect;

                if (mmeCode == null)
                    throw new ArgumentNullException(nameof(mmeCode));
                if (string.IsNullOrEmpty(mmeCode))
                    profileSelect = _selectAllTopProfile;
                else if (string.IsNullOrEmpty(name))
                {
                    profileSelect = _profilesByMmeSelect;
                    profileSelect.Parameters["@MME_CODE"].Value = mmeCode;
                }
                else
                {
                    throw new NotImplementedException("_ProfilesByNameByMMESelect");
                    //profileSelect = _ProfilesByNameByMMESelect;
                    //profileSelect.Parameters["@PROF_NAME"].Value = name;
                    //profileSelect.Parameters["@MME_CODE"].Value = mmeCode;
                }

                if (_enableCache)
                {
                    _cacheProfilesByMmeCode.TryGetValue(mmeCode, out profiles);
                    if (profiles != null)
                        return profiles.Select(m => m.Copy()).ToList();

                    _cacheProfilesByMmeCode[mmeCode] = profiles = new List<MyProfile>();
                }

                using var reader = profileSelect.ExecuteReader();
                while (reader.Read())
                {
                    var readProfile = new MyProfile(reader.GetInt32(0), reader.GetString(1), reader.GetGuid(2), reader.GetInt32(3), reader.GetDateTime(4));
                    profiles.Add(readProfile);
                    if (_enableCache)
                        _cacheProfileById[readProfile.Id] = new ProfileCache(readProfile);
                }

                return profiles.Select(m => m.Copy()).ToList();
            }
            catch (Exception ex)
            {
                throw new FaultException(ex.ToString());
            }
        }

        //        public MyProfile GetProfileByKey(Guid key)
        //        {
        //            _profileByKeySelect.Parameters["@PROF_GUID"].Value = key;
        //            using (var reader = _profileByKeySelect.ExecuteReader())
        //                while (reader.Read())
        //                    return new MyProfile(reader.GetInt32(0), reader.GetString(1), reader.GetGuid(2), reader.GetInt32(3), reader.GetDateTime(4));
        //            throw new Exception($"GetProfileByKey, could not find item by key {key}");
        //        }

        public List<MyProfile> GetProfileChildSuperficially(MyProfile profile)
        {
            _childSelect.Parameters["@PROF_NAME"].Value = profile.Name;
            _childSelect.Parameters["@PROF_ID_EXCLUDE"].Value = profile.Id;

            var cacheProfile = _cacheProfileById[profile.Id];
            if (cacheProfile.IsChildLoad == false)
            {
                using (var reader = _childSelect.ExecuteReader())
                    while (reader.Read())
                    {
                        var readProfile = new MyProfile(reader.GetInt32(0), reader.GetString(1), reader.GetGuid(2), reader.GetInt32(3), reader.GetDateTime(4));
                        _cacheProfileById[readProfile.Id] = new ProfileCache(readProfile);
                        cacheProfile.Profile.Children.Add(readProfile);
                    }

                cacheProfile.IsChildLoad = true;
                _cacheProfileById[profile.Id] = cacheProfile;
            }

            return cacheProfile.Profile.Children.Select((m => m.Copy())).ToList();
        }

        public void InvalidCacheById(int id, string mmeCode)
        {
            _cacheProfileById.Remove(id);
            _cacheProfilesByMmeCode.Remove(mmeCode);
        }
        
        public ProfileDeepData LoadProfileDeepData(MyProfile profile)
        {
            _cacheProfileById.TryGetValue(profile.Id, out var cacheProfile);

            if (cacheProfile == null)
                cacheProfile = new ProfileCache(profile);

            if (cacheProfile.IsDeepLoad == false || !_enableCache)
            {
                var testTypes = new Dictionary<long, long>();

                _testTypeSelect.Parameters["@PROF_ID"].Value = profile.Id;

                using (var reader = _testTypeSelect.ExecuteReader())
                    while (reader.Read())
                        testTypes.Add(reader.GetInt32(0), reader.GetInt32(1));

                foreach (var testType in testTypes)
                    FillParameters(cacheProfile.Profile.DeepData, testType.Key, testType.Value);

                if (_enableCache)
                {
                    cacheProfile.IsDeepLoad = true;
                    _cacheProfileById[profile.Id] = cacheProfile;
                }
            }

            return cacheProfile.Profile.DeepData.Copy();
        }

        public bool ProfileNameExists(string profileName)
        {
            _profileNameExists.Parameters["@PROF_NAME"].Value = profileName;
            return Convert.ToInt32(_profileNameExists.ExecuteScalar()) > 0;
        }

        public string GetFreeProfileName()
        {
            var id = Convert.ToInt32(_getFreeProfileName.ExecuteScalar());
            string newProfileName;
            do
            {
                id++;
                newProfileName = $"New Profile{id}";
            } while (ProfileNameExists(newProfileName));

            return newProfileName;
        }

        public List<string> GetMmeCodesByProfile(int profileId, DbTransaction dbTransaction = null)
        {
            _mmeCodesByProfile.Parameters["@PROFILE_ID"].Value = profileId;
            _mmeCodesByProfile.Transaction = dbTransaction;
            using var reader = _mmeCodesByProfile.ExecuteReader();

            List<string> mmeCodes = new List<string>();

            while (reader.Read())
                mmeCodes.Add(reader.GetString(0));

            return mmeCodes;
        }

        #region Fill

        private void FillParameters(ProfileDeepData data, long testTypeId, long testParametersType)
        {
            switch ((TestParametersType) testParametersType)
            {
                case TestParametersType.Gate:
                    var gatePars = FillGateConditions(testTypeId);
                    FillGateParameters(gatePars, testTypeId);
                    data.TestParametersAndNormatives.Add(gatePars);
                    break;
                case TestParametersType.Bvt:
                    var bvtPars = FillBvtConditions(testTypeId);
                    FillBvtParameters(bvtPars, testTypeId);
                    data.TestParametersAndNormatives.Add(bvtPars);
                    break;
                case TestParametersType.StaticLoses:
                    var slParams = FillSlConditions(testTypeId);
                    FillSlParameters(slParams, testTypeId);
                    data.TestParametersAndNormatives.Add(slParams);
                    break;
                case TestParametersType.Dvdt:
                    var dVdtParams = FillDvdtConditions(testTypeId);
                    data.TestParametersAndNormatives.Add(dVdtParams);
                    break;
                case TestParametersType.ATU:
                    var atuParams = FillAtuConditions(testTypeId);
                    FillAtuParameters(atuParams, testTypeId);
                    data.TestParametersAndNormatives.Add(atuParams);
                    break;
                case TestParametersType.QrrTq:
                    var qrrTqParams = FillQrrTqConditions(testTypeId);
                    data.TestParametersAndNormatives.Add(qrrTqParams);
                    break;
                case TestParametersType.TOU:
                    var touParams = FillTOUConditions(testTypeId);
                    data.TestParametersAndNormatives.Add(touParams);
                    break;
                case TestParametersType.Clamping:
                    FillClampConditions(data, testTypeId);
                    break;
                case TestParametersType.Commutation:
                    FillComutationConditions(data, testTypeId);
                    break;
            }
        }

        private Types.dVdt.TestParameters FillDvdtConditions(long testTypeId)
        {
            var results = new Dictionary<string, object>(3);
            var testParams = new Types.dVdt.TestParameters() {IsEnabled = true, TestTypeId = testTypeId};

            FillOrder(testTypeId, testParams);

            FillConditionsResults(testTypeId, results);

            foreach (var result in results)
            {
                switch (result.Key)
                {
                    case "DVDT_En":
                        testParams.IsEnabled = Boolean.Parse(result.Value.ToString());
                        break;
                    case "DVDT_Mode":
                        testParams.Mode = (DvdtMode) Enum.Parse(typeof(DvdtMode), result.Value.ToString());
                        break;
                    case "DVDT_Voltage":
                        testParams.Voltage = UInt16.Parse(result.Value.ToString());
                        break;
                    case "DVDT_VoltageRate":
                        testParams.VoltageRate = (VoltageRate) Enum.Parse(typeof(VoltageRate), result.Value.ToString());
                        break;
                    case "DVDT_ConfirmationCount":
                        testParams.ConfirmationCount = UInt16.Parse(result.Value.ToString());
                        break;
                    case "DVDT_VoltageRateLimit":
                        testParams.VoltageRateLimit = UInt16.Parse(result.Value.ToString());
                        break;
                    case "DVDT_VoltageRateOffSet":
                        testParams.VoltageRateOffSet = UInt16.Parse(result.Value.ToString());
                        break;
                }
            }

            return testParams;
        }

        private Types.ATU.TestParameters FillAtuConditions(long testTypeId)
        {
            var results = new Dictionary<string, object>(3);
            var testParams = new Types.ATU.TestParameters() {IsEnabled = true, TestTypeId = testTypeId};

            FillOrder(testTypeId, testParams);

            FillConditionsResults(testTypeId, results);

            foreach (var result in results)
            {
                switch (result.Key)
                {
                    case "ATU_En":
                        testParams.IsEnabled = Boolean.Parse(result.Value.ToString());
                        break;

                    case "ATU_PrePulseValue":
                        testParams.PrePulseValue = UInt16.Parse(result.Value.ToString());
                        break;

                    case "ATU_PowerValue":
                        testParams.PowerValue = Float.ParseInternationally(result.Value.ToString());
                        break;
                }
            }

            return testParams;
        }

        private void FillAtuParameters(Types.ATU.TestParameters parameters, long testTypeId)
        {
            var results = new List<Tuple<string, float?, float?>>();
            FillParametersResults(testTypeId, results);

            foreach (var result in results)
            {
                switch (result.Item1)
                {
                    case "PRSM":
                        parameters.PRSM_Min = result.Item2 ?? 0;
                        parameters.PRSM_Max = result.Item3 ?? 0;
                        break;
                }
            }
        }

        private Types.QrrTq.TestParameters FillQrrTqConditions(long testTypeId)
        {
            var results = new Dictionary<string, object>(8);
            var testParams = new Types.QrrTq.TestParameters() {IsEnabled = true, TestTypeId = testTypeId};

            FillOrder(testTypeId, testParams);

            FillConditionsResults(testTypeId, results);

            foreach (var result in results)
            {
                switch (result.Key)
                {
                    case "QrrTq_En":
                        testParams.IsEnabled = Boolean.Parse(result.Value.ToString());
                        break;

                    case "QrrTq_Mode":
                        testParams.Mode = (Types.QrrTq.TMode) Enum.Parse(typeof(Types.QrrTq.TMode), result.Value.ToString());
                        break;

                    case "QrrTq_TrrMeasureBy9050Method":
                        testParams.TrrMeasureBy9050Method = bool.Parse(result.Value.ToString());
                        break;

                    case "QrrTq_DirectCurrent":
                        testParams.DirectCurrent = UInt16.Parse(result.Value.ToString());
                        break;

                    case "QrrTq_DCPulseWidth":
                        testParams.DCPulseWidth = UInt16.Parse(result.Value.ToString());
                        break;

                    case "QrrTq_DCRiseRate":
                        testParams.DCRiseRate = Float.ParseInternationally(result.Value.ToString());
                        break;

                    case "QrrTq_DCFallRate":
                        testParams.DCFallRate = (Types.QrrTq.TDcFallRate) Enum.Parse(typeof(Types.QrrTq.TDcFallRate), result.Value.ToString());
                        break;

                    case "QrrTq_OffStateVoltage":
                        testParams.OffStateVoltage = UInt16.Parse(result.Value.ToString());
                        break;

                    case "QrrTq_OsvRate":
                        testParams.OsvRate = (Types.QrrTq.TOsvRate) Enum.Parse(typeof(Types.QrrTq.TOsvRate), result.Value.ToString());
                        break;
                }
            }

            return testParams;
        }

        private Types.TOU.TestParameters FillTOUConditions(long testTypeId)
        {
            var results = new Dictionary<string, object>(2);

            var testParams = new Types.TOU.TestParameters();
            testParams.IsEnabled = true;
            testParams.TestTypeId = testTypeId;

            FillOrder(testTypeId, testParams);

            FillConditionsResults(testTypeId, results);

            foreach (var result in results)
            {
                switch (result.Key)
                {
                    case "TOU_En":
                        testParams.IsEnabled = bool.Parse(result.Value.ToString());
                        break;

                    case "TOU_ITM":
                        testParams.CurrentAmplitude = ushort.Parse(result.Value.ToString());
                        break;
                }
            }

            return testParams;
        }

        private void FillComutationConditions(ProfileDeepData data, long testTypeId)
        {
            var results = new Dictionary<string, object>(2);

            FillConditionsResults(testTypeId, results);

            foreach (var result in results)
                data.CommutationType = (ModuleCommutationType) Enum.Parse(typeof(ModuleCommutationType), result.Value.ToString());
        }

        private void FillClampConditions(ProfileDeepData data, long testTypeId)
        {
            var results = new Dictionary<string, object>(1);

            FillConditionsResults(testTypeId, results);

            foreach (var result in results)
            {
                switch (result.Key)
                {
                    case "CLAMP_HeightMeasure":
                        data.IsHeightMeasureEnabled = Boolean.Parse(result.Value.ToString());
                        break;
                    case "CLAMP_HeightValue":
                        data.Height = ushort.Parse(result.Value.ToString());
                        break;
                    case "CLAMP_Force":
                        data.ParameterClamp = long.Parse(result.Value.ToString());
                        break;
                    case "CLAMP_Temperature":
                        data.Temperature = ushort.Parse(result.Value.ToString());
                        break;
                }
            }
        }

        private Types.Gate.TestParameters FillGateConditions(long testTypeId)
        {
            var results = new Dictionary<string, object>(3);
            var testParams = new Types.Gate.TestParameters() {IsEnabled = true, TestTypeId = testTypeId};

            FillOrder(testTypeId, testParams);

            FillConditionsResults(testTypeId, results);

            foreach (var result in results)
            {
                switch (result.Key)
                {
                    case "Gate_IHEn":
                        testParams.IsIhEnabled = Boolean.Parse(result.Value.ToString());
                        break;
                    case "Gate_ILEn":
                        testParams.IsIlEnabled = Boolean.Parse(result.Value.ToString());
                        break;
                    case "Gate_EnableCurrent":
                        testParams.IsCurrentEnabled = Boolean.Parse(result.Value.ToString());
                        break;
                    case "Gate_EnableIHStrike":
                        testParams.IsIhStrikeCurrentEnabled = Boolean.Parse(result.Value.ToString());
                        break;
                }
            }

            return testParams;
        }

        private void FillGateParameters(Types.Gate.TestParameters parameters, long testTypeId)
        {
            var results = new List<Tuple<string, float?, float?>>();
            FillParametersResults(testTypeId, results);

            foreach (var result in results)
            {
                switch (result.Item1)
                {
                    case "RG":
                        if (result.Item3.HasValue)
                            parameters.Resistance = result.Item3.Value;
                        break;
                    case "IGT":
                        if (result.Item3.HasValue)
                            parameters.IGT = result.Item3.Value;
                        if (result.Item2.HasValue)
                            parameters.MinIGT = result.Item2.Value;
                        break;
                    case "VGT":
                        if (result.Item3.HasValue)
                            parameters.VGT = result.Item3.Value;
                        break;
                    case "IH":
                        if (parameters.IsIhEnabled && result.Item3.HasValue)
                            parameters.IH = result.Item3.Value;
                        break;
                    case "IL":
                        if (parameters.IsIlEnabled && result.Item3.HasValue)
                            parameters.IL = result.Item3.Value;
                        break;
                }
            }
        }

        private Types.BVT.TestParameters FillBvtConditions(long testTypeId)
        {
            var results = new Dictionary<string, object>(9);
            var testParams = new Types.BVT.TestParameters() {IsEnabled = true, TestTypeId = testTypeId};

            FillOrder(testTypeId, testParams);

            FillConditionsResults(testTypeId, results);

            foreach (var result in results)
            {
                switch (result.Key)
                {
                    case "BVT_UseUdsmUrsm":
                        testParams.UseUdsmUrsm = Convert.ToBoolean(result.Value);
                        break;

                    case "BVT_PulseFrequency":
                        testParams.PulseFrequency = Convert.ToUInt16(result.Value);
                        break;

                    case "BVT_Type":
                        testParams.TestType = (BVTTestType) (Enum.Parse(typeof(BVTTestType), result.Value.ToString()));
                        break;

                    case "BVT_I":
                        testParams.CurrentLimit = Float.ParseInternationally(result.Value.ToString());
                        break;

                    case "BVT_RumpUp":
                        testParams.RampUpVoltage = Float.ParseInternationally(result.Value.ToString());
                        break;

                    case "BVT_StartV":
                        testParams.StartVoltage = UInt16.Parse(result.Value.ToString());
                        break;

                    case "BVT_F":
                        testParams.VoltageFrequency = UInt16.Parse(result.Value.ToString());
                        break;

                    case "BVT_FD":
                        testParams.FrequencyDivisor = UInt16.Parse(result.Value.ToString());
                        break;

                    case "BVT_Mode":
                        testParams.MeasurementMode =
                            (BVTMeasurementMode) (Enum.Parse(typeof(BVTMeasurementMode), result.Value.ToString()));
                        break;

                    case "BVT_VR":
                        switch (testParams.TestType)
                        {
                            case BVTTestType.Both:
                            case BVTTestType.Reverse:
                                testParams.VoltageLimitR = UInt16.Parse(result.Value.ToString());
                                break;
                        }

                        break;

                    case "BVT_VD":
                        switch (testParams.TestType)
                        {
                            case BVTTestType.Both:
                            case BVTTestType.Direct:
                                testParams.VoltageLimitD = UInt16.Parse(result.Value.ToString());
                                break;
                        }

                        break;

                    case "BVT_PlateTime":
                        testParams.PlateTime = UInt16.Parse(result.Value.ToString());
                        break;


                    case "BVT_UdsmUrsm_PulseFrequency":
                        testParams.UdsmUrsmPulseFrequency = Convert.ToUInt16(result.Value);
                        break;

                    case "BVT_UdsmUrsm_Type":
                        testParams.UdsmUrsmTestType = (BVTTestType) (Enum.Parse(typeof(BVTTestType), result.Value.ToString()));
                        break;

                    case "BVT_UdsmUrsm_I":
                        testParams.UdsmUrsmCurrentLimit = Float.ParseInternationally(result.Value.ToString());
                        break;

                    case "BVT_UdsmUrsm_RumpUp":
                        testParams.UdsmUrsmRampUpVoltage = Float.ParseInternationally(result.Value.ToString());
                        break;

                    case "BVT_UdsmUrsm_StartV":
                        testParams.UdsmUrsmStartVoltage = UInt16.Parse(result.Value.ToString());
                        break;

                    case "BVT_UdsmUrsm_F":
                        testParams.UdsmUrsmVoltageFrequency = UInt16.Parse(result.Value.ToString());
                        break;

                    case "BVT_UdsmUrsm_FD":
                        testParams.UdsmUrsmFrequencyDivisor = UInt16.Parse(result.Value.ToString());
                        break;

                    case "BVT_UdsmUrsm_VR":
                        switch (testParams.TestType)
                        {
                            case BVTTestType.Both:
                            case BVTTestType.Reverse:
                                testParams.UdsmUrsmVoltageLimitR = UInt16.Parse(result.Value.ToString());
                                break;
                        }

                        break;

                    case "BVT_UdsmUrsm_VD":
                        switch (testParams.TestType)
                        {
                            case BVTTestType.Both:
                            case BVTTestType.Direct:
                                testParams.UdsmUrsmVoltageLimitD = UInt16.Parse(result.Value.ToString());
                                break;
                        }

                        break;

                    case "BVT_UdsmUrsm_PlateTime":
                        testParams.UdsmUrsmPlateTime = UInt16.Parse(result.Value.ToString());
                        break;
                }
            }

            return testParams;
        }

        private void FillBvtParameters(Types.BVT.TestParameters parameters, long testTypeId)
        {
            var results = new List<Tuple<string, float?, float?>>();
            FillParametersResults(testTypeId, results);

            foreach (var result in results)
            {
                switch (result.Item1)
                {
                    case "VRRM":
                        if (result.Item2.HasValue)
                            parameters.VRRM = Convert.ToUInt16(result.Item2.Value);
                        break;
                    case "VDRM":
                        if (result.Item2.HasValue)
                            parameters.VDRM = Convert.ToUInt16(result.Item2.Value);
                        break;
                    case "IRRM":
                        if (result.Item3.HasValue)
                            parameters.IRRM = Convert.ToUInt16(result.Item3.Value);
                        break;
                    case "IDRM":
                        if (result.Item3.HasValue)
                            parameters.IDRM = Convert.ToUInt16(result.Item3.Value);
                        break;

                    case "UdsmUrsm_IRRM":
                        if (result.Item3.HasValue)
                            parameters.UdsmUrsmIRRM = Convert.ToUInt16(result.Item3.Value);
                        break;
                    case "UdsmUrsm_IDRM":
                        if (result.Item3.HasValue)
                            parameters.UdsmUrsmIDRM = Convert.ToUInt16(result.Item3.Value);
                        break;
                }
            }
        }

        private Types.VTM.TestParameters FillSlConditions(long testTypeId)
        {
            var results = new Dictionary<string, object>(9);
            var testParams = new Types.VTM.TestParameters() {IsEnabled = true, TestTypeId = testTypeId};

            FillOrder(testTypeId, testParams);

            FillConditionsResults(testTypeId, results);

            #region switchSL

            foreach (var result in results)
            {
                switch (result.Key)
                {
                    case "SL_Type":
                        testParams.TestType = (Types.VTM.VTMTestType) (Enum.Parse(typeof(Types.VTM.VTMTestType), result.Value.ToString()));
                        break;
                    case "SL_FS":
                        testParams.UseFullScale = Boolean.Parse(result.Value.ToString());
                        break;
                    case "SL_N":
                        testParams.Count = UInt16.Parse(result.Value.ToString());
                        break;
                    case "SL_ITM":
                        switch (testParams.TestType)
                        {
                            case VTMTestType.Ramp:
                                testParams.RampCurrent = UInt16.Parse(result.Value.ToString());
                                break;
                            case VTMTestType.Sinus:
                                testParams.SinusCurrent = UInt16.Parse(result.Value.ToString());
                                break;
                            case VTMTestType.Curve:
                                testParams.CurveCurrent = UInt16.Parse(result.Value.ToString());
                                break;
                        }

                        break;
                    case "SL_Time":
                        switch (testParams.TestType)
                        {
                            case VTMTestType.Ramp:
                                testParams.RampTime = UInt16.Parse(result.Value.ToString());
                                break;
                            case VTMTestType.Sinus:
                                testParams.SinusTime = UInt16.Parse(result.Value.ToString());
                                break;
                            case VTMTestType.Curve:
                                testParams.CurveTime = UInt16.Parse(result.Value.ToString());
                                break;
                        }

                        break;
                    case "SL_OpenEn":
                        testParams.IsRampOpeningEnabled = Boolean.Parse(result.Value.ToString());
                        break;
                    case "SL_OpenI":
                        testParams.RampOpeningCurrent = UInt16.Parse(result.Value.ToString());
                        break;
                    case "SL_TimeEx":
                        switch (testParams.TestType)
                        {
                            case VTMTestType.Ramp:
                                testParams.RampOpeningTime = UInt16.Parse(result.Value.ToString());
                                break;
                            case VTMTestType.Curve:
                                testParams.CurveAddTime = UInt16.Parse(result.Value.ToString());
                                break;
                        }

                        break;
                    case "SL_Factor":
                        testParams.CurveFactor = UInt16.Parse(result.Value.ToString());
                        break;
                }
            }

            #endregion

            return testParams;
        }

        private void FillSlParameters(Types.VTM.TestParameters parameters, long testTypeId)
        {
            var results = new List<Tuple<string, float?, float?>>();
            FillParametersResults(testTypeId, results);

            foreach (var result in results)
            {
                switch (result.Item1)
                {
                    case "VTM":
                        if (result.Item3.HasValue)
                            parameters.VTM = result.Item3.Value;
                        break;
                }
            }
        }

        private void FillOrder(long testTypeId, BaseTestParametersAndNormatives testParams)
        {
            _orderSelect.Parameters["@TEST_TYPE_ID"].Value = testTypeId;

            var order = _orderSelect.ExecuteScalar().ToString();
            testParams.Order = int.Parse(order);
        }

        private void FillConditionsResults(long testTypeId, IDictionary<string, object> results)
        {
            _conditionSelect.Parameters["@TEST_TYPE_ID"].Value = testTypeId;

            using (var reader = _conditionSelect.ExecuteReader())
            {
                while (reader.Read())
                    results.Add(((string) reader[0]).Trim(), reader[1]);
            }
        }

        private void FillParametersResults(long testTypeId, ICollection<Tuple<string, float?, float?>> results)
        {
            _paramSelect.Parameters["@TEST_TYPE_ID"].Value = testTypeId;

            using (var reader = _paramSelect.ExecuteReader())
            {
                while (reader.Read())
                {
                    var name = reader[0].ToString().Trim();
                    float minVal;
                    var minValParsed = float.TryParse(reader[1].ToString(), out minVal);
                    float maxVal;
                    var maxValParsed = float.TryParse(reader[2].ToString(), out maxVal);

                    results.Add(new Tuple<string, float?, float?>(name, minValParsed ? minVal : (float?) null, maxValParsed ? maxVal : (float?) null));
                }
            }
        }

        #endregion
    }
}