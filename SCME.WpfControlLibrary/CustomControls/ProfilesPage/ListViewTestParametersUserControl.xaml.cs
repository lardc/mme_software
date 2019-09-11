using PropertyChanged;
using System;
using System.Collections;
using System.Collections.Generic;
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

namespace SCME.WpfControlLibrary.CustomControls.ProfilesPage
{
    /// <summary>
    /// Логика взаимодействия для ListBoxTestParameters.xaml
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public partial class ListViewTestParametersUserControl : UserControl
    {


        public IEnumerable ItemSource
        {
            get { return (IEnumerable)GetValue(ItemSourceProperty); }
            set { SetValue(ItemSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemSourceProperty =
            DependencyProperty.Register("ItemSource", typeof(IEnumerable), typeof(ListViewTestParametersUserControl), new PropertyMetadata(null));


        public ListViewTestParametersUserControl()
        {
            InitializeComponent();
        }


    }
}
