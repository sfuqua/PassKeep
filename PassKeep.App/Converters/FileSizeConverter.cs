using System;
using System.Runtime.InteropServices;
using System.Text;
using Windows.UI.Xaml.Data;

namespace PassKeep.Converters
{
    /// <summary>
    /// A converter for representing a file size (in bytes) as a clean string.
    /// </summary>
    public sealed class FileSizeConverter : IValueConverter
    {
        private const uint bufferSize = 64;

        [DllImport("Shlwapi.dll", CharSet=CharSet.Unicode)]
        private static extern void StrFormatByteSizeW(
            ulong fileSize,
            [MarshalAs(UnmanagedType.LPWStr), Out] StringBuilder outputBuffer,
            uint bufferSize
        );

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
            if (targetType != typeof(String))
            {
                throw new ArgumentException("targetType must be String", "targetType");
            }

            ulong convertedValue = (value as ulong?) ?? 0;

            StringBuilder buffer = new StringBuilder((int)bufferSize);
            StrFormatByteSizeW(convertedValue, buffer, bufferSize);

            return buffer.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
