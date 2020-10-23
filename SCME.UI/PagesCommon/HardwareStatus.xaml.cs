using System;
using System.Collections.Generic;
using System.Windows;
using SCME.Types;
using SCME.UI.IO;

namespace SCME.UI.PagesCommon
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class HardwareStatusPage
    {
        private bool m_IsBackEnable;
        private bool m_IsRestartEnable;

        public HardwareStatusPage()
        {
            InitializeComponent();

            IsBackEnable = false;
            IsRestartEnable = false;

            btnRestart.Click += (object sender, RoutedEventArgs e) => System.Windows.Application.Current.Shutdown();
            
            
            
        }

        internal bool IsBackEnable
        {
            get { return m_IsBackEnable; }
            set
            {
                m_IsBackEnable = value;
                btnBack.Visibility = m_IsBackEnable ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        internal bool IsRestartEnable
        {
            get { return m_IsRestartEnable; }
            set
            {
                m_IsRestartEnable = value;
                    //btnRestart.Visibility = m_IsRestartEnable ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        internal void State(ComplexParts device, DeviceConnectionState connectionState, string message)
        {
            switch (device)
            {
                case ComplexParts.Service:
                    serviceControl.SetConnectionStatus(connectionState, message);
                    break;
                case ComplexParts.Sync:
                    syncControl.SetConnectionStatus(connectionState, message);
                    break;
                case ComplexParts.Adapter:
                    adapterControl.SetConnectionStatus(connectionState, message);
                    break;
                case ComplexParts.Gateway:
                    gatewayControl.SetConnectionStatus(connectionState, message);
                    break;
                case ComplexParts.Commutation:
                    commutationControl.SetConnectionStatus(connectionState, message);
                    break;
                case ComplexParts.Gate:
                    gateControl.SetConnectionStatus(connectionState, message);
                    break;
                case ComplexParts.SL:
                    vtmControl.SetConnectionStatus(connectionState, message);
                    break;
                case ComplexParts.BVT:
                    bvtControl.SetConnectionStatus(connectionState, message);
                    break;
                case ComplexParts.Clamping:
                    clampControl.SetConnectionStatus(connectionState, message);
                    break;
                case ComplexParts.CommutationEx:
                    commutationControlEx.SetConnectionStatus(connectionState, message);
                    break;
                case ComplexParts.DvDt:
                    dVdtControl.SetConnectionStatus(connectionState, message);
                    break;
                case ComplexParts.ATU:
                    aTUControl.SetConnectionStatus(connectionState, message);
                    break;
                case ComplexParts.QrrTq:
                    qrrTqControl.SetConnectionStatus(connectionState, message);
                    break;
                case ComplexParts.RAC:
                    rACControl.SetConnectionStatus(connectionState, message);
                    break;
                case ComplexParts.IH:
                    ihControl.SetConnectionStatus(connectionState, message);
                    break;
                case ComplexParts.TOU:
                    touControl.SetConnectionStatus(connectionState, message);
                    break;
                case ComplexParts.SSRTU:
                    sSRTUControl.SetConnectionStatus(connectionState, message);
                    break;
            }
        }

        internal bool IsDeviceEnabled(ComplexParts Device)
        {
            switch (Device)
            {
                case ComplexParts.Service:
                    return !serviceControl.IsDisabled;
                case ComplexParts.Sync:
                    return !syncControl.IsDisabled;
                case ComplexParts.Adapter:
                    return !adapterControl.IsDisabled;
                case ComplexParts.Gateway:
                    return !gatewayControl.IsDisabled;
                case ComplexParts.Commutation:
                    return !commutationControl.IsDisabled;
                case ComplexParts.Gate:
                    return !gateControl.IsDisabled;
                case ComplexParts.SL:
                    return !vtmControl.IsDisabled;
                case ComplexParts.BVT:
                    return !bvtControl.IsDisabled;
                case ComplexParts.Clamping:
                    return !clampControl.IsDisabled;
                case ComplexParts.CommutationEx:
                    return !commutationControlEx.IsDisabled;
                case ComplexParts.DvDt:
                    return !dVdtControl.IsDisabled;
                case ComplexParts.ATU:
                    return !aTUControl.IsDisabled;
                case ComplexParts.QrrTq:
                    return !qrrTqControl.IsDisabled;
                case ComplexParts.RAC:
                    return !rACControl.IsDisabled;
                case ComplexParts.IH:
                    return !ihControl.IsDisabled;
                case ComplexParts.TOU:
                    return !touControl.IsDisabled;
                case ComplexParts.SSRTU:
                    return !sSRTUControl.IsDisabled;
                default:
                    return false;
            }
        }

        internal int GetTimeout(ComplexParts Device)
        {
            switch (Device)
            {
                case ComplexParts.Service:
                    return serviceControl.OperationTimeout;
                case ComplexParts.Sync:
                    return syncControl.OperationTimeout;
                case ComplexParts.Commutation:
                    return commutationControl.OperationTimeout;
                case ComplexParts.CommutationEx:
                    return commutationControlEx.OperationTimeout;
                case ComplexParts.Adapter:
                    return adapterControl.OperationTimeout;
                case ComplexParts.Gateway:
                    return gatewayControl.OperationTimeout;
                case ComplexParts.Gate:
                    return gateControl.OperationTimeout;
                case ComplexParts.SL:
                    return vtmControl.OperationTimeout;
                case ComplexParts.BVT:
                    return bvtControl.OperationTimeout;
                case ComplexParts.Clamping:
                    return clampControl.ClampTimeout;
                case ComplexParts.DvDt:
                    return dVdtControl.OperationTimeout;
                case ComplexParts.ATU:
                    return aTUControl.OperationTimeout;
                case ComplexParts.QrrTq:
                    return qrrTqControl.OperationTimeout;
                case ComplexParts.RAC:
                    return rACControl.OperationTimeout;
                case ComplexParts.IH:
                    return ihControl.OperationTimeout;
                case ComplexParts.TOU:
                    return touControl.OperationTimeout;
                case ComplexParts.SSRTU:
                    return sSRTUControl.OperationTimeout;
                default:
                    return 10000;
            }
        }

        private void Back_Click(object Sender, RoutedEventArgs E)
        {
            if (NavigationService != null)
                NavigationService.GoBack();
        }

        private void welcomeScreen_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}