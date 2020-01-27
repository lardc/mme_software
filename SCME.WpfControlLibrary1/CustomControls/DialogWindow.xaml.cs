using System.Windows;
using SCME.WpfControlLibrary.Commands;

namespace SCME.WpfControlLibrary.CustomControls
{
    public partial class DialogWindow
    {
        private bool Result { get; set; }
        public string Message { get; set; }
        public bool IsShowCancelButton  { get; private set; }
        public DialogWindow(string title, string message, bool isShowCancelButton = false)
        {
            Title = title;
            Message = message;
            IsShowCancelButton = isShowCancelButton;
            InitializeComponent();
        }

        public RelayCommand OkCommand => new RelayCommand(o =>
        {
            Result = true;
            Close();
        });
        
        public  RelayCommand CancelCommand => new RelayCommand(o =>
        {
            Result = false;
            Close();
        });


        private void DialogWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var curApp = Application.Current;
            var mainWindow = curApp.MainWindow;
            if (mainWindow == null) return;
            Left = mainWindow.Left + (mainWindow.Width - this.ActualWidth) / 2;
            Top = mainWindow.Top + (mainWindow.Height - this.ActualHeight) / 2;
        }

        public bool ShowDialogWithResult()
        {
            ShowDialog();
            return Result;
        }
    }
}