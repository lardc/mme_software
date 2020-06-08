using System.Runtime.Serialization;

namespace SCME.Types.Commutation
{
    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public enum HWDeviceState
    {
        [EnumMember] None = 0,
        [EnumMember] Fault = 1,
        [EnumMember] Disabled = 2,
        [EnumMember] PowerReady = 3
    };

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public enum HWFaultReason
    {
        [EnumMember] None = 0,
        [EnumMember] SafetyCircuit = 301,
        [EnumMember] LowPressure = 302
    };

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public enum HWDisableReason
    {
        [EnumMember] None = 0,
        [EnumMember] BadClock = 1001
    };

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public enum HWWarningReason
    {
        [EnumMember] None = 0,
        [EnumMember] WatchdogReset = 1001
    };

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public enum HWModulePosition
    {
        [EnumMember] Position1 = 1,
        [EnumMember] Position2 = 2
    };

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public enum HWBlockIndex
    {
        [EnumMember] Block1 = 1,
        [EnumMember] Block2 = 2
    };

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public enum HWModuleCommutationType
    {
        [EnumMember] Direct = 0x00,
        [EnumMember] MT1 = 0x01,
        [EnumMember] MD1 = 0x02,
        [EnumMember] MT3 = 0x03,
        [EnumMember] MT4 = 0x04,
        [EnumMember] MT5 = 0x05,
        [EnumMember] MD3 = 0x06,
        [EnumMember] MD4 = 0x07,
        [EnumMember] MD5 = 0x08,
        [EnumMember] MTD3 = 0x09,
        [EnumMember] MDT3 = 0x0A,
        [EnumMember] MTD4 = 0x0B,
        [EnumMember] MDT4 = 0x0C,
        [EnumMember] MTD5 = 0x0D,
        [EnumMember] MDT5 = 0x0E,
        [EnumMember] Reverse = 0x0F,
        [EnumMember] Undefined = 0xFF
    }

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public class TestParameters
    {
        [DataMember]
        public HWModulePosition Position { get; set; }

        [DataMember]
        public HWBlockIndex BlockIndex { get; set; }

        [DataMember]
        public HWModuleCommutationType CommutationType { get; set; }
    }
}