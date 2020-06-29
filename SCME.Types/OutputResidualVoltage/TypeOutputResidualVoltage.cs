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
    [DataContract(Name = "Tou.TestParameters", Namespace = "http://proton-electrotex.com/SCME")]
    [KnownType(typeof(BaseTestParametersAndNormatives))]
    public class TestParameters : BaseTestParametersAndNormatives, ICloneable
    {

        [DependsOn(nameof(TypeManagement))]
        public bool ShowVoltage => TypeManagement == TypeManagement.DCVoltage || TypeManagement == TypeManagement.ACVoltage;
        [DependsOn(nameof(TypeManagement))]
        public bool ShowAmperage => TypeManagement == TypeManagement.DCAmperage;


        public int NumberPosition { get; set; }
        public TypeManagement TypeManagement { get; set; }

        public double ControlVoltage { get; set; }
        public double ControlCurrent { get; set; }

        public PolarityDCSwitchingVoltageApplication PolarityDCSwitchingVoltageApplication { get; set; }

        public double SwitchedAmperage { get; set; }
        public double SwitchedVoltage { get; set; }

        public double AuxiliaryVoltagePowerSupply1 { get; set; }
        public double AuxiliaryCurrentPowerSupply1 { get; set; }

        public double AuxiliaryVoltagePowerSupply2 { get; set; }
        public double AuxiliaryCurrentPowerSupply2 { get; set; }

        public SwitchingCurrentPulseShape SwitchingCurrentPulseShape { get; set; }
        public double SwitchingCurrentPulseDuration { get; set; }

        public double OutputResidualVoltageMinimum{get;set;}
        public double OutputResidualVoltageMaximum { get; set; }



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