using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SCME.WpfControlLibrary.IValueConverters
{
    public class TryBooleanToVisibilityInverseConverter : IValueConverter
    {
        public TryBooleanToVisibilityConverter _Converter = new TryBooleanToVisibilityConverter();

        /// <summary>
        /// Инверсионно конвертортирует System.Windows.Visibility в bool
        /// </summary>
        /// <param name="value">Значение, произведенное исходной привязкой.</param>
        /// <param name="targetType">Тип целевого свойства привязки.</param>
        /// <param name="parameter">Используемый параметр преобразователя.</param>
        /// <param name="culture">Язык и региональные параметры, используемые в преобразователе.</param>
        /// <returns>Преобразованное значение</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            _Converter.ConvertCheck(value);
            bool inverseValue = !((bool)value);
            return _Converter.ConvertWithoutCheckValue(inverseValue);
        }

        /// <summary>
        /// Инверсионно конвертортирует System.Windows.Visibility в bool
        /// </summary>
        /// <param name="value">Значение, произведенное исходной привязкой.</param>
        /// <param name="targetType">Тип целевого свойства привязки.</param>
        /// <param name="parameter">Используемый параметр преобразователя.</param>
        /// <param name="culture">Язык и региональные параметры, используемые в преобразователе.</param>
        /// <returns>Преобразованное значение</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            _Converter.ConvertBackCheck(value);
            Visibility inverseValue = (Visibility)value == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            return _Converter.ConvertBackWithoutCheckValue(inverseValue);
        }
    }
}
