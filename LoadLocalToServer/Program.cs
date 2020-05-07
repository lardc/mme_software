using System;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Linq;
using SCME.InterfaceImplementations.NewImplement.MSSQL;
using SCME.InterfaceImplementations.NewImplement.SQLite;

namespace LoadLocalToServer
{
    class Program
    {
        static void Main(string[] args)
        {
            string mmeCode = "MME013";
            SQLiteDbService q = new SQLiteDbService(new SQLiteConnection(new SQLiteConnectionStringBuilder()
            {
                DataSource = "SCME_ResultsDB_Local.sqlite",
                DefaultTimeout = 15,
                SyncMode = SynchronizationModes.Full,
                JournalMode = SQLiteJournalModeEnum.Truncate,
                FailIfMissing = true
            }.ToString()));

            var w = new MSSQLDbService(new SqlConnection(new SqlConnectionStringBuilder()
            {
                DataSource = @"tcp:192.168.0.134, 1444",
                InitialCatalog = "SCME_ResultsDB",
                IntegratedSecurity = false,
                ConnectTimeout = 15,
                UserID = "sa",
                Password = "Hpl1520"
            }.ToString()));
            
            if(w.GetMmeCodes().Count(m=> m.Key == mmeCode) == 0)
            {
                w.InsertMmeCode(mmeCode);    
            }
            
            
            var e = q.GetProfilesDeepByMmeCode(mmeCode);
            foreach (var i in e)
            {
                w.InsertUpdateProfile(null, i, mmeCode);
            }
            e = null;
        }
    }
}