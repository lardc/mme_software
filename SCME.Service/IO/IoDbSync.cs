using System;
using SCME.InterfaceImplementations;
using SCME.Service.Properties;
using SCME.Types;
using SCME.Types.Interfaces;

// ReSharper disable InvertIf

namespace SCME.Service.IO
{
    internal class IoDbSync
    {
        private readonly BroadcastCommunication _communication;
        private ISyncService _syncService;
        private AfterSyncProfilesRoutine _afterSyncProfilesRoutine;

        public IoDbSync(BroadcastCommunication communication)
        {
            _communication = communication;
        }

        internal void Initialize(string databasePath, string databaseOptions, string mmeCode = null)
        {
            try
            {
                var connectionString = $"data source={databasePath};{databaseOptions}";
                _syncService = new SyncService(connectionString, mmeCode);
                _communication.PostDbSyncState(DeviceConnectionState.ConnectionInProcess, string.Empty);
                if (Settings.Default.IsLocal)
                    AfterSyncWithServerRoutineHandler("Synchronization of the local database with a central database is prohibited by parameter DisableResultDB");
                else
                    SyncWithServer(AfterSyncWithServerRoutineHandler);
            }
            catch (Exception e)
            {
                SystemHost.Journal.AppendLog(ComplexParts.Sync, LogMessageType.Error, $"Local database not synced with a central database. Reason: {e.ToString()}");
            }
        }

        private void SyncWithServer(AfterSyncProfilesRoutine afterSyncProfilesRoutine)
        {
            //запоминаем что нам надо вызвать после того как будет выполнена синхронизация результатов измерений
            _afterSyncProfilesRoutine = afterSyncProfilesRoutine;

            //последовательно синхронизируем данные: сначала вызываем синхронизацию результатов измерений, которая после своего исполнения вызовет синхронизацию профилей
            _syncService.SyncResults(AfterSyncResultsHandler);
        }

        private void AfterSyncResultsHandler(string error)
        {
            _syncService.SyncProfiles(_afterSyncProfilesRoutine);
        }

        private void AfterSyncWithServerRoutineHandler(string notSyncedReason)
        {
            //данная реализация будет вызвана после того, как будет пройдена стадия синхронизации данных SQLLite базы данных с данными центральной базы
            if (notSyncedReason == string.Empty)
            {
                SystemHost.IsSyncedWithServer = true;
                SystemHost.Journal.AppendLog(ComplexParts.Service, LogMessageType.Info, "Local database was successfully synced with a central database");
                _communication.PostDbSyncState(DeviceConnectionState.ConnectionSuccess, string.Empty);
            }
            else
            {
                SystemHost.IsSyncedWithServer = false;
                LogMessageType logMessageType;

                if (Settings.Default.IsLocal)
                {
                    logMessageType = LogMessageType.Info;
                    _communication.PostDbSyncState(DeviceConnectionState.ConnectionSuccess, string.Empty);
                }
                else
                {
                    logMessageType = LogMessageType.Error;
                    _communication.PostDbSyncState(DeviceConnectionState.ConnectionFailed, "Sync error");
                }

                SystemHost.Journal.AppendLog(ComplexParts.Sync, logMessageType, $"Local database not synced with a central database. Reason: {notSyncedReason}");
            }

            FireSyncDbAreProcessedEvent();
        }

        private void FireSyncDbAreProcessedEvent()
        {
            var sCommunicationLive = string.Empty;
            _communication.PostSyncDBAreProcessedEvent();


            if (SystemHost.Journal != null)
            {
                var mess = "Sync DataBases are processed";

                if (sCommunicationLive != string.Empty)
                    mess = mess + ", " + sCommunicationLive;

                SystemHost.Journal.AppendLog(ComplexParts.Sync, LogMessageType.Info, mess);
            }
        }
    }
}