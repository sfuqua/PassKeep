using PassKeep.Lib.Contracts.Models;
using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace PassKeep.Converters
{
    /// <summary>
    /// Maps node glyphs to the fonts needed to render them.
    /// </summary>
    /// <remarks>
    /// Most gylphs are standard unicode emoji so we use Segoe UI Symbol -
    /// some don't have good analogues so glyphs are taken from other fonts.
    /// </remarks>
    public class IKeePassNodeToFontConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            IKeePassNode node = value as IKeePassNode;
            if (node != null)
            {
                FontFamily font = null;

                // Fallback based on the IconID
                switch (node.IconID)
                {
                    case 3: // Disk connected to LAN ("Network Server")
                    case 5: // Man with speech bubble ("UserCommunication")
                    case 12: // Wireless transmission thingy ("IR communication")
                    case 22: // Open notebook ("Paper New")
                    case 24: // Plug with lightning ("Energy careful")
                    case 30: // Console ("Console")
                    case 42: // RAM DIMM ("Memory")
                    case 44: // Sticky note with push pin ("Note")
                    case 52: // Square with closed padlock ("Paper locked")
                        font = new FontFamily("Segoe MDL2 Assets");
                        break;
                    default:
                        font = new FontFamily("Segoe UI Symbol");
                        break;
                }

                return font;
            }
            else
            {
                throw new ArgumentException("Value is not an IKeePassNode", nameof(value));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
