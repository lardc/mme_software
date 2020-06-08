using System.Runtime.Serialization;
using SCME.Types.BaseTestParams;

namespace SCME.Types.SCTU
{
    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public class SctuTestResults : BaseTestResults
    {
        /// <summary>
        /// Напряжение мВ
        /// </summary>
        [DataMember]
        public int VoltageValue { get; set; }

        /// <summary>
        /// Ударный ток А
        /// </summary>
        [DataMember]
        public int CurrentValue { get; set; }
    }
}
