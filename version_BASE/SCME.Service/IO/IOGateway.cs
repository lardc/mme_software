using System;
using SCME.Service.Properties;
using SCME.Types;

namespace SCME.Service.IO
{
    internal class IOGateway
    {
        private readonly IOAdapter m_IOAdapter;
        private readonly BroadcastCommunication m_Communication;
        private readonly ThreadService m_Thread;
        private readonly bool m_IsGatewayEmulation;
        private readonly ushort m_Node;
        private readonly bool[] m_ButtonStates = {false, false, false, false};
        private DeviceConnectionState m_ConnectionState;

        internal IOGateway(IOAdapter Adapter, BroadcastCommunication Communication)
        {
            m_IOAdapter = Adapter;
            m_Communication = Communication;
            m_IsGatewayEmulation = Settings.Default.GatewayEmulation;
            m_Node = (ushort) Settings.Default.GatewayNode;

            m_Thread = new ThreadService();
            m_Thread.FinishedHandler += Thread_FinishedHandler;

            SystemHost.Journal.AppendLog(ComplexParts.Gateway, LogMessageType.Info,
                                         String.Format("Gateway created. Emulation mode: {0}", m_IsGatewayEmulation));
        }

        internal DeviceConnectionState Initialize()
        {
            m_ConnectionState = DeviceConnectionState.ConnectionInProcess;
            FireConnectionEvent(m_ConnectionState, "Gateway initializing");

            if (m_IsGatewayEmulation)
            {
                m_ConnectionState = DeviceConnectionState.ConnectionSuccess;

                FireConnectionEvent(m_ConnectionState, "Gateway initialized");

                return m_ConnectionState;
            }

            try
            {
                ReadRegister(REG_DEVICE_STATE);

                m_Thread.StartCycle(ThreadWorker, REQUEST_DELAY_MS);

                m_ConnectionState = DeviceConnectionState.ConnectionSuccess;
                FireConnectionEvent(m_ConnectionState, "Gateway initialized");
            }
            catch (Exception ex)
            {
                m_ConnectionState = DeviceConnectionState.ConnectionFailed;

                FireConnectionEvent(m_ConnectionState, String.Format("Gateway initialization error: {0}", ex.Message));
            }

            return m_ConnectionState;
        }

        internal void Deinitialize()
        {
            var oldState = m_ConnectionState;

            m_ConnectionState = DeviceConnectionState.DisconnectionInProcess;
            FireConnectionEvent(DeviceConnectionState.DisconnectionInProcess, "Gateway disconnecting");

            if (!m_IsGatewayEmulation && oldState == DeviceConnectionState.ConnectionSuccess)
                m_Thread.StopCycle(true);

            m_ConnectionState = DeviceConnectionState.DisconnectionSuccess;
            FireConnectionEvent(DeviceConnectionState.DisconnectionSuccess, "Gateway disconnected");
        }

        #region Standart API

        internal void ClearFault()
        {
            SystemHost.Journal.AppendLog(ComplexParts.Gate, LogMessageType.Note, "Gateway fault cleared");

            CallAction(ACT_CLEAR_FAULT);
        }


        internal ushort ReadRegister(ushort Address, bool SkipJournal = false)
        {
            ushort value = 0;

            if (!m_IsGatewayEmulation)
                value = m_IOAdapter.Read16(m_Node, Address);

            if (!SkipJournal)
                SystemHost.Journal.AppendLog(ComplexParts.Gateway, LogMessageType.Note,
                                         string.Format("Gateway @ReadRegister, address {0}, value {1}", Address, value));

            return value;
        }

        internal void WriteRegister(ushort Address, ushort Value, bool SkipJournal = false)
        {
            if (!SkipJournal)
                SystemHost.Journal.AppendLog(ComplexParts.Gateway, LogMessageType.Note,
                                         string.Format("Gateway @WriteRegister, address {0}, value {1}", Address, Value));

            if (m_IsGatewayEmulation)
                return;

            m_IOAdapter.Write16(m_Node, Address, Value);
        }

        internal void CallAction(ushort Action, bool SkipJournal = false)
        {
            if (!SkipJournal)
                SystemHost.Journal.AppendLog(ComplexParts.Gateway, LogMessageType.Note,
                                         string.Format("Gateway @Call, action {0}", Action));

            if (m_IsGatewayEmulation)
                return;

            m_IOAdapter.Call(m_Node, Action);
        }

        #endregion

        internal bool GetButtonState(ComplexButtons Button)
        {
            switch (Button)
            {
                case ComplexButtons.ButtonSC1:
                    return m_IsGatewayEmulation || m_ButtonStates[0];
                case ComplexButtons.ButtonSC2:
                    return m_IsGatewayEmulation || m_ButtonStates[1];
                case ComplexButtons.ButtonStart:
                    return !m_IsGatewayEmulation && m_ButtonStates[2];
                case ComplexButtons.ButtonStop:
                    return !m_IsGatewayEmulation && m_ButtonStates[3];
                default:
                    throw new ArgumentOutOfRangeException("Button");
            }
        }

        internal void SetLamps(bool Lamp1, bool Lamp2, bool Lamp3)
        {
            SystemHost.Journal.AppendLog(ComplexParts.Gateway, LogMessageType.Note,
                                         String.Format("Set lamps: lamp 1 - {0}, lamp 2 - {1}, lamp 3 - {2}", Lamp1,
                                                       Lamp2, Lamp3));

            if (m_IsGatewayEmulation)
                return;

            WriteRegister(REG_LAMP1, (ushort) (Lamp1 ? 1 : 0));
            WriteRegister(REG_LAMP2, (ushort)(Lamp2 ? 1 : 0));
            WriteRegister(REG_LAMP3, (ushort)(Lamp3 ? 1 : 0));
        }

        private void ThreadWorker()
        {
            var devState = (Types.Gateway.HWDeviceState)ReadRegister(REG_DEVICE_STATE, true);
            var disableReason = (Types.Gateway.HWDisableReason)ReadRegister(REG_DISABLE_REASON, true);
            var warning = (Types.Gateway.HWWarningReason)ReadRegister(REG_WARNING, true);
            var faultReason = (Types.Gateway.HWFaultReason)ReadRegister(REG_FAULT_REASON, true);
            
            var nButton1 = ReadRegister(REG_SENSOR1, true) != 0;
            var nButton2 = ReadRegister(REG_SENSOR2, true) != 0;
            var nButton3 = ReadRegister(REG_SENSOR3, true) != 0;
            var nButton4 = ReadRegister(REG_SENSOR4, true) != 0;

            if (warning != Types.Gateway.HWWarningReason.None)
            {
                FireNotificationEvent(warning, Types.Gateway.HWFaultReason.None, Types.Gateway.HWDisableReason.None);
                CallAction(ACT_CLEAR_WARNING, true);
            }

            if (devState == Types.Gateway.HWDeviceState.Fault)
            {
                FireNotificationEvent(Types.Gateway.HWWarningReason.None, faultReason,
                                      Types.Gateway.HWDisableReason.None);
                throw new Exception(string.Format("Device is in fault state, reason - {0}", faultReason));
            }

            if (devState == Types.Gateway.HWDeviceState.Disabled)
            {
                FireNotificationEvent(Types.Gateway.HWWarningReason.None, Types.Gateway.HWFaultReason.None,
                                      disableReason);
                throw new Exception(string.Format("Device is in disabled state, reason - {0}", disableReason));
            }

            if (m_ButtonStates[0] != nButton1)
            {
                m_ButtonStates[0] = nButton1;
                FireButtonEvent(ComplexButtons.ButtonSC1, m_ButtonStates[0]);
            }
            if (m_ButtonStates[1] != nButton2)
            {
                m_ButtonStates[1] = nButton2;
                FireButtonEvent(ComplexButtons.ButtonSC2, m_ButtonStates[1]);
            }
            if (m_ButtonStates[2] != nButton3)
            {
                m_ButtonStates[2] = nButton3;
                FireButtonEvent(ComplexButtons.ButtonStart, m_ButtonStates[2]);
            }
            if (m_ButtonStates[3] != nButton4)
            {
                m_ButtonStates[3] = nButton4;
                FireButtonEvent(ComplexButtons.ButtonStop, m_ButtonStates[3]);
            }
        }

        private void Thread_FinishedHandler(object Sender, ThreadFinishedEventArgs E)
        {
            if (E.Error != null)
                FireExceptionEvent(E.Error.Message);
        }

        private void FireConnectionEvent(DeviceConnectionState State, string Message)
        {
            SystemHost.Journal.AppendLog(ComplexParts.Gateway, LogMessageType.Info, Message);
            m_Communication.PostDeviceConnectionEvent(ComplexParts.Gateway, State, Message);
        }

        private void FireNotificationEvent(Types.Gateway.HWWarningReason Warning, Types.Gateway.HWFaultReason Fault,
                                           Types.Gateway.HWDisableReason Disable)
        {
            SystemHost.Journal.AppendLog(ComplexParts.Gateway, LogMessageType.Warning,
                                         string.Format(
                                             "Gateway device notification: warning {0}, fault {1}, disable {2}",
                                             Warning, Fault, Disable));

            m_Communication.PostGatewayNotificationEvent(Warning, Fault, Disable);
        }

        private void FireExceptionEvent(string Message)
        {
            SystemHost.Journal.AppendLog(ComplexParts.Gateway, LogMessageType.Error, Message);
            m_Communication.PostExceptionEvent(ComplexParts.Gateway, Message);
        }

        private void FireButtonEvent(ComplexButtons Button, bool State)
        {
            SystemHost.Journal.AppendLog(ComplexParts.Gateway, LogMessageType.Info,
                                         string.Format("Gateway button {0}, state {1}", Button, State));
            m_Communication.PostButtonPressedEvent(Button, State);
        }

        #region Registers

        private const ushort
            ACT_CLEAR_FAULT = 3,
            ACT_CLEAR_WARNING = 4,
            REG_LAMP1 = 128,
            REG_LAMP2 = 129,
            REG_LAMP3 = 130,
            REG_SENSOR1 = 197,
            REG_SENSOR2 = 198,
            REG_SENSOR3 = 199,
            REG_SENSOR4 = 200,
            REG_DEVICE_STATE = 192,
            REG_FAULT_REASON = 193,
            REG_DISABLE_REASON = 194,
            REG_WARNING = 195;

        private const int REQUEST_DELAY_MS = 100;

        #endregion
    }
}