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
    [DataContract(Name = "Tou.TestParameters", Namespace = "http://proton-electrotex.com/SCME")]
    [KnownType(typeof(BaseTestParametersAndNormatives))]
    public class TestParameters : BaseTestParametersAndNormatives, ICloneable
    {

        [DependsOn(nameof(TypeManagement))]
        public bool ShowVoltage => TypeManagement == TypeManagement.DCVoltage || TypeManagement == TypeManagement.ACVoltage;
        [DependsOn(nameof(TypeManagement))]
        public bool ShowAmperage => TypeManagement == TypeManagement.DCAmperage;


        public int NumberPosition { get; set; } = 1;
        public TypeManagement TypeManagement { get; set; }

        public double ControlVoltage { get; set; }
        public double ControlCurrent { get; set; }


        public double AuxiliaryVoltagePowerSupply1 { get; set; }
        public double AuxiliaryCurrentPowerSupply1 { get; set; }

        public double AuxiliaryVoltagePowerSupply2 { get; set; }
        public double AuxiliaryCurrentPowerSupply2 { get; set; }

        public double InputCurrentMinimum { get; set; }
        public double InputCurrentMaximum { get; set; }

        public double InputVoltageMinimum { get; set; }
        public double InputVoltageMaximum { get; set; }

        

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