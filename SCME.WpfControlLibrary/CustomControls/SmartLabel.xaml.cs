using PropertyChanged;
using System;
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

namespace SCME.WpfControlLibrary.CustomControls
{
    /// <summary>
    /// Логика взаимодействия для SmartLabel.xaml
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public partial class SmartLabel : UserControl
    {
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(SmartLabel), new PropertyMetadata(double.NegativeInfinity));

        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(SmartLabel), new PropertyMetadata(double.PositiveInfinity));

        public double Interval
        {
            get { return (double)GetValue(IntervalProperty); }
            set { SetValue(IntervalProperty, value); }
        }
        public static readonly DependencyProperty IntervalProperty = DependencyProperty.Register(nameof(Interval), typeof(double), typeof(SmartLabel), new PropertyMetadata(1.0));

        public double? Value
        {
            get { return (double?)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(double?), typeof(SmartLabel), new FrameworkPropertyMetadata
        {
            DefaultValue = null,
            BindsTwoWayByDefault = true,
            DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });

        public SmartLabel()
        {
            InitializeComponent();
        }
    }
}
