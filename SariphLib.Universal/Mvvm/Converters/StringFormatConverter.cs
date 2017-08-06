// Copyright 2017 Steven Fuqua
// This file is part of the SariphLib library and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using System;
using Windows.UI.Xaml.Data;

namespace SariphLib.Mvvm.Converters
{
    /// <summary>
    /// A converter for formatting strings in bindings.
    /// </summary>
    public class StringFormatConverter : IValueConverter
    {
        /// <summary>
        /// Formats <paramref name="value"/> with format string <paramref name="parameter"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return String.Empty;
            }

            if (parameter == null)
            {
                return value.ToString();
            }

            string strFormat = parameter.ToString();
            return String.Format(strFormat, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
