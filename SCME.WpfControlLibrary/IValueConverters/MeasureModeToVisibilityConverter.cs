using SCME.Types.BVT;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SCME.WpfControlLibrary.IValueConverters
{
    public class MeasureModeToVisibilityConverter : IValueConverter
    {
        public object Convert(object Value, Type TargetType, object Parameter, CultureInfo Culture)
        {
            var en = (BVTMeasurementMode)Value;
            var index = int.Parse((string)Parameter);

            return en == BVTMeasurementMode.ModeI && index == (int)BVTMeasurementMode.ModeI ||
                   en == BVTMeasurementMode.ModeV && index == (int)BVTMeasurementMode.ModeV
                       ? Visibility.Visible
                       : Visibility.Collapsed;
        }

        public object ConvertBack(object Value, Type TargetType, object Parameter, CultureInfo Culture)
        {
            throw new InvalidOperationException("ConvertBack method is not implemented in MeasureModeToVisibilityConverter");
        }
    }
}
