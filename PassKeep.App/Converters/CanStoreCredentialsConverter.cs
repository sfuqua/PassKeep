using System;
using Windows.Security.Credentials.UI;
using Windows.UI.Xaml.Data;

namespace PassKeep.Converters
{
    /// <summary>
    /// One-off converter for converting <see cref="UserConsentVerifierAvailability"/> to a bool
    /// if a verifier is available.
    /// </summary>
    public sealed class CanStoreCredentialsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            UserConsentVerifierAvailability? availability = value as UserConsentVerifierAvailability?;
            return availability == UserConsentVerifierAvailability.Available;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
