﻿using PropertyChanged;
using SCME.Types.BaseTestParams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SCME.Types.InputOptions
{
    [AddINotifyPropertyChangedInterface]
    [DataContract(Name = "InputOptions.TestParameters", Namespace = "http://proton-electrotex.com/SCME")]
    [KnownType(typeof(BaseTestParametersAndNormatives))]
    [KnownType(typeof(BaseTestParametersAndNormativesSSRTU))]
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


        [DataMember]
        public double AuxiliaryCurrentPowerSupplyMinimum1 { get; set; }
        [DataMember]
        public double AuxiliaryCurrentPowerSupplyMaximum1 { get; set; }

        [DataMember]
        public double AuxiliaryCurrentPowerSupplyMinimum2 { get; set; }
        [DataMember]
        public double AuxiliaryCurrentPowerSupplyMaximum2 { get; set; }

        public TestParameters()
        {
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