using System.Windows;
using System.Windows.Media;
using Color = System.Drawing.Color;

namespace SCME.ProfileBuilder.CustomControl
{
    /// <summary>
    ///     Interaction logic for DialogWindow.xaml
    /// </summary>
    public partial class InsertNameDialog
    {
        public enum EbConfig
        {
            OK,
            Cancel,
            OKCancel
        }

        private string title;

        public InsertNameDialog(string Title, string Message)
        {
            InitializeComponent();
            title = Title;
            lblTitle.Content = Title;
            tbMessage.Text = Message;
        }

        public void ButtonConfig(EbConfig Conf)
        {
            switch (Conf)
            {
                case EbConfig.OKCancel:
                    mainGrid.ColumnDefinitions[0].Width = new GridLength(60, GridUnitType.Star);
                    mainGrid.ColumnDefinitions[1].Width = new GridLength(60, GridUnitType.Star);
                    break;
                case EbConfig.OK:
                    mainGrid.ColumnDefinitions[0].Width = new GridLength(60, GridUnitType.Star);
                    mainGrid.ColumnDefinitions[1].Width = new GridLength(0, GridUnitType.Star);
                    break;
                case EbConfig.Cancel:
                    mainGrid.ColumnDefinitions[0].Width = new GridLength(0, GridUnitType.Star);
                    mainGrid.ColumnDefinitions[1].Width = new GridLength(60, GridUnitType.Star);
                    break;
            }
        }

        private void BtnOk_Click(object Sender, RoutedEventArgs E)
        {
            if (Cache.ConnectionsPage.CheckForExistingName(Cache.ConnectionsPage.MmeCodes, tbMessage.Text))
            {
                lblTitle.Content = "Такое название уже существует!";
                return;
            }
            DialogResult = true;
        }
    }
}