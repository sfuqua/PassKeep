using System.Xml.Linq;
using PassKeep.KeePassLib;

namespace PassKeep.Models.Abstraction
{
    /// <summary>
    /// Represents a class that can be serialized to a KeePass database file.
    /// </summary>
    public interface IKeePassSerializable
    {
        /// <summary>
        /// Serializes and returns this instance using the provided random number generator.
        /// </summary>
        /// <param name="rng">A random number generator with potential state.</param>
        /// <returns>This instance as XML.</returns>
        XElement ToXml(KeePassRng rng);
    }
}
