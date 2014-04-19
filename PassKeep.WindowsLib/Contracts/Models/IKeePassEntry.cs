using System.Collections.ObjectModel;
using Windows.UI;

namespace PassKeep.Lib.Contracts.Models
{
    public interface IKeePassEntry : IKeePassNode
    {
        IProtectedString Password { get; set; }
        IProtectedString Url { get; set; }
        IProtectedString UserName { get; set; }
        string OverrideUrl { get; set; }
        string Tags { get; set; }

        ObservableCollection<IProtectedString> Fields { get; }

        Color? ForegroundColor { get; }
        Color? BackgroundColor { get; }

        IKeePassAutoType AutoType { get; }
        ObservableCollection<IKeePassBinary> Binaries { get; }

        void Update(IKeePassEntry template, bool updateModificationTime = true);
        IKeePassEntry Clone(bool preserveHistory = true);
    }
}