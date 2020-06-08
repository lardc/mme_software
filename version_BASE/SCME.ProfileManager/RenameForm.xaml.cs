using System;
using System.ComponentModel;
using System.Windows;


namespace SCME.ProfileManager
{
    /// <summary>
    /// Interaction logic for RenameForm.xaml
    /// </summary>
    public partial class RenameForm : Window, INotifyPropertyChanged
    {
        private string m_TextValue;

        public string TextValue
        {
            get
            {
                return m_TextValue;
            }
            set
            {
                m_TextValue = value;
                OnPropertyChanged("TextValue");
            }
        }

        public RenameForm(string InitialValue)
        {
            InitializeComponent();

            TextValue = InitialValue;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string PropertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(PropertyName));
        }

        private void BtnOk_OnClick(object Sender, RoutedEventArgs E)
        {
            DialogResult = true;
        }

        private void RenameForm_OnActivated(object Sender, EventArgs E)
        {
            textBox.Focus();
            textBox.SelectAll();
        }
    }
}
