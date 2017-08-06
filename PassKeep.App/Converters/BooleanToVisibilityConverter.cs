// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace PassKeep.Converters
{
    /// <summary>
    /// An IValueConverter that converts between bool values and Control visibility.
    /// A Control is treated as Visible if the value is true, and vice versa.
    /// </summary>
    public sealed class BooleanToVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool? bValue = value as bool?;
            return (bValue.HasValue && bValue.Value ? Visibility.Visible : Visibility.Collapsed);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (!(value is Visibility))
            {
                throw new ArgumentException();
            }
            Visibility vValue = (Visibility)value;
            return (vValue == Visibility.Visible);
        }
    }
}
