using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SCME.ProfileManager
{
    [ValueConversion(typeof(System.Drawing.Bitmap), typeof(ImageSource))]
    public class BitmapToImageSourceConverter : IValueConverter
    {
        public object Convert(object Value, Type TargetType, object Parameter, CultureInfo Culture)
        {
            var bmp = Value as System.Drawing.Bitmap;
            if (bmp == null)
                return null;
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        bmp.GetHbitmap(),
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
        }

        public object ConvertBack(object Value, Type TargetType, object Parameter, CultureInfo Culture)
        {
            throw new NotSupportedException();
        }
    }

    [ValueConversion(typeof(Enum), typeof(String))]
    public class EnumValueToString : IValueConverter
    {
        public object Convert(object Value, Type TargetType, object Parameter, CultureInfo Culture)
        {
            return Value.ToString();
        }

        public object ConvertBack(object Value, Type TargetType, object Parameter, CultureInfo Culture)
        {
            return Enum.Parse(TargetType, (string)Value);
        }
    }

    [ValueConversion(typeof(UInt16), typeof(UInt16))]
    public class FrequencyDivisorToFrequency : IValueConverter
    {
        public object Convert(object Value, Type TargetType, object Parameter, CultureInfo Culture)
        {
            return (50 / (ushort)Value).ToString(CultureInfo.InvariantCulture);
        }

        public object ConvertBack(object Value, Type TargetType, object Parameter, CultureInfo Culture)
        {
            return (ushort)(50 / UInt16.Parse((string)Value));
        }
    }

}
