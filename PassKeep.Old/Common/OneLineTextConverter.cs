using System;
using Windows.UI.Xaml.Data;

namespace PassKeep.Common
{
    /// <summary>
    /// Value converter that translates true to false and vice versa.
    /// </summary>
    public sealed class OneLineTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string sValue = value as string;
            if (sValue == null)
            {
                return null;
            }

            return sValue.Replace('\n', ' ').Replace("\r", string.Empty);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return Convert(value, targetType, parameter, language);
        }
    }
}
