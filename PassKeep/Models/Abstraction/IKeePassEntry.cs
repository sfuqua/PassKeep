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

        Color? ForegroundColor { get; }
        Color? BackgroundColor { get; }

        KdbxAutoType AutoType { get; }
        ObservableCollection<KdbxBinary> Binaries { get; }

        void Update(IKeePassEntry template);
        IKeePassEntry Clone(bool preserveHistory = true);
    }
}