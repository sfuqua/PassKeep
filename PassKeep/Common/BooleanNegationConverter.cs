using System;
using Windows.UI.Xaml.Data;

namespace PassKeep.Common
{
    /// <summary>
    /// Value converter that translates true to false and vice versa.
    /// </summary>
    public sealed class BooleanNegationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool? bValue = value as bool?;
            if (bValue == null)
            {
                return null;
            }

            return !(bValue.Value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return Convert(value, targetType, parameter, language);
        }
    }
}
