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
        public double ShowAuxiliaryCurrentPowerSupply1 { get; set; }
        public double ShowAuxiliaryCurrentPowerSupply2 { get; set; }

    }
}
