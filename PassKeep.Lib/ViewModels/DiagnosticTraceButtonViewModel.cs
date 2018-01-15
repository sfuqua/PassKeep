// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.ViewModels;
using SariphLib.Diagnostics;
using SariphLib.Files;
using SariphLib.Mvvm;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation.Diagnostics;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.System.Profile;

namespace PassKeep.Lib.ViewModels
{
    public sealed class DiagnosticTraceButtonViewModel : AbstractViewModel, IDiagnosticTraceButtonViewModel
    {
        private readonly object stateLock = new object();

        private readonly IFolderPickerService folderPicker;
        private readonly IEventLogger logger;
        private readonly IEventTracer tracer;
        private readonly AsyncActionCommand traceCommand;

        private readonly string startTraceLabel;
        private readonly string stopTraceLabel;

        private bool isBusy;
        private bool isTracing;

        public DiagnosticTraceButtonViewModel(IFolderPickerService folderPicker, IEventLogger logger, IEventTracer tracer, string startTraceLabel, string stopTraceLabel)
        {
            this.isBusy = false;
            this.folderPicker = folderPicker ?? throw new ArgumentNullException(nameof(folderPicker));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
            this.traceCommand = new AsyncActionCommand(() => !this.isBusy, ToggleTrace);

            this.startTraceLabel = startTraceLabel;
            this.stopTraceLabel = stopTraceLabel;

            IsTracing = false;
        }

        /// <summary>
        /// Whether a trace is currently running.
        /// </summary>
        public bool IsTracing
        {
            get { return this.isTracing; }
            private set
            {
                if (TrySetProperty(ref this.isTracing, value))
                {
                    OnPropertyChanged(nameof(Label));
                }
            }
        }

        /// <summary>
        /// The label to display to the view based on the current state.
        /// </summary>
        public string Label { get { return IsTracing ? this.stopTraceLabel : this.startTraceLabel; } }

        /// <summary>
        /// Starts/stops a trace based on the current state.
        /// </summary>
        public IAsyncCommand Command { get { return this.traceCommand; } }

        private async Task ToggleTrace()
        {
            lock (this.stateLock)
            {
                if (this.isBusy)
                {
                    throw new InvalidOperationException();
                }

                this.isBusy = true;
                this.traceCommand.RaiseCanExecuteChanged();
            }

            try
            {
                if (!IsTracing)
                {
                    PackageVersion pkgVersion = Package.Current.Id.Version;
                    string appVersion = $"{pkgVersion.Major}.{pkgVersion.Minor}.{pkgVersion.Revision}";
                    string deviceFamily = AnalyticsInfo.VersionInfo.DeviceFamily;

                    // Get the OS version number
                    string deviceFamilyVersion = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
                    ulong version = ulong.Parse(deviceFamilyVersion);
                    ulong majorVersion = (version & 0xFFFF000000000000L) >> 48;
                    ulong minorVersion = (version & 0x0000FFFF00000000L) >> 32;
                    ulong buildVersion = (version & 0x00000000FFFF0000L) >> 16;
                    ulong revisionVersion = (version & 0x000000000000FFFFL);
                    string systemVersion = $"{majorVersion}.{minorVersion}.{buildVersion}.{revisionVersion}";

                    IsTracing = true;
                    this.tracer.StartTrace();

                    LoggingFields fields = new LoggingFields();
                    fields.AddString("AppVersion", appVersion);
                    fields.AddString("DeviceFamily", deviceFamily);
                    fields.AddString("SystemVersion", systemVersion);

                    this.logger.LogEvent("TraceStarted", fields, EventVerbosity.Critical);
                    return;
                }
                else
                {
                    // XXX hardcoded values
                    IsTracing = false;
                    this.logger.LogEvent("TraceStopped", EventVerbosity.Critical);

                    StorageFolder tempFolder = ApplicationData.Current.TemporaryFolder;
                    ITestableFile file = await this.tracer.StopTraceAndSaveAsync(tempFolder, $"passkeep-{Guid.NewGuid()}.etl");
                    IStorageFolder finalFolder = await this.folderPicker.PickFolderAsync();
                    if (finalFolder == null)
                    {
                        try
                        {
                            // Try to clean up if possible
                            await file.AsIStorageFile.DeleteAsync();
                        }
                        finally { }
                        return;
                    }

                    await file.AsIStorageFile.MoveAsync(finalFolder);
                    await this.folderPicker.LaunchFolderWithSelectionAsync(finalFolder, file);
                }
            }
            finally
            {
                lock (this.stateLock)
                {
                    this.isBusy = false;
                    this.traceCommand.RaiseCanExecuteChanged();
                }
            }
        }
    }
}
