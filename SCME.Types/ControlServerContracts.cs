﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using SCME.Types.BaseTestParams;
using SCME.Types.BVT;
using SCME.Types.Clamping;
using SCME.Types.Profiles;
using SCME.Types.SCTU;
using SCME.Types.SQL;
using TestParameters = SCME.Types.Commutation.TestParameters;

namespace SCME.Types
{
    [ServiceContract(Namespace = "http://proton-electrotex.com/SCME",
        SessionMode = SessionMode.Required,
        CallbackContract = typeof(IClientCallback))]
    public interface IExternalControl
    {
        [OperationContract]
        [FaultContract(typeof(FaultData))]
        (MyProfile profile, bool IsInMmeCode) SyncProfile(MyProfile profile);
        
        [OperationContract]
        [FaultContract(typeof(FaultData))]
        void SetUserWorkMode(UserWorkMode userWorkMode);

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        void SetSafetyMode(SafetyMode safetyMode);

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        void Check();

        [OperationContract(IsInitiating = true)]
        [FaultContract(typeof(FaultData))]
        void Subscribe();

        [OperationContract(IsTerminating = true)]
        [FaultContract(typeof(FaultData))]
        void Unsubscribe();

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        void Initialize(TypeCommon.InitParams Param);

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        void Deinitialize();

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        void GateWriteCalibrationParameters(Gate.CalibrationParameters Parameters);

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        Gate.CalibrationParameters GateReadCalibrationParameters();

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        Gate.CalibrationResultGate GatePulseCalibrationGate(ushort Current);

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        ushort GatePulseCalibrationMain(ushort Current);

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        InitializationResponse IsInitialized();

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        bool GetButtonState(ComplexButtons Button);

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        void ProvocationButtonResponse(ComplexButtons Button);

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        ComplexSafety GetSafetyType();

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        bool Start(Gate.TestParameters ParametersGate, VTM.TestParameters ParametersSL, BVT.TestParameters ParametersBVT, ATU.TestParameters ParametersATU, QrrTq.TestParameters ParametersQrrTq, IH.TestParameters ParametersIH, RCC.TestParameters ParametersRCC, Commutation.TestParameters ParametersCommutation, Clamping.TestParameters ParametersClamp, Types.TOU.TestParameters parametersTOU);

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        bool StartDynamic(TestParameters parametersCommutation, Clamping.TestParameters parametersClamp, Gate.TestParameters[] parametersGate, VTM.TestParameters[] parametersVtm, BVT.TestParameters[] parametersBvt, dVdt.TestParameters[] parametersDvDt, ATU.TestParameters[] parametersAtu, QrrTq.TestParameters[] parametersQrrTq, SctuTestParameters[] parametersSctu, Types.TOU.TestParameters[] parametersTOU);

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        void Stop();

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        void StopByButtonStop();

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        void ClearSafetyTrig();

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        void SafetySystemOn();

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        void SafetySystemOff();

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        string NotReadyDevicesToStart();

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        string NotReadyDevicesToStartDynamic();

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        void SqueezeClamping(Clamping.TestParameters ParametersClamping);

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        void UnsqueezeClamping(Clamping.TestParameters ParametersClamping);

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        void ClearFault(ComplexParts Device);

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        ushort ActivationWorkPlace(ComplexParts Device, ChannelByClumpType ChByClumpType, SctuWorkPlaceActivationStatuses ActivationStatus);

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        ushort ReadRegister(ComplexParts Device, ushort Address);

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        void WriteRegister(ComplexParts Device, ushort Address, ushort Value);

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        void CallAction(ComplexParts Device, ushort Address);

      

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        void WriteResults(ResultItem Item, List<string> Errors);

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        List<ProfileForSqlSelect> SaveProfiles(List<ProfileItem> Item);

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        bool RequestRemotePrinting(string GroupName, string CustomerName, string DeviceType, ReportSelectionPredicate Predicate);

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        bool StartHeating(int temperature);

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        void StopHeating();

        [OperationContract]
        [FaultContract(typeof(FaultData))]
        void SetPermissionToUseCanDataBus(bool PermissionToUseCanDataBus);
    }

    [ServiceContract(Namespace = "http://proton-electrotex.com/SCME")]
    public interface IClientCallback
    {
        [OperationContract(IsOneWay = true)]
        void CommonConnectionHandler(DeviceConnectionState State, string Message);

        [OperationContract(IsOneWay = true)]
        void DeviceConnectionHandler(ComplexParts Device, DeviceConnectionState State, string Message);

        [OperationContract(IsOneWay = true)]
        void GatewayButtonPressHandler(ComplexButtons Button, bool State);

        [OperationContract(IsOneWay = true)]
        void SafetyHandler(bool Alarm, ComplexSafety SafetyType, ComplexButtons Button);

        [OperationContract(IsOneWay = true)]
        void StopHandler();

        [OperationContract(IsOneWay = true)]
        void SyncDBAreProcessedHandler();

        [OperationContract(IsOneWay = true)]
        void GatewayNotificationHandler(Gateway.HWWarningReason Warning, Gateway.HWFaultReason Fault,
                                        Gateway.HWDisableReason Disable);

        [OperationContract(IsOneWay = true)]
        void TestAllHandler(DeviceState State, string Message);

        [OperationContract(IsOneWay = true)]
        void CommutationSwitchHandler(Commutation.CommutationMode SwitchState);

        [OperationContract(IsOneWay = true)]
        void CommutationNotificationHandler(Commutation.HWWarningReason Warning, Commutation.HWFaultReason Fault);

        [OperationContract(IsOneWay = true)]
        void GateAllHandler(DeviceState State);

        [OperationContract(IsOneWay = true)]
        void GateKelvinHandler(DeviceState state, bool isKelvinOk, IList<short> array, long testTypeId);

        [OperationContract(IsOneWay = true)]
        void GateResistanceHandler(DeviceState state, float resistance, long testTypeId);

        [OperationContract(IsOneWay = true)]
        void GateIgtVgtHandler(DeviceState state, float igt, float vgt, IList<short> arrayI, IList<short> arrayV, long testTypeId);

        [OperationContract(IsOneWay = true)]
        void GateIhHandler(DeviceState state, float ih, IList<short> array, long testTypeId);

        [OperationContract(IsOneWay = true)]
        void GateIlHandler(DeviceState state, float il, long testTypeId);

        [OperationContract(IsOneWay = true)]
        void GateNotificationHandler(Gate.HWProblemReason Problem, Gate.HWWarningReason Warning,
                                     Gate.HWFaultReason Fault, Gate.HWDisableReason Disable);

        [OperationContract(IsOneWay = true)]
        void SLHandler(DeviceState state, VTM.TestResults result);

        [OperationContract(IsOneWay = true)]
        void SLNotificationHandler(VTM.HWProblemReason Problem, VTM.HWWarningReason Warning, VTM.HWFaultReason Fault,
                                    VTM.HWDisableReason Disable);

        [OperationContract(IsOneWay = true)]
        void BVTAllHandler(DeviceState State);

        [OperationContract(IsOneWay = true)]
        void BVTDirectHandler(DeviceState State, BVT.TestResults Result);

        [OperationContract(IsOneWay = true)]
        void BVTReverseHandler(DeviceState State, BVT.TestResults Result);

        [OperationContract(IsOneWay = true)]
        void BVTNotificationHandler(BVT.HWProblemReason Problem, BVT.HWWarningReason Warning, BVT.HWFaultReason Fault,
                                    BVT.HWDisableReason Disable);

        [OperationContract(IsOneWay = true)]
        void DvDtHandler(DeviceState State, dVdt.TestResults Result);

        [OperationContract(IsOneWay = true)]
        void DvDtNotificationHandler(dVdt.HWWarningReason Warning, dVdt.HWFaultReason Fault,
                                    dVdt.HWDisableReason Disable);

        [OperationContract(IsOneWay = true)]
        void TOUHandler(DeviceState State, TOU.TestResults Result);

        [OperationContract(IsOneWay = true)]
        void TOUNotificationHandler(ushort Problem, ushort Warning, ushort Fault, ushort Disable);

        [OperationContract(IsOneWay = true)]
        void ATUHandler(DeviceState State, ATU.TestResults Result);

        [OperationContract(IsOneWay = true)]
        void ATUNotificationHandler(ushort Warning, ushort Fault, ushort Disable);

        [OperationContract(IsOneWay = true)]
        void QrrTqHandler(DeviceState State, QrrTq.TestResults Result);

        [OperationContract(IsOneWay = true)]
        void QrrTqNotificationHandler(ushort Problem, ushort Warning, ushort Fault, ushort Disable);

        [OperationContract(IsOneWay = true)]
        void QrrTqKindOfFreezingHandler(ushort KindOfFreezing);

        [OperationContract(IsOneWay = true)]
        void IHHandler(DeviceState State, IH.TestResults Result);

        [OperationContract(IsOneWay = true)]
        void IHNotificationHandler(ushort Problem, ushort Warning, ushort Fault, ushort Disable);

        [OperationContract(IsOneWay = true)]
        void RCCHandler(DeviceState State, RCC.TestResults Result);

        [OperationContract(IsOneWay = true)]
        void RCCNotificationHandler(ushort Problem, ushort Warning, ushort Fault, ushort Disable);

        [OperationContract(IsOneWay = true)]
        void ClampingSwitchHandler(Clamping.SqueezingState Up, IList<float> ArrayF, IList<float> ArrayFd);

        [OperationContract(IsOneWay = true)]
        void ClampingNotificationHandler(Clamping.HWWarningReason Warning, Clamping.HWProblemReason Problem, Clamping.HWFaultReason Fault);

        [OperationContract(IsOneWay = true)]
        void ExceptionHandler(ComplexParts Device, string Message);

        [OperationContract(IsOneWay = true)]
        void ClampingTemperatureHandler(HeatingChannel channel, int temeprature);

        [OperationContract(IsOneWay = true)]
        void SctuHandler(SctuHwState state, SctuTestResults results);

        [OperationContract(IsOneWay = true)]
        void DbSyncState(DeviceConnectionState state, string message);

        [OperationContract(IsOneWay = true)]
        void BVTUdsmUrsmDirectHandler(DeviceState state, TestResults result);
        [OperationContract(IsOneWay = true)]
        void BVTUdsmUrsmReverseHandler(DeviceState state, TestResults result);
    }

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public class FaultData
    {
        [DataMember]
        public ComplexParts Device { get; set; }

        [DataMember]
        public DateTime TimeStamp { get; set; } = DateTime.Now;

        [DataMember]
        public string Message { get; set; }
    }
}