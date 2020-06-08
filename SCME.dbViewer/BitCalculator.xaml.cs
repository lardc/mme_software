using SCME.Types;
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
    /// Interaction logic for BitCalculator.xaml
    /// </summary>
    public partial class BitCalculator : Window
    {
        public BitCalculator(long userID, ulong permissionsLo, string title)
        {
            InitializeComponent();

            this.FUserID = userID;
            this.FValueLo = permissionsLo;

            this.Owner = Application.Current.MainWindow;
            this.Title = title;
        }

        private long FUserID = -1;
        private ulong FValueLo = 0;

        public bool? ShowModal(out ulong permissionsLo)
        {
            this.ShowData();

            bool? result = this.ShowDialog();

            if (result ?? false)
            {
                //сохраняем сформированное значение битовой маски разрешений в базу данных
                DbRoutines.SaveToUsers(this.FUserID, this.FValueLo);
            }

            permissionsLo = this.FValueLo;
            return result;
        }

        private void checkBoxClick(CheckBox cb, byte numberOfBit)
        {            
            this.FValueLo = (cb.IsChecked ?? false) ? Routines.SetBit(this.FValueLo, numberOfBit) : Routines.DropBit(this.FValueLo, numberOfBit);
        }

        private void cbBit0_Click(object sender, RoutedEventArgs e)
        {
            const byte numberOfBit = 0;

            this.checkBoxClick(sender as CheckBox, numberOfBit);
        }

        private void cbBit1_Click(object sender, RoutedEventArgs e)
        {
            const byte numberOfBit = 1;

            this.checkBoxClick(sender as CheckBox, numberOfBit);
        }

        private void cbBit2_Click(object sender, RoutedEventArgs e)
        {
            const byte numberOfBit = 2;

            this.checkBoxClick(sender as CheckBox, numberOfBit);
        }

        private void cbBit3_Click(object sender, RoutedEventArgs e)
        {
            const byte numberOfBit = 3;

            this.checkBoxClick(sender as CheckBox, numberOfBit);
        }

        private void cbBit4_Click(object sender, RoutedEventArgs e)
        {
            const byte numberOfBit = 4;

            this.checkBoxClick(sender as CheckBox, numberOfBit);
        }

        private void cbBit5_Click(object sender, RoutedEventArgs e)
        {
            const byte numberOfBit = 5;

            this.checkBoxClick(sender as CheckBox, numberOfBit);
        }

        private void cbBit6_Click(object sender, RoutedEventArgs e)
        {
            const byte numberOfBit = 6;

            this.checkBoxClick(sender as CheckBox, numberOfBit);
        }

        private void cbBit7_Click(object sender, RoutedEventArgs e)
        {
            const byte numberOfBit = 7;

            this.checkBoxClick(sender as CheckBox, numberOfBit);
        }

        private void cbBit8_Click(object sender, RoutedEventArgs e)
        {
            const byte numberOfBit = 8;

            this.checkBoxClick(sender as CheckBox, numberOfBit);
        }

        private void cbBit9_Click(object sender, RoutedEventArgs e)
        {
            const byte numberOfBit = 9;

            this.checkBoxClick(sender as CheckBox, numberOfBit);
        }

        private void cbBit10_Click(object sender, RoutedEventArgs e)
        {
            const byte numberOfBit = 10;

            this.checkBoxClick(sender as CheckBox, numberOfBit);
        }

        private void cbBit11_Click(object sender, RoutedEventArgs e)
        {
            const byte numberOfBit = 11;

            this.checkBoxClick(sender as CheckBox, numberOfBit);
        }

        private void cbBit12_Click(object sender, RoutedEventArgs e)
        {
            const byte numberOfBit = 12;

            this.checkBoxClick(sender as CheckBox, numberOfBit);
        }

        private void cbBit13_Click(object sender, RoutedEventArgs e)
        {
            const byte numberOfBit = 13;

            this.checkBoxClick(sender as CheckBox, numberOfBit);
        }

        private void cbBit14_Click(object sender, RoutedEventArgs e)
        {
            const byte numberOfBit = 14;

            this.checkBoxClick(sender as CheckBox, numberOfBit);
        }

        private void cbBit15_Click(object sender, RoutedEventArgs e)
        {
            const byte numberOfBit = 15;

            this.checkBoxClick(sender as CheckBox, numberOfBit);
        }





        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    this.DialogResult = false;
                    break;

                case Key.Enter:
                    this.DialogResult = true;
                    break;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void ShowData()
        {
            //перебираем все имеющиеся на форме CheckBox и выставляем их свойства IsChecked
            foreach (CheckBox cb in FindVisualChildren<CheckBox>(mainGrid))
            {
                cb.IsChecked = Routines.CheckBit(this.FValueLo, Convert.ToByte(cb.Tag));
            }
        }

        public IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}
