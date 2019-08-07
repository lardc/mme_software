using System;
using System.Collections.Generic;
using System.Threading;
using SCME.Service.Properties;
using SCME.Types;
using SCME.Types.TOU;

namespace SCME.Service.IO
{
    internal class IOTOU
    {
        private const int REQUEST_DELAY_MS = 50;

        private readonly IOAdapter m_IOAdapter;
        private readonly BroadcastCommunication m_Communication;
        private readonly ushort m_Node;
        private readonly bool m_IsTOUEmulationHard;
        private IOCommutation m_IOCommutation;
        private bool m_IsTOUEmulation;
        private TestParameters m_Parameters;
        private DeviceConnectionState m_ConnectionState;
        private volatile DeviceState m_State;
        private volatile TestResults m_Result;
        private volatile bool m_Stop;

        private int m_Timeout = 25000;

        internal IOTOU(IOAdapter Adapter, BroadcastCommunication Communication)
        {
            m_IOAdapter = Adapter;
            m_Communication = Communication;
            //Устанавливаем режим эмуляции
            m_IsTOUEmulation = m_IsTOUEmulationHard = Settings.Default.TOUEmulation;
            ///////////////////////////////////////////////////////////
            m_Node = (ushort)Settings.Default.TOUNode;
            m_Result = new TestResults();

            SystemHost.Journal.AppendLog(ComplexParts.TOU, LogMessageType.Info,
                                         String.Format("TOU created. Emulation mode: {0}", m_IsTOUEmulation));
        }


        internal IOCommutation ActiveCommutation
        {
            get { return m_IOCommutation; }
            set { m_IOCommutation = value; }
        }

        internal DeviceConnectionState Initialize(bool Enable, int timeoutTOU)
        {
            m_Timeout = timeoutTOU;
            m_IsTOUEmulation = m_IsTOUEmulationHard || !Enable;

            m_ConnectionState = DeviceConnectionState.ConnectionInProcess;
            FireConnectionEvent(m_ConnectionState, "TOU initializing");

            if (m_IsTOUEmulation)
            {
                m_ConnectionState = DeviceConnectionState.ConnectionSuccess;
                FireConnectionEvent(m_ConnectionState, "TOU initialized");

                return m_ConnectionState;
            }

            try
            {
                var timeStamp = Environment.TickCount + m_Timeout;

                ClearWarning();

                var devState = (HWDeviceState)ReadRegister(REG_DEV_STATE);
                if (devState != HWDeviceState.PowerReady)
                {
                    if (devState == HWDeviceState.Fault)
                    {
                        ClearFault();
                        Thread.Sleep(100);

                        devState = (HWDeviceState)ReadRegister(REG_DEV_STATE);

                        if (devState == HWDeviceState.Fault)
                            throw new Exception(string.Format("TOU is in fault state, reason: {0}",
                                (HWFaultReason)ReadRegister(REG_FAULT_REASON)));
                    }

                    if (devState == HWDeviceState.Disabled)
                        throw new Exception(string.Format("TOU is in disabled state, reason: {0}",
                                (HWDisableReason)ReadRegister(REG_DISABLE_REASON)));

                    CallAction(ACT_ENABLE_POWER);
                }

                while (Environment.TickCount < timeStamp)
                {
                    Thread.Sleep(100);

                    devState = (HWDeviceState)
                               ReadRegister(REG_DEV_STATE);

                    if (devState == HWDeviceState.PowerReady)
                        break;

                    if (devState == HWDeviceState.Fault)
                        throw new Exception(string.Format("TOU is in fault state, reason: {0}",
                                                          (HWFaultReason)ReadRegister(REG_FAULT_REASON)));
                    if (devState == HWDeviceState.Disabled)
                        throw new Exception(string.Format("TOU is in disabled state, reason: {0}",
                                                          (HWDisableReason)ReadRegister(REG_DISABLE_REASON)));
                }

                if (Environment.TickCount > timeStamp)
                    throw new Exception("Timeout while waiting for device to power up");

                m_ConnectionState = DeviceConnectionState.ConnectionSuccess;

                FireConnectionEvent(m_ConnectionState, "TOU initialized");
            }
            catch (Exception ex)
            {
                m_ConnectionState = DeviceConnectionState.ConnectionFailed;
                FireConnectionEvent(m_ConnectionState, String.Format("TOU initialization error: {0}", ex.Message));
            }

            return m_ConnectionState;
        }

        internal void Deinitialize()
        {
            var oldState = m_ConnectionState;

            m_ConnectionState = DeviceConnectionState.DisconnectionInProcess;
            FireConnectionEvent(DeviceConnectionState.DisconnectionInProcess, "TOU disconnecting");

            try
            {
                if (!m_IsTOUEmulation && oldState == DeviceConnectionState.ConnectionSuccess)
                {
                    Stop();
                    CallAction(ACT_DISABLE_POWER);
                }

                m_ConnectionState = DeviceConnectionState.DisconnectionSuccess;
                FireConnectionEvent(DeviceConnectionState.DisconnectionSuccess, "TOU disconnected");
            }
            catch (Exception)
            {
                m_ConnectionState = DeviceConnectionState.DisconnectionError;
                FireConnectionEvent(DeviceConnectionState.DisconnectionError, "TOU disconnection error");
            }
        }

        internal DeviceState Start(TestParameters Parameters, Types.Commutation.TestParameters commParameters)
        {
            m_Parameters = Parameters;

            if (m_State == DeviceState.InProcess)
                throw new Exception("TOU test is already started");

            m_Result = new TestResults()
            {
                TestTypeId = m_Parameters.TestTypeId,
            };
            m_Stop = false;

            ClearWarning();

            if (!m_IsTOUEmulation)
            {
                var devState = (HWDeviceState)ReadRegister(REG_DEV_STATE);
                if (devState == HWDeviceState.Fault)
                {
                    var faultReason = (HWFaultReason)ReadRegister(REG_FAULT_REASON);
                    FireNotificationEvent(HWWarningReason.None, faultReason,
                                          HWDisableReason.None);

                    throw new Exception(string.Format("TOU is in fault state, reason: {0}", faultReason));
                }

                if (devState == HWDeviceState.Disabled)
                {
                    var disableReason = (HWDisableReason)ReadRegister(REG_DISABLE_REASON);
                    FireNotificationEvent(HWWarningReason.None,
                                          HWFaultReason.None, disableReason);

                    throw new Exception(string.Format("TOU is in disabled state, reason: {0}", disableReason));
                }
            }

            MeasurementLogicRoutine(commParameters);

            return m_State;
        }

        internal void Stop()
        {
            CallAction(ACT_STOP);
            m_Stop = true;
            m_State = DeviceState.Stopped;
        }

        internal bool IsReadyToStart()
        {
            var devState = (Types.TOU.HWDeviceState)ReadRegister(REG_DEV_STATE);

            return !((devState == Types.TOU.HWDeviceState.Fault) || (devState == Types.TOU.HWDeviceState.Disabled) || (m_State == DeviceState.InProcess));
        }

        #region Standart API

        internal void ClearFault()
        {
            SystemHost.Journal.AppendLog(ComplexParts.TOU, LogMessageType.Note, "TOU fault cleared");

            CallAction(ACT_CLEAR_FAULT);
        }

        private void ClearWarning()
        {
            SystemHost.Journal.AppendLog(ComplexParts.TOU, LogMessageType.Note, "TOU warning cleared");

            CallAction(ACT_CLEAR_WARNING);
        }

        internal ushort ReadRegister(ushort Address, bool SkipJournal = false)
        {
            ushort value = 0;

            if (!m_IsTOUEmulation)
                value = m_IOAdapter.Read16(m_Node, Address);

            if (!SkipJournal)
                SystemHost.Journal.AppendLog(ComplexParts.TOU, LogMessageType.Note,
                                         string.Format("TOU @ReadRegister, address {0}, value {1}", Address, value));

            return value;
        }

        internal short ReadRegisterS(ushort Address, bool SkipJournal = false)
        {
            short value = 0;

            if (!m_IsTOUEmulation)
                value = m_IOAdapter.Read16S(m_Node, Address);

            if (!SkipJournal)
                SystemHost.Journal.AppendLog(ComplexParts.TOU, LogMessageType.Note,
                                         string.Format("TOU @ReadRegisterS, address {0}, value {1}", Address, value));

            return value;
        }

        internal void WriteRegister(ushort Address, ushort Value, bool SkipJournal = false)
        {
            if (!SkipJournal)
                SystemHost.Journal.AppendLog(ComplexParts.TOU, LogMessageType.Note,
                                         string.Format("TOU @WriteRegister, address {0}, value {1}", Address, Value));

            if (m_IsTOUEmulation)
                return;

            m_IOAdapter.Write16(m_Node, Address, Value);
        }

        internal void CallAction(ushort Action)
        {
            SystemHost.Journal.AppendLog(ComplexParts.TOU, LogMessageType.Note,
                                         string.Format("TOU @Call, action {0}", Action));

            if (m_IsTOUEmulation)
                return;

            m_IOAdapter.Call(m_Node, Action);
        }

        #endregion

        //internal void WriteCalibrationParams(CalibrationParams Parameters)
        //{
        //    SystemHost.Journal.AppendLog(ComplexParts.TOU, LogMessageType.Note,
        //                                 "TOU @WriteCalibrationParams begin");

        //    //WriteRegister(REG_V_FINE_N, Parameters.VFineN, true);
        //    //WriteRegister(REG_V_FINE_D, Parameters.VFineD, true);

        //    //WriteRegister(REG_G500, Parameters.V500, true);
        //    //WriteRegister(REG_G1000, Parameters.V1000, true);
        //    //WriteRegister(REG_G1500, Parameters.V1500, true);
        //    //WriteRegister(REG_G2000, Parameters.V2000, true);
        //    //WriteRegister(REG_G2500, Parameters.V2500, true);

        //    //if (!m_IsdVdtEmulation)
        //    //    m_IOAdapter.Call(m_Node, ACT_SAVE_TO_ROM);

        //    SystemHost.Journal.AppendLog(ComplexParts.TOU, LogMessageType.Note,
        //                                 "TOU @WriteCalibrationParams end");
        //}

        //internal CalibrationParams ReadCalibrationParams()
        //{
        //    SystemHost.Journal.AppendLog(ComplexParts.TOU, LogMessageType.Note,
        //                                 "TOU @ReadCalibrationParams begin");

        //    var parameters = new CalibrationParams
        //    {
        //        //VFineN = ReadRegister(REG_V_FINE_N, true),
        //        //VFineD = ReadRegister(REG_V_FINE_D, true),

        //        //V500 = ReadRegister(REG_G500, true),
        //        //V1000 = ReadRegister(REG_G1000, true),
        //        //V1500 = ReadRegister(REG_G1500, true),
        //        //V2000 = ReadRegister(REG_G2000, true),
        //        //V2500 = ReadRegister(REG_G2500, true)
        //    };

        //    SystemHost.Journal.AppendLog(ComplexParts.TOU, LogMessageType.Note,
        //                                 "TOU @ReadCalibrationParams end");

        //    return parameters;
        //}


        //private void PrepareSteps(List<ushort> Steps)
        //{
        //    //формирует в принятом Steps список шагов роста напряжения на которых прибор тестируется на открытие
        //    if (Steps != null)
        //    {
        //        if (m_Parameters.VoltageRateOffSet <= 0) throw new Exception("scme.service.io.iotou.cs PrepareSteps. m_Parameters.VoltageRateOffSet<=0.");

        //        //очищаем содержимое Steps
        //        Steps.Clear();

        //        //исходная точка от которой вычисляется значение роста напряжения есть 500 В/мкс. на 20.04.2017 это ограничено аппаратными возможностями блока dVdt
        //        ushort VoltageRateStart = 500;
        //        Steps.Add(VoltageRateStart);

        //        //значение роста напряжения на любом последующем шаге отличается от текщего шага на величину смещения VoltageRateOffSet, но итоговое значение роста напряжения не должно превысить ограничения роста напряжения VoltageRateLimit
        //        int VoltageRate = VoltageRateStart;
        //        int CalcedVoltageRate = 0;

        //        while (true)
        //        {
        //            CalcedVoltageRate = VoltageRate + m_Parameters.VoltageRateOffSet;

        //            if (CalcedVoltageRate >= m_Parameters.VoltageRateLimit)
        //            {
        //                //CalcedVoltageRate вылезает за VoltageRateLimit. поэтому пишем вместо него VoltageRateLimit
        //                Steps.Add(m_Parameters.VoltageRateLimit);

        //                break;
        //            }
        //            else
        //            {
        //                VoltageRate = CalcedVoltageRate;
        //                Steps.Add((ushort)VoltageRate);
        //            }

        //        }

        //    }
        //}


        private void MeasurementLogicRoutine(Types.Commutation.TestParameters Commutation)
        {
            try
            {
                m_State = DeviceState.InProcess;
                FireTOUEvent(m_State, m_Result);

                if (m_IsTOUEmulation)
                {
                    //в режиме эмуляции эмулируем успешный результат проверки
                    m_State = DeviceState.Success;

                    Random rand = new Random(DateTime.Now.Millisecond);

                    m_Result.ITM = (float)rand.NextDouble() * 1000;
                    m_Result.TGD = (float)rand.NextDouble() * 1000;
                    m_Result.TGT = (float)rand.NextDouble() * 1000;
                }
                else
                {
                    //перед измерением dVdt исполняем команду включения коммутации см. требование http://elma.pe.local/Tasks/Task/Execute/108699
                    if (m_IOCommutation.Switch(Types.Commutation.CommutationMode.TOU, Commutation.CommutationType, Commutation.Position) == DeviceState.Fault)
                    {
                        m_State = DeviceState.Fault;
                        FireTOUEvent(m_State, m_Result);
                        return;
                    }

                    WriteRegister(REG_CURRENT_VALUE, m_Parameters.CurrentAmplitude);
                    CallAction(ACT_START_TEST);
                    m_State = WaitForEndOfTest();

                    m_Result.ITM = ReadRegister(REG_MEAS_CURRENT_VALUE);
                    m_Result.TGD = ReadRegister(REG_MEAS_TIME_DELAY);
                    m_Result.TGT = ReadRegister(REG_MEAS_TIME_ON);

                }

                FireTOUEvent(m_State, m_Result);
            }

            catch (Exception ex)
            {
                m_State = DeviceState.Fault;
                FireTOUEvent(m_State, m_Result);
                FireExceptionEvent(ex.Message);

                throw;
            }

        }


        private DeviceState WaitForEndOfTest()
        {
            var timeStamp = Environment.TickCount + m_Timeout;

            while (Environment.TickCount < timeStamp)
            {

                var devState = (HWDeviceState)ReadRegister(REG_DEV_STATE, true);

                if (devState == HWDeviceState.Fault)
                {
                    var faultReason = (HWFaultReason)ReadRegister(REG_FAULT_REASON);

                    FireNotificationEvent(HWWarningReason.None, faultReason,
                                          HWDisableReason.None);
                    throw new Exception(string.Format("TOU device is in fault state, reason: {0}", faultReason));
                }

                if (devState == HWDeviceState.Disabled)
                {
                    var disableReason = (HWDisableReason)ReadRegister(REG_DISABLE_REASON);

                    FireNotificationEvent(HWWarningReason.None,
                                          HWFaultReason.None, disableReason);
                    throw new Exception(string.Format("TOU device is in disabled state, reason: {0}", disableReason));
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
                FireExceptionEvent("Timeout while waiting for TOU test to end");
                throw new Exception("Timeout while waiting for TOU test to end");
            }

            return DeviceState.Success;
        }

        #region Events

        private void FireConnectionEvent(DeviceConnectionState State, string Message)
        {
            SystemHost.Journal.AppendLog(ComplexParts.TOU, LogMessageType.Info, Message);
            m_Communication.PostDeviceConnectionEvent(ComplexParts.TOU, State, Message);
        }

        private void FireTOUEvent(DeviceState State, TestResults Result)
        {
            var message = string.Format("TOU test state {0}", State);

            if (State == DeviceState.Success)
                message = string.Format("TOU test result {0} {1} {2}", Result.ITM, Result.TGD, Result.TGT);

            SystemHost.Journal.AppendLog(ComplexParts.TOU, LogMessageType.Info, message);
            m_Communication.PostTOUEvent(State, Result);
        }

        private void FireNotificationEvent(HWWarningReason Warning, HWFaultReason Fault, HWDisableReason Disable)
        {
            SystemHost.Journal.AppendLog(ComplexParts.TOU, LogMessageType.Warning,
                                         string.Format(
                                             "TOU device notification: problem None, warning {0}, fault {1}, disable {2}",
                                             Warning, Fault, Disable));

            m_Communication.PostTOUNotificationEvent(Warning, Fault, Disable);
        }

        private void FireExceptionEvent(string Message)
        {
            SystemHost.Journal.AppendLog(ComplexParts.TOU, LogMessageType.Error, Message);
            m_Communication.PostExceptionEvent(ComplexParts.TOU, Message);
        }

        #endregion

        #region Registers

        private const ushort

            //Стандартные команды
            ACT_ENABLE_POWER = 1,
            ACT_DISABLE_POWER = 2,
            ACT_CLEAR_FAULT = 3,
            ACT_CLEAR_WARNING = 4,

            //Спецефичные команды
            ACT_START_TEST = 100,
            ACT_STOP = 101,

            //Регистры, задающие режим работы:
            REG_CURRENT_VALUE = 128, // Test current amplitude (in A) 128 – амплитуда тока(в А) [160-1250];

            //Регистры измеренных значений:
            REG_MEAS_CURRENT_VALUE = 250, //фактическое значение прямого тока (в А);
            REG_MEAS_TIME_DELAY = 251, // измеренное значение задержки включения (в нс);
            REG_MEAS_TIME_ON = 252, // измеренное значение времени включения (в нс).

            REG_DEV_STATE = 192, // Device state
            REG_FAULT_REASON = 193, // Fault reason in the case DeviceState -> FAULT
            REG_DISABLE_REASON = 194, // Disbale reason in the case DeviceState -> DISABLE
            REG_WARNING = 195, // Warning if present
            REG_PROBLEM = 196, // Problem if present	
            REG_TEST_FINISHED = 197, // Indicates that test is done and there is result or fault

            //Значения регистра 197:
            OPRESULT_NONE = 0, // No information or not finished
            OPRESULT_OK = 1, // Operation was successful
            OPRESULT_FAIL = 2; // Operation failed

        #endregion
    }
}