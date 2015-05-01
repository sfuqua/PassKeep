using PassKeep.Lib.Contracts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PassKeep.Lib.KeePass.Dom
{
    public abstract class KdbxNode : KdbxPart, IKeePassNode
    {
        private KeePassUuid _uuid;
        public KeePassUuid Uuid
        {
            get { return _uuid; }
            set { TrySetProperty(ref _uuid, value); }
        }

        private IProtectedString _title;
        public IProtectedString Title
        {
            get { return _title; }
            set { TrySetProperty(ref _title, value); }
        }

        private IProtectedString _notes;
        public IProtectedString Notes
        {
            get { return _notes; }
            set { TrySetProperty(ref _notes, value); }
        }

        private IKeePassGroup _parent;
        public IKeePassGroup Parent
        {
            get { return _parent; }
            protected set { TrySetProperty(ref _parent, value); }
        }

        private int _iconId;
        public int IconID
        {
            get { return _iconId; }
            protected set { TrySetProperty(ref _iconId, value); }
        }

        private KeePassUuid _customIconUuid;
        public KeePassUuid CustomIconUuid
        {
            get { return _customIconUuid; }
            protected set { TrySetProperty(ref _customIconUuid, value); }
        }

        private IKeePassTimes _times;
        public IKeePassTimes Times
        {
            get { return _times; }
            protected set { TrySetProperty(ref _times, value); }
        }

        public bool HasAncestor(IKeePassGroup group)
        {
            if (group == null)
            {
                throw new ArgumentNullException("group");
            }

            IKeePassGroup currentAncestor = Parent;
            while (currentAncestor != null)
            {
                if (currentAncestor.Uuid.Equals(group.Uuid))
                {
                    return true;
                }

                currentAncestor = currentAncestor.Parent;
            }

            return false;
        }

        public abstract bool MatchesQuery(string query);

        protected KdbxNode() { }
        protected KdbxNode(XElement xml)
            : base(xml) { }
    }
}
