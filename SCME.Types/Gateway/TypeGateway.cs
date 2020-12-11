using System.Runtime.Serialization;

namespace SCME.Types.Gateway
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
        [EnumMember] None1 = 1,
        [EnumMember] None2 = 2,
        [EnumMember] None3 = 3,
        [EnumMember] None4 = 4,
        [EnumMember] None5 = 5,
        [EnumMember] None6 = 6,
        [EnumMember] None7 = 7,
        [EnumMember] None8 = 8,
        [EnumMember] None9 = 9,
        [EnumMember] None10 = 10,
        [EnumMember] None11 = 11,
        [EnumMember] None12 = 12,
        [EnumMember] None13 = 13,
        [EnumMember] WatchdogReset = 1001
    };
}