using PropertyChanged;
using SCME.Types.BaseTestParams;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SCME.WpfControlLibrary.Commands;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;

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
            DependencyProperty.Register(nameof(ItemSource), typeof(ObservableCollection<BaseTestParametersAndNormatives>), typeof(ListViewTestParametersUserControl), new FrameworkPropertyMetadata(null,new PropertyChangedCallback(OnFirstPropertyChanged)));

        private static void OnFirstPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var self = sender as ListViewTestParametersUserControl;
            if (e.OldValue != null)
                (e.OldValue as ObservableCollection<BaseTestParametersAndNormatives>).CollectionChanged -= self.ItemSource_CollectionChanged;
            if(e.NewValue != null)
                (e.NewValue as ObservableCollection<BaseTestParametersAndNormatives>).CollectionChanged += self.ItemSource_CollectionChanged;
        }

        public ListViewTestParametersUserControl()
        {
            InitializeComponent();
            
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

        private void ItemSource_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                var sv = FindParent<ScrollViewer>(this);
                sv.ScrollToBottom();
            }
        }

        public ICommand DeleteRelayCommand => new RelayCommand<BaseTestParametersAndNormatives>(q =>
        {
            ItemSource.Remove(q);
            var n = 1;
            foreach (var i in ItemSource.Where(m => m.TestParametersType == q.TestParametersType && m.NumberPosition == q.NumberPosition))
            {
                if (i.Index != n)
                    i.Index = n;
                n++;
            }
        });
        
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


    }
}
