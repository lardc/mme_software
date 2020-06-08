using System;
using System.Threading;
using SCME.Service.Properties;
using SCME.Types;
using SCME.Types.SCTU;

namespace SCME.Service.IO
{
    internal class IoSctu
    {
        #region Fields

        private readonly IOAdapter _ioAdapter;
        private readonly BroadcastCommunication _broadcastCommunication;
        private readonly bool _isSctuEmulationHard;
        private bool _isSctuEmulation;
        private ushort _node;
        private SctuTestParameters _testParameters;
        private DeviceConnectionState _deviceConnectionState;

        private volatile SctuTestResults _testResults;
        private volatile DeviceState m_State;
        private volatile SctuHwState _deviseState;

        private volatile bool _stop;

        private int _timeout;

        private const int ChargeTimeout = 60000;

        #endregion

        #region Commands

        /// <summary>
        /// перевод блока в начальное состояние (состояние после включения)
        /// </summary>
        private const ushort ACT_DS_NONE = 1;

        /// <summary>
        /// включить заряд конденсаторов
        /// </summary>
        private const ushort ACT_BAT_START_CHARGE = 2;

        /// <summary>
        /// очистить Fault
        /// </summary>
        private const ushort ACT_FAULT_CLEAR = 3;

        /// <summary>
        /// очистить Warning
        /// </summary>
        private const ushort ACT_WARNING_CLEAR = 4;

        /// <summary>
        /// сконфигурировать блок на заданное значение ударного тока
        /// </summary>
        private const ushort ACT_SC_PULSE_CONFIG = 100;

        /// <summary>
        /// запустить формирование ударного тока
        /// </summary>
        private const ushort ACT_SC_PULSE_START = 101;




        #endregion

        #region Registers

        #region Режим работы

        /// <summary>
        /// Значение ударного тока в Амперах. Диапазон задания 100-39000
        /// </summary>
        private const ushort REG_SC_VALUE = 64;

        /// <summary>
        /// Тип прибора, 1234 – диод, 5678 – тристор. По умолчанию – диод.
        /// </summary>
        private const ushort REG_DUT_TYPE = 65;

        /// <summary>
        /// Сопротивление шунта, мкОм
        /// </summary>
        private const ushort REG_R_SHUNT = 66;

        #endregion

        #region Регистры измеренных значений

        /// <summary>
        /// Измеренное значение прямое напряжение, мВ.
        /// </summary>
        private const ushort REG_DUT_U = 96;

        /// <summary>
        /// Измеренное значение ударного тока, А.
        /// </summary>
        private const ushort REG_DUT_I = 97;

        #endregion

        #region Регисты состояния

        /// <summary>
        /// Регистр состояния установки.
        /// </summary>
        private const ushort REG_DEV_STATE = 98;

        /// <summary>
        /// Код проблемы.
        /// </summary>
        private const ushort REG_FAULT_REASON = 99;

        /// <summary>
        /// Код предупреждения.
        /// </summary>
        private const ushort REG_WARNING = 100;

        #endregion

        #endregion



        internal IoSctu(IOAdapter ioAdapter, BroadcastCommunication broadcastCommunication)
        {
            _ioAdapter = ioAdapter;
            _broadcastCommunication = broadcastCommunication;
            _isSctuEmulationHard = Settings.Default.SctuEmulation;
            _isSctuEmulation = _isSctuEmulationHard;
            _node = (ushort)Settings.Default.SctuNode;
            _testResults = new SctuTestResults();
            SystemHost.Journal.AppendLog(ComplexParts.Sctu, LogMessageType.Info,
                                         String.Format("Sctu created. Emulation mode: {0}", Settings.Default.SctuEmulation));
        }

        internal DeviceConnectionState Initialize(bool isEnable, int timeOut = 25000)
        {
            _timeout = timeOut;
            _isSctuEmulation = _isSctuEmulationHard || !isEnable;

            _deviceConnectionState = DeviceConnectionState.ConnectionInProcess;
            FireConnectionEvent(_deviceConnectionState, "Sctu initializing");

            if (_isSctuEmulation)
            {
                _deviceConnectionState = DeviceConnectionState.ConnectionSuccess;
                _deviseState = SctuHwState.Ready;
                FireConnectionEvent(_deviceConnectionState, "Sctu initialized");

                return _deviceConnectionState;
            }
            try
            {
                var timeStamp = Environment.TickCount + ChargeTimeout;

                ClearWarning();


                var devState = (SctuHwState)ReadRegister(REG_DEV_STATE);
                if (devState != SctuHwState.Ready)
                {
                    if (devState == SctuHwState.Fault)
                    {
                        ClearFault();
                        Thread.Sleep(100);

                        devState = (SctuHwState)ReadRegister(REG_DEV_STATE);

                        if (devState == SctuHwState.Fault)
                            throw new Exception(string.Format("Sctu is in fault state, reason: {0}",
                                ReadRegister(REG_FAULT_REASON)));

                    }

                    if (devState == SctuHwState.Disabled)
                        throw new Exception("Sctu is in disabled state");

                    if (devState != SctuHwState.BatteryChargeStart)
                    {
                        CallAction(ACT_BAT_START_CHARGE);
                    }
                    
                }
                
                while (Environment.TickCount < timeStamp)
                {
                    Thread.Sleep(100);

                    devState = (SctuHwState)ReadRegister(REG_DEV_STATE);


                    if (devState == SctuHwState.Ready)
                    {
                        FiredSctuEvent(devState, _testResults);
                        break;
                    }
                        

                    if (devState == SctuHwState.Fault)
                        throw new Exception(string.Format("Sctu is in fault state, reason: {0}",
                            ReadRegister(REG_FAULT_REASON)));

                    if (devState == SctuHwState.Disabled)
                        throw new Exception("Sctu is in disabled state");
                }

                if (Environment.TickCount > timeStamp)
                {
                    throw new Exception("Timeout while waiting for device to power up");
                }
                    

                _deviceConnectionState = DeviceConnectionState.ConnectionSuccess;

                FireConnectionEvent(_deviceConnectionState, "Sctu initialized");

            }
            catch (Exception ex)
            {
                _deviceConnectionState = DeviceConnectionState.ConnectionFailed;
                FireConnectionEvent(_deviceConnectionState, string.Format("Sctu initialization error: {0}", ex.Message));
            }

            return _deviceConnectionState;
        }


        internal void Deinitialize()
        {
            var oldState = _deviceConnectionState;

            _deviceConnectionState = DeviceConnectionState.DisconnectionInProcess;
            FireConnectionEvent(DeviceConnectionState.DisconnectionInProcess, "Sctu disconnecting");

            try
            {
                if (!_isSctuEmulation && oldState == DeviceConnectionState.ConnectionSuccess)
                {
                    Stop();
                    //CallAction(ACT_DISABLE_POWER);
                }

                _deviceConnectionState = DeviceConnectionState.DisconnectionSuccess;
                FireConnectionEvent(DeviceConnectionState.DisconnectionSuccess, "Sctu disconnected");
            }
            catch (Exception)
            {
                _deviceConnectionState = DeviceConnectionState.DisconnectionError;
                FireConnectionEvent(DeviceConnectionState.DisconnectionError, "Sctu disconnection error");
            }
        }

        internal void Start(SctuTestParameters parameters)
        {
            _testParameters = parameters;


            _testResults = new SctuTestResults();

            _stop = false;

            ClearWarning();

            if (!_isSctuEmulation)
            {
                var devState = (SctuHwState)ReadRegister(REG_DEV_STATE);

                if (devState == SctuHwState.Fault)
                    throw new Exception(string.Format("Sctu is in fault state, reason: {0}",
                        ReadRegister(REG_FAULT_REASON)));

                if (devState == SctuHwState.Disabled)
                    throw new Exception("Sctu is in disabled state");
            }
            MeasurementLogicRoutine();

        }

        private void MeasurementLogicRoutine()
        {
            try
            {
                FiredSctuEvent(SctuHwState.InProcess, _testResults);

                WriteRegister(REG_DUT_TYPE, (ushort)_testParameters.Type);
                WriteRegister(REG_SC_VALUE, (ushort)_testParameters.Value);
                WriteRegister(REG_R_SHUNT, (ushort)_testParameters.ShuntResistance);

                if (!_isSctuEmulation)
                {
                    CallAction(ACT_SC_PULSE_CONFIG);
                    var devState = WaitForPulseConfig();
                    if (devState == SctuHwState.PulseConfigReady)
                    {
                        CallAction(ACT_SC_PULSE_START);
                        devState = WaitForTestEnd();
                        _testResults.VoltageValue = ReadRegister(REG_DUT_U);
                        _testResults.CurrentValue = ReadRegister(REG_DUT_I);
                        FiredSctuEvent(devState, _testResults);
                        devState = WaitForReady();
                        FiredSctuEvent(devState, _testResults);
                    }
                }
                else
                {
                    _deviseState = SctuHwState.PulseEnd;
                    _testResults.CurrentValue = 200;
                    _testResults.VoltageValue = 1000;
                    FiredSctuEvent(_deviseState,_testResults);
                    Thread.Sleep(2000);
                    _deviseState = SctuHwState.Ready;
                    FiredSctuEvent(_deviseState, _testResults);
                }


            }
            catch (Exception ex)
            {
                _deviseState = SctuHwState.Fault;
                FiredSctuEvent(_deviseState, _testResults);
                FireExceptionEvent(ex.Message);

                throw;
            }
        }

        private SctuHwState WaitForReady()
        {
            Thread.Sleep(65000);
            var timeStamp = Environment.TickCount + _timeout;
            while (Environment.TickCount < timeStamp)
            {
                Thread.Sleep(100);

                var state = (SctuHwState)ReadRegister(REG_DEV_STATE);


                if (state == SctuHwState.Ready)
                    return SctuHwState.Ready;

                if (state == SctuHwState.Fault)
                    throw new Exception(string.Format("Sctu is in fault state, reason: {0}",
                        ReadRegister(REG_FAULT_REASON)));

                if (state == SctuHwState.Disabled)
                    throw new Exception("Sctu is in disabled state");
            }
            throw new Exception("WaitForPulseConfig timeout");
        }

        private SctuHwState WaitForTestEnd()
        {
            var timeStamp = Environment.TickCount + _timeout;
            while (Environment.TickCount < timeStamp)
            {
                Thread.Sleep(100);

                var state = (SctuHwState)ReadRegister(REG_DEV_STATE);


                if (state == SctuHwState.PulseEnd)
                    return SctuHwState.PulseEnd;

                if (state == SctuHwState.Fault)
                    throw new Exception(string.Format("Sctu is in fault state, reason: {0}",
                        ReadRegister(REG_FAULT_REASON)));

                if (state == SctuHwState.Disabled)
                    throw new Exception("Sctu is in disabled state");
            }
            throw new Exception("WaitForPulseConfig timeout");
        }

        private SctuHwState WaitForPulseConfig()
        {
            var timeStamp = Environment.TickCount + _timeout;
            while (Environment.TickCount < timeStamp)
            {
                Thread.Sleep(100);

                var state = (SctuHwState)ReadRegister(REG_DEV_STATE);


                switch (state)
                {
                    case SctuHwState.PulseConfigReady:
                        return state;
                    case SctuHwState.Fault:
                        throw new Exception(string.Format("Sctu is in fault state, reason: {0}",
                            ReadRegister(REG_FAULT_REASON)));
                    case SctuHwState.Disabled:
                        throw new Exception("Sctu is in disabled state");
                }
            }
            throw new Exception("WaitForPulseConfig timeout");
        }

        internal void Stop()
        {
            _stop = true;
        }

        private void FireConnectionEvent(DeviceConnectionState state, string message)
        {
            SystemHost.Journal.AppendLog(ComplexParts.Sctu, LogMessageType.Info, message);
            _broadcastCommunication.PostDeviceConnectionEvent(ComplexParts.Sctu, state, message);
        }

        private void FiredSctuEvent(SctuHwState state, SctuTestResults result)
        {
            var message = string.Format("Sctu test state {0}", state);

            if (state == SctuHwState.WaitTimeOut)
                message = string.Format("Sctu test result - Voltage: {0}, Current: {1} ", result.VoltageValue, result.CurrentValue);

            SystemHost.Journal.AppendLog(ComplexParts.Sctu, LogMessageType.Info, message);
            _broadcastCommunication.PostSctuEvent(state, result);
        }

        //private void FireNotificationEvent(Types.Sctu.HWWarningReason Warning, Types.Sctu.HWFaultReason Fault, Types.Sctu.HWDisableReason Disable)
        //{
        //    SystemHost.Journal.AppendLog(ComplexParts.DvDt, LogMessageType.Warning,
        //                                 string.Format(
        //                                     "Sctu device notification: problem None, warning {0}, fault {1}, disable {2}",
        //                                     Warning, Fault, Disable));

        //    _broadcastCommunication.PostdVdtNotificationEvent(Warning, Fault, Disable);
        //}

        private void FireExceptionEvent(string Message)
        {
            SystemHost.Journal.AppendLog(ComplexParts.Sctu, LogMessageType.Error, Message);
            _broadcastCommunication.PostExceptionEvent(ComplexParts.Sctu, Message);
        }

        #region Standart API

        internal void ClearFault()
        {
            SystemHost.Journal.AppendLog(ComplexParts.Sctu, LogMessageType.Note, "Sctu fault cleared");

            CallAction(ACT_FAULT_CLEAR);
        }

        private void ClearWarning()
        {
            SystemHost.Journal.AppendLog(ComplexParts.Sctu, LogMessageType.Note, "Sctu warning cleared");

            CallAction(ACT_WARNING_CLEAR);
        }

        internal ushort ReadRegister(ushort address, bool skipJournal = false)
        {
            ushort value = 0;

            if (!_isSctuEmulation)
                value = _ioAdapter.Read16(_node, address);

            if (!skipJournal)
                SystemHost.Journal.AppendLog(ComplexParts.Sctu, LogMessageType.Note,
                                         string.Format("Sctu @ReadRegister, address {0}, value {1}", address, value));

            return value;
        }

        internal short ReadRegisterS(ushort address, bool skipJournal = false)
        {
            short value = 0;

            if (!_isSctuEmulation)
                value = _ioAdapter.Read16S(_node, address);

            if (!skipJournal)
                SystemHost.Journal.AppendLog(ComplexParts.Sctu, LogMessageType.Note,
                                         string.Format("Sctu @ReadRegisterS, address {0}, value {1}", address, value));

            return value;
        }

        internal void WriteRegister(ushort address, ushort value, bool skipJournal = false)
        {
            if (!skipJournal)
                SystemHost.Journal.AppendLog(ComplexParts.Sctu, LogMessageType.Note,
                                         string.Format("Sctu @WriteRegister, address {0}, value {1}", address, value));

            if (_isSctuEmulation)
                return;

            _ioAdapter.Write16(_node, address, value);
        }

        internal void CallAction(ushort action)
        {
            SystemHost.Journal.AppendLog(ComplexParts.Sctu, LogMessageType.Note,
                                         string.Format("Sctu @Call, action {0}", action));

            if (_isSctuEmulation)
                return;

            _ioAdapter.Call(_node, action);
        }

        #endregion

    }
}
