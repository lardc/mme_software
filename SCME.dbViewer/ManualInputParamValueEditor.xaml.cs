using SCME.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for ManualInputParamValueEditor.xaml
    /// </summary>
    public partial class ManualInputParamValueEditor : Window
    {
        public ManualInputParamValueEditor()
        {
            InitializeComponent();

            this.Owner = Application.Current.MainWindow;
        }

        public bool? ShowModal(int dev_ID, int manualInputParamID, double value)
        {
            //данная реализация принимает на вход идентификаторы manualInputParamID и Dev_ID
            //возвращает True - пользователь нажал кнопку OK. на момент получения такого результата в базу данных уже было выполнено сохранение значения параметра с идентификатором manualInputParamID для изделия Dev_ID;
            //возвращает False - пользователь закрыл форму, т.е. отказался от редактирования значения параметра manualInputParamID для изделия Dev_ID
            tbManualInputDevParamValue.Text = value.ToString();
            bool? result = this.ShowDialog();

            if (result ?? false)
            {
                //пользователь хочет сохранить введённое значение параметра
                double settedValue;

                if (Routines.TryStringToDouble(tbManualInputDevParamValue.Text, out settedValue))
                {
                    DbRoutines.SaveToManualInputDevParam(dev_ID, manualInputParamID, settedValue);
                }
            }

            return result;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.DialogResult = false;
        }

        private void FloatValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = (Routines.SystemDecimalSeparator() == ',') ? new Regex("[^0-9,-]+") : new Regex("[^0-9.-]+");

            e.Handled = regex.IsMatch(e.Text);
        }

        private void btOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

    }
}
