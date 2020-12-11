using PropertyChanged;
using SCME.Types.BaseTestParams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SCME.Types.AuxiliaryPower
{
    [AddINotifyPropertyChangedInterface]
    [DataContract(Name = "AuxiliaryPower.TestParameters", Namespace = "http://proton-electrotex.com/SCME")]
    [KnownType(typeof(BaseTestParametersAndNormatives))]
    [KnownType(typeof(BaseTestParametersAndNormativesSSRTU))]
    public class TestParameters : BaseTestParametersAndNormatives, ICloneable
    {
        public double AuxiliaryCurrentPowerSupplyMinimumMin => 0;
        public double AuxiliaryCurrentPowerSupplyMinimumMax => 100;

        public double AuxiliaryCurrentPowerSupplyMaximumMin => 0.01;
        public double AuxiliaryCurrentPowerSupplyMaximumMax => 100;


        public double AuxiliaryVoltagePowerSupply1Min => 0.05;
        public double AuxiliaryVoltagePowerSupply1Max => 150;
        public double AuxiliaryVoltagePowerSupply2Min => 0.05;
        public double AuxiliaryVoltagePowerSupply2Max => 20;


        [DataMember]
        public double AuxiliaryVoltagePowerSupply1 { get; set; }
        [DataMember]
        public double AuxiliaryVoltagePowerSupply2 { get; set; }


        [DataMember]
        public double AuxiliaryCurrentPowerSupplyMinimum1 { get; set; }

        [DataMember]
        public double AuxiliaryCurrentPowerSupplyMinimum2 { get; set; }

        [XmlIgnore]
        public bool OpenState { get; set; }

        public TestParameters()
        {
            IsProfileStyle = true;
            DutPackageType = DutPackageType.V108;
            TestParametersType = TestParametersType.AuxiliaryPower;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public override bool IsHasChanges(BaseTestParametersAndNormatives oldParametersBase)
        {
            return false;
        }
    }
}
