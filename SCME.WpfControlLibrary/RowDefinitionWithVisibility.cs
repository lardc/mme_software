using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SCME.WpfControlLibrary
{
    public class RowDefinitionWithVisibility : RowDefinition
    {
        public Visibility Visibility
        {
            get => (Visibility) GetValue(VisibilityProperty);
            set => SetValue(VisibilityProperty, value);
        }

        public static readonly DependencyProperty VisibilityProperty =
            DependencyProperty.Register(nameof(Visibility), typeof(Visibility), typeof(RowDefinitionWithVisibility), new FrameworkPropertyMetadata(Visibility.Visible, (o, args) =>
            {
                var row = (RowDefinitionWithVisibility) o;
                var visibility = (Visibility) args.NewValue;

                row.Height = visibility == Visibility.Visible ? new GridLength(1, GridUnitType.Auto) : new GridLength(0);

                row.Visibility = (Visibility) args.NewValue;
            })
            {
                BindsTwoWayByDefault = true,
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
    }
}