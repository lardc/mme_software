using SCME.UI.CustomControl;
using SCME.UIServiceConfig.Properties;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Xml;

namespace SCME.UI.PagesTech
{
    public partial class SettingsPage : Page
    {
        /// <summary>Инициализирует новый экземпляр класса SettingsPage</summary>
        public SettingsPage()
        {
            InitializeComponent();
        }

        private void settingsPage_Loaded(object sender, RoutedEventArgs e) //Загрузка страницы
        {
            //Эмулируемые и отключенные блоки
            AdapterEmulation.IsChecked = Settings.Default.AdapterEmulation;
            GatewayEmulation.IsChecked = Settings.Default.GatewayEmulation;
            CommutationEmulation.IsChecked = Settings.Default.CommutationEmulation;
            ClampEmulation.IsChecked = Settings.Default.ClampingSystemEmulation;
            ClampVisible.IsChecked = Settings.Default.ClampIsVisible;
            GTUEmulation.IsChecked = Settings.Default.GateEmulation;
            GTUVisible.IsChecked = Settings.Default.GateIsVisible;
            SLEmulation.IsChecked = Settings.Default.SLEmulation;
            SLVisible.IsChecked = Settings.Default.SLIsVisible;
            BVTEmulation.IsChecked = Settings.Default.BVTEmulation;
            BVTVisible.IsChecked = Settings.Default.BvtIsVisible;
            dVdtEmulation.IsChecked = Settings.Default.dVdtEmulation;
            dVdtVisible.IsChecked = Settings.Default.dVdtIsVisible;
            ATUEmulation.IsChecked = Settings.Default.ATUEmulation;
            ATUVisible.IsChecked = Settings.Default.ATUIsVisible;
            QrrTqEmulation.IsChecked = Settings.Default.QrrTqEmulation;
            QrrTqVisible.IsChecked = Settings.Default.QrrTqIsVisible;
            TOUEmulation.IsChecked = Settings.Default.TOUEmulation;
            TOUVisible.IsChecked = Settings.Default.TOUIsVisible;
        }

        private void btnBack_Click(object sender, RoutedEventArgs e) //Переход на предыдущую страницу
        {
            if (NavigationService != null)
                NavigationService.GoBack();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e) //Сохранение параметров конфигурации
        {
            DialogWindow DialogWindow = new DialogWindow(Properties.Resources.Warning, Properties.Resources.ConfigChangeMessage);
            DialogWindow.ButtonConfig(DialogWindow.EbConfig.OKCancel);
            if (!DialogWindow.ShowDialog().Value)
                return;
            //Расположение сборки
            string ExePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            //Расположение конфигурационных параметров
            string ConfigPath = Path.Combine(Directory.GetParent(Path.GetDirectoryName(ExePath)).FullName, "SCME.UIServiceConfig.dll.config");
            XmlDocument Document = new XmlDocument();
            Document.Load(ConfigPath);
            //Получение списка параметров
            foreach (XmlElement Element in Document.SelectNodes(@"//applicationSettings/SCME.UIServiceConfig.Properties.Settings")[0])
            {
                string NameSetting = Element.GetAttribute("name");
                switch (NameSetting)
                {
                    case "AdapterEmulation":
                        Element.InnerText = AdapterEmulation.IsChecked.ToString();
                        break;
                    case "GatewayEmulation":
                        Element.InnerText = GatewayEmulation.IsChecked.ToString();
                        break;
                    case "CommutationEmulation":
                        Element.InnerText = CommutationEmulation.IsChecked.ToString();
                        break;
                    case "ClampingSystemEmulation":
                        Element.InnerText = ClampEmulation.IsChecked.ToString();
                        break;
                    case "ClampIsVisible":
                        Element.InnerText = ClampVisible.IsChecked.ToString();
                        break;
                    case "GateEmulation":
                        Element.InnerText = GTUEmulation.IsChecked.ToString();
                        break;
                    case "GateIsVisible":
                        Element.InnerText = GTUVisible.IsChecked.ToString();
                        break;
                    case "SLEmulation":
                        Element.InnerText = SLEmulation.IsChecked.ToString();
                        break;
                    case "SLIsVisible":
                        Element.InnerText = SLVisible.IsChecked.ToString();
                        break;
                    case "BVTEmulation":
                        Element.InnerText = BVTEmulation.IsChecked.ToString();
                        break;
                    case "BvtIsVisible":
                        Element.InnerText = BVTVisible.IsChecked.ToString();
                        break;
                    case "dVdtEmulation":
                        Element.InnerText = dVdtEmulation.IsChecked.ToString();
                        break;
                    case "dVdtIsVisible":
                        Element.InnerText = dVdtVisible.IsChecked.ToString();
                        break;
                    case "ATUEmulation":
                        Element.InnerText = ATUEmulation.IsChecked.ToString();
                        break;
                    case "ATUIsVisible":
                        Element.InnerText = ATUVisible.IsChecked.ToString();
                        break;
                    case "QrrTqEmulation":
                        Element.InnerText = QrrTqEmulation.IsChecked.ToString();
                        break;
                    case "QrrTqIsVisible":
                        Element.InnerText = QrrTqVisible.IsChecked.ToString();
                        break;
                    case "TOUEmulation":
                        Element.InnerText = TOUEmulation.IsChecked.ToString();
                        break;
                    case "TOUIsVisible":
                        Element.InnerText = TOUVisible.IsChecked.ToString();
                        break;
                }
            }
            Document.Save(ConfigPath);
            Process Service = Process.GetProcessesByName("SCME.Service")[0];
            Service.Kill();
            Application.Current.Shutdown();
        }
    }
}
