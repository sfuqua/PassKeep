using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;

namespace PassKeep.KeePassTests
{
    public abstract class TestClassBase
    {
        public TestContext TestContext
        {
            get;
            set;
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
            /// <param name="initDatabase">Whether to initialize the ViewModel's Candidate database file.</param>
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
