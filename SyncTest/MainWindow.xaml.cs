using SCME.InterfaceImplementations.NewImplement.MSSQL;
using SCME.InterfaceImplementations.NewImplement.SQLite;
using SCME.Types.Profiles;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SyncTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string mme = File.ReadAllText("mme.txt");

                var mssqlDbService = new MSSQLDbService(new SqlConnection(File.ReadAllText("mssql.txt")));
                var sqliteDbService = new SQLiteDbService(new System.Data.SQLite.SQLiteConnection(File.ReadAllText("sqlite.txt")));

                var centralProfiles = mssqlDbService.GetProfilesSuperficially(mme);
                var localProfiles = sqliteDbService.GetProfilesDeepByMmeCode(mme);

                var deletingProfiles = localProfiles.Except(centralProfiles, new MyProfile.ProfileByVersionTimeEqualityComparer()).ToList();
                var addingProfiles = centralProfiles.Except(localProfiles, new MyProfile.ProfileByVersionTimeEqualityComparer()).ToList();


                try
                {
                    foreach (var i in deletingProfiles)
                        sqliteDbService.RemoveProfile(i, mme);

                    foreach (var i in addingProfiles)
                    {
                        i.DeepData = mssqlDbService.LoadProfileDeepData(i);
                        sqliteDbService.InsertUpdateProfile(null, i, mme);
                    }
                }
                catch (Exception ex)
                {
                    tb.Text = ($"Ошибка при синхронизации по времени версии имени {ex}");

                    try
                    {
                        deletingProfiles = localProfiles.Except(centralProfiles, new MyProfile.ProfileByKeyEqualityComparer()).ToList();
                        addingProfiles = centralProfiles.Except(localProfiles, new MyProfile.ProfileByKeyEqualityComparer()).ToList();

                        foreach (var i in deletingProfiles)
                            sqliteDbService.RemoveProfile(i, mme);

                        foreach (var i in addingProfiles)
                        {
                            i.DeepData = mssqlDbService.LoadProfileDeepData(i);
                            sqliteDbService.InsertUpdateProfile(null, i, mme);
                        }
                    }
                    catch (Exception ex1)
                    {
                        tb.Text = ($"Ошибка при синхронизации по ключу {ex1}");
                    }
                }

            }
            catch (Exception ex)
            {
                tb.Text = ex.ToString();
            }
        }
    }
}
