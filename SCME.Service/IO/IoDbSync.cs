using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SCME.InterfaceImplementations;
using SCME.InterfaceImplementations.NewImplement.SQLite;
using SCME.Service.Properties;
using SCME.Types;
using SCME.Types.Database;
using SCME.Types.DatabaseServer;
using SCME.Types.Interfaces;
using SCME.Types.Profiles;
using SCME.UIServiceConfig.Properties;

// ReSharper disable InvertIf

namespace SCME.Service.IO
{
    internal class IoDbSync
    {
        private readonly BroadcastCommunication _communication;
        private readonly MonitoringSender _monitoringSender;
        private string _mmeCode;
        private SQLiteDbService _sqLiteDbService;
        private IDbService _msSqlDbService;

        public IoDbSync(BroadcastCommunication communication, MonitoringSender monitoringSender)
        {
            _communication = communication;
            _monitoringSender = monitoringSender;
        }

        internal void Initialize(string databasePath, string databaseOptions, string mmeCode = null)
        {
            try
            {
                var connectionString = $"data source={databasePath};{databaseOptions}";
                _mmeCode = mmeCode;
                _sqLiteDbService = new SQLiteDbService(new SQLiteConnection(connectionString));
                _msSqlDbService = new DatabaseProxy(Settings.Default.CentralDatabase);
                _communication.PostDbSyncState(DeviceConnectionState.ConnectionInProcess, string.Empty);
//                    AfterSyncWithServerRoutineHandler("Synchronization of the local database with a central database is prohibited by parameter DisableResultDB");
//                else
//                    SyncWithServer(AfterSyncWithServerRoutineHandler);
            }
            catch (Exception e)
            {
                SystemHost.Journal.AppendLog(ComplexParts.Sync, LogMessageType.Error, $"Local database not synced with a central database. Reason: {e.ToString()}");
            }
        }

        public Task<InitializationResponse> SyncProfilesAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                var initializationResponse = new InitializationResponse()
                {
                    MmeCode = _mmeCode
                };

                try
                {
                    if (Settings.Default.IsLocal)
                        initializationResponse.SyncMode = SyncMode.Local;
                    else
                    {
                        _monitoringSender.Sync(SyncProfiles());
                        SendUnSendedResult();
                        initializationResponse.SyncMode = SyncMode.Sync;
                    }
                }
                catch (Exception e)
                {
                    initializationResponse.SyncMode = SyncMode.NotSync;
                    _communication.PostDbSyncState(DeviceConnectionState.ConnectionFailed, e.Message);
                    MessageBox.Show(e.ToString(), "Error sync, передайте сообщение разработчику");
                }

                _communication.PostDbSyncState(DeviceConnectionState.ConnectionSuccess, string.Empty);

                return initializationResponse;
            });
        }

        private int SyncProfiles()
        {
            try
            {
                var localProfiles = _sqLiteDbService.GetProfilesSuperficially(_mmeCode);
                var centralProfiles = _msSqlDbService.GetProfilesDeepByMmeCode(_mmeCode);
                if (!_sqLiteDbService.GetMmeCodes().ContainsKey(_mmeCode))
                    _sqLiteDbService.InsertMmeCode(_mmeCode);

                List<MyProfile> deletingProfiles;
                List<MyProfile> addingProfiles;

                if (_sqLiteDbService.Migrate())
                {
                    deletingProfiles = localProfiles;
                    addingProfiles = centralProfiles;
                }
                else
                {
                    deletingProfiles = localProfiles.Except(centralProfiles, new MyProfile.ProfileByVersionTimeEqualityComparer()).ToList();
                    addingProfiles = centralProfiles.Except(localProfiles, new MyProfile.ProfileByVersionTimeEqualityComparer()).ToList();
                }

                foreach (var i in deletingProfiles)
                    _sqLiteDbService.RemoveProfile(i, _mmeCode);

                foreach (var i in addingProfiles)
                {
                    //i.DeepData = _msSqlDbService.LoadProfileDeepData(i);
                    _sqLiteDbService.InsertUpdateProfile(null, i, _mmeCode);
                }

                return localProfiles.Count - deletingProfiles.Count + addingProfiles.Count;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error while syncing profiles from local database with a master: {0}", ex.ToString()));
            }
        }

        private void SendUnSendedResult()
        {
            using (var centralDbClient = new CentralDatabaseServiceClient(Settings.Default.CentralDatabaseService))
                foreach (var unSendedDevice in _sqLiteDbService.SqLiteResultsServiceLocal.GetUnsendedDevices())
                    if (centralDbClient.SendResultToServer(unSendedDevice))
                        _sqLiteDbService.SqLiteResultsServiceLocal.SetResultSended(unSendedDevice.Id);
        }

        public (MyProfile profile, bool IsInMmeCode) SyncProfile(MyProfile profile)
        {
            var (centralProfile, isInMmeCode) = _msSqlDbService.GetTopProfileByName(_mmeCode, profile.Name);
            
            if (!isInMmeCode)
                return (null, false);
            
            if (!new MyProfile.ProfileByVersionTimeEqualityComparer().Equals(profile, centralProfile))
            {
                centralProfile.DeepData = _msSqlDbService.LoadProfileDeepData(centralProfile);
                _sqLiteDbService.InsertUpdateProfile(profile, centralProfile, _mmeCode);
                return (centralProfile, true);
            }

            return (null, true);
        }


//        private void SyncWithServer(AfterSyncProfilesRoutine afterSyncProfilesRoutine)
//        {
//            //запоминаем что нам надо вызвать после того как будет выполнена синхронизация результатов измерений
//            _afterSyncProfilesRoutine = afterSyncProfilesRoutine;
//
//            //последовательно синхронизируем данные: сначала вызываем синхронизацию результатов измерений, которая после своего исполнения вызовет синхронизацию профилей
//            _syncService.SyncResults(AfterSyncResultsHandler);
//        }
//
//        private void AfterSyncResultsHandler(string error)
//        {
//            _syncService.SyncProfiles(_afterSyncProfilesRoutine);
//        }
//
//        private void AfterSyncWithServerRoutineHandler(string notSyncedReason)
//        {
//            //данная реализация будет вызвана после того, как будет пройдена стадия синхронизации данных SQLLite базы данных с данными центральной базы
//            if (notSyncedReason == string.Empty)
//            {
//                SystemHost.IsSyncedWithServer = true;
//                SystemHost.Journal.AppendLog(ComplexParts.Service, LogMessageType.Info, "Local database was successfully synced with a central database");
//                //Включим в процесс синхронизации загрузку профилей, UI пошлёт сообщение для ползунка об окончании синхронизации 
//                //_communication.PostDbSyncState(DeviceConnectionState.ConnectionSuccess, string.Empty);
//            }
//            else
//            {
//                SystemHost.IsSyncedWithServer = false;
//                LogMessageType logMessageType;
//
//                if (Settings.Default.IsLocal)
//                {
//                    logMessageType = LogMessageType.Info;
//                        //_communication.PostDbSyncState(DeviceConnectionState.ConnectionSuccess, string.Empty);
//                }
//                else
//                {
//                    logMessageType = LogMessageType.Error;
//                    _communication.PostDbSyncState(DeviceConnectionState.ConnectionFailed, "Sync error");
//                }
//
//                SystemHost.Journal.AppendLog(ComplexParts.Sync, logMessageType, $"Local database not synced with a central database. Reason: {notSyncedReason}");
//            }
//
//            FireSyncDbAreProcessedEvent();
//        }
//
//        private void FireSyncDbAreProcessedEvent()
//        {
//            var sCommunicationLive = string.Empty;
//            _communication.PostSyncDBAreProcessedEvent();
//
//
//            if (SystemHost.Journal != null)
//            {
//                var mess = "Sync DataBases are processed";
//
//                if (sCommunicationLive != string.Empty)
//                    mess = mess + ", " + sCommunicationLive;
//
//                SystemHost.Journal.AppendLog(ComplexParts.Sync, LogMessageType.Info, mess);
//            }
//        }
    }
}