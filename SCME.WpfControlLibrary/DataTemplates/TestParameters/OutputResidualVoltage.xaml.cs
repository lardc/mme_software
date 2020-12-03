using SCME.Types.BaseTestParams;
using SCME.WpfControlLibrary.CustomControls;
using SCME.WpfControlLibrary.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace SCME.WpfControlLibrary.DataTemplates.TestParameters
{
    /// <summary>
    /// Логика взаимодействия для GateDataTemplate.xaml
    /// </summary>
    public partial class OutputResidualVoltage : Grid
    {
        public OutputResidualVoltage()
        {
            InitializeComponent();
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {

        }


        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var q = FindParent<ListViewMouseLeftButtonScroll>(this);
            var w = q.ItemsSource as ObservableCollection<BaseTestParametersAndNormatives>;
            ProfilesPage.CheckIndexes(w);
        }

        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindParent<T>(parentObject);
        }
    }
}
