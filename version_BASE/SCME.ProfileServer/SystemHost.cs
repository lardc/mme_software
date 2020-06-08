using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;
using SCME.ProfileServer.Properties;
using SCME.Types.Profiles;
using SCME.Types.Utils;

namespace SCME.ProfileServer
{
    internal static class SystemHost
    {
        private static ServiceHost ms_ServiceHost;

        internal static LogJournal Journal { get; private set; }

        internal static ProfileDictionary Dictionary { get; private set; }

        internal static ConfigurationList ConfigList { get; private set; }

        internal static void StartService()
        {
            InitializeInternal();

            try
            {
                ms_ServiceHost = new ServiceHost(typeof(ProfileServer));
                ms_ServiceHost.Open();
            }
            catch (Exception ex)
            {
                Journal.AppendLog("SystemHost", LogJournalMessageType.Error,
                    String.Format("Error starting profile service: {0}", ex.Message));
                return;
            }

            Journal.AppendLog("SystemHost", LogJournalMessageType.Info,
                String.Format("SCME profile service started on {0}",
                    ms_ServiceHost.BaseAddresses.FirstOrDefault()));
        }

        internal static void StopService()
        {
            try
            {
                if (ms_ServiceHost != null)
                {
                    if(ms_ServiceHost.State != CommunicationState.Opened)
                        ms_ServiceHost.Abort();
                    else
                        ms_ServiceHost.Close();

                    ms_ServiceHost = null;
                }
            }
            catch (Exception ex)
            {
                Journal.AppendLog("SystemHost", LogJournalMessageType.Warning,
                    String.Format("SCME profile service can not be stopped properly: {0}", ex.Message));
            }

            ShutdownInternal();
        }

        private static void InitializeInternal()
        {
            Journal = new LogJournal();

            try
            {
                var path = String.Format(Settings.Default.LogPathTemplate,
                    DateTime.Now.ToString(CultureInfo.CurrentCulture).Replace('/', '_').Replace(':', '_'));

                if (!Path.IsPathRooted(path))
                    path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);

                Journal.Open(path, true, true);
            }
            catch (Exception ex)
            {
                LogCriticalErrorMessage(ex);
                return;
            } 
            
            try
            {
                var profileDictionaryPath = Settings.Default.ProfileDictionaryPath;
                if (!Path.IsPathRooted(profileDictionaryPath))
                    profileDictionaryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, profileDictionaryPath);

                Dictionary = new ProfileDictionary(profileDictionaryPath);            
            }
            catch (Exception ex)
            {
                Dictionary = null;
                Journal.AppendLog("ProfileDictionary", LogJournalMessageType.Error, ex.Message);
            }

            try
            {
                var configurationListPath = Settings.Default.ConfigurationListPath;
                if (!Path.IsPathRooted(configurationListPath))
                    configurationListPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configurationListPath);

                ConfigList = new ConfigurationList(configurationListPath);
            }
            catch (Exception ex)
            {
                ConfigList = null;
                Journal.AppendLog("ConfigList", LogJournalMessageType.Error, ex.Message);
            }
        }

        private static void ShutdownInternal()
        {
            try
            {
                if (Dictionary != null)
                    Dictionary.SaveToXml();
            }
            catch (Exception ex)
            {
                Journal.AppendLog("ProfileDictionary", LogJournalMessageType.Error, ex.Message);
            }

            try
            {
                if (ConfigList != null)
                    ConfigList.Save();
            }
            catch (Exception ex)
            {
                Journal.AppendLog("ConfigList", LogJournalMessageType.Error, ex.Message);
            }

            if (Journal != null)
            {
                Journal.AppendLog("SystemHost", LogJournalMessageType.Info, "SCME profile service stopped");
                Journal.Close();
                Journal = null;
            }            
        }

        internal static void LogCriticalErrorMessage(Exception Ex)
        {
            try
            {
                File.AppendAllText(@"SCME.ProfileService error.txt",
                    String.Format("\n\n{0}\nEXCEPTION: {1}\nINNER EXCEPTION: {2}\n", DateTime.Now, Ex,
                        Ex.InnerException ??
                        new Exception("No additional information - InnerException is null")));
            }
            catch(Exception ex)
            {
                Trace.TraceError(ex.ToString());
            }
        }
    }
}
