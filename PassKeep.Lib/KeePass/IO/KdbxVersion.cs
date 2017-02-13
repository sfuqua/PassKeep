namespace PassKeep.Lib.KeePass.IO
{
    /// <summary>
    /// Handles switching parser logic based on the file format.
    /// </summary>
    public enum KdbxVersion
    {
        /// <summary>
        /// Unknown/unspecified version.
        /// </summary>
        Unspecified,

        /// <summary>
        /// Legacy 3.1/3.2 KDBX file format.
        /// </summary>
        Three,

        /// <summary>
        /// Modern (as of KP 2.35) KDBX 4 file format.
        /// </summary>
        Four
    }
}
