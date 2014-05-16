using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using PassKeep.Lib.KeePass.Dom;
using PassKeep.Lib.KeePass.IO;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace PassKeep.KeePassTests
{
    public abstract class DatabasePersistenceViewModelTests<T> : TestClassBase
        where T : IDatabasePersistenceViewModel
    {
        private const string StructureTestingDatabase = "StructureTesting.kdbx";

        protected T viewModel;

        protected async Task ValidateCancelledSave()
        {
            TaskCompletionSource<object> tcsStart = new TaskCompletionSource<object>();
            TaskCompletionSource<object> tcsStop = new TaskCompletionSource<object>();

            EventHandler<CancellableEventArgs> startedEventHandler = (s, e) =>
            {
                e.Cts.Cancel();
                tcsStart.SetResult(null);
            };

            EventHandler stoppedEventHandler = (s, e) =>
            {
                tcsStop.SetResult(null);
            };

            this.viewModel.StartedSave += startedEventHandler;
            this.viewModel.StoppedSave += stoppedEventHandler;

            Assert.IsFalse(await this.viewModel.TrySave(), "TrySave should return true if it succeeds");
            await tcsStart.Task;
            await tcsStop.Task;
        }

        protected async Task ValidateSave()
        {
            TaskCompletionSource<object> tcsStart = new TaskCompletionSource<object>();
            TaskCompletionSource<object> tcsStop = new TaskCompletionSource<object>();

            EventHandler<CancellableEventArgs> startedEventHandler = (s, e) =>
            {
                tcsStart.SetResult(null);
            };

            EventHandler stoppedEventHandler = (s, e) =>
            {
                tcsStop.SetResult(null);
            };

            this.viewModel.StartedSave += startedEventHandler;
            this.viewModel.StoppedSave += stoppedEventHandler;

            Assert.IsTrue(await this.viewModel.TrySave(), "TrySave should return true if it succeeds");
            await tcsStart.Task;
            await tcsStop.Task;
        }
    }
}
