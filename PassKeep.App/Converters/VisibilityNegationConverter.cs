using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace PassKeep.Converters
{
    /// <summary>
    /// Value converter that translates visible to hidden and vice versa.
    /// </summary>
    public sealed class VisibilityNegationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Visibility? visValue = value as Visibility?;
            if (visValue == null)
            {
                return null;
            }

            return (visValue.Value == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return Convert(value, targetType, parameter, language);
        }
    }
}
