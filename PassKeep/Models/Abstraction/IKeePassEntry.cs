using System.Collections.ObjectModel;
using Windows.UI;

namespace PassKeep.Models.Abstraction
{
    public interface IKeePassEntry : IKeePassNode
    {
        IProtectedString Password { get; set; }
        IProtectedString Url { get; set; }
        IProtectedString UserName { get; set; }
        string OverrideUrl { get; set; }
        string Tags { get; set; }

        ObservableCollection<IProtectedString> Fields { get; }

        KdbxHistory History { get; }

        Color? ForegroundColor { get; }
        Color? BackgroundColor { get; }

        KdbxAutoType AutoType { get; }
        ObservableCollection<KdbxBinAttachment> Binaries { get; }

        void SyncTo(IKeePassEntry template, bool updateModificationTime = true);
        IKeePassEntry Clone(bool preserveHistory = true);
    }
}