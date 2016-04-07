using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PassKeep.Tests
{
    [TestClass]
    public sealed class TaskNotificationServiceTests : TestClassBase
    {
        private ITaskNotificationService serviceUnderTest;

        public override TestContext TestContext
        {
            get;
            set;
        }

        [TestInitialize]
        public void Init()
        {
            this.serviceUnderTest = new TaskNotificationService();
        }

        /// <summary>
        /// Test that cancellation works for happy path.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Cancellation()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            Task op = GetCancellableTask(cts.Token);

            this.serviceUnderTest.PushOperation(op, cts, AsyncOperationType.Unspecified);
            Assert.IsTrue(this.serviceUnderTest.RequestCancellation());

            await Task.Delay(200);

            Assert.IsTrue(this.serviceUnderTest.CurrentTask.IsCanceled);
        }

        /// <summary>
        /// Test that certain tasks cannot be cancelled.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public void CannotCancel()
        {
            Task delay = Task.Delay(100);
            this.serviceUnderTest.PushOperation(delay, AsyncOperationType.Unspecified);
            Assert.IsFalse(this.serviceUnderTest.RequestCancellation());
            Assert.IsFalse(this.serviceUnderTest.CurrentTask.IsCanceled);
        }

        /// <summary>
        /// Tests what happens with multiple subsequent updates.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task MultiplePushes()
        {
            Task delay = Task.Delay(100);
            this.serviceUnderTest.PushOperation(delay, AsyncOperationType.Unspecified);
            Assert.ThrowsException<InvalidOperationException>(
                () => { this.serviceUnderTest.PushOperation(Task.CompletedTask, AsyncOperationType.Unspecified); }
            );

            await delay;

            // Validate pushing over completed, faulted, and cancelled tasks.
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();

            this.serviceUnderTest.PushOperation(Task.CompletedTask, AsyncOperationType.Unspecified);
            Assert.IsTrue(this.serviceUnderTest.CurrentTask.IsSuccessfullyCompleted);

            this.serviceUnderTest.PushOperation(Task.FromCanceled(cts.Token), AsyncOperationType.Unspecified);
            Assert.IsTrue(this.serviceUnderTest.CurrentTask.IsCanceled);

            this.serviceUnderTest.PushOperation(Task.FromException(new Exception()), AsyncOperationType.Unspecified);
            Assert.IsTrue(this.serviceUnderTest.CurrentTask.IsFaulted);
        }

        private static async Task GetCancellableTask(CancellationToken token)
        {
            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(100);
                token.ThrowIfCancellationRequested();
            }
        }
    }
}
