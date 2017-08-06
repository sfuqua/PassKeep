// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace PassKeep.Converters
{
    /// <summary>
    /// Converts a nonzero double to Visible, everything else to Collapsed.
    /// </summary>
    public sealed class NonZeroDoubleToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Attempts to convert a value, where not !0 is true and null is false.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double? d = value as double?;
            return (d.HasValue && d != 0 ? Visibility.Visible : Visibility.Collapsed);
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
