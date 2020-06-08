using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using SCME.Interfaces;
using SCME.Types;
using SCME.Types.BaseTestParams;
using SCME.Types.BVT;
using SCME.Types.Commutation;
using SCME.Types.dVdt;
using SCME.Types.Profiles;
using SCME.Types.SL;
using TestParameters = SCME.Types.Gate.TestParameters;

namespace SCME.InterfaceImplementations
{
    public class SQLiteLoadProfilesService : ILoadProfilesService
    {
        private readonly SQLiteConnection _connection;

        public SQLiteLoadProfilesService(SQLiteConnection connection)
        {
            _connection = connection;
            if (_connection.State != ConnectionState.Open)
                _connection.Open();
        }

        public List<ProfileItem> GetProfileItems()
        {
            try
            {
                var profilesList = new List<ProfileItem>();
                var profilesSelect = "SELECT P.PROF_ID, P.PROF_NAME, P.PROF_GUID, Max(P.PROF_TS), P.PROF_VERS, P.IsDelete FROM PROFILES P GROUP BY P.PROF_NAME";
                var profilesDict = new List<Tuple<long, string, Guid, DateTime>>();

                using (var condCmd = _connection.CreateCommand())
                {
                    condCmd.CommandText = profilesSelect;

                    using (var reader = condCmd.ExecuteReader())
                    {
                        while (reader.Read())
                            if((int)reader[5] == 0)
                                profilesDict.Add(new Tuple<long, string, Guid, DateTime>((long)reader[0], (string)reader[1], (Guid)reader[2], DateTime.Parse((string)reader[3])));
                    }
                }

                foreach (var prof in profilesDict)
                {
                    var profile = Profile(prof);

                    var profilesChildsDict = new List<Tuple<long, string, Guid, DateTime>>();

                    using (var childsCmd = _connection.CreateCommand())
                    {
                        childsCmd.CommandText =
                            "SELECT PROF_ID,PROF_NAME,PROF_GUID,PROF_TS,PROF_VERS FROM PROFILES WHERE PROF_NAME=@PROF_NAME ORDER BY PROF_TS DESC;";
                        childsCmd.Parameters.Add("@PROF_NAME", DbType.String);
                        childsCmd.Prepare();
                        childsCmd.Parameters["@PROF_NAME"].Value = prof.Item2;
                        using (var reader = childsCmd.ExecuteReader())
                        {
                            while (reader.Read())
                                profilesChildsDict.Add(new Tuple<long, string, Guid, DateTime>((long)reader[0], (string)reader[1], (Guid)reader[2], (DateTime)reader[3]));
                        }
                    }

                    var profileItem = new ProfileItem
                    {
                        ProfileId = prof.Item1,
                        ProfileName = profile.Name,
                        ProfileKey = profile.Key,
                        ProfileTS = profile.Timestamp,
                        GateTestParameters = new List<TestParameters>(),
                        VTMTestParameters = new List<Types.SL.TestParameters>(),
                        BVTTestParameters = new List<Types.BVT.TestParameters>(),
                        DvDTestParameterses = new List<Types.dVdt.TestParameters>(),
                        CommTestParameters = profile.ParametersComm,
                        IsHeightMeasureEnabled = profile.IsHeightMeasureEnabled,
                        ParametersClamp = profile.ParametersClamp,
                        Height = profile.Height,
                        Temperature = profile.Temperature,
                        ChildProfileItems = new List<ProfileItem>(profilesChildsDict.Count - 1)

                    };
                    AddParameters(profile, profileItem);
                    for (int i = 0; i < profilesChildsDict.Count; i++)
                    {
                        if (i == 0)
                            continue;
                        var childProfile = Profile(profilesChildsDict[i]);

                        var childProfileItem = new ProfileItem
                        {
                            ProfileId = profilesChildsDict[i].Item1,
                            ProfileName = childProfile.Name,
                            ProfileKey = childProfile.Key,
                            ProfileTS = childProfile.Timestamp,
                            GateTestParameters = new List<TestParameters>(),
                            VTMTestParameters = new List<Types.SL.TestParameters>(),
                            BVTTestParameters = new List<Types.BVT.TestParameters>(),
                            DvDTestParameterses = new List<Types.dVdt.TestParameters>(),
                            CommTestParameters = childProfile.ParametersComm,
                            IsHeightMeasureEnabled = childProfile.IsHeightMeasureEnabled,
                            ParametersClamp = childProfile.ParametersClamp,
                            Height = childProfile.Height,
                            Temperature = childProfile.Temperature
                        };
                        AddParameters(childProfile, childProfileItem);

                        profileItem.ChildProfileItems.Add(childProfileItem);
                    }

                    profilesList.Add(profileItem);
                }



                return profilesList;

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private static void AddParameters(Profile profile, ProfileItem profileItem)
        {
            foreach (var baseTestParametersAndNormativese in profile.TestParametersAndNormatives)
            {
                var gate = baseTestParametersAndNormativese as TestParameters;
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
                if(dvdt != null)
                    profileItem.DvDTestParameterses.Add(dvdt);
            }
        }

        private Profile Profile(Tuple<long, string, Guid, DateTime> prof)
        {
            var profile = new Profile(prof.Item2, prof.Item3, prof.Item4);

            var testTypes = new Dictionary<long, long>(5);

            using (
                var condCmd = new SQLiteCommand("SELECT ID, TEST_TYPE_ID FROM PROF_TEST_TYPE WHERE PROF_ID=@PROF_ID",
                    _connection))
            {
                condCmd.Parameters.Add("@PROF_ID", DbType.Int64);
                condCmd.Prepare();
                condCmd.Parameters["@PROF_ID"].Value = prof.Item1;

                using (var reader = condCmd.ExecuteReader())
                {
                    while (reader.Read())
                        testTypes.Add((long)reader[0], (long)reader[1]);
                }
                foreach (var testType in testTypes)
                {
                    FillParameters(profile, testType.Key, testType.Value);
                }
            }
            return profile;
        }

        public List<ProfileItem> GetProfileItems(string mmeCode)
        {
            try
            {
                var profilesList = new List<ProfileItem>();
                var profilesSelect = @"SELECT P.PROF_ID, P.PROF_NAME, P.PROF_GUID, Max(P.PROF_TS), P.PROF_VERS FROM PROFILES P 
                                       JOIN MME_CODES_TO_PROFILES MCP ON MCP.PROFILE_ID = P.PROF_ID
                                       JOIN MME_CODES MC ON MC.MME_CODE_ID = MCP.MME_CODE_ID
                                       WHERE MC.MME_CODE = @MME_CODE
                                       GROUP BY P.PROF_NAME";

                var profilesDict = new List<Tuple<long, string, Guid, DateTime>>();

                using (var condCmd = _connection.CreateCommand())
                {
                    condCmd.CommandText = profilesSelect;
                    condCmd.Parameters.Add("@MME_CODE", DbType.String);
                    condCmd.Prepare();
                    condCmd.Parameters["@MME_CODE"].Value = mmeCode;

                    using (var reader = condCmd.ExecuteReader())
                    {
                        while (reader.Read())
                            profilesDict.Add(new Tuple<long, string, Guid, DateTime>((long)reader[0], (string)reader[1], (Guid)reader[2], DateTime.Parse((string)reader[3])));
                    }
                }

                foreach (var prof in profilesDict)
                {
                    var profile = new Profile(prof.Item2, prof.Item3, prof.Item4);

                    var testTypes = new Dictionary<long, long>(5);

                    using (var condCmd = new SQLiteCommand("SELECT ID, TEST_TYPE_ID FROM PROF_TEST_TYPE WHERE PROF_ID=@PROF_ID", _connection))
                    {
                        condCmd.Parameters.Add("@PROF_ID", DbType.Int64);
                        condCmd.Prepare();
                        condCmd.Parameters["@PROF_ID"].Value = prof.Item1;

                        using (var reader = condCmd.ExecuteReader())
                        {
                            while (reader.Read())
                                testTypes.Add((long)reader[0], (long)reader[1]);
                        }
                        foreach (var testType in testTypes)
                        {
                            FillParameters(profile, testType.Key, testType.Value);
                        }
                    }

                    var profileItem = new ProfileItem
                    {
                        ProfileId = prof.Item1,
                        ProfileName = profile.Name,
                        ProfileKey = profile.Key,
                        ProfileTS = profile.Timestamp,
                        GateTestParameters = new List<TestParameters>(),
                        VTMTestParameters = new List<Types.SL.TestParameters>(),
                        BVTTestParameters = new List<Types.BVT.TestParameters>(),
                        DvDTestParameterses = new List<Types.dVdt.TestParameters>(),
                        CommTestParameters = profile.ParametersComm,
                        IsHeightMeasureEnabled = profile.IsHeightMeasureEnabled,
                        ParametersClamp = profile.ParametersClamp,
                        Height = profile.Height,
                        Temperature = profile.Temperature

                    };
                    foreach (var baseTestParametersAndNormativese in profile.TestParametersAndNormatives)
                    {
                        var gate = baseTestParametersAndNormativese as TestParameters;
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

                    }

                    profilesList.Add(profileItem);
                }



                return profilesList;

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        private void FillParameters(Profile profile, long testTypeId, long testParametersType)
        {
            switch (testParametersType)
            {
                case (int)TestParametersType.Gate:
                    var gatePars = FillGateConditions(testTypeId);
                    FillGateParameters(gatePars, testTypeId);
                    profile.TestParametersAndNormatives.Add(gatePars);
                    break;
                case (int)TestParametersType.Bvt:
                    var bvtPars = FillBvtConditions(testTypeId);
                    FillBvtParameters(bvtPars, testTypeId);
                    profile.TestParametersAndNormatives.Add(bvtPars);
                    break;
                case (int)TestParametersType.StaticLoses:
                    var slParams = FillSlConditions(testTypeId);
                    FillSlParameters(slParams, testTypeId);
                    profile.TestParametersAndNormatives.Add(slParams);
                    break;
                case (int)TestParametersType.Dvdt:
                    var dVdtParams = FillDvdtConditions(testTypeId);
                    profile.TestParametersAndNormatives.Add(dVdtParams);
                    break;
                case (int)TestParametersType.Clamping:
                    FillClampConditions(profile, testTypeId);
                    break;
                case (int)TestParametersType.Commutation:
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

        private TestParameters FillGateConditions(long testTypeId)
        {
            var results = new Dictionary<string, object>(3);
            var testParams = new TestParameters() { IsEnabled = true, TestTypeId = testTypeId };

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

        private void FillOrder(long testTypeId, BaseTestParametersAndNormatives testParams)
        {
            using (var condCmd = new SQLiteCommand("SELECT [ORDER] FROM PROF_TEST_TYPE WHERE ID=@TEST_TYPE_ID", _connection))
            {
                condCmd.Parameters.Add("@TEST_TYPE_ID", DbType.Int64);
                condCmd.Prepare();
                condCmd.Parameters["@TEST_TYPE_ID"].Value = testTypeId;

                var order = condCmd.ExecuteScalar().ToString();
                testParams.Order = int.Parse(order);
            }
        }

        private void FillGateParameters(TestParameters parameters, long testTypeId)
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

        private Types.SL.TestParameters FillSlConditions(long testTypeId)
        {
            var results = new Dictionary<string, object>(9);
            var testParams = new Types.SL.TestParameters() { IsEnabled = true, TestTypeId = testTypeId };

            FillOrder(testTypeId, testParams);

            FillConditionsResults(testTypeId, results);

            #region switchSL

            foreach (var result in results)
            {
                switch (result.Key)
                {

                    case "SL_Type":
                        testParams.TestType = (VTMTestType)(Enum.Parse(typeof(VTMTestType), result.Value.ToString()));
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

        private void FillSlParameters(Types.SL.TestParameters parameters, long testTypeId)
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

        private void FillConditionsResults(long testTypeId, IDictionary<string, object> results)
        {
            using (var condCmd = new SQLiteCommand("SELECT C.COND_NAME, PC.VALUE FROM PROF_COND PC LEFT JOIN CONDITIONS C on C.COND_ID = PC.COND_ID WHERE PC.PROF_TESTTYPE_ID=@TEST_TYPE_ID", _connection))
            {
                condCmd.Parameters.Add("@TEST_TYPE_ID", DbType.Int64);
                condCmd.Prepare();
                condCmd.Parameters["@TEST_TYPE_ID"].Value = testTypeId;

                using (var reader = condCmd.ExecuteReader())
                {
                    while (reader.Read())
                        results.Add((string)reader[0], reader[1]);
                }
            }
        }

        private void FillParametersResults(long testTypeId, ICollection<Tuple<string, float?, float?>> results)
        {
            using (var condCmd = new SQLiteCommand("SELECT P.PARAM_NAME, PP.MIN_VAL, PP.MAX_VAL FROM PROF_PARAM PP LEFT JOIN PARAMS P on P.PARAM_ID = PP.PARAM_ID WHERE PP.PROF_TESTTYPE_ID=@TEST_TYPE_ID", _connection))
            {
                condCmd.Parameters.Add("@TEST_TYPE_ID", DbType.Int64);
                condCmd.Prepare();
                condCmd.Parameters["@TEST_TYPE_ID"].Value = testTypeId;

                using (var reader = condCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var name = reader[0].ToString();
                        float minVal;
                        var minValParsed = float.TryParse(reader[1].ToString(), out minVal);
                        float maxVal;
                        var maxValParsed = float.TryParse(reader[2].ToString(), out maxVal);

                        results.Add(new Tuple<string, float?, float?>(name, minValParsed ? minVal : (float?)null, maxValParsed ? maxVal : (float?)null));
                    }

                }
            }
        }
    }
}
