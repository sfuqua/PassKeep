using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Data;

namespace PassKeep.Converters
{
    /// <summary>
    /// An IValueConverter that combines the result of N child IValueConverters in a chain.
    /// </summary>
    public class ValueConverterGroup : List<IValueConverter>, IValueConverter
    {
        #region IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return this.Aggregate(value, (current, converter) => converter.Convert(current, targetType, parameter, language));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
