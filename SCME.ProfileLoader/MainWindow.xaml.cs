using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Globalization;
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
using Microsoft.Win32;
using SCME.InterfaceImplementations.NewImplement.SQLite;
using SCME.Types;
using SCME.Types.Database;
using SCME.Types.Profiles;
using SQLitePCL;

namespace SCME.ProfileLoader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindowVm Vm { get; set; } = new MainWindowVm();
        private Properties Settings => Properties.Default;

        private SqlConnectionStringBuilder GetSqlConnectionStringBuilder =>
            new SqlConnectionStringBuilder()
            {
                DataSource = Settings.MSSQLServer,
                InitialCatalog = Settings.MSSQLDatabase,
                IntegratedSecurity = Settings.MSSQLIntegratedSecurity,
                ConnectTimeout = Settings.SQLTimeout,
                UserID = Settings.MSSQLIntegratedSecurity == false ? Settings.MSSQLUserId : null,
                Password = Settings.MSSQLIntegratedSecurity == false ? Settings.MSSQLPassword : null
            };

        private SQLiteConnectionStringBuilder GetSQLiteConnectionStringBuilder =>
            new SQLiteConnectionStringBuilder()
            {
                DataSource = Settings.SQLiteFileName,
                DefaultTimeout = Settings.SQLTimeout,
                SyncMode = SynchronizationModes.Full,
                JournalMode = SQLiteJournalModeEnum.Truncate,
                FailIfMissing = true
            };

        private IDbService GetMsSqlDbService()
        {
            var connectionStringBuilder = GetSqlConnectionStringBuilder;
          
            var sqlConnection = new SqlConnection(connectionStringBuilder.ToString());
            var service = new InterfaceImplementations.NewImplement.MSSQL.MSSQLDbService(sqlConnection);
            service.Migrate();
            return service;
        }

        private IDbService GetSqliteDbService()
        {
            var connectionStringBuilder = GetSQLiteConnectionStringBuilder;

            var sqliteConnection = new SQLiteConnection(connectionStringBuilder.ToString());
            var service = new InterfaceImplementations.NewImplement.SQLite.SQLiteDbService(sqliteConnection);
            service.Migrate();
            return service;
        }

        private IDbService _dbService;
        
        public MainWindow()
        {
            InitializeComponent();
            _dbService = Settings.TypeDb switch
            {
                TypeDb.SQLite => GetSqliteDbService(),
                TypeDb.MSSQL => GetMsSqlDbService(),
                _ => throw new Exception()
            };
            Vm.MmeCodes = new ObservableCollection<string>(_dbService.GetMmeCodes().Select(m => m.Key).Where(m=> m != "IsActive"));
        }

        private double Parse(string value)
        {
            return double.Parse(value.Replace('.', ','));
        }
        
        private bool ParseBool(string value)
        {
            return value != "0";
        }

        private void AddOutputLeackingCurrent(double[] values, ref int n, int numberPosition, MyProfile profile)
        {
            if (double.IsNegativeInfinity(values[n + 6]))
            {
                n += 15;
                return;
            }

            var parameter = new Types.OutputLeakageCurrent.TestParameters {NumberPosition = numberPosition};
            if (!double.IsNegativeInfinity(values[n]))
            {
                parameter.TypeManagement = TypeManagement.DCVoltage;
                parameter.ControlVoltage = values[n++];
                parameter.ControlCurrentMaximum = values[n++];
                n += 4;
            }
            else if (!double.IsNegativeInfinity(values[n + 2]))
            {
                n += 2;
                parameter.TypeManagement = TypeManagement.ACVoltage;
                parameter.ControlVoltage = values[n++];
                parameter.ControlCurrentMaximum = values[n++];
                n += 2;
            }
            else
            {
                n += 4;
                parameter.TypeManagement = TypeManagement.DCAmperage;
                parameter.ControlCurrent = values[n++];
                parameter.ControlVoltageMaximum = values[n++];
            }

            if (!double.IsNegativeInfinity(values[n]))
            {
                parameter.ApplicationPolarityConstantSwitchingVoltage = ApplicationPolarityConstantSwitchingVoltage.DCVoltage;
                parameter.SwitchedVoltage = values[n++];
                n++;
            }
            else
            {
                n++;
                ;
                parameter.ApplicationPolarityConstantSwitchingVoltage = ApplicationPolarityConstantSwitchingVoltage.ACVoltage;
                parameter.SwitchedVoltage = values[n++];
            }

            if (!double.IsNegativeInfinity(values[n++]))
                parameter.AuxiliaryVoltagePowerSupply1 = values[n - 1];

            if (!double.IsNegativeInfinity(values[n++]))
                parameter.AuxiliaryCurrentPowerSupplyMaximum1 = values[n - 1];

            if (!double.IsNegativeInfinity(values[n++]))
                parameter.AuxiliaryVoltagePowerSupply2 = values[n - 1];

            if (!double.IsNegativeInfinity(values[n++]))
                parameter.AuxiliaryCurrentPowerSupplyMaximum2 = values[n - 1];

            parameter.LeakageCurrentMinimum = values[n++];
            parameter.LeakageCurrentMaximum = values[n++];
            n++;
            profile.DeepData.TestParametersAndNormatives.Add(parameter);
        }

        private void AddOutputResidualVoltage(double[] values, ref int n, int numberPosition, MyProfile profile)
        {
            if (double.IsNegativeInfinity(values[n + 6]))
            {
                n += 17;
                return;
            }

            var parameter = new Types.OutputResidualVoltage.TestParameters {NumberPosition = numberPosition};
            if (!double.IsNegativeInfinity(values[n]))
            {
                parameter.TypeManagement = TypeManagement.DCVoltage;
                parameter.ControlVoltage = values[n++];
                parameter.ControlCurrentMaximum = values[n++];
                n += 4;
            }
            else if (!double.IsNegativeInfinity(values[n + 2]))
            {
                n += 2;
                parameter.TypeManagement = TypeManagement.ACVoltage;
                parameter.ControlVoltage = values[n++];
                parameter.ControlCurrentMaximum = values[n++];
                n += 2;
            }
            else
            {
                n += 4;
                parameter.TypeManagement = TypeManagement.DCAmperage;
                parameter.ControlCurrent = values[n++];
                parameter.ControlVoltageMaximum = values[n++];
            }

            parameter.OpenState = !double.IsNegativeInfinity(values[n++]) || Math.Abs(values[n - 1]) < double.Epsilon;
            parameter.SwitchedAmperage = values[n++];
            
            if (!double.IsNegativeInfinity(values[n++]))
                parameter.AuxiliaryVoltagePowerSupply1 = values[n - 1];

            if (!double.IsNegativeInfinity(values[n++]))
                parameter.AuxiliaryCurrentPowerSupplyMaximum1 = values[n - 1];

            if (!double.IsNegativeInfinity(values[n++]))
                parameter.AuxiliaryVoltagePowerSupply2 = values[n - 1];

            if (!double.IsNegativeInfinity(values[n++]))
                parameter.AuxiliaryCurrentPowerSupplyMaximum2 = values[n - 1];

            if (!double.IsNegativeInfinity(values[n++]))
                parameter.OpenResistanceMinimum = values[n - 1];
            
            if (!double.IsNegativeInfinity(values[n++]))
                parameter.OpenResistanceMaximum = values[n - 1];
                
            parameter.OutputResidualVoltageMinimum = values[n++];
            parameter.OutputResidualVoltageMaximum = values[n++];
            n++;
            profile.DeepData.TestParametersAndNormatives.Add(parameter);
        }

        private void AddInputOptions(double[] values, ref int n, int numberPosition, MyProfile profile)
        {
            if (double.IsNegativeInfinity(values[n + 6]))
            {
                n += 12;
                return;
            }

            var parameter = new Types.InputOptions.TestParameters {NumberPosition = numberPosition};
            if (!double.IsNegativeInfinity(values[n]))
            {
                parameter.TypeManagement = TypeManagement.DCVoltage;
                parameter.ControlVoltage = values[n++];
                n += 2;
            }
            else if (!double.IsNegativeInfinity(values[n + 1]))
            {
                n ++;
                parameter.TypeManagement = TypeManagement.ACVoltage;
                parameter.ControlVoltage = values[n++];
                n ++;
            }
            else
            {
                n += 2;
                parameter.TypeManagement = TypeManagement.DCAmperage;
                parameter.ControlCurrent = values[n++];
            }

            if (!double.IsNegativeInfinity(values[n++]))
                parameter.AuxiliaryVoltagePowerSupply1 = values[n - 1];

            if (!double.IsNegativeInfinity(values[n++]))
                parameter.AuxiliaryCurrentPowerSupplyMaximum1 = values[n - 1];

            if (!double.IsNegativeInfinity(values[n++]))
                parameter.AuxiliaryVoltagePowerSupply2 = values[n - 1];

            if (!double.IsNegativeInfinity(values[n++]))
                parameter.AuxiliaryCurrentPowerSupplyMaximum2 = values[n - 1];

            if (!double.IsNegativeInfinity(values[n++]))
                parameter.InputCurrentMinimum = values[n - 1];
            
            if (!double.IsNegativeInfinity(values[n++]))
                parameter.InputCurrentMaximum = values[n - 1];
            
            if (!double.IsNegativeInfinity(values[n++]))
                parameter.InputVoltageMinimum = values[n - 1];
            
            if (!double.IsNegativeInfinity(values[n++]))
                parameter.InputVoltageMaximum = values[n - 1];

            n++;
            profile.DeepData.TestParametersAndNormatives.Add(parameter);
        }

        private void AddAuxiliaryPower(double[] values, ref int n, int numberPosition, MyProfile profile)
        {
            if (double.IsNegativeInfinity(values[n]))
                return;

            var parameter = new Types.AuxiliaryPower.TestParameters {NumberPosition = numberPosition};
         

            if (!double.IsNegativeInfinity(values[n++]))
                parameter.AuxiliaryVoltagePowerSupply1 = values[n - 1];
            
            if (!double.IsNegativeInfinity(values[n++]))
                parameter.AuxiliaryVoltagePowerSupply2 = values[n - 1];
            
            if (!double.IsNegativeInfinity(values[n++]))
                parameter.AuxiliaryCurrentPowerSupplyMinimum1 = values[n - 1];
            
            if (!double.IsNegativeInfinity(values[n++]))
                parameter.AuxiliaryCurrentPowerSupplyMaximum1 = values[n - 1];

            if (!double.IsNegativeInfinity(values[n++]))
                parameter.AuxiliaryCurrentPowerSupplyMinimum2 = values[n - 1];
            
            if (!double.IsNegativeInfinity(values[n++]))
                parameter.AuxiliaryCurrentPowerSupplyMaximum2 = values[n - 1];
        }
        
        private void SelectExcelFile_OnClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender ;
            var mme = button.Content as string;
            var dialog = new OpenFileDialog()
            {
                Filter = "Excel file (*.csv)|*.csv"

            };
            dialog.ShowDialog();
            if (string.IsNullOrEmpty(dialog.FileName) || File.Exists(dialog.FileName) == false)
                return;
            //dialog.FileName = $@"D:\Profile_Batch_Loader_Template_101 - Лист1.csv";
          

            var delimeter = Vm.CommaIsDelimiter ? ',' : ';';
            var rawData = File.ReadLines(dialog.FileName).ToList();
            rawData.RemoveRange(0, 3);
            foreach(var i in rawData)
            {
                int n = 0;
                var splitData = i.Split(delimeter);
                var q = splitData.ToList().GetRange(2, splitData.Count() - 2).ToList();
                var values = splitData.ToList().GetRange(2, splitData.Count() - 2).Select(m=> string.IsNullOrEmpty(m) ? double.NegativeInfinity : Parse(m)).ToArray();
                var profile = new MyProfile(1, splitData[n+1], Guid.NewGuid(), 1, DateTime.Now);
                profile.DeepData = new ProfileDeepData();
                profile.DeepData.TestParametersAndNormatives = new ObservableCollection<Types.BaseTestParams.BaseTestParametersAndNormatives>();

                for(var j = 0; j < 3; j++)
                {
                    for (var t = 0; t < 3; t++)
                        AddOutputLeackingCurrent(values, ref n, j + 1, profile);
                    for (var t = 0; t < 2; t++)
                        AddOutputResidualVoltage(values, ref n, j + 1, profile);
                    for (var t = 0; t < 4; t++)
                        AddInputOptions(values, ref n, j + 1, profile);
                    n++;
                }
                
                AddAuxiliaryPower(values, ref n, 1, profile);

                _dbService.InsertUpdateProfile(null, profile, mme);
            }
            
                

            
        }
    }
}
