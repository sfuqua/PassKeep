using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace PassKeep.Common
{
    public class NotNullToSnappedWidthConverter : IValueConverter
    {
        #region IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double thickness = (value == null ? 0 : 346);
            return new GridLength(thickness);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
