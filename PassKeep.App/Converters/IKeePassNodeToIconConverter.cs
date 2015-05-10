using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.KeePass.Dom;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace PassKeep.Converters
{
    /// <summary>
    /// Gets an icon (as a string) from an IKeePassNode.
    /// </summary>
    public sealed class IKeePassNodeNodeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            IKeePassNode node = value as IKeePassNode;
            if (node != null)
            {
                string icon = null;

                // Fallback based on the IconID
                switch (node.IconID)
                {
                    case 0: // Key ("Key")
                        icon = "\uE192";
                        break;
                    case 1: // Globe ("World")
                        icon = "\uE128";
                        break;
                    case 2: // Triangular hazard/warning ("Warning")
                        icon = "\u26A0";
                        break;
                    case 3: // Disk connected to LAN ("Network Server")
                        icon = "\uE968"; // MDL2 Assets
                        break;
                    case 4: // Folder with pinned note ("MarkedDirectory")
                        goto default;
                    case 5: // Man with speech bubble ("UserCommunication")
                        icon = "\uE939"; // MDL2 Assets
                        break;
                    case 6: // Three blocks (red green orange) in a triangle ("Parts")
                        icon = "\uE152"; // Three squares
                        break;
                    case 7: // Notepad and pencil ("Notepad")
                        icon = "\U0001F4DD"; // 📝
                        break;
                    case 8: // Globe with red plug thing ("World Socket")
                        goto case 1; // Just a globe
                    case 9: // Contact card ("Identity")
                        icon = "\uE136";
                        break;
                    case 10: // Clipboard with green star ("Paper ready")
                        goto default;
                    case 11: // Camera ("Digicam")
                        icon = "\uE114";
                        break;
                    case 12: // Wireless transmission thingy ("IR communication")
                        icon = "\uE704"; // Tower broadcasting signal - MDL2 Assets
                        break;
                    case 13: // Keychain ("Multikeys")
                        icon = "\U0001F510"; // 🔐
                        break;
                    case 14: // Power plug ("Energy")
                        icon = "\uE83E"; // Plugged in battery icon
                        break;
                    case 15: // Scanner
                        icon = "\uE8FE";
                        break;
                    case 16: // Globe with red star ("World star")
                        goto case 1; // Just a globe
                    case 17: // Optical disc ("CDRom")
                        icon = "\U0001F5B8"; // 🖸
                        break;
                    case 18: // Display/monitor ("Monitor")
                        icon = "\U0001F4FA"; // Television 📺
                        break;
                    case 19: // Open envelope ("Email")
                        icon = "\U0001F4E7"; // 📧
                        break;
                    case 20: // Cog/gear ("Configuration")
                        icon = "\uE115";
                        break;
                    case 21: // Clipboard with checkmark ("Clipboard ready")
                        icon = "\uE133"; // List of checkmarks
                        break;
                    case 22: // Open notebook ("Paper New")
                        icon = "\uE8F4"; // Page with pencil and + - MDL2 Assets
                        break;
                    case 23: // Desktop ("Screen")
                        icon = "\uE1E4"; // Monitor with start screen tiles
                        break;
                    case 24: // Plug with lightning ("Energy careful")
                        icon = "\uE945"; // Lightning bolt - MDL2 Assets
                        break;
                    case 25: // Folder with envelope ("EmailBox")
                        icon = "\U0001F4EC"; // 📬
                        break;
                    case 26: // Floppy disk ("Disk")
                        icon = "\uE105";
                        break;
                    case 27: // Device connected to LAN ("Drive")
                        icon = "\uE17B";
                        break;
                    case 28: // Quicktime logo ("PaperQ")
                        icon = "\uE173"; // Video icon
                        break;
                    case 29: // Green screen w/key ("Terminal encrypted")
                        icon = "\uE1A7"; // Rectangle with admin shield
                        break;
                    case 30: // Console ("Console")
                        icon = "\uE756"; // MDL2 Assets
                        break;
                    case 31: // Printer ("Printer")
                        icon = "\uE2F6";
                        break;
                    case 32: // Colored squares ("Program icons")
                        icon = "\uE138";
                        break;
                    case 33: // Red/white checkered flag ("Run")
                        goto default;
                    case 34: // Wrench ("Settings")
                        icon = "\uE15E";
                        break;
                    case 35: // PC with globe ("World computer")
                        goto default;
                    case 36: // WinZip icon ("Archive")
                        goto default;
                    case 37: // % sign ("Homebanking")
                        icon = "%";
                        break;
                    case 38: // Drive with Windows logo ("Drive windows")
                        icon = "\uE1E4"; // Monitor with start screen tiles
                        break;
                    case 39: // Clock ("Clock")
                        icon = "\uE2AD";
                        break;
                    case 40: // Envelope with magnifying glass ("Email search")
                        icon = "\uE721"; // Magnifying glass
                        break;
                    case 41: // Weird white square with red thing ("Paper flag")
                        goto default;
                    case 42: // RAM DIMM ("Memory")
                        icon = "\uE88E"; // Flash drive - MDL2 Assets
                        break;
                    case 43: // Recycling bin / trash can ("Trash bin")
                        icon = "\uE107";
                        break;
                    case 44: // Sticky note with push pin ("Note")
                        icon = "\uE840"; // Push pin
                        break;
                    case 45: // Red X ("Expired")
                        icon = "\uE10A";
                        break;
                    case 46: // Circled question mark (blue) ("Info")
                        icon = "\uE11B"; // Stylized question mark
                        break;
                    case 47: // Box ("Package")
                        icon = "\U0001F4E6"; // 📦
                        break;
                    case 48: // Closed folder ("Folder")
                        icon = "\U0001F4C1"; // 📁
                        break;
                    case 49: // Open folder ("Folder open")
                        icon = "\U0001F4C2"; // 📂
                        break;
                    case 50: // Folder with yellow box ("Folder package")
                        goto default;
                    case 51: // Open padlock ("Lock open")
                        icon = "\uE1F7";
                        break;
                    case 52: // Square with closed padlock ("Paper locked")
                        icon = "\uE755"; // Closed padlock on document - MDL2 Assets
                        break;
                    case 53: // Green checkmark ("Checked")
                        icon = "\uE10B";
                        break;
                    case 54: // Fountain pen ("Pen")
                        icon = "\U0001F58B"; // 🖋
                        break;
                    case 55: // Photograph ("Thumbnail")
                        icon = "\U0001F5BB"; // 🖻
                        break;
                    case 56: // Open book ("Book")
                        icon = "\U0001F4D6"; // 📖
                        break;
                    case 57: // List of details ("List")
                        icon = "\uE179";
                        break;
                    case 58: // Man with key ("User key")
                        goto default;
                    case 59: // Hammer ("Tool")
                        icon = "\U0001F528"; // 🔨
                        break;
                    case 60: // House ("Home")
                        icon = "\U0001F3E0"; // 🏠
                        break;
                    case 61: // Yellow star ("Star")
                        icon = "\uE208";
                        break;
                    case 62: // Penguin ("Tux")
                        icon = "\U0001F427"; // 🐧
                        break;
                    case 63: // Feather ("Feather")
                        goto default;
                    case 64: // Apple ("Apple")
                        icon = "\U0001F34E"; // 🍎
                        break;
                    case 65: // Wikipedia logo ("Wiki")
                        icon = "W";
                        break;
                    case 66: // $ sign ("Money")
                        icon = "\U0001F4B2"; // 💲
                        break;
                    case 67: // Certificate ("Certificate")
                        icon = "\U0001F4DC"; // 📜 (scroll)
                        break;
                    case 68: // Cellphone ("BlackBerry")
                        icon = "\U0001F4F1"; // 📱
                        break;
                    default:
                        if (value is IKeePassEntry)
                        {
                            goto case KdbxEntry.DefaultIconId;
                        }
                        else if (value is IKeePassGroup)
                        {
                            goto case KdbxGroup.DefaultIconId;
                        }
                        break;
                }

                return icon;
            }
            else
            {
                throw new ArgumentException("Value is not an IKeePassNode", nameof(value));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
