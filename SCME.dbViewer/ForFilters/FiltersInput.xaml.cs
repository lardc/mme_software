using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Collections.ObjectModel;
using System.ComponentModel;
using SCME.dbViewer.CustomControl;
using System.Windows.Input;
using System.Globalization;

namespace SCME.dbViewer.ForFilters
{
    /// <summary>
    /// Interaction logic for FiltersInput.xaml
    /// </summary>
    public partial class FiltersInput : Window
    {
        public ActiveFilters Filters { get; set; }

        public FiltersInput(ActiveFilters activeFilters)
        {
            InitializeComponent();

            this.Owner = Application.Current.MainWindow;
            this.ShowInTaskbar = false;

            this.Filters = activeFilters;
            DataContext = this;
        }       

        public bool? Demonstrate(Point position)
        {
            if (position != null)
            {
                double workAreaWidth = System.Windows.SystemParameters.WorkArea.Width;
                double left;

                if ((position.X + this.Width) > workAreaWidth)
                    left = workAreaWidth - this.Width;
                else left = position.X;

                this.Left = left;
                this.Top = position.Y;
            }

            return this.ShowDialog();
        }

        private void DeleteLastFilter()
        {
            //удаление последнего фильтра
            if (lvFilters.ItemsSource != null)
            {
                var collection = lvFilters.ItemsSource as ActiveFilters;
                int index = collection.Count - 1;

                if (index >= 0)
                    collection?.RemoveAt(index);
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DeleteLastFilter();
                Close();
            }
        }

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            DeleteLastFilter();
        }

        private void btOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void btDeleteAllFilters_Click(object sender, RoutedEventArgs e)
        {
            //удаление всех фильтров
            var collection = lvFilters.ItemsSource as ActiveFilters;
            collection?.Clear();

            this.DialogResult = true;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
