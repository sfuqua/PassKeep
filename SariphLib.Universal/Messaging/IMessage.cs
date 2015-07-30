namespace SariphLib.Messaging
{
    /// <summary>
    /// A publishable message.
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// The name of this message.
        /// </summary>
        string Name { get; }
    }
}
