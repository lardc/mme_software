using SCME.InterfaceImplementations.NewImplement.MSSQL;
using SCME.InterfaceImplementations.NewImplement.SQLite;
using SCME.Types;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseRepair
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
            var q = new SqlConnectionStringBuilder()
                {
                    DataSource = @"tcp:192.168.0.134, 1444",
                    InitialCatalog = @"SCME_ResultsDB",
                    IntegratedSecurity = false,
                    UserID = "sa",
                    Password = "йцу",
                    ConnectTimeout = 15
                };
            var  mssqlDbService = new MSSQLDbService(new  SqlConnection(q.ToString()));

            var msSqlDbServiceProxy = new MSSQLDbService(new SqlConnection(q.ToString()));

                var centralProfiles = msSqlDbServiceProxy.GetProfilesDeepByMmeCode("MME002");
            var centralProfiles1 = msSqlDbServiceProxy.GetProfilesDeepByMmeCode("MME002");

            
                foreach (var i in centralProfiles1)
                {
                    i.Version ++;
                    i.Timestamp = DateTime.Now;
                    i.Key = Guid.NewGuid();
                    i.Id = 0;
                }
                var w = centralProfiles.Zip(centralProfiles1, (m,n) => new {oldP = m, newP = n  }).ToList();

            int qwe = 0;

                foreach (var i in w)
                {
                    mssqlDbService.InsertUpdateProfile(i.oldP, i.newP, "MME002");
                qwe++;
                }
            }
            catch(Exception ex)
            {
                File.WriteAllText("error.txt", ex.ToString());
            }

        }
    }
}
