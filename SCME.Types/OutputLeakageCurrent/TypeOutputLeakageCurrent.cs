using PropertyChanged;
using SCME.Types.BaseTestParams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SCME.Types.OutputLeakageCurrent
{
   

    [AddINotifyPropertyChangedInterface]
    [DataContract(Name = "Tou.OutputLeakageCurrent", Namespace = "http://proton-electrotex.com/SCME")]
    [KnownType(typeof(BaseTestParametersAndNormatives))]
    public class TestParameters : BaseTestParametersAndNormatives, ICloneable
    {
        [DependsOn(nameof(TypeManagement))]
        public double ControlVoltageMin => TypeManagement == TypeManagement.ACVoltage ? 5 : 0.05;
        [DependsOn(nameof(TypeManagement))]
        public double ControlVoltageMax => TypeManagement == TypeManagement.ACVoltage ? 300 : 250;

        public double ControlCurrentMaximumMin => 0.01;
        public double ControlCurrentMaximumMax => 100;


        [DependsOn(nameof(ApplicationPolarityConstantSwitchingVoltage))]
        public double SwitchedVoltageMin => ApplicationPolarityConstantSwitchingVoltage == ApplicationPolarityConstantSwitchingVoltage.ACVoltage ? 5 :  25;
        [DependsOn(nameof(ApplicationPolarityConstantSwitchingVoltage))]
        public double SwitchedVoltageMax => ApplicationPolarityConstantSwitchingVoltage == ApplicationPolarityConstantSwitchingVoltage.ACVoltage ? 300 : 2000;


        public double LeakageCurrentMinimumMin => 0;
        [DependsOn(nameof(ApplicationPolarityConstantSwitchingVoltage))]
        public double LeakageCurrentMinimumMax => ApplicationPolarityConstantSwitchingVoltage == ApplicationPolarityConstantSwitchingVoltage.ACVoltage ? 100 : 20;

        [DependsOn(nameof(ApplicationPolarityConstantSwitchingVoltage))]
        public double LeakageCurrentMaximumMin => ApplicationPolarityConstantSwitchingVoltage == ApplicationPolarityConstantSwitchingVoltage.ACVoltage ? 0.01 : 0.006;
        [DependsOn(nameof(ApplicationPolarityConstantSwitchingVoltage))]
        public double LeakageCurrentMaximumMax => ApplicationPolarityConstantSwitchingVoltage == ApplicationPolarityConstantSwitchingVoltage.ACVoltage ? 100 : 20;


        public double AuxiliaryVoltagePowerSupply1Min => 0.05;
        public double AuxiliaryVoltagePowerSupply1Max => 150;
        public double AuxiliaryVoltagePowerSupply2Min => 0.05;
        public double AuxiliaryVoltagePowerSupply2Max => 20;


        [DependsOn(nameof(TypeManagement))]
        public bool ShowVoltage => TypeManagement == TypeManagement.DCVoltage || TypeManagement == TypeManagement.ACVoltage;
        [DependsOn(nameof(TypeManagement))]
        public bool ShowAmperage => TypeManagement == TypeManagement.DCAmperage;

        [DataMember]
        public TypeManagement TypeManagement { get; set; }

        [DataMember]
        public double ControlVoltage { get; set; }
        [DataMember]
        public double ControlCurrent  { get; set; }

        [DataMember]
        public ApplicationPolarityConstantSwitchingVoltage ApplicationPolarityConstantSwitchingVoltage { get; set; }

        [XmlIgnore]
        [DependsOn(nameof(ApplicationPolarityConstantSwitchingVoltage))]
        public bool PolarityDCSwitchingVoltageApplicationIsVisible => ApplicationPolarityConstantSwitchingVoltage == ApplicationPolarityConstantSwitchingVoltage.DCVoltage;

        [DataMember]
        public PolarityDCSwitchingVoltageApplication PolarityDCSwitchingVoltageApplication { get; set; }

        //[DataMember]
        //public double SwitchedAmperage { get; set; }
        [DataMember]
        public double SwitchedVoltage { get; set; }

        [DataMember]
        public double AuxiliaryVoltagePowerSupply1 { get; set; }
        [DataMember]
        public double AuxiliaryVoltagePowerSupply2 { get; set; }

        [DataMember]
        public double LeakageCurrentMinimum { get; set; }
        [DataMember]
        public double LeakageCurrentMaximum { get; set; }


        [DataMember]
        public double ControlVoltageMaximum { get; set; }
        [DataMember]
        public double ControlCurrentMaximum { get; set; }

        [XmlIgnore]
        public bool OpenState { get; set; }

        public TestParameters() : base()
        {
            IsProfileStyle = true;
            DutPackageType = DutPackageType.A1;
            TestParametersType = TestParametersType.OutputLeakageCurrent;
            ApplicationPolarityConstantSwitchingVoltage = ApplicationPolarityConstantSwitchingVoltage.ACVoltage;
            PolarityDCSwitchingVoltageApplication = PolarityDCSwitchingVoltageApplication.Direct;
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