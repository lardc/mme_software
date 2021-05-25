using System.Windows;

namespace SCME.UI.CustomControl
{
    public partial class DialogWindow
    {
        /// <summary>Инициализирует новый экземпляр класса DialogWindow</summary>
        /// <param name="title">Заголовок диалогового окна</param>
        /// <param name="message">Текст сообщения</param>
        public DialogWindow(string title, string message)
        {
            InitializeComponent();
            lblTitle.Content = title;
            tbMessage.Text = message;
        }

        /// <summary>Конфигурирование диалогового окна</summary>
        /// <param name="conf">Параметр конфигурации</param>
        public void ButtonConfig(EbConfig conf)
        {
            switch (conf)
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

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        /// <summary>Конфигурация диалогового окна</summary>
        public enum EbConfig
        {
            OK,
            Cancel,
            OKCancel
        }
    }
}