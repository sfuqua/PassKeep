namespace PassKeep.Lib.Contracts.Providers
{
    /// <summary>
    /// Provides an interface for fetching localized resources.
    /// </summary>
    public interface IResourceProvider
    {
        /// <summary>
        /// Returns a string resource given a lookup.
        /// </summary>
        /// <param name="key">The resource to fetch.</param>
        /// <returns>The fetched resource.</returns>
        string GetString(string key);
    }
}
