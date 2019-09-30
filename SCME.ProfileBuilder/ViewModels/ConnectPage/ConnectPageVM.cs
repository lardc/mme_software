using PropertyChanged;
using SCME.WpfControlLibrary.ViewModels;

namespace SCME.ProfileBuilder.ViewModels.ConnectPage
{
    [AddINotifyPropertyChangedInterface]
    public class ConnectPageVM
    {
        public ConnectToMSSQLVM ConnectToMSSQLVM { get; set; } = new ConnectToMSSQLVM();
        public ConnectToSQLiteVM ConnectToSQLiteVM { get; set; } = new ConnectToSQLiteVM();
        public AccentAndThemeUserControlVM AccentAndThemeUserControlVM { get; set; } = new AccentAndThemeUserControlVM();
    }
}
