using SCME.Types;
using System.Windows;

namespace SCME.UI.PagesCommon
{
    public partial class HardwareStatusPage
    {
        private bool m_IsBackEnable;
        private bool m_IsRestartEnable;

        /// <summary>Инициализирует новый экземпляр класса HardwareStatusPage</summary>
        public HardwareStatusPage()
        {
            InitializeComponent();
            IsBackEnable = false;
            IsRestartEnable = false;
        }

        /// <summary>Активация перехода назад</summary>
        internal bool IsBackEnable
        {
            get => m_IsBackEnable;
            set
            {
                m_IsBackEnable = value;
                btnBack.Visibility = m_IsBackEnable ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>Активация перезапуска приложения</summary>
        internal bool IsRestartEnable
        {
            get => m_IsRestartEnable;
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
                default:
                    return false;
            }
        }
        
        internal void DeviceSetEnabled(ComplexParts device, bool value)
        {
            switch (device)
            {
                case ComplexParts.Service:
                    serviceControl.IsDisabled = value;
                    break;
                case ComplexParts.Sync:
                     syncControl.IsDisabled= value;
                    break;
                case ComplexParts.Adapter:
                     adapterControl.IsDisabled= value;
                    break;
                case ComplexParts.Gateway:
                     gatewayControl.IsDisabled= value;
                    break;
                case ComplexParts.Commutation:
                     commutationControl.IsDisabled= value;
                     break;
                case ComplexParts.Gate:
                     gateControl.IsDisabled= value;
                     break;
                case ComplexParts.SL:
                     vtmControl.IsDisabled= value;
                     break;
                case ComplexParts.BVT:
                     bvtControl.IsDisabled= value;
                     break;
                case ComplexParts.Clamping:
                     clampControl.IsDisabled= value;
                     break;
                case ComplexParts.CommutationEx:
                     commutationControlEx.IsDisabled= value;
                     break;
                case ComplexParts.DvDt:
                     dVdtControl.IsDisabled= value;
                     break;
                case ComplexParts.ATU:
                     aTUControl.IsDisabled= value;
                     break;
                case ComplexParts.QrrTq:
                     qrrTqControl.IsDisabled= value;
                     break;
                case ComplexParts.RAC:
                     rACControl.IsDisabled= value;
                     break;
                case ComplexParts.IH:
                     ihControl.IsDisabled= value;
                     break;
                case ComplexParts.TOU:
                     touControl.IsDisabled= value;
                     break;
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
                default:
                    return 10000;
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e) //Переход назад
        {
            if (NavigationService != null)
                NavigationService.GoBack();
        }

        private void btnRestart_Click(object sender, RoutedEventArgs e) //Перезапуск приложения
        {
            Application.Current.Shutdown();
        }
    }
}