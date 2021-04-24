using System.ServiceProcess;

namespace SCME.DatabaseServer
{
    /// <summary>Служба базы данных</summary>
    public partial class DatabaseService : ServiceBase
    {
        /// <summary>Инициализирует новый экземпляр класса DatabaseService</summary>
        public DatabaseService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] Args) //Запуск службы
        {
            base.OnStart(Args);
            SystemHost.StartService();
        }

        protected override void OnStop() //Остановка службы
        {
            base.OnStop();
            SystemHost.StopService();
        }
    }
}
