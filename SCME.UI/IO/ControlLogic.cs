﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using SCME.Types;
using SCME.Types.BaseTestParams;
using SCME.Types.Clamping;
using SCME.Types.DatabaseServer;
using SCME.Types.Profiles;
using SCME.Types.SCTU;
using SCME.Types.SQL;
using SCME.Types.TOU;
using SCME.UI.CustomControl;
using SCME.UI.PagesUser;
using SCME.UI.Properties;
using SCME.UIServiceConfig.Properties;
using TestParameters = SCME.Types.Commutation.TestParameters;

namespace SCME.UI.IO
{
    public class ControlLogic
    {
        private const int REQUEST_DELAY_MS = 100;

        private const string CONTROL_SERVER_ENDPOINT_NAME = "SCME.Service.ExternalControl";
        private const string DATABASE_SERVER_ENDPOINT_NAME = "SCME.Service.DatabaseServer";

        private readonly ExternalControlCallbackHost m_CallbackHost;
        private readonly DispatcherTimer m_NetPingTimer;
        private ControlServerProxy m_ControlClient;
        public DatabaseCommunicationProxy DatabaseClient;
        private TypeCommon.InitParams m_InitParams;
        private volatile bool m_IsServerConnected, m_StopInit;

        public ControlLogic()
        {

            m_CallbackHost = new ExternalControlCallbackHost(this);

            m_NetPingTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 10) };
            m_NetPingTimer.Tick += NetPingTimerOnTick;
        }

        public (MyProfile profile, bool IsInMmeCode) SyncProfile(MyProfile profile)
        {
            return m_ControlClient.SyncProfile(profile);
        }

        public bool IsServerConnected
        {
            get { return m_IsServerConnected; }
            private set { m_IsServerConnected = value; }
        }

        public InitializationResponse GetStateService => m_ControlClient.IsInitialized();

        public bool IsStopButtonPressed
        {
            get { return m_ControlClient.GetButtonState(ComplexButtons.ButtonStop); }
        }

        public ExternalControlCallbackHost CallbackManager
        {
            get { return m_CallbackHost; }
        }

        public void Initialize(TypeCommon.InitParams Params)
        {
            m_StopInit = false;
            m_InitParams = Params;

            ThreadPool.QueueUserWorkItem(InitializeInternal, Params);
        }

        public void Deinitialize()
        {
            m_CallbackHost.AddCommonConnectionEvent(DeviceConnectionState.DisconnectionInProcess, "Disconnecting");

            try
            {
                DeinitializeInternal();

                m_CallbackHost.AddCommonConnectionEvent(DeviceConnectionState.DisconnectionSuccess, "Disconnected");
            }
            catch (Exception)
            {
                m_CallbackHost.AddCommonConnectionEvent(DeviceConnectionState.DisconnectionError, "Disconnection error");
            }
        }

        private void InitializeInternal(object Arg)
        {
            var param = (TypeCommon.InitParams)Arg;

            try
            {
                m_CallbackHost.AddCommonConnectionEvent(DeviceConnectionState.ConnectionInProcess, "Connecting");

                m_CallbackHost.AddDeviceConnectionEvent(ComplexParts.Service,
                                                       DeviceConnectionState.ConnectionInProcess,
                                                       "Connecting");

                Exception savedEx = null;
                var timeoutStamp = Environment.TickCount;

                while ((Environment.TickCount < timeoutStamp + param.TimeoutService) && !m_StopInit)
                {
                    try
                    {
                        if (m_ControlClient != null)
                            if (m_ControlClient.State == CommunicationState.Faulted)
                                m_ControlClient.Abort();
                            else
                                m_ControlClient.Close();

                        if (DatabaseClient != null)
                            if (DatabaseClient.State == CommunicationState.Faulted)
                                DatabaseClient.Abort();
                            else
                                DatabaseClient.Close();


                        m_ControlClient = new ControlServerProxy(new InstanceContext(m_CallbackHost), Settings.Default.ControlService);
                        DatabaseClient = new DatabaseCommunicationProxy(Settings.Default.DatabaseService);

                        m_NetPingTimer.Start();

                        m_ControlClient.Subscribe();
                        m_ControlClient.Initialize(param);

                        IsServerConnected = true;

                        m_CallbackHost.AddDeviceConnectionEvent(ComplexParts.Service, DeviceConnectionState.ConnectionSuccess, "Connected");

                        break;
                    }
                    catch (Exception ex)
                    {
                        savedEx = ex;
                    }

                    Thread.Sleep(REQUEST_DELAY_MS);
                }

                if (Environment.TickCount > timeoutStamp + param.TimeoutService)
                {
                    var str = "Unknown error trying to connect to service application";

                    if (savedEx != null)
                        str = savedEx.Message;

                    throw new Exception(str);
                }
            }
            catch (Exception ex)
            {
                m_CallbackHost.AddDeviceConnectionEvent(ComplexParts.Service,
                                                       DeviceConnectionState.ConnectionFailed,
                                                       ex.Message);
                m_CallbackHost.AddCommonConnectionEvent(DeviceConnectionState.ConnectionFailed, ex.Message);
            }
        }

        public void GateWriteCalibrationParameters(Types.Gate.CalibrationParameters Parameters)
        {
            try
            {
                m_ControlClient.GateWriteCalibrationParameters(Parameters);
            }
            catch (FaultException<FaultData> ex)
            {
                ShowFaultError("Calibration error", ex);
            }
            catch (CommunicationException ex)
            {
                ProcessCommunicationException(ex);
            }
            catch (Exception ex)
            {
                ProcessGeneralException(ex);
            }
        }

        public Types.Gate.CalibrationParameters GateReadCalibrationParameters()
        {
            var parameters = new Types.Gate.CalibrationParameters();
            try
            {
                parameters = m_ControlClient.GateReadCalibrationParameters();
            }
            catch (FaultException<FaultData> ex)
            {
                ShowFaultError("Calibration error", ex);
            }
            catch (CommunicationException ex)
            {
                ProcessCommunicationException(ex);
            }
            catch (Exception ex)
            {
                ProcessGeneralException(ex);
            }

            return parameters;
        }

        public Types.Gate.CalibrationResultGate GatePulseCalibrationGate(ushort Current)
        {
            try
            {
                return m_ControlClient.GatePulseCalibrationGate(Current);
            }
            catch (FaultException<FaultData> ex)
            {
                ShowFaultError("Calibration error", ex);
                return new Types.Gate.CalibrationResultGate();
            }
            catch (CommunicationException ex)
            {
                ProcessCommunicationException(ex);
                return new Types.Gate.CalibrationResultGate();
            }
            catch (Exception ex)
            {
                ProcessGeneralException(ex);
                return new Types.Gate.CalibrationResultGate();
            }
        }

        public ushort GatePulseCalibrationMain(ushort Current)
        {
            try
            {
                return m_ControlClient.GatePulseCalibrationMain(Current);
            }
            catch (FaultException<FaultData> ex)
            {
                ShowFaultError("Calibration error", ex);
                return 0;
            }
            catch (CommunicationException ex)
            {
                ProcessCommunicationException(ex);
                return 0;
            }
            catch (Exception ex)
            {
                ProcessGeneralException(ex);
                return 0;
            }
        }

        private void DeinitializeInternal()
        {
            Exception savedEx = null;

            try
            {
                m_StopInit = true;
                m_NetPingTimer.Stop();

                m_CallbackHost.AddDeviceConnectionEvent(ComplexParts.Service, DeviceConnectionState.DisconnectionInProcess,
                                                       "Disconnecting");

                try
                {
                    if (m_ControlClient != null)
                    {
                        try
                        {
                            m_ControlClient.Deinitialize();
                            m_ControlClient.Unsubscribe();
                        }
                        finally
                        {
                            if (m_ControlClient.State == CommunicationState.Faulted)
                                m_ControlClient.Abort();
                            else
                                m_ControlClient.Close();

                            m_ControlClient = null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    savedEx = ex;
                    ProcessGeneralException(ex);
                }

                try
                {
                    if (m_ControlClient != null)
                    {
                        if (DatabaseClient.State == CommunicationState.Faulted)
                            DatabaseClient.Abort();
                        else
                            DatabaseClient.Close();

                        DatabaseClient = null;
                    }
                }
                catch (Exception ex)
                {
                    savedEx = ex;
                    ProcessGeneralException(ex);
                }

                if (savedEx != null)
                    throw savedEx;

                m_CallbackHost.AddDeviceConnectionEvent(ComplexParts.Service, DeviceConnectionState.DisconnectionSuccess,
                                                       "Disconnected");
            }
            finally
            {
                IsServerConnected = false;
            }
        }

        private static void ShowFaultError(string Caption, FaultException<FaultData> Ex)
        {
            var dw = new DialogWindow(Caption, $"{Ex.ToString()}\r\n{Ex.Detail.Message}");
            dw.ButtonConfig(DialogWindow.EbConfig.OK);
            dw.ShowDialog();
        }

        private void ProcessGeneralException(Exception Ex)
        {
            m_NetPingTimer.Stop();

            m_CallbackHost.AddExceptionEvent(ComplexParts.Service,
                                            string.Format("Exception - {0}", Ex.ToString()));
        }

        public int? ReadDeviceRTClass(string devCode, string profileName)
        {
            int? result = null;

            using (var centralDbClient = new CentralDatabaseServiceClient(Settings.Default.CentralDatabaseService))
            {
                try
                {
                    result = centralDbClient.ReadDeviceRTClass(devCode, profileName);
                }
                catch (FaultException<FaultData> ex)
                {
                    ShowFaultError("Read device RT class error", ex);
                }
                catch (CommunicationException ex)
                {
                    throw ex;
                    //ProcessCommunicationException(ex);
                }
            }

            return result;
        }

        public int? ReadDeviceClass(string devCode, string profileName)
        {
            int? result = null;

            using (var centralDbClient = new CentralDatabaseServiceClient(Settings.Default.CentralDatabaseService))
            {
                try
                {
                    result = centralDbClient.ReadDeviceClass(devCode, profileName);
                }
                catch (FaultException<FaultData> ex)
                {
                    ShowFaultError("Read device class error", ex);
                }
                catch (CommunicationException ex)
                {
                    throw ex;
                    //ProcessCommunicationException(ex);
                }
            }

            return result;
        }
        
        private void ProcessCommunicationException(CommunicationException Ex)
        {
            Cache.Main.ConnectionLabelVisible = true;

            try
            {
                var dw = new DialogWindow("Critical error - restart UI", Ex.ToString());
                dw.ButtonConfig(DialogWindow.EbConfig.OK);
                dw.ShowDialog();

                m_NetPingTimer.Stop();

                Thread.Sleep(1000);

                Exception savedEx = null;
                var timeoutStamp = Environment.TickCount;

                while ((Environment.TickCount < timeoutStamp + m_InitParams.TimeoutService) && !m_StopInit)
                {
                    try
                    {
                        if (m_ControlClient != null)
                            if (m_ControlClient.State == CommunicationState.Faulted)
                                m_ControlClient.Abort();
                            else
                                m_ControlClient.Close();

                        if (DatabaseClient != null)
                            if (DatabaseClient.State == CommunicationState.Faulted)
                                DatabaseClient.Abort();
                            else
                                DatabaseClient.Close();

                        m_ControlClient = new ControlServerProxy(new InstanceContext( m_CallbackHost), Settings.Default.ControlService);
                        DatabaseClient = new DatabaseCommunicationProxy(Settings.Default.DatabaseService);

                        m_ControlClient.Subscribe();

                        break;
                    }
                    catch (Exception ex)
                    {
                        savedEx = ex;
                    }

                    Thread.Sleep(REQUEST_DELAY_MS);
                }

                if (savedEx != null)
                {
                    ProcessGeneralException(savedEx);
                }
                else
                {
                    var initState = m_ControlClient.IsInitialized();

                    if ((initState.InitializationResult & InitializationResult.ModulesInitialized) == InitializationResult.None)
                        Cache.Main.RestartRoutine(null, null);
                    else
                        m_NetPingTimer.Start();
                }
            }
            finally
            {
                Cache.Main.ConnectionLabelVisible = false;
            }
        }

        private void NetPingTimerOnTick(object Sender, EventArgs Args)
        {
            if (!IsServerConnected)
                return;

            try
            {
                //m_ControlClient.Check();
                //DatabaseClient.Check();
            }
            catch (FaultException<FaultData>)
            {
            }
            catch (CommunicationException ex)
            
            
            
            {
                ProcessCommunicationException(ex);
            }
            catch (Exception ex)
            {
                ProcessGeneralException(ex);
            }
        }


        #region ExternalControl members

        public bool Start(Types.Gate.TestParameters ParametersGate, Types.VTM.TestParameters ParametersVtm,
                          Types.BVT.TestParameters ParametersBvt, Types.ATU.TestParameters ParametersAtu, Types.QrrTq.TestParameters ParametersQrrTq, Types.IH.TestParameters ParametersIH, Types.RCC.TestParameters ParametersRCC, Types.Commutation.TestParameters ParametersCommutation, Types.Clamping.TestParameters ParametersClamping, Types.TOU.TestParameters ParametersTOU, bool SkipSC = false)
        {
            if (!IsServerConnected)
                return false;

            try
            {
                if (!ParametersGate.IsEnabled && !ParametersVtm.IsEnabled && !ParametersBvt.IsEnabled && !ParametersAtu.IsEnabled && !ParametersQrrTq.IsEnabled && !ParametersIH.IsEnabled && !ParametersRCC.IsEnabled && !ParametersTOU.IsEnabled)
                {
                    var dw = new DialogWindow(Resources.Information,
                                              Resources.CanNotStartTest + Environment.NewLine +
                                              Resources.AllUnitsAreDisabled);
                    dw.ButtonConfig(DialogWindow.EbConfig.OK);
                    dw.ShowDialog();

                    return false;
                }

                bool result = false;

                var isStopButtonPressed = IsStopButtonPressed;
                var isSafetyCircutClosed = m_ControlClient.GetButtonState(ComplexButtons.ButtonSC1) || SkipSC || Settings.Default.IgnoreSC;
                bool isClampSlideOut = Settings.Default.ClampWithSlidingDevice && !m_ControlClient.GetButtonState(ComplexButtons.ClampSlidingSensor);

                if (isStopButtonPressed || !isSafetyCircutClosed || isClampSlideOut)
                {
                    var message = Resources.CanNotStartTest + Environment.NewLine;

                    if (isStopButtonPressed)
                        message += string.Format("{0}\n{1}\n", Resources.StopButtonIsPressed, Resources.PullStopButton);

                    //если защитная цепь разомкнута и установлена механическая шторка безопасности - выдаём пользователю сообщение об этом
                    if (!isSafetyCircutClosed)
                        if (m_ControlClient.GetSafetyType() == ComplexSafety.Mechanical)
                            message += string.Format("{0}\n{1}\n", Resources.SafetyCircuitIsOpen, Resources.CloseSafetyHood);

                    if (isClampSlideOut)
                        message += string.Format("{0}\n{1}\n", Resources.ClampSlidingDeviceIsOut, Resources.MoveClampSlidingDeviceToHome);

                    var dw = new DialogWindow(Resources.Information, message);

                    dw.ButtonConfig(DialogWindow.EbConfig.OK);
                    dw.ShowDialog();

                    return false;
                }

                result = m_ControlClient.Start(ParametersGate, ParametersVtm, ParametersBvt, ParametersAtu, ParametersQrrTq, ParametersIH, ParametersRCC, ParametersCommutation, ParametersClamping, ParametersTOU);

                return result;

            }
            catch (CommunicationException ex)
            {
                ProcessCommunicationException(ex);
            }
            catch (Exception ex)
            {
                ProcessGeneralException(ex);
            }

            return false;
        }

        public bool Start(TestParameters paramsComm, Types.Clamping.TestParameters paramsClamp, List<BaseTestParametersAndNormatives> parameters, bool SkipSC = false)
        {
            if (!IsServerConnected)
                return false;

            try
            {
                if (parameters.Count == 0)
                {
                    var dw = new DialogWindow(Resources.Information,
                                              Resources.CanNotStartTest + Environment.NewLine +
                                              Resources.AllUnitsAreDisabled);
                    dw.ButtonConfig(DialogWindow.EbConfig.OK);
                    dw.ShowDialog();

                    return false;
                }

                bool result = false;

                var isStopButtonPressed = IsStopButtonPressed;
                var isSafetyCircutClosed = m_ControlClient.GetButtonState(ComplexButtons.ButtonSC1) || SkipSC || Settings.Default.IgnoreSC;
                bool isClampSlideOut = (Settings.Default.ClampWithSlidingDevice && !m_ControlClient.GetButtonState(ComplexButtons.ClampSlidingSensor));

                if (isStopButtonPressed || !isSafetyCircutClosed || isClampSlideOut)
                {
                    var message = Resources.CanNotStartTest + Environment.NewLine;

                    if (isStopButtonPressed)
                        message += string.Format("{0}\n{1}\n", Resources.StopButtonIsPressed, Resources.PullStopButton);

                    //если защитная цепь разомкнута и установлена механическая шторка безопасности - выдаём пользователю сообщение об этом
                    if (!isSafetyCircutClosed)
                        if (m_ControlClient.GetSafetyType() == ComplexSafety.Mechanical)
                            message += string.Format("{0}\n{1}\n", Resources.SafetyCircuitIsOpen, Resources.CloseSafetyHood);

                    if (isClampSlideOut)
                        message += string.Format("{0}\n{1}\n", Resources.ClampSlidingDeviceIsOut, Resources.MoveClampSlidingDeviceToHome);

                    var dw = new DialogWindow(Resources.Information, message);

                    dw.ButtonConfig(DialogWindow.EbConfig.OK);
                    dw.ShowDialog();

                    return false;
                }

                result = m_ControlClient.StartDynamic(paramsComm, paramsClamp, parameters.OfType<Types.Gate.TestParameters>().Where(t => t.IsEnabled).ToArray(), parameters.OfType<Types.VTM.TestParameters>().Where(t => t.IsEnabled).ToArray(), parameters.OfType<Types.BVT.TestParameters>().Where(t => t.IsEnabled).ToArray(), parameters.OfType<Types.dVdt.TestParameters>().Where(t => t.IsEnabled).ToArray(), parameters.OfType<Types.ATU.TestParameters>().Where(t => t.IsEnabled).ToArray(), parameters.OfType<Types.QrrTq.TestParameters>().Where(t => t.IsEnabled).ToArray(), parameters.OfType<SctuTestParameters>().ToArray(), parameters.OfType<Types.TOU.TestParameters>().Where(t => t.IsEnabled).ToArray());

                return result;
            }
            catch (CommunicationException ex)
            {
                ProcessCommunicationException(ex);
            }
            catch (Exception ex)
            {
                ProcessGeneralException(ex);
            }

            return false;
        }

        public bool StartHeating(int temperature)
        {
            if (!IsServerConnected)
                return false;

            try
            {
                return m_ControlClient.StartHeating(temperature);
            }
            catch (CommunicationException ex)
            {
                ProcessCommunicationException(ex);
            }
            catch (Exception ex)
            {
                ProcessGeneralException(ex);
                throw;
            }
            return false;
        }

        public void StopHeating()
        {
            if (!IsServerConnected)
                return;

            try
            {
                m_ControlClient.StopHeating();
            }
            catch (CommunicationException ex)
            {
                ProcessCommunicationException(ex);
            }
            catch (Exception ex)
            {
                ProcessGeneralException(ex);
                throw;
            }
        }

        public void Stop()
        {
            if (!IsServerConnected)
                return;

            try
            {
                m_ControlClient.Stop();
            }
            catch (CommunicationException ex)
            {
                ProcessCommunicationException(ex);
            }
            catch (Exception ex)
            {
                ProcessGeneralException(ex);
            }
        }

        public void StopByButtonStop()
        {
            if (!IsServerConnected)
                return;

            try
            {
                m_ControlClient.StopByButtonStop();
            }
            catch (CommunicationException ex)
            {
                ProcessCommunicationException(ex);
            }
            catch (Exception ex)
            {
                ProcessGeneralException(ex);
            }
        }

        public bool GetButtonState(ComplexButtons Button)
        {
            var res = false;

            try
            {
                res = m_ControlClient.GetButtonState(Button);
            }
            catch (FaultException<FaultData>)
            {
                throw;
            }
            catch (CommunicationException ex)
            {
                ProcessCommunicationException(ex);
            }
            catch (Exception ex)
            {
                ProcessGeneralException(ex);
            }

            return res;
        }

        public void ClearSafetyTrig()
        {
            if (!IsServerConnected)
                return;

            try
            {
                m_ControlClient.ClearSafetyTrig();
            }
            catch (CommunicationException ex)
            {
                ProcessCommunicationException(ex);
            }
            catch (Exception ex)
            {
                ProcessGeneralException(ex);
            }
        }

        public void SafetySystemOn()
        {
            if (!IsServerConnected)
                return;

            try
            {
                m_ControlClient.SafetySystemOn();
            }
            catch(FaultException<FaultData> ex)
            {
                MessageBox.Show(ex.Detail.Message);
            }
            catch (CommunicationException ex)
            {
                ProcessCommunicationException(ex);
            }
            catch (Exception ex)
            {
                ProcessGeneralException(ex);
            }
        }

        public void SetSafetyMode(SafetyMode safetyMode)
        {
            if (!IsServerConnected)
                return;

            try
            {
                m_ControlClient.SetSafetyMode(safetyMode);
                Cache.Main.VM.SafetyMode = safetyMode;
            }
            catch (CommunicationException ex)
            {
                ProcessCommunicationException(ex);
            }
            catch (Exception ex)
            {
                ProcessGeneralException(ex);
            }
        }

        public void SetUserWorkMode(UserWorkMode userWorkMode)
        {
            if (!IsServerConnected)
                return;

            try
            {
                m_ControlClient.SetUserWorkMode(userWorkMode);
            }
            catch (CommunicationException ex)
            {
                ProcessCommunicationException(ex);
            }
            catch (Exception ex)
            {
                ProcessGeneralException(ex);
            }
        }

        public void SafetySystemOff()
        {
            if (!IsServerConnected)
                return;

            try
            {
                m_ControlClient.SafetySystemOff();
                Cache.Main.VM.SafetyMode = SafetyMode.Disabled;
            }
            catch (CommunicationException ex)
            {
                ProcessCommunicationException(ex);
            }
            catch (Exception ex)
            {
                ProcessGeneralException(ex);
            }
        }

        public string NotReadyDevicesToStart()
        {
            string res = "";

            if (!IsServerConnected)
                return "Server is not connected";

            try
            {
                res = m_ControlClient.NotReadyDevicesToStart();
            }
            catch (CommunicationException ex)
            {
                ProcessCommunicationException(ex);
            }
            catch (Exception ex)
            {
                ProcessGeneralException(ex);
            }

            return res;
        }

        public string NotReadyDevicesToStartDynamic()
        {
            string res = "";

            if (!IsServerConnected)
                return "Server is not connected";

            try
            {
                res = m_ControlClient.NotReadyDevicesToStartDynamic();
            }
            catch (CommunicationException ex)
            {
                ProcessCommunicationException(ex);
            }
            catch (Exception ex)
            {
                ProcessGeneralException(ex);
            }

            return res;
        }

        public ushort ReadRegister(ComplexParts Device, ushort Address)
        {
            ushort res = 0;

            try
            {
                res = m_ControlClient.ReadRegister(Device, Address);
            }
            catch (FaultException<FaultData>)
            {
                throw;
            }
            catch (CommunicationException ex)
            {
                ProcessCommunicationException(ex);
            }
            catch (Exception ex)
            {
                ProcessGeneralException(ex);
            }

            return res;
        }

        public void WriteRegister(ComplexParts Device, ushort Address, ushort Value)
        {
            try
            {
                m_ControlClient.WriteRegister(Device, Address, Value);
            }
            catch (FaultException<FaultData>)
            {
                throw;
            }
            catch (CommunicationException ex)
            {
                ProcessCommunicationException(ex);
            }
            catch (Exception ex)
            {
                ProcessGeneralException(ex);
            }
        }

        public void CallAction(ComplexParts Device, ushort Address)
        {
            try
            {
                m_ControlClient.CallAction(Device, Address);
            }
            catch (FaultException<FaultData>)
            {
                throw;
            }
            catch (CommunicationException ex)
            {
                ProcessCommunicationException(ex);
            }
            catch (Exception ex)
            {
                ProcessGeneralException(ex);
            }
        }

        public void WriteResultServer(ResultItem item, List<string> errors)
        {
            using (var centralDbClient = new CentralDatabaseServiceClient(Settings.Default.CentralDatabaseService))
            {
                centralDbClient.SaveResults(item, errors);
                //try
                //{
                //    centralDbClient.SaveResults(item, errors);
                //}
                //catch (FaultException<FaultData> ex)
                //{
                //    ShowFaultError("Write database error", ex);
                //}
                //catch (CommunicationException ex)
                //{
                //    var dw = new DialogWindow("Write database error", ex.ToString());
                //    dw.ButtonConfig(DialogWindow.EbConfig.OK);
                //    dw.ShowDialog();
                //}
            }
        }

        public void WriteJournal(ComplexParts device, LogMessageType type, DateTime dateTime, string message)
        {
            m_ControlClient.WriteJournal(device, type, dateTime, message);
        }

        public void WriteResultLocal(ResultItem item, List<string> errors)
        {
            try
            {
                m_ControlClient.WriteResults(item, errors);
            }
            catch (FaultException<FaultData> ex)
            {
                ShowFaultError("Write database error", ex);
            }
            catch (CommunicationException ex)
            {
                ProcessCommunicationException(ex);
            }
            catch (Exception ex)
            {
                ProcessGeneralException(ex);
            }
        }

        public List<ProfileForSqlSelect> SaveProfilesToServer(List<ProfileItem> profileItems)
        {
            using (var centralDbClient = new CentralDatabaseServiceClient(Settings.Default.CentralDatabaseService))
            {
                try
                {
                    return centralDbClient.SaveProfilesFromMme(profileItems, Cache.Main.VM.MmeCode);
                }
                catch (FaultException<FaultData> ex)
                {
                    ShowFaultError("Write database error", ex);
                }
                catch (CommunicationException ex)
                {
                    ProcessCommunicationException(ex);
                }
            }
            return null;

        }

        public List<ProfileForSqlSelect> SaveProfilesToLocal(List<ProfileItem> profileItems)
        {
            try
            {
                return m_ControlClient.SaveProfiles(profileItems);
            }
            catch (FaultException<FaultData> ex)
            {
                ShowFaultError("Write database error", ex);
            }
            catch (CommunicationException ex)
            {
                ProcessCommunicationException(ex);
            }
            catch (Exception ex)
            {
                ProcessGeneralException(ex);
            }
            return null;
        }

        public void Squeeze(Types.Clamping.TestParameters ParametersClamping)
        {
            try
            {
                m_ControlClient.SqueezeClamping(ParametersClamping);
            }
            catch (FaultException<FaultData> ex)
            {
                ShowFaultError("Clamping error", ex);
            }
            catch (CommunicationException ex)
            {
                ProcessCommunicationException(ex);
            }
            catch (Exception ex)
            {
                ProcessGeneralException(ex);
            }
        }

        public void Unsqueeze(Types.Clamping.TestParameters ParametersClamping)
        {
            try
            {
                m_ControlClient.UnsqueezeClamping(ParametersClamping);
            }
            catch (FaultException<FaultData> ex)
            {
                ShowFaultError("Clamping error", ex);
            }
            catch (CommunicationException ex)
            {
                ProcessCommunicationException(ex);
            }
            catch (Exception ex)
            {
                ProcessGeneralException(ex);
            }
        }

        public bool RequestRemotePrinting(string GroupName, string CustomerName, string DeviceType,
            ReportSelectionPredicate Predicate)
        {
            try
            {
                return m_ControlClient.RequestRemotePrinting(GroupName, CustomerName, DeviceType, Predicate);
            }
            catch (FaultException<FaultData> ex)
            {
                ShowFaultError("Printing error", ex);
            }
            catch (CommunicationException ex)
            {
                ProcessCommunicationException(ex);
            }
            catch (Exception ex)
            {
                ProcessGeneralException(ex);
            }

            return false;
        }

        #endregion

        #region DatabaseServer members

        public List<LogItem> ReadLogsFromLocal(long tail, long count)
        {
            try
            {
                return DatabaseClient.ReadLogs(tail, count);
            }
            catch (FaultException<FaultData> ex)
            {
                ShowFaultError("Read database error", ex);
                return new List<LogItem>();
            }
            catch (CommunicationException ex)
            {
                ProcessCommunicationException(ex);
                return new List<LogItem>();
            }
            catch (Exception ex)
            {
                ProcessGeneralException(ex);
                return new List<LogItem>();
            }
        }

        public List<string> ReadGroupsFromLocal(DateTime? @from, DateTime? to)
        {
            try
            {
                return DatabaseClient.ReadGroups(@from, to);
            }
            catch (FaultException<FaultData> ex)
            {
                ShowFaultError("Read database error", ex);
                return new List<string>();
            }
            catch (CommunicationException ex)
            {
                ProcessCommunicationException(ex);
                return new List<string>();
            }
            catch (Exception ex)
            {
                ProcessGeneralException(ex);
                return new List<string>();
            }
        }

        public List<string> ReadGroupsFromServer(DateTime? @from, DateTime? to)
        {
            using (var centralDbClient = new CentralDatabaseServiceClient(Settings.Default.CentralDatabaseService))
            {
                try
                {
                    return centralDbClient.GetGroups(@from, to);
                }
                catch (FaultException<FaultData> ex)
                {
                    ShowFaultError("Read database error", ex);
                    return new List<string>();
                }
                catch (CommunicationException ex)
                {
                    ProcessCommunicationException(ex);
                    return new List<string>();
                }
            }

        }

        public List<DeviceItem> ReadDevicesFromLocal(string @group)
        {
            try
            {
                return DatabaseClient.ReadDevices(@group);
            }
            catch (FaultException<FaultData> ex)
            {
                ShowFaultError("Read database error", ex);
                return new List<DeviceItem>();
            }
            catch (CommunicationException ex)
            {
                ProcessCommunicationException(ex);
                return new List<DeviceItem>();
            }
            catch (Exception ex)
            {
                ProcessGeneralException(ex);
                return new List<DeviceItem>();
            }
        }

        public List<DeviceItem> ReadDevicesFromServer(string @group)
        {
            using (var centralDbClient = new CentralDatabaseServiceClient(Settings.Default.CentralDatabaseService))
            {
                try
                {
                    return centralDbClient.GetDevices(@group);
                }
                catch (FaultException<FaultData> ex)
                {
                    ShowFaultError("Read database error", ex);
                    return null;
                }
                catch (CommunicationException ex)
                {
                    ProcessCommunicationException(ex);
                    return new List<DeviceItem>();
                }
            }
        }

        public List<ParameterItem> ReadDeviceParametersFromLocal(long internalId)
        {
            try
            {
                return DatabaseClient.ReadDeviceParameters(internalId);
            }
            catch (FaultException<FaultData> ex)
            {
                ShowFaultError("Read database error", ex);
                return new List<ParameterItem>();
            }
            catch (CommunicationException ex)
            {
                ProcessCommunicationException(ex);
                return new List<ParameterItem>();
            }
            catch (Exception ex)
            {
                ProcessGeneralException(ex);
                return new List<ParameterItem>();
            }
        }

        public List<ConditionItem> ReadDeviceConditionsFromLocal(long internalId)
        {
            try
            {
                return DatabaseClient.ReadDeviceConditions(internalId);
            }
            catch (FaultException<FaultData> ex)
            {
                ShowFaultError("Read database error", ex);
                return new List<ConditionItem>();
            }
            catch (CommunicationException ex)
            {
                ProcessCommunicationException(ex);
                return new List<ConditionItem>();
            }
            catch (Exception ex)
            {
                ProcessGeneralException(ex);
                return new List<ConditionItem>();
            }
        }

        #endregion

        public List<ProfileItem> GetProfilesFromServerDb(string mmeCode, out bool serviceConnected)
        {
            serviceConnected = true;

            List<ProfileItem> profiles = null;
            using (var centralDbClient = new CentralDatabaseServiceClient(Settings.Default.CentralDatabaseService))
            {
                try
                {
                    profiles = centralDbClient.GetProfileItemsByMme(mmeCode);
                }
                catch (FaultException<FaultData> ex)
                {
                    ShowFaultError("Get profiles from service", ex);
                }
                catch (CommunicationException ex)
                {
                    ProcessCommunicationException(ex);
                    serviceConnected = false;
                }
                catch (Exception ex)
                {
                    ProcessGeneralException(ex);
                    serviceConnected = false;
                }
            }

            return profiles;

        }

        public List<ProfileItem> GetProfilesFromLocalDb(string mmeCode, out bool serviceConnected)
        {
            serviceConnected = true;

            var profiles = new List<ProfileItem>();
            try
            {
                profiles = DatabaseClient.GetProfileItemsByMmeCode(mmeCode);
            }
            catch (FaultException<FaultData> ex)
            {
                ShowFaultError("Get profiles from service", ex);
            }
            catch (CommunicationException ex)
            {
                ProcessCommunicationException(ex);
                serviceConnected = false;
            }
            catch (Exception ex)
            {
                ProcessGeneralException(ex);
                serviceConnected = false;
            }

            return profiles;
        }

        public ProfileItem GetProfileFromServerDb(string profName, string mmmeCode, ref bool Found)
        {
            //получение профиля с именем profName, связанного с mmmeCode от SCME.DatabaseServer без использования SCME.Service
            ProfileItem Result = null;

            using (var centralDbClient = new CentralDatabaseServiceClient(Settings.Default.CentralDatabaseService))
            {
                try
                {
                    Result = centralDbClient.GetProfileByProfName(profName, mmmeCode, ref Found);
                }
                catch (FaultException<FaultData> ex)
                {
                    ShowFaultError("Get profile by name from SCME.DatabaseServer", ex);
                }
                catch (CommunicationException ex)
                {
                    ProcessCommunicationException(ex);
                }
            }

            return Result;
        }

        public void GetState()
        {
            if(m_ControlClient == null)
                return;
            var q = m_ControlClient.IsInitialized();
        }
    }

    public class ExternalControlCallbackHost : IClientCallback
    {
        private readonly QueueWorker m_QueueWorker;

        public ExternalControlCallbackHost(ControlLogic NetProxy)
        {
            m_QueueWorker = new QueueWorker(NetProxy);
            m_QueueWorker.Start();
        }

        public void BVTUdsmUrsmDirectHandler(DeviceState State, Types.BVT.TestResults Result)
        {
            m_QueueWorker.AddBvtUdsmUrsmDirectEvent(State, Result);
        }

        public void BVTUdsmUrsmReverseHandler(DeviceState State, Types.BVT.TestResults Result)
        {
            m_QueueWorker.AddBvtUdsmUrsmReverseEvent(State, Result);
        }
        
        internal void AddCommonConnectionEvent(DeviceConnectionState State, string Message)
        {
            m_QueueWorker.AddCommonConnectionEvent(State, Message);
        }

        internal void AddDeviceConnectionEvent(ComplexParts Part, DeviceConnectionState State, string Message)
        {
            m_QueueWorker.AddDeviceConnectionEvent(Part, State, Message);
        }

        internal void AddExceptionEvent(ComplexParts Part, string Message)
        {
            m_QueueWorker.AddExceptionEvent(Part, Message);
        }

        #region Callbacks

        public void CommonConnectionHandler(DeviceConnectionState State, string Message)
        {
            m_QueueWorker.AddCommonConnectionEvent(State, Message);
        }

        public void DeviceConnectionHandler(ComplexParts Device, DeviceConnectionState State, string Message)
        {
            m_QueueWorker.AddDeviceConnectionEvent(Device, State, Message);
        }

        public void TestAllHandler(DeviceState State, string Message)
        {
            m_QueueWorker.AddTestAllEvent(State, Message);
        }

        public void ExceptionHandler(ComplexParts Device, string Message)
        {
            m_QueueWorker.AddExceptionEvent(Device, Message);
        }

        public void ClampingTemperatureHandler(HeatingChannel channel, int temeprature)
        {
            switch (channel)
            {
                case (HeatingChannel.Top):
                    m_QueueWorker.AddClampingTopTempEvent(temeprature);
                    break;

                case (HeatingChannel.Bottom):
                    m_QueueWorker.AddClampingBottomTempEvent(temeprature);
                    break;

                case (HeatingChannel.Setting):
                    m_QueueWorker.AddClampingSettingTemperatureEvent(temeprature);
                    break;
            }
        }

        public void SctuHandler(SctuHwState state, SctuTestResults results)
        {
        }

        public void DbSyncState(DeviceConnectionState state, string message)
        {
            m_QueueWorker.DbSyncState(state, message);
        }

        public void GatewayButtonPressHandler(ComplexButtons Button, bool State)
        {
            m_QueueWorker.AddButtonPressedEvent(Button, State);
        }

        public void SafetyHandler(bool Alarm, ComplexSafety SafetyType, ComplexButtons Button)
        {
            m_QueueWorker.AddSafetyHandlerEvent(Alarm, SafetyType, Button);
        }

        public void StopHandler()
        {
            m_QueueWorker.AddStopEvent();
        }

        public void SyncDBAreProcessedHandler()
        {
            throw  new NotImplementedException();
            //m_QueueWorker.AddSyncDbAreProcessedEvent();
        }

        public void GatewayNotificationHandler(Types.Gateway.HWWarningReason Warning, Types.Gateway.HWFaultReason Fault,
                                               Types.Gateway.HWDisableReason Disable)
        {
            if (Warning != Types.Gateway.HWWarningReason.None)
                m_QueueWorker.AddGatewayWarningEvent(Warning);

            if (Fault != Types.Gateway.HWFaultReason.None)
                m_QueueWorker.AddGatewayFaultEvent(Fault);
        }

        public void CommutationSwitchHandler(Types.Commutation.CommutationMode SwitchState)
        {
            m_QueueWorker.AddCommutationSwitchEvent(SwitchState);
        }

        public void CommutationNotificationHandler(Types.Commutation.HWWarningReason Warning,
                                                   Types.Commutation.HWFaultReason Fault)
        {
            if (Warning != Types.Commutation.HWWarningReason.None)
                m_QueueWorker.AddCommutationWarningEvent(Warning);

            if (Fault != Types.Commutation.HWFaultReason.None)
                m_QueueWorker.AddCommutationFaultEvent(Fault);
        }

        public void GateAllHandler(DeviceState State)
        {
            m_QueueWorker.AddGateAllEvent(State);
        }

        public void GateKelvinHandler(DeviceState state, bool isKelvinOk, IList<short> array, long testTypeId)
        {
            m_QueueWorker.AddGateKelvinEvent(state, isKelvinOk, array, testTypeId);
        }

        public void GateResistanceHandler(DeviceState state, float resistance, long testTypeId)
        {
            m_QueueWorker.AddGateResistanceEvent(state, resistance, testTypeId);
        }

        public void GateIgtVgtHandler(DeviceState state, float igt, float vgt, IList<short> arrayI, IList<short> arrayV, long testTypeId)
        {
            m_QueueWorker.AddGateGateEvent(state, igt, vgt, arrayI, arrayV, testTypeId);
        }

        public void GateIhHandler(DeviceState state, float ih, IList<short> array, long testTypeId)
        {
            m_QueueWorker.AddGateIhEvent(state, ih, array, testTypeId);
        }

        public void GateIlHandler(DeviceState state, float il, long testTypeId)
        {
            m_QueueWorker.AddGateIlEvent(state, il, testTypeId);
        }

        public void GateNotificationHandler(Types.Gate.HWProblemReason Problem, Types.Gate.HWWarningReason Warning,
                                            Types.Gate.HWFaultReason Fault,
                                            Types.Gate.HWDisableReason Disable)
        {
            if (Warning != Types.Gate.HWWarningReason.None)
                m_QueueWorker.AddGateWarningEvent(Warning);

            if (Problem != Types.Gate.HWProblemReason.None)
                m_QueueWorker.AddGateProblemEvent(Problem);

            if (Fault != Types.Gate.HWFaultReason.None)
                m_QueueWorker.AddGateFaultEvent(Fault);

            // if (Disable != Types.Gate.HWDisableReason.None)
            //      m_QueueWorker.AddVtmFaultEvent(Disable);
        }

        public void SLHandler(DeviceState state, Types.VTM.TestResults result)
        {
            m_QueueWorker.AddSLEvent(state, result);
        }

        public void SLNotificationHandler(Types.VTM.HWProblemReason Problem, Types.VTM.HWWarningReason Warning,
                                           Types.VTM.HWFaultReason Fault,
                                           Types.VTM.HWDisableReason Disable)
        {
            if (Warning != Types.VTM.HWWarningReason.None)
                m_QueueWorker.AddSLWarningEvent(Warning);

            if (Problem != Types.VTM.HWProblemReason.None)
                m_QueueWorker.AddSLProblemEvent(Problem);

            if (Fault != Types.VTM.HWFaultReason.None)
                m_QueueWorker.AddSLFaultEvent(Fault);

            // if (Disable != Types.VTM.HWDisableReason.None)
            //      m_QueueWorker.AddVtmFaultEvent(Disable);
        }

        public void BVTAllHandler(DeviceState State)
        {
            m_QueueWorker.AddBvtAllEvent(State);
        }

        public void BVTDirectHandler(DeviceState State, Types.BVT.TestResults Result)
        {
            m_QueueWorker.AddBvtDirectEvent(State, Result);
        }

        public void BVTReverseHandler(DeviceState State, Types.BVT.TestResults Result)
        {
            m_QueueWorker.AddBvtReverseEvent(State, Result);
        }

        public void BVTNotificationHandler(Types.BVT.HWProblemReason Problem, Types.BVT.HWWarningReason Warning,
                                           Types.BVT.HWFaultReason Fault,
                                           Types.BVT.HWDisableReason Disable)
        {
            if (Warning != Types.BVT.HWWarningReason.None)
                m_QueueWorker.AddBvtWarningEvent(Warning);

            if (Problem != Types.BVT.HWProblemReason.None)
                m_QueueWorker.AddBvtProblemEvent(Problem);

            if (Fault != Types.BVT.HWFaultReason.None)
                m_QueueWorker.AddBvtFaultEvent(Fault);

            // if (Disable != Types.BVT.HWDisableReason.None)
            //      m_QueueWorker.AddBvtFaultEvent(Disable);
        }

        public void DvDtHandler(DeviceState State, Types.dVdt.TestResults Result)
        {
            m_QueueWorker.AddDVdtEvent(State, Result);
        }

        public void DvDtNotificationHandler(Types.dVdt.HWWarningReason Warning, Types.dVdt.HWFaultReason Fault, Types.dVdt.HWDisableReason Disable)
        {
            if (Warning != Types.dVdt.HWWarningReason.None)
                m_QueueWorker.AddDVdtWarningEvent(Warning);

            if (Fault != Types.dVdt.HWFaultReason.None)
                m_QueueWorker.AddDVdtFaultEvent(Fault);
        }

        public void ATUHandler(DeviceState State, Types.ATU.TestResults Result)
        {
            m_QueueWorker.AddATUEvent(State, Result);
        }

        public void ATUNotificationHandler(ushort Warning, ushort Fault, ushort Disable)
        {
            if (Warning != (ushort)Types.ATU.HWWarningReason.None) m_QueueWorker.AddATUWarningEvent(Warning);

            if (Fault != (ushort)Types.ATU.HWFaultReason.None) m_QueueWorker.AddATUFaultEvent(Fault);
        }

        public void QrrTqHandler(DeviceState State, Types.QrrTq.TestResults Result)
        {
            m_QueueWorker.AddQrrTqEvent(State, Result);
        }

        public void QrrTqNotificationHandler(ushort Problem, ushort Warning, ushort Fault, ushort Disable)
        {
            if (Problem != (ushort)Types.QrrTq.HWProblemReason.None)
                m_QueueWorker.AddQrrTqProblemEvent(Problem);

            if (Warning != (ushort)Types.QrrTq.HWWarningReason.None)
                m_QueueWorker.AddQrrTqWarningEvent(Warning);

            if (Fault != (ushort)Types.QrrTq.HWFaultReason.None)
                m_QueueWorker.AddQrrTqFaultEvent(Fault);
        }

        public void QrrTqKindOfFreezingHandler(ushort KindOfFreezing)
        {
            m_QueueWorker.AddQrrTqKindOfFreezingEvent(KindOfFreezing);
        }


        public void TOUHandler(DeviceState State, TestResults Result)
        {
            m_QueueWorker.AddTOUEvent(State, Result);
        }


        public void TOUNotificationHandler(ushort Problem, ushort Warning, ushort Fault, ushort Disable)
        {
            if (Problem != (ushort)Types.TOU.HWProblemReason.None)
                m_QueueWorker.AddTOUProblemEvent(Problem);

            if (Warning != (ushort)Types.TOU.HWWarningReason.None)
                m_QueueWorker.AddTOUWarningEvent(Warning);

            if (Fault != (ushort)Types.TOU.HWFaultReason.None)
                m_QueueWorker.AddTOUFaultEvent(Fault);
        }

        public void IHHandler(DeviceState State, Types.IH.TestResults Result)
        {
            m_QueueWorker.AddIHEvent(State, Result);
        }

        public void IHNotificationHandler(ushort Problem, ushort Warning, ushort Fault, ushort Disable)
        {
            if (Problem != (ushort)Types.IH.HWProblemReason.None)
                m_QueueWorker.AddIHProblemEvent(Problem);

            if (Warning != (ushort)Types.IH.HWWarningReason.None)
                m_QueueWorker.AddIHWarningEvent(Warning);

            if (Fault != (ushort)Types.IH.HWFaultReason.None)
                m_QueueWorker.AddIHFaultEvent(Fault);
        }

        public void RCCHandler(DeviceState State, Types.RCC.TestResults Result)
        {
            //UI не использует виртуальный блок RCC, его использует только комплекс АКИМ
        }

        public void RCCNotificationHandler(ushort Problem, ushort Warning, ushort Fault, ushort Disable)
        {
            //UI не использует виртуальный блок RCC, его использует только комплекс АКИМ
        }

        public void ClampingSwitchHandler(SqueezingState Up, IList<float> ArrayF, IList<float> ArrayFd)
        {
            m_QueueWorker.AddClampingSwitchEvent(Up, ArrayF, ArrayFd);
        }

        public void ClampingNotificationHandler(Types.Clamping.HWWarningReason Warning, Types.Clamping.HWProblemReason Problem, Types.Clamping.HWFaultReason Fault)
        {
            if (Warning != Types.Clamping.HWWarningReason.None)
                m_QueueWorker.AddClampingWarningEvent(Warning);

            if (Problem != Types.Clamping.HWProblemReason.None)
                m_QueueWorker.AddClampingProblemEvent(Problem);

            if (Fault != Types.Clamping.HWFaultReason.None)
                m_QueueWorker.AddClampingFaultEvent(Fault);
        }





        #endregion
    }
}