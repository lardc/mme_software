using System;
using System.Collections.Generic;
using System.ServiceModel;
using SCME.Types.BaseTestParams;
using SCME.Types.Commutation;
using SCME.Types.Gate;
using SCME.Types.Profiles;
using SCME.Types.SCTU;
using SCME.Types.SQL;

namespace SCME.Types
{
    public class ControlServerProxy : DuplexClientBase<IExternalControl>, IExternalControl
    {
          public ControlServerProxy(InstanceContext instanceContext, string uri) : base(instanceContext, WcfClientBindings.DefaultNetTcpBinding, new EndpointAddress(uri))
        {
        }

        public void Check()
        {
            Channel.Check();
        }

        public void Subscribe()
        {
            Channel.Subscribe();
        }

        public void Unsubscribe()
        {
            Channel.Unsubscribe();
        }

        public void Initialize(TypeCommon.InitParams Param)
        {
            Channel.Initialize(Param);
        }

        public void Deinitialize()
        {
            Channel.Deinitialize();
        }

        public InitializationResponse IsInitialized()
        {
            return Channel.IsInitialized();
        }

        public bool GetButtonState(ComplexButtons Button)
        {
            return Channel.GetButtonState(Button);
        }

        public void ProvocationButtonResponse(ComplexButtons Button)
        {
            Channel.ProvocationButtonResponse(Button);
        }

        public ComplexSafety GetSafetyType()
        {
            return Channel.GetSafetyType();
        }

        public bool Start(Gate.TestParameters ParametersGate, VTM.TestParameters ParametersVTM,
                          BVT.TestParameters ParametersBVT, ATU.TestParameters ParametersATU, QrrTq.TestParameters ParametersQrrTq, IH.TestParameters ParametersIH, RCC.TestParameters ParametersRCC,
                          Commutation.TestParameters ParametersCommutation, Clamping.TestParameters ParametersClamping, TOU.TestParameters ParametersTOU)
        {
            return Channel.Start(ParametersGate, ParametersVTM, ParametersBVT, ParametersATU, ParametersQrrTq, ParametersIH, ParametersRCC, ParametersCommutation, ParametersClamping, ParametersTOU);
        }

        public void Stop()
        {
            Channel.Stop();
        }

        public void StopImpulse()
        {
            Channel.StopImpulse();
        }

        public void StopByButtonStop()
        {
            Channel.StopByButtonStop();
        }

        public void ClearSafetyTrig()
        {
            Channel.ClearSafetyTrig();
        }

        public void SafetySystemOn()
        {
            Channel.SafetySystemOn();
        }

        public void SafetySystemOff()
        {
            Channel.SafetySystemOff();
        }

        public string NotReadyDevicesToStart()
        {
            return Channel.NotReadyDevicesToStart();
        }

        public string NotReadyDevicesToStartDynamic()
        {
            return Channel.NotReadyDevicesToStartDynamic();
        }

        public void SqueezeClamping(Clamping.TestParameters ParametersClamping)
        {
            Channel.SqueezeClamping(ParametersClamping);
        }

        public void UnsqueezeClamping(Clamping.TestParameters ParametersClamping)
        {
            Channel.UnsqueezeClamping(ParametersClamping);
        }

        public void ClearFault(ComplexParts Device)
        {
            Channel.ClearFault(Device);
        }

        public ushort ActivationWorkPlace(ComplexParts Device, ChannelByClumpType ChByClumpType, SctuWorkPlaceActivationStatuses ActivationStatus)
        {
            return Channel.ActivationWorkPlace(Device, ChByClumpType, ActivationStatus);
        }

        public ushort ReadRegister(ComplexParts Device, ushort Address)
        {
            return Channel.ReadRegister(Device, Address);
        }

        public void WriteRegister(ComplexParts Device, ushort Address, ushort Value)
        {
            Channel.WriteRegister(Device, Address, Value);
        }

        public void CallAction(ComplexParts Device, ushort Address)
        {
            Channel.CallAction(Device, Address);
        }

      

        public void WriteResults(ResultItem Item, List<string> Errors)
        {
            Channel.WriteResults(Item, Errors);
        }

        public List<ProfileForSqlSelect> SaveProfiles(List<ProfileItem> Item)
        {
            return Channel.SaveProfiles(Item);
        }

        public bool RequestRemotePrinting(string GroupName, string CustomerName, string DeviceType,
            ReportSelectionPredicate Predicate)
        {
            return Channel.RequestRemotePrinting(GroupName, CustomerName, DeviceType, Predicate);
        }

        public bool StartDynamic(Commutation.TestParameters paramsComm, Clamping.TestParameters paramsClamp, Gate.TestParameters[] parametersGate, VTM.TestParameters[] parametersVtm,
                         BVT.TestParameters[] parametersBvt, dVdt.TestParameters[] parametersDvDt, ATU.TestParameters[] parametersAtu,
                         QrrTq.TestParameters[] parametersQrrTq, SctuTestParameters[] parametersSctu, TOU.TestParameters[] parametersTOU)
        {
            return Channel.StartDynamic(paramsComm, paramsClamp, parametersGate, parametersVtm, parametersBvt, parametersDvDt, parametersAtu, parametersQrrTq, parametersSctu, parametersTOU);
        }

        public bool StartImpulse(List<BaseTestParametersAndNormatives> parameters, DutPackageType dutPackageType)
        {
            return Channel.StartImpulse(parameters, dutPackageType);
        }

   
        public bool StartHeating(int temperature)
        {
            return Channel.StartHeating(temperature);
        }

        public void StopHeating()
        {
            Channel.StopHeating();
        }

        public void SetPermissionToUseCanDataBus(bool PermissionToUseCanDataBus)
        {
            Channel.SetPermissionToUseCanDataBus(PermissionToUseCanDataBus);
        }

        public void SetSafetyMode(SafetyMode safetyMode)
        {
            Channel.SetSafetyMode(safetyMode);
        }

        public (MyProfile profile, bool IsInMmeCode) SyncProfile(MyProfile profile)
        {
            return Channel.SyncProfile(profile);
        }

        public void SetUserWorkMode(UserWorkMode userWorkMode)
        {
            Channel.SetUserWorkMode(userWorkMode);
        }

        public void GateWriteCalibrationParameters(CalibrationParameters Parameters)
        {
            Channel.GateWriteCalibrationParameters(Parameters);
        }

        public CalibrationParameters GateReadCalibrationParameters()
        {
            return Channel.GateReadCalibrationParameters();
        }

        public CalibrationResultGate GatePulseCalibrationGate(ushort Current)
        {
            return Channel.GatePulseCalibrationGate(Current);
        }

        public ushort GatePulseCalibrationMain(ushort Current)
        {
            return Channel.GatePulseCalibrationMain(Current);
        }

        void IExternalControl.StopImpulse()
        {
            Channel.StopImpulse();
        }
    }
}