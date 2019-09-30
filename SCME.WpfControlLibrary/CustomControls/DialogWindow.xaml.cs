using System.Windows;
using System.Windows.Input;
using PropertyChanged;
using SCME.WpfControlLibrary.Commands;

namespace SCME.WpfControlLibrary.CustomControls
{
    public partial class DialogWindow
    {
        public string Message { get; set; }
        public DialogWindow(string title, string message)
        {
            Title = title;
            Message = message;
            InitializeComponent();
            
        }
        
        public static ICommand CloseCommand  => new RelayCommand<Window>(w => w.Close());

        private void DialogWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var curApp = Application.Current;
            var mainWindow = curApp.MainWindow;
            if (mainWindow == null) return;
            Left = mainWindow.Left + (mainWindow.Width - this.ActualWidth) / 2;
            Top = mainWindow.Top + (mainWindow.Height - this.ActualHeight) / 2;
        }
    }
}