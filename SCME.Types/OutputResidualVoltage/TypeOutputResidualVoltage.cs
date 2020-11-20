using PropertyChanged;
using SCME.Types.BaseTestParams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SCME.Types.OutputResidualVoltage
{

    [AddINotifyPropertyChangedInterface]
    [DataContract(Name = "Tou.OutputResidualVoltage", Namespace = "http://proton-electrotex.com/SCME")]
    [KnownType(typeof(BaseTestParametersAndNormatives))]
    public class TestParameters : BaseTestParametersAndNormatives, ICloneable
    {

        [DependsOn(nameof(TypeManagement))]
        public bool ShowVoltage => TypeManagement == TypeManagement.DCVoltage || TypeManagement == TypeManagement.ACVoltage;
        [DependsOn(nameof(TypeManagement))]
        public bool ShowAmperage => TypeManagement == TypeManagement.DCAmperage;

        [DataMember]
        public TypeManagement TypeManagement { get; set; }

        [DataMember]
        public bool OpenState { get; set; }

        [DataMember]
        public double ControlVoltage { get; set; }
        [DataMember]
        public double ControlCurrent { get; set; }

        [DataMember]
        public PolarityDCSwitchingVoltageApplication PolarityDCSwitchingVoltageApplication { get; set; }

        //[DataMember]
        //public double SwitchedAmperage { get; set; }
        //[DataMember]
        //public double SwitchedVoltage { get; set; }

        [DataMember]
        public double AuxiliaryVoltagePowerSupply1 { get; set; }
        [DataMember]
        public double AuxiliaryVoltagePowerSupply2 { get; set; }

        [DataMember]
        public SwitchingCurrentPulseShape SwitchingCurrentPulseShape { get; set; }
        [DataMember]
        public double SwitchingCurrentPulseDuration { get; set; }

        [DataMember]
        public double OutputResidualVoltageMinimum {get;set;}
        [DataMember]
        public double OutputResidualVoltageMaximum { get; set; }

        [DataMember]
        public double OpenResistanceMinimum { get; set; }
        [DataMember]
        public double OpenResistanceMaximum { get; set; }

        public TestParameters()
        {
            DutPackageType = DutPackageType.A1;
            TestParametersType = TestParametersType.OutputResidualVoltage;
            PolarityDCSwitchingVoltageApplication = PolarityDCSwitchingVoltageApplication.Direct;
            SwitchingCurrentPulseShape = SwitchingCurrentPulseShape.Sinus;
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