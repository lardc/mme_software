using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using SCME.Types.VTM;
using SCME.WpfControlLibrary.Properties;

namespace SCME.WpfControlLibrary.IValueConverters
{
    public class BoolToBrush : IValueConverter
    {
        public Brush TrueBrush { get; set; }
        public Brush FalseBrush { get; set; }
        public Brush NullBrush { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (bool?)value;
            if (val == null)
                return NullBrush;
            else if (val.Value)
                return TrueBrush;
            else
                return FalseBrush;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}