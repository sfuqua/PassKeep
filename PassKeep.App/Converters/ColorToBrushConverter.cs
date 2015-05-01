using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace PassKeep.Converters
{
    /// <summary>
    /// A Converter for switching between Brushes and Colors.
    /// </summary>
    public sealed class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || !(value is Color))
            {
                // If the value being converted is not appropriate, return the parameter,
                // which should be a brush.
                return parameter as Brush;
            }

            return new SolidColorBrush((Color)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == null || !(value is SolidColorBrush))
            {
                Color? defaultValue = parameter as Color?;
                if (!defaultValue.HasValue)
                {
                    throw new ArgumentException("value");
                }

                return defaultValue.Value;
            }

            return ((SolidColorBrush)value).Color;
        }
    }
}
