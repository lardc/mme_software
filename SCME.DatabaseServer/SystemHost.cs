using SCME.DatabaseServer.Properties;
using SCME.InterfaceImplementations;
using SCME.InterfaceImplementations.NewImplement.MSSQL;
using SCME.Types.Utils;
using System;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;

namespace SCME.DatabaseServer
{
    /// <summary>Хост служб</summary>
    internal static class SystemHost
    {
        //Хосты служб
        private static ServiceHost MsServiceHost;
        private static ServiceHost DatabaseServiceHost;

        /// <summary>Журнал логов</summary>
        internal static LogJournal Journal
        {
            get; private set;
        }

        /// <summary>Запуск службы</summary>
        internal static void StartService()
        {
            //Создание журнала логов
            Journal = new LogJournal();
            try
            {
                string LogPath = string.Format(Settings.Default.LogPathTemplate, DateTime.Now.ToString(CultureInfo.CurrentCulture).Replace('/', '_').Replace(':', '_'));
                if (!Path.IsPathRooted(LogPath))
                    LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LogPath);
                Journal.Open(LogPath, true, true);
            }
            catch (Exception ex)
            {
                LogCriticalErrorMessage(ex);
                return;
            }
            try
            {
                SQLCentralDatabaseService Service = new SQLCentralDatabaseService(Settings.Default);
                MsServiceHost = new ServiceHost(Service);
                ServiceBehaviorAttribute Behaviour = MsServiceHost.Description.Behaviors.Find<ServiceBehaviorAttribute>();
                Behaviour.InstanceContextMode = InstanceContextMode.Single;
                MsServiceHost.Open();
            }
            catch (Exception ex)
            {
                Journal.AppendLog("SystemHost", LogJournalMessageType.Error, string.Format("Error on starting database service: {0}", ex.Message));
                return;
            }
            try
            {
                //Строка подключения к БД
                string ConnectionString = string.Format(@"Data Source={0}; Initial Catalog={1}; Integrated Security={2}; User ID={3}; Password={4}; Connect Timeout=5", Settings.Default.DbPath, Settings.Default.DBName, Settings.Default.DBIntegratedSecurity, Settings.Default.DBUser, Settings.Default.DBPassword);
                MSSQLDbService MssqlDbService;
                DatabaseServiceHost = new ServiceHost(MssqlDbService = new MSSQLDbService(new SqlConnection(ConnectionString), false));
                ServiceBehaviorAttribute Behaviour = DatabaseServiceHost.Description.Behaviors.Find<ServiceBehaviorAttribute>();
                Behaviour.InstanceContextMode = InstanceContextMode.Single;
                MssqlDbService.Migrate();
                DatabaseServiceHost.Open();
            }
            catch (Exception ex)
            {
                Journal.AppendLog("SystemHost", LogJournalMessageType.Error, string.Format("Error on starting database service: {0}", ex?.InnerException?.ToString() ?? ex.ToString()));
                return;
            }
            Journal.AppendLog("SystemHost", LogJournalMessageType.Info, string.Format("Database service loaded on {0}", DatabaseServiceHost.BaseAddresses.FirstOrDefault()));
        }

        /// <summary>Остановка службы</summary>
        internal static void StopService()
        {
            try
            {
                //Остановка службы
                if (MsServiceHost != null)
                {
                    if (MsServiceHost.State != CommunicationState.Opened)
                        MsServiceHost.Abort();
                    else
                        MsServiceHost.Close();
                    MsServiceHost = null;
                }
                //Остановка службы базы данных
                if (DatabaseServiceHost != null)
                {
                    if (DatabaseServiceHost.State != CommunicationState.Opened)
                        DatabaseServiceHost.Abort();
                    else
                        DatabaseServiceHost.Close();
                    DatabaseServiceHost = null;
                }
            }
            catch (Exception ex)
            {
                Journal.AppendLog("SystemHost", LogJournalMessageType.Warning, string.Format("Couldn't exit the database service properly: {0}", ex.Message));
            }
            //Закрытие журнала логов
            if (Journal != null)
            {
                Journal.AppendLog("SystemHost", LogJournalMessageType.Info, "Database service stopped");
                Journal.Close();
                Journal = null;
            }
        }

        /// <summary>Критическая ошибка службы</summary>
        internal static void LogCriticalErrorMessage(Exception ex)
        {
            try
            {
                File.AppendAllText(@"SCME.DatabseServer error.txt", string.Format("\n\n{0}\nEXCEPTION: {1}\nINNER EXCEPTION: {2}\n", DateTime.Now, ex, ex.InnerException ?? new Exception("No additional information - InnerException is null")));
            }
            catch { }
        }

        /// <summary>Возвращает слушающий порт</summary>
        /// <returns>При неудачном чтении значение равно -1</returns>
        public static int GetPort()
        {
            int Result = -1;
            //Служба не запущена
            if (MsServiceHost == null)
                return Result;
            try
            {
                if (MsServiceHost.ChannelDispatchers.Count == 1)
                    Result = MsServiceHost.ChannelDispatchers.FirstOrDefault().Listener.Uri.Port;
            }
            catch (Exception ex)
            {
                Journal.AppendLog("SystemHost", LogJournalMessageType.Error, string.Format("Error getting listening port from database service: {0}", ex.Message));
                Result = -1;
            }
            return Result;
        }

        /// <summary>Возвращает имя хоста слушающего порта</summary>
        /// <returns>При неудачном чтении значение равно пустой строке</returns>
        public static string GetHost()
        {
            string Result = string.Empty;
            //Служба не запущена
            if (MsServiceHost == null)
                return Result;
            try
            {
                if (MsServiceHost.ChannelDispatchers.Count == 1)
                    Result = MsServiceHost.ChannelDispatchers.FirstOrDefault().Listener.Uri.Host;
            }
            catch (Exception ex)
            {
                Journal.AppendLog("SystemHost", LogJournalMessageType.Error, string.Format("Error getting listening port from database service: {0}", ex.Message));
                Result = string.Empty;
            }
            return Result;
        }
    }
}
