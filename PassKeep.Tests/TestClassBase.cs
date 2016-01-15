using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PassKeep.Tests
{
    public abstract class TestClassBase
    {
        public abstract TestContext TestContext
        {
            get;
            set;
        }

        /// <summary>
        /// Returns a Task that completes after the specified time.
        /// </summary>
        /// <param name="milliseconds">The number of seconds to spin the Task.</param>
        /// <returns>An awaitable Task that takes the specified amount of time to complete.</returns>
        protected Task AwaitableTimeout(int milliseconds = 2000)
        {
            return Task.Run(() => new ManualResetEvent(false).WaitOne(milliseconds));
        }

        protected class TestDataAttribute : Attribute
        {
            /// <summary>
            /// Whether to skip initializing a ViewModel for the test altogether.
            /// </summary>
            public bool SkipInitialization
            {
                get;
                private set;
            }

            /// <summary>
            /// Whether to initialize the ViewModel's CandidateFile.
            /// </summary>
            public bool InitDatabase
            {
                get;
                private set;
            }

            /// <summary>
            /// Whether to set the ViewModel's Password.
            /// </summary>
            public bool SetPassword
            {
                get;
                private set;
            }

            /// <summary>
            /// Whether to set the ViewModel's KeyFile.
            /// </summary>
            public bool SetKeyFile
            {
                get;
                private set;
            }

            /// <summary>
            /// Whether to initialize the ViewModel's sample flag.
            /// </summary>
            public bool InitSample
            {
                get;
                private set;
            }

            /// <summary>
            /// Simple initialization constructor.
            /// </summary>
            /// <param name="skipInitialization">Whether to skip initializating a ViewModel altogether.</param>
            /// <param name="initDatabase">Whether to initialize the ViewModel's Candidate document file.</param>
            /// <param name="setPassword">Whether to set the ViewModel's Password.</param>
            /// <param name="setKeyFile">Whether to set the ViewModel's KeyFile.</param>
            /// <param name="initSample">Whether to initialize the ViewModel's sample flag.</param>
            public TestDataAttribute(
                bool skipInitialization = false,
                bool initDatabase = true,
                bool setPassword = false,
                bool setKeyFile = false,
                bool initSample = false
            )
            {
                this.SkipInitialization = skipInitialization;
                this.InitDatabase = initDatabase;
                this.SetPassword = setPassword;
                this.SetKeyFile = setKeyFile;
                this.InitSample = initSample;
            }
        }
    }
}
