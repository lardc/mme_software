using PropertyChanged;
using SCME.Types.BaseTestParams;
using System;
using System.Collections;
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

namespace SCME.WpfControlLibrary.CustomControls.ProfilesPageComponents
{
    /// <summary>
    /// Логика взаимодействия для ListBoxTestParameters.xaml
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public partial class ListViewTestParametersUserControl : UserControl
    {
        public bool ContentIsEnabled
        {
            get => (bool)GetValue(ContentIsEnabledProperty);
            set => SetValue(ContentIsEnabledProperty, value);
        }

        // Using a DependencyProperty as the backing store for ItemSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentIsEnabledProperty =
            DependencyProperty.Register(nameof(ContentIsEnabled), typeof(bool), typeof(ListViewTestParametersUserControl), new PropertyMetadata(true));
        

        public ObservableCollection<BaseTestParametersAndNormatives> ItemSource
        {
            get => (ObservableCollection<BaseTestParametersAndNormatives>)GetValue(ItemSourceProperty);
            set => SetValue(ItemSourceProperty, value);
        }

        // Using a DependencyProperty as the backing store for ItemSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemSourceProperty =
            DependencyProperty.Register(nameof(ItemSource), typeof(IEnumerable), typeof(ListViewTestParametersUserControl), new PropertyMetadata(null));


        public ListViewTestParametersUserControl()
        {
            InitializeComponent();
        }

        private void DeleteParameter_Click(object sender, RoutedEventArgs e)
        {
            ItemSource.Remove((sender as Button).DataContext as BaseTestParametersAndNormatives);
        }
        
    }
}
