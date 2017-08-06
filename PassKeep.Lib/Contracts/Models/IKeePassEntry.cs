// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

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

        IKeePassHistory History { get; }

        Color? ForegroundColor { get; }
        Color? BackgroundColor { get; }

        IKeePassAutoType AutoType { get; }
        ObservableCollection<IKeePassBinAttachment> Binaries { get; }

        void SyncTo(IKeePassEntry template, bool updateModificationTime = true);
        IKeePassEntry Clone(bool preserveHistory = true);
    }
}