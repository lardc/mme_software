﻿using PropertyChanged;
using SCME.Types.BaseTestParams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SCME.Types.OutputLeakageCurrent
{
   

    [AddINotifyPropertyChangedInterface]
    [DataContract(Name = "Tou.OutputLeakageCurrent", Namespace = "http://proton-electrotex.com/SCME")]
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
        public double ControlVoltage { get; set; }
        [DataMember]
        public double ControlCurrent  { get; set; }

        [DataMember]
        public ApplicationPolarityConstantSwitchingVoltage ApplicationPolarityConstantSwitchingVoltage { get; set; }
        [DataMember]
        public PolarityDCSwitchingVoltageApplication PolarityDCSwitchingVoltageApplication { get; set; }

        [DataMember]
        public double SwitchedAmperage { get; set; }
        [DataMember]
        public double SwitchedVoltage { get; set; }

        [DataMember]
        public double AuxiliaryVoltagePowerSupply1 { get; set; }
        [DataMember]
        public double AuxiliaryCurrentPowerSupply1 { get; set; }

        [DataMember]
        public double AuxiliaryVoltagePowerSupply2 { get; set; }
        [DataMember]
        public double AuxiliaryCurrentPowerSupply2 { get; set; }

        [DataMember]
        public double LeakageCurrentMinimum { get; set; }
        [DataMember]
        public double LeakageCurrentMaximum { get; set; }


        public TestParameters()
        {
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