// Copyright 2017 Steven Fuqua
// This file is part of the SariphLib library and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;

namespace SariphLib.Mvvm.Converters
{
    /// <summary>
    /// A converter for representing a bool as "Yes" or "No".
    /// </summary>
    public sealed class BooleanToYesNoConverter : IValueConverter
    {
        private ResourceLoader resourceLoader;

        public BooleanToYesNoConverter()
        {
            this.resourceLoader = ResourceLoader.GetForViewIndependentUse("SariphLib.Universal/Resources");
        }

        /// <summary>
        /// Converts the specified number, in bytes, to a friendly file size string.
        /// </summary>
        /// <param name="value">The file size as a ulong, in bytes.</param>
        /// <param name="targetType">String.</param>
        /// <param name="parameter">Not used.</param>
        /// <param name="language">Not used.</param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (targetType != typeof(string))
            {
                throw new ArgumentException("targetType must be String", nameof(targetType));
            }

            bool boolValue = (value as bool?) ?? false;
            return this.resourceLoader.GetString(boolValue ? "Yes" : "No");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
