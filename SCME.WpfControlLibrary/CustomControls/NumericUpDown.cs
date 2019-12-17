using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using SCME.Types;
using SCME.WpfControlLibrary.Commands;

namespace SCME.WpfControlLibrary.CustomControls
{
    public class NumericUpDown : UserControl
    {
        public double? Minimum
        {
            get => (double?)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            nameof(Minimum), typeof(double?), typeof(NumericUpDown),new PropertyMetadata(null));
        
        public double? Maximum
        {
            get => (double?)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            nameof(Maximum), typeof(double?), typeof(NumericUpDown),new PropertyMetadata(null));
        
        public double Interval
        {
            get => (double)GetValue(IntervalProperty);
            set => SetValue(IntervalProperty, value);
        }
        public static readonly DependencyProperty IntervalProperty = DependencyProperty.Register(
            nameof(Interval), typeof(double), typeof(NumericUpDown),new PropertyMetadata(1.0));
        
        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value), typeof(double), typeof(NumericUpDown),new FrameworkPropertyMetadata()
            {
                BindsTwoWayByDefault = true,
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                DefaultValue = 1.0
            });
        
     
        
        public string StringFormat
        {
            get => (string)GetValue(StringFormatProperty);
            set => SetValue(StringFormatProperty, value);
        }
        public static readonly DependencyProperty StringFormatProperty = DependencyProperty.Register(
            nameof(StringFormat), typeof(string), typeof(NumericUpDown),new PropertyMetadata(null));
        
        public RelayCommand Up => new RelayCommand((obj) =>
        {
            if(Maximum == null)
                Value += Interval;
            else
            {
                if (Value + Interval < Maximum)
                    Value += Interval;
                else
                    Value = Maximum.Value;
            }

        }, (obj) =>
        {
            if (Maximum == null)
                return true;
            return Value < Maximum;
        });

     
        
        public RelayCommand Down => new RelayCommand((obj) =>
        {
            if(Minimum == null)
                Value -= Interval;
            else
            {
                if (Value - Interval > Minimum)
                    Value -= Interval;
                else
                    Value = Minimum.Value;
            }
        }, (obj) =>
        {
            if (Minimum == null)
                return true;
            return Value > Minimum;
        }); 
        

        public ConstantsMinMax.MinMaxInterval MinMaxInterval
        {
            set
            {
                Minimum = value.Minimum;
                Maximum = value.Maximum;
                Interval = value.Interval;
            }
        }

        public static T FindChild<T>(DependencyObject parent, string childName)
            where T : DependencyObject
        {    
            // Confirm parent and childName are valid. 
            if (parent == null) return null;
            T foundChild = null;
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);
                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }
            return foundChild;
        }
        
        public NumericUpDown()
        {
            Loaded +=OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            
        }

//        public new float Minimum
//        {
//            get => Convert.ToSingle(base.Minimum);
//            set => base.Minimum = Convert.ToSingle(value);
//        }
    }
}