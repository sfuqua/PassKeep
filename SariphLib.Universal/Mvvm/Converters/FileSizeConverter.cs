// Copyright 2017 Steven Fuqua
// This file is part of the SariphLib library and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;

namespace SariphLib.Mvvm.Converters
{
    /// <summary>
    /// A converter for representing a file size (in bytes) as a clean string.
    /// </summary>
    public sealed class FileSizeConverter : IValueConverter
    {
        private static readonly string[] ResourceKeys =
        {
            "Bytes",
            "KibibyteSuffix",
            "MebibyteSuffix",
            "GibibyteSuffix",
            "TebibyteSuffix"
        };

        private ResourceLoader resourceLoader;

        public FileSizeConverter()
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

            ulong sizeInBytes = (value as ulong?) ?? 0;

            int suffixIndex;
            if (sizeInBytes != 0)
            {
                suffixIndex = System.Convert.ToInt32(
                    Math.Min(
                        Math.Floor(Math.Log(sizeInBytes, 1024)),
                        ResourceKeys.Length - 1
                    )
                );
            }
            else
            {
                suffixIndex = 0;
            }

            string suffix = this.resourceLoader.GetString(ResourceKeys[suffixIndex]);
            double units = sizeInBytes / Math.Pow(1024, suffixIndex);

            if (suffixIndex > 0)
            {
                return $"{units:0.00} {suffix}";
            }
            else
            {
                // For bytes, we don't use a decimal
                return $"{units} {suffix}";
            }
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
