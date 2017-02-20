using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.KeePass.IO;
using PassKeep.Lib.Models;
using System;
using System.Xml.Linq;

namespace PassKeep.Lib.KeePass.Dom
{
    /// <summary>
    /// Represents a collection of <see cref="KdbxBinary"/> objects.
    /// </summary>
    public class KdbxBinaries : KdbxPart
    {
        public static string RootName
        {
            get { return "Binaries"; }
        }

        protected override string rootName
        {
            get { return RootName; }
        }

        public KdbxBinaries(XElement xml)
            : base(xml)
        {
            throw new NotImplementedException();
        }

        public override void PopulateChildren(XElement xml, IRandomNumberGenerator rng, KdbxSerializationParameters parameters) { }

        public override bool Equals(object obj)
        {
            KdbxBinaries other = obj as KdbxBinaries;
            if (other == null)
            {
                return false;
            }

            return false;
            //return XElement.DeepEquals(original, other.original);
        }

        public ProtectedBinary GetById(int id)
        {
            return null;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
