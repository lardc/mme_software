using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Documents;

namespace SCME.WpfControlLibrary.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class SSRTUResultComponentVM
    {
        public int Postition { get; set; }


        public double? LeakageCurrent { get; set; }
        public double? InputAmperage { get; set; }
        public double? InputVoltage { get; set; }
        public double? ResidualVoltage { get; set; }
        public double? ProhibitionVoltage { get; set; }


        public double? LeakageCurrentMin { get; set; }
        public double? InputAmperageMin { get; set; }
        public double? InputVoltageMin { get; set; }
        public double? ResidualVoltageMin { get; set; }
        public double? ProhibitionVoltageMin { get; set; }


        public double? LeakageCurrentMax { get; set; }
        public double? InputAmperageMax { get; set; }
        public double? InputVoltageMax { get; set; }
        public double? ResidualVoltageMax { get; set; }
        public double? ProhibitionVoltageMax { get; set; }


        [DependsOn(nameof(LeakageCurrent), nameof(LeakageCurrentMin), nameof(LeakageCurrentMax))]
        public bool? LeakageCurrentIsOk => LeakageCurrentMin == null ? (bool?)null : (LeakageCurrentMin < LeakageCurrent && LeakageCurrent < LeakageCurrentMax);


        [DependsOn(nameof(InputAmperage), nameof(InputAmperageMin), nameof(InputAmperageMax))]
        public bool? InputAmperageIsOk => InputAmperageMin == null ? (bool?)null : InputAmperageMin < InputAmperage && InputAmperage < InputAmperageMax;


        [DependsOn(nameof(InputVoltage), nameof(InputVoltageMin), nameof(InputVoltageMax))]
        public bool? InputVoltageIsOk => InputVoltageMin == null ? (bool?)null : InputVoltageMin < InputVoltage && InputVoltage < InputVoltageMax;


        [DependsOn(nameof(ResidualVoltage), nameof(ResidualVoltageMin), nameof(ResidualVoltageMax))]
        public bool? ResidualVoltageIsOk => ResidualVoltageMin == null ? (bool?)null : ResidualVoltageMin < ResidualVoltage && ResidualVoltage < ResidualVoltageMax;


        [DependsOn(nameof(ProhibitionVoltage), nameof(ProhibitionVoltageMin), nameof(ProhibitionVoltageMax))]
        public bool? ProhibitionVoltageIsOk => ProhibitionVoltageMin == null ? (bool?)null : ProhibitionVoltageMin < ProhibitionVoltage && ProhibitionVoltage < ProhibitionVoltageMax;

    }
}
