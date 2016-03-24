using System;
using PassKeep.Lib.Contracts.Providers;

namespace PassKeep.Tests.Mocks
{
    /// <summary>
    /// Test class for <see cref="IMotdProvider"/>.
    /// </summary>
    public class MockMotdProvider : IMotdProvider
    {
        /// <summary>
        /// True by default.
        /// </summary>
        public bool ShouldDisplay
        {
            get;
            private set;
        } = true;

        /// <summary>
        /// The string "Body".
        /// </summary>
        /// <returns>"Body"</returns>
        public string GetBody()
        {
            return "Body";
        }

        /// <summary>
        /// The string "Dismiss".
        /// </summary>
        /// <returns>"Dismiss"</returns>
        public string GetDismiss()
        {
            return "Dismiss";
        }

        /// <summary>
        /// The string "Title".
        /// </summary>
        /// <returns>"Title"</returns>
        public string GetTitle()
        {
            return "Title";
        }

        /// <summary>
        /// Sets <see cref="ShouldDisplay"/> to false.
        /// </summary>
        public void MarkAsDisplayed()
        {
            this.ShouldDisplay = false;
        }
    }
}
