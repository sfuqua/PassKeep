using System;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Models;

namespace PassKeep.Tests.Mocks
{
    /// <summary>
    /// Test class for <see cref="IMotdProvider"/>.
    /// </summary>
    public class MockMotdProvider : IMotdProvider
    {
        public const string TitleText = "Title";
        public const string BodyText = "Body";
        public const string DismissText = "Dismiss";

        private bool shouldDisplay = true;

        public MessageOfTheDay GetMotdForDisplay()
        {
            if (this.shouldDisplay)
            {
                this.shouldDisplay = false;
                return new MessageOfTheDay(TitleText, BodyText, DismissText);
            }
            else
            {
                return MessageOfTheDay.Hidden;
            }
        }
    }
}
