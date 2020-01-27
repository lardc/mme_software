using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace SCME.WpfControlLibrary.CustomControls
{
    public partial class PanelButton : UserControl
    {
//        public Brush Background
//        {
//            get => (Brush) GetValue(BackgroundProperty);
//            set => SetValue(BackgroundProperty, value);
//        }
//
//        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
//            nameof(Background), typeof(Brush), typeof(PanelButton), new FrameworkPropertyMetadata
//            {
//                DefaultValue = Brushes.Aqua,
//                BindsTwoWayByDefault = true,
//                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
//            });
        
        public Geometry Data
        {
            get => (Geometry) GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
            nameof(Data), typeof(Geometry), typeof(PanelButton), new FrameworkPropertyMetadata
            {
                DefaultValue = null,
                BindsTwoWayByDefault = true,
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
            
        public string Caption
        {
            get => (string) GetValue(CaptionProperty);
            set => SetValue(CaptionProperty, value);
        }

        public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register(
            nameof(Caption), typeof(string), typeof(PanelButton), new FrameworkPropertyMetadata
            {
                DefaultValue = string.Empty,
                BindsTwoWayByDefault = true,
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

        public PanelButton()
        {
            InitializeComponent();
        }
    }
}