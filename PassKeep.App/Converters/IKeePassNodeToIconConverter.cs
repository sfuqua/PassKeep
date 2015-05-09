using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.KeePass.Dom;
using System;
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
                string icon = string.Empty;

                // Fallback based on the IconID
                switch (node.IconID)
                {
                    case 0: // Key ("Key")
                        icon = "\uD83D\uDD11";
                        break;
                    case 1: // Globe ("World")
                        icon = "\uE128";
                        break;
                    case 2: // Triangular hazard/warning ("Warning")
                        icon = "\u26A0";
                        break;
                    case 3: // Disk connected to LAN ("Network Server")
                        goto case 27;
                    case 4: // Folder with pinned note ("MarkedDirectory")
                        goto default;
                    case 5: // Man with speech bubble ("UserCommunication")
                        icon = "\uD83D\uDC81"; // Man with informational "i"
                        break;
                    case 6: // Three blocks (red green orange) in a triangle ("Parts")
                        icon = "\uE152"; // Three squares
                        break;
                    case 7: // Notepad and pencil ("Notepad")
                        icon = "\uD83D\uDCDD";
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
                        icon = "\uD83D\uDCF6"; // WiFi/cell signal
                        break;
                    case 13: // Keychain ("Multikeys")
                        icon = "\uD83D\uDD10"; // Padlock and key
                        break;
                    case 14: // Power plug ("Energy")
                        icon = "\uD83D\uDD0C";
                        break;
                    case 15: // Scanner
                        goto default;
                    case 16: // Globe with red star ("World star")
                        goto case 1; // Just a globe
                    case 17: // Optical disc ("CDRom")
                        icon = "\uD83D\uDCBF";
                        break;
                    case 18: // Display/monitor ("Monitor")
                        icon = "\uD83D\uDCFA"; // Television set
                        break;
                    case 19: // Open envelope ("Email")
                        icon = "\uD83D\uDCE7"; // Email
                        break;
                    case 20: // Cog/gear ("Configuration")
                        icon = "\uE115";
                        break;
                    case 21: // Clipboard with checkmark ("Clipboard ready")
                        icon = "\uE133"; // List of checkmarks
                        break;
                    case 22: // Open notebook ("Paper New")
                        icon = "\uD83D\uDCD4";
                        break;
                    case 23: // Desktop ("Screen")
                        goto case 38; // Start screen
                    case 24: // Plug with lightning ("Energy careful")
                        goto case 14;
                    case 25: // Folder with envelope ("EmailBox")
                        icon = "\uD83D\uDCEB"; // Mailbox
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
                        icon = "\uE1A6"; // Rectangle with user
                        break;
                    case 31: // Printer ("Printer")
                        icon = "\u2399"; // Crappy printer ("Print Screen Symbol")
                        break;
                    case 32: // Colored squares ("Program icons")
                        icon = "\uE138";
                        break;
                    case 33: // Red/white checkered flag ("Run")
                        goto default;
                    case 34: // Wrench ("Settings")
                        icon = "\uD83D\uDD27";
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
                        icon = "\uD83D\uDD53";
                        break;
                    case 40: // Envelope with magnifying glass ("Email search")
                        icon = "\u01F50D"; // Magnifying glass
                        break;
                    case 41: // Weird white square with red thing ("Paper flag")
                        goto default;
                    case 42: // RAM DIMM ("Memory")
                        goto default;
                    case 43: // Recycling bin / trash can ("Trash bin")
                        icon = "\uE107";
                        break;
                    case 44: // Sticky note with push pin ("Note")
                        icon = "\uD83D\uDCCD"; // Push pin
                        break;
                    case 45: // Red X ("Expired")
                        icon = "\uE10A";
                        break;
                    case 46: // Circled question mark (blue) ("Info")
                        icon = "\uE11B"; // Stylized question mark
                        break;
                    case 47: // Box ("Package")
                        icon = "\uD83D\uDCE6";
                        break;
                    case 48: // Closed folder ("Folder")
                        icon = "\uD83D\uDCC1";
                        break;
                    case 49: // Open folder ("Folder open")
                        icon = "\uD83D\uDCC2";
                        break;
                    case 50: // Folder with yellow box ("Folder package")
                        goto default;
                    case 51: // Open padlock ("Lock open")
                        icon = "\uD83D\uDD13";
                        break;
                    case 52: // Square with closed padlock ("Paper locked")
                        icon = "\uE131";
                        break;
                    case 53: // Green checkmark ("Checked")
                        icon = "\uE10B";
                        break;
                    case 54: // Fountain pen ("Pen")
                        icon = "\uE1C2"; // Pencil (different from "Edit")
                        break;
                    case 55: // Photograph ("Thumbnail")
                        icon = "\uE155"; // Blank photography thing
                        break;
                    case 56: // Open book ("Book")
                        icon = "\uD83D\uDCD6";
                        break;
                    case 57: // List of details ("List")
                        icon = "\uE179";
                        break;
                    case 58: // Man with key ("User key")
                        goto default;
                    case 59: // Hammer ("Tool")
                        icon = "\uD83D\uDD28";
                        break;
                    case 60: // House ("Home")
                        icon = "\uD83C\uDFE0";
                        break;
                    case 61: // Yellow star ("Star")
                        icon = "\uE1CE";
                        break;
                    case 62: // Penguin ("Tux")
                        icon = "\uD83D\uDC27";
                        break;
                    case 63: // Feather ("Feather")
                        goto default;
                    case 64: // Apple ("Apple")
                        icon = "\uD83C\uDF4E";
                        break;
                    case 65: // Wikipedia logo ("Wiki")
                        icon = "W";
                        break;
                    case 66: // $ sign ("Money")
                        icon = "\uD83D\uDCB2";
                        break;
                    case 67: // Certificate ("Certificate")
                        goto default;
                    case 68: // Cellphone ("BlackBerry")
                        icon = "\uD83D\uDCF1";
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
