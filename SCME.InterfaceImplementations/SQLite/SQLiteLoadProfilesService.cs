using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using SCME.Types;
using SCME.Types.BaseTestParams;
using SCME.Types.BVT;
using SCME.Types.Commutation;
using SCME.Types.dVdt;
using SCME.Types.Interfaces;
using SCME.Types.Profiles;
using SCME.Types.SL;
using SCME.Types.SQL;

namespace SCME.InterfaceImplementations
{
    public class SQLiteLoadProfilesService : ILoadProfilesService
    {
        private readonly SQLiteConnection _connection;

        //private Entities GetContext => new Entities(ConnectionStringForEF);

        //private string _ConnectionStringForEF;
        //private string ConnectionStringForEF
        //{
        //    get
        //    {
        //        return _ConnectionStringForEF;
        //    }
        //    set
        //    {
        //        string cs = value;
        //        int index1 = cs.IndexOf("data source");
        //        int index2 = cs.IndexOf(";");
        //        _ConnectionStringForEF = cs.Substring(index1, index2 - index1);
        //    }

        //}
        public SQLiteLoadProfilesService(SQLiteConnection connection)
        {
            //ConnectionStringForEF = connection.ConnectionString;
            _connection = connection;
            if (_connection.State != ConnectionState.Open)
                _connection.Open();
        }

        public List<ProfileItem> GetProfileItems()
        {
            try
            {
                var profilesList = new List<ProfileItem>();
                var profilesSelect = "SELECT P.PROF_ID, P.PROF_NAME, P.PROF_GUID, P.PROF_VERS, Max(P.PROF_TS), P.PROF_VERS, P.IsDelete FROM PROFILES P GROUP BY P.PROF_NAME";
                var profilesDict = new List<ProfileForSqlSelect>();

                using (var condCmd = _connection.CreateCommand())
                {
                    condCmd.CommandText = profilesSelect;

                    using (var reader = condCmd.ExecuteReader())
                    {
                        while (reader.Read())
                            if ((int)reader[5] == 0)
                                profilesDict.Add(new ProfileForSqlSelect((int)reader[0], (string)reader[1], (Guid)reader[2], (int)reader[3], (DateTime)reader[4]));
                    }
                }

                foreach (var prof in profilesDict)
                {
                    var profile = Profile(prof);

                    var profilesChildsDict = new List<ProfileForSqlSelect>();

                    using (var childsCmd = _connection.CreateCommand())
                    {
                        childsCmd.CommandText = "SELECT PROF_ID,PROF_NAME,PROF_GUID,PROF_TS,PROF_VERS FROM PROFILES WHERE PROF_NAME=@PROF_NAME ORDER BY PROF_TS DESC;";
                        childsCmd.Parameters.Add("@PROF_NAME", DbType.String);
                        childsCmd.Prepare();
                        childsCmd.Parameters["@PROF_NAME"].Value = prof.Name;
                        using (var reader = childsCmd.ExecuteReader())
                        {
                            while (reader.Read())
                                profilesChildsDict.Add(new ProfileForSqlSelect((int)reader[0], (string)reader[1], (Guid)reader[2], (int)reader[3], (DateTime)reader[4] ));
                        }
                    }

                    var profileItem = new ProfileItem
                    {
                        ProfileId = prof.Id,
                        ProfileName = profile.Name,
                        ProfileKey = profile.Key,
                        ProfileTS = profile.Timestamp,
                        Version = prof.Version,
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
                            ProfileId = profilesChildsDict[i].Id,
                            ProfileName = childProfile.Name,
                            ProfileKey = childProfile.Key,
                            ProfileTS = childProfile.Timestamp,
                            Version = profilesChildsDict[i].Version,
                            GateTestParameters = new List<Types.Gate.TestParameters>(),
                            VTMTestParameters = new List<Types.SL.TestParameters>(),
                            BVTTestParameters = new List<Types.BVT.TestParameters>(),
                            DvDTestParameterses = new List<Types.dVdt.TestParameters>(),
                            ATUTestParameters = new List<Types.ATU.TestParameters>(),
                            QrrTqTestParameters = new List<Types.QrrTq.TestParameters>(),
                            RACTestParameters = new List<Types.RAC.TestParameters>(),
                            TOUTestParameters = new List<Types.TOU.TestParameters>(),
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
        }

        private Profile Profile(ProfileForSqlSelect prof)
        {
            var profile = new Profile(prof.Name, prof.Key, prof.Version, prof.TS);

            var testTypes = new Dictionary<long, long>(5);

            using (
                var condCmd = new SQLiteCommand("SELECT ID, TEST_TYPE_ID FROM PROF_TEST_TYPE WHERE PROF_ID=@PROF_ID",
                    _connection))
            {
                condCmd.Parameters.Add("@PROF_ID", DbType.Int64);
                condCmd.Prepare();
                condCmd.Parameters["@PROF_ID"].Value = prof.Id;

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

        //public List<ProfileItem> GetProfileItems(string mmeCode)
        //{
        //    List<ProfileItem> ProfileItems = new List<ProfileItem>();

        //    List<PROFILE> highVersionProfiles;

        //    using (var db = GetContext)
        //    {
        //        var profilesByMME = db.MME_CODES.Include(m=> m.MME_CODES_TO_PROFILES)
        //                                        .ThenInclude(m=> m.PROFILE)
        //                                        .ThenInclude(m=> m.PROF_TEST_TYPE)
        //            .Single(m => m.Name == mmeCode).MME_CODES_TO_PROFILES.Select(m => m.PROFILE).GroupBy(m => m.PROF_NAME);
        //        highVersionProfiles = profilesByMME.Select(m => m.OrderByDescending(n => n.PROF_VERS).First()).ToList();
        //        db.PROF_TEST_TYPE.Load();
        //    }

        //    foreach (var prof in highVersionProfiles)
        //    {
        //        var profile = new Profile(prof.PROF_NAME, prof.PROF_GUID, prof.PROF_TS);

        //        foreach (var testType in prof.PROF_TEST_TYPE)
        //            FillParameters(profile, testType.ID, testType.TEST_TYPE_ID);

        //        var profileItem = new ProfileItem
        //        {
        //            ProfileId = prof.PROF_ID,
        //            ProfileName = prof.PROF_NAME,
        //            ProfileKey = prof.PROF_GUID,
        //            ProfileTS = prof.PROF_TS,
        //            GateTestParameters = new List<Types.Gate.TestParameters>(),
        //            VTMTestParameters = new List<Types.SL.TestParameters>(),
        //            BVTTestParameters = new List<Types.BVT.TestParameters>(),
        //            DvDTestParameterses = new List<Types.dVdt.TestParameters>(),
        //            ATUTestParameters = new List<Types.ATU.TestParameters>(),
        //            QrrTqTestParameters = new List<Types.QrrTq.TestParameters>(),
        //            RACTestParameters = new List<Types.RAC.TestParameters>(),
        //            TOUTestParameters = new List<Types.TOU.TestParameters>(),
        //            CommTestParameters = profile.ParametersComm,
        //            IsHeightMeasureEnabled = profile.IsHeightMeasureEnabled,
        //            ParametersClamp = profile.ParametersClamp,
        //            Height = profile.Height,
        //            Temperature = profile.Temperature
        //        };

        //        foreach (var baseTestParametersAndNormativese in profile.TestParametersAndNormatives)
        //        {
        //            switch (baseTestParametersAndNormativese)
        //            {
        //                case Types.Gate.TestParameters gate:
        //                    profileItem.GateTestParameters.Add(gate);
        //                    break;
        //                case Types.SL.TestParameters sl:
        //                    profileItem.VTMTestParameters.Add(sl);
        //                    break;
        //                case Types.BVT.TestParameters bvt:
        //                    profileItem.BVTTestParameters.Add(bvt);
        //                    break;
        //                case Types.dVdt.TestParameters dvdt:
        //                    profileItem.DvDTestParameterses.Add(dvdt);
        //                    break;
        //                case Types.ATU.TestParameters atu:
        //                    profileItem.ATUTestParameters.Add(atu);
        //                    break;
        //                case Types.QrrTq.TestParameters qrrTq:
        //                    profileItem.QrrTqTestParameters.Add(qrrTq);
        //                    break;
        //                case Types.RAC.TestParameters rac:
        //                    profileItem.RACTestParameters.Add(rac);
        //                    break;
        //                case Types.TOU.TestParameters tou:
        //                    profileItem.TOUTestParameters.Add(tou);
        //                    break;
        //            }
        //        }

        //        ProfileItems.Add(profileItem);
        //    }

        //    return ProfileItems;
        //}


        public List<ProfileForSqlSelect> LoadProfilesByMME(string mmeCode)
        {
            /*
            работает не корректно, дурной синтасис группировки, при исполнении запроса ошибок нет, но возвращаемый результат не предсказуем
            var profilesSelect = @"SELECT P.PROF_ID, P.PROF_NAME, P.PROF_GUID, Max(P.PROF_TS), P.PROF_VERS FROM PROFILES P 
                                   JOIN MME_CODES_TO_PROFILES MCP ON MCP.PROFILE_ID = P.PROF_ID
                                   JOIN MME_CODES MC ON MC.MME_CODE_ID = MCP.MME_CODE_ID
                                   WHERE MC.MME_CODE = @MME_CODE
                                   GROUP BY P.PROF_NAME";

            */
            //построение списка профилей последних редакций. последние редакции профилей имеют максимальное значение идентификаторов PROF_ID
            string profilesSelect = @"SELECT P.PROF_ID, P.PROF_NAME, P.PROF_GUID, P.PROF_VERS, DATETIME(P.PROF_TS)
                                          FROM (
                                                 SELECT MAX(PR.PROF_ID) AS MAX_PROF_ID
                                                 FROM PROFILES PR
                                                 GROUP BY PR.PROF_NAME
	                                           ) PP
                                           INNER JOIN PROFILES P ON (P.PROF_ID=PP.MAX_PROF_ID)
                                           INNER JOIN MME_CODES_TO_PROFILES MCP ON (MCP.PROFILE_ID=P.PROF_ID)
                                           INNER JOIN MME_CODES MC ON (
                                                                       (MC.MME_CODE_ID=MCP.MME_CODE_ID) AND
	                                                                   (MC.MME_CODE=@MME_CODE)
                                                                      )";

            var res = new List<ProfileForSqlSelect>();

            using (var condCmd = _connection.CreateCommand())
            {
                condCmd.CommandText = profilesSelect;
                condCmd.Parameters.Add("@MME_CODE", DbType.String);
                condCmd.Prepare();
                condCmd.Parameters["@MME_CODE"].Value = mmeCode;

                using (var reader = condCmd.ExecuteReader())
                    while (reader.Read())
                        res.Add(new ProfileForSqlSelect( Convert.ToInt32((long)reader[0]), (string)reader[1], (Guid)reader[2], Convert.ToInt32((long)reader[3]), DateTime.Parse((string)reader[4])));
            }

            return res;
        }

        public List<ProfileItem> GetProfileItems(string mmeCode)
        {
            try
            {
                var profilesList = new List<ProfileItem>();

                var profilesDict = LoadProfilesByMME(mmeCode);

                foreach (var prof in profilesDict)
                {
                    var profile = new Profile(prof);

                    var testTypes = new Dictionary<long, long>(5);

                    using (var condCmd = new SQLiteCommand("SELECT ID, TEST_TYPE_ID FROM PROF_TEST_TYPE WHERE PROF_ID=@PROF_ID", _connection))
                    {
                        condCmd.Parameters.Add("@PROF_ID", DbType.Int64);
                        condCmd.Prepare();
                        condCmd.Parameters["@PROF_ID"].Value = prof.Id;

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
                        ProfileId = prof.Id,
                        ProfileName = profile.Name,
                        ProfileKey = profile.Key,
                        ProfileTS = profile.Timestamp,
                        Version = prof.Version,
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

                case (int)TestParametersType.ATU:
                    var atuParams = FillAtuConditions(testTypeId);
                    profile.TestParametersAndNormatives.Add(atuParams);
                    break;

                case (int)TestParametersType.QrrTq:
                    var qrrTqParams = FillQrrTqConditions(testTypeId);
                    profile.TestParametersAndNormatives.Add(qrrTqParams);
                    break;

                case (int)TestParametersType.RAC:
                    var racParams = FillRacConditions(testTypeId);
                    profile.TestParametersAndNormatives.Add(racParams);
                    break;

                case (int)TestParametersType.TOU:
                    var touParams = FillTOUConditions(testTypeId);
                    profile.TestParametersAndNormatives.Add(touParams);
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

            var testParams = new Types.ATU.TestParameters();
            testParams.IsEnabled = true;
            testParams.TestTypeId = testTypeId;

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

            var testParams = new Types.QrrTq.TestParameters();
            testParams.IsEnabled = true;
            testParams.TestTypeId = testTypeId;

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
                        testParams.DirectCurrent = ushort.Parse(result.Value.ToString());
                        break;

                    case "QrrTq_DCPulseWidth":
                        testParams.DCPulseWidth = ushort.Parse(result.Value.ToString());
                        break;

                    case "QrrTq_DCRiseRate":
                        testParams.DCRiseRate = float.Parse(result.Value.ToString());
                        break;

                    case "QrrTq_DCFallRate":
                        testParams.DCFallRate = (Types.QrrTq.TDcFallRate)Enum.Parse(typeof(Types.QrrTq.TDcFallRate), result.Value.ToString());
                        break;

                    case "QrrTq_OffStateVoltage":
                        testParams.OffStateVoltage = ushort.Parse(result.Value.ToString());
                        break;

                    case "QrrTq_OsvRate":
                        testParams.OsvRate = (Types.QrrTq.TOsvRate)Enum.Parse(typeof(Types.QrrTq.TOsvRate), result.Value.ToString());
                        break;
                }
            }

            return testParams;
        }

        private Types.RAC.TestParameters FillRacConditions(long testTypeId)
        {
            var results = new Dictionary<string, object>(2);

            var testParams = new Types.RAC.TestParameters();
            testParams.IsEnabled = true;
            testParams.TestTypeId = testTypeId;

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
                        testParams.ResVoltage = ushort.Parse(result.Value.ToString());
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

        public ProfileItem GetProfileByProfName(string profName, string mmmeCode, ref bool Found)
        {
            //чтение профиля с принятыми profName и mmmeCode самой последней редакции
            try
            {
                ProfileItem Result = null;

                string profileSelect = @"SELECT P.PROF_ID, P.PROF_NAME, P.PROF_GUID, P.PROF_VERS, P.PROF_TS
                                         FROM (
                                                SELECT MAX(PR.PROF_ID) AS MAX_PROF_ID
                                                FROM PROFILES PR
                                                WHERE (PR.PROF_NAME=@ProfName)
                                              ) PP
                                          INNER JOIN PROFILES P ON (P.PROF_ID=PP.MAX_PROF_ID)
                                          INNER JOIN MME_CODES_TO_PROFILES MCP ON (MCP.PROFILE_ID=P.PROF_ID)
                                          INNER JOIN MME_CODES MC ON (
                                                                      (MC.MME_CODE_ID=MCP.MME_CODE_ID) AND
                                                                      (MC.MME_CODE=@MmmeCode)
                                                                     )";

                ProfileForSqlSelect profileDict = null;

                using (var condCmd = _connection.CreateCommand())
                {
                    condCmd.CommandText = profileSelect;
                    condCmd.Parameters.Add("@ProfName", DbType.String);
                    condCmd.Parameters.Add("@MmmeCode", DbType.String);
                    condCmd.Prepare();
                    condCmd.Parameters["@ProfName"].Value = profName;
                    condCmd.Parameters["@MmmeCode"].Value = mmmeCode;

                    using (var reader = condCmd.ExecuteReader())
                        while (reader.Read())
                            profileDict = new ProfileForSqlSelect(Convert.ToInt32((long)reader[0]), (string)reader[1], (Guid)reader[2], Convert.ToInt32((long)reader[3]), DateTime.Parse((string)reader[4]));
                }

                if (profileDict == null)
                {
                    //профиль с именем profName, связанный с кодом MME mmmeCode в базе данных не найден
                    Found = false;
                    return Result;
                }
                else
                {
                    Found = true;

                    var testTypes = new Dictionary<long, long>(5);

                    using (var condCmd = new SQLiteCommand("SELECT ID, TEST_TYPE_ID FROM PROF_TEST_TYPE WHERE PROF_ID=@PROF_ID", _connection))
                    {
                        condCmd.Parameters.Add("@PROF_ID", DbType.Int64);
                        condCmd.Prepare();
                        condCmd.Parameters["@PROF_ID"].Value = profileDict.Id;

                        using (var reader = condCmd.ExecuteReader())
                        {
                            while (reader.Read())
                                testTypes.Add((long)reader[0], (long)reader[1]);
                        }
                    }

                    Profile profile = new Profile(profileDict);

                    foreach (var testType in testTypes)
                        FillParameters(profile, testType.Key, testType.Value);

                    Result = new ProfileItem
                    {
                        ProfileId = profileDict.Id,
                        ProfileName = profileDict.Name,
                        ProfileKey = profileDict.Key,
                        ProfileTS = profileDict.TS,
                        Version = profileDict.Version,
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
                            Result.GateTestParameters.Add(gate);

                        var sl = baseTestParametersAndNormativese as Types.SL.TestParameters;
                        if (sl != null)
                            Result.VTMTestParameters.Add(sl);

                        var bvt = baseTestParametersAndNormativese as Types.BVT.TestParameters;
                        if (bvt != null)
                            Result.BVTTestParameters.Add(bvt);

                        var dvdt = baseTestParametersAndNormativese as Types.dVdt.TestParameters;
                        if (dvdt != null)
                            Result.DvDTestParameterses.Add(dvdt);

                        var atu = baseTestParametersAndNormativese as Types.ATU.TestParameters;
                        if (atu != null)
                            Result.ATUTestParameters.Add(atu);

                        var qrrTq = baseTestParametersAndNormativese as Types.QrrTq.TestParameters;
                        if (qrrTq != null)
                            Result.QrrTqTestParameters.Add(qrrTq);

                        var rac = baseTestParametersAndNormativese as Types.RAC.TestParameters;
                        if (rac != null)
                            Result.RACTestParameters.Add(rac);

                        var tou = baseTestParametersAndNormativese as Types.TOU.TestParameters;
                        if (tou != null)
                            Result.TOUTestParameters.Add(tou);
                    }
                }

                return Result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
