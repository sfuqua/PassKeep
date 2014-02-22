using PassKeep.Lib.EventArgClasses;
using SariphLib.Mvvm;
using System;
using Windows.Storage;

namespace PassKeep.Lib.Contracts.ViewModels
{
    public interface IDatabaseUnlockViewModel
    {
        StorageFile CandidateFile { get; set; }
        bool IsSampleFile { get; set; }

        string Password { get; set; }
        StorageFile KeyFile { get; set; }

        ActionCommand UnlockCommand { get; }
        event EventHandler<CancelableEventArgs> StartedUnlocking;
        event EventHandler StoppedUnlocking;

        event EventHandler<DocumentReadyEventArgs> DocumentReady;

        bool HasGoodHeader { get; }
    }
}
