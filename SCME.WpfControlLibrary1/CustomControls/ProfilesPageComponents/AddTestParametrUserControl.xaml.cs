using System;
using System.Windows;
using System.Windows.Controls;

namespace SCME.WpfControlLibrary.CustomControls.ProfilesPageComponents
{
    /// <summary>
    /// Логика взаимодействия для AddTestParametrUserControl.xaml
    /// </summary>
    public partial class AddTestParameterUserControl : UserControl
    {
        public bool IsReadOnly { get; set; }
        
        public event Action AddTestParametersEvent;

        public AddTestParameterUserControl()
        {
            InitializeComponent();
        }

        private void AddTestParameters_Click(object sender, RoutedEventArgs e)
        {
            AddTestParametersEvent?.Invoke();
        }
    }
}
