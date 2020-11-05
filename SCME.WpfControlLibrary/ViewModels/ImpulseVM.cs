using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Text;

namespace SCME.WpfControlLibrary.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class SSRTUVM
    {
        public bool CanStart { get; set; } = true;

        [DependsOn(nameof(CanStart))]
        public bool CanCanselMeasument => !CanStart;

        public double Result { get; set; }
        public double OpenResistance { get; internal set; }

        public bool ShowInputOptions { get; set; }
        public bool ShowOutputLeakageCurrent { get; set; }
        public bool ShowOutputResidualVoltage { get; set; }



        public double AuxiliaryCurrentPowerSupply1 { get; set; }
        public double AuxiliaryCurrentPowerSupply2 { get; set; }
        
        
    }
}
