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
    public class TOUPageVM
    {
        private const int RoomTemp = 25;
        private const int TIME_STEP = 5;

        public Types.TOU.TestParameters TOU { get; set; } = new Types.TOU.TestParameters() { IsEnabled = true };

        public Types.Clamping.TestParameters Clamping { get; set; } = new Types.Clamping.TestParameters
        {
            StandardForce = Types.Clamping.ClampingForceInternal.Custom,
            CustomForce = 5,
            IsHeightMeasureEnabled = false
        };

        

        public Types.Commutation.ModuleCommutationType CommutationType { get; set; } = Settings.Default.SinglePositionModuleMode ? Types.Commutation.ModuleCommutationType.Direct : Types.Commutation.ModuleCommutationType.MT3;
        public Types.Commutation.ModulePosition Position { get; set; }

        public int Temperature { get; set; } = RoomTemp;

        public string State { get; set; }

        public float ITM { get; set; }

        public float TGD { get; set; }

        public float TGT { get; set; }


        public bool IsRunning { get; set; }

    }
}
