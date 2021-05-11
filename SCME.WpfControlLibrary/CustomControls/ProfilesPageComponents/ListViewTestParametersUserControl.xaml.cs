using PropertyChanged;
using SCME.Types.BaseTestParams;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SCME.WpfControlLibrary.Commands;

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
            set
            {
                SetValue(ItemSourceProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for ItemSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemSourceProperty =
            DependencyProperty.Register(nameof(ItemSource), typeof(IEnumerable), typeof(ListViewTestParametersUserControl), new PropertyMetadata(null));


        public ListViewTestParametersUserControl()
        {
            InitializeComponent();
        }

        public ICommand DeleteRelayCommand => new RelayCommand<BaseTestParametersAndNormatives>(q => ItemSource.Remove(q));
        
        public ICommand MoveUp => new RelayCommand<BaseTestParametersAndNormatives>((o) =>
        {
            var index = ItemSource.IndexOf(o);
            var upItem = ItemSource[index - 1];

            int tmp = o.Order;
            o.Order = upItem.Order;
            upItem.Order = tmp;
            
            ItemSource.Move(index, index-1);

        }, (o) => ItemSource?.IndexOf(o) > 0);
        
        public ICommand MoveDown => new RelayCommand<BaseTestParametersAndNormatives>((o) =>
        {
            var index = ItemSource.IndexOf(o);
            var downItem = ItemSource[index + 1];

            int tmp = o.Order;
            o.Order = downItem.Order;
            downItem.Order = tmp;
            
            ItemSource.Move(index, index+1);

        }, (o) => ItemSource?.IndexOf(o) < ItemSource?.Count - 1);
        
        
        
        private void DeleteParameter_Click(object sender, RoutedEventArgs e)
        {
            ItemSource.Remove(((Button) sender).DataContext as BaseTestParametersAndNormatives);
        }

        private void ParametersList_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ParametersList.Items.Count > 0)
            ParametersList.ScrollIntoView(ParametersList.Items[ParametersList.Items.Count - 1]);
        }
    }
}
