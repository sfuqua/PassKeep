using PassKeep.Lib.Contracts.KeePass;
using System.Xml.Linq;

namespace PassKeep.Lib.Contracts.Models
{
    /// <summary>
    /// Represents a class that can be serialized to a KeePass document file.
    /// </summary>
    public interface IKeePassSerializable
    {
        /// <summary>
        /// Serializes and returns this instance using the provided random number generator.
        /// </summary>
        /// <param name="rng">A random number generator with potential state.</param>
        /// <returns>This instance as XML.</returns>
        XElement ToXml(IRandomNumberGenerator rng);
    }
}
