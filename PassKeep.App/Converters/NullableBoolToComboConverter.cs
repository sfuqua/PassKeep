using System;
using Windows.UI.Xaml.Data;

namespace PassKeep.Converters
{
    /// <summary>
    /// Handles converting a three-option ComboBox to null -> true -> false.
    /// </summary>
    public sealed class NullableBoolToComboConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is bool))
            {
                return (value == null ? 0 : -1);
            }

            bool bValue = (bool)value;
            return (bValue ? 1 : 2);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (!(value is int))
            {
                return null;
            }

            int iValue = (int)value;
            switch (iValue)
            {
                case 0:
                    return null;
                case 1:
                    return true;
                case 2:
                    return false;
                default:
                    return null;
            }
        }
    }
}
