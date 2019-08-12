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

        private readonly IOAdapter _IOAdapter;
        private readonly BroadcastCommunication _Communication;
        private readonly ushort _Node;
        private readonly bool _IsTOUEmulationHard;
        private bool _IsTOUEmulation;
        private TestParameters _Parameters;
        private DeviceConnectionState _ConnectionState;
        private volatile DeviceState _State;
        private volatile TestResults _Result;
        internal IOCommutation ActiveCommutation { get; set; }


        private int m_Timeout = 25000;

        internal IOTOU(IOAdapter Adapter, BroadcastCommunication Communication)
        {
            _IOAdapter = Adapter;
            _Communication = Communication;
            //Устанавливаем режим эмуляции
            _IsTOUEmulation = _IsTOUEmulationHard = Settings.Default.TOUEmulation;
            ///////////////////////////////////////////////////////////
            _Node = (ushort)Settings.Default.TOUNode;
            _Result = new TestResults();

            SystemHost.Journal.AppendLog(ComplexParts.TOU, LogMessageType.Info,
                                         String.Format("TOU created. Emulation mode: {0}", _IsTOUEmulation));
        }


        


        private void CheckDevStateThrow(HWDeviceState devState)
        {
            //if (devState == HWDeviceState.Fault)
            //    throw new Exception(string.Format("TOU is in fault state, reason: {0}",
            //                                      (HWFaultReason)ReadRegister(REG_FAULT_REASON)));
            //if (devState == HWDeviceState.Disabled)
            //    throw new Exception(string.Format("TOU is in disabled state, reason: {0}",
            //                                      (HWDisableReason)ReadRegister(REG_DISABLE_REASON)));

            if (devState == HWDeviceState.Fault)
            {
                var faultReason = (HWFaultReason)ReadRegister(REG_FAULT_REASON);
                FireNotificationEvent(HWWarningReason.None, faultReason,
                                      HWDisableReason.None);

                throw new Exception(string.Format("TOU is in fault state, reason: {0}", faultReason));
            }

            //if (devState == HWDeviceState.Disabled)
            //{
            //    var disableReason = (HWDisableReason)ReadRegister(REG_DISABLE_REASON);
            //    FireNotificationEvent(HWWarningReason.None,
            //                          HWFaultReason.None, disableReason);

            //    throw new Exception(string.Format("TOU is in disabled state, reason: {0}", disableReason));
            //}
        }

        private void WaitState(HWDeviceState needState)
        {
            var timeStamp = Environment.TickCount + m_Timeout;
            HWDeviceState devState;
            while (Environment.TickCount < timeStamp)
            {
                Thread.Sleep(100);

                devState = (HWDeviceState)
                           ReadRegister(REG_DEV_STATE);

                if (devState == needState)
                    break;

                CheckDevStateThrow(devState);
            }

            if (Environment.TickCount > timeStamp)
                throw new Exception("Timeout while waiting for device to power up");
        }

        internal DeviceConnectionState Initialize(bool Enable, int timeoutTOU)
        {
            m_Timeout = timeoutTOU;
            _IsTOUEmulation = _IsTOUEmulationHard || !Enable;

            _ConnectionState = DeviceConnectionState.ConnectionInProcess;
            FireConnectionEvent(_ConnectionState, "TOU initializing");

            if (_IsTOUEmulation)
            {
                _ConnectionState = DeviceConnectionState.ConnectionSuccess;
                FireConnectionEvent(_ConnectionState, "TOU initialized");

                return _ConnectionState;
            }

            try
            {
                var timeStamp = Environment.TickCount + m_Timeout;
                ClearWarning();

                var devState = (HWDeviceState)ReadRegister(REG_DEV_STATE);

                //Если блок в состоянии None то просто подаём сигнал включения
                if (devState == HWDeviceState.None)
                    CallAction(ACT_ENABLE_POWER);
                else if (devState != HWDeviceState.Ready)//Если какое то друго состояние отличное от готовного 
                {
                    //Выключаем питание
                    CallAction(ACT_DISABLE_POWER);
                    //Ждём перехода в выключенное состояние
                    WaitState(HWDeviceState.None);
                    //Включаем питание
                    CallAction(ACT_ENABLE_POWER);
                }

                //Ждём когда блок будет готов
                WaitState(HWDeviceState.Ready);
                
                _ConnectionState = DeviceConnectionState.ConnectionSuccess;

                FireConnectionEvent(_ConnectionState, "TOU initialized");
            }
            catch (Exception ex)
            {
                _ConnectionState = DeviceConnectionState.ConnectionFailed;
                FireConnectionEvent(_ConnectionState, String.Format("TOU initialization error: {0}", ex.Message));
            }

            return _ConnectionState;
        }

        internal void Deinitialize()
        {
            var oldState = _ConnectionState;

            _ConnectionState = DeviceConnectionState.DisconnectionInProcess;
            FireConnectionEvent(DeviceConnectionState.DisconnectionInProcess, "TOU disconnecting");

            try
            {
                if (!_IsTOUEmulation && oldState == DeviceConnectionState.ConnectionSuccess)
                {
                    Stop();
                    CallAction(ACT_DISABLE_POWER);
                }

                _ConnectionState = DeviceConnectionState.DisconnectionSuccess;
                FireConnectionEvent(DeviceConnectionState.DisconnectionSuccess, "TOU disconnected");
            }
            catch (Exception)
            {
                _ConnectionState = DeviceConnectionState.DisconnectionError;
                FireConnectionEvent(DeviceConnectionState.DisconnectionError, "TOU disconnection error");
            }
        }

        internal DeviceState Start(TestParameters Parameters, Types.Commutation.TestParameters commParameters)
        {
            _Parameters = Parameters;

            if (_State == DeviceState.InProcess)
                throw new Exception("TOU test is already started");

            _Result = new TestResults()
            {
                TestTypeId = _Parameters.TestTypeId,
            };

            if (!_IsTOUEmulation)
            {
                //Считываем регистр состояния
                var devState = (HWDeviceState)ReadRegister(REG_DEV_STATE);
                //Проверяем на наличие ошибки либо отключения
                CheckDevStateThrow(devState);
                if (devState != HWDeviceState.Ready)
                {
                    string error = "Launch test, TOU State not Ready, function Start";
                    SystemHost.Journal.AppendLog(ComplexParts.TOU, LogMessageType.Note, error);
                    throw new Exception(error);
                }
            }

            MeasurementLogicRoutine(commParameters);

            return _State;
        }

        internal void Stop()
        {
            CallAction(ACT_STOP);
            _State = DeviceState.Stopped;
        }

        internal bool IsReadyToStart()
        {
            var devState = (Types.TOU.HWDeviceState)ReadRegister(REG_DEV_STATE);
            return devState == HWDeviceState.None;
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

            if (!_IsTOUEmulation)
                value = _IOAdapter.Read16(_Node, Address);

            if (!SkipJournal)
                SystemHost.Journal.AppendLog(ComplexParts.TOU, LogMessageType.Note,
                                         string.Format("TOU @ReadRegister, address {0}, value {1}", Address, value));

            return value;
        }

        internal void WriteRegister(ushort Address, ushort Value, bool SkipJournal = false)
        {
            if (!SkipJournal)
                SystemHost.Journal.AppendLog(ComplexParts.TOU, LogMessageType.Note,
                                         string.Format("TOU @WriteRegister, address {0}, value {1}", Address, Value));

            if (_IsTOUEmulation)
                return;

            _IOAdapter.Write16(_Node, Address, Value);
        }

        internal void CallAction(ushort Action, bool SkipJournal = false)
        {
            if (!SkipJournal)
                SystemHost.Journal.AppendLog(ComplexParts.TOU, LogMessageType.Note,
                                         string.Format("TOU @Call, action {0}", Action));

            if (_IsTOUEmulation)
                return;

            _IOAdapter.Call(_Node, Action);
        }

        #endregion

        private void MeasurementLogicRoutine(Types.Commutation.TestParameters Commutation)
        {
            try
            {
                _State = DeviceState.InProcess;
                //FireTOUEvent(m_State, m_Result);

                if (_IsTOUEmulation)
                {
                    Random rand = new Random(DateTime.Now.Millisecond);

                    var randValue = rand.Next(0, 2);

                    if (randValue == 0)
                    {
                        _State = DeviceState.Problem;
                        Thread.Sleep(500);
                        //проверяем отображение Problem, Warning, Fault
                        FireNotificationEvent(HWWarningReason.AnperageOutOfRange, HWFaultReason.NoPotensialSignal, HWDisableReason.None);
                        Thread.Sleep(500);
                        FireTOUEvent(_State, _Result);
                    }
                    else
                    {
                        Thread.Sleep(2000);
                        _Result.ITM = (float)rand.NextDouble() * 1000;
                        _Result.TGD = (float)rand.NextDouble() * 1000;
                        _Result.TGT = (float)rand.NextDouble() * 1000;
                        _State = DeviceState.Success;
                        FireTOUEvent(_State, _Result);
                    }
                }
                else
                {
                    if (ActiveCommutation.Switch(Types.Commutation.CommutationMode.TOU, Commutation.CommutationType, Commutation.Position) == DeviceState.Fault)
                    {
                        _State = DeviceState.Fault;
                        FireTOUEvent(_State, _Result);
                        return;
                    }

                    WriteRegister(REG_CURRENT_VALUE, _Parameters.ITM);
                    CallAction(ACT_START_TEST);
                    WaitState(HWDeviceState.Ready);

                    ushort finish = ReadRegister(REG_TEST_FINISHED);
                    if (finish == OPRESULT_OK)
                    {
                        _Result.ITM = ReadRegister(REG_MEAS_CURRENT_VALUE);
                        _Result.TGD = ReadRegister(REG_MEAS_TIME_DELAY);
                        _Result.TGT = ReadRegister(REG_MEAS_TIME_ON);

                        _State = DeviceState.Success;
                        FireTOUEvent(_State, _Result);
                    }
                    else
                    {
                        FireTOUEvent(DeviceState.Problem, _Result);

                        HWFaultReason faultReason = (HWFaultReason)ReadRegister(REG_PROBLEM);
                        HWWarningReason warningReason = (HWWarningReason)ReadRegister(REG_WARNING);
                        FireNotificationEvent(warningReason, faultReason, HWDisableReason.None);
                        throw new Exception(string.Format("TOU device state != InProcess and register 197 = : {0}", finish));
                    }
                }

            }

            catch (Exception ex)
            {
                _State = DeviceState.Fault;
                FireTOUEvent(_State, _Result);
                FireExceptionEvent(ex.Message);

                throw;
            }

        }

        //internal short ReadRegisterS(ushort Address, bool SkipJournal = false)
        //{
        //    short value = 0;

        //    if (!m_IsTOUEmulation)
        //        value = m_IOAdapter.Read16S(m_Node, Address);

        //    if (!SkipJournal)
        //        SystemHost.Journal.AppendLog(ComplexParts.TOU, LogMessageType.Note,
        //                                 string.Format("TOU @ReadRegisterS, address {0}, value {1}", Address, value));

        //    return value;
        //}
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


        //private DeviceState WaitForEndOfTest()
        //{
        //    var timeStamp = Environment.TickCount + m_Timeout;

        //    while (Environment.TickCount < timeStamp)
        //    {

        //        var devState = (HWDeviceState)ReadRegister(REG_DEV_STATE, true);

        //        if (devState == HWDeviceState.Fault)
        //        {
        //            var faultReason = (HWFaultReason)ReadRegister(REG_FAULT_REASON);

        //            FireNotificationEvent(HWWarningReason.None, faultReason,
        //                                  HWDisableReason.None);
        //            throw new Exception(string.Format("TOU device is in fault state, reason: {0}", faultReason));
        //        }

        //        if (devState == HWDeviceState.Disabled)
        //        {
        //            var disableReason = (HWDisableReason)ReadRegister(REG_DISABLE_REASON);

        //            FireNotificationEvent(HWWarningReason.None,
        //                                  HWFaultReason.None, disableReason);
        //            throw new Exception(string.Format("TOU device is in disabled state, reason: {0}", disableReason));
        //        }

        //        if (devState != HWDeviceState.InProcess)
        //        {
        //            ushort finish = ReadRegister(REG_TEST_FINISHED);
        //            if(finish != OPRESULT_OK)
        //                throw new Exception(string.Format("TOU device state != InProcess and register 197 = : {0}", finish));

        //            var warning = (HWWarningReason)ReadRegister(REG_WARNING);

        //            if (warning != HWWarningReason.None)
        //            {
        //                FireNotificationEvent(warning, HWFaultReason.None,
        //                                      HWDisableReason.None);
        //                ClearWarning();
        //            }

        //            break;
        //        }

        //        Thread.Sleep(REQUEST_DELAY_MS);
        //    }

        //    if (Environment.TickCount > timeStamp)
        //    {
        //        FireExceptionEvent("Timeout while waiting for TOU test to end");
        //        throw new Exception("Timeout while waiting for TOU test to end");
        //    }

        //    return DeviceState.Success;
        //}

        #region Events

        private void FireConnectionEvent(DeviceConnectionState State, string Message)
        {
            SystemHost.Journal.AppendLog(ComplexParts.TOU, LogMessageType.Info, Message);
            _Communication.PostDeviceConnectionEvent(ComplexParts.TOU, State, Message);
        }

        private void FireTOUEvent(DeviceState State, TestResults Result)
        {
            var message = string.Format("TOU test state {0}", State);

            if (State == DeviceState.Success)
                message = string.Format("TOU test result {0} {1} {2}", Result.ITM, Result.TGD, Result.TGT);

            SystemHost.Journal.AppendLog(ComplexParts.TOU, LogMessageType.Info, message);
            _Communication.PostTOUEvent(State, Result);
        }

        private void FireNotificationEvent(HWWarningReason Warning, HWFaultReason Fault, HWDisableReason Disable)
        {
            SystemHost.Journal.AppendLog(ComplexParts.TOU, LogMessageType.Warning,
                                         string.Format(
                                             "TOU device notification: problem None, warning {0}, fault {1}, disable {2}",
                                             Warning, Fault, Disable));

            _Communication.PostTOUNotificationEvent(Warning, Fault, Disable);
        }

        private void FireExceptionEvent(string Message)
        {
            SystemHost.Journal.AppendLog(ComplexParts.TOU, LogMessageType.Error, Message);
            _Communication.PostExceptionEvent(ComplexParts.TOU, Message);
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