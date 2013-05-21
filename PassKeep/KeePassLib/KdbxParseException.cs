using System;

namespace PassKeep.KeePassLib
{
    public class KdbxParseException : Exception
    {
        public KeePassError Error
        {
            get;
            private set;
        }

        public KdbxParseException(KeePassError error)
        {
            Error = error;
        }

        public KdbxParseException(KdbxParseError error)
            : this(new KeePassError(error))
        { }
    }
}
