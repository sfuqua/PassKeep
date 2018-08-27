using Microsoft.VisualStudio.TestTools.UnitTesting;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.ViewModels;
using PassKeep.Tests.Mocks;
using SariphLib.Testing;
using System.Threading.Tasks;
using Windows.Storage;

namespace PassKeep.Tests
{
    [TestClass]
    public class DiagnosticTraceButtonTests : TestClassBase
    {
        private const string StartTraceLabel = "Start";
        private const string StopTraceLabel = "Stop";

        public override TestContext TestContext
        {
            get;
            set;
        }

        /// <summary>
        /// Basic tests of ViewModel state during interaction with the trace button.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task TraceToggling()
        {
            MockFolderPickerService folderPicker = new MockFolderPickerService(ApplicationData.Current.TemporaryFolder);
            MockEventLogger logger = new MockEventLogger();
            IDiagnosticTraceButtonViewModel vm = CreateViewModel(folderPicker, logger);

            Assert.AreEqual(StartTraceLabel, vm.Label, "Initial ViewModel label should indicate tracing will start");
            Assert.IsFalse(vm.IsTracing, "ViewModel should not be tracing initially");
            Assert.IsFalse(logger.IsTracing, "Logger should not be tracing initially");
            Assert.IsTrue(vm.Command.CanExecute(null), "ViewModel trace button should be enabled initially");

            int eventsTraced = 0;
            logger.EventTraced += (s, e) =>
            {
                eventsTraced++;
            };

            bool folderLaunched = false;
            folderPicker.FolderLaunched += (s, e) =>
            {
                folderLaunched = true;
            };

            await vm.Command.ExecuteAsync(null);
            Assert.AreEqual(StopTraceLabel, vm.Label, "ViewModel label after starting a trace should indicate tracing will stop");
            Assert.IsTrue(vm.IsTracing, "ViewModel should be tracing after using the trace button");
            Assert.IsTrue(logger.IsTracing, "Logger should be tracing after using the trace button");
            Assert.AreEqual(1, eventsTraced, "Starting a trace should fire an event");
            Assert.IsFalse(folderLaunched, "Folder should not be launched until tracing stops");

            await vm.Command.ExecuteAsync(null);
            Assert.AreEqual(StartTraceLabel, vm.Label, "ViewModel label after ending a trace should indicate tracing will start again");
            Assert.IsFalse(vm.IsTracing, "ViewModel should not be tracing after using the trace button a second time");
            Assert.IsFalse(logger.IsTracing, "Logger should not be tracing after using the trace button a second time");
            Assert.AreEqual(2, eventsTraced, "Stopping a trace should fire a second event");
            Assert.IsTrue(folderLaunched, "Folder should be launched once tracing stops");
        }

        private DiagnosticTraceButtonViewModel CreateViewModel(MockFolderPickerService folderPicker, MockEventLogger logger)
        {
            return new DiagnosticTraceButtonViewModel(
                folderPicker,
                logger,
                logger,
                StartTraceLabel,
                StopTraceLabel
            );
        }
    }
}
