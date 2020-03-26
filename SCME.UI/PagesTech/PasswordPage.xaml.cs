using System.Windows;
using System.Windows.Navigation;
using SCME.UIServiceConfig.Properties;

namespace SCME.UI.PagesTech
{
    /// <summary>
    /// Interaction logic for PasswordPage.xaml
    /// </summary>
    public partial class PasswordPage
    {
        public delegate void delegateAfterOkRoutine(NavigationService NavigationService);
        public delegateAfterOkRoutine AfterOkRoutine;

        public PasswordPage()
        {
            InitializeComponent();
            AfterOkRoutine = null;
        }

        private void BtnOk_Clicked(object Sender, RoutedEventArgs E)
        {
            var q = Settings.Default.TechPassword;
            if (tbPassword.Text == Settings.Default.TechPassword && NavigationService != null)
            {
                if (AfterOkRoutine != null)
                {
                    AfterOkRoutine(NavigationService);
                    AfterOkRoutine = null;
                }
                lblIncorrect.Visibility = Visibility.Hidden;
            }
            else
                lblIncorrect.Visibility = Visibility.Visible;

            tbPassword.Text = "";
        }

        private void BtnCancel_OnClick(object Sender, RoutedEventArgs E)
        {
            NavigationService?.GoBack();
        }

        private void PasswordPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            lblIncorrect.Visibility = Visibility.Hidden;
        }
    }
}