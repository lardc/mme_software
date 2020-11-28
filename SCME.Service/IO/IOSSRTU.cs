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

        private double ReadRegisterFrom1616To32(ushort addressLow, int power = 3, bool skipJournal = false)
        {
            return ReadRegisterFrom1616To32(addressLow, (ushort) (addressLow + 1), power, skipJournal);
        }
        
        private double ReadRegisterFrom1616To32(ushort addressLow, ushort addressHigh, int power = 3,  bool skipJournal = false)
        {
            if (_IsSSRTUEmulation)
                return 0;

            uint valueUl = ReadRegister(addressHigh);
            valueUl <<= 16;
            valueUl += ReadRegister(addressLow);
            
            if (!skipJournal)
                SystemHost.Journal.AppendLog(ComplexParts.SSRTU, LogMessageType.Info, $"SSRTU @ReadRegister, addressLow {addressLow}, addressHigh {addressHigh}, value {valueUl} ");

            return Math.Round(valueUl / Math.Pow(10, power), power);
        }
        
        private void WriteRegisterFrom32To1616(ushort addressLow, double value, int power = 3, bool skipJournal = false)
        {
            WriteRegisterFrom32To1616(addressLow, addressLow, value, power, skipJournal);
        }
        
        private void WriteRegisterFrom32To1616(ushort addressLow, ushort addressHigh, double value, int power = 3, bool skipJournal = false)
        {
            WriteRegisterFrom32To1616(addressLow, addressHigh, Convert.ToUInt32(value * Math.Pow(10, power)), skipJournal);
        }

        private void WriteRegisterFrom32To1616(ushort addressLow, ushort addressHigh, uint value, bool skipJournal = false)
        {
            if (!skipJournal)
                SystemHost.Journal.AppendLog(ComplexParts.SSRTU, LogMessageType.Info, $"SSRTU @WriteRegister32, addressLow {addressLow}, addressHigh {addressHigh}, value: {value}");

            if (_IsSSRTUEmulation)
                return;

            _IOAdapter.Write16(_Node, addressLow, (ushort)(value & 0xffff));
            _IOAdapter.Write16(_Node, addressHigh, (ushort)(value >> 16));
        }

        internal void CallAction(ushort Action, bool SkipJournal = false)
        {
            if (!SkipJournal)
                SystemHost.Journal.AppendLog(ComplexParts.SSRTU, LogMessageType.Info,
                                         string.Format("SSRTU @Call, action {0}", Action));

            if (_IsSSRTUEmulation)
                return;

                _IOAdapter.Call(_Node, Action);
        }

        #endregion



        private bool MeasurementLogicRoutine(BaseTestParametersAndNormatives parameters, DutPackageType dutPackageType)
        {
            //FireAlarmEvent("Нарушен периметр безопасности");
            //return false;

            //FireNotificationEvent();
            //return false;

            _Result = new TestResults();
            _Result.NumberPosition = parameters.NumberPosition;
            
            try
            {
                //_State = DeviceState.InProcess;
                CallAction(ACT_CLR_WARNING);

                if (_IsSSRTUEmulation)
                {
                    Thread.Sleep(1000);
                    Random rand = new Random(DateTime.Now.Millisecond);

                    var randValue = rand.Next(0, 2);

                    if (parameters is Types.InputOptions.TestParameters)
                        if ((parameters as Types.InputOptions.TestParameters).ShowVoltage)
                            _Result.InputOptionsIsAmperage = true;

                    //throw new Exception("System.Exception: Operation - @Call, Node - 0, Address - 100, Message - SCCI protocol error: code - UserError, details - 5");

                    _Result.Value = (float)rand.Next(1, 500) / 1000  ;
                    _Result.AuxiliaryCurrentPowerSupply1 = (float)rand.Next() / 1000;
                    _Result.AuxiliaryCurrentPowerSupply2 = (float)rand.Next() / 1000;
                    _Result.OpenResistance = (float)rand.NextDouble() / 1000;
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
                            WriteRegisterFrom32To1616(REG_AUX_PS1_VOLTAGE_LOW, REG_AUX_PS1_VOLTAGE_HIGH, io.AuxiliaryVoltagePowerSupply1,6);
                            WriteRegisterFrom32To1616(REG_AUX_PS2_VOLTAGE_LOW, REG_AUX_PS2_VOLTAGE_HIGH, io.AuxiliaryVoltagePowerSupply2, 6);
                            
                            if(io.ShowAmperage)
                            {
                                WriteRegisterFrom32To1616(REG_CONTROL_CURRENT_LOW, REG_CONTROL_CURRENT_HIGH, io.ControlCurrent);
                                WriteRegisterFrom32To1616(REG_INPUT_VOLTAGE_MAX_LOW, REG_INPUT_VOLTAGE_MAX_HIGH, io.InputVoltageMaximum, 6);
                            }
                            else
                            {
                                WriteRegisterFrom32To1616(REG_CONTROL_VOLTAGE_LOW, REG_CONTROL_VOLTAGE_HIGH, io.ControlVoltage, 6);
                                WriteRegisterFrom32To1616(REG_INPUT_AMPERAGE_MAX_LOW, REG_INPUT_AMPERAGE_MAX_HIGH, io.InputCurrentMaximum);
                            }

                            
                            WriteRegisterFrom32To1616(REG_AUX_1_CURRENT_MAX_LOW1, REG_AUX_1_CURRENT_MAX_HIGH1, io.AuxiliaryCurrentPowerSupplyMaximum1);
                            WriteRegisterFrom32To1616(REG_AUX_1_CURRENT_MAX_LOW2, REG_AUX_1_CURRENT_MAX_HIGH2, io.AuxiliaryCurrentPowerSupplyMaximum2);
                            break;
                        case Types.OutputLeakageCurrent.TestParameters lc:
                            WriteRegister(REG_MEASUREMENT_TYPE, 1);
                            WriteRegister(REG_CONTROL_TYPE, (ushort)lc.TypeManagement);
                            WriteRegisterFrom32To1616(REG_AUX_PS1_VOLTAGE_LOW, REG_AUX_PS1_VOLTAGE_HIGH, lc.AuxiliaryVoltagePowerSupply1, 6);
                            WriteRegisterFrom32To1616(REG_AUX_PS2_VOLTAGE_LOW, REG_AUX_PS2_VOLTAGE_HIGH, lc.AuxiliaryVoltagePowerSupply2, 6);

                            //WriteRegisterFrom32To1616(REG_COMM_CURRENT_LOW, REG_COMM_CURRENT_HIGH, lc.SwitchedAmperage);
                            WriteRegisterFrom32To1616(REG_COMM_VOLTAGE_LOW, REG_COMM_VOLTAGE_HIGH, lc.SwitchedVoltage, 6);

                            WriteRegister(REG_COMMUTATION_VOLTAGE_POLARITY, (ushort)lc.PolarityDCSwitchingVoltageApplication);
                            WriteRegister(REG_COMMUTATION_VOLTAGE_TYPE_LEAKAGE, (ushort)lc.ApplicationPolarityConstantSwitchingVoltage);

                            WriteRegisterFrom32To1616(REG_LEAKAGE_CURRENT_MAX_LOW, REG_LEAKAGE_CURRENT_MAX_HIGH, lc.LeakageCurrentMaximum);

                            if (lc.ShowAmperage)
                            {
                                WriteRegisterFrom32To1616(REG_CONTROL_CURRENT_LOW, REG_CONTROL_CURRENT_HIGH, lc.ControlCurrent);
                                WriteRegisterFrom32To1616(REG_INPUT_VOLTAGE_MAX_LOW, REG_INPUT_VOLTAGE_MAX_HIGH, lc.ControlVoltageMaximum, 6);
                            }
                            else
                            {
                                WriteRegisterFrom32To1616(REG_CONTROL_VOLTAGE_LOW, REG_CONTROL_VOLTAGE_HIGH, lc.ControlVoltage, 6);
                                WriteRegisterFrom32To1616(REG_INPUT_AMPERAGE_MAX_LOW, REG_INPUT_AMPERAGE_MAX_HIGH, lc.ControlCurrentMaximum);
                            }

                            break;
                        case Types.OutputResidualVoltage.TestParameters rv:
                            WriteRegister(REG_MEASUREMENT_TYPE, 2);
                            WriteRegister(REG_CONTROL_TYPE, (ushort)rv.TypeManagement);
                            WriteRegister(REG_COMMUTATION_VOLTAGE_POLARITY, (ushort)rv.PolarityDCSwitchingVoltageApplication);
                            WriteRegister(REG_OPEN_RESISTANCE, Convert.ToUInt16(rv.OpenState));

                            WriteRegisterFrom32To1616(REG_COMM_CURRENT_LOW, REG_COMM_CURRENT_HIGH, rv.SwitchedAmperage, 6);
                            //WriteRegisterFrom32To1616(REG_COMM_VOLTAGE_LOW, REG_COMM_VOLTAGE_HIGH, rv.SwitchedVoltage);

                            WriteRegisterFrom32To1616(REG_AUX_PS1_VOLTAGE_LOW, REG_AUX_PS1_VOLTAGE_HIGH, rv.AuxiliaryVoltagePowerSupply1,6);
                            WriteRegisterFrom32To1616(REG_AUX_PS2_VOLTAGE_LOW, REG_AUX_PS2_VOLTAGE_HIGH, rv.AuxiliaryVoltagePowerSupply2,6);
                            WriteRegister(REG_COMMUTATION_CURRENT_SHAPE, (ushort)rv.SwitchingCurrentPulseShape);
                            WriteRegister(REG_COMMUTATION_CURRENT_TIME, (ushort)rv.SwitchingCurrentPulseDuration);

                            WriteRegisterFrom32To1616(REG_OUTPUT_RESIDUAL_VOLTAGE_MAX_LOW, REG_OUTPUT_RESIDUAL_VOLTAGE_MAX_HIGH, rv.OutputResidualVoltageMaximum, 6);

                            if (rv.ShowAmperage)
                            {
                                WriteRegisterFrom32To1616(REG_CONTROL_CURRENT_LOW, REG_CONTROL_CURRENT_HIGH, rv.ControlCurrent);
                                WriteRegisterFrom32To1616(REG_INPUT_VOLTAGE_MAX_LOW, REG_INPUT_VOLTAGE_MAX_HIGH, rv.ControlVoltageMaximum, 6);
                            }
                            else
                            {
                                WriteRegisterFrom32To1616(REG_CONTROL_VOLTAGE_LOW, REG_CONTROL_VOLTAGE_HIGH, rv.ControlVoltage, 6);
                                WriteRegisterFrom32To1616(REG_INPUT_AMPERAGE_MAX_LOW, REG_INPUT_AMPERAGE_MAX_HIGH, rv.ControlCurrentMaximum);
                            }
                            break;
                        //case Types.ProhibitionVoltage.TestParameters pv:
                        //    WriteRegister(REG_MEASUREMENT_TYPE, 4);
                        //    WriteRegister(REG_CONTROL_TYPE, (ushort)pv.TypeManagement);
                        //    WriteRegisterFrom32To1616(REG_CONTROL_CURRENT_LOW, REG_CONTROL_CURRENT_HIGH, pv.ControlCurrent);
                        //    WriteRegisterFrom32To1616(REG_CONTROL_VOLTAGE_LOW, REG_CONTROL_VOLTAGE_HIGH, pv.ControlVoltage);

                        //    //WriteRegisterFrom32To1616(REG_COMM_CURRENT_LOW, REG_COMM_CURRENT_HIGH, pv.SwitchedAmperage);
                        //    //WriteRegisterFrom32To1616(REG_COMM_VOLTAGE_LOW, REG_COMM_VOLTAGE_HIGH, pv.SwitchedVoltage);

                        //    WriteRegisterFrom32To1616(REG_AUX_PS1_VOLTAGE_LOW, REG_AUX_PS1_VOLTAGE_HIGH, pv.AuxiliaryVoltagePowerSupply1);
                        //    WriteRegisterFrom32To1616(REG_AUX_PS2_VOLTAGE_LOW, REG_AUX_PS2_VOLTAGE_HIGH, pv.AuxiliaryVoltagePowerSupply2);
                        //    break;
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


                    switch (parameters)
                    {
                        case Types.InputOptions.TestParameters io:
                            _Result.TestParametersType = TestParametersType.InputOptions;
                            if (io.ShowAmperage)
                                _Result.Value = ReadRegisterFrom1616To32(REG_RESULT_CONTROL_VOLTAGE_LOW, REG_RESULT_CONTROL_VOLTAGE_HIGH, 6);
                            else
                            {
                                _Result.Value = ReadRegisterFrom1616To32(REG_RESULT_CONTROL_CURRENT_LOW, REG_RESULT_CONTROL_CURRENT_HIGH);
                                _Result.InputOptionsIsAmperage = true;
                            }
                            if(io.ShowAuxiliaryVoltagePowerSupply1)
                                _Result.AuxiliaryCurrentPowerSupply1 = ReadRegister(AUXILARY_CURRENT_POWER_SUPPLY1);
                            if (io.ShowAuxiliaryVoltagePowerSupply2)
                                _Result.AuxiliaryCurrentPowerSupply2 = ReadRegister(AUXILARY_CURRENT_POWER_SUPPLY2);
                            break;
                        case Types.OutputLeakageCurrent.TestParameters lc:
                            _Result.TestParametersType = TestParametersType.OutputLeakageCurrent;
                            _Result.Value = ReadRegisterFrom1616To32(REG_RESULT_LEAKAGE_CURRENT_LOW, REG_RESULT_LEAKAGE_CURRENT_HIGH);
                            break;
                        case Types.OutputResidualVoltage.TestParameters rv:
                            _Result.TestParametersType = TestParametersType.OutputResidualVoltage;
                            _Result.Value = ReadRegisterFrom1616To32(REG_RESULT_RESIDUAL_OUTPUT_VOLTAGE_LOW, REG_RESULT_RESIDUAL_OUTPUT_VOLTAGE_HIGH, 6);
                            if (rv.OpenState)
                                _Result.OpenResistance = ReadRegisterFrom1616To32(REG_OPEN_RESISTANCE_LOW, REG_OPEN_RESISTANCE_HIGH);
                            break;
                        //case Types.ProhibitionVoltage.TestParameters pv:
                        //    _Result.TestParametersType = TestParametersType.ProhibitionVoltage;
                        //    _Result.Value = ReadRegisterFrom1616To32(REG_RESULT_PROHIBITION_VOLTAGE_LOW, REG_RESULT_PROHIBITION_VOLTAGE_HIGH);
                        //    break;
                    }

                    //_State = DeviceState.Success;
                    FireSSRTUEvent(DeviceState.Success, _Result);

                }

            }

            catch (Exception ex)
            {
                FireNotificationEvent(ex.Message);
                return false;
            }

            return true;
        }

        internal void StartAttestation(int position, int parameter, AttestationType attestationType, uint value)
        {
            double formedValue;
            double measuredValue;

            if (_IsSSRTUEmulation)
            {
                Random rnd = new Random(DateTime.Now.Millisecond);
                formedValue = (uint)(Math.Abs(rnd.Next()) * 2);
                measuredValue = (uint)(Math.Abs(rnd.Next()) * 2);
                SystemHost.Journal.AppendLog(ComplexParts.SSRTU, LogMessageType.Info, $"SSRTU attestation emulation formedValue: {formedValue}, measuredValue: {measuredValue}");
                _Communication.FireSSRTUAttestation(formedValue, measuredValue);
                return;
            }

            WriteRegister(REG_DUT_POSITION, (ushort)parameter);
            WriteRegister(NODE_CODE, (ushort)parameter);
            WriteRegister(CAL_TYPE, (ushort)attestationType);
            WriteRegisterFrom32To1616(CAL_VALUE_LOW, CAL_VALUE_HIGHT, value);

            CallAction(ACT_CALIBRATE);

            formedValue = ReadRegisterFrom1616To32(REG_CALIBRATION_MEASURED_VALUE_LOW);
            measuredValue = ReadRegisterFrom1616To32(REG_CALIBRATION_MEASURED_VALUE_HIGHT);

            SystemHost.Journal.AppendLog(ComplexParts.SSRTU, LogMessageType.Info, $"SSRTU attestation formedValue: {formedValue}, measuredValue: {measuredValue}");
            _Communication.FireSSRTUAttestation(formedValue, measuredValue);
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

        private void FireNotificationEvent(string message)
        {
            var fault = ReadRegister(REG_FAULT_REASON);
            var disable = ReadRegister(REG_DISABLE_REASON);
            var warning = ReadRegister(REG_WARNING);
            var problem = ReadRegister(REG_PROBLEM);

            SystemHost.Journal.AppendLog(ComplexParts.SSRTU, LogMessageType.Warning,$"SSRTU device notification: problem {problem} warning {warning}, fault {fault}, disable {disable}");

            _Communication.PostSSRTUNotificationEvent(message, problem, warning, fault, disable);
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
            ACT_CALIBRATE = 104,

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

            REG_CONTROL_VOLTAGE_LOW = 132, // Control voltage / Напряжение управления(in mV / мВ)
            REG_CONTROL_VOLTAGE_HIGH = 150,

            REG_CONTROL_CURRENT_LOW = 133, // Control current / Ток управления(in mA / мА)
            REG_CONTROL_CURRENT_HIGH = 151,

            REG_CONTROL_VOLTAGE_MAXIMUM_LOW = 132,
            REG_CONTROL_VOLTAGE_MAXIMUM_HIGH = 150,

            REG_CONTROL_CURRENT_MAXIMUM_LOW = 133,
            REG_CONTROL_CURRENT_MAXIMUM_HIGH = 151,

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
            REG_COMM_CURRENT_LOW = 138, // Commutation current / Коммутируемый ток(in mA / мА)
            REG_COMM_CURRENT_HIGH = 152,
            REG_COMM_VOLTAGE_LOW = 139, // Commutation voltage / Коммутируемого напряжение(in mV / мВ)
            REG_COMM_VOLTAGE_HIGH = 153,
            REG_AUX_PS1_VOLTAGE_LOW = 140, // Auxiliary power supply 1 voltage / Напряжение вспомогательного питания 2 (in mV / мВ)
            REG_AUX_PS1_VOLTAGE_HIGH = 154,
            REG_AUX_1_CURRENT = 141, // Auxiliary power supply 1 current(in mA / мА)
            REG_AUX_PS2_VOLTAGE_LOW = 142, // Auxiliary power supply 1 voltage / Напряжение вспомогательного питания 2 (in mV / мВ)
            REG_AUX_PS2_VOLTAGE_HIGH = 156,
            REG_AUX_2_CURRENT = 143, // Auxiliary power supply 1 current(in mA / мА)
            REG_OPEN_RESISTANCE = 144,

            NODE_CODE = 150,
            CAL_TYPE = 161,
            CAL_VALUE_LOW = 162,
            CAL_VALUE_HIGHT = 163,

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


            REG_RESULT_LEAKAGE_CURRENT_LOW = 198, // Leakage current / Ток утечки на выходе(mA / мА)
            REG_RESULT_LEAKAGE_CURRENT_HIGH = 230,
            REG_RESULT_RESIDUAL_OUTPUT_VOLTAGE_LOW = 199, // Residual output voltage / Остаточное напряжение на выходе(mV / мВ)
            REG_RESULT_RESIDUAL_OUTPUT_VOLTAGE_HIGH = 231,
            REG_RESULT_CONTROL_CURRENT_LOW = 200, // Control current / Ток управления(mA / мА)
            REG_RESULT_CONTROL_CURRENT_HIGH = 232,
            REG_RESULT_CONTROL_VOLTAGE_LOW = 201, // Control voltage / Напряжение управления(mV / мВ)
            REG_RESULT_CONTROL_VOLTAGE_HIGH = 233, // Control voltage / Напряжение управления(mV / мВ)
            REG_RESULT_PROHIBITION_VOLTAGE_LOW = 202, // Prohibition voltage / Напряжение запрета(mV / мВ)
            REG_RESULT_PROHIBITION_VOLTAGE_HIGH = 234, // Prohibition voltage / Напряжение запрета(mV / мВ)


            AUXILARY_CURRENT_POWER_SUPPLY1 = 203,
            AUXILARY_CURRENT_POWER_SUPPLY2 = 204,
            REG_OPEN_RESISTANCE_LOW = 205,
            REG_OPEN_RESISTANCE_HIGH = 237,

            REG_CALIBRATION_GENERATED_VALUE_LOW = 230,
            REG_CALIBRATION_GENERATED_VALUE_HIGHT = 231,
            REG_CALIBRATION_MEASURED_VALUE_LOW = 232,
            REG_CALIBRATION_MEASURED_VALUE_HIGHT = 233,


            //MAX Registers
            //Ток утечки на выходе, макс. - 138 и 152 регистры
            REG_LEAKAGE_CURRENT_MAX_LOW = 138,
            REG_LEAKAGE_CURRENT_MAX_HIGH = 152,
            REG_OUTPUT_RESIDUAL_VOLTAGE_MAX_LOW = 139,
            REG_OUTPUT_RESIDUAL_VOLTAGE_MAX_HIGH = 153,
            REG_INPUT_AMPERAGE_MAX_LOW = 133,
            REG_INPUT_AMPERAGE_MAX_HIGH = 151,
            REG_INPUT_VOLTAGE_MAX_LOW = 132,
            REG_INPUT_VOLTAGE_MAX_HIGH = 150,
            REG_AUX_1_CURRENT_MAX_LOW1 = 141,
            REG_AUX_1_CURRENT_MAX_HIGH1 = 155,
            REG_AUX_1_CURRENT_MAX_LOW2 = 143,
            REG_AUX_1_CURRENT_MAX_HIGH2 = 157
            ;
        #endregion
    }
}
