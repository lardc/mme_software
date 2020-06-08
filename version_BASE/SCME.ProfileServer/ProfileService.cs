using System.ServiceProcess;

namespace SCME.ProfileServer
{
    public partial class ProfileService : ServiceBase
    {
        public ProfileService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] Args)
        {
            base.OnStart(Args);

            SystemHost.StartService();
        }

        protected override void OnStop()
        {
            base.OnStop();

            SystemHost.StopService();
        }
    }
}
