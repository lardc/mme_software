using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using SCME.Interfaces;
using SCME.Types;
using SCME.Types.DatabaseServer;

namespace SCME.InterfaceImplementations
{
    public class SQLiteSyncService : ISyncService
    {
        private readonly string _mmeCode;
        private readonly SQLiteConnection _connection;
        private readonly IProfilesService _saveProfileService;
        private readonly IResultsService _resultsService;

        public SQLiteSyncService(string databasePath, string mmeCode)
        {
            _mmeCode = mmeCode;
            _connection = new SQLiteConnection(databasePath, false);
            _connection.Open();
            _saveProfileService = new SQLiteProfilesService(databasePath);
            _resultsService = new SQLiteResultsServiceLocal(databasePath);
        }

        public void SyncResults()
        {
            var unsendedDevices = _resultsService.GetUnsendedDevices();
            using (var centralDbClient = new CentralDatabaseServiceClient())
            {
                foreach (var unsendedDevice in unsendedDevices)
                {
                    var sended = centralDbClient.SendResultToServer(unsendedDevice);
                    if (sended)
                        _resultsService.SetResultSended(unsendedDevice.Id);
                }
            }
        }

        public void SyncProfiles()
        {
            using (var centralDbClient = new CentralDatabaseServiceClient())
            {
                var serverProfiles = centralDbClient.GetProfileItemsByMme(_mmeCode);
                SaveProfiles(serverProfiles);
            }

        }

        public void SyncProfiles(ICentralDatabaseService centralDatabaseService)
        {
            var serverProfiles = centralDatabaseService.GetProfileItemsByMme(_mmeCode);
            SaveProfiles(serverProfiles);
        }

        private void SaveProfiles(List<ProfileItem> serverProfiles)
        {
            _saveProfileService.SaveProfilesFromMme(serverProfiles, _mmeCode);
        }

        public void Dispose()
        {
            if (_connection != null && _connection.State == ConnectionState.Open)
                _connection.Close();
        }
    }
}
