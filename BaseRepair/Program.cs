using SCME.InterfaceImplementations.NewImplement.MSSQL;
using SCME.Types;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using SCME.Types.Gate;

namespace BaseRepair
{
    class Program
    {
        public static void ReSaveProfiles()
        {
            try
            {
                var q = new SqlConnectionStringBuilder()
                {
                    DataSource = @"tcp:192.168.0.134, 1444",
                    InitialCatalog = @"SCME_ResultsDB",
                    IntegratedSecurity = false,
                    UserID = "sa",
                    Password = File.ReadAllText("password.txt"),
                    ConnectTimeout = 15
                };
                var mssqlDbService = new MSSQLDbService(new SqlConnection(q.ToString()));

                var msSqlDbServiceProxy = new MSSQLDbService(new SqlConnection(q.ToString()));

                var centralProfiles = msSqlDbServiceProxy.GetProfilesDeepByMmeCode("MME007");
                var centralProfiles1 = msSqlDbServiceProxy.GetProfilesDeepByMmeCode("MME007");


                foreach (var i in centralProfiles1)
                {
                    i.Version++;
                    i.Timestamp = DateTime.Now;
                    i.Key = Guid.NewGuid();
                    i.Id = 0;
                }

                var w = centralProfiles.Zip(centralProfiles1, (m, n) => new {oldP = m, newP = n}).ToList();

                int qwe = 0;

                foreach (var i in w)
                {
                    mssqlDbService.InsertUpdateProfile(i.oldP, i.newP, "MME007");
                    qwe++;
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText("error.txt", ex.ToString());
            }
        }

        private static void ReSaveForGateItm()
        {
            var mssqlDbService = new MSSQLDbService(new SqlConnection(File.ReadAllText("ConnectionString.txt")));
            mssqlDbService.Migrate();
            var mmeCodes = mssqlDbService.GetMmeCodes().Where(m => m.Key != Constants.MME_CODE_IS_ACTIVE_NAME);
            foreach (var i in mmeCodes)
            {
                Console.WriteLine($"Начата обработка mme: {i.Key}");
                int countUpdates = 0;
                foreach (var j in mssqlDbService.GetProfilesDeepByMmeCode(i.Key).ToArray())
                {
                    var needReSave = false;
                    foreach (var t in j.DeepData.TestParametersAndNormatives.OfType<TestParameters>())
                    {
                        if (!t.IsIhEnabled || t.Itm != 0) continue;

                        needReSave = true;
                        t.Itm = 500;
                    }

                    if (!needReSave) continue;

                    var copy = j.Copy();
                    copy.Version++;
                    copy.Timestamp = DateTime.Now;
                    copy.Key = Guid.NewGuid();
                    copy.Id = 0;
                    mssqlDbService.InsertUpdateProfile(j, copy, i.Key);
                    countUpdates++;
                }

                Console.WriteLine($"Количество обновлённых профилей {countUpdates}");
                Console.WriteLine($"Закончена обработка mme: {i.Key}");
            }
        }

        private static void Main(string[] args)
        {
            try
            {
                //ReSaveForGateItm();
                ReSaveProfiles();
            }
            catch (Exception ex)
            {
                File.WriteAllText("error.txt", ex.ToString());
            }
        }
    }
}