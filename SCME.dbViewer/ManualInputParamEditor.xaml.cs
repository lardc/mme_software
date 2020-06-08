using SCME.Types;
using SCME.Types.Profiles;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for ManualInputParamEditor.xaml
    /// </summary>
    public partial class ManualInputParamEditor : Window
    {
        public ManualInputParamEditor()
        {
            InitializeComponent();

            this.Owner = Application.Current.MainWindow;
        }

        private bool CheckData()
        {
            //выполняет проверку введённых пользователем данных с точки зрения возможности выполнения сохранения в базу данных
            if ((tbName.Text == null) || (tbName.Text.Trim() == string.Empty))
            {
                MessageBox.Show(string.Concat(Properties.Resources.ParameterNameNotDefined, " ", Properties.Resources.DataWillNotBeSaved), System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            if ((tbUm.Text == null) || (tbUm.Text.Trim() == string.Empty))
            {
                MessageBox.Show(string.Concat(Properties.Resources.UnitMeasureIsNotDefined, " ", Properties.Resources.DataWillNotBeSaved), System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }

            return true;
        }

        public bool? ShowModal(int? manualInputParamID, string name, TemperatureCondition temperatureCondition, string um, string descrEN, string descrRU)
        {
            //данная реализация принимает на вход идентификатор manualInputParamID и реквизиты параметра, отображает их значения в данной форме
            //возвращает True - реквизиты принятого параметра были обновлены пользователем - пользователь нажал кнопку OK. на момент получения такого результата в базу данных уже было выполнено сохранение этих изменений;
            //возвращает False - пользователь закрыл форму, т.е. отказался от редактирования реквизитов параметра
            tbName.Text = name;
            cmbTemperatureCondition.Text = temperatureCondition.ToString();
            tbUm.Text = um;
            tbDescrEN.Text = descrEN;
            tbDescrRU.Text = descrRU;

            bool? result = this.ShowDialog();

            if (result ?? false)
            {
                //пользователь хочет сохранить сделанные изменения
                if (this.CheckData())
                {
                    string editedName = tbName.Text;
                    TemperatureCondition editedTemperatureCondition = (TemperatureCondition)Enum.Parse(typeof(TemperatureCondition), cmbTemperatureCondition.Text.ToString());
                    string editedUm = tbUm.Text;
                    string editedDescrEN = tbDescrEN.Text;
                    string editedDescrRU = tbDescrRU.Text;

                    DbRoutines.SaveToManualInputParams(manualInputParamID, editedName, editedTemperatureCondition, editedUm, editedDescrEN, editedDescrRU);
                }
            }

            return result;
        }

        private void btOK_Click(object sender, RoutedEventArgs e)
        {
            if (this.CheckData())
                this.DialogResult = true;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.DialogResult = false;
        }
    }
}
