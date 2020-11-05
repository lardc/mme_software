using PropertyChanged;
using SCME.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace SCME.UI.ViewModels
{
    

    [AddINotifyPropertyChangedInterface]
    public class AttestationPageVM
    {
        public string NameParameter { get; set; }
        public AttestationType AttestationType { get; set; } = AttestationType.Current;
        public int Parameter { get; set; }
        public uint Value { get; set; }
        public int NumberPosition { get; set; } = 1;

        public double FormedValue { get; set; }
        public double MeasuredValue { get; set; }

    }
}
