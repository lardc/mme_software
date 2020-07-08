using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Text;

namespace SCME.WpfControlLibrary.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class ImpulseResultVM
    {
        public bool CanStart { get; set; } = true;

        [DependsOn(nameof(CanStart))]
        public bool CanCanselMeasument => !CanStart;
    }
}
