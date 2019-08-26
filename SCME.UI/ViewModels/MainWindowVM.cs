using PropertyChanged;
using SCME.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SCME.UI.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class MainWindowVM
    {
        public SafetyMode SafetyMode { get; set; } = SafetyMode.Internal;
        public Visibility SafetyVisibility { get; set; }

        public bool IsSafetyBreakIconVisible { get; set; }

        public Visibility GoTechButtonVisibility { get; set; }

        public Visibility AccountButtonVisibility { get; set; }

        public Visibility TechPasswordVisibility { get; set; }

        public string SyncState { get; set; }

        public string TopTitle { get; set; }

    }
}
