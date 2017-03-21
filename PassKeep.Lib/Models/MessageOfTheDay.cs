namespace PassKeep.Lib.Models
{
    /// <summary>
    /// Represents a "Message of the Day" (MOTD) that might be displayed to a user.
    /// </summary>
    public sealed class MessageOfTheDay
    {
        /// <summary>
        /// An instance that should not be displayed.
        /// </summary>
        public static readonly MessageOfTheDay Hidden = new MessageOfTheDay();

        /// <summary>
        /// Constructs a MOTD that should be shown, with the given data.
        /// </summary>
        /// <param name="title">The title of the MOTD.</param>
        /// <param name="body">The body/content of the MOTD.</param>
        /// <param name="dismiss">Text prompt for dismissing the MOTD.</param>
        public MessageOfTheDay(string title, string body, string dismiss)
        {
            ShouldDisplay = true;
            Title = title;
            Body = body;
            DismissText = dismiss;
        }

        /// <summary>
        /// Constructs a MOTD that should not be shown.
        /// </summary>
        private MessageOfTheDay()
        {
            ShouldDisplay = false;
        }

        /// <summary>
        /// The title of the MOTD.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// The body or main content of the MOTD.
        /// </summary>
        public string Body { get; private set; }

        /// <summary>
        /// The text that should be shown as a prompt for dismissing the MOTD.
        /// </summary>
        public string DismissText { get; private set; }

        /// <summary>
        /// Whether the MOTD should be shown at all.
        /// </summary>
        public bool ShouldDisplay { get; private set; }
    }
}
