using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using SCME.Types;
using SCME.Types.BaseTestParams;
using SCME.UI.ModelViews;
using SCME.UI.Properties;

namespace SCME.UI.PagesTech
{
    /// <summary>
    /// Interaction logic for ClampPage.xaml
    /// </summary>
    public partial class TOUPage : Page
    {
        
        public TOUPageVM VM { get; set; } = new TOUPageVM();
        public Types.Clamping.TestParameters ClampParameters { get; set; }
        public Types.TOU.TestParameters paramTOU { get; set; }
        public Types.Commutation.ModuleCommutationType CommType { get; set; }
        public Types.Commutation.ModulePosition ModPosition { get; set; }

        private const int RoomTemp = 25;
        private const int TIME_STEP = 5;

        public int Temperature { get; set; }

        public TOUPage()
        {
            paramTOU = new Types.TOU.TestParameters { IsEnabled = true };
            ClampParameters = new Types.Clamping.TestParameters { StandardForce = Types.Clamping.ClampingForceInternal.Custom, CustomForce = 5 };
            CommType = Settings.Default.SinglePositionModuleMode ? Types.Commutation.ModuleCommutationType.Direct : Types.Commutation.ModuleCommutationType.MT3;
            Temperature = RoomTemp;

            InitializeComponent();

            ClearStatus();
        }



        private void ClearStatus()
        {
            lblWarning.Visibility = Visibility.Collapsed;
            lblFault.Visibility = Visibility.Collapsed;
        }

        internal void SetWarning(Types.TOU.HWWarningReason Warning)
        {
            lblWarning.Content = Warning.ToString();
            lblWarning.Visibility = Visibility.Visible;
        }

        internal void SetFault(Types.TOU.HWFaultReason Fault)
        {
            lblFault.Content = Fault.ToString();
            lblFault.Visibility = Visibility.Visible;
            VM.IsRunning = false;
        }

        internal void SetResult(DeviceState State, Types.TOU.TestResults Result)
        {
            VM.IsRunning = false;
            VM.State = State.ToString();
            VM.ITM = Result.ITM;
            VM.TGD = Result.TGD;
            VM.TGT = Result.TGT;
        }

        private void Stop_Click(object Sender, RoutedEventArgs E)
        {
            Cache.Net.StopByButtonStop();
        }

        private void Back_Click(object Sender, RoutedEventArgs E)
        {
            if (NavigationService != null)
                NavigationService.GoBack();
        }        

        private void BtnStart_OnClick(object sender, RoutedEventArgs e)
        {
            if (VM.IsRunning)
                return;

            var paramGate = new Types.Gate.TestParameters { IsEnabled = false };
            var paramVtm = new Types.SL.TestParameters { IsEnabled = false };
            var paramBvt = new Types.BVT.TestParameters { IsEnabled = false };
            var paramATU = new Types.ATU.TestParameters { IsEnabled = false };
            var paramQrrTq = new Types.QrrTq.TestParameters { IsEnabled = false };
            var paramRAC = new Types.RAC.TestParameters { IsEnabled = false };
            var paramIH = new Types.IH.TestParameters { IsEnabled = false };
            var paramRCC = new Types.RCC.TestParameters { IsEnabled = false };

            //если пресс был зажат вручную - не стоит пробовать зажимать его ещё раз
            ClampParameters.SkipClamping = Cache.Clamp.ManualClamping;

            if (!Cache.Net.Start(paramGate, paramVtm, paramBvt, paramATU, paramQrrTq, paramRAC, paramIH, paramRCC,
                                 new Types.Commutation.TestParameters
                                 {
                                     BlockIndex = (!Cache.Clamp.clampPage.UseTmax) ? Types.Commutation.HWBlockIndex.Block1 : Types.Commutation.HWBlockIndex.Block2,
                                     CommutationType = ConverterUtil.MapCommutationType(CommType),
                                     Position = ConverterUtil.MapModulePosition(ModPosition)
                                 }, ClampParameters, paramTOU))
                return;

            ClearStatus();
            VM.IsRunning = true;


            //Cache.Net.Start()

        }
    }
}
