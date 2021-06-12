using System.Runtime.Serialization;

namespace SCME.Types.dVdt
{
    /// <summary>Режим тестирования</summary>
    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public enum DvdtMode
    {
        /// <summary>Подтверждение</summary>
        [EnumMember]
        Confirmation = 0,
        /// <summary>Определение</summary>
        [EnumMember]
        Detection = 1
    };

    /// <summary>Скорость роста</summary>
    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public enum VoltageRate
    {
        [EnumMember]
        V500 = 500,
        [EnumMember]
        V1000 = 1000,
        [EnumMember]
        V1600 = 1600,
        [EnumMember]
        V2000 = 2000,
        [EnumMember]
        V2500 = 2500
    };
}