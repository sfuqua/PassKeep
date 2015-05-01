using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace PassKeep.Converters
{
    /// <summary>
    /// Converts an int (item count) to a Visibility (Visible if count == 0).
    /// </summary>
    /// <remarks>
    /// Useful for the visibility of an "empty data text" label.
    /// </remarks>
    public sealed class ItemCountToEmptyLabelVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Attempts to convert an integer value, where 0 is Visible and nonzero is Collapsed.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int itemCount = (value as int?) ?? 0;
            return (itemCount == 0 ? Visibility.Visible : Visibility.Collapsed);
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
