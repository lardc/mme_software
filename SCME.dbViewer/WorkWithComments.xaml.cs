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
using SCME.Types;

namespace SCME.dbViewer
{
    /// <summary>
    /// Interaction logic for WorkWithComments.xaml
    /// </summary>
    public partial class WorkWithComments : Window, INotifyPropertyChanged
    {
        private int FDevID;

        public WorkWithComments(int devID)
        {
            InitializeComponent();

            this.Owner = Application.Current.MainWindow;

            this.FDevID = devID;
            this.Title = string.Concat(Properties.Resources.DeviceComments, " ", DbRoutines.DeviceCodeByDevID(devID));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public DataView DeviceComments
        {
            get
            {
                DataTable dt = new DataTable();
                SqlConnection connection = DBConnections.Connection;
                connection.Open();

                try
                {
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    string sqlText = string.Format("SELECT DC.ID, DC.USERLOGIN, CONCAT(DC.LASTNAME, ' ', CONCAT(SUBSTRING(DC.FIRSTNAME, 1, 1), '.', SUBSTRING(DC.MIDDLENAME, 1, 1), '.')) AS FULLUSERNAME, C.RECORDDATE, C.COMMENTS" +
                                                   " FROM DEVICECOMMENTS AS C WITH(NOLOCK)" +
                                                   "  INNER JOIN [SA-011].[SL_PE_DC20002].dbo.RUSDC_USERS AS DC WITH(NOLOCK) ON (C.USERID=DC.ID)" +
                                                   " WHERE (C.DEV_ID={0})" +
                                                   " ORDER BY C.RECORDDATE", this.FDevID);

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

        public bool? ShowModal()
        {
            MainWindow main = (MainWindow)this.Owner;

            if (Routines.IsUserCanReadCreateComments(main.FPermissionsLo))
            {
                lbComment.Visibility = Visibility.Visible;
                tbComment.Visibility = Visibility.Visible;
                btOk.Visibility = Visibility.Visible;
            }
            else
            {
                lbComment.Visibility = Visibility.Collapsed;
                tbComment.Visibility = Visibility.Collapsed;
                btOk.Visibility = Visibility.Collapsed;
            }

            bool? result = this.ShowDialog();

            return result;
        }

        private void btOk_Click(object sender, RoutedEventArgs e)
        {
            //добавляем новый комментарий
            if (this.tbComment.Text.Trim() != string.Empty)
            {
                DbRoutines.InsertToDeviceComments(this.FDevID, ((MainWindow)this.Owner).FUserID, this.tbComment.Text);
                this.tbComment.Clear();

                OnPropertyChanged();
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.DialogResult = false;
        }
    }
}
