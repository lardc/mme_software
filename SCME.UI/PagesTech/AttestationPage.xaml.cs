using SCME.UI.ViewModels;
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
    public partial class AttestationPage : Page
    {
        public AttestationPageVM VM { get; set; } = new AttestationPageVM();

        public AttestationPage()
        {
            InitializeComponent();
        }

        public void SetValues(double formedValue, double measuredValue)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                VM.FormedValue = formedValue;
                VM.MeasuredValue = measuredValue;
            }));
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            Cache.Net.StartAttestation(VM.NumberPosition, VM.Parameter, VM.AttestationType, VM.Value);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
