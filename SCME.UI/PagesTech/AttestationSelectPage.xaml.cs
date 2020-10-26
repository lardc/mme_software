using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SCME.UI.PagesTech
{
    /// <summary>
    /// Логика взаимодействия для Attestation.xaml
    /// </summary>
    public partial class AttestationSelectPage : Page
    {
        public AttestationSelectPage()
        {
            InitializeComponent();
        }

        private void Button_Click(object Sender, RoutedEventArgs E)
        {
            var btn = (Button)Sender;

            var page = new AttestationPage();
            page.VM.Parameter = Convert.ToUInt16(btn.CommandParameter);
            page.VM.NameParameter = ((Label)((Button)Sender).Content).Content as string; ;
            Cache.FireSSRTUAttestation = page.SetValues;
            if (page != null && NavigationService != null)
                NavigationService.Navigate(page);
        }
    }
}
