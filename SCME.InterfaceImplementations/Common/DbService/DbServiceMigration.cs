using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Resources;
using System.ServiceModel.Channels;
using SCME.Types;
using SCME.Types.BaseTestParams;

namespace SCME.InterfaceImplementations.Common.DbService
{
    public partial class DbService<TDbCommand, TDbConnection>
    {
        private readonly (string name, string localName, bool isTech)[] _conditionsList =
        {
            ("Gate_En", "Gate_En", true),
            ("Gate_EnableCurrent", "Gate_EnableCurrent", false),
            ("Gate_IHEn", "Gate_IHEn", true),
            ("Gate_ILEn", "Gate_ILEn", true),
            ("Gate_EnableIHStrike", "Gate_EnableIHStrike", true),
            ("SL_En", "SL_En", true),
            ("SL_Type", "SL_TestType", false),
            ("SL_ITM", "SL_ITM", false),
            ("SL_Time", "SL_Time", false),
            ("SL_OpenEn", "SL_OpenEn", true),
            ("SL_OpenI", "SL_OpenI", false),
            ("SL_Factor", "SL_Factor", true),
            ("SL_TimeEx", "SL_TimeEx", true),
            ("SL_FS", "SL_FullScale", true),
            ("SL_N", "SL_N", true),
            ("SL_HeatEn", "SL_HeatEn", false),
            ("SL_RampHeatCurrent", "SL_RampHeatCurrent", false),
            ("SL_RampHeatTime", "SL_RampHeatTime", false),

            ("BVT_En", "BVT_En", true),
            ("BVT_Type", "BVT_Type", false),
            ("BVT_I", "BVT_I", false),
            ("BVT_VD", "BVT_VD", false),
            ("BVT_VR", "BVT_VR", false),
            ("BVT_RumpUp", "BVT_RumpUp", true),
            ("BVT_StartV", "BVT_StartV", true),
            ("BVT_F", "BVT_F", false),
            ("BVT_FD", "BVT_FD", false),
            ("BVT_Mode", "BVT_Mode", false),
            (@"BVT_PlateTime", @"BVT_PlateTime", true),
            ("BVT_UseUdsmUrsm", "BVT_UseUdsmUrsm", true),
            ("BVT_PulseFrequency", "BVT_PulseFrequency", true),

            ("BVT_UdsmUrsm_Type", "BVT_UdsmUrsm_Type", false),
            ("BVT_UdsmUrsm_I", "BVT_UdsmUrsm_I", false),
            ("BVT_UdsmUrsm_VD", "BVT_UdsmUrsm_VD", false),
            ("BVT_UdsmUrsm_VR", "BVT_UdsmUrsm_VR", false),
            ("BVT_UdsmUrsm_RumpUp", "BVT_UdsmUrsm_RumpUp", true),
            ("BVT_UdsmUrsm_StartV", "BVT_UdsmUrsm_StartV", true),
            ("BVT_UdsmUrsm_F", "BVT_UdsmUrsm_F", false),
            ("BVT_UdsmUrsm_FD", "BVT_UdsmUrsm_FD", false),
            ("BVT_UdsmUrsm_PlateTime", "BVT_UdsmUrsm_PlateTime", true),
            ("BVT_UdsmUrsm_PulseFrequency", "BVT_UdsmUrsm_PulseFrequency", true),

            ("DutPackageType", "DutPackageType", true),
            ("COMM_Type", "COMM_Type", true),
            ("CLAMP_Type", "CLAMP_Type", true),
            ("CLAMP_Force", "CLAMP_Force", true),
            ("CLAMP_HeightMeasure", "CLAMP_HeightMeasure", true),
            ("CLAMP_HeightValue", "CLAMP_HeightValue", true),
            ("CLAMP_Temperature", "CLAMP_Temperature", true),
            ("DVDT_En", "DVDT_En", true),
            ("DVDT_Mode", "DVDT_Mode", true),
            ("DVDT_Voltage", "DVDT_Voltage", true),
            ("DVDT_VoltageRate", "DVDT_VoltageRate", true),
            ("DVDT_ConfirmationCount", "DVDT_ConfirmationCount", true),
            ("DVDT_VoltageRateLimit", "DVDT_VoltageRateLimit", true),
            ("DVDT_VoltageRateOffSet", "DVDT_VoltageRateOffSet", true),

            ("ATU_En", "ATU_En", true),
            ("ATU_PrePulseValue", "ATU_PrePulseValue", true),
            ("ATU_PowerValue", "ATU_PowerValue", true),
            ("QrrTq_En", "QrrTq_En", true),
            ("QrrTq_Mode", "QrrTq_Mode", true),
            ("QrrTq_TrrMeasureBy9050Method", "QrrTq_TrrMeasureBy9050Method", true),
            ("QrrTq_DirectCurrent", "QrrTq_DirectCurrent", true),
            ("QrrTq_DCPulseWidth", "QrrTq_DCPulseWidth", true),
            ("QrrTq_DCRiseRate", "QrrTq_DCRiseRate", true),
            ("QrrTq_DCFallRate", "QrrTq_DCFallRate", true),
            ("QrrTq_OffStateVoltage", "QrrTq_OffStateVoltage", true),
            ("QrrTq_OsvRate", "QrrTq_OsvRate", true),
            ("RAC_En", "RAC_En", true),
            ("RAC_ResVoltage", "RAC_ResVoltage", true),
            ("TOU_En", "TOU_En", true),
            ("TOU_ITM", "TOU_ITM", true),




            ("Im_Position", "Im_Position", true),
            ("Im_TypeManagement", "Im_TypeManagement", true),
            ("Im_ControlVoltage", "Im_ControlVoltage", true),
            ("Im_ControlCurrent", "Im_ControlCurrent", true),
            ("Im_AuxiliaryVoltagePowerSupply1", "Im_AuxiliaryVoltagePowerSupply1", true),
            ("Im_AuxiliaryVoltagePowerSupply2", "Im_AuxiliaryVoltagePowerSupply2", true),
            ("Im_AppPolSwitchingVoltage", "Im_ApplicationPolarityConstantSwitchingVoltage", true),
            ("Im_PolDCSwVoltageApplication", "Im_PolDCSwVoltageApplication", true),
            ("Im_SwitchedAmperage", "Im_SwitchedAmperage", true),
            ("Im_SwitchedVoltage", "Im_SwitchedVoltage", true),
            ("Im_SwitchingCurrentPulseShape", "Im_SwitchingCurrentPulseShape", true),
            ("Im_SwitchingCurrentPulseDuration", "Im_SwitchingCurrentPulseDuration", true),
            ("Im_OpenState", "Im_OpenState", true),
            ("Im_ControlVoltageMax", "Im_ControlVoltageMax", true),
            ("Im_ControlCurrentMax", "Im_ControlCurrentMax", true),

            //("Im_ProhibitionVoltageMinimum", "Im_ProhibitionVoltageMinimum", true),
            //("Im_ProhibitionVoltageMaximum", "Im_ProhibitionVoltageMaximum", true),

            //("Im_OutputResidualVoltageMinimum", "Im_OutputResidualVoltageMinimum", true),
            //("Im_OutputResidualVoltageMaximum", "Im_OutputResidualVoltageMaximum", true),

            //("Im_LeakageCurrentMinimum", "Im_LeakageCurrentMinimum", true),
            //("Im_LeakageCurrentMaximum", "Im_LeakageCurrentMaximum", true),

            //("Im_InputCurrentMinimum", "Im_InputCurrentMinimum", true),
            //("Im_InputCurrentMaximum", "Im_InputCurrentMaximum", true),
            //("Im_InputVoltageMinimum", "Im_InputVoltageMinimum", true),
            //("Im_InputVoltageMaximum", "Im_InputVoltageMaximum", true)

        };


        private readonly (string name, string localName, bool isTech)[] _paramsList =
        {
            ("K", "K", true),
            ("RG", "RG, Ohm", false),
            ("IGT", "IGT, mA", false),
            ("VGT", "VGT, V", false),
            ("IH", "IH, mA", false),
            ("IL", "IL, mA", false),
            ("VTM", "VTM, V", false),
            ("VDRM", "VDRM, V", false),
            ("VRRM", "VRRM, V", false),
            ("IDRM", "IDRM, mA", false),
            ("IRRM", "IRRM, mA", false),
            ("VDSM", "VDRM, V", false),
            ("VRSM", "VRRM, V", false),
            ("IDSM", "IDRM, mA", false),
            ("IRSM", "IRRM, mA", false),

            //("UdsmUrsm_IDRM", "UdsmUrsm_IDRM, A", false),
            //("UdsmUrsm_IRRM", "UdsmUrsm_IRRM, A", false),

            ("IsHeightOk", "IsHeightOk", false),
            ("UBR", "UBR, V", false),
            ("UPRSM", "UPRSM, V", false),
            ("IPRSM", "IPRSM, A", false),
            ("PRSM", "PRSM, kW", false),
            ("IDC", "IDC, A", false),
            ("QRR", "QRR, uC", false),
            ("IRR", "IRR, A", false),
            ("TRR", "TRR, us", false),
            ("DCFactFallRate", "dIDC/dt, A/us", false),
            ("TQ", "TQ, us", false),
            ("ResultR", "ResultR, MOhm", false),
            ("TOU_TGD", "TOU_TGD, us", false),
            ("TOU_TGT", "TOU_TGT, us", false),
            ("BVT_VDSM", "BVT_VDSM, V", false),
            ("BVT_VRSM", "BVT_VRSM, V", false),
            ("BVT_IDSM", "BVT_IDSM, A", false),
            ("BVT_IRSM", "BVT_IRSM, A", false),

            ("DVDT_OK", "DVDT_OK", false),

            ("InputAmperage", "Im_InputAmperage", false),
            ("InputVoltage", "Im_InputVoltage", false),
            ("LeakageCurrent", "Im_LeakageCurrent", false),
            ("ResidualVoltage", "Im_ResidualVoltage", false),
            ("ProhVoltage", "Im_ProhibitionVoltage", false),
            ("AuxCurrentPower1", "AuxCurrentPower1", true),
            ("AuxCurrentPower2", "AuxCurrentPower2", true),
            ("OpenResistance", "OpenResistance", true),
            
        };

        private readonly (string name, string localName, int code)[] _errorsList =
        {
            ("ERR_KELVIN", "Error connection", 11),
            ("ERR_RG", "RG out of range", 12),
            ("ERR_IGT", "IGT out of range", 13),
            ("ERR_VGT", "VGT out of range", 14),
            ("ERR_IH", "IH out of range", 15),
            ("ERR_IL", "IL out of range", 16),
            ("ERR_IHL_PROBLEM", "IH-IL problem", 17),
            ("ERR_GT_PROBLEM", "IGT-VGT problem", 18),
            ("ERR_VTM", "VTM out of range", 20),
            ("ERR_ITM_PROBLEM", "ITM problem", 21),
            ("ERR_VTM_PROBLEM", "VTM problem", 22),
            ("ERR_VDRM", "VDRM out of range", 31),
            ("ERR_VRRM", "VRRM out of range", 32),
            ("ERR_IDRM", "IDRM out of range", 33),
            ("ERR_IRRM", "IRRM out of range", 34),
            ("ERR_RM_OVERLOAD", "RM overload", 35),
            ("ERR_UBR", "UBR out of range", 36),
            ("ERR_UPRSM", "UPRSM out of range", 37),
            ("ERR_IPRSM", "IPRSM out of range", 38),
            ("ERR_PRSM", "PRSM out of range", 39),
            ("ERR_NO_CTRL_NO_PWR", "Lack of control current and power current", 40),
            ("ERR_NO_PWR", "Lack of power current", 41),
            ("ERR_SHORT", "Short circuit on output", 42),
            ("ERR_NO_POT_SIGNAL", "There is no signal from the potential line", 43),
            ("ERR_OVERFLOW90", "90% Level Counter Overflow", 44),
            ("ERR_OVERFLOW10", "Overflow level counter 10%", 45),
            ("ERR_IDSM", "IDSM out of range", 46),
            ("ERR_IRSM", "IRSM out of range", 47),
        };

        private readonly (int id, string name)[] _testTypes =
        {
            (1, "Gate"),
            (2, "SL"),
            (3, "BVT"),
            (4, "Commutation"),
            (5, "Clamping"),
            (6, "Dvdt"),
            (7, "SCTU"),
            (8, "ATU"),
            (9, "QrrTq"),
            (10, "RAC"),
            (13, "TOU"),
            ((int)TestParametersType.InputOptions, "InputOptions"),
            ((int)TestParametersType.OutputLeakageCurrent, "LeakageCurrent"),
            ((int)TestParametersType.OutputResidualVoltage, "ResidualVoltage"),
            ((int)TestParametersType.ProhibitionVoltage, "ProhibitionVoltage"),
                ((int)TestParametersType.AuxiliaryPower, "AuxiliaryPower")
        };

        private DbCommand CreateCountCmdCount(string tableName, string columnName)
        {
            return CreateCommand($@"SELECT COUNT (*) FROM {tableName} WHERE {columnName} = @WHERE_PARAMETER", new List<DbCommandParameter>()
            {
                new DbCommandParameter("@WHERE_PARAMETER", DbType.String, 32)
            });
        }

        protected virtual string DatabaseFieldTestTypeName => "TEST_TYPE_NAME";

        public bool Migrate()
        {
            var isMigrate = false;
            using (_dbTransaction = Connection.BeginTransaction())
            {
                _insertCondition.Transaction = _dbTransaction;
                _insertTestType.Transaction = _dbTransaction;
                _insertParameter.Transaction = _dbTransaction;
                _insertError.Transaction = _dbTransaction;

                _selectAllTopProfile.Transaction = _dbTransaction;

                _insertMmeCode.Transaction = _dbTransaction;

                _checkMmeCodeIsActive.Transaction = _dbTransaction;
                _checkCondition.Transaction = _dbTransaction;
                _checkParameter.Transaction = _dbTransaction;
                _checkError.Transaction = _dbTransaction;
                _checkTestType.Transaction = _dbTransaction;

                try
                {
                    foreach (var (name, localName, isTech) in _conditionsList)
                    {
                        _checkCondition.Parameters["@WHERE_PARAMETER"].Value = name;
                        if (Convert.ToInt32(_checkCondition.ExecuteScalar()) > 0)
                            continue;
                        _insertCondition.Parameters["@COND_NAME"].Value = name;
                        _insertCondition.Parameters["@COND_NAME_LOCAL"].Value = localName;
                        _insertCondition.Parameters["@COND_IS_TECH"].Value = isTech;
                        _insertCondition.ExecuteNonQuery();
                        isMigrate = true;
                    }

                    foreach (var (name, localName, isTech) in _paramsList)
                    {
                        _checkParameter.Parameters["@WHERE_PARAMETER"].Value = name;
                        if (Convert.ToInt32(_checkParameter.ExecuteScalar()) > 0)
                            continue;
                        _insertParameter.Parameters["@PARAM_NAME"].Value = name;
                        _insertParameter.Parameters["@PARAM_NAME_LOCAL"].Value = localName;
                        _insertParameter.Parameters["@PARAM_IS_HIDE"].Value = isTech;
                        _insertParameter.ExecuteNonQuery();
                        isMigrate = true;
                    }

                    foreach (var (name, localName, code) in _errorsList)
                    {
                        _checkError.Parameters["@WHERE_PARAMETER"].Value = name;
                        if (Convert.ToInt32(_checkError.ExecuteScalar()) > 0)
                            continue;
                        _insertError.Parameters["@ERR_NAME"].Value = name;
                        _insertError.Parameters["@ERR_NAME_LOCAL"].Value = localName;
                        _insertError.Parameters["@ERR_CODE"].Value = code;
                        _insertError.ExecuteNonQuery();
                        isMigrate = true;
                    }

                    foreach (var (id, name) in _testTypes)
                    {
                        _checkTestType.Parameters["@WHERE_PARAMETER"].Value = name;
                        if (Convert.ToInt32(_checkTestType.ExecuteScalar()) > 0)
                            continue;
                        _insertTestType.Parameters["@ID"].Value = id;
                        _insertTestType.Parameters["@NAME"].Value = name;
                        _insertTestType.ExecuteNonQuery();
                        isMigrate = true;
                    }

                    if (Convert.ToInt32(_checkMmeCodeIsActive.ExecuteScalar()) == 0)
                    {
                        InsertMmeCode(Constants.MME_CODE_IS_ACTIVE_NAME);

                        foreach (var i in GetProfilesSuperficially(string.Empty))
                            InsertMmeCodeToProfile(i.Id, Constants.MME_CODE_IS_ACTIVE_NAME, _dbTransaction);
                    }

                    _dbTransaction.Commit();
                }
                catch
                {
                    _dbTransaction.Rollback();
                    throw;
                }

                if (isMigrate)
                    LoadDictionary();

            }
            BvtCheck();
            return isMigrate;
        }

        private void BvtCheck()
        {
            Dictionary<string, List<string>> newParamsOldParams = new Dictionary<string, List<string>>
            {
                {"VDSM", new List<string> { "BVT_VDSM"} },
                {"VRSM", new List<string> { "BVT_VRSM" } },
                {"IDSM", new List<string> { "BVT_IDSM", "UdsmUrsm_IDRM" } },
                {"IRSM", new List<string> { "BVT_IRSM", "UdsmUrsm_IRRM" } }
            };


            using (_dbTransaction = Connection.BeginTransaction())
            {
                TDbCommand getParamCmd = CreateCommand(@"SELECT PARAM_ID FROM PARAMS WHERE PARAM_NAME = @PARAM_NAME", new List<DbCommandParameter>()
                {
                    new DbCommandParameter("@PARAM_NAME", DbType.String, 32)
                }, _dbTransaction);

                TDbCommand profParamUpdate = CreateCommand(@"Update PROF_PARAM SET PARAM_ID = @NEW_PARAM_ID WHERE PARAM_ID = @OLD_PARAM_ID", new List<DbCommandParameter>()
                {
                    new DbCommandParameter("@OLD_PARAM_ID", DbType.Int32),
                    new DbCommandParameter("@NEW_PARAM_ID", DbType.Int32)
                }, _dbTransaction);

                TDbCommand devParamUpdate = CreateCommand(@"Update DEV_PARAM SET PARAM_ID = @NEW_PARAM_ID WHERE PARAM_ID = @OLD_PARAM_ID", new List<DbCommandParameter>()
                {
                    new DbCommandParameter("@OLD_PARAM_ID", DbType.Int32),
                    new DbCommandParameter("@NEW_PARAM_ID", DbType.Int32)
                }, _dbTransaction);

                foreach (var i in newParamsOldParams)
                {
                    getParamCmd.Parameters["@PARAM_NAME"].Value = i.Key;
                    var newParamId = Convert.ToInt32(getParamCmd.ExecuteScalar());
                    if (newParamId == 0)
                        continue;
                    profParamUpdate.Parameters["@NEW_PARAM_ID"].Value = newParamId;
                    devParamUpdate.Parameters["@NEW_PARAM_ID"].Value = newParamId;
                    foreach (var j in i.Value)
                    {
                        getParamCmd.Parameters["@PARAM_NAME"].Value = j;
                        var oldParamId = Convert.ToInt32(getParamCmd.ExecuteScalar());
                        if (oldParamId == 0)
                            continue;
                        profParamUpdate.Parameters["@OLD_PARAM_ID"].Value = oldParamId;
                        devParamUpdate.Parameters["@OLD_PARAM_ID"].Value = oldParamId;
                        profParamUpdate.ExecuteNonQuery();
                    }


                }


            }
        }
    }
}