using System;
using System.Globalization;
using System.IO;
using System.ServiceModel;
using SCME.Logger;
using SCME.Service.Properties;
using SCME.Types;

namespace SCME.Service
{
    internal static class SystemHost
    {
        private static ExternalControlServer ms_ControlService;
        private static ServiceHost ms_ControlServiceHost;
        private static ServiceHost ms_DatabaseServiceHost;
        private static ServiceHost ms_MaintenanceServiceHost;

        internal static bool Initialize()
        {
            try
            {
                Journal = new EventJournal();
                Journal.Open(Settings.Default.DisableLogDB ? String.Empty : Settings.Default.LogsDatabasePath, Settings.Default.DBOptionsLogs,
                                String.Format(Settings.Default.LogsTracePathTemplate, DateTime.Now.ToString(CultureInfo.CurrentCulture).Replace('/', '_').Replace(':', '_')),
                                Settings.Default.ForceLogFlush,
                                Settings.Default.IncludeDetailsInLog);
                Journal.AppendLog(ComplexParts.Service, LogMessageType.Info, Resources.Log_SystemHost_Application_started);
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"SCME.Service error.txt",
                    String.Format("\n\n{0}\nEXCEPTION: {1}\nINNER EXCEPTION: {2}\n", DateTime.Now, ex,
                        ex.InnerException ??
                        new Exception("No additional information - InnerException is null")));

                return false;
            }

            try
            {
                Results = new ResultsJournal();
                Results.Open(Settings.Default.DisableResultDB ? String.Empty : Settings.Default.ResultsDatabasePath, Settings.Default.DBOptionsResults, Settings.Default.MMECode);
                if (!Settings.Default.DisableResultDB)
                    Journal.AppendLog(ComplexParts.Service, LogMessageType.Info, Resources.Log_SystemHost_Result_journal_opened);
                try
                {
                    Results.SyncWithServer();
                }
                catch (Exception ex)
                {
                    Journal.AppendLog(ComplexParts.Service, LogMessageType.Error, ex.Message);
                }


                ms_ControlService = new ExternalControlServer();
                ms_ControlServiceHost = new ServiceHost(ms_ControlService);
                ms_ControlServiceHost.Open();
                Journal.AppendLog(ComplexParts.Service, LogMessageType.Info, String.Format(Resources.Log_SystemHost_Control_service_is_listening));

                ms_DatabaseServiceHost = new ServiceHost(typeof(DatabaseServer));
                try
                {
                    ms_DatabaseServiceHost.AddServiceEndpoint(typeof(IDatabaseCommunicationService),
                        new NetTcpBinding("DefaultTcpBinding"), Settings.Default.DBServiceExternalEndpoint);
                }
                catch (Exception ex)
                {
                    Journal.AppendLog(ComplexParts.Service, LogMessageType.Warning, String.Format("Can't open external database service port: {0}", ex.Message));
                }
                ms_DatabaseServiceHost.Open();
                Journal.AppendLog(ComplexParts.Service, LogMessageType.Info, String.Format(Resources.Log_SystemHost_Database_service_is_listening));

                ms_MaintenanceServiceHost = new ServiceHost(typeof(MaintenanceServer));
                try
                {
                    ms_MaintenanceServiceHost.AddServiceEndpoint(typeof(IDatabaseMaintenanceService),
                        new NetTcpBinding("DefaultTcpBinding"), Settings.Default.MaintenanceServiceExternalEndpoint);
                }
                catch (Exception ex)
                {
                    Journal.AppendLog(ComplexParts.Service, LogMessageType.Warning, String.Format("Can't open external maintenance service port: {0}", ex.Message));
                }
                ms_MaintenanceServiceHost.Open();
                Journal.AppendLog(ComplexParts.Service, LogMessageType.Info, String.Format(Resources.Log_SystemHost_Maintenance_service_is_listening));

                return true;
            }
            catch (Exception ex)
            {
                Journal.AppendLog(ComplexParts.Service, LogMessageType.Error, ex.Message);
                Journal.Close();

                return false;
            }
        }

        internal static EventJournal Journal { get; private set; }
        internal static ResultsJournal Results { get; private set; }

        internal static void Close()
        {
            try
            {
                if (ms_ControlServiceHost != null)
                {
                    try
                    {
                        (ms_ControlService as IExternalControl).Deinitialize();
                    }
                    finally
                    {
                        if (ms_ControlServiceHost.State == CommunicationState.Faulted)
                            ms_ControlServiceHost.Abort();
                        else
                            ms_ControlServiceHost.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                if (Journal != null)
                    Journal.AppendLog(ComplexParts.Service, LogMessageType.Error,
                                        String.Format(Resources.Log_SystemHost_Error_while_closing, @"Control host", ex.Message));
            }

            try
            {
                if (ms_DatabaseServiceHost != null)
                    if (ms_DatabaseServiceHost.State == CommunicationState.Faulted)
                        ms_DatabaseServiceHost.Abort();
                    else
                        ms_DatabaseServiceHost.Close();
            }
            catch (Exception ex)
            {
                if (Journal != null)
                    Journal.AppendLog(ComplexParts.Service, LogMessageType.Error,
                                        String.Format(Resources.Log_SystemHost_Error_while_closing, @"DB host", ex.Message));
            }

            try
            {
                if (ms_MaintenanceServiceHost != null)
                    if (ms_MaintenanceServiceHost.State == CommunicationState.Faulted)
                        ms_MaintenanceServiceHost.Abort();
                    else
                        ms_MaintenanceServiceHost.Close();
            }
            catch (Exception ex)
            {
                if (Journal != null)
                    Journal.AppendLog(ComplexParts.Service, LogMessageType.Error,
                                        String.Format(Resources.Log_SystemHost_Error_while_closing, @"Maintenance host", ex.Message));
            }

            try
            {
                Results.Close();
            }
            catch (Exception ex)
            {
                if (Journal != null)
                    Journal.AppendLog(ComplexParts.Database, LogMessageType.Error,
                                        String.Format(Resources.Log_SystemHost_Error_while_closing, @"Result journal", ex.Message));
            }

            if (Journal != null)
            {
                Journal.AppendLog(ComplexParts.Service, LogMessageType.Info, Resources.Log_SystemHost_Application_closed);
                Journal.Close();
            }
        }
    }
}