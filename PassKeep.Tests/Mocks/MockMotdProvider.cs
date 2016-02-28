using PassKeep.Lib.Contracts.Providers;

namespace PassKeep.Tests.Mocks
{
    /// <summary>
    /// Test class for <see cref="IMotdProvider"/>.
    /// </summary>
    public class MockMotdProvider : IMotdProvider
    {
        /// <summary>
        /// True
        /// </summary>
        public bool ShouldDisplay
        {
            get
            {
                return true;
            }
        }

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
    }
}
