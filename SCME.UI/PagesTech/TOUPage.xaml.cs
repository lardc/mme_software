using System.Windows;
using System.Windows.Controls;
using SCME.Types;
using SCME.UI.ModelViews;

namespace SCME.UI.PagesTech
{
    /// <summary>
    /// Interaction logic for ClampPage.xaml
    /// </summary>
    public partial class TOUPage : Page
    {
        private bool m_IsRunning;
        public TOUPageVM VM { get; set; } = new TOUPageVM();

        public TOUPage()
        {
            InitializeComponent();

            ClearStatus();
        }

        internal bool IsRunning
        {
            get
            {
                return m_IsRunning;
            }
            set
            {
                m_IsRunning = value;
                btnBack.IsEnabled = !m_IsRunning;
            }
        }

        private void ClearStatus()
        {
            lblWarning.Visibility = Visibility.Collapsed;
            lblFault.Visibility = Visibility.Collapsed;
        }

        internal void SetWarning(Types.dVdt.HWWarningReason Warning)
        {
            lblWarning.Content = Warning.ToString();
            lblWarning.Visibility = Visibility.Visible;
        }

        internal void SetFault(Types.dVdt.HWFaultReason Fault)
        {
            lblFault.Content = Fault.ToString();
            lblFault.Visibility = Visibility.Visible;
            IsRunning = false;
        }

        internal void SetResult(DeviceState State, Types.TOU.TestResults Result)
        {
            IsRunning = false;
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

        //    if (IsRunning)
        //        return;

        //    var paramGate = new Types.Gate.TestParameters { IsEnabled = false };
        //    var paramVtm = new Types.SL.TestParameters { IsEnabled = false };
        //    var paramBvt = new Types.BVT.TestParameters { IsEnabled = false };
        //    var paramATU = new Types.ATU.TestParameters { IsEnabled = false };
        //    var paramQrrTq = new Types.QrrTq.TestParameters { IsEnabled = false };
        //    var paramRAC = new Types.RAC.TestParameters { IsEnabled = false };
        //    var paramRCC = new Types.RCC.TestParameters { IsEnabled = false };

        //    //если пресс был зажат вручную - не стоит пробовать зажимать его ещё раз
        //    ClampParameters.SkipClamping = Cache.Clamp.ManualClamping;

        //    if (!Cache.Net.Start(paramGate, paramVtm, paramBvt, paramATU, paramQrrTq, paramRAC, Parameters, paramRCC,
        //                         new Types.Commutation.TestParameters
        //                         {
        //                             BlockIndex = (!Cache.Clamp.clampPage.UseTmax) ? Types.Commutation.HWBlockIndex.Block1 : Types.Commutation.HWBlockIndex.Block2,
        //                             CommutationType = ConverterUtil.MapCommutationType(CommType),
        //                             Position = ConverterUtil.MapModulePosition(ModPosition)
        //                         }, ClampParameters))
        //        return;

        //    ClearStatus();
        //    IsRunning = true;

        //    if (IsRunning)
        //        return;

        //    //если пресс был зажат вручную - не стоит пробовать зажимать его ещё раз
        //    ClampParameters.SkipClamping = Cache.Clamp.ManualClamping;

        //    var commPar = new Types.Commutation.TestParameters()
        //    {
        //        BlockIndex = (!Cache.Clamp.UseTmax) ? Types.Commutation.HWBlockIndex.Block1 : Types.Commutation.HWBlockIndex.Block2,
        //        CommutationType = ConverterUtil.MapCommutationType(CommType),
        //        Position = ConverterUtil.MapModulePosition(ModPosition)
        //    };

        //    var parameters = new List<BaseTestParametersAndNormatives>(1);
        //    parameters.Add(Parameters);

        //    if (!Cache.Net.Start(commPar, ClampParameters, parameters))
        //        return;
        //    IsRunning = true;
        //}


        //Cache.Net.Start()

        }
    }
}
