using System;
using System.Runtime.Serialization;
using SCME.Types.BaseTestParams;

namespace SCME.Types.SCTU
{
    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public class SctuTestParameters : BaseTestParametersAndNormatives, ICloneable
    {
        public SctuTestParameters()
        {
            TestParametersType = TestParametersType.Sctu;
            Type = SctuDutType.Diod;
            Value = 100;
            ShuntResistance = 1;
        }

        [DataMember]
        public SctuDutType Type { get; set; }


        [DataMember]
        public int Value { get; set; }

        [DataMember]
        public ushort ShuntResistance { get; set; }

        public override bool IsHasChanges(BaseTestParametersAndNormatives oldParametersBase)
        {
            return false;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    [DataContract(Namespace = "http://proton-electrotex.com/SCME")]
    public enum SctuDutType
    {
        [EnumMember]
        Diod = 1234,
        [EnumMember]
        Tristor = 5678
    };
}
