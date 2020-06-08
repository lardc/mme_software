using SCME.Types;
using SCME.Types.Profiles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SCME.dbViewer
{
    /// <summary>
    /// Interaction logic for ManualInputParams.xaml
    /// </summary>
    public partial class ManualInputParams : Window, INotifyPropertyChanged
    {
        public ManualInputParams()
        {
            InitializeComponent();

            this.Owner = Application.Current.MainWindow;


            mnuCreate.Visibility = Routines.IsUserCanCreateParameter(((MainWindow)this.Owner).FPermissionsLo) ? Visibility.Visible : Visibility.Collapsed;
            mnuEdit.Visibility = Routines.IsUserCanEditParameter(((MainWindow)this.Owner).FPermissionsLo) ? Visibility.Visible : Visibility.Collapsed;
            mnuDelete.Visibility = Routines.IsUserCanDeleteParameter(((MainWindow)this.Owner).FPermissionsLo) ? Visibility.Visible : Visibility.Collapsed;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public DataView ManualInputParamsList
        {
            get
            {
                DataTable dt = new DataTable();
                SqlConnection connection = DBConnections.Connection;
                connection.Open();

                try
                {
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    string sqlText = "SELECT MANUALINPUTPARAMID, NAME, TEMPERATURECONDITION, UM, DESCREN, DESCRRU" +
                                     " FROM MANUALINPUTPARAMS WITH(NOLOCK)";
                    adapter.SelectCommand = new SqlCommand(sqlText, connection);
                    adapter.Fill(dt);
                }

                finally
                {
                    connection.Close();
                }

                return dt.DefaultView;
            }
        }

        public bool? ShowModal(out int manualInputParamID)
        {
            //демонстрация списка параметров, котрые пользователи создали вручную
            //возврат True - в out парамеметре manualInputParamID идентификатор выбранного параметра;
            //возврат False - в out парамеметре manualInputParamID=-1
            bool? result = this.ShowDialog();

            if (result ?? false)
            {
                //пользователь выбрал параметр из списка
                manualInputParamID = Convert.ToInt32(this.dgManualInputParams.ValueFromSelectedRow("MANUALINPUTPARAMID"));
            }
            else
            {
                //пользователь не выбран параметр
                manualInputParamID = -1;
            }

            return result;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.DialogResult = false;
        }

        private void btOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void dgManualInputParams_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.DialogResult = true;
        }

        private void mnuCreateClick(object sender, RoutedEventArgs e)
        {
            if (Routines.IsUserCanCreateParameter(((MainWindow)this.Owner).FPermissionsLo))
            {
                ManualInputParamEditor manualInputParamEditor = new ManualInputParamEditor();

                if (manualInputParamEditor.ShowModal(null, string.Empty, TemperatureCondition.RT, string.Empty, string.Empty, string.Empty) ?? false)
                {
                    //пользователь выполнил сохранение параметра
                    OnPropertyChanged();
                }
            }
        }

        private int SelectedManualInputParamID()
        {
            //считывает из выбранной пользователем записи идентификатор параметра
            //возвращает:
            //-1 - пользователь не выбрал запись в dgManualInputParams
            //иначе - идентификтор параметра
            int manualInputParamID = 0;

            //если ячейка в dgManualInputParams выбрана - считываем идентификатор параметра
            if (this.dgManualInputParams.CurrentCell.IsValid)
            {
                manualInputParamID = Convert.ToInt32(this.dgManualInputParams.ValueFromSelectedRow("MANUALINPUTPARAMID"));
            }

            return (manualInputParamID == 0) ? -1 : manualInputParamID;
        }

        private void mnuEditClick(object sender, RoutedEventArgs e)
        {
            if (Routines.IsUserCanEditParameter(((MainWindow)this.Owner).FPermissionsLo))
            {
                int manualInputParamID = this.SelectedManualInputParamID();

                if (manualInputParamID == -1)
                {
                    MessageBox.Show(Properties.Resources.NoEditingObjectSelected, System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                string name = this.dgManualInputParams.ValueFromSelectedRow("NAME").ToString();
                TemperatureCondition temperatureCondition = (TemperatureCondition)Enum.Parse(typeof(TemperatureCondition), this.dgManualInputParams.ValueFromSelectedRow("TEMPERATURECONDITION").ToString());
                string um = this.dgManualInputParams.ValueFromSelectedRow("UM").ToString();
                string descrEN = this.dgManualInputParams.ValueFromSelectedRow("DESCREN").ToString();
                string descrRU = this.dgManualInputParams.ValueFromSelectedRow("DESCRRU").ToString();

                ManualInputParamEditor manualInputParamEditor = new ManualInputParamEditor();

                if (manualInputParamEditor.ShowModal(manualInputParamID, name, temperatureCondition, um, descrEN, descrRU) ?? false)
                {
                    //пользователь выполнил сохранение параметра
                    OnPropertyChanged();
                }
            }
        }

        private void mnuDeleteClick(object sender, RoutedEventArgs e)
        {
            if (Routines.IsUserCanDeleteParameter(((MainWindow)this.Owner).FPermissionsLo))
            {
                int manualInputParamID = this.SelectedManualInputParamID();

                if (manualInputParamID == -1)
                {
                    MessageBox.Show(Properties.Resources.ObjectForDeleteNotSelected, System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                string name = this.dgManualInputParams.ValueFromSelectedRow("NAME").ToString();
                TemperatureCondition temperatureCondition = (TemperatureCondition)Enum.Parse(typeof(TemperatureCondition), this.dgManualInputParams.ValueFromSelectedRow("TEMPERATURECONDITION").ToString());
                string fullName = string.Concat(name, " (", temperatureCondition.ToString(), ")");

                if (MessageBox.Show(string.Format(Properties.Resources.ConfirmationMessForDeleteManualInputParam, fullName), System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    DbRoutines.DeleteAllByManualInputParamID(manualInputParamID);

                    //удаление параметра выполнено
                    OnPropertyChanged();
                }
            }
        }
    }
}
