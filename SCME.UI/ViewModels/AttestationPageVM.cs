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
        public AttestationType AttestationType { get; set; } = AttestationType.Current;
        public string NameParameter { get; set; }
        public int Parameter { get; set; }
        public uint Current { get; set; }
        public uint Voltage { get; set; }
        public int NumberPosition { get; set; } = 1;

        public bool ShowAttestationType => Parameter <= 3; 

        public double CurrentResult { get; set; }
        public double VoltageResult { get; set; }

    }
}
