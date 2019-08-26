using PropertyChanged;
using SCME.UI.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SCME.UI.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class UserWorkModePageVM 
    {
        public bool ButtonsModeIsEnabled { get; set; } = true;

        public Visibility SpecialMeasureVisibility { get; set; } = Settings.Default.SpecialMeasureForUse ? Visibility.Visible : Visibility.Collapsed;
    }
}
