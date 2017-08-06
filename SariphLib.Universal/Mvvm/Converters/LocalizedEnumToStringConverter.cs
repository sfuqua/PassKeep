// Copyright 2017 Steven Fuqua
// This file is part of the SariphLib library and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;

namespace SariphLib.Mvvm.Converters
{
    /// <summary>
    /// A converter that looks up localized strings for an enum value.
    /// </summary>
    public sealed class LocalizedEnumToStringConverter : IValueConverter
    {
        /// <summary>
        /// Converts the specified enum <paramref name="value"/> to a localized string.
        /// The name of the enum value is looked up in the default ResourceLoader.
        /// If <paramref name="parameter"/> is specified, it is used to choose the right
        /// resource file.
        /// </summary>
        /// <param name="value">The enum value to convert.</param>
        /// <param name="targetType">Should be string.</param>
        /// <param name="parameter">If specified, the resource file to load the string from.</param>
        /// <param name="language">Not used.</param>
        /// <returns>A localized representation of <paramref name="value"/>.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || !(value is Enum))
            {
                return null;
            }

            if (targetType != typeof(string))
            {
                throw new ArgumentException($"{nameof(targetType)} should be string", nameof(targetType));
            }

            ResourceLoader resourceLoader;
            if (parameter != null)
            {
                string resourceFile = parameter as string;
                if (resourceFile == null)
                {
                    throw new ArgumentException("Parameter must be a string", nameof(parameter));
                }

                resourceLoader = ResourceLoader.GetForViewIndependentUse(resourceFile);
            }
            else
            {
                resourceLoader = ResourceLoader.GetForViewIndependentUse();
            }

            string localizedText = resourceLoader.GetString(value.ToString());
            return localizedText;
        }

        /// <summary>
        /// Not implemented.
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
