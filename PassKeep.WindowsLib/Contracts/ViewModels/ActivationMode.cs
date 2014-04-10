namespace PassKeep.Lib.Contracts.ViewModels
{
    /// <summary>
    /// How the app was activated.
    /// </summary>
    public enum ActivationMode
    {
        /// <summary>
        /// Activated via a standard app launch.
        /// </summary>
        Regular,

        /// <summary>
        /// Activated via global search.
        /// </summary>
        Search,

        /// <summary>
        /// Activated via opening a file.
        /// </summary>
        File
    }
}
