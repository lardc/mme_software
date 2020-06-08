using System;
using System.Threading;
using SCME.Types;

namespace SCME.Service.IO
{
    internal class IOCommutation
    {
        private readonly IOAdapter m_IOAdapter;
        private readonly BroadcastCommunication m_Communication;
        private readonly ushort m_Node;
        private readonly bool m_IsCommutationEmulation, m_Type6;
        private readonly ComplexParts m_ID;
        private DeviceConnectionState m_ConnectionState;

        internal IOCommutation(IOAdapter Adapter, BroadcastCommunication Communication, bool CommutationEmulation,
                             int CommutationNode, bool Type6, ComplexParts ID)
        {
            m_ID = ID;
            m_IOAdapter = Adapter;
            m_Communication = Communication;

            m_IsCommutationEmulation = CommutationEmulation;
            m_Node = (ushort) CommutationNode;
            m_Type6 = Type6;

            SystemHost.Journal.AppendLog(m_ID, LogMessageType.Info,
                                         String.Format("Commutation created. Emulation mode: {0}",
                                                       m_IsCommutationEmulation));
        }

        internal DeviceConnectionState Initialize()
        {
            m_ConnectionState = DeviceConnectionState.ConnectionInProcess;
            FireConnectionEvent(m_ConnectionState, "Commutation initializing");

            if (m_IsCommutationEmulation)
            {
                m_ConnectionState = DeviceConnectionState.ConnectionSuccess;
                FireConnectionEvent(m_ConnectionState, "Commutation initialized");

                return m_ConnectionState;
            }

            try
            {
                ClearFault();
                ClearWarning();

                var devState = (Types.Commutation.HWDeviceState) ReadRegister(REG_DEVICE_STATE);
                if (devState != Types.Commutation.HWDeviceState.PowerReady)
                {
                    if (devState == Types.Commutation.HWDeviceState.Fault)
                    {
                        ClearFault();
                        Thread.Sleep(1000);

                        devState = (Types.Commutation.HWDeviceState)ReadRegister(REG_DEVICE_STATE);

                        if (devState == Types.Commutation.HWDeviceState.Fault)
                            throw new Exception(string.Format("Device is in fault state, reason: {0}",
                                (Types.Commutation.HWFaultReason)ReadRegister(REG_FAULT_REASON)));
                    }

                    if (devState == Types.Commutation.HWDeviceState.Disabled)
                        throw new Exception(string.Format("Device is in disabled state, reason: {0}",
                                     (Types.Commutation.HWDisableReason)ReadRegister(REG_DISABLE_REASON)));

                    CallAction(ACT_ENABLE_POWER);
                }

                Thread.Sleep(1000);

                devState = (Types.Commutation.HWDeviceState)
                            ReadRegister(REG_DEVICE_STATE);

                if (devState == Types.Commutation.HWDeviceState.Fault)
                    throw new Exception(string.Format("Device is in fault state, reason: {0}",
                                                        (Types.Commutation.HWFaultReason)ReadRegister(REG_FAULT_REASON)));
                if (devState == Types.Commutation.HWDeviceState.Disabled)
                    throw new Exception(string.Format("Device is in disabled state, reason: {0}",
                                                        (Types.Commutation.HWDisableReason)ReadRegister(REG_DISABLE_REASON)));

                if (m_Type6)
                {
                    WriteRegister(REG_MODULE_TYPE, (ushort) Types.Commutation.HWModuleCommutationType.MT3);
                    WriteRegister(REG_MODULE_POSITION,
                                        (ushort) Types.Commutation.HWModulePosition.Position1);
                    CallAction(ACT_COMM6_SL);
                    CallAction(ACT_COMM6_NONE);
                    WriteRegister(REG_MODULE_POSITION,
                                        (ushort) Types.Commutation.HWModulePosition.Position2);
                    CallAction(ACT_COMM6_SL);
                    CallAction(ACT_COMM6_NONE);
                }
                else
                {
                    CallAction(ACT_COMM2_SL);
                    CallAction(ACT_COMM2_NONE);
                }

                m_ConnectionState = DeviceConnectionState.ConnectionSuccess;

                FireConnectionEvent(m_ConnectionState, "Commutation initialized");
            }
            catch (Exception ex)
            {
                m_ConnectionState = DeviceConnectionState.ConnectionFailed;
                FireConnectionEvent(m_ConnectionState,
                                    String.Format("Commutation initialization error: {0}", ex.Message));
            }

            return m_ConnectionState;
        }

        internal void Deinitialize()
        {
            var oldState = m_ConnectionState;

            m_ConnectionState = DeviceConnectionState.DisconnectionInProcess;
            FireConnectionEvent(DeviceConnectionState.DisconnectionInProcess, "Commutation disconnecting");

            try
            {
                if (!m_IsCommutationEmulation && oldState == DeviceConnectionState.ConnectionSuccess)
                    CallAction(ACT_DISABLE_POWER);

                m_ConnectionState = DeviceConnectionState.DisconnectionSuccess;
                FireConnectionEvent(DeviceConnectionState.DisconnectionSuccess, "Commutation disconnected");
            }
            catch (Exception)
            {
                m_ConnectionState = DeviceConnectionState.DisconnectionError;
                FireConnectionEvent(DeviceConnectionState.DisconnectionError, "Commutation disconnection error");
            }
        }

        #region Standart API

        internal void ClearFault()
        {
            SystemHost.Journal.AppendLog(m_ID, LogMessageType.Note, "Commutation fault cleared");

            if (m_IsCommutationEmulation)
                return;

            CallAction(ACT_CLEAR_FAULT);
        }

        private void ClearWarning()
        {
            SystemHost.Journal.AppendLog(m_ID, LogMessageType.Note, "Commutation warning cleared");

            if (m_IsCommutationEmulation)
                return;

            CallAction(ACT_CLEAR_WARNING);
        }

        internal ushort ReadRegister(ushort Address, bool SkipJournal = false)
        {
            ushort value = 0;

            if (!m_IsCommutationEmulation)
                value = m_IOAdapter.Read16(m_Node, Address);

            if (!SkipJournal)
                SystemHost.Journal.AppendLog(m_ID, LogMessageType.Note,
                                         string.Format("Commutation @ReadRegister, address {0}, value {1}", Address,
                                                       value));

            return value;
        }

        internal void WriteRegister(ushort Address, ushort Value, bool SkipJournal = false)
        {
            if (!SkipJournal)
                SystemHost.Journal.AppendLog(m_ID, LogMessageType.Note,
                                         string.Format("Commutation @WriteRegister, address {0}, value {1}", Address,
                                                       Value));

            if (m_IsCommutationEmulation)
                return;

            m_IOAdapter.Write16(m_Node, Address, Value);
        }

        internal void CallAction(ushort Action)
        {
            SystemHost.Journal.AppendLog(m_ID, LogMessageType.Note,
                                         string.Format("Commutation @Call, action {0}", Action));

            if (m_IsCommutationEmulation)
                return;

            m_IOAdapter.Call(m_Node, Action);
        }

        #endregion

        internal DeviceState Switch(Types.Commutation.CommutationMode Mode,
                                  Types.Commutation.HWModuleCommutationType CommutationType =
                                      Types.Commutation.HWModuleCommutationType.Undefined,
                                  Types.Commutation.HWModulePosition Position =
                                      Types.Commutation.HWModulePosition.Position1)
        {
            if (m_IsCommutationEmulation)
                return DeviceState.Success;

            if (CommutationType == Types.Commutation.HWModuleCommutationType.Undefined)
                CommutationType = m_Type6 ? Types.Commutation.HWModuleCommutationType.MT3 : Types.Commutation.HWModuleCommutationType.Direct;

            try
            {
                var warning = (Types.Commutation.HWWarningReason)
                              ReadRegister(REG_WARNING);

                if (warning != Types.Commutation.HWWarningReason.None)
                {
                    FireNotificationEvent(warning, Types.Commutation.HWFaultReason.None);
                    ClearWarning();
                }

                var devState = (Types.Commutation.HWDeviceState)
                               ReadRegister(REG_DEVICE_STATE);

                if (devState == Types.Commutation.HWDeviceState.Fault)
                {
                    FireNotificationEvent(Types.Commutation.HWWarningReason.None,
                                          (Types.Commutation.HWFaultReason) ReadRegister(REG_FAULT_REASON));
                    return DeviceState.Fault;
                }

                WriteRegister(REG_MODULE_TYPE, (ushort) CommutationType);
                WriteRegister(REG_MODULE_POSITION, (ushort)Position);

                switch (Mode)
                {
                    case Types.Commutation.CommutationMode.None:
                        CallAction(m_Type6 ? ACT_COMM6_NONE : ACT_COMM2_NONE);
                        break;
                    case Types.Commutation.CommutationMode.Gate:
                        CallAction(m_Type6 ? ACT_COMM6_GATE : ACT_COMM2_GATE);
                        break;
                    case Types.Commutation.CommutationMode.SL:
                        CallAction(m_Type6 ? ACT_COMM6_SL : ACT_COMM2_SL);
                        break;
                    case Types.Commutation.CommutationMode.BVTD:
                        CallAction(m_Type6 ? ACT_COMM6_BVT_D : ACT_COMM2_BVT_D);
                        break;
                    case Types.Commutation.CommutationMode.BVTR:
                        CallAction(m_Type6 ? ACT_COMM6_BVT_R : ACT_COMM2_BVT_R);
                        break;
                }

                FireSwitchEvent(Mode, CommutationType, Position);
                return DeviceState.Success;
            }
            catch (Exception ex)
            {
                FireExceptionEvent(ex.Message);
                return DeviceState.Fault;
            }
        }


        private void FireConnectionEvent(DeviceConnectionState State, string Message)
        {
            SystemHost.Journal.AppendLog(m_ID, LogMessageType.Info, Message);

            m_Communication.PostDeviceConnectionEvent(m_ID, State, Message);
        }

        private void FireNotificationEvent(Types.Commutation.HWWarningReason Warning,
                                           Types.Commutation.HWFaultReason Fault)
        {
            SystemHost.Journal.AppendLog(m_ID, LogMessageType.Warning,
                                         string.Format("Commutation device notification: warning {0}, fault {1}",
                                                       Warning, Fault));

            m_Communication.PostCommutationNotificationEvent(Warning, Fault);
        }

        private void FireSwitchEvent(Types.Commutation.CommutationMode SwitchState, Types.Commutation.HWModuleCommutationType CommutationType, Types.Commutation.HWModulePosition Position)
        {
            SystemHost.Journal.AppendLog(m_ID, LogMessageType.Info,
                                         string.Format("Switch state {0} on {1}:{2}", SwitchState, CommutationType, Position));

            m_Communication.PostCommutationSwitchEvent(SwitchState);
        }

        private void FireExceptionEvent(string Message)
        {
            SystemHost.Journal.AppendLog(m_ID, LogMessageType.Error, Message);

            m_Communication.PostExceptionEvent(m_ID, Message);
        }

        #region Registers

        private const ushort

            ACT_ENABLE_POWER = 1,
            ACT_DISABLE_POWER = 2,
            ACT_CLEAR_FAULT = 3,
            ACT_CLEAR_WARNING = 4,
            ACT_COMM2_NONE = 110,
            ACT_COMM2_GATE = 111,
            ACT_COMM2_SL = 112,
            ACT_COMM2_BVT_D = 113,
            ACT_COMM2_BVT_R = 114,
            ACT_COMM6_NONE = 120,
            ACT_COMM6_GATE = 121,
            ACT_COMM6_SL = 122,
            ACT_COMM6_BVT_D = 123,
            ACT_COMM6_BVT_R = 124,
            REG_DEVICE_STATE = 96,
            REG_FAULT_REASON = 97,
            REG_DISABLE_REASON = 98,
            REG_WARNING = 99,
            REG_MODULE_TYPE = 70,
            REG_MODULE_POSITION = 71;

        #endregion
    }
}