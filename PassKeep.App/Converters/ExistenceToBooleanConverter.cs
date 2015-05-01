using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace PassKeep.Converters
{
    /// <summary>
    /// Converts a value (not null) to true, and null to false.
    /// </summary>
    public sealed class ExistenceToBooleanConverter : IValueConverter
    {
        /// <summary>
        /// Attempts to convert a value, where not null is true and null is false.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value != null;
        }

        /// <summary>
        /// Not implemented - ConvertBack is undefined for this converter.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
