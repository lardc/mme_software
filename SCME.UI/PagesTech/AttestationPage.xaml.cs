using SCME.Types;
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

        public void SetValues(AttestationParameterResponse attestationParameterResponse)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                VM.CurrentResult = attestationParameterResponse.Current;
                VM.VoltageResult = attestationParameterResponse.Voltage;
            }));
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            Cache.Net.StartAttestation(new AttestationParameterRequest( VM.Parameter, VM.Current, VM.Voltage, VM.NumberPosition, VM.AttestationType));
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
