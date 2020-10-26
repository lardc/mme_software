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

        public uint FormedValue { get; set; }
        public uint MeasuredValue { get; set; }

    }
}
