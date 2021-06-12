using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using SCME.Service.Properties;
using SCME.Types;
using SCME.Types.BVT;
using SCME.Types.Commutation;
using SCME.UIServiceConfig.Properties;

namespace SCME.Service.IO
{
    internal class IOBvt
    {
        private const ushort EMU_DEFAULT_VDRM = 1775;
        private const float EMU_DEFAULT_IDRM = 1.0f;
        private const ushort EMU_DEFAULT_VRRM = 1100;
        private const float EMU_DEFAULT_IRRM = 1.1f;
        private const float EMU_DEFAULT_IDSM = 4;
        private const float EMU_DEFAULT_IRSM = 49;
        private const int REQUEST_DELAY_MS = 50;

        private readonly IOAdapter m_IOAdapter;
        private readonly BroadcastCommunication m_Communication;
        private readonly ushort m_Node;
        private readonly bool m_IsBVTEmulationHard;
        private readonly bool m_ReadGraph;
        private IOCommutation m_IOCommutation;
        private bool m_IsBVTEmulation;
        private Types.BVT.TestParameters m_Parameters;
        private DeviceConnectionState m_ConnectionState;
        private volatile Types.DeviceState m_State;
        private volatile Types.BVT.TestResults m_Result;
        private volatile bool m_Stop;

        private int m_Timeout = 25000;

        internal IOBvt(IOAdapter Adapter, BroadcastCommunication Communication)
        {
            m_IOAdapter = Adapter;
            m_Communication = Communication;
            m_IsBVTEmulationHard = Settings.Default.BVTEmulation;
            m_IsBVTEmulation = m_IsBVTEmulationHard;
            m_ReadGraph = Settings.Default.BVTReadGraph;

            m_Node = (ushort) Settings.Default.BVTNode;
            m_Result = new Types.BVT.TestResults();

            SystemHost.Journal.AppendLog(ComplexParts.BVT, LogMessageType.Info,
                String.Format("BVT created. Emulation mode: {0}", Settings.Default.BVTEmulation));
        }

        internal IOCommutation ActiveCommutation
        {
            get { return m_IOCommutation; }
            set { m_IOCommutation = value; }
        }

        internal DeviceConnectionState Initialize(bool Enable, int Timeout)
        {
            m_Timeout = Timeout;
            m_IsBVTEmulation = m_IsBVTEmulationHard || !Enable;

            m_ConnectionState = DeviceConnectionState.ConnectionInProcess;
            FireConnectionEvent(m_ConnectionState, "BVT initializing");

            if (m_IsBVTEmulation)
            {
                m_ConnectionState = DeviceConnectionState.ConnectionSuccess;
                FireConnectionEvent(m_ConnectionState, "BVT initialized");

                return m_ConnectionState;
            }

            try
            {
                var timeStamp = Environment.TickCount + m_Timeout;

                ClearWarning();

                var devState = (Types.BVT.HWDeviceState) ReadRegister(REG_DEV_STATE);
                if (devState != Types.BVT.HWDeviceState.DS_Powered)
                {
                    if (devState == Types.BVT.HWDeviceState.DS_Fault)
                    {
                        ClearFault();
                        Thread.Sleep(100);

                        devState = (Types.BVT.HWDeviceState) ReadRegister(REG_DEV_STATE);

                        if (devState == Types.BVT.HWDeviceState.DS_Fault)
                            throw new Exception(string.Format("BVT is in fault state, reason: {0}",
                                (Types.BVT.HWFaultReason) ReadRegister(REG_FAULT_REASON)));
                    }

                    if (devState == Types.BVT.HWDeviceState.DS_Disabled)
                        throw new Exception(string.Format("BVT is in disabled state, reason: {0}",
                            (Types.BVT.HWDisableReason) ReadRegister(REG_DISABLE_REASON)));

                    CallAction(ACT_ENABLE_POWER);
                }

                while (Environment.TickCount < timeStamp)
                {
                    Thread.Sleep(100);

                    devState = (Types.BVT.HWDeviceState)
                        ReadRegister(REG_DEV_STATE);

                    if (devState == Types.BVT.HWDeviceState.DS_Powered)
                        break;

                    if (devState == Types.BVT.HWDeviceState.DS_Fault)
                        throw new Exception(string.Format("BVT is in fault state, reason: {0}",
                            (Types.BVT.HWFaultReason) ReadRegister(REG_FAULT_REASON)));
                    if (devState == Types.BVT.HWDeviceState.DS_Disabled)
                        throw new Exception(string.Format("BVT is in disabled state, reason: {0}",
                            (Types.BVT.HWDisableReason) ReadRegister(REG_DISABLE_REASON)));
                }

                if (Environment.TickCount > timeStamp)
                    throw new Exception("Timeout while waiting for device to power up");

                m_ConnectionState = DeviceConnectionState.ConnectionSuccess;

                FireConnectionEvent(m_ConnectionState, "BVT initialized");
            }
            catch (Exception ex)
            {
                m_ConnectionState = DeviceConnectionState.ConnectionFailed;
                FireConnectionEvent(m_ConnectionState, String.Format("BVT initialization error: {0}", ex.Message));
            }

            return m_ConnectionState;
        }

        internal void Deinitialize()
        {
            var oldState = m_ConnectionState;

            m_ConnectionState = DeviceConnectionState.DisconnectionInProcess;
            FireConnectionEvent(DeviceConnectionState.DisconnectionInProcess, "BVT disconnecting");

            try
            {
                if (!m_IsBVTEmulation && oldState == DeviceConnectionState.ConnectionSuccess)
                {
                    Stop();
                    CallAction(ACT_DISABLE_POWER);
                }

                m_ConnectionState = DeviceConnectionState.DisconnectionSuccess;
                FireConnectionEvent(DeviceConnectionState.DisconnectionSuccess, "BVT disconnected");
            }
            catch (Exception)
            {
                m_ConnectionState = DeviceConnectionState.DisconnectionError;
                FireConnectionEvent(DeviceConnectionState.DisconnectionError, "BVT disconnection error");
            }
        }

        internal Types.DeviceState Start(Types.BVT.TestParameters parameters, Types.Commutation.TestParameters commParameters)
        {
            m_Parameters = parameters;

            if (m_State == Types.DeviceState.InProcess)
                throw new Exception("BVT test is already started");

            m_Result = new Types.BVT.TestResults {TestTypeId = parameters.TestTypeId};
            m_Stop = false;

            ClearWarning();

            if (!m_IsBVTEmulation)
            {
                var devState = (Types.BVT.HWDeviceState) ReadRegister(REG_DEV_STATE);
                if (devState == Types.BVT.HWDeviceState.DS_Fault)
                {
                    var faultReason = (Types.BVT.HWFaultReason) ReadRegister(REG_FAULT_REASON);
                    FireNotificationEvent(Types.BVT.HWProblemReason.None, Types.BVT.HWWarningReason.None, faultReason,
                        Types.BVT.HWDisableReason.None);

                    throw new Exception(string.Format("BVT is in fault state, reason: {0}", faultReason));
                }

                if (devState == Types.BVT.HWDeviceState.DS_Disabled)
                {
                    var disableReason = (Types.BVT.HWDisableReason) ReadRegister(REG_DISABLE_REASON);
                    FireNotificationEvent(Types.BVT.HWProblemReason.None, Types.BVT.HWWarningReason.None,
                        Types.BVT.HWFaultReason.None, disableReason);

                    throw new Exception(string.Format("BVT is in disabled state, reason: {0}", disableReason));
                }
            }

            m_State = Types.DeviceState.InProcess;
            FireBvtAllEvent(m_State);

            MeasurementLogicRoutine(commParameters);

            if (m_State != Types.DeviceState.Success)
                FireBvtAllEvent(m_State);

            return m_State;
        }

        internal void Stop()
        {
            CallAction(ACT_STOP);
            m_Stop = true;
            m_State = Types.DeviceState.Stopped;
        }

        internal bool IsReadyToStart()
        {
            var devState = (Types.BVT.HWDeviceState) ReadRegister(REG_DEV_STATE);

            return !((devState == Types.BVT.HWDeviceState.DS_Fault) || (devState == Types.BVT.HWDeviceState.DS_Disabled) || (m_State == Types.DeviceState.InProcess));
        }

        #region Standart API

        internal void ClearFault()
        {
            SystemHost.Journal.AppendLog(ComplexParts.BVT, LogMessageType.Note, "BVT fault cleared");

            CallAction(ACT_CLEAR_FAULT);
        }

        private void ClearWarning()
        {
            SystemHost.Journal.AppendLog(ComplexParts.BVT, LogMessageType.Note, "BVT warning cleared");

            CallAction(ACT_CLEAR_WARNING);
        }

        internal ushort ReadRegister(ushort Address, bool SkipJournal = false)
        {
            ushort value = 0;

            if (!m_IsBVTEmulation)
                value = m_IOAdapter.Read16(m_Node, Address);

            if (!SkipJournal)
                SystemHost.Journal.AppendLog(ComplexParts.BVT, LogMessageType.Note,
                    string.Format("BVT @ReadRegister, address {0}, value {1}", Address, value));

            return value;
        }

        internal short ReadRegisterS(ushort Address, bool SkipJournal = false)
        {
            short value = 0;

            if (!m_IsBVTEmulation)
                value = m_IOAdapter.Read16S(m_Node, Address);

            if (!SkipJournal)
                SystemHost.Journal.AppendLog(ComplexParts.BVT, LogMessageType.Note,
                    string.Format("BVT @ReadRegisterS, address {0}, value {1}", Address, value));

            return value;
        }

        internal ushort ReadDeviceState(bool skipJournal = false)
        {
            return ReadRegister(REG_DEV_STATE, skipJournal);
        }

        internal void WriteRegister(ushort Address, ushort Value, bool SkipJournal = false)
        {
            if (!SkipJournal)
                SystemHost.Journal.AppendLog(ComplexParts.BVT, LogMessageType.Note,
                    string.Format("BVT @WriteRegister, address {0}, value {1}", Address, Value));

            if (m_IsBVTEmulation)
                return;

            m_IOAdapter.Write16(m_Node, Address, Value);
        }

        internal void CallAction(ushort Action)
        {
            SystemHost.Journal.AppendLog(ComplexParts.BVT, LogMessageType.Note,
                string.Format("BVT @Call, action {0}", Action));

            if (m_IsBVTEmulation)
                return;

            m_IOAdapter.Call(m_Node, Action);
        }

        #endregion

     

//        private void MeasurementLogicRoutine(Types.Commutation.TestParameters Commutation)
//        {
//            var internalState = DeviceState.InProcess;
//
//            try
//            {
//                WriteRegister(REG_LIMIT_CURRENT, (ushort) (m_Parameters.CurrentLimit * 10));
//                WriteRegister(REG_PLATE_TIME, m_Parameters.PlateTime);
//                WriteRegister(REG_RAMPUP_RATE, (ushort) (m_Parameters.RampUpVoltage * 10));
//                WriteRegister(REG_START_VOLTAGE, m_Parameters.StartVoltage);
//                WriteRegister(REG_VOLTAGE_FREQUENCY, m_Parameters.VoltageFrequency);
//                WriteRegister(REG_FREQUENCY_DIVISOR, m_Parameters.FrequencyDivisor);
//                WriteRegister(REG_MEASUREMENT_MODE,
//                    m_Parameters.MeasurementMode == Types.BVT.BVTMeasurementMode.ModeI
//                        ? MEASURE_MODE_I
//                        : MEASURE_MODE_V);
//
//                if (m_Parameters.TestType == Types.BVT.BVTTestType.Both ||
//                    m_Parameters.TestType == Types.BVT.BVTTestType.Direct)
//                {
//                    FireBvtDirectEvent(internalState, m_Result);
//
//                    if (m_IOCommutation.Switch(Types.Commutation.CommutationMode.BVTD, Commutation.CommutationType, Commutation.Position) ==
//                        DeviceState.Fault)
//                    {
//                        m_State = DeviceState.Fault;
//                        FireBvtAllEvent(m_State);
//                        return;
//                    }
//
//                    WriteRegister(REG_LIMIT_VOLTAGE, m_Parameters.VoltageLimitD);
//                    WriteRegister(REG_MEASUREMENT_TYPE, MEASURE_TYPE_AC_D);
//
//                    CallAction(ACT_START_TEST);
//
//                    if (!m_IsBVTEmulation)
//                    {
//                        internalState = WaitForEndOfTest();
//
//                        m_Result.VDRM = (ushort) Math.Abs(ReadRegisterS(REG_RESULT_V));
//                        m_Result.IDRM = Math.Abs(ReadRegisterS(REG_RESULT_I) / 10.0f);
//
//                        if (m_ReadGraph)
//                            ReadArrays(m_Result, true);
//                    }
//                    else
//                    {
//                        internalState = DeviceState.Success;
//
//                        m_Result.VDRM = EMU_DEFAULT_VDRM;
//                        m_Result.IDRM = EMU_DEFAULT_IDRM;
//                    }
//
//                    FireBvtDirectEvent(internalState, m_Result);
//
//                    if (m_IOCommutation.Switch(Types.Commutation.CommutationMode.None) == DeviceState.Fault)
//                    {
//                        m_State = DeviceState.Fault;
//                        FireBvtAllEvent(m_State);
//                        return;
//                    }
//                }
//                else
//                    internalState = DeviceState.Success;
//
//                if ((internalState == DeviceState.Success)
//                    && (m_Parameters.TestType == Types.BVT.BVTTestType.Both ||
//                        m_Parameters.TestType == Types.BVT.BVTTestType.Reverse))
//                {
//                    internalState = DeviceState.InProcess;
//                    FireBvtReverseEvent(internalState, m_Result);
//
//                    if (m_IOCommutation.Switch(Types.Commutation.CommutationMode.BVTR, Commutation.CommutationType, Commutation.Position) ==
//                        DeviceState.Fault)
//                    {
//                        m_State = DeviceState.Fault;
//                        FireBvtAllEvent(m_State);
//                        return;
//                    }
//
//                    WriteRegister(REG_LIMIT_VOLTAGE, m_Parameters.VoltageLimitR);
//                    WriteRegister(REG_MEASUREMENT_TYPE, MEASURE_TYPE_AC_R);
//                    CallAction(ACT_START_TEST);
//
//                    if (!m_IsBVTEmulation)
//                    {
//                        internalState = WaitForEndOfTest();
//
//                        m_Result.VRRM = (ushort) Math.Abs(ReadRegisterS(REG_RESULT_V));
//                        m_Result.IRRM = Math.Abs(ReadRegisterS(REG_RESULT_I) / 10.0f);
//
//                        if (m_ReadGraph)
//                            ReadArrays(m_Result, false);
//                    }
//                    else
//                    {
//                        internalState = DeviceState.Success;
//
//                        m_Result.VRRM = EMU_DEFAULT_VRRM;
//                        m_Result.IRRM = EMU_DEFAULT_IRRM;
//                    }
//
//                    FireBvtReverseEvent(internalState, m_Result);
//                }
//
//                if (m_IOCommutation.Switch(Types.Commutation.CommutationMode.None) == DeviceState.Fault)
//                {
//                    m_State = DeviceState.Fault;
//                    FireBvtAllEvent(m_State);
//                    return;
//                }
//
//                m_State = internalState;
//            }
//            catch (Exception ex)
//            {
//                m_IOCommutation.Switch(Types.Commutation.CommutationMode.None);
//
//                m_State = DeviceState.Fault;
//                FireBvtAllEvent(m_State);
//                FireExceptionEvent(ex.Message);
//
//                throw;
//            }
//        }


        private ushort CalcUdsmUrsmVoltage()
        {
            //вычисляет значение напряжения, при котором выполняется тест UDSM/URSM
            ushort result = (m_Parameters.ClassByProfileName == null) ? (ushort) 0 : (ushort) (110 + 100 * m_Parameters.ClassByProfileName);

            return result;
        }

//        private void MeasurementLogicRoutineUdsmUrsm(Types.Commutation.TestParameters Commutation)
//        {
//            //реализация теста UDSM/URSM
//            //это часть группы тестов BVT, он может быть исполнен только после теста BVT MeasurementLogicRoutine. предшедствующий данному тесту UDSM/URSM тест BVT поставляет исходные данные для данного теста
//            //если профиль измерения не предполагает исполнение теста BVT, то и тест UDSM/URSM никогда не будет исполнен
//
//            //проверяем состояние флага UDSM/URSM, исполняем тест UDSM/URSM только если он установлен
//            if (m_Parameters.UseUdsmUrsm)
//            {
//                //для исполнения данного теста блок должен находится в состоянии DS_Powered
//                if (!m_IsBVTEmulation)
//                {
//                    var devState = (Types.BVT.HWDeviceState) ReadRegister(REG_DEVICE_STATE, true);
//
//                    if (devState != Types.BVT.HWDeviceState.PowerReady)
//                        throw new Exception("BVT is not in 'PowerReady' state, UDSM/URSM test is not possible");
//                }
//
//                var internalState = DeviceState.InProcess;
//
//                try
//                {
//                    m_State = DeviceState.InProcess;
//
//                    WriteRegister(REG_LIMIT_CURRENT, (ushort) (m_Parameters.UdsmUrsmCurrentLimit * 10));
//                    WriteRegister(REG_PLATE_TIME, m_Parameters.UdsmUrsmPlateTime);
//                    WriteRegister(REG_RAMPUP_RATE, (ushort) (m_Parameters.UdsmUrsmRampUpVoltage * 10));
//                    WriteRegister(REG_START_VOLTAGE, m_Parameters.UdsmUrsmStartVoltage);
//                    WriteRegister(REG_VOLTAGE_FREQUENCY, m_Parameters.UdsmUrsmVoltageFrequency);
//                    WriteRegister(REG_FREQUENCY_DIVISOR, m_Parameters.UdsmUrsmFrequencyDivisor);
//                    WriteRegister(REG_MEASUREMENT_MODE, MEASURE_MODE_I);
//
//
//                    if (m_Parameters.UdsmUrsmTestType == Types.BVT.BVTTestType.Both || m_Parameters.UdsmUrsmTestType == Types.BVT.BVTTestType.Direct)
//                    {
//                        FireBvtUdsmUrsmDirectEvent(internalState, m_Result);
//
//                        if (m_IOCommutation.Switch(Types.Commutation.CommutationMode.BVTD, Commutation.CommutationType, Commutation.Position) == DeviceState.Fault)
//                        {
//                            m_State = DeviceState.Fault;
//                            FireBvtAllEvent(m_State);
//                            return;
//                        }
//
//                        ushort VDSM = CalcUdsmUrsmVoltage();
//                        WriteRegister(REG_LIMIT_VOLTAGE, VDSM);
//                        WriteRegister(REG_MEASUREMENT_TYPE, MEASURE_TYPE_AC_D);
//
//                        CallAction(ACT_START_TEST);
//
//                        m_Result.VDSM = VDSM;
//
//                        if (m_IsBVTEmulation)
//                        {
//                            internalState = DeviceState.Success;
//                            m_Result.IDSM = EMU_DEFAULT_IDSM;
//                        }
//                        else
//                        {
//                            internalState = WaitForEndOfTest();
//                            m_Result.IDSM = Math.Abs(ReadRegisterS(REG_RESULT_I) / 10.0f);
//                        }
//
//                        FireBvtUdsmUrsmDirectEvent(internalState, m_Result);
//
//                        if (m_IOCommutation.Switch(Types.Commutation.CommutationMode.None) == DeviceState.Fault)
//                        {
//                            m_State = DeviceState.Fault;
//                            FireBvtAllEvent(m_State);
//                            return;
//                        }
//                    }
//                    else
//                        internalState = DeviceState.Success;
//
//                    if ((internalState == DeviceState.Success) && (m_Parameters.UdsmUrsmTestType == Types.BVT.BVTTestType.Both || m_Parameters.UdsmUrsmTestType == Types.BVT.BVTTestType.Reverse))
//                    {
//                        internalState = DeviceState.InProcess;
//                        FireBvtUdsmUrsmReverseEvent(internalState, m_Result);
//
//                        if (m_IOCommutation.Switch(Types.Commutation.CommutationMode.BVTR, Commutation.CommutationType, Commutation.Position) == DeviceState.Fault)
//                        {
//                            m_State = DeviceState.Fault;
//                            FireBvtAllEvent(m_State);
//                            return;
//                        }
//
//                        ushort VRSM = CalcUdsmUrsmVoltage();
//                        WriteRegister(REG_LIMIT_VOLTAGE, VRSM);
//                        WriteRegister(REG_MEASUREMENT_TYPE, MEASURE_TYPE_AC_R);
//
//                        CallAction(ACT_START_TEST);
//
//                        m_Result.VRSM = VRSM;
//
//                        if (m_IsBVTEmulation)
//                        {
//                            internalState = DeviceState.Success;
//                            m_Result.IRSM = EMU_DEFAULT_IRSM;
//                        }
//                        else
//                        {
//                            internalState = WaitForEndOfTest();
//                            m_Result.IRSM = Math.Abs(ReadRegisterS(REG_RESULT_I) / 10.0f);
//                        }
//
//                        FireBvtUdsmUrsmReverseEvent(internalState, m_Result);
//                    }
//
//                    if (m_IOCommutation.Switch(Types.Commutation.CommutationMode.None) == DeviceState.Fault)
//                    {
//                        m_State = DeviceState.Fault;
//                        FireBvtAllEvent(m_State);
//                        return;
//                    }
//
//                    m_State = internalState;
//                }
//                catch (Exception ex)
//                {
//                    m_IOCommutation.Switch(Types.Commutation.CommutationMode.None);
//
//                    m_State = DeviceState.Fault;
//                    FireBvtAllEvent(m_State);
//                    FireExceptionEvent(ex.Message);
//
//                    throw;
//                }
//            }
//        }

        private void ReadArrays(Types.BVT.TestResults Result, bool IsDirect)
        {
            List<short> bufferArray;

            SystemHost.Journal.AppendLog(ComplexParts.BVT, LogMessageType.Note,
                "BVT @ReadArrays begin");

            Result.CurrentData.Clear();
            Result.VoltageData.Clear();

            m_IOAdapter.Call(m_Node, ACT_READ_MOVE_BACK);
            do
            {
                m_IOAdapter.Call(m_Node, ACT_READ_FRAGMENT);

                bufferArray = m_IOAdapter.ReadArrayFast16S(m_Node, ARR_SCOPE_I).ToList();
                Result.CurrentData.AddRange(bufferArray);

                bufferArray = m_IOAdapter.ReadArrayFast16S(m_Node, ARR_SCOPE_V).ToList();
                Result.VoltageData.AddRange(bufferArray);
            } while (bufferArray.Count > 0);

            Result.CurrentData =
                Result.CurrentData.Select(
                    Value => IsDirect ? (Value < 0 ? Math.Abs(Value) : (short) 0) : (Value > 0 ? (short) 0 : Value)).ToList();
            Result.VoltageData =
                Result.VoltageData.Select(
                    Value => IsDirect ? (Value < 0 ? Math.Abs(Value) : (short) 0) : (Value > 0 ? (short) 0 : Value)).ToList();

            SystemHost.Journal.AppendLog(ComplexParts.BVT, LogMessageType.Note,
                string.Format("BVT @ReadArrays data length {0}", Result.VoltageData.Count));

            SystemHost.Journal.AppendLog(ComplexParts.BVT, LogMessageType.Note,
                "BVT @ReadArrays end");
        }

        private Types.DeviceState WaitForEndOfTest()
        {
            var timeStamp = Environment.TickCount + m_Timeout;

            while (Environment.TickCount < timeStamp)
            {
                if (m_Stop)
                {
                    CallAction(ACT_STOP);
                    return Types.DeviceState.Stopped;
                }

                var devState = (Types.BVT.HWDeviceState) ReadRegister(REG_DEV_STATE, true);
                var opResult = (Types.BVT.HWOperationResult) ReadRegister(REG_TEST_FINISHED, true);

                if (devState == Types.BVT.HWDeviceState.DS_Fault)
                {
                    var faultReason = (Types.BVT.HWFaultReason) ReadRegister(REG_FAULT_REASON);

                    FireNotificationEvent(Types.BVT.HWProblemReason.None, Types.BVT.HWWarningReason.None, faultReason,
                        Types.BVT.HWDisableReason.None);
                    throw new Exception(string.Format("BVT device is in fault state, reason: {0}", faultReason));
                }

                if (devState == Types.BVT.HWDeviceState.DS_Disabled)
                {
                    var disableReason = (Types.BVT.HWDisableReason) ReadRegister(REG_DISABLE_REASON);

                    FireNotificationEvent(Types.BVT.HWProblemReason.None, Types.BVT.HWWarningReason.None,
                        Types.BVT.HWFaultReason.None, disableReason);
                    throw new Exception(string.Format("BVT device is in disabled state, reason: {0}", disableReason));
                }

                if (opResult != Types.BVT.HWOperationResult.InProcess)
                {
                    var warning = (Types.BVT.HWWarningReason) ReadRegister(REG_WARNING);
                    var problem = (Types.BVT.HWProblemReason) ReadRegister(REG_PROBLEM);

                    if (problem != Types.BVT.HWProblemReason.None)
                    {
                        FireNotificationEvent(problem, Types.BVT.HWWarningReason.None, Types.BVT.HWFaultReason.None,
                            Types.BVT.HWDisableReason.None);
                    }

                    if (warning != Types.BVT.HWWarningReason.None)
                    {
                        FireNotificationEvent(Types.BVT.HWProblemReason.None, warning, Types.BVT.HWFaultReason.None,
                            Types.BVT.HWDisableReason.None);
                        ClearWarning();
                    }

                    break;
                }

                Thread.Sleep(REQUEST_DELAY_MS);
            }

            if (Environment.TickCount > timeStamp)
            {
                FireExceptionEvent("Timeout while waiting for BVT test to end");
                throw new Exception("Timeout while waiting for BVT test to end");
            }

            return Types.DeviceState.Success;
        }

        #region Events

        private void FireConnectionEvent(DeviceConnectionState State, string Message)
        {
            SystemHost.Journal.AppendLog(ComplexParts.BVT, LogMessageType.Info, Message);
            m_Communication.PostDeviceConnectionEvent(ComplexParts.BVT, State, Message);
        }

        private void FireBvtAllEvent(Types.DeviceState State)
        {
            var message = string.Format("BVT test all state {0}", State);
            m_Communication.PostBVTAllEvent(State);
        }


        private void FireBvtEvent(Types.DeviceState State, Types.BVT.TestResults Result, BVTTestType type, bool IsUdsmUrsm)
        {
            var message = string.Format($"BVT {(IsUdsmUrsm ? "UdsmUrsm" : "")} {(type == BVTTestType.Direct ? "direct" : "reverse")} test state {0}", State);

            if (State == Types.DeviceState.Success)
                message = string.Format($"BVT {(IsUdsmUrsm ? "UdsmUrsm" : "")} {(type == BVTTestType.Direct ? "direct" : "reverse")} test result " +
                                        $"{(type == BVTTestType.Direct ? Result.VDRM : Result.VRRM)} V, {(type == BVTTestType.Direct ? Result.IDRM : Result.IRRM)} mA");

            SystemHost.Journal.AppendLog(ComplexParts.BVT, LogMessageType.Info, message);
            if (type == BVTTestType.Direct)
            {
                if (IsUdsmUrsm)
                {
                    m_Communication.PostBVTUdsmUrsmDirectEvent(State, Result);
                }
                else
                {
                    m_Communication.PostBVTDirectEvent(State, Result);
                }
            }
            else
            {
                if (IsUdsmUrsm)
                {
                    m_Communication.PostBVTUdsmUrsmReverseEvent(State, Result);
                }
                else
                {
                    m_Communication.PostBVTReverseEvent(State, Result);
                }
            }
        }

//        private void FireBvtDirectEvent(DeviceState State, Types.BVT.TestResults Result, bool IsUdsmUrsm)
//        {
//            var message = string.Format("BVT direct test state {0}", State);
//
//            if (State == DeviceState.Success)
//                message = string.Format($"BVT {(IsUdsmUrsm ? "UdsmUrsm" : "")} direct test result {0} V, {1} mA", Result.VDRM, Result.IDRM);
//
//            SystemHost.Journal.AppendLog(ComplexParts.BVT, LogMessageType.Info, message);
//            m_Communication.PostBVTDirectEvent(State, Result);
//        }
//
//        private void FireBvtReverseEvent(DeviceState State, Types.BVT.TestResults Result, bool IsUdsmUrsm)
//        {
//            var message = string.Format("BVT reverse test state {0}", State);
//
//            if (State == DeviceState.Success)
//                message = string.Format($"BVT {(IsUdsmUrsm ? "UdsmUrsm" : "")}  reverse test result {0} V, {1} mA", Result.VRRM, Result.IRRM);
//
//            SystemHost.Journal.AppendLog(ComplexParts.BVT, LogMessageType.Info, message);
//            m_Communication.PostBVTReverseEvent(State, Result);
//        }

        private void FireNotificationEvent(Types.BVT.HWProblemReason Problem, Types.BVT.HWWarningReason Warning,
            Types.BVT.HWFaultReason Fault, Types.BVT.HWDisableReason Disable)
        {
            SystemHost.Journal.AppendLog(ComplexParts.BVT, LogMessageType.Warning,
                string.Format(
                    "BVT device notification: problem {0}, warning {1}, fault {2}, disable {3}",
                    Problem, Warning, Fault, Disable));

            m_Communication.PostBVTNotificationEvent(Problem, Warning, Fault, Disable);
        }

//        private void FireBvtUdsmUrsmDirectEvent(DeviceState State, Types.BVT.TestResults Result)
//        {
//            var message = string.Format("BVT UdsmUrsm direct test state {0}", State);
//
//            if (State == DeviceState.Success)
//                message = string.Format("BVT UdsmUrsm direct test result VDSM={0}, IDSM={1}, ", Result.VDRM, Result.IDRM);
//
//            SystemHost.Journal.AppendLog(ComplexParts.BVT, LogMessageType.Info, message);
//            m_Communication.PostBVTUdsmUrsmDirectEvent(State, Result);
//        }
//
//        private void FireBvtUdsmUrsmReverseEvent(DeviceState State, Types.BVT.TestResults Result)
//        {
//            var message = string.Format("BVT UdsmUrsm reverse test state {0}", State);
//
//            if (State == DeviceState.Success)
//                message = string.Format("BVT UdsmUrsm reverse test result VRSM={0}, IRSM={1}", Result.VRRM, Result.IRRM);
//
//            SystemHost.Journal.AppendLog(ComplexParts.BVT, LogMessageType.Info, message);
//            m_Communication.PostBVTUdsmUrsmReverseEvent(State, Result);
//        }

        private void FireExceptionEvent(string Message)
        {
            SystemHost.Journal.AppendLog(ComplexParts.BVT, LogMessageType.Error, Message);
            m_Communication.PostExceptionEvent(ComplexParts.BVT, Message);
        }

        #endregion

        #region Registers

        internal const ushort
            ACT_ENABLE_POWER = 1,
            ACT_DISABLE_POWER = 2,
            ACT_CLEAR_FAULT = 3,
            ACT_CLEAR_WARNING = 4,
            ACT_START_TEST = 100,
            ACT_STOP = 101,
            ACT_READ_FRAGMENT = 110,
            ACT_READ_MOVE_BACK = 111,
            ACT_SAVE_TO_ROM = 200,
            MEASURE_TYPE_AC_D = 2,
            MEASURE_TYPE_AC_R = 3,
            MEASURE_MODE_I = 0,
            MEASURE_MODE_V = 1,
            REG_MEASUREMENT_TYPE = 128,
            REG_MEASUREMENT_MODE = 129,
            REG_LIMIT_CURRENT = 130,
            REG_LIMIT_VOLTAGE = 131,
            REG_VOLTAGE_PLATE_TIME = 132,
            REG_VOLTAGE_AC_RATE = 133,
            REG_START_VOLTAGE_AC = 134,
            REG_VOLTAGE_FREQUENCY = 135,
            REG_FREQUENCY_DIVISOR = 136,
            REG_S1_CURRENT1_FINE_N = 96,
            REG_S1_CURRENT1_FINE_D = 97,
            REG_S1_CURRENT2_FINE_N = 98,
            REG_S1_CURRENT2_FINE_D = 99,
            REG_S1_VOLTAGE1_FINE_N = 104,
            REG_S1_VOLTAGE1_FINE_D = 105,
            REG_S1_VOLTAGE2_FINE_N = 106,
            REG_S1_VOLTAGE2_FINE_D = 107,
            REG_DEV_STATE = 192,
            REG_FAULT_REASON = 193,
            REG_DISABLE_REASON = 194,
            REG_WARNING = 195,
            REG_PROBLEM = 196,
            REG_TEST_FINISHED = 197,
            REG_RESULT_V = 198,
            REG_RESULT_I = 199,
            ARR_SCOPE_I = 1,
            ARR_SCOPE_V = 2;

        #endregion


        class BvtInputParameters
        {
            public BVTTestType TestType { get; set; }
            public float CurrentLimit { get; set; }
            public ushort PlateTime { get; set; }
            public float RampUpVoltage { get; set; }
            public ushort StartVoltage { get; set; }
            public ushort VoltageFrequency { get; set; }
            public ushort FrequencyDivisor { get; set; }
            public ushort VoltageLimitD { get; set; }
            public ushort VoltageLimitR { get; set; }
            public BVTMeasurementMode MeasurementMode { get; set; }

            public bool IsUdsmUrsm { get; set; }
        }

        private void MeasurementLogicRoutine(Types.Commutation.TestParameters commutation)
        {
            var internalState = Types.DeviceState.InProcess;

            var data = new List<BvtInputParameters>()
            {
                new BvtInputParameters
                {
                    TestType = m_Parameters.TestType,
                    CurrentLimit = m_Parameters.CurrentLimit,
                    PlateTime = m_Parameters.PlateTime,
                    RampUpVoltage = m_Parameters.RampUpVoltage,
                    StartVoltage = m_Parameters.StartVoltage,
                    VoltageFrequency = m_Parameters.VoltageFrequency,
                    FrequencyDivisor = m_Parameters.FrequencyDivisor,
                    VoltageLimitD = m_Parameters.VoltageLimitD,
                    VoltageLimitR = m_Parameters.VoltageLimitR,
                    MeasurementMode = m_Parameters.MeasurementMode,
                    IsUdsmUrsm = false
                }
            };
            if (m_Parameters.UseUdsmUrsm)
                data.Add(new BvtInputParameters()
                {
                    TestType = m_Parameters.UdsmUrsmTestType,
                    CurrentLimit = m_Parameters.UdsmUrsmCurrentLimit,
                    PlateTime = m_Parameters.UdsmUrsmPlateTime,
                    RampUpVoltage = m_Parameters.UdsmUrsmRampUpVoltage,
                    StartVoltage = m_Parameters.UdsmUrsmStartVoltage,
                    VoltageFrequency = m_Parameters.UdsmUrsmVoltageFrequency,
                    FrequencyDivisor = m_Parameters.UdsmUrsmFrequencyDivisor,
                    VoltageLimitD = m_Parameters.UdsmUrsmVoltageLimitD,
                    VoltageLimitR = m_Parameters.UdsmUrsmVoltageLimitR,
                    MeasurementMode = BVTMeasurementMode.ModeI,
                    IsUdsmUrsm = true
                });

      
            foreach (var bvtInputParameter in data)
            {

                try
                {
                    WriteRegister(REG_LIMIT_CURRENT, (ushort) (bvtInputParameter.CurrentLimit * 10));
                    WriteRegister(REG_VOLTAGE_PLATE_TIME, bvtInputParameter.PlateTime);
                    WriteRegister(REG_VOLTAGE_AC_RATE, (ushort) (bvtInputParameter.RampUpVoltage * 10));
                    WriteRegister(REG_START_VOLTAGE_AC, bvtInputParameter.StartVoltage);
                    WriteRegister(REG_VOLTAGE_FREQUENCY, bvtInputParameter.VoltageFrequency);
                    WriteRegister(REG_FREQUENCY_DIVISOR, bvtInputParameter.FrequencyDivisor);
                    WriteRegister(REG_MEASUREMENT_MODE,
                        bvtInputParameter.MeasurementMode == Types.BVT.BVTMeasurementMode.ModeI
                            ? MEASURE_MODE_I
                            : MEASURE_MODE_V);

                    if (bvtInputParameter.TestType == Types.BVT.BVTTestType.Both ||
                        bvtInputParameter.TestType == Types.BVT.BVTTestType.Direct)
                    {
                        internalState = Types.DeviceState.InProcess;
                        FireBvtEvent(internalState, m_Result, BVTTestType.Direct, bvtInputParameter.IsUdsmUrsm);

                        if (m_IOCommutation.Switch( commutation.CommutationType == HWModuleCommutationType.Reverse ?
                            CommutationMode.BVTR : CommutationMode.BVTD
                                , commutation.CommutationType, commutation.Position) ==
                            Types.DeviceState.Fault)
                        {
                            m_State = Types.DeviceState.Fault;
                            FireBvtAllEvent(m_State);
                            return;
                        }

                        WriteRegister(REG_LIMIT_VOLTAGE, bvtInputParameter.VoltageLimitD);
                        WriteRegister(REG_MEASUREMENT_TYPE, MEASURE_TYPE_AC_D);

                        CallAction(ACT_START_TEST);

                        if (!m_IsBVTEmulation)
                        {
                            internalState = WaitForEndOfTest();

                            if (bvtInputParameter.IsUdsmUrsm)
                            {
                                m_Result.VDSM = (ushort)Math.Abs(ReadRegisterS(REG_RESULT_V));
                                m_Result.IDSM = Math.Abs(ReadRegisterS(REG_RESULT_I) / 10.0f);
                            }
                            else
                            {
                                m_Result.VDRM = (ushort)Math.Abs(ReadRegisterS(REG_RESULT_V));
                                m_Result.IDRM = Math.Abs(ReadRegisterS(REG_RESULT_I) / 10.0f);
                            }

                            if (m_ReadGraph)
                                ReadArrays(m_Result, true);
                        }
                        else
                        {
                            internalState = Types.DeviceState.Success;

                            if (bvtInputParameter.IsUdsmUrsm)
                            {
                                m_Result.VDSM = Convert.ToUInt16(EMU_DEFAULT_VDRM * new Random((int)DateTime.Now.Ticks).NextDouble());
                                m_Result.IDSM = Convert.ToUInt16(EMU_DEFAULT_IDRM * 300 * new Random((int)DateTime.Now.Ticks).NextDouble());
                            }
                            else
                            {
                                m_Result.VDRM = Convert.ToUInt16(EMU_DEFAULT_VDRM * new Random((int)DateTime.Now.Ticks).NextDouble());
                                m_Result.IDRM = Convert.ToUInt16(EMU_DEFAULT_IDRM * 300 * new Random((int)DateTime.Now.Ticks).NextDouble());
                            }
                        }

                        FireBvtEvent(internalState, m_Result, BVTTestType.Direct, bvtInputParameter.IsUdsmUrsm);


                        if (m_IOCommutation.Switch(Types.Commutation.CommutationMode.None) == Types.DeviceState.Fault)
                        {
                            m_State = Types.DeviceState.Fault;
                            FireBvtAllEvent(m_State);
                            return;
                        }
                    }
                    else
                        internalState = Types.DeviceState.Success;

                    if ((internalState == Types.DeviceState.Success)
                        && (bvtInputParameter.TestType == Types.BVT.BVTTestType.Both ||
                            bvtInputParameter.TestType == Types.BVT.BVTTestType.Reverse))
                    {
                        internalState = Types.DeviceState.InProcess;
                        FireBvtEvent(internalState, m_Result, BVTTestType.Reverse, bvtInputParameter.IsUdsmUrsm);

                        if (m_IOCommutation.Switch(commutation.CommutationType == HWModuleCommutationType.Reverse ?
                                CommutationMode.BVTD : CommutationMode.BVTR, commutation.CommutationType, commutation.Position) ==
                            Types.DeviceState.Fault)
                        {
                            m_State = Types.DeviceState.Fault;
                            FireBvtAllEvent(m_State);
                            return;
                        }

                        WriteRegister(REG_LIMIT_VOLTAGE, bvtInputParameter.VoltageLimitR);
                        WriteRegister(REG_MEASUREMENT_TYPE, MEASURE_TYPE_AC_R);
                        CallAction(ACT_START_TEST);

                        if (!m_IsBVTEmulation)
                        {
                            internalState = WaitForEndOfTest();

                            if (bvtInputParameter.IsUdsmUrsm)
                            {
                                m_Result.VRSM = (ushort)Math.Abs(ReadRegisterS(REG_RESULT_V));
                                m_Result.IRSM = Math.Abs(ReadRegisterS(REG_RESULT_I) / 10.0f);
                            }
                            else
                            {
                                m_Result.VRRM = (ushort)Math.Abs(ReadRegisterS(REG_RESULT_V));
                                m_Result.IRRM = Math.Abs(ReadRegisterS(REG_RESULT_I) / 10.0f);
                            }

                            if (m_ReadGraph)
                                ReadArrays(m_Result, false);
                        }
                        else
                        {
                            internalState = Types.DeviceState.Success;

                            if (bvtInputParameter.IsUdsmUrsm)
                            {
                                m_Result.VRSM = Convert.ToUInt16(EMU_DEFAULT_VRRM * new Random((int)DateTime.Now.Ticks).NextDouble());
                                m_Result.IRSM = Convert.ToUInt16(EMU_DEFAULT_IRRM * 300 * new Random((int)DateTime.Now.Ticks).NextDouble());
                            }
                            else
                            {
                                m_Result.VRRM = Convert.ToUInt16(EMU_DEFAULT_VRRM * new Random((int)DateTime.Now.Ticks).NextDouble());
                                m_Result.IRRM = Convert.ToUInt16(EMU_DEFAULT_IRRM * 300 * new Random((int)DateTime.Now.Ticks).NextDouble());
                            }
                        }

                        FireBvtEvent(internalState, m_Result, BVTTestType.Reverse, bvtInputParameter.IsUdsmUrsm);
                    }

                    if (m_IOCommutation.Switch(Types.Commutation.CommutationMode.None) == Types.DeviceState.Fault)
                    {
                        m_State = Types.DeviceState.Fault;
                        FireBvtAllEvent(m_State);
                        return;
                    }

                    m_State = internalState;
                }
                catch (Exception ex)
                {
                    m_IOCommutation.Switch(Types.Commutation.CommutationMode.None);
                    m_State = Types.DeviceState.Fault;
                    FireBvtAllEvent(m_State);

                    FireExceptionEvent(ex.Message);
                    throw;
                }
            }
        }
    }
}