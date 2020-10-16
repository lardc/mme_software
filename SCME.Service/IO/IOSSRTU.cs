using SCME.Types;
using SCME.UIServiceConfig.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCME.Types.SSRTU;
using System.Threading;
using SCME.Types.BaseTestParams;
using System.Windows.Forms;

namespace SCME.Service.IO
{
    public class IOSSRTU
    {
        private const int REQUEST_DELAY_MS = 50;
        private readonly BroadcastCommunication _Communication;
        private readonly IOAdapter _IOAdapter;
        private readonly ushort _Node;
        private bool _IsSSRTUEmulation;
        private DeviceConnectionState _connectionState;
        //private volatile DeviceState _State;
        private volatile TestResults _Result;
        internal IOCommutation ActiveCommutation { get; set; }
        public bool PressStop { get; set; } = false;

        private int _timeoutSSRTU = 25000;

        internal IOSSRTU(IOAdapter Adapter, BroadcastCommunication Communication)
        {
            _IOAdapter = Adapter;
            _Communication = Communication;
            //Устанавливаем режим эмуляции
            _IsSSRTUEmulation = Settings.Default.SSRTUEmulation;
            ///////////////////////////////////////////////////////////
            _Node = (ushort)Settings.Default.SSRTUNode;
            //_Result = new TestResults();

            SystemHost.Journal.AppendLog(ComplexParts.SSRTU, LogMessageType.Info,
                                         String.Format("SSRTU created. Emulation mode: {0}", _IsSSRTUEmulation));
        }






        private void WaitState(HWDeviceState needState)
        {
            var timeStamp = Environment.TickCount + _timeoutSSRTU;
            HWDeviceState devState = HWDeviceState.None;
            while (Environment.TickCount <= timeStamp)
            {
                Thread.Sleep(100);

                devState = (HWDeviceState)
                           ReadRegister(REG_DEV_STATE);

                if (devState == needState)
                    break;

                CheckDevStateThrow(devState);
            }

            if (Environment.TickCount > timeStamp)
                throw new Exception($"Timeout expired: expected state {needState} but actual is still {devState}");
        }

        private (bool alarm, int error) WaitStateWithSafety()
        {
            var timeStamp = Environment.TickCount + _timeoutSSRTU;
            HWDeviceState devState = HWDeviceState.None;
            while (Environment.TickCount <= timeStamp)
            {
                Thread.Sleep(100);

                devState = (HWDeviceState) ReadRegister(REG_DEV_STATE);

                if (devState == HWDeviceState.Alarm)
                {
                    CallAction(ACT_CLR_SAFETY);
                    return (true, -1);
                }

                var res = (HWFinishedState)ReadRegister(REG_FINISHED);
                if (res == HWFinishedState.Success)
                    return (false, -1);

                else if(res == HWFinishedState.Failed)
                    return (false, ReadRegister(REG_FAULT_REASON));

                CheckDevStateThrow(devState);
            }

            throw new Exception($"Timeout expired: REG_DEV_STATE = {devState} , wait state alarm or success");
        }

        internal DeviceConnectionState Initialize(bool enable, int timeoutSSRTU)
        {
            _timeoutSSRTU = timeoutSSRTU;

            _connectionState = DeviceConnectionState.ConnectionInProcess;
            FireConnectionEvent(_connectionState, "SSRTU initializing");

            if (_IsSSRTUEmulation)
            {
                _connectionState = DeviceConnectionState.ConnectionSuccess;
                FireConnectionEvent(_connectionState, "SSRTU initialized");
                return _connectionState;
            }

            try
            {
                ReadRegister(0);
                
                var timeStamp = Environment.TickCount + _timeoutSSRTU;
                ClearWarning();

                var devState = (HWDeviceState)ReadRegister(REG_DEV_STATE);

                if (devState == HWDeviceState.Fault)
                {
                    ClearFault();
                    devState = (HWDeviceState)ReadRegister(REG_DEV_STATE);
                    if (devState == HWDeviceState.Fault)
                        throw new Exception("SSRTU не удалось сбросить fault");
                }

                switch (devState)
                {
                    case HWDeviceState.None:
                        CallAction(ACT_ENABLE_POWER);
                        WaitState(HWDeviceState.Ready);
                        break;
                    case HWDeviceState.Fault:
                        break;
                    case HWDeviceState.Disabled:
                        throw new Exception("SSRTU требуется перезагрузка питания");
                    case HWDeviceState.Ready:
                        break;
                    case HWDeviceState.InProcess:
                        WaitState(HWDeviceState.Ready);
                        break;
                    case HWDeviceState.Alarm:
                        CallAction(ACT_CLR_SAFETY);
                        break;
                    default:
                        break;
                }

                _connectionState = DeviceConnectionState.ConnectionSuccess;
                FireConnectionEvent(_connectionState, "SSRTU initialized");
            }
            catch (Exception ex)
            {
                _connectionState = DeviceConnectionState.ConnectionFailed;
                FireConnectionEvent(_connectionState, String.Format("SSRTU initialization error: {0}", ex.Message));
            }

            return _connectionState;
        }

        internal void Deinitialize()
        {
            var oldState = _connectionState;

            _connectionState = DeviceConnectionState.DisconnectionInProcess;
            FireConnectionEvent(DeviceConnectionState.DisconnectionInProcess, "SSRTU disconnecting");

            try
            {
                if (!_IsSSRTUEmulation && oldState == DeviceConnectionState.ConnectionSuccess)
                {
                    Stop();
                    CallAction(ACT_DISABLE_POWER);
                }

                _connectionState = DeviceConnectionState.DisconnectionSuccess;
                FireConnectionEvent(DeviceConnectionState.DisconnectionSuccess, "SSRTU disconnected");
            }
            catch (Exception)
            {
                _connectionState = DeviceConnectionState.DisconnectionError;
                FireConnectionEvent(DeviceConnectionState.DisconnectionError, "SSRTU disconnection error");
            }
        }

        internal bool Start(BaseTestParametersAndNormatives parameters, DutPackageType dutPackageType)
        {
            if (PressStop)
                return false;
            if (!_IsSSRTUEmulation)
            {
                //Считываем регистр состояния
                var devState = (HWDeviceState)ReadRegister(REG_DEV_STATE);
                //Проверяем на наличие ошибки либо отключения
                CheckDevStateThrow(devState);
                if (devState != HWDeviceState.Ready)
                {
                    string error = "Launch test, SSRTU State not Ready, function Start";
                    SystemHost.Journal.AppendLog(ComplexParts.SSRTU, LogMessageType.Info, error);
                    throw new Exception(error);
                }
            }

            return MeasurementLogicRoutine(parameters, dutPackageType);
        }

        private void CheckDevStateThrow(HWDeviceState devState)
        {
            //if((HWDeviceState)ReadRegister(REG_DEV_STATE) == HWDeviceState.Disabled ||)
            //throw new NotImplementedException();
        }

        internal void Stop()
        {
            CallAction(ACT_STOP);
            PressStop = true;
            //_State = DeviceState.Stopped;
        }



        #region Standart API

        internal void ClearFault()
        {
            SystemHost.Journal.AppendLog(ComplexParts.SSRTU, LogMessageType.Info, "SSRTU fault cleared");

            CallAction(ACT_CLR_FAULT);
        }

        private void ClearWarning()
        {
            SystemHost.Journal.AppendLog(ComplexParts.SSRTU, LogMessageType.Info, "SSRTU warning cleared");

            CallAction(ACT_CLR_WARNING);
        }

        internal ushort ReadRegister(ushort Address, bool SkipJournal = false)
        {
            ushort value = 0;

            if (!_IsSSRTUEmulation)
                value = _IOAdapter.Read16(_Node, Address);

            if (!SkipJournal)
                SystemHost.Journal.AppendLog(ComplexParts.SSRTU, LogMessageType.Info,
                                         string.Format("SSRTU @ReadRegister, address {0}, value {1}", Address, value));

            return value;
        }

        internal void WriteRegister(ushort Address, ushort Value, bool SkipJournal = false)
        {
            if (!SkipJournal)
                SystemHost.Journal.AppendLog(ComplexParts.SSRTU, LogMessageType.Info,
                                         string.Format("SSRTU @WriteRegister, address {0}, value {1}", Address, Value));

            if (_IsSSRTUEmulation)
                return;

            _IOAdapter.Write16(_Node, Address, Value);
        }

        internal void CallAction(ushort Action, bool SkipJournal = false)
        {
            if (!SkipJournal)
                SystemHost.Journal.AppendLog(ComplexParts.SSRTU, LogMessageType.Info,
                                         string.Format("SSRTU @Call, action {0}", Action));

            if (_IsSSRTUEmulation)
                return;

            try
            {
                _IOAdapter.Call(_Node, Action);
            }
            catch (Exception ex)
            {
                SystemHost.Journal.AppendLog(ComplexParts.SSRTU, LogMessageType.Error, ex.ToString());
            }
        }

        #endregion



        private bool MeasurementLogicRoutine(BaseTestParametersAndNormatives parameters, DutPackageType dutPackageType)
        {
            //FireAlarmEvent("Нарушен периметр безопасности");
            //return false;

            //FireNotificationEvent();
            //return false;

            try
            {
                //_State = DeviceState.InProcess;
                CallAction(ACT_CLR_WARNING);
                _Result = new TestResults();
                _Result.NumberPosition = parameters.NumberPosition;

                if (_IsSSRTUEmulation)
                {
                    Thread.Sleep(1000);
                    Random rand = new Random(DateTime.Now.Millisecond);

                    var randValue = rand.Next(0, 2);

                    if (parameters is Types.InputOptions.TestParameters)
                        if ((parameters as Types.InputOptions.TestParameters).ShowVoltage)
                            _Result.InputOptionsIsAmperage = true;
                            

                    _Result.Value = (float)rand.NextDouble() * 1000;
                    _Result.TestParametersType = parameters.TestParametersType;
                    //_State = DeviceState.Success;
                    FireSSRTUEvent(DeviceState.Success, _Result);
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
                            WriteRegister(REG_AUX_1_VOLTAGE, (ushort)io.AuxiliaryVoltagePowerSupply1);
                            WriteRegister(REG_AUX_2_VOLTAGE, (ushort)io.AuxiliaryVoltagePowerSupply2);
                            WriteRegister(REG_AUX_1_CURRENT, (ushort)io.AuxiliaryCurrentPowerSupply1);
                            WriteRegister(REG_AUX_2_CURRENT, (ushort)io.AuxiliaryCurrentPowerSupply2);
                            WriteRegister(REG_CONTROL_CURRENT, (ushort)io.ControlCurrent);
                            WriteRegister(REG_CONTROL_VOLTAGE, (ushort)io.ControlVoltage);
                            break;
                        case Types.OutputLeakageCurrent.TestParameters lc:
                            WriteRegister(REG_MEASUREMENT_TYPE, 1);
                            WriteRegister(REG_CONTROL_TYPE, (ushort)lc.TypeManagement);
                            WriteRegister(REG_AUX_1_VOLTAGE, (ushort)lc.AuxiliaryVoltagePowerSupply1);
                            WriteRegister(REG_AUX_2_VOLTAGE, (ushort)lc.AuxiliaryVoltagePowerSupply2);
                            WriteRegister(REG_AUX_1_CURRENT, (ushort)lc.AuxiliaryCurrentPowerSupply1);
                            WriteRegister(REG_AUX_2_CURRENT, (ushort)lc.AuxiliaryCurrentPowerSupply2);
                            WriteRegister(REG_CONTROL_CURRENT, (ushort)lc.ControlCurrent);
                            WriteRegister(REG_CONTROL_VOLTAGE, (ushort)lc.ControlVoltage);
                            WriteRegister(REG_COMMUTATION_CURRENT, (ushort)lc.SwitchedAmperage);
                            WriteRegister(REG_COMMUTATION_VOLTAGE, (ushort)lc.SwitchedVoltage);
                            WriteRegister(REG_COMMUTATION_VOLTAGE_POLARITY, (ushort)lc.PolarityDCSwitchingVoltageApplication);
                            WriteRegister(REG_COMMUTATION_VOLTAGE_TYPE_LEAKAGE, (ushort)lc.ApplicationPolarityConstantSwitchingVoltage);
                            break;
                        case Types.OutputResidualVoltage.TestParameters rv:
                            WriteRegister(REG_MEASUREMENT_TYPE, 2);
                            WriteRegister(REG_CONTROL_TYPE, (ushort)rv.TypeManagement);
                            WriteRegister(REG_CONTROL_CURRENT, (ushort)rv.ControlCurrent);
                            WriteRegister(REG_CONTROL_VOLTAGE, (ushort)rv.ControlVoltage);
                            WriteRegister(REG_COMMUTATION_VOLTAGE_POLARITY, (ushort)rv.PolarityDCSwitchingVoltageApplication);
                            WriteRegister(REG_COMMUTATION_CURRENT, (ushort)rv.SwitchedAmperage);
                            WriteRegister(REG_COMMUTATION_VOLTAGE, (ushort)rv.SwitchedVoltage);
                            WriteRegister(REG_AUX_1_VOLTAGE, (ushort)rv.AuxiliaryVoltagePowerSupply1);
                            WriteRegister(REG_AUX_2_VOLTAGE, (ushort)rv.AuxiliaryVoltagePowerSupply2);
                            WriteRegister(REG_AUX_1_CURRENT, (ushort)rv.AuxiliaryCurrentPowerSupply1);
                            WriteRegister(REG_AUX_2_CURRENT, (ushort)rv.AuxiliaryCurrentPowerSupply2);
                            WriteRegister(REG_COMMUTATION_CURRENT_SHAPE, (ushort)rv.SwitchingCurrentPulseShape);
                            WriteRegister(REG_COMMUTATION_CURRENT_TIME, (ushort)rv.SwitchingCurrentPulseDuration);
                            break;
                        case Types.ProhibitionVoltage.TestParameters pv:
                            WriteRegister(REG_MEASUREMENT_TYPE, 4);
                            WriteRegister(REG_CONTROL_TYPE, (ushort)pv.TypeManagement);
                            WriteRegister(REG_CONTROL_CURRENT, (ushort)pv.ControlCurrent);
                            WriteRegister(REG_CONTROL_VOLTAGE, (ushort)pv.ControlVoltage);
                            WriteRegister(REG_COMMUTATION_CURRENT, (ushort)pv.SwitchedAmperage);
                            WriteRegister(REG_COMMUTATION_VOLTAGE, (ushort)pv.SwitchedVoltage);
                            WriteRegister(REG_AUX_1_VOLTAGE, (ushort)pv.AuxiliaryVoltagePowerSupply1);
                            WriteRegister(REG_AUX_2_VOLTAGE, (ushort)pv.AuxiliaryVoltagePowerSupply2);
                            WriteRegister(REG_AUX_1_CURRENT, (ushort)pv.AuxiliaryCurrentPowerSupply1);
                            WriteRegister(REG_AUX_2_CURRENT, (ushort)pv.AuxiliaryCurrentPowerSupply2);
                            break;
                    }

                    CallAction(ACT_CLR_FAULT);
                    CallAction(ACT_CLR_WARNING);
                    CallAction(ACT_SET_ACTIVE);
                    CallAction(ACT_START_TEST);

                    bool alarm;
                    int res;

                    try
                    {
                        (alarm, res) = WaitStateWithSafety();
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        CallAction(ACT_SET_INACTIVE);
                    }
                    
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
                    FireSSRTUEvent(DeviceState.Success, testResults);

                }

            }

            catch (Exception ex)
            {
                FireExceptionEvent(ex.Message);
                return false;
            }

            return true;
        }


        #region Events

        private void FireConnectionEvent(DeviceConnectionState State, string Message)
        {
            SystemHost.Journal.AppendLog(ComplexParts.SSRTU, LogMessageType.Info, Message);
            _Communication.PostDeviceConnectionEvent(ComplexParts.SSRTU, State, Message);
        }

        private void FireSSRTUEvent(DeviceState State, TestResults Result)
        {
            var message = string.Format("SSRTU test state {0}", State);

            if (State == DeviceState.Success)
                message = string.Format("SSRTU test result {0}", Result.Value);

            SystemHost.Journal.AppendLog(ComplexParts.SSRTU, LogMessageType.Info, message);
            _Communication.PostSSRTUEvent(State, Result);
        }

        private void FireNotificationEvent()
        {
            var fault = ReadRegister(REG_FAULT_REASON);
            var disable = ReadRegister(REG_DISABLE_REASON);
            var warning = ReadRegister(REG_WARNING);
            var problem = ReadRegister(REG_PROBLEM);

            SystemHost.Journal.AppendLog(ComplexParts.SSRTU, LogMessageType.Warning,$"SSRTU device notification: problem {problem} warning {warning}, fault {fault}, disable {disable}");

            _Communication.PostSSRTUNotificationEvent(problem, warning, fault, disable);
        }

        private void FireExceptionEvent(string Message)
        {
            SystemHost.Journal.AppendLog(ComplexParts.SSRTU, LogMessageType.Error, Message);
            _Communication.PostExceptionEvent(ComplexParts.SSRTU, Message);
        }

        private void FireAlarmEvent(string Message)
        {
            SystemHost.Journal.AppendLog(ComplexParts.SSRTU, LogMessageType.Note, Message);
            _Communication.PostAlarmEvent();
        }

        #endregion

        #region Registers

        public const ushort

            ACT_ENABLE_POWER = 1, // Enable
            ACT_DISABLE_POWER = 2, // Disable
            ACT_CLR_FAULT = 3, // Clear fault
            ACT_CLR_WARNING = 4, // Clear warning
            ACT_CLR_SAFETY =5, // Clear safety state

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
