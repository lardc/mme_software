using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using SCME.Service.Properties;
using SCME.Types;
using SCME.Types.BaseTestParams;
using SCME.Types.Commutation;
using SCME.Types.Profiles;
using SCME.Types.SCTU;

namespace SCME.Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
        Namespace = "http://proton-electrotex.com/SCME",
        ConcurrencyMode = ConcurrencyMode.Reentrant, IncludeExceptionDetailInFaults = true)]
    public class ExternalControlServer : IExternalControl
    {
        private readonly BroadcastCommunication m_Communication;
        private readonly LogicContainer m_IOMain;

        private const string PRINTING_ENDPOINT_NAME = "SCME.PrintingService";

        internal ExternalControlServer()
        {
            m_Communication = new BroadcastCommunication();
            m_IOMain = new LogicContainer(m_Communication);
        }

        #region Interface implementation

        void IExternalControl.Check()
        {
        }

        void IExternalControl.Subscribe()
        {
            var item = OperationContext.Current.GetCallbackChannel<IClientCallback>();

            if (!m_Communication.Subscribers.Contains(item))
                m_Communication.Subscribers.Add(item);
        }

        void IExternalControl.Unsubscribe()
        {
            var item = OperationContext.Current.GetCallbackChannel<IClientCallback>();

            if (m_Communication.Subscribers.Contains(item))
                m_Communication.Subscribers.Add(item);
        }

        void IExternalControl.Initialize(TypeCommon.InitParams Param)
        {
            ThreadPool.QueueUserWorkItem(Func => m_IOMain.Initialize(Param));
        }

        void IExternalControl.Deinitialize()
        {
            m_IOMain.Deinitialize();
        }

        bool IExternalControl.IsInitialized()
        {
            return m_IOMain.IsInitialized;
        }

        bool IExternalControl.GetButtonState(ComplexButtons Button)
        {
            return m_IOMain.GetButtonState(Button);
        }

        bool IExternalControl.Start(Types.Gate.TestParameters ParametersGate, Types.SL.TestParameters ParametersSL,
                                    Types.BVT.TestParameters ParametersBvt,
                                    Types.Commutation.TestParameters ParametersComm, Types.Clamping.TestParameters ParametersClamp)
        {
            return m_IOMain.Start(ParametersGate, ParametersSL, ParametersBvt, ParametersComm, ParametersClamp);
        }

        bool IExternalControl.StartDynamic(TestParameters parametersCommutation, Types.Clamping.TestParameters parametersClamp, Types.Gate.TestParameters[] parametersGate, Types.SL.TestParameters[] parametersSl, Types.BVT.TestParameters[] parametersBvt, Types.dVdt.TestParameters[] parametersDvDt, SctuTestParameters[] parametersSctu)
        {
            return m_IOMain.Start(parametersCommutation, parametersClamp, parametersGate, parametersSl, parametersBvt, parametersDvDt, parametersSctu);
        }

        public bool StartHeating(int temperature)
        {
            return m_IOMain.StartHeating(temperature);
        }

        public void StopMeasureTemperature()
        {
            m_IOMain.StopMeasureTemperature();
        }
        
        void IExternalControl.Stop()
        {
            ThreadPool.QueueUserWorkItem(Func => m_IOMain.Stop());
        }

        void IExternalControl.SqueezeClamping(Types.Clamping.TestParameters ParametersClamping)
        {
            m_IOMain.Squeeze(ParametersClamping);
        }

        void IExternalControl.UnsqueezeClamping(Types.Clamping.TestParameters ParametersClamping)
        {
            m_IOMain.Unsqueeze(ParametersClamping);
        }

        void IExternalControl.ClearFault(ComplexParts Device)
        {
            m_IOMain.ClearFault(Device);
        }

        ushort IExternalControl.ReadRegister(ComplexParts Device, ushort Address)
        {
            return m_IOMain.ReadRegister(Device, Address);
        }

        void IExternalControl.WriteRegister(ComplexParts Device, ushort Address, ushort Value)
        {
            m_IOMain.WriteRegister(Device, Address, Value);
        }

        void IExternalControl.CallAction(ComplexParts Device, ushort Address)
        {
            m_IOMain.CallAction(Device, Address);
        }

        Types.Gate.CalibrationResultGate IExternalControl.GatePulseCalibrationGate(ushort Current)
        {
            return m_IOMain.GatePulseCalibrationGate(Current);
        }

        ushort IExternalControl.GatePulseCalibrationMain(ushort Current)
        {
            return m_IOMain.GatePulseCalibrationMain(Current);
        }

        void IExternalControl.GateWriteCalibrationParameters(Types.Gate.CalibrationParameters Parameters)
        {
            m_IOMain.GateWriteCalibrationParams(Parameters);
        }

        Types.Gate.CalibrationParameters IExternalControl.GateReadCalibrationParameters()
        {
            return m_IOMain.GateReadCalibrationParams();
        }

        void IExternalControl.SLWriteCalibrationParameters(Types.SL.CalibrationParameters Parameters)
        {
            m_IOMain.SLWriteCalibrationParams(Parameters);
        }

        Types.SL.CalibrationParameters IExternalControl.SLReadCalibrationParameters()
        {
            return m_IOMain.SLReadCalibrationParams();
        }

        void IExternalControl.BVTWriteCalibrationParameters(Types.BVT.CalibrationParams Parameters)
        {
            m_IOMain.BVTWriteCalibrationParams(Parameters);
        }

        Types.BVT.CalibrationParams IExternalControl.BVTReadCalibrationParameters()
        {
            return m_IOMain.BVTReadCalibrationParams();
        }

        void IExternalControl.CSWriteCalibrationParameters(Types.Clamping.CalibrationParams Parameters)
        {
            m_IOMain.CSWriteCalibrationParams(Parameters);
        }

        Types.Clamping.CalibrationParams IExternalControl.CSReadCalibrationParameters()
        {
            return m_IOMain.CSReadCalibrationParams();
        }

        void IExternalControl.DvDtWriteCalibrationParameters(Types.dVdt.CalibrationParams Parameters)
        {
            m_IOMain.DvDtWriteCalibrationParams(Parameters);
        }

        Types.dVdt.CalibrationParams IExternalControl.DvDtReadCalibrationParameters()
        {
            return m_IOMain.DvDtReadCalibrationParams();
        }

        void IExternalControl.QRRWriteCalibrationParameters(Types.QRR.CalibrationParams Parameters)
        {
            //m_IOMain.QRRWriteCalibrationParams(Parameters);
        }

        Types.QRR.CalibrationParams IExternalControl.QRRReadCalibrationParameters()
        {
            return new Types.QRR.CalibrationParams(); //m_IOMain.QRRReadCalibrationParams();
        }

        void IExternalControl.WriteResults(ResultItem Item, List<string> Errors)
        {
            m_IOMain.WriteResults(Item, Errors);
        }

        void IExternalControl.SaveProfiles(List<ProfileItem> profileItems)
        {
            m_IOMain.SaveProfiles(profileItems);
        }

        bool IExternalControl.RequestRemotePrinting(string GroupName, string CustomerName, string DeviceType,
            ReportSelectionPredicate Predicate)
        {
            try
            {
                var printingProxy = new PrintingServiceProxy(PRINTING_ENDPOINT_NAME);
                printingProxy.Open();

                SystemHost.Journal.AppendLog(ComplexParts.Service, LogMessageType.Info, "Printing service connection is opened");

                try
                {
                    printingProxy.RequestRemotePrinting(Settings.Default.MMECode, GroupName, CustomerName,
                        DeviceType, Predicate);

                    SystemHost.Journal.AppendLog(ComplexParts.Service, LogMessageType.Info,
                        String.Format("Printing request: GroupName - {0}, Selection - {1}", GroupName, Predicate));
                }
                finally
                {
                    if (printingProxy.State == CommunicationState.Faulted)
                        printingProxy.Abort();
                    else
                        printingProxy.Close();
                }

                return true;
            }
            catch (FaultException<FaultData> ex)
            {
                SystemHost.Journal.AppendLog(ComplexParts.Service, LogMessageType.Error,
                    String.Format("Printing request failed: {0}", ex.Message));

                return false;
            }
            catch (Exception ex)
            {
                SystemHost.Journal.AppendLog(ComplexParts.Service, LogMessageType.Error,
                    String.Format("Printing request failed: {0}", ex.Message));

                return false;
            }
        }

       

        #endregion
    }
}