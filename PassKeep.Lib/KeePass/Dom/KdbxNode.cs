using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.KeePass.IO;
using SariphLib.Infrastructure;
using System;
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
            set { TrySetProperty(ref _iconId, value); }
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

        private KdbxCustomData customData;
        public KdbxCustomData CustomData
        {
            get { return this.customData; }
            protected set { this.customData = value; }
        }

        /// <summary>
        /// <paramref name="newParent"/> adopts this node as its own.
        /// </summary>
        /// <param name="newParent">The group that will adopt this node.</param>
        public void Reparent(IKeePassGroup newParent)
        {
            if (newParent == null)
            {
                throw new ArgumentNullException(nameof(newParent));
            }

            if (Parent != null)
            {
                if (Parent == newParent)
                {
                    return;
                }

                Dbg.Assert(Parent.Children.Contains(this));
                Parent.Children.Remove(this);
            }
            else
            {
                throw new InvalidOperationException("Cannot reparent the root group");
            }

            newParent.Children.Add(this);
            Parent = newParent;
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
        protected KdbxNode(XElement xml, KdbxSerializationParameters parameters)
            : base(xml)
        {
            Uuid = GetUuid("UUID");
            IconID = GetInt("IconID");
            CustomIconUuid = GetUuid("CustomIconUUID", false);
            Times = new KdbxTimes(GetNode(KdbxTimes.RootName), parameters);

            XElement dataElement = GetNode(KdbxCustomData.RootName);
            if (dataElement != null)
            {
                CustomData = new KdbxCustomData(dataElement);
            }
        }
    }
}
