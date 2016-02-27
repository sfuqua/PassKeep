using PassKeep.Lib.Contracts.Providers;

namespace PassKeep.Lib.Providers
{
    public sealed class ResourceBasedMotdProvider : IMotdProvider
    {
        private readonly IResourceProvider resourceProvider;

        /// <summary>
        /// Instantiates the provider around the specified <see cref="IResourceProvider"/>.
        /// </summary>
        /// <param name="provider">The <see cref="IResourceProvider"/> from which to fetch
        /// MOTD strings.</param>
        public ResourceBasedMotdProvider(IResourceProvider provider)
        {
            this.resourceProvider = provider;
        }

        /// <summary>
        /// Provides the title of the message-of-the-day.
        /// </summary>
        /// <returns>A title.</returns>
        public string GetTitle()
        {
            return this.resourceProvider.GetString("Title");
        }

        /// <summary>
        /// Provides a body for the message-of-the-day.
        /// </summary>
        /// <returns>The body.</returns>
        public string GetBody()
        {
            return this.resourceProvider.GetString("Body");
        }

        /// <summary>
        /// Provides a description of an action that will dismiss the message-of-the-day.
        /// </summary>
        /// <returns>Dismissal text.</returns>
        public string GetDismiss()
        {
            return this.resourceProvider.GetString("Dismiss");
        }
    }
}
