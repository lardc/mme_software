using System.Runtime.Serialization;

namespace SCME.Types.VTM
{
    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public enum VTMTestType
    {
        [EnumMember] Ramp,
        [EnumMember] Sinus,
        [EnumMember] Curve
    };
}