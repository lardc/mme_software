using SCME.InterfaceImplementations.NewImplement.MSSQL;
using SCME.Types;
using SCME.Types.Gate;
using SCME.Types.Profiles;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace BaseRepair
{
    static class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //ReSaveForGateItm();
                Profiles_Resave();
            }
            catch (Exception ex)
            {
                File.WriteAllText("error.txt", ex.ToString());
            }

        }

        private static void Profiles_Resave() //Пересоздание профилей
        {
            try
            {
                //Строка подключения к БД
                string ConnectionString = string.Format(@"Data Source=tcp:192.168.0.134, 1444; Initial Catalog=SCME_ResultsDB; User ID=sa; Password={0}", File.ReadAllText("password.txt"));
                //Инициализация подключения
                MSSQLDbService MsSqlDbService = new MSSQLDbService(new SqlConnection(ConnectionString));
                List<MyProfile> CentralProfiles = MsSqlDbService.GetProfilesDeepByMmeCode("MME007");
                List<MyProfile> NewCentralProfiles = new List<MyProfile>(CentralProfiles);
                //Обновление версий профилей
                foreach (MyProfile Profile in NewCentralProfiles)
                {
                    Profile.Version++;
                    Profile.Timestamp = DateTime.Now;
                    Profile.Key = Guid.NewGuid();
                    Profile.Id = 0;
                }
                List<(MyProfile oldP, MyProfile newP) > CortegeList = CentralProfiles.Zip(NewCentralProfiles, (oldProfile, newProfile) => (oldP: oldProfile, newP: newProfile)).ToList();
                foreach ((MyProfile oldP, MyProfile newP) in CortegeList)
                    MsSqlDbService.InsertUpdateProfile(oldP, newP, "MME007");
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
    }
}