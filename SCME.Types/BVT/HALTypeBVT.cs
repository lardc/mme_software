using System.Runtime.Serialization;

namespace SCME.Types.BVT
{
    /// <summary>Тип направления</summary>
    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public enum BVTTestType
    {
        /// <summary>Оба направления</summary>
        [EnumMember]
        Both,
        /// <summary>Прямое</summary>
        [EnumMember]
        Direct,
        /// <summary>Обратное</summary>
        [EnumMember]
        Reverse
    };

    /// <summary>Режим тестирования</summary>
    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public enum BVTMeasurementMode
    {
        [EnumMember]
        ModeI,
        [EnumMember]
        ModeV
    };

    public static class BVTRates
    {
        public static string[] GetRates()
        {
            return new string[] { "5", "50" };
        }
    }
}