// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.KeePass.Kdf;
using SariphLib.Diagnostics;
using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;

namespace PassKeep.Converters
{
    /// <summary>
    /// Converts KeePass KDF algorithm GUIDs to friendly names and back again.
    /// </summary>
    public class KdfGuidToStringConverter : IValueConverter
    {
        private readonly string aesName;
        private readonly string argon2Name;

        public KdfGuidToStringConverter()
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("AlgorithmNames");
            this.aesName = loader.GetString("Aes");
            this.argon2Name = loader.GetString("Argon2");
        }

        /// <summary>
        /// Converts a GUID to a human readable name for the algorithm.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            DebugHelper.Assert(targetType == typeof(string));
            if (value is Guid kdfGuid)
            {
                if (kdfGuid == AesParameters.AesUuid)
                {
                    return this.aesName;
                }
                else if (kdfGuid == Argon2Parameters.Argon2Uuid)
                {
                    return this.argon2Name;
                }
                else
                {
                    return "Unknown KDF";
                }
            }
            else
            {
                return "BAD VALUE";
            }
        }

        /// <summary>
        /// Converts the algorithm name back to its GUID.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            DebugHelper.Assert(targetType == typeof(Guid));
            if (value is string kdfString)
            {
                if (kdfString == this.aesName)
                {
                    return AesParameters.AesUuid;
                }
                else if (kdfString == this.argon2Name)
                {
                    return Argon2Parameters.Argon2Uuid;
                }
                else
                {
                    DebugHelper.Assert(false);
                    return Guid.Empty;
                }
            }
            else
            {
                return Guid.Empty;
            }
        }
    }
}
