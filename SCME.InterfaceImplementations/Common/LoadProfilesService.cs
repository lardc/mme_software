using SCME.Types;
using SCME.Types.BaseTestParams;
using SCME.Types.BVT;
using SCME.Types.Commutation;
using SCME.Types.dVdt;
using SCME.Types.Profiles;
using SCME.Types.VTM;
using SCME.Types.SQL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCME.InterfaceImplementations.Common
{
    //interface ILoadProfilesService_
    //{
    //    void PrepareQueries();
    //}

    public abstract class LoadProfilesService<TDbCommand, TDbConnection> where TDbCommand : DbCommand where TDbConnection : DbConnection
    {
        protected TDbConnection _Connection;
        protected TDbCommand _ChildsCmd;
        protected TDbCommand _CondCmd;
        protected TDbCommand _ProfileSelect;
        protected TDbCommand _ProfileSelectWithMME;
        protected TDbCommand _OrderSelect;
        protected TDbCommand _CondSelect;
        protected TDbCommand _ParamSelect;
        protected TDbCommand _ProfileSingleSelect;
        protected TDbCommand _ProfileByKey;

        public LoadProfilesService(TDbConnection connection)
        {
            _Connection = connection;
            if (_Connection.State != ConnectionState.Open)
                _Connection.Open();

            PrepareQueries();
        }

        public abstract void PrepareQueries();

        public List<ProfileForSqlSelect> GetProfilesSuperficially(string mmeCode)
        {
            DbCommand profileSelect;
            List<ProfileForSqlSelect> res = new List<ProfileForSqlSelect>();

            if (mmeCode == null)
                profileSelect = _ProfileSelect;
            else
            {
                profileSelect = _ProfileSelectWithMME;
                profileSelect.Parameters["@MME_CODE"].Value = mmeCode;
            }

            using (var reader = profileSelect.ExecuteReader())
                while (reader.Read())
                    res.Add(new ProfileForSqlSelect((int)reader[0], (string)reader[1], (Guid)reader[2], (int)reader[3], (DateTime)reader[4]));

            return res;
        }

        public ProfileForSqlSelect GetProfileByKey(Guid key)
        {
            _ProfileByKey.Parameters["@PROF_GUID"].Value = key;
            using (var reader = _ProfileByKey.ExecuteReader())
                while (reader.Read())
                    return new ProfileForSqlSelect((int)reader[0], (string)reader[1], (Guid)reader[2], (int)reader[3], (DateTime)reader[4]);
            throw new Exception($"GetProfileByKey, could not find item by key {key}");
        }

        public List<ProfileForSqlSelect> GetProfileChildsSuperficially(ProfileForSqlSelect profile)
        {
            List<ProfileForSqlSelect> res = new List<ProfileForSqlSelect>();

            _ChildsCmd.Parameters["@PROF_NAME"].Value = profile.Name;
            using (var reader = _ChildsCmd.ExecuteReader())
                while (reader.Read())
                    res.Add(new ProfileForSqlSelect((int)reader[0], (string)reader[1], (Guid)reader[2], (int)reader[4], (DateTime)reader[3]));

            return res;
        }

        public List<ProfileItem> GetProfileItemsSuperficially(string mmeCode)
        {
            return GetProfilesSuperficially(mmeCode).Select(m=> m.ToProfileItem()).ToList();
        }

        public List<ProfileItem> GetProfileItemsDeep(string mmeCode)
        {
            return GetProfilesSuperficially(mmeCode).Select(m=> LoadConditionsForProfile(m).ToProfileItem()).ToList();
        }

        public List<ProfileItem> GetProfileItemsWithChildSuperficially(string mmeCode)
        {
            return GetProfilesSuperficially(mmeCode).Select(m => m.ToProfileItemWithChild(GetProfileChildsSuperficially(m))).ToList();
        }


        public Profile GetProfileDeep(Guid key)
        {
            return LoadConditionsForProfile(GetProfileByKey(key));
        }

        private Profile LoadConditionsForProfile(ProfileForSqlSelect prof)
        {
            var profile = prof.ToProfile();
            var testTypes = new Dictionary<long, long>();

            _CondCmd.Parameters["@PROF_ID"].Value = prof.Id;

            using (var reader = _CondCmd.ExecuteReader())
                while (reader.Read())
                    testTypes.Add((int)reader[0], (int)reader[1]);

            foreach (var testType in testTypes)
                FillParameters(profile, testType.Key, testType.Value);

            return profile;
        }

        #region Fill

        private void FillParameters(Profile profile, long testTypeId, long testParametersType)
        {
            switch ((TestParametersType)testParametersType)
            {
                case TestParametersType.Gate:
                    var gatePars = FillGateConditions(testTypeId);
                    FillGateParameters(gatePars, testTypeId);
                    profile.TestParametersAndNormatives.Add(gatePars);
                    break;
                case TestParametersType.Bvt:
                    var bvtPars = FillBvtConditions(testTypeId);
                    FillBvtParameters(bvtPars, testTypeId);
                    profile.TestParametersAndNormatives.Add(bvtPars);
                    break;
                case TestParametersType.StaticLoses:
                    var slParams = FillSlConditions(testTypeId);
                    FillSlParameters(slParams, testTypeId);
                    profile.TestParametersAndNormatives.Add(slParams);
                    break;
                case TestParametersType.Dvdt:
                    var dVdtParams = FillDvdtConditions(testTypeId);
                    profile.TestParametersAndNormatives.Add(dVdtParams);
                    break;
                case TestParametersType.ATU:
                    var atuParams = FillAtuConditions(testTypeId);
                    profile.TestParametersAndNormatives.Add(atuParams);
                    break;
                case TestParametersType.QrrTq:
                    var qrrTqParams = FillQrrTqConditions(testTypeId);
                    profile.TestParametersAndNormatives.Add(qrrTqParams);
                    break;
                case TestParametersType.RAC:
                    var racParams = FillRACConditions(testTypeId);
                    profile.TestParametersAndNormatives.Add(racParams);
                    break;
                case TestParametersType.TOU:
                    var touParams = FillTOUConditions(testTypeId);
                    profile.TestParametersAndNormatives.Add(touParams);
                    break;
                case TestParametersType.Clamping:
                    FillClampConditions(profile, testTypeId);
                    break;
                case TestParametersType.Commutation:
                    FillComutationConditions(profile, testTypeId);
                    break;
            }
        }

        private Types.dVdt.TestParameters FillDvdtConditions(long testTypeId)
        {
            var results = new Dictionary<string, object>(3);
            var testParams = new Types.dVdt.TestParameters() { IsEnabled = true, TestTypeId = testTypeId };

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
                        testParams.Mode = (DvdtMode)Enum.Parse(typeof(DvdtMode), result.Value.ToString());
                        break;
                    case "DVDT_Voltage":
                        testParams.Voltage = UInt16.Parse(result.Value.ToString());
                        break;
                    case "DVDT_VoltageRate":
                        testParams.VoltageRate = (VoltageRate)Enum.Parse(typeof(VoltageRate), result.Value.ToString());
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
            var testParams = new Types.ATU.TestParameters() { IsEnabled = true, TestTypeId = testTypeId };

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
                        testParams.PowerValue = float.Parse(result.Value.ToString());
                        break;
                }
            }

            return testParams;
        }

        private Types.QrrTq.TestParameters FillQrrTqConditions(long testTypeId)
        {
            var results = new Dictionary<string, object>(8);
            var testParams = new Types.QrrTq.TestParameters() { IsEnabled = true, TestTypeId = testTypeId };

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
                        testParams.Mode = (Types.QrrTq.TMode)Enum.Parse(typeof(Types.QrrTq.TMode), result.Value.ToString());
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
                        testParams.DCRiseRate = float.Parse(result.Value.ToString());
                        break;

                    case "QrrTq_DCFallRate":
                        testParams.DCFallRate = (Types.QrrTq.TDcFallRate)Enum.Parse(typeof(Types.QrrTq.TDcFallRate), result.Value.ToString());
                        break;

                    case "QrrTq_OffStateVoltage":
                        testParams.OffStateVoltage = UInt16.Parse(result.Value.ToString());
                        break;

                    case "QrrTq_OsvRate":
                        testParams.OsvRate = (Types.QrrTq.TOsvRate)Enum.Parse(typeof(Types.QrrTq.TOsvRate), result.Value.ToString());
                        break;
                }
            }

            return testParams;
        }

        private Types.RAC.TestParameters FillRACConditions(long testTypeId)
        {
            var results = new Dictionary<string, object>(2);
            var testParams = new Types.RAC.TestParameters() { IsEnabled = true, TestTypeId = testTypeId };

            FillOrder(testTypeId, testParams);

            FillConditionsResults(testTypeId, results);

            foreach (var result in results)
            {
                switch (result.Key)
                {
                    case "RAC_En":
                        testParams.IsEnabled = Boolean.Parse(result.Value.ToString());
                        break;

                    case "RAC_ResVoltage":
                        testParams.ResVoltage = UInt16.Parse(result.Value.ToString());
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

        private void FillComutationConditions(Profile profile, long testTypeId)
        {
            var results = new Dictionary<string, object>(2);

            FillConditionsResults(testTypeId, results);

            foreach (var result in results)
            {
                profile.ParametersComm =
                    (ModuleCommutationType)Enum.Parse(typeof(ModuleCommutationType), result.Value.ToString());
            }
        }

        private void FillClampConditions(Profile profile, long testTypeId)
        {
            var results = new Dictionary<string, object>(1);

            FillConditionsResults(testTypeId, results);

            foreach (var result in results)
            {
                switch (result.Key)
                {
                    case "CLAMP_HeightMeasure":
                        profile.IsHeightMeasureEnabled = Boolean.Parse(result.Value.ToString());
                        break;
                    case "CLAMP_HeightValue":
                        profile.Height = ushort.Parse(result.Value.ToString());
                        break;
                    case "CLAMP_Force":
                        profile.ParametersClamp = long.Parse(result.Value.ToString());
                        break;
                    case "CLAMP_Temperature":
                        profile.Temperature = ushort.Parse(result.Value.ToString());
                        break;
                }
            }
        }

        private Types.Gate.TestParameters FillGateConditions(long testTypeId)
        {
            var results = new Dictionary<string, object>(3);
            var testParams = new Types.Gate.TestParameters() { IsEnabled = true, TestTypeId = testTypeId };

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
            var testParams = new Types.BVT.TestParameters() { IsEnabled = true, TestTypeId = testTypeId };

            FillOrder(testTypeId, testParams);

            FillConditionsResults(testTypeId, results);

            foreach (var result in results)
            {
                switch (result.Key)
                {
                    case "BVT_Type":
                        testParams.TestType = (BVTTestType)(Enum.Parse(typeof(BVTTestType), result.Value.ToString()));
                        break;

                    case "BVT_I":
                        testParams.CurrentLimit = float.Parse(result.Value.ToString());
                        break;

                    case "BVT_RumpUp":
                        testParams.RampUpVoltage = float.Parse(result.Value.ToString());
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
                            (BVTMeasurementMode)(Enum.Parse(typeof(BVTMeasurementMode), result.Value.ToString()));
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
                }
            }
        }

        private Types.VTM.TestParameters FillSlConditions(long testTypeId)
        {
            var results = new Dictionary<string, object>(9);
            var testParams = new Types.VTM.TestParameters() { IsEnabled = true, TestTypeId = testTypeId };

            FillOrder(testTypeId, testParams);

            FillConditionsResults(testTypeId, results);

            #region switchSL

            foreach (var result in results)
            {
                switch (result.Key)
                {
                    case "SL_Type":
                        testParams.TestType = (Types.VTM.VTMTestType)(Enum.Parse(typeof(Types.VTM.VTMTestType), result.Value.ToString()));
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
            _OrderSelect.Parameters["@TEST_TYPE_ID"].Value = testTypeId;

            var order = _OrderSelect.ExecuteScalar().ToString();
            testParams.Order = int.Parse(order);
        }

        private void FillConditionsResults(long testTypeId, IDictionary<string, object> results)
        {
            _CondSelect.Parameters["@TEST_TYPE_ID"].Value = testTypeId;

            using (var reader = _CondSelect.ExecuteReader())
            {
                while (reader.Read())
                    results.Add(((string)reader[0]).Trim(), reader[1]);
            }
        }

        private void FillParametersResults(long testTypeId, ICollection<Tuple<string, float?, float?>> results)
        {
            _ParamSelect.Parameters["@TEST_TYPE_ID"].Value = testTypeId;

            using (var reader = _ParamSelect.ExecuteReader())
            {
                while (reader.Read())
                {
                    var name = reader[0].ToString().Trim();
                    float minVal;
                    var minValParsed = float.TryParse(reader[1].ToString(), out minVal);
                    float maxVal;
                    var maxValParsed = float.TryParse(reader[2].ToString(), out maxVal);

                    results.Add(new Tuple<string, float?, float?>(name, minValParsed ? minVal : (float?)null, maxValParsed ? maxVal : (float?)null));
                }
            }
        }

        #endregion
    }
}
