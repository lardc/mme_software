using PropertyChanged;
using SCME.Types;
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
        public DutPackageType DutPackageType { get; set; }

        public bool ShowAuxiliaryCurrentPowerSupply1 => DutPackageType == DutPackageType.B5 || DutPackageType == DutPackageType.V108;

        public bool ShowAuxiliaryCurrentPowerSupply2 => DutPackageType == DutPackageType.V108;


        public double? LeakageCurrent { get; set; }
        public double? InputAmperage { get; set; }
        public double? InputVoltage { get; set; }
        public double? ResidualVoltage { get; set; }
        public double? ProhibitionVoltage { get; set; }
        public double? AuxiliaryCurrentPowerSupply1 { get; set; }
        public double? AuxiliaryCurrentPowerSupply2 { get; set; }

        

        public double? LeakageCurrentMin { get; set; }
        public double? InputAmperageMin { get; set; }
        public double? InputVoltageMin { get; set; }
        public double? ResidualVoltageMin { get; set; }
        public double? ProhibitionVoltageMin { get; set; }
        public double? AuxiliaryCurrentPowerSupplyMin1 { get; set; }
        public double? AuxiliaryCurrentPowerSupplyMin2 { get; set; }

        


        public double? LeakageCurrentMax { get; set; }
        public double? InputAmperageMax { get; set; }
        public double? InputVoltageMax { get; set; }
        public double? ResidualVoltageMax { get; set; }
        public double? ProhibitionVoltageMax { get; set; }
        public double? AuxiliaryCurrentPowerSupplyMax1 { get; set; }
        public double? AuxiliaryCurrentPowerSupplyMax2 { get; set; }


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


        [DependsOn(nameof(AuxiliaryCurrentPowerSupply1), nameof(AuxiliaryCurrentPowerSupplyMin1), nameof(AuxiliaryCurrentPowerSupplyMax1))]
        public bool? AuxiliaryCurrentPowerSupply1IsOk => AuxiliaryCurrentPowerSupplyMin1 == null ? (bool?)null : AuxiliaryCurrentPowerSupplyMin1 < AuxiliaryCurrentPowerSupply1 && AuxiliaryCurrentPowerSupply1 < AuxiliaryCurrentPowerSupplyMax1;


        [DependsOn(nameof(AuxiliaryCurrentPowerSupply2), nameof(AuxiliaryCurrentPowerSupplyMin2), nameof(AuxiliaryCurrentPowerSupplyMax2))]
        public bool? AuxiliaryCurrentPowerSupply2IsOk => AuxiliaryCurrentPowerSupplyMin2 == null ? (bool?)null : AuxiliaryCurrentPowerSupplyMin2 < AuxiliaryCurrentPowerSupply2 && AuxiliaryCurrentPowerSupply2 < AuxiliaryCurrentPowerSupplyMax2;

    }
}
