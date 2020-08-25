using PropertyChanged;
using SCME.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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

        public SyncMode SyncMode { get; set; }
        public string TopTitle { get; set; }

        [DependsOn(nameof(SyncMode))] public bool IsLocal => SyncMode == SyncMode.Local;
        [DependsOn(nameof(SyncMode))] public bool IsCentral => SyncMode != SyncMode.Local;

        public string  MmeCode { get; set; }
        
        public bool AccountNameIsVisibility { get; set; }

        public string AccountName { get; set; }

        public string Version { get; set; } = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;

        public bool WaitProgressBarIsShow { get; set; } = false;

    }
}
