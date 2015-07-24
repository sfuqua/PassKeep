using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Models;
using SariphLib.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using Windows.UI;

namespace PassKeep.Lib.KeePass.Dom
{
    public class KdbxEntry : KdbxNode, IKeePassEntry
    {
        public const int DefaultIconId = 0;

        private Color? _foregroundColor;
        public Color? ForegroundColor
        {
            get { return _foregroundColor; }
            private set { TrySetProperty(ref _foregroundColor, value); }
        }

        private Color? _backgroundColor;
        public Color? BackgroundColor
        {
            get { return _backgroundColor; }
            private set { TrySetProperty(ref _backgroundColor, value); }
        }

        private string _overrideUrl;
        public string OverrideUrl
        {
            get { return _overrideUrl; }
            set { TrySetProperty(ref _overrideUrl, value); }
        }

        private string _tags;
        public string Tags
        {
            get { return _tags; }
            set { TrySetProperty(ref _tags, value); }
        }

        private ObservableCollection<IProtectedString> _fields;
        public ObservableCollection<IProtectedString> Fields
        {
            get { return _fields; }
            private set { TrySetProperty(ref _fields, value); }
        }

        private IProtectedString _username;
        public IProtectedString UserName
        {
            get { return _username; }
            set { TrySetProperty(ref _username, value); }
        }

        private IProtectedString _password;
        public IProtectedString Password
        {
            get { return _password; }
            set { TrySetProperty(ref _password, value); }
        }

        private IProtectedString _url;
        public IProtectedString Url
        {
            get { return _url; }
            set { TrySetProperty(ref _url, value); }
        }

        private ObservableCollection<IKeePassBinary> _binaries;
        public ObservableCollection<IKeePassBinary> Binaries
        {
            get { return _binaries; }
            private set { TrySetProperty(ref _binaries, value); }
        }

        public IKeePassAutoType AutoType
        {
            get;
            private set;
        }

        public KdbxHistory History
        {
            get;
            set;
        }

        private KdbxMetadata _metadata;

        public KdbxEntry(IKeePassGroup parent, IRandomNumberGenerator rng, KdbxMetadata metadata)
            : this()
        {
            Dbg.Assert(parent != null);
            if (parent == null)
            {
                throw new ArgumentNullException("parent");
            }

            Dbg.Assert(rng != null);
            if (rng == null)
            {
                throw new ArgumentNullException("rng");
            }

            Dbg.Assert(metadata != null);
            if (metadata == null)
            {
                throw new ArgumentNullException("metadata");
            }

            Parent = parent;
            Uuid = new KeePassUuid();
            IconID = KdbxEntry.DefaultIconId;
            Times = new KdbxTimes();

            KdbxMemoryProtection memProtection = metadata.MemoryProtection;
            Title = new KdbxString("Title", string.Empty, rng, memProtection.ProtectTitle);
            string initialUsername = metadata.DefaultUserName ?? string.Empty;
            UserName = new KdbxString("UserName", initialUsername, rng, memProtection.ProtectUserName);
            Password = new KdbxString("Password", string.Empty, rng, memProtection.ProtectPassword);
            Url = new KdbxString("URL", string.Empty, rng, memProtection.ProtectUrl);
            Notes = new KdbxString("Notes", string.Empty, rng, memProtection.ProtectNotes);
            Tags = string.Empty;
            OverrideUrl = string.Empty;
            _metadata = metadata;
        }

        private KdbxEntry()
        {
            Fields = new ObservableCollection<IProtectedString>();
            Binaries = new ObservableCollection<IKeePassBinary>();
        }

        public KdbxEntry(XElement xml, IKeePassGroup parent, IRandomNumberGenerator rng, KdbxMetadata metadata)
            : base(xml)
        {
            Parent = parent;

            Uuid = GetUuid("UUID");
            IconID = GetInt("IconID");
            CustomIconUuid = GetUuid("CustomIconUUID", false);

            ForegroundColor = GetNullableColor("ForegroundColor");
            BackgroundColor = GetNullableColor("BackgroundColor");
            OverrideUrl = GetString("OverrideURL") ?? string.Empty;
            Tags = GetString("Tags") ?? string.Empty;
            Times = new KdbxTimes(GetNode(KdbxTimes.RootName));

            Fields = new ObservableCollection<IProtectedString>();
            IEnumerable<KdbxString> strings = GetNodes(KdbxString.RootName)
                .Select(x => new KdbxString(x, rng));
            foreach (KdbxString s in strings)
            {
                switch(s.Key)
                {
                    case "Notes":
                        Notes = s;
                        break;
                    case "Password":
                        Password = s;
                        break;
                    case "Title":
                        Title = s;
                        break;
                    case "URL":
                        Url = s;
                        break;
                    case "UserName":
                        UserName = s;
                        break;
                    default:
                        Fields.Add(s);
                        break;
                }
            }

            KdbxMemoryProtection memProtection = metadata.MemoryProtection;
            if (Password == null)
            {
                Password = new KdbxString("Password", string.Empty, rng, memProtection.ProtectPassword);
            }
            if (Title == null)
            {
                Title = new KdbxString("Title", string.Empty, rng, memProtection.ProtectTitle);
            }
            if (Url == null)
            {
                Url = new KdbxString("URL", string.Empty, rng, memProtection.ProtectUrl);
            }
            if (UserName == null)
            {
                UserName = new KdbxString("UserName", string.Empty, rng, memProtection.ProtectUserName);
            }
            if (Notes == null)
            {
                Notes = new KdbxString("Notes", string.Empty, rng, memProtection.ProtectNotes);
            }

            var binNodes = GetNodes(KdbxBinary.RootName).Select(x => new KdbxBinary(x));
            Binaries = new ObservableCollection<IKeePassBinary>(binNodes);

            var autoTypeNode = GetNode(KdbxAutoType.RootName);
            if (autoTypeNode != null)
            {
                AutoType = new KdbxAutoType(autoTypeNode);
            }

            XElement historyElement = GetNode(KdbxHistory.RootName);
            if (historyElement != null)
            {
                History = new KdbxHistory(historyElement, rng, metadata);
            }
            else
            {
                History = null;
            }

            _metadata = metadata;
        }

        public static string RootName
        {
            get { return "Entry"; }
        }

        protected override string rootName
        {
            get { return RootName; }
        }

        public override void PopulateChildren(XElement xml, IRandomNumberGenerator rng)
        {
            xml.Add(
                GetKeePassNode("UUID", Uuid),
                GetKeePassNode("IconID", IconID)
            );

            if (CustomIconUuid != null)
            {
                xml.Add(GetKeePassNode("CustomIconUUID", CustomIconUuid));
            }

            xml.Add(
                GetKeePassNode("ForegroundColor", ToKeePassColor(ForegroundColor)),
                GetKeePassNode("BackgroundColor", ToKeePassColor(BackgroundColor)),
                GetKeePassNode("OverrideURL", OverrideUrl),
                GetKeePassNode("Tags", Tags),
                Times.ToXml(rng)
            );

            foreach(var str in Fields)
            {
                xml.Add(str.ToXml(rng));
            }

            if (Notes != null)
            {
                xml.Add(
                    Notes.ToXml(rng)
                );
            }

            xml.Add(
                Password.ToXml(rng),
                Title.ToXml(rng),
                Url.ToXml(rng),
                UserName.ToXml(rng)
            );

            foreach (var bin in Binaries)
            {
                xml.Add(bin.ToXml(rng));
            }

            if (AutoType != null)
            {
                xml.Add(AutoType.ToXml(rng));
            }

            if (History != null)
            {
                xml.Add(History.ToXml(rng));
            }
        }

        public override bool MatchesQuery(string query)
        {
            string newQuery = query.ToUpperInvariant();

            return Title.ClearValue.ToUpperInvariant().Contains(newQuery) ||
                (Tags != null ?
                    Tags.ToUpperInvariant().Contains(newQuery)
                    : false
                );
        }

        public override bool Equals(object obj)
        {
            KdbxEntry other = obj as KdbxEntry;
            if (other == null)
            {
                return false;
            }

            if (Parent != null)
            {
                if (other.Parent == null || !Parent.Uuid.Equals(other.Parent.Uuid)) { return false; }
            }
            else
            {
                if (other.Parent != null) { return false; }
            }

            Dbg.Assert(Uuid != null);
            if (!Uuid.Equals(other.Uuid))
            {
                return false;
            }

            if (IconID != other.IconID)
            {
                return false;
            }

            if (CustomIconUuid != null)
            {
                if (!CustomIconUuid.Equals(other.CustomIconUuid)) { return false; }
            }
            else
            {
                if (other.CustomIconUuid != null) { return false; }
            }

            if (ForegroundColor != other.ForegroundColor || BackgroundColor != other.BackgroundColor)
            {
                return false;
            }

            if (OverrideUrl != other.OverrideUrl)
            {
                return false;
            }

            if (Tags != other.Tags)
            {
                return false;
            }

            if (!Times.Equals(other.Times))
            {
                return false;
            }

            if (!Title.Equals(other.Title) ||
                !UserName.Equals(other.UserName) ||
                !Password.Equals(other.Password) ||
                !Url.Equals(other.Url))
            {
                return false;
            }

            if (Notes != null)
            {
                if (!Notes.Equals(other.Notes)) { return false; }
            }
            else
            {
                if (other.Notes != null) { return false; }
            }

            if (Fields.Count != other.Fields.Count)
            {
                return false;
            }

            for (int i = 0; i < Fields.Count; i++)
            {
                if (!Fields[i].Equals(other.Fields[i]))
                {
                    return false;
                }
            }

            if (Binaries.Count != other.Binaries.Count)
            {
                return false;
            }

            for (int i = 0; i < Binaries.Count; i++)
            {
                if (!Binaries[i].Equals(other.Binaries[i]))
                {
                    return false;
                }
            }

            if (AutoType != null)
            {
                if (!AutoType.Equals(other.AutoType)) { return false; }
            }
            else
            {
                if (other.AutoType != null) { return false; }
            }

            if (History != null)
            {
                if (!History.Equals(other.History)) { return false; }
            }
            else
            {
                if (other.History != null) { return false; }
            }

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public IKeePassEntry Clone(bool preserveHistory = true)
        {
            KdbxEntry clone = new KdbxEntry();
            clone.Parent = this.Parent;
            clone.Uuid = this.Uuid.Clone();
            clone.IconID = this.IconID;
            if (this.CustomIconUuid != null)
            {
                clone.CustomIconUuid = this.CustomIconUuid.Clone();
            }
            else
            {
                clone.CustomIconUuid = null;
            }
            clone.ForegroundColor = this.ForegroundColor;
            clone.BackgroundColor = this.BackgroundColor;
            clone.OverrideUrl = this.OverrideUrl;
            clone.Tags = this.Tags;
            clone.Times = this.Times.Clone();
            if (Title != null)
            {
                clone.Title = this.Title.Clone();
            }
            else
            {
                clone.Title = null;
            }
            clone.Fields = new ObservableCollection<IProtectedString>(this.Fields.Select(f => f.Clone()));
            if (UserName != null)
            {
                clone.UserName = this.UserName.Clone();
            }
            else
            {
                clone.UserName = null;
            }
            if (Password != null)
            {
                clone.Password = this.Password.Clone();
            }
            else
            {
                clone.Password = null;
            }
            if (Url != null)
            {
                clone.Url = this.Url.Clone();
            }
            else
            {
                clone.Url = null;
            }
            if (Notes != null)
            {
                clone.Notes = this.Notes.Clone();
            }
            else
            {
                clone.Notes = null;
            }
            clone.Binaries = this.Binaries;
            clone.AutoType = this.AutoType;
            if (preserveHistory && this.History != null)
            {
                clone.History = this.History.Clone();
            }
            else
            {
                clone.History = null;
            }
            clone._metadata = _metadata;
            return clone;
        }

        public void SyncTo(IKeePassEntry newEntry, bool isUpdate = true)
        {
            Dbg.Assert(newEntry != null);
            if (newEntry == null)
            {
                throw new ArgumentNullException(nameof(newEntry));
            }

            if (History == null)
            {
                History = new KdbxHistory(_metadata);
            }

            IconID = newEntry.IconID;
            CustomIconUuid = newEntry.CustomIconUuid;
            ForegroundColor = newEntry.ForegroundColor;
            BackgroundColor = newEntry.BackgroundColor;
            OverrideUrl = newEntry.OverrideUrl;
            Tags = newEntry.Tags;

            Title = (newEntry.Title != null ? newEntry.Title.Clone() : null);
            UserName = (newEntry.UserName != null ? newEntry.UserName.Clone() : null);
            Password = (newEntry.Password != null ? newEntry.Password.Clone() : null);
            Url = (newEntry.Url != null ? newEntry.Url.Clone() : null);
            Notes = (newEntry.Notes != null ? newEntry.Notes.Clone() : null);

            /*Fields.Clear();
            foreach(IProtectedString str in newEntry.Fields.Select(f => f.Clone()))
            {
                Fields.Add(str);
            }*/
            Fields = new ObservableCollection<IProtectedString>(newEntry.Fields.Select(f => f.Clone()));

            Binaries = newEntry.Binaries;
            AutoType = newEntry.AutoType;

            this.Times.SyncTo(newEntry.Times);

            if (isUpdate)
            {
                History.Add(this);
                Times.LastModificationTime = DateTime.Now;
            }
        }
    }
}
