using System;
using Windows.UI.Xaml.Data;

namespace PassKeep.Common
{
    public sealed class UintToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is uint))
            {
                return string.Empty;
            }

            return ((uint)value).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            string strValue = value as string;
            if (strValue == null)
            {
                return 0;
            }

            uint retVal;
            if (!uint.TryParse(strValue, out retVal))
            {
                return 0;
            }
            return retVal;
        }
    }
}
