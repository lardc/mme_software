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
using System.Collections.ObjectModel;
using System.Reflection;

namespace SCME.InterfaceImplementations.Common
{
    public abstract class LoadProfilesService<TDbCommand, TDbConnection> : ILoadProfilesService  where TDbCommand : DbCommand where TDbConnection : DbConnection
    {
        protected virtual string _ChildsSelectString => @"SELECT [PROF_ID], [PROF_NAME], [PROF_GUID], [PROF_VERS], [PROF_TS] FROM [PROFILES] WHERE [PROF_NAME] = @PROF_NAME AND PROF_ID <> @PROF_ID_EXCLUDE ORDER BY [PROF_TS] DESC";
        protected virtual string _TestTypeSelectString => @"SELECT [PTT_ID], [TEST_TYPE_ID] FROM [PROF_TEST_TYPE] WHERE [PROF_ID] = @PROF_ID";
        //protected virtual string _AllTopProfilesSelectString =>
        //    @"SELECT * FROM PROFILES WHERE PROF_ID IN 
        //        (SELECT PROFILE_ID FROM MME_CODES_TO_PROFILES )";
        protected virtual string _ProfilesByMMESelectString =>
            @"SELECT PROF_ID, PROF_NAME, PROF_GUID, PROF_VERS, PROF_TS FROM PROFILES WHERE PROF_ID IN
	            (SELECT PROFILE_ID FROM MME_CODES_TO_PROFILES WHERE MME_CODE_ID IN
		            (SELECT MME_CODE_ID FROM MME_CODES WHERE MME_CODE = @MME_CODE)) ORDER BY PROF_TS DESC";

        //protected virtual string _ProfilesByNameByMMESelectString =>
        //    @"SELECT PROF_ID, PROF_NAME, PROF_GUID, PROF_VERS, PROF_TS FROM PROFILES WHERE  PROF_NAME LIKE '%' + @PROF_NAME + '%' AND PROF_ID IN
        //        (SELECT PROFILE_ID FROM MME_CODES_TO_PROFILES WHERE MME_CODE_ID IN
        //            (SELECT MME_CODE_ID FROM MME_CODES WHERE MME_CODE = @MME_CODE))";

        protected virtual string _OrderSelectString => @"SELECT [ORD] FROM [PROF_TEST_TYPE] WHERE [PTT_ID] = @TEST_TYPE_ID";
        protected virtual string _ConditionSelectString => @"SELECT C.[COND_NAME], PC.[VALUE] FROM [PROF_COND] PC LEFT JOIN [CONDITIONS] C on C.[COND_ID] = PC.[COND_ID] WHERE PC.[PROF_TESTTYPE_ID] = @TEST_TYPE_ID";
        protected virtual string _ParamSelectString => "SELECT P.[PARAM_NAME], PP.[MIN_VAL], PP.[MAX_VAL] FROM [PROF_PARAM] PP LEFT JOIN [PARAMS] P on P.[PARAM_ID] = PP.[PARAM_ID] WHERE PP.[PROF_TESTTYPE_ID] = @TEST_TYPE_ID";
        protected virtual string _ProfileByKeySelectString => @"SELECT PROF_ID, PROF_NAME, PROF_GUID, PROF_VERS, PROF_TS FROM PROFILES WHERE PROF_GUID = @PROF_GUID";
        protected virtual string _AllMMECodesSelectString => @"SELECT MME_CODE, MME_CODE_ID FROM MME_CODES";

        private TDbConnection _Connection;
        private TDbCommand _ChildsSelect;
        private TDbCommand _TestTypeSelect;
        //private TDbCommand _AllTopProfilesSelect;
        private TDbCommand _ProfilesByMMESelect;
        //private TDbCommand _ProfilesByNameByMMESelect;
        private TDbCommand _OrderSelect;
        private TDbCommand _ConditionSelect;
        private TDbCommand _ParamSelect;

        private TDbCommand _ProfileByKeySelect;
        private TDbCommand _AllMMECodesSelect;

        ConstructorInfo _CommandConstructor = typeof(TDbCommand).GetConstructor(new Type[] { typeof(string), typeof(TDbConnection) });

        public LoadProfilesService(TDbConnection connection)
        {
            _Connection = connection;
            if (_Connection.State != ConnectionState.Open)
                _Connection.Open();

            PrepareQueries();
        }

        private TDbCommand CreateCommand(string commandString, List<TDbCommandParametr> parameters)
        {
            var command = _CommandConstructor.Invoke(new object[] { commandString, _Connection }) as TDbCommand;
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

        public void PrepareQueries()
        {
            _ChildsSelect = CreateCommand(_ChildsSelectString, new List<TDbCommandParametr>()
            {
                new TDbCommandParametr("@PROF_NAME", DbType.String, 32 ),
                new TDbCommandParametr("@PROF_ID_EXCLUDE", DbType.String, 32 )
            });
            _TestTypeSelect = CreateCommand(_TestTypeSelectString, new List<TDbCommandParametr>()
            {
                new TDbCommandParametr("@PROF_ID", DbType.Int32)
            });

            //_AllTopProfilesSelect = CreateCommand(_AllTopProfilesSelectString, new List<TDbCommandParametr>());
            _ProfilesByMMESelect = CreateCommand(_ProfilesByMMESelectString, new List<TDbCommandParametr>()
            {
                new TDbCommandParametr("@MME_CODE", DbType.String, 64 )
            });
            //_ProfilesByNameByMMESelect = CreateCommand(_ProfilesByNameByMMESelectString, new List<TDbCommandParametr>()
            //{
            //    new TDbCommandParametr("@PROF_NAME", DbType.String, 32 ),
            //    new TDbCommandParametr("@MME_CODE", DbType.String, 64 )
            //});

            _OrderSelect = CreateCommand(_OrderSelectString, new List<TDbCommandParametr>()
            {
                new TDbCommandParametr("@TEST_TYPE_ID", DbType.Int32 )
            });
            _ConditionSelect = CreateCommand(_ConditionSelectString, new List<TDbCommandParametr>()
            {
                new TDbCommandParametr("@TEST_TYPE_ID",DbType.Int32)
            });
            _ParamSelect = CreateCommand(_ParamSelectString, new List<TDbCommandParametr>()
            {
                new TDbCommandParametr("@TEST_TYPE_ID", DbType.Int32)
            });

            _ProfileByKeySelect = CreateCommand(_ProfileByKeySelectString, new List<TDbCommandParametr>()
            {
                new TDbCommandParametr("@PROF_GUID", DbType.Guid)
            });

            _AllMMECodesSelect = CreateCommand(_AllMMECodesSelectString, new List<TDbCommandParametr>());
        }


        public Dictionary<string, int> GetMmeCodes()
        {
            Dictionary<string, int> MMECodes = new Dictionary<string, int>();

            using (var reader = _AllMMECodesSelect.ExecuteReader())
                while (reader.Read())
                    MMECodes.Add(reader.GetString(0), reader.GetInt32(1));

            return MMECodes;
        }

        public List<MyProfile> GetProfilesSuperficially(string mmeCode, string name = null)
        {
            DbCommand profileSelect;
            List<MyProfile> profiles = new List<MyProfile>();

            if (string.IsNullOrEmpty(mmeCode))
                throw new ArgumentNullException(nameof(mmeCode));
            //profileSelect = _AllTopProfilesSelect;
            else
            {
                if (string.IsNullOrEmpty(name))
                {
                    profileSelect = _ProfilesByMMESelect;
                    profileSelect.Parameters["@MME_CODE"].Value = mmeCode;
                }
                else
                {
                    throw new NotImplementedException("_ProfilesByNameByMMESelect");
                    //profileSelect = _ProfilesByNameByMMESelect;
                    //profileSelect.Parameters["@PROF_NAME"].Value = name;
                    //profileSelect.Parameters["@MME_CODE"].Value = mmeCode;
                }
            }

            using (var reader = profileSelect.ExecuteReader())
                while (reader.Read())
                    profiles.Add(new MyProfile(reader.GetInt32(0), reader.GetString(1), reader.GetGuid(2), reader.GetInt32(3), reader.GetDateTime(4)));

            return profiles;
        }

        public MyProfile GetProfileByKey(Guid key)
        {
            _ProfileByKeySelect.Parameters["@PROF_GUID"].Value = key;
            using (var reader = _ProfileByKeySelect.ExecuteReader())
                while (reader.Read())
                    return new MyProfile(reader.GetInt32(0), reader.GetString(1), reader.GetGuid(2), reader.GetInt32(3), reader.GetDateTime(4));
            throw new Exception($"GetProfileByKey, could not find item by key {key}");
        }

        public List<MyProfile> GetProfileChildSuperficially(MyProfile profile)
        {
            List<MyProfile> profiles = new List<MyProfile>();

            _ChildsSelect.Parameters["@PROF_NAME"].Value = profile.Name;
            _ChildsSelect.Parameters["@PROF_ID_EXCLUDE"].Value = profile.Id;
            using (var reader = _ChildsSelect.ExecuteReader())
                while (reader.Read())
                    profiles.Add(new MyProfile(reader.GetInt32(0), reader.GetString(1), reader.GetGuid(2), reader.GetInt32(3), reader.GetDateTime(4)));

            return profiles;
        }

        public ProfileDeepData LoadProfileDeepData(MyProfile profile)
        {
            ProfileDeepData profileDeepData = new ProfileDeepData();
            var testTypes = new Dictionary<long, long>();

            _TestTypeSelect.Parameters["@PROF_ID"].Value = profile.Id;

            using (var reader = _TestTypeSelect.ExecuteReader())
                while (reader.Read())
                    testTypes.Add(reader.GetInt32(0), reader.GetInt32(1));
            
            foreach (var testType in testTypes)
                FillParameters(profileDeepData, testType.Key, testType.Value);

            return profileDeepData;
        }

        #region Fill

        private void FillParameters(ProfileDeepData data, long testTypeId, long testParametersType)
        {
            switch ((TestParametersType)testParametersType)
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
                    data.TestParametersAndNormatives.Add(atuParams);
                    break;
                case TestParametersType.QrrTq:
                    var qrrTqParams = FillQrrTqConditions(testTypeId);
                    data.TestParametersAndNormatives.Add(qrrTqParams);
                    break;
                case TestParametersType.RAC:
                    var racParams = FillRACConditions(testTypeId);
                    data.TestParametersAndNormatives.Add(racParams);
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

        private void FillComutationConditions(ProfileDeepData data, long testTypeId)
        {
            var results = new Dictionary<string, object>(2);

            FillConditionsResults(testTypeId, results);

            foreach (var result in results)
                data.ComutationType = (ModuleCommutationType)Enum.Parse(typeof(ModuleCommutationType), result.Value.ToString());
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
            _ConditionSelect.Parameters["@TEST_TYPE_ID"].Value = testTypeId;

            using (var reader = _ConditionSelect.ExecuteReader())
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
