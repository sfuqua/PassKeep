// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.KeePass.IO;
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
        /// <param name="parameters">Parameters used to control serialization behavior.</param>
        /// <returns>This instance as XML.</returns>
        XElement ToXml(IRandomNumberGenerator rng, KdbxSerializationParameters parameters);
    }
}
