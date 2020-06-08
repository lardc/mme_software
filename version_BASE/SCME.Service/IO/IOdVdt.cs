using System;
using System.Collections.Generic;
using System.Threading;
using SCME.Service.Properties;
using SCME.Types;
using SCME.Types.dVdt;

namespace SCME.Service.IO
{
    internal class IOdVdt
    {
        private const int REQUEST_DELAY_MS = 50;

        private readonly IOAdapter m_IOAdapter;
        private readonly BroadcastCommunication m_Communication;
        private readonly ushort m_Node;
        private readonly bool m_IsdVdtEmulationHard;
        private bool m_IsdVdtEmulation;
        private TestParameters m_Parameters;
        private DeviceConnectionState m_ConnectionState;
        private volatile DeviceState m_State;
        private volatile TestResults m_Result;
        private volatile bool m_Stop;

        private int m_Timeout = 25000;

        internal IOdVdt(IOAdapter Adapter, BroadcastCommunication Communication)
        {
            m_IOAdapter = Adapter;
            m_Communication = Communication;
            m_IsdVdtEmulationHard = Settings.Default.dVdtEmulation;
            m_IsdVdtEmulation = m_IsdVdtEmulationHard;

            m_Node = (ushort)Settings.Default.dVdtNode;
            m_Result = new TestResults();

            SystemHost.Journal.AppendLog(ComplexParts.DvDt, LogMessageType.Info,
                                         String.Format("dVdt created. Emulation mode: {0}", Settings.Default.dVdtEmulation));
        }

        internal DeviceConnectionState Initialize(bool Enable, int timeoutdVdt)
        {
            m_Timeout = timeoutdVdt;
            m_IsdVdtEmulation = m_IsdVdtEmulationHard || !Enable;

            m_ConnectionState = DeviceConnectionState.ConnectionInProcess;
            FireConnectionEvent(m_ConnectionState, "dVdt initializing");

            if (m_IsdVdtEmulation)
            {
                m_ConnectionState = DeviceConnectionState.ConnectionSuccess;
                FireConnectionEvent(m_ConnectionState, "dVdt initialized");

                return m_ConnectionState;
            }

            try
            {
                var timeStamp = Environment.TickCount + m_Timeout;

                ClearWarning();

                var devState = (HWDeviceState)ReadRegister(REG_DEVICE_STATE);
                if (devState != HWDeviceState.PowerReady)
                {
                    if (devState == HWDeviceState.Fault)
                    {
                        ClearFault();
                        Thread.Sleep(100);

                        devState = (HWDeviceState)ReadRegister(REG_DEVICE_STATE);

                        if (devState == HWDeviceState.Fault)
                            throw new Exception(string.Format("dVdt is in fault state, reason: {0}",
                                (HWFaultReason)ReadRegister(REG_FAULT_REASON)));
                    }

                    if (devState == HWDeviceState.Disabled)
                        throw new Exception(string.Format("dVdt is in disabled state, reason: {0}",
                                (HWDisableReason)ReadRegister(REG_DISABLE_REASON)));

                    CallAction(ACT_ENABLE_POWER);
                }

                while (Environment.TickCount < timeStamp)
                {
                    Thread.Sleep(100);

                    devState = (HWDeviceState)
                               ReadRegister(REG_DEVICE_STATE);

                    if (devState == HWDeviceState.PowerReady)
                        break;

                    if (devState == HWDeviceState.Fault)
                        throw new Exception(string.Format("dVdt is in fault state, reason: {0}",
                                                          (HWFaultReason)ReadRegister(REG_FAULT_REASON)));
                    if (devState == HWDeviceState.Disabled)
                        throw new Exception(string.Format("dVdt is in disabled state, reason: {0}",
                                                          (HWDisableReason)ReadRegister(REG_DISABLE_REASON)));
                }

                if (Environment.TickCount > timeStamp)
                    throw new Exception("Timeout while waiting for device to power up");

                m_ConnectionState = DeviceConnectionState.ConnectionSuccess;

                FireConnectionEvent(m_ConnectionState, "dVdt initialized");
            }
            catch (Exception ex)
            {
                m_ConnectionState = DeviceConnectionState.ConnectionFailed;
                FireConnectionEvent(m_ConnectionState, String.Format("dVdt initialization error: {0}", ex.Message));
            }

            return m_ConnectionState;
        }

        internal void Deinitialize()
        {
            var oldState = m_ConnectionState;

            m_ConnectionState = DeviceConnectionState.DisconnectionInProcess;
            FireConnectionEvent(DeviceConnectionState.DisconnectionInProcess, "dVdt disconnecting");

            try
            {
                if (!m_IsdVdtEmulation && oldState == DeviceConnectionState.ConnectionSuccess)
                {
                    Stop();
                    CallAction(ACT_DISABLE_POWER);
                }

                m_ConnectionState = DeviceConnectionState.DisconnectionSuccess;
                FireConnectionEvent(DeviceConnectionState.DisconnectionSuccess, "dVdt disconnected");
            }
            catch (Exception)
            {
                m_ConnectionState = DeviceConnectionState.DisconnectionError;
                FireConnectionEvent(DeviceConnectionState.DisconnectionError, "dVdt disconnection error");
            }
        }

        internal DeviceState Start(TestParameters Parameters)
        {
            m_Parameters = Parameters;

            if (m_State == DeviceState.InProcess)
                throw new Exception("dVdt test is already started");

            m_Result = new TestResults()
            {
                TestTypeId = m_Parameters.TestTypeId,
                Mode = m_Parameters.Mode,
                VoltageRate = Parameters.VoltageRate
            };
            m_Stop = false;

            ClearWarning();

            if (!m_IsdVdtEmulation)
            {
                var devState = (HWDeviceState)ReadRegister(REG_DEVICE_STATE);
                if (devState == HWDeviceState.Fault)
                {
                    var faultReason = (HWFaultReason)ReadRegister(REG_FAULT_REASON);
                    FireNotificationEvent(HWWarningReason.None, faultReason,
                                          HWDisableReason.None);

                    throw new Exception(string.Format("dVdt is in fault state, reason: {0}", faultReason));
                }

                if (devState == HWDeviceState.Disabled)
                {
                    var disableReason = (HWDisableReason)ReadRegister(REG_DISABLE_REASON);
                    FireNotificationEvent(HWWarningReason.None,
                                          HWFaultReason.None, disableReason);

                    throw new Exception(string.Format("dVdt is in disabled state, reason: {0}", disableReason));
                }
            }

            MeasurementLogicRoutine();

            return m_State;
        }

        internal void Stop()
        {
            m_Stop = true;
        }

        #region Standart API

        internal void ClearFault()
        {
            SystemHost.Journal.AppendLog(ComplexParts.DvDt, LogMessageType.Note, "dVdt fault cleared");

            CallAction(ACT_CLEAR_FAULT);
        }

        private void ClearWarning()
        {
            SystemHost.Journal.AppendLog(ComplexParts.DvDt, LogMessageType.Note, "dVdt warning cleared");

            CallAction(ACT_CLEAR_WARNING);
        }

        internal ushort ReadRegister(ushort Address, bool SkipJournal = false)
        {
            ushort value = 0;

            if (!m_IsdVdtEmulation)
                value = m_IOAdapter.Read16(m_Node, Address);

            if (!SkipJournal)
                SystemHost.Journal.AppendLog(ComplexParts.DvDt, LogMessageType.Note,
                                         string.Format("dVdt @ReadRegister, address {0}, value {1}", Address, value));

            return value;
        }

        internal short ReadRegisterS(ushort Address, bool SkipJournal = false)
        {
            short value = 0;

            if (!m_IsdVdtEmulation)
                value = m_IOAdapter.Read16S(m_Node, Address);

            if (!SkipJournal)
                SystemHost.Journal.AppendLog(ComplexParts.DvDt, LogMessageType.Note,
                                         string.Format("dVdt @ReadRegisterS, address {0}, value {1}", Address, value));

            return value;
        }

        internal void WriteRegister(ushort Address, ushort Value, bool SkipJournal = false)
        {
            if (!SkipJournal)
                SystemHost.Journal.AppendLog(ComplexParts.DvDt, LogMessageType.Note,
                                         string.Format("dVdt @WriteRegister, address {0}, value {1}", Address, Value));

            if (m_IsdVdtEmulation)
                return;

            m_IOAdapter.Write16(m_Node, Address, Value);
        }

        internal void CallAction(ushort Action)
        {
            SystemHost.Journal.AppendLog(ComplexParts.DvDt, LogMessageType.Note,
                                         string.Format("dVdt @Call, action {0}", Action));

            if (m_IsdVdtEmulation)
                return;

            m_IOAdapter.Call(m_Node, Action);
        }

        #endregion

        internal void WriteCalibrationParams(CalibrationParams Parameters)
        {
            SystemHost.Journal.AppendLog(ComplexParts.DvDt, LogMessageType.Note,
                                         "dVdt @WriteCalibrationParams begin");

            //WriteRegister(REG_V_FINE_N, Parameters.VFineN, true);
            //WriteRegister(REG_V_FINE_D, Parameters.VFineD, true);

            //WriteRegister(REG_G500, Parameters.V500, true);
            //WriteRegister(REG_G1000, Parameters.V1000, true);
            //WriteRegister(REG_G1500, Parameters.V1500, true);
            //WriteRegister(REG_G2000, Parameters.V2000, true);
            //WriteRegister(REG_G2500, Parameters.V2500, true);

            //if (!m_IsdVdtEmulation)
            //    m_IOAdapter.Call(m_Node, ACT_SAVE_TO_ROM);

            SystemHost.Journal.AppendLog(ComplexParts.DvDt, LogMessageType.Note,
                                         "dVdt @WriteCalibrationParams end");
        }

        internal CalibrationParams ReadCalibrationParams()
        {
            SystemHost.Journal.AppendLog(ComplexParts.DvDt, LogMessageType.Note,
                                         "dVdt @ReadCalibrationParams begin");

            var parameters = new CalibrationParams
                {
                    //VFineN = ReadRegister(REG_V_FINE_N, true),
                    //VFineD = ReadRegister(REG_V_FINE_D, true),

                    //V500 = ReadRegister(REG_G500, true),
                    //V1000 = ReadRegister(REG_G1000, true),
                    //V1500 = ReadRegister(REG_G1500, true),
                    //V2000 = ReadRegister(REG_G2000, true),
                    //V2500 = ReadRegister(REG_G2500, true)
                };

            SystemHost.Journal.AppendLog(ComplexParts.DvDt, LogMessageType.Note,
                                         "dVdt @ReadCalibrationParams end");

            return parameters;
        }

        private void MeasurementLogicRoutine()
        {
            try
            {
                m_State = DeviceState.InProcess;
                FiredVdtEvent(m_State, m_Result);

                WriteRegister(REG_DESIRED_VOLTAGE, m_Parameters.Voltage);

                if (!m_IsdVdtEmulation)
                {
                    if (m_Parameters.Mode == DvdtMode.Confirmation)
                    {
                        switch (m_Parameters.VoltageRate)
                        {
                            case VoltageRate.V500:
                                CallAction(ACT_START_TEST_500);
                                break;
                            case VoltageRate.V1000:
                                CallAction(ACT_START_TEST_1000);
                                break;
                            case VoltageRate.V1500:
                                CallAction(ACT_START_TEST_1500);
                                break;
                            case VoltageRate.V2000:
                                CallAction(ACT_START_TEST_2000);
                                break;
                            case VoltageRate.V2500:
                                CallAction(ACT_START_TEST_2500);
                                break;
                        }

                        m_State = WaitForEndOfTest();
                        var opened = ReadRegisterS(REG_TEST_RESULT) == 0;
                        m_Result.Passed = !opened;
                    }
                    else
                    {
                        var voltageRates = new Stack<VoltageRate>();
                        voltageRates.Push(VoltageRate.V500);//Добавляем первое измерение
                        var straitDirection = true;

                        while (voltageRates.Count > 0)
                        {
                            var currentRate = voltageRates.Peek();//берем верхнее измерение

                            switch (currentRate)
                            {
                                case VoltageRate.V500:
                                    CallAction(ACT_START_TEST_500);
                                    if (straitDirection)
                                        voltageRates.Push(VoltageRate.V1000);
                                    break;
                                case VoltageRate.V1000:
                                    CallAction(ACT_START_TEST_1000);
                                    if (straitDirection)
                                        voltageRates.Push(VoltageRate.V1500);
                                    break;
                                case VoltageRate.V1500:
                                    CallAction(ACT_START_TEST_1500);
                                    if (straitDirection)
                                        voltageRates.Push(VoltageRate.V2000);
                                    break;
                                case VoltageRate.V2000:
                                    CallAction(ACT_START_TEST_2000);
                                    if (straitDirection)
                                        voltageRates.Push(VoltageRate.V2500);
                                    break;
                                case VoltageRate.V2500:
                                    CallAction(ACT_START_TEST_2500);
                                    break;
                            }

                            m_State = WaitForEndOfTest();
                            var opened = ReadRegisterS(REG_TEST_RESULT) == 0;

                            if (currentRate == VoltageRate.V500 && opened)//случай брака сразу или на последнем обратном шаге
                            {
                                m_Result.Passed = false;
                                break;
                            }
                            if (currentRate == VoltageRate.V2500 && !opened)//случай не открытия на максимальной скорости
                            {
                                m_Result.Passed = true;
                                m_Result.VoltageRate = VoltageRate.V2500;
                                break;
                            }

                            if (opened)
                            {
                                if (straitDirection)
                                {
                                    voltageRates.Pop();//тот который добавился пока было прямое 
                                    straitDirection = false;
                                }
                                voltageRates.Pop(); // убираем текущий
                            }
                            else if (!straitDirection)
                            {
                                m_Result.Passed = true;
                                m_Result.VoltageRate = currentRate;
                                break;
                            }

                        }
                    }

                }
                else
                {
                    m_State = DeviceState.Success;
                    m_Result.Passed = true;
                }

                FiredVdtEvent(m_State, m_Result);
            }
            catch (Exception ex)
            {
                m_State = DeviceState.Fault;
                FiredVdtEvent(m_State, m_Result);
                FireExceptionEvent(ex.Message);

                throw;
            }
        }

        private DeviceState WaitForEndOfTest()
        {
            var timeStamp = Environment.TickCount + m_Timeout;

            while (Environment.TickCount < timeStamp)
            {
                
                var devState = (HWDeviceState)ReadRegister(REG_DEVICE_STATE, true);
                
                if (devState == HWDeviceState.Fault)
                {
                    var faultReason = (HWFaultReason)ReadRegister(REG_FAULT_REASON);

                    FireNotificationEvent(HWWarningReason.None, faultReason,
                                          HWDisableReason.None);
                    throw new Exception(string.Format("dVdt device is in fault state, reason: {0}", faultReason));
                }

                if (devState == HWDeviceState.Disabled)
                {
                    var disableReason = (HWDisableReason)ReadRegister(REG_DISABLE_REASON);

                    FireNotificationEvent(HWWarningReason.None,
                                          HWFaultReason.None, disableReason);
                    throw new Exception(string.Format("dVdt device is in disabled state, reason: {0}", disableReason));
                }

                if (devState != HWDeviceState.InProcess)
                {
                    var warning = (HWWarningReason)ReadRegister(REG_WARNING);

                    if (warning != HWWarningReason.None)
                    {
                        FireNotificationEvent(warning, HWFaultReason.None,
                                              HWDisableReason.None);
                        ClearWarning();
                    }

                    break;
                }

                Thread.Sleep(REQUEST_DELAY_MS);
            }

            if (Environment.TickCount > timeStamp)
            {
                FireExceptionEvent("Timeout while waiting for dVdt test to end");
                throw new Exception("Timeout while waiting for dVdt test to end");
            }

            return DeviceState.Success;
        }

        #region Events

        private void FireConnectionEvent(DeviceConnectionState State, string Message)
        {
            SystemHost.Journal.AppendLog(ComplexParts.DvDt, LogMessageType.Info, Message);
            m_Communication.PostDeviceConnectionEvent(ComplexParts.DvDt, State, Message);
        }

        private void FiredVdtEvent(DeviceState State, TestResults Result)
        {
            var message = string.Format("dVdt test state {0}", State);

            if (State == DeviceState.Success)
                message = string.Format("dVdt test result {0}", Result.Passed);

            SystemHost.Journal.AppendLog(ComplexParts.DvDt, LogMessageType.Info, message);
            m_Communication.PostdVdtEvent(State, Result);
        }

        private void FireNotificationEvent(HWWarningReason Warning, HWFaultReason Fault, HWDisableReason Disable)
        {
            SystemHost.Journal.AppendLog(ComplexParts.DvDt, LogMessageType.Warning,
                                         string.Format(
                                             "dVdt device notification: problem None, warning {0}, fault {1}, disable {2}",
                                             Warning, Fault, Disable));

            m_Communication.PostdVdtNotificationEvent(Warning, Fault, Disable);
        }

        private void FireExceptionEvent(string Message)
        {
            SystemHost.Journal.AppendLog(ComplexParts.DvDt, LogMessageType.Error, Message);
            m_Communication.PostExceptionEvent(ComplexParts.DvDt, Message);
        }

        #endregion

        #region Registers

        private const ushort
            ACT_ENABLE_POWER = 1,
            ACT_DISABLE_POWER = 2,
            ACT_CLEAR_FAULT = 3,
            ACT_CLEAR_WARNING = 4,

            ACT_START_TEST_500 = 101,
            ACT_START_TEST_1000 = 102,
            ACT_START_TEST_1500 = 103,
            ACT_START_TEST_2000 = 104,
            ACT_START_TEST_2500 = 105,

            REG_DESIRED_VOLTAGE = 128,

            REG_DEVICE_STATE = 192,
            REG_FAULT_REASON = 193,
            REG_DISABLE_REASON = 194,
            REG_WARNING = 195,
            REG_PROBLEM = 196,
            REG_TEST_FINISHED = 197,

            REG_TEST_RESULT = 198;





        #endregion
    }
}