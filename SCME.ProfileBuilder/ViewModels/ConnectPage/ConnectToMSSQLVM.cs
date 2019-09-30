using PropertyChanged;

namespace SCME.ProfileBuilder.ViewModels.ConnectPage
{
    [AddINotifyPropertyChangedInterface]
    public class ConnectToMSSQLVM
    {
        public string Server { get => Properties.Settings.Default.MSSQLServer; set => Properties.Settings.Default.MSSQLServer = value; }
        public string Database { get => Properties.Settings.Default.MSSQLDatabase; set => Properties.Settings.Default.MSSQLDatabase = value; }
        public bool IntegratedSecurity { get => Properties.Settings.Default.MSSQLIntegratedSecurity; set => Properties.Settings.Default.MSSQLIntegratedSecurity = value; }
        [DependsOn(nameof(IntegratedSecurity))]
        public bool IsUserPasswordEnabled => !IntegratedSecurity;

        public string UserId { get => Properties.Settings.Default.MSSQLUserId; set => Properties.Settings.Default.MSSQLUserId = value; }
        public string Password { get; set; }

        [DependsOn(nameof(Server), nameof(Database), nameof(IntegratedSecurity), nameof(UserId), nameof(Password))]
        public bool ConnectedButtonEnabled
        {
            get 
            {
                if (IntegratedSecurity)
                    return string.IsNullOrEmpty(Server) == false && string.IsNullOrEmpty(Database) == false;
                else
                    return string.IsNullOrEmpty(Server) == false && string.IsNullOrEmpty(Database) == false && string.IsNullOrEmpty(UserId) == false && string.IsNullOrEmpty(Password) == false;
            }
        }
    }
}
