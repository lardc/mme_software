using SCME.Types;
using SCME.UIServiceConfig.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCME.Types.Impulse;
using System.Threading;
using SCME.Types.BaseTestParams;

namespace SCME.Service.IO
{
    public class IOImpulse
    {
        private const int REQUEST_DELAY_MS = 50;
        private readonly BroadcastCommunication _Communication;
        private readonly IOAdapter _IOAdapter;
        private readonly ushort _Node;
        private bool _IsImpulseEmulation;
        private DeviceConnectionState _connectionState;
        //private volatile DeviceState _State;
        private volatile TestResults _Result;
        internal IOCommutation ActiveCommutation { get; set; }


        private int _timeoutImpulse = 25000;

        internal IOImpulse(IOAdapter Adapter, BroadcastCommunication Communication)
        {
            _IOAdapter = Adapter;
            _Communication = Communication;
            //Устанавливаем режим эмуляции
            _IsImpulseEmulation = Settings.Default.ImpulseEmulation;
            ///////////////////////////////////////////////////////////
            _Node = (ushort)Settings.Default.TOUNode;
            //_Result = new TestResults();

            SystemHost.Journal.AppendLog(ComplexParts.Impulse, LogMessageType.Info,
                                         String.Format("Impulse created. Emulation mode: {0}", _IsImpulseEmulation));
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
                var faultReason = ReadRegister(REG_FAULT_REASON);
                FireNotificationEvent(0, 0, faultReason,0);

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
            var timeStamp = Environment.TickCount + _timeoutImpulse;
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

        private (bool alarm, int error) WaitStateWithSafety()
        {
            var timeStamp = Environment.TickCount + _timeoutImpulse;
            HWDeviceState devState;
            while (Environment.TickCount < timeStamp)
            {
                Thread.Sleep(100);

                devState = (HWDeviceState) ReadRegister(REG_DEV_STATE);

                if (devState == HWDeviceState.Alarm)
                    return (true, -1);

                var res = (HWFinishedState)ReadRegister(REG_FINISHED);
                if (res == HWFinishedState.Success)
                    return (false, -1);

                else if(res == HWFinishedState.Failed)
                    return (false, ReadRegister(REG_FAULT_REASON));

                CheckDevStateThrow(devState);
            }

            throw new Exception("Timeout while waiting for device to power up");
        }

        internal DeviceConnectionState Initialize(bool enable, int timeoutImpulse)
        {
            _timeoutImpulse = timeoutImpulse;

            _connectionState = DeviceConnectionState.ConnectionInProcess;
            FireConnectionEvent(_connectionState, "Impulse initializing");

            if (_IsImpulseEmulation)
            {
                _connectionState = DeviceConnectionState.ConnectionSuccess;
                FireConnectionEvent(_connectionState, "Impulse initialized");
                return _connectionState;
            }

            try
            {
                var timeStamp = Environment.TickCount + _timeoutImpulse;
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

                _connectionState = DeviceConnectionState.ConnectionSuccess;

                FireConnectionEvent(_connectionState, "Impulse initialized");
            }
            catch (Exception ex)
            {
                _connectionState = DeviceConnectionState.ConnectionFailed;
                FireConnectionEvent(_connectionState, String.Format("Impulse initialization error: {0}", ex.Message));
            }

            return _connectionState;
        }

        internal void Deinitialize()
        {
            var oldState = _connectionState;

            _connectionState = DeviceConnectionState.DisconnectionInProcess;
            FireConnectionEvent(DeviceConnectionState.DisconnectionInProcess, "Impulse disconnecting");

            try
            {
                if (!_IsImpulseEmulation && oldState == DeviceConnectionState.ConnectionSuccess)
                {
                    Stop();
                    CallAction(ACT_DISABLE_POWER);
                }

                _connectionState = DeviceConnectionState.DisconnectionSuccess;
                FireConnectionEvent(DeviceConnectionState.DisconnectionSuccess, "Impulse disconnected");
            }
            catch (Exception)
            {
                _connectionState = DeviceConnectionState.DisconnectionError;
                FireConnectionEvent(DeviceConnectionState.DisconnectionError, "Impulse disconnection error");
            }
        }

        internal bool Start(BaseTestParametersAndNormatives parameters, DutPackageType dutPackageType)
        {
            if (!_IsImpulseEmulation)
            {
                //Считываем регистр состояния
                var devState = (HWDeviceState)ReadRegister(REG_DEV_STATE);
                //Проверяем на наличие ошибки либо отключения
                CheckDevStateThrow(devState);
                if (devState != HWDeviceState.Ready)
                {
                    string error = "Launch test, Impulse State not Ready, function Start";
                    SystemHost.Journal.AppendLog(ComplexParts.Impulse, LogMessageType.Note, error);
                    throw new Exception(error);
                }
            }

            return MeasurementLogicRoutine(parameters, dutPackageType);
        }

        internal void Stop()
        {
            CallAction(ACT_STOP);
            //_State = DeviceState.Stopped;
        }



        #region Standart API

        internal void ClearFault()
        {
            SystemHost.Journal.AppendLog(ComplexParts.TOU, LogMessageType.Note, "TOU fault cleared");

            CallAction(ACT_CLR_FAULT);
        }

        private void ClearWarning()
        {
            SystemHost.Journal.AppendLog(ComplexParts.TOU, LogMessageType.Note, "TOU warning cleared");

            CallAction(ACT_CLR_WARNING);
        }

        internal ushort ReadRegister(ushort Address, bool SkipJournal = false)
        {
            ushort value = 0;

            if (!_IsImpulseEmulation)
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

            if (_IsImpulseEmulation)
                return;

            _IOAdapter.Write16(_Node, Address, Value);
        }

        internal void CallAction(ushort Action, bool SkipJournal = false)
        {
            if (!SkipJournal)
                SystemHost.Journal.AppendLog(ComplexParts.TOU, LogMessageType.Note,
                                         string.Format("TOU @Call, action {0}", Action));

            if (_IsImpulseEmulation)
                return;

            try
            {
                _IOAdapter.Call(_Node, Action);
            }
            catch (Exception ex)
            {
                SystemHost.Journal.AppendLog(ComplexParts.TOU, LogMessageType.Error, ex.ToString());
            }
        }

        #endregion

        private bool MeasurementLogicRoutine(BaseTestParametersAndNormatives parameters, DutPackageType dutPackageType)
        {
            try
            {
                //_State = DeviceState.InProcess;
                CallAction(ACT_CLR_WARNING);
                _Result = new TestResults();
                _Result.NumberPosition = parameters.NumberPosition;

                if (_IsImpulseEmulation)
                {
                    Random rand = new Random(DateTime.Now.Millisecond);

                    var randValue = rand.Next(0, 2);

                    if (parameters is Types.InputOptions.TestParameters)
                        if ((parameters as Types.InputOptions.TestParameters).ShowVoltage)
                            _Result.InputOptionsIsAmperage = true;
                            

                    _Result.Value = (float)rand.NextDouble() * 1000;
                    _Result.TestParametersType = parameters.TestParametersType;
                    //_State = DeviceState.Success;
                    FireImpulseEvent(DeviceState.Success, _Result);
                }
                else
                {
                    WriteRegister(REG_DUT_POSITION, (ushort)parameters.NumberPosition);
                    WriteRegister(REG_DUT_PACKAGE_TYPE, (ushort)dutPackageType);

                    switch (parameters)
                    {
                        case Types.InputOptions.TestParameters io:
                            WriteRegister(REG_MEASUREMENT_TYPE, 3);
                            WriteRegister(REG_CONTROL_TYPE, (ushort)io.TypeManagement);
                            WriteRegister(REG_AUX_1_VOLTAGE, io.AuxiliaryVoltagePowerSupply1);
                            WriteRegister(REG_AUX_2_VOLTAGE, io.AuxiliaryVoltagePowerSupply2);
                            WriteRegister(REG_AUX_1_CURRENT, io.AuxiliaryCurrentPowerSupply1);
                            WriteRegister(REG_AUX_2_CURRENT, io.AuxiliaryCurrentPowerSupply2);
                            WriteRegister(REG_CONTROL_CURRENT, io.ControlCurrent);
                            WriteRegister(REG_CONTROL_VOLTAGE, io.ControlVoltage);
                            break;
                        case Types.OutputLeakageCurrent.TestParameters lc:
                            WriteRegister(REG_MEASUREMENT_TYPE, 1);
                            WriteRegister(REG_CONTROL_TYPE, (ushort)lc.TypeManagement);
                            WriteRegister(REG_AUX_1_VOLTAGE, lc.AuxiliaryVoltagePowerSupply1);
                            WriteRegister(REG_AUX_2_VOLTAGE, lc.AuxiliaryVoltagePowerSupply2);
                            WriteRegister(REG_AUX_1_CURRENT, lc.AuxiliaryCurrentPowerSupply1);
                            WriteRegister(REG_AUX_2_CURRENT, lc.AuxiliaryCurrentPowerSupply2);
                            WriteRegister(REG_CONTROL_CURRENT, lc.ControlCurrent);
                            WriteRegister(REG_CONTROL_VOLTAGE, lc.ControlVoltage);
                            WriteRegister(REG_COMMUTATION_CURRENT, lc.SwitchedAmperage);
                            WriteRegister(REG_COMMUTATION_VOLTAGE, lc.SwitchedVoltage);
                            WriteRegister(REG_COMMUTATION_VOLTAGE_POLARITY, (ushort)lc.PolarityDCSwitchingVoltageApplication);
                            WriteRegister(REG_COMMUTATION_VOLTAGE_TYPE_LEAKAGE, (ushort)lc.ApplicationPolarityConstantSwitchingVoltage);
                            break;
                        case Types.OutputResidualVoltage.TestParameters rv:
                            WriteRegister(REG_MEASUREMENT_TYPE, 2);
                            WriteRegister(REG_CONTROL_TYPE, (ushort)rv.TypeManagement);
                            WriteRegister(REG_CONTROL_CURRENT, rv.ControlCurrent);
                            WriteRegister(REG_CONTROL_VOLTAGE, rv.ControlVoltage);
                            WriteRegister(REG_COMMUTATION_VOLTAGE_POLARITY, (ushort)rv.PolarityDCSwitchingVoltageApplication);
                            WriteRegister(REG_COMMUTATION_CURRENT, rv.SwitchedAmperage);
                            WriteRegister(REG_COMMUTATION_VOLTAGE, rv.SwitchedVoltage);
                            WriteRegister(REG_AUX_1_VOLTAGE, rv.AuxiliaryVoltagePowerSupply1);
                            WriteRegister(REG_AUX_2_VOLTAGE, rv.AuxiliaryVoltagePowerSupply2);
                            WriteRegister(REG_AUX_1_CURRENT, rv.AuxiliaryCurrentPowerSupply1);
                            WriteRegister(REG_AUX_2_CURRENT, rv.AuxiliaryCurrentPowerSupply2);
                            WriteRegister(REG_COMMUTATION_CURRENT_SHAPE, (ushort)rv.SwitchingCurrentPulseShape);
                            WriteRegister(REG_COMMUTATION_CURRENT_TIME, rv.SwitchingCurrentPulseDuration);
                            break;
                        case Types.ProhibitionVoltage.TestParameters pv:
                            WriteRegister(REG_MEASUREMENT_TYPE, 4);
                            WriteRegister(REG_CONTROL_TYPE, (ushort)pv.TypeManagement);
                            WriteRegister(REG_CONTROL_CURRENT, pv.ControlCurrent);
                            WriteRegister(REG_CONTROL_VOLTAGE, pv.ControlVoltage);
                            WriteRegister(REG_COMMUTATION_CURRENT, pv.SwitchedAmperage);
                            WriteRegister(REG_COMMUTATION_VOLTAGE, pv.SwitchedVoltage);
                            WriteRegister(REG_AUX_1_VOLTAGE, pv.AuxiliaryVoltagePowerSupply1);
                            WriteRegister(REG_AUX_2_VOLTAGE, pv.AuxiliaryVoltagePowerSupply2);
                            WriteRegister(REG_AUX_1_CURRENT, pv.AuxiliaryCurrentPowerSupply1);
                            WriteRegister(REG_AUX_2_CURRENT, pv.AuxiliaryCurrentPowerSupply2);
                            break;
                    }

                    CallAction(ACT_CLR_FAULT);
                    CallAction(ACT_CLR_WARNING);
                    CallAction(ACT_SET_ACTIVE);
                    CallAction(ACT_START_TEST);
                    var (alarm, res) = WaitStateWithSafety();

                    CallAction(ACT_SET_INACTIVE);
                    if (alarm)
                    {
                        FireAlarmEvent("Нарушен периметр безопасности");
                        return false;
                    }

                    if (res != -1)
                        throw new Exception($"Ошибка измерения, код{res}");

                    TestResults testResults = new TestResults();

                    switch (parameters)
                    {
                        case Types.InputOptions.TestParameters io:
                            testResults.TestParametersType = TestParametersType.InputOptions;
                            if (io.ShowAmperage)
                                testResults.Value = ReadRegister(REG_RESULT_CONTROL_VOLTAGE);
                            else
                            {
                                testResults.Value = ReadRegister(REG_RESULT_CONTROL_CURRENT);
                                testResults.InputOptionsIsAmperage = true;
                            }
                            break;
                        case Types.OutputLeakageCurrent.TestParameters lc:
                            testResults.TestParametersType = TestParametersType.OutputLeakageCurrent;
                            testResults.Value = ReadRegister(REG_RESULT_LEAKAGE_CURRENT);
                            break;
                        case Types.OutputResidualVoltage.TestParameters rv:
                            testResults.TestParametersType = TestParametersType.OutputResidualVoltage;
                            testResults.Value = ReadRegister(REG_RESULT_RESIDUAL_OUTPUT_VOLTAGE);
                            break;
                        case Types.ProhibitionVoltage.TestParameters pv:
                            testResults.TestParametersType = TestParametersType.ProhibitionVoltage;
                            testResults.Value = ReadRegister(REG_RESULT_PROHIBITION_VOLTAGE);
                            break;
                    }

                    //_State = DeviceState.Success;
                    FireImpulseEvent(DeviceState.Success, testResults);

                }

            }

            catch (Exception ex)
            {
                FireExceptionEvent(ex.Message);
                return false;
            }

            return true;
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
            SystemHost.Journal.AppendLog(ComplexParts.Impulse, LogMessageType.Info, Message);
            _Communication.PostDeviceConnectionEvent(ComplexParts.Impulse, State, Message);
        }

        private void FireImpulseEvent(DeviceState State, TestResults Result)
        {
            var message = string.Format("Impulse test state {0}", State);

            if (State == DeviceState.Success)
                message = string.Format("Impulse test result {0}", Result.Value);

            SystemHost.Journal.AppendLog(ComplexParts.Impulse, LogMessageType.Info, message);
            _Communication.PostImpulseEvent(State, Result);
        }

        private void FireNotificationEvent(ushort problem, ushort warning, ushort fault, ushort disable)
        {
            SystemHost.Journal.AppendLog(ComplexParts.Impulse, LogMessageType.Warning,
                                         string.Format(
                                             "Impulse device notification: problem None, warning {0}, fault {1}, disable {2}",
                                             warning, fault, disable));

            _Communication.PostImpulseNotificationEvent((ushort)problem, (ushort)warning, (ushort)fault, (ushort)disable);
        }

        private void FireExceptionEvent(string Message)
        {
            SystemHost.Journal.AppendLog(ComplexParts.Impulse, LogMessageType.Error, Message);
            _Communication.PostExceptionEvent(ComplexParts.Impulse, Message);
        }

        private void FireAlarmEvent(string Message)
        {
            SystemHost.Journal.AppendLog(ComplexParts.Impulse, LogMessageType.Note, Message);
            _Communication.PostAlarmEvent(ComplexParts.Impulse, Message);
        }

        #endregion

        #region Registers

        public const ushort

            ACT_ENABLE_POWER = 1, // Enable
            ACT_DISABLE_POWER = 2, // Disable
            ACT_CLR_FAULT = 3, // Clear fault
            ACT_CLR_WARNING = 4, // Clear warning

            ACT_START_TEST = 100, // Start test with defined parameters / Запуск процесса измерения
            ACT_STOP = 101, // Stop test sequence / Принудительная остановка процесса измерения
            ACT_SET_ACTIVE = 102, // Switch safety circuit to active mode / Активация системы безопасности
            ACT_SET_INACTIVE = 103, // Switch safety circuit to inactive mode / Деактивация системы безопасности

            REG_MEASUREMENT_TYPE = 128, // Measurement type / Тип измерения
            //1 – Ток утечки на выходе
            //2 – Выходное остаточное напряжение
            //3 – Параметры входа
            //4 – Напряжение запрета

            REG_DUT_PACKAGE_TYPE = 129, // DUT housing type / Тип корпуса
            //1 – А1
            //2 – И1
            //3 – И6
            //4 – Б1
            //5 – Б2
            //6 – Б3
            //7 – Б5
            //8 – В1
            //9 – В2
            //10 – В108
            //11 – Л1
            //12 – Л2
            //13 – Д1
            //14 – Д2
            //15 – Д192

            REG_DUT_POSITION = 130, // DUT position / Номер позиции
            //1 – 1
            //2 – 2
            //3 – 3
            REG_CONTROL_TYPE = 131, // Control type / Тип управления
            //1 – Постоянный ток
            //2 – Постоянное напряжение
            //3 – Переменное напряжение

            REG_CONTROL_VOLTAGE = 132, // Control voltage / Напряжение управления(in mV / мВ)
            REG_CONTROL_CURRENT = 133, // Control current / Ток управления(in mA / мА)
            REG_COMMUTATION_VOLTAGE_TYPE_LEAKAGE = 134, // Commutation voltage type while leakage measurements / Тип коммутируемого напряжения при измерении утечки
            //1 – Постоянное
            //2 – Переменное
            REG_COMMUTATION_VOLTAGE_POLARITY = 135, // Commutation voltage polarity / Полярность приложения постоянного коммутируемого напряжения
            //1 – Прямая
            //2 – Обратная
            REG_COMMUTATION_CURRENT_SHAPE = 136, // Commutation current shape / Форма импульса коммутируемого тока
            //1 – Трапеция
            //2 - Синус
            REG_COMMUTATION_CURRENT_TIME = 137, // Commutation current time / Длительность импульса коммутируемого тока(in ms /мс)
            REG_COMMUTATION_CURRENT = 138, // Commutation current / Коммутируемый ток(in mA / мА)
            REG_COMMUTATION_VOLTAGE = 139, // Commutation voltage / Коммутируемого напряжение(in mV / мВ)
            REG_AUX_1_VOLTAGE = 140, // Auxiliary power supply 1 voltage / Напряжение вспомогательного питания 2 (in mV / мВ)
            REG_AUX_1_CURRENT = 141, // Auxiliary power supply 1 current(in mA / мА)
            REG_AUX_2_VOLTAGE = 142, // Auxiliary power supply 1 voltage / Напряжение вспомогательного питания 2 (in mV / мВ)
            REG_AUX_2_CURRENT = 143, // Auxiliary power supply 1 current(in mA / мА)
            
            REG_DEV_STATE = 192, // Device state / Текущее состояние
            //0 – состояние после включения питания
            //1 – состояние fault(состояние ошибки, которое можно сбросить)
            //2 – состояние disabled(состояние ошибки, требующее перезапуска питания)
            //3 – включён и готов к работе
            //4 – в процессе измерения
            //5 – сработала система безопасности
            REG_FAULT_REASON = 193, // Fault reason in the case DeviceState -&gt; FAULT / Код причины состояния fault(если в состоянии fault)
            REG_DISABLE_REASON = 194, // Fault reason in the case DeviceState -&gt; DISABLED / Код причины состояния disabled(если в состоянии disabled)
            REG_WARNING = 195, // Warning if present / Код предупреждения
            REG_PROBLEM = 196, // Problem reason / Код проблемы
            REG_FINISHED = 197, // Indicates that test is done and there is result or fault / Код окончания измерений
            //0 - No information or not finished
            //1 - Operation was successful
            //3 - Operation failed


            REG_RESULT_LEAKAGE_CURRENT = 198, // Leakage current / Ток утечки на выходе(mA / мА)
            REG_RESULT_RESIDUAL_OUTPUT_VOLTAGE = 199, // Residual output voltage / Остаточное напряжение на выходе(mV / мВ)
            REG_RESULT_CONTROL_CURRENT = 200, // Control current / Ток управления(mA / мА)
            REG_RESULT_CONTROL_VOLTAGE = 201, // Control voltage / Напряжение управления(mV / мВ)
            REG_RESULT_PROHIBITION_VOLTAGE = 202 // Prohibition voltage / Напряжение запрета(mV / мВ)
            ;
        #endregion
    }
}
