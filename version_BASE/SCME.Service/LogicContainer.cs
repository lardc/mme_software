using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Threading;
using SCME.Service.IO;
using SCME.Service.Properties;
using SCME.Types;
using SCME.Types.BaseTestParams;
using SCME.Types.Commutation;
using SCME.Types.Profiles;
using SCME.Types.SCTU;

namespace SCME.Service
{
    internal class LogicContainer
    {
        private readonly BroadcastCommunication m_Communication;

        private readonly IOAdapter m_IOAdapter;
        private readonly IOGateway m_IOGateway;
        private readonly IOCommutation m_IOCommutation, m_IOCommutationEx;
        private readonly IOGate m_IOGate;
        private readonly IOStLs m_IOStls;
        private readonly IOBvt m_IOBvt;
        private readonly IOClamping m_IOClamping;
        private readonly IOdVdt m_IOdVdt;
        private readonly IoSctu _ioSctu;
        private readonly ThreadService m_Thread;
        private readonly bool m_ClampingSystemConnected;

        private Types.Gate.TestParameters m_ParametersGate;
        private Types.SL.TestParameters m_ParametersSL;
        private Types.BVT.TestParameters m_ParametersBvt;
        private Types.dVdt.TestParameters m_ParametersdVdt;

        private Types.Gate.TestParameters[] m_ParametersGateDyn;
        private Types.SL.TestParameters[] m_ParametersSLDyn;
        private Types.BVT.TestParameters[] m_ParametersBvtDyn;
        private Types.dVdt.TestParameters[] m_ParametersdVdtDyn;
        private SctuTestParameters[] _sctuTestParameters;

        private TypeCommon.InitParams m_Param;
        private DeviceState m_State = DeviceState.None;
        private DeviceConnectionState m_ConnectionState = DeviceConnectionState.None;
        private Boolean m_Stop;

        public LogicContainer(BroadcastCommunication Communication)
        {
            m_ClampingSystemConnected = Settings.Default.IsClampingSystemConnected;
            m_Communication = Communication;

            m_Thread = new ThreadService();
            m_Thread.FinishedHandler += Thread_FinishedHandler;

            m_ParametersGate = new Types.Gate.TestParameters { IsEnabled = false };
            m_ParametersSL = new Types.SL.TestParameters { IsEnabled = false };
            m_ParametersBvt = new Types.BVT.TestParameters { IsEnabled = false };
            m_ParametersdVdt = new Types.dVdt.TestParameters { IsEnabled = false };

            m_IOAdapter = new IOAdapter(m_Communication);
            m_IOGateway = new IOGateway(m_IOAdapter, m_Communication);
            m_IOCommutation = new IOCommutation(m_IOAdapter, m_Communication, Settings.Default.CommutationEmulation,
                                                Settings.Default.CommutationNode, Settings.Default.IsCommutationType6, ComplexParts.Commutation);
            m_IOCommutationEx = new IOCommutation(m_IOAdapter, m_Communication, Settings.Default.CommutationExEmulation,
                                                  Settings.Default.CommutationExNode,
                                                  Settings.Default.IsCommutationExType6, ComplexParts.CommutationEx);
            m_IOClamping = new IOClamping(m_IOAdapter, m_Communication);

            m_IOGate = new IOGate(m_IOAdapter, m_Communication);
            m_IOStls = new IOStLs(m_IOAdapter, m_Communication);
            m_IOBvt = new IOBvt(m_IOAdapter, m_Communication);
            m_IOdVdt = new IOdVdt(m_IOAdapter, m_Communication);
            _ioSctu = new IoSctu(m_IOAdapter,m_Communication);

            m_IOGate.ActiveCommutation = m_IOCommutation;
            m_IOStls.ActiveCommutation = m_IOCommutation;
            m_IOBvt.ActiveCommutation = m_IOCommutation;
        }

        void Thread_FinishedHandler(object Sender, ThreadFinishedEventArgs E)
        {
            if (E.Error != null)
                m_Communication.PostExceptionEvent(ComplexParts.Service, E.Error.Message);
        }

        internal void Initialize(TypeCommon.InitParams Params)
        {
            try
            {
                Deinitialize();
            }
            catch (Exception ex)
            {
                SystemHost.Journal.AppendLog(ComplexParts.Service, LogMessageType.Error, string.Format(Resources.Error_LogicContainer_Exception, ex.Message));
                m_Communication.PostCommonConnectionEvent(DeviceConnectionState.DisconnectionError, ex.Message);
                return;
            }

            m_Param = Params;
            var state = DeviceConnectionState.ConnectionInProcess;

            SystemHost.Journal.AppendLog(ComplexParts.Service, LogMessageType.Info,
                                         string.Format(Resources.Log_LogicContainer_Requested_block_modes, m_Param.IsGateEnabled, m_Param.IsSLEnabled, m_Param.IsBVTEnabled, m_Param.IsClampEnabled));

            try
            {
                state = m_IOAdapter.Initialize(m_Param.TimeoutAdapter);

                if (state == DeviceConnectionState.ConnectionSuccess)
                    state = m_IOGateway.Initialize();
                if (state == DeviceConnectionState.ConnectionSuccess)
                    state = m_IOCommutation.Initialize();
                if (state == DeviceConnectionState.ConnectionSuccess)
                    state = m_IOCommutationEx.Initialize();
                if (state == DeviceConnectionState.ConnectionSuccess)
                    state = m_IOGate.Initialize(m_Param.IsGateEnabled, m_Param.TimeoutGate);
                if (state == DeviceConnectionState.ConnectionSuccess)
                    state = m_IOStls.Initialize(m_Param.IsSLEnabled, m_Param.TimeoutSL);
                if (state == DeviceConnectionState.ConnectionSuccess)
                    state = m_IOBvt.Initialize(m_Param.IsBVTEnabled, m_Param.TimeoutBVT);
                if (state == DeviceConnectionState.ConnectionSuccess && m_ClampingSystemConnected)
                    state = m_IOClamping.Initialize(m_Param.IsClampEnabled, m_Param.TimeoutClamp);
                if (state == DeviceConnectionState.ConnectionSuccess)
                    state = m_IOdVdt.Initialize(m_Param.IsdVdtEnabled, m_Param.TimeoutdVdt);
                if (state == DeviceConnectionState.ConnectionSuccess)
                    state = _ioSctu.Initialize(m_Param.IsSctuEnabled, m_Param.TimeoutSctu);

                SystemHost.Journal.AppendLog(ComplexParts.Service, LogMessageType.Info,
                    string.Format(Resources.Log_LogicContainer_Connection_state,
                        state == DeviceConnectionState.ConnectionSuccess
                            ? Resources.Log_LogicContainer_Connected
                            : Resources.Log_LogicContainer_Failed));
            }
            catch (Exception ex)
            {
                SystemHost.Journal.AppendLog(ComplexParts.Service, LogMessageType.Error, string.Format(Resources.Error_LogicContainer_Exception, ex.Message));
            }

            m_ConnectionState = state;
            m_Communication.PostCommonConnectionEvent(state, state == DeviceConnectionState.ConnectionSuccess ? Resources.Log_LogicContainer_Connected : Resources.Log_LogicContainer_Failed);

            IsInitialized = true;
        }

        internal void Deinitialize()
        {
            if (m_ConnectionState != DeviceConnectionState.ConnectionSuccess
                && m_ConnectionState != DeviceConnectionState.ConnectionFailed)
                return;

            if (m_ConnectionState == DeviceConnectionState.ConnectionInProcess)
                throw new ApplicationException("Initialization is in process");

            Exception savedEx = null;

            try
            {
                m_IOdVdt.Deinitialize();
            }
            catch (Exception ex)
            {
                var message = string.Format(Resources.Error_LogicContainer_Exception, ex.Message);
                SystemHost.Journal.AppendLog(ComplexParts.Service, LogMessageType.Error, message);
                savedEx = ex;
            }

            try
            {
                if (Settings.Default.IsClampingSystemConnected)
                    m_IOClamping.Deinitialize();
            }
            catch (Exception ex)
            {
                var message = string.Format(Resources.Error_LogicContainer_Exception, ex.Message);
                SystemHost.Journal.AppendLog(ComplexParts.Service, LogMessageType.Error, message);
                savedEx = ex;
            }

            try
            {
                m_IOBvt.Deinitialize();
            }
            catch (Exception ex)
            {
                var message = string.Format(Resources.Error_LogicContainer_Exception, ex.Message);
                SystemHost.Journal.AppendLog(ComplexParts.Service, LogMessageType.Error, message);
                savedEx = ex;
            }

            try
            {
                m_IOStls.Deinitialize();
            }
            catch (Exception ex)
            {
                var message = string.Format(Resources.Error_LogicContainer_Exception, ex.Message);
                SystemHost.Journal.AppendLog(ComplexParts.Service, LogMessageType.Error, message);
                savedEx = ex;
            }

            try
            {
                m_IOGate.Deinitialize();
            }
            catch (Exception ex)
            {
                var message = string.Format(Resources.Error_LogicContainer_Exception, ex.Message);
                SystemHost.Journal.AppendLog(ComplexParts.Service, LogMessageType.Error, message);
                savedEx = ex;
            }

            try
            {
                m_IOCommutation.Deinitialize();
            }
            catch (Exception ex)
            {
                var message = string.Format(Resources.Error_LogicContainer_Exception, ex.Message);
                SystemHost.Journal.AppendLog(ComplexParts.Service, LogMessageType.Error, message);
                savedEx = ex;
            }

            try
            {
                m_IOCommutationEx.Deinitialize();
            }
            catch (Exception ex)
            {
                var message = string.Format(Resources.Error_LogicContainer_Exception, ex.Message);
                SystemHost.Journal.AppendLog(ComplexParts.Service, LogMessageType.Error, message);
                savedEx = ex;
            }

            try
            {
                m_IOGateway.Deinitialize();
            }
            catch (Exception ex)
            {
                var message = string.Format(Resources.Error_LogicContainer_Exception, ex.Message);
                SystemHost.Journal.AppendLog(ComplexParts.Service, LogMessageType.Error, message);
                savedEx = ex;
            }

            try
            {
                m_IOAdapter.Deinitialize();
            }
            catch (Exception ex)
            {
                var message = string.Format(Resources.Error_LogicContainer_Exception, ex.Message);
                SystemHost.Journal.AppendLog(ComplexParts.Service, LogMessageType.Error, message);
                savedEx = ex;
            }
            try
            {
                _ioSctu.Deinitialize();
            }
            catch (Exception ex)
            {
                var message = string.Format(Resources.Error_LogicContainer_Exception, ex.Message);
                SystemHost.Journal.AppendLog(ComplexParts.Service, LogMessageType.Error, message);
                savedEx = ex;
            }

            GC.Collect();

            SystemHost.Journal.AppendLog(ComplexParts.Service, LogMessageType.Info,
                                            Resources.Log_LogicContainer_All_disconnected);

            if (savedEx != null)
                m_Communication.PostCommonConnectionEvent(DeviceConnectionState.DisconnectionError,
                                                          string.Format(Resources.Error_LogicContainer_Exception, savedEx.Message));

            IsInitialized = false;
        }

        internal bool IsInitialized { get; private set; }

        internal bool GetButtonState(ComplexButtons Button)
        {
            try
            {
                return m_IOGateway.GetButtonState(Button);
            }
            catch (Exception ex)
            {
                ThrowFaultException(ComplexParts.Gateway, ex.ToString(), String.Format(@"{0}.{1}", GetType().Name, MethodBase.GetCurrentMethod().Name), false);
            }

            return false;
        }

        #region Test sequence

        public bool Start(Types.Gate.TestParameters ParametersGate, Types.SL.TestParameters ParametersSL,
                          Types.BVT.TestParameters ParametersBvt, Types.Commutation.TestParameters ParametersComm, Types.Clamping.TestParameters ParametersClamp)
        {
            m_Stop = false;

            if (m_State == DeviceState.InProcess)
                ThrowFaultException(ComplexParts.Service, "Test is already started", String.Format(@"{0}.{1}", GetType().Name, MethodBase.GetCurrentMethod().Name), false);

            if (ParametersComm.BlockIndex == Types.Commutation.HWBlockIndex.Block1)
            {
                m_IOGate.ActiveCommutation = m_IOCommutation;
                m_IOStls.ActiveCommutation = m_IOCommutation;
                m_IOBvt.ActiveCommutation = m_IOCommutation;
            }
            else
            {
                m_IOGate.ActiveCommutation = m_IOCommutationEx;
                m_IOStls.ActiveCommutation = m_IOCommutationEx;
                m_IOBvt.ActiveCommutation = m_IOCommutationEx;
            }

            m_ParametersGate = ParametersGate;
            m_ParametersSL = ParametersSL;
            m_ParametersBvt = ParametersBvt;

            m_State = DeviceState.InProcess;

            var message = string.Format("Start main test, state {0}; test enabled: Gate - {1}, SL, - {2}, BVT - {3}",
                                        m_State, m_ParametersGate.IsEnabled, m_ParametersSL.IsEnabled,
                                        m_ParametersBvt.IsEnabled);
            SystemHost.Journal.AppendLog(ComplexParts.Service, LogMessageType.Info, message);

            m_Communication.PostTestAllEvent(m_State, "Starting tests");

            try
            {
                m_Thread.StartSingle(Dummy => MeasurementLogicRoutine(ParametersComm, ParametersClamp));
            }
            catch (Exception e)
            {
                ThrowFaultException(ComplexParts.None, e.Message, String.Format(@"{0}.{1}", GetType().Name, MethodBase.GetCurrentMethod().Name), false);
            }

            return true;
        }

        public bool Start(TestParameters parametersCommutation, Types.Clamping.TestParameters parametersClamp, Types.Gate.TestParameters[] parametersGate, Types.SL.TestParameters[] parametersSl, Types.BVT.TestParameters[] parametersBvt, Types.dVdt.TestParameters[] parametersDvDt, SctuTestParameters[] parametersSctu)
        {
            m_Stop = false;

            if (m_State == DeviceState.InProcess)
                ThrowFaultException(ComplexParts.Service, "Test is already started", String.Format(@"{0}.{1}", GetType().Name, MethodBase.GetCurrentMethod().Name), false);

            if (parametersCommutation.BlockIndex == HWBlockIndex.Block1)
            {
                m_IOGate.ActiveCommutation = m_IOCommutation;
                m_IOStls.ActiveCommutation = m_IOCommutation;
                m_IOBvt.ActiveCommutation = m_IOCommutation;
            }
            else
            {
                m_IOGate.ActiveCommutation = m_IOCommutationEx;
                m_IOStls.ActiveCommutation = m_IOCommutationEx;
                m_IOBvt.ActiveCommutation = m_IOCommutationEx;
            }

            m_ParametersGateDyn = parametersGate;
            m_ParametersBvtDyn = parametersBvt;
            m_ParametersSLDyn = parametersSl;
            m_ParametersdVdtDyn = parametersDvDt;
            _sctuTestParameters = parametersSctu;

            m_State = DeviceState.InProcess;

            var message = string.Format("Start main test, state {0}; test enabled:", m_State);
            SystemHost.Journal.AppendLog(ComplexParts.Service, LogMessageType.Info, message);

            m_Communication.PostTestAllEvent(m_State, "Starting tests");

            try
            {
                m_Thread.StartSingle(Dummy => MeasurementDynamicLogicRoutine(parametersCommutation, parametersClamp));
            }
            catch (Exception e)
            {
                ThrowFaultException(ComplexParts.None, e.Message, String.Format(@"{0}.{1}", GetType().Name, MethodBase.GetCurrentMethod().Name), false);
            }

            return true;
        }


        public bool StartHeating(int temperature)
        {
            m_Communication.PostTestAllEvent(DeviceState.Heating, "Starting heating");

            try
            {
                m_Thread.StartSingle(Dummy => HeatingRoutine(temperature));
            }
            catch (Exception e)
            {
                ThrowFaultException(ComplexParts.None, e.Message, String.Format(@"{0}.{1}", GetType().Name, MethodBase.GetCurrentMethod().Name), false);
            }

            return true;
        }

        public void StopMeasureTemperature()
        {
            m_IOClamping.DisposeTimer();
        }

        private void MeasurementDynamicLogicRoutine(TestParameters parametersCommutation, Types.Clamping.TestParameters parametersClamp)
        {
            var res = DeviceState.Success;

            try
            {
                try
                {
                    if (m_ClampingSystemConnected && m_Param.IsClampEnabled && !m_Stop)
                        res = m_IOClamping.Squeeze(parametersClamp);
                }
                catch (Exception ex)
                {
                    ThrowFaultException(ComplexParts.Clamping, ex.Message, "Start squeezing");
                }

                if (res == DeviceState.Success)
                {
                    var orderedParameters = new List<BaseTestParametersAndNormatives>(m_ParametersGateDyn.Length + m_ParametersSLDyn.Length + m_ParametersBvtDyn.Length);
                    orderedParameters.AddRange(m_ParametersGateDyn);
                    orderedParameters.AddRange(m_ParametersSLDyn);
                    orderedParameters.AddRange(m_ParametersBvtDyn);
                    orderedParameters.AddRange(m_ParametersdVdtDyn);
                    orderedParameters.AddRange(_sctuTestParameters);
                    orderedParameters = orderedParameters.OrderBy(o => o.Order).ToList();

                    foreach (var baseTestParametersAndNormativese in orderedParameters)
                    {
                        if (m_Stop) continue;

                        var gateParams = baseTestParametersAndNormativese as Types.Gate.TestParameters;

                        if (!ReferenceEquals(gateParams, null))
                            try
                            {
                                if (m_ClampingSystemConnected && m_Param.IsClampEnabled && !m_Stop)
                                    m_IOClamping.ReturnForceToDefault();
                                res = m_IOGate.Start(gateParams, parametersCommutation);
                            }
                            catch (Exception ex)
                            {
                                ThrowFaultException(ComplexParts.Gate, ex.Message, "Start Gate test");
                            }

                        var slParameters = baseTestParametersAndNormativese as Types.SL.TestParameters;

                        if (!ReferenceEquals(slParameters, null))
                            try
                            {
                                if (m_ClampingSystemConnected && m_Param.IsClampEnabled && !m_Stop)
                                    m_IOClamping.SetCustomForce();
                                res = m_IOStls.Start(slParameters, parametersCommutation);
                            }
                            catch (Exception ex)
                            {
                                ThrowFaultException(ComplexParts.SL, ex.Message, "Start SL test");
                            }

                        var bvtParameters = baseTestParametersAndNormativese as Types.BVT.TestParameters;
                        if (!ReferenceEquals(bvtParameters, null))
                        {
                            try
                            {
                                if (m_ClampingSystemConnected && m_Param.IsClampEnabled && !m_Stop)
                                    m_IOClamping.ReturnForceToDefault();
                                res = m_IOBvt.Start(bvtParameters, parametersCommutation);
                            }
                            catch (Exception ex)
                            {
                                ThrowFaultException(ComplexParts.BVT, ex.Message, "Start BVT test");
                            } 
                        }
                        var dvDtParameters = baseTestParametersAndNormativese as Types.dVdt.TestParameters;
                        if (!ReferenceEquals(dvDtParameters, null))
                        {
                            try
                            {
                                if (m_ClampingSystemConnected && m_Param.IsClampEnabled && !m_Stop)
                                    m_IOClamping.SetCustomForce();
                                res = m_IOdVdt.Start(dvDtParameters);
                            }
                            catch (Exception ex)
                            {
                                ThrowFaultException(ComplexParts.BVT, ex.Message, "Start dVdt test");
                            }  
                        }
                        var sctuParameters = baseTestParametersAndNormativese as SctuTestParameters;
                        if (!ReferenceEquals(sctuParameters, null))
                        {
                            try
                            {
                                if (m_ClampingSystemConnected && m_Param.IsClampEnabled && !m_Stop)
                                    m_IOClamping.ReturnForceToDefault();
                               _ioSctu.Start(sctuParameters);
                            }
                            catch (Exception ex)
                            {
                                ThrowFaultException(ComplexParts.Sctu, ex.Message, "Start sctu test");
                            }  
                        }

                    }

                }
            }
            finally
            {
                try
                {
                    if (m_ClampingSystemConnected && m_Param.IsClampEnabled && !parametersClamp.IsHeightMeasureEnabled)
                        res = m_IOClamping.Unsqueeze(parametersClamp);
                    if (parametersClamp.IsHeightMeasureEnabled)
                    {
                        SystemHost.Journal.AppendLog(ComplexParts.Service, LogMessageType.Info, "Height measure enabled");
                    }
                }
                catch (Exception ex)
                {
                    ThrowFaultException(ComplexParts.Clamping, ex.Message, "Start unsqueezing");
                }
            }

            var msg = string.Format("Main testing, state - {0}", res);
            SystemHost.Journal.AppendLog(ComplexParts.Service, LogMessageType.Info, msg);

            m_State = DeviceState.Success;
            m_Communication.PostTestAllEvent(m_State, "Tests are done");
        }

        private void HeatingRoutine(int temperature)
        {
            try
            {
                var res = DeviceState.Heating;

                if ((m_ClampingSystemConnected || m_IOClamping.m_IsClampingEmulation) && m_Param.IsClampEnabled && !m_Stop)
                    res = m_IOClamping.StartHeating(temperature);

                var msg = string.Format("Heating, state - {0}", res);
                SystemHost.Journal.AppendLog(ComplexParts.Service, LogMessageType.Info, msg);
            }
            catch (Exception ex)
            {
                ThrowFaultException(ComplexParts.Clamping, ex.Message, "Start heating");
            }
        }

        private void MeasurementLogicRoutine(Types.Commutation.TestParameters ParametersComm, Types.Clamping.TestParameters ParametersClamp)
        {
            var res = DeviceState.Success;

            try
            {
                try
                {
                    if (m_ClampingSystemConnected && m_Param.IsClampEnabled && !m_Stop)
                        res = m_IOClamping.Squeeze(ParametersClamp);
                }
                catch (Exception ex)
                {
                    ThrowFaultException(ComplexParts.Clamping, ex.Message, "Start squeezing");
                }

                if (res == DeviceState.Success)
                {
                    if (m_ParametersGate.IsEnabled && m_Param.IsGateEnabled && !m_Stop)
                    {
                        try
                        {
                            if (m_ClampingSystemConnected && m_Param.IsClampEnabled && !m_Stop)
                                m_IOClamping.ReturnForceToDefault();
                            res = m_IOGate.Start(m_ParametersGate, ParametersComm);
                        }
                        catch (Exception ex)
                        {
                            ThrowFaultException(ComplexParts.Gate, ex.Message, "Start Gate test");
                        }
                    }

                    if (m_ParametersSL.IsEnabled && m_Param.IsSLEnabled && !m_Stop)
                    {
                        try
                        {
                            m_IOClamping.SetCustomForce();
                            res = m_IOStls.Start(m_ParametersSL, ParametersComm);
                        }
                        catch (Exception ex)
                        {
                            ThrowFaultException(ComplexParts.SL, ex.Message, "Start SL test");
                        }
                    }

                    if (m_ParametersBvt.IsEnabled && m_Param.IsBVTEnabled && !m_Stop)
                    {
                        try
                        {
                            res = m_IOBvt.Start(m_ParametersBvt, ParametersComm);
                        }
                        catch (Exception ex)
                        {
                            ThrowFaultException(ComplexParts.BVT, ex.Message, "Start BVT test");
                        }
                    }
                }
            }
            finally
            {
                try
                {
                    if (m_ClampingSystemConnected && m_Param.IsClampEnabled && !ParametersClamp.IsHeightMeasureEnabled)
                        res = m_IOClamping.Unsqueeze(ParametersClamp);
                    if (ParametersClamp.IsHeightMeasureEnabled)
                    {
                        SystemHost.Journal.AppendLog(ComplexParts.Service, LogMessageType.Info, "Height measure enabled");
                    }
                }
                catch (Exception ex)
                {
                    ThrowFaultException(ComplexParts.Clamping, ex.Message, "Start unsqueezing");
                }
            }

            var msg = string.Format("Main testing, state - {0}", res);
            SystemHost.Journal.AppendLog(ComplexParts.Service, LogMessageType.Info, msg);

            m_State = DeviceState.Success;
            m_Communication.PostTestAllEvent(m_State, "Tests are done");

        }



        #endregion

        internal void Stop()
        {
            m_Stop = true;

            if (m_ParametersGate.IsEnabled)
                m_IOGate.Stop();
            if (m_ParametersSL.IsEnabled)
                m_IOStls.Stop();
            if (m_ParametersBvt.IsEnabled)
                m_IOBvt.Stop();
            if (m_ClampingSystemConnected)
                m_IOClamping.Stop();
            if (m_ParametersdVdt.IsEnabled)
                m_IOdVdt.Stop();
            
            SystemHost.Journal.AppendLog(ComplexParts.Service, LogMessageType.Info, Resources.Log_LogicContainer_Main_test_manual_stop);
        }

        internal void Squeeze(Types.Clamping.TestParameters ClampingParameters)
        {
            try
            {
                m_Thread.StartSingle(Dummy => m_IOClamping.Squeeze(ClampingParameters, false));
            }
            catch (Exception ex)
            {
                ThrowFaultException(ComplexParts.Clamping, ex.ToString(), String.Format(@"{0}.{1}", GetType().Name, MethodBase.GetCurrentMethod().Name), false);
            }
        }

        internal void Unsqueeze(Types.Clamping.TestParameters ClampingParameters)
        {
            try
            {
                if (m_ClampingSystemConnected && m_Param.IsClampEnabled)
                    m_Thread.StartSingle(Dummy => m_IOClamping.Unsqueeze(ClampingParameters));
            }
            catch (Exception ex)
            {
                ThrowFaultException(ComplexParts.Clamping, ex.ToString(), String.Format(@"{0}.{1}", GetType().Name, MethodBase.GetCurrentMethod().Name), false);
            }
        }

        internal void WriteResults(ResultItem Item, List<string> Errors)
        {
            try
            {
                ThreadPool.QueueUserWorkItem(delegate
                    {
                        try
                        {
                            SystemHost.Results.WriteResult(Item, Errors);
                        }
                        catch (Exception ex)
                        {
                            SystemHost.Journal.AppendLog(ComplexParts.Database, LogMessageType.Error, ex.Message);
                        }
                    });
            }
            catch (Exception ex)
            {
                ThrowFaultException(ComplexParts.Database, ex.ToString(), String.Format(@"{0}.{1}", GetType().Name, MethodBase.GetCurrentMethod().Name), false);
            }
        }

        internal void SaveProfiles(List<ProfileItem> profileItems)
        {
            try
            {
                 
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        try
                        {
                            SystemHost.Results.SaveProfiles(profileItems, Settings.Default.MMECode);
                        }
                        catch (Exception ex)
                        {
                            SystemHost.Journal.AppendLog(ComplexParts.Database, LogMessageType.Error, ex.Message);
                        }
                    });
                   
                
            }
            catch (Exception ex)
            {
                ThrowFaultException(ComplexParts.Database, ex.ToString(), String.Format(@"{0}.{1}", GetType().Name, MethodBase.GetCurrentMethod().Name), false);
            }
        }



        #region Standart API

        internal ushort ReadRegister(ComplexParts Device, ushort Address)
        {
            ushort res = 0;

            try
            {
                switch (Device)
                {
                    case ComplexParts.Gateway:
                        res = m_IOGateway.ReadRegister(Address);
                        break;
                    case ComplexParts.Commutation:
                        res = m_IOCommutation.ReadRegister(Address);
                        break;
                    case ComplexParts.CommutationEx:
                        res = m_IOCommutationEx.ReadRegister(Address);
                        break;
                    case ComplexParts.Gate:
                        res = m_IOGate.ReadRegister(Address);
                        break;
                    case ComplexParts.SL:
                        res = m_IOStls.ReadRegister(Address);
                        break;
                    case ComplexParts.BVT:
                        res = m_IOBvt.ReadRegister(Address);
                        break;
                    case ComplexParts.Clamping:
                        res = m_IOClamping.ReadRegister(Address);
                        break;
                    case ComplexParts.DvDt:
                        res = m_IOdVdt.ReadRegister(Address);
                        break;
                }
            }
            catch (Exception ex)
            {
                ThrowFaultException(Device, ex.Message, String.Format(@"{0}.{1}", GetType().Name, MethodBase.GetCurrentMethod().Name));
            }

            return res;
        }

        internal void WriteRegister(ComplexParts Device, ushort Address, ushort Value)
        {
            try
            {
                switch (Device)
                {
                    case ComplexParts.Gateway:
                        m_IOGateway.WriteRegister(Address, Value);
                        break;
                    case ComplexParts.Commutation:
                        m_IOCommutation.WriteRegister(Address, Value);
                        break;
                    case ComplexParts.CommutationEx:
                        m_IOCommutationEx.WriteRegister(Address, Value);
                        break;
                    case ComplexParts.Gate:
                        m_IOGate.WriteRegister(Address, Value);
                        break;
                    case ComplexParts.SL:
                        m_IOStls.WriteRegister(Address, Value);
                        break;
                    case ComplexParts.BVT:
                        m_IOBvt.WriteRegister(Address, Value);
                        break;
                    case ComplexParts.Clamping:
                        m_IOClamping.WriteRegister(Address, Value);
                        break;
                    case ComplexParts.DvDt:
                        m_IOdVdt.WriteRegister(Address, Value);
                        break;
                }
            }
            catch (Exception ex)
            {
                ThrowFaultException(Device, ex.Message, String.Format(@"{0}.{1}", GetType().Name, MethodBase.GetCurrentMethod().Name));
            }
        }

        internal void CallAction(ComplexParts Device, ushort Address)
        {
            try
            {
                switch (Device)
                {
                    case ComplexParts.Commutation:
                        m_IOCommutation.CallAction(Address);
                        break;
                    case ComplexParts.CommutationEx:
                        m_IOCommutationEx.CallAction(Address);
                        break;
                    case ComplexParts.Gate:
                        m_IOGate.CallAction(Address);
                        break;
                    case ComplexParts.SL:
                        m_IOStls.CallAction(Address);
                        break;
                    case ComplexParts.BVT:
                        m_IOBvt.CallAction(Address);
                        break;
                    case ComplexParts.Clamping:
                        m_IOClamping.CallAction(Address);
                        break;
                    case ComplexParts.DvDt:
                        m_IOdVdt.CallAction(Address);
                        break;
                }
            }
            catch (Exception ex)
            {
                ThrowFaultException(Device, ex.Message, String.Format(@"{0}.{1}", GetType().Name, MethodBase.GetCurrentMethod().Name));
            }
        }

        internal void ClearFault(ComplexParts Device)
        {
            try
            {
                switch (Device)
                {
                    case ComplexParts.Gateway:
                        m_IOGateway.ClearFault();
                        break;
                    case ComplexParts.Commutation:
                        m_IOCommutation.ClearFault();
                        break;
                    case ComplexParts.CommutationEx:
                        m_IOCommutationEx.ClearFault();
                        break;
                    case ComplexParts.Gate:
                        m_IOGate.ClearFault();
                        break;
                    case ComplexParts.SL:
                        m_IOStls.ClearFault();
                        break;
                    case ComplexParts.BVT:
                        m_IOBvt.ClearFault();
                        break;
                    case ComplexParts.Clamping:
                        m_IOClamping.ClearFault();
                        break;
                    case ComplexParts.DvDt:
                        m_IOdVdt.ClearFault();
                        break;
                }
            }
            catch (Exception ex)
            {
                ThrowFaultException(Device, ex.Message, String.Format(@"{0}.{1}", GetType().Name, MethodBase.GetCurrentMethod().Name));
            }
        }

        #endregion

        #region Calibration

        internal Types.Gate.CalibrationResultGate GatePulseCalibrationGate(ushort Current)
        {
            try
            {
                var res = m_IOGate.PulseCalibrationGate(Current);

                return new Types.Gate.CalibrationResultGate { Current = res.Item1, Voltage = res.Item2 };
            }
            catch (Exception ex)
            {
                ThrowFaultException(ComplexParts.Gate, ex.Message, String.Format(@"{0}.{1}", GetType().Name, MethodBase.GetCurrentMethod().Name));
                return new Types.Gate.CalibrationResultGate();
            }
        }

        internal ushort GatePulseCalibrationMain(ushort Current)
        {
            try
            {
                return m_IOGate.PulseCalibrationMain(Current);
            }
            catch (Exception ex)
            {
                ThrowFaultException(ComplexParts.Gate, ex.Message, String.Format(@"{0}.{1}", GetType().Name, MethodBase.GetCurrentMethod().Name));
                return 0;
            }
        }

        internal void GateWriteCalibrationParams(Types.Gate.CalibrationParameters Parameters)
        {
            try
            {
                m_IOGate.WriteCalibrationParams(Parameters);
            }
            catch (Exception ex)
            {
                ThrowFaultException(ComplexParts.Gate, ex.Message, String.Format(@"{0}.{1}", GetType().Name, MethodBase.GetCurrentMethod().Name));
            }
        }

        internal Types.Gate.CalibrationParameters GateReadCalibrationParams()
        {
            var parameters = new Types.Gate.CalibrationParameters();

            try
            {
                parameters = m_IOGate.ReadCalibrationParams();
            }
            catch (Exception ex)
            {
                ThrowFaultException(ComplexParts.Gate, ex.Message, String.Format(@"{0}.{1}", GetType().Name, MethodBase.GetCurrentMethod().Name));
            }

            return parameters;
        }


        internal void SLWriteCalibrationParams(Types.SL.CalibrationParameters Parameters)
        {
            try
            {
                m_IOStls.WriteCalibrationParams(Parameters);
            }
            catch (Exception ex)
            {
                ThrowFaultException(ComplexParts.SL, ex.Message, String.Format(@"{0}.{1}", GetType().Name, MethodBase.GetCurrentMethod().Name));
            }
        }

        internal Types.SL.CalibrationParameters SLReadCalibrationParams()
        {
            var parameters = new Types.SL.CalibrationParameters();

            try
            {
                parameters = m_IOStls.ReadCalibrationParams();
            }
            catch (Exception ex)
            {
                ThrowFaultException(ComplexParts.SL, ex.Message, String.Format(@"{0}.{1}", GetType().Name, MethodBase.GetCurrentMethod().Name));
            }

            return parameters;
        }

        internal void BVTWriteCalibrationParams(Types.BVT.CalibrationParams Parameters)
        {
            try
            {
                m_IOBvt.WriteCalibrationParams(Parameters);
            }
            catch (Exception ex)
            {
                ThrowFaultException(ComplexParts.BVT, ex.Message, String.Format(@"{0}.{1}", GetType().Name, MethodBase.GetCurrentMethod().Name));
            }
        }

        internal Types.BVT.CalibrationParams BVTReadCalibrationParams()
        {
            var parameters = new Types.BVT.CalibrationParams();

            try
            {
                parameters = m_IOBvt.ReadCalibrationParams();
            }
            catch (Exception ex)
            {
                ThrowFaultException(ComplexParts.BVT, ex.Message, String.Format(@"{0}.{1}", GetType().Name, MethodBase.GetCurrentMethod().Name));
            }

            return parameters;
        }

        internal void CSWriteCalibrationParams(Types.Clamping.CalibrationParams Parameters)
        {
            try
            {
                m_IOClamping.WriteCalibrationParams(Parameters);
            }
            catch (Exception ex)
            {
                ThrowFaultException(ComplexParts.Clamping, ex.Message, String.Format(@"{0}.{1}", GetType().Name, MethodBase.GetCurrentMethod().Name));
            }
        }

        internal Types.Clamping.CalibrationParams CSReadCalibrationParams()
        {
            var parameters = new Types.Clamping.CalibrationParams();

            try
            {
                parameters = m_IOClamping.ReadCalibrationParams();
            }
            catch (Exception ex)
            {
                ThrowFaultException(ComplexParts.Clamping, ex.Message, String.Format(@"{0}.{1}", GetType().Name, MethodBase.GetCurrentMethod().Name));
            }

            return parameters;
        }

        internal void DvDtWriteCalibrationParams(Types.dVdt.CalibrationParams Parameters)
        {
            try
            {
                m_IOdVdt.WriteCalibrationParams(Parameters);
            }
            catch (Exception ex)
            {
                ThrowFaultException(ComplexParts.DvDt, ex.Message, String.Format(@"{0}.{1}", GetType().Name, MethodBase.GetCurrentMethod().Name));
            }
        }

        internal Types.dVdt.CalibrationParams DvDtReadCalibrationParams()
        {
            var parameters = new Types.dVdt.CalibrationParams();

            try
            {
                parameters = m_IOdVdt.ReadCalibrationParams();
            }
            catch (Exception ex)
            {
                ThrowFaultException(ComplexParts.DvDt, ex.Message, String.Format(@"{0}.{1}", GetType().Name, MethodBase.GetCurrentMethod().Name));
            }

            return parameters;
        }

        #endregion

        private void ThrowFaultException(ComplexParts Device, string Exception, string Func, bool SwitchToFault = true)
        {
            if (SwitchToFault)
                m_State = DeviceState.Fault;

            var message = string.Format(Resources.Error_LogicContainer_Main_testing_state_exception, m_State, Exception);
            SystemHost.Journal.AppendLog(ComplexParts.Service, LogMessageType.Error, message);

            var fd = new FaultData { Device = Device, Message = message, TimeStamp = DateTime.Now };
            throw new FaultException<FaultData>(fd, Func);
        }


        
    }
}