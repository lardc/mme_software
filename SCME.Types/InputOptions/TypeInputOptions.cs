using PropertyChanged;
using SCME.Types.BaseTestParams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SCME.Types.InputOptions
{
    [AddINotifyPropertyChangedInterface]
    [DataContract(Name = "InputOptions.TestParameters", Namespace = "http://proton-electrotex.com/SCME")]
    [KnownType(typeof(BaseTestParametersAndNormatives))]
    [KnownType(typeof(BaseTestParametersAndNormativesSSRTU))]
    public class TestParameters : BaseTestParametersAndNormatives, ICloneable
    {
        [DependsOn(nameof(TypeManagement))]
        public double ControlVoltageMin => TypeManagement == TypeManagement.ACVoltage ? 5 : 0.05;
        [DependsOn(nameof(TypeManagement))]
        public double ControlVoltageMax => TypeManagement == TypeManagement.ACVoltage ? 300 : 250;


        public double ControlCurrentMin => 0.01;
        public double ControlCurrentMax => 100;


        public double InputCurrentMinimumMin => 0;
        public double InputCurrentMinimumMax => 100;
        public double InputCurrentMaximumMin => 0.01;
        public double InputCurrentMaximumMax => 100;


        public double AuxiliaryVoltagePowerSupply1Min => 0.05;
        public double AuxiliaryVoltagePowerSupply1Max => 150;
        public double AuxiliaryVoltagePowerSupply2Min => 0.05;
        public double AuxiliaryVoltagePowerSupply2Max => 20;


        public double InputVoltageMinimumMin => 0;
        public double InputVoltageMinimumMax => 250;

        public double InputVoltageMaximumMin => 0.05;
        public double InputVoltageMaximumMax => 250;



        [DependsOn(nameof(TypeManagement))]
        public bool ShowVoltage => TypeManagement == TypeManagement.DCVoltage || TypeManagement == TypeManagement.ACVoltage;
        [DependsOn(nameof(TypeManagement))]
        public bool ShowAmperage => TypeManagement == TypeManagement.DCAmperage;



        [XmlIgnore]
        public bool OpenState { get; set; }

        [DataMember]
        public TypeManagement TypeManagement { get; set; }

        [DataMember]
        public double ControlVoltage { get; set; }
        [DataMember]
        public double ControlCurrent { get; set; }

        [DataMember]
        public double AuxiliaryVoltagePowerSupply1 { get; set; }
        [DataMember]
        public double AuxiliaryVoltagePowerSupply2 { get; set; }

        [DataMember]
        public double InputCurrentMinimum { get; set; }
        [DataMember]
        public double InputCurrentMaximum { get; set; }

        [DataMember]
        public double InputVoltageMinimum { get; set; }
        [DataMember]
        public double InputVoltageMaximum { get; set; }


        //[DataMember]
        //public double AuxiliaryCurrentPowerSupplyMinimum1 { get; set; }
        //[DataMember]
        //public double AuxiliaryCurrentPowerSupplyMaximum1 { get; set; }

        //[DataMember]
        //public double AuxiliaryCurrentPowerSupplyMinimum2 { get; set; }
        //[DataMember]
        //public double AuxiliaryCurrentPowerSupplyMaximum2 { get; set; }

        public TestParameters()
        {
            IsProfileStyle = true;
            DutPackageType = DutPackageType.A1;
            TestParametersType = TestParametersType.InputOptions;
            TypeManagement = TypeManagement.ACVoltage;
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
