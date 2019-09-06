using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SCME.WpfControlLibrary.IValueConverters
{
    public class TryBooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Конвертирует bool в Visibility
        /// </summary>
        /// <param name="value">Конвертируемое значение</param>
        /// <returns></returns>
        public object ConvertWithoutCheckValue(object value)
        {
            switch ((bool)value)
            {
                case true:
                    return Visibility.Visible;
                case false:
                    return Visibility.Hidden;
                default:
                    throw new NotImplementedException($"{nameof(TryBooleanToVisibilityConverter)} {nameof(NotImplementedException)} error");
            }
        }

        /// <summary>
        ///Конвертирует Visibility в bool
        /// </summary>
        /// <param name="value">Конвертируемое значение</param>
        /// <returns></returns>
        public object ConvertBackWithoutCheckValue(object value)
        {
            switch ((Visibility)value)
            {
                case Visibility.Visible:
                    return true;
                case Visibility.Hidden:
                    return false;
                case Visibility.Collapsed:
                    return false;
                default:
                    throw new NotImplementedException($"{nameof(TryBooleanToVisibilityConverter)} {nameof(NotImplementedException)} error");
            }
        }

        /// <summary>
        /// Проверяет значение на null и на принадлежность к типу bool
        /// </summary>
        /// <param name="value">Проверяемое значение</param>
        public void ConvertCheck(object value)
        {
            if (value == null)
                throw new NullReferenceException("Value is null");
            if (typeof(bool).Equals(value.GetType()) == false)
                throw new InvalidCastException($"Value has type {value.GetType()} , need bool ");
        }

        /// <summary>
        /// Проверяет значение на null и на принадлежность к типу Visibility
        /// </summary>
        /// <param name="value">Проверяемое значение</param>
        public void ConvertBackCheck(object value)
        {
            if (value == null)
                throw new NullReferenceException("Value is null");
            if (typeof(Visibility).Equals(value.GetType()) == false)
                throw new InvalidCastException($"Value has type {value.GetType()} , need bool ");
        }

        /// <summary>
        /// Конвертортирует bool в System.Windows.Visibility
        /// </summary>
        /// <param name="value">Значение, произведенное исходной привязкой.</param>
        /// <param name="targetType">Тип целевого свойства привязки.</param>
        /// <param name="parameter">Используемый параметр преобразователя.</param>
        /// <param name="culture">Язык и региональные параметры, используемые в преобразователе.</param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ConvertCheck(value);
            return ConvertWithoutCheckValue(value);
        }

        /// <summary>
        /// Конвертортирует System.Windows.Visibility в bool
        /// </summary>
        /// <param name="value">Значение, произведенное исходной привязкой.</param>
        /// <param name="targetType">Тип целевого свойства привязки.</param>
        /// <param name="parameter">Используемый параметр преобразователя.</param>
        /// <param name="culture">Язык и региональные параметры, используемые в преобразователе.</param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ConvertBackCheck(value);
            return ConvertBackWithoutCheckValue(value);
        }

    }
}
