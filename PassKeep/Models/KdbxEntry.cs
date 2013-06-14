using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using PassKeep.KeePassLib;
using PassKeep.Models.Abstraction;
using Windows.UI;

namespace PassKeep.Models
{
    public class KdbxEntry : KdbxPart, IKeePassEntry
    {
        private IKeePassGroup _parent;
        public IKeePassGroup Parent
        {
            get { return _parent; }
            private set { SetProperty(ref _parent, value); }
        }

        private KeePassUuid _uuid;
        public KeePassUuid Uuid
        {
            get { return _uuid; }
            set { SetProperty(ref _uuid, value); }
        }

        public const int DefaultIconId = 0;
        private int _iconId;
        public int IconID
        {
            get { return _iconId; }
            private set { SetProperty(ref _iconId, value); }
        }

        private KeePassUuid _customIconUuid;
        public KeePassUuid CustomIconUuid
        {
            get { return _customIconUuid; }
            private set { SetProperty(ref _customIconUuid, value); }
        }

        private Color? _foregroundColor;
        public Color? ForegroundColor
        {
            get { return _foregroundColor; }
            private set { SetProperty(ref _foregroundColor, value); }
        }

        private Color? _backgroundColor;
        public Color? BackgroundColor
        {
            get { return _backgroundColor; }
            private set { SetProperty(ref _backgroundColor, value); }
        }

        private string _overrideUrl;
        public string OverrideUrl
        {
            get { return _overrideUrl; }
            set { SetProperty(ref _overrideUrl, value); }
        }

        private string _tags;
        public string Tags
        {
            get { return _tags; }
            set { SetProperty(ref _tags, value); }
        }

        private KdbxTimes _times;
        public KdbxTimes Times
        {
            get { return _times; }
            private set { SetProperty(ref _times, value); }
        }

        private IProtectedString _title;
        public IProtectedString Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private ObservableCollection<IProtectedString> _fields;
        public ObservableCollection<IProtectedString> Fields
        {
            get { return _fields; }
            private set { SetProperty(ref _fields, value); }
        }

        private IProtectedString _username;
        public IProtectedString UserName
        {
            get { return _username; }
            set { SetProperty(ref _username, value); }
        }

        private IProtectedString _password;
        public IProtectedString Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value); }
        }

        private IProtectedString _url;
        public IProtectedString Url
        {
            get { return _url; }
            set { SetProperty(ref _url, value); }
        }

        private IProtectedString _notes;
        public IProtectedString Notes
        {
            get { return _notes; }
            set { SetProperty(ref _notes, value); }
        }

        private ObservableCollection<KdbxBinary> _binaries;
        public ObservableCollection<KdbxBinary> Binaries
        {
            get { return _binaries; }
            private set { SetProperty(ref _binaries, value); }
        }

        public KdbxAutoType AutoType
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

        public KdbxEntry(IKeePassGroup parent, KeePassRng rng, KdbxMetadata metadata)
            : this()
        {
            Parent = parent;
            Uuid = new KeePassUuid();
            IconID = KdbxEntry.DefaultIconId;
            Times = new KdbxTimes();

            KdbxMemoryProtection memProtection = metadata.MemoryProtection;
            Title = new KdbxString("Title", string.Empty, rng, memProtection.ProtectTitle);
            UserName = new KdbxString("UserName", string.Empty, rng, memProtection.ProtectUserName);
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
            Binaries = new ObservableCollection<KdbxBinary>();
        }

        public KdbxEntry(XElement xml, IKeePassGroup parent, KeePassRng rng, KdbxMetadata metadata)
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
            Binaries = new ObservableCollection<KdbxBinary>(binNodes);

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

        public override void PopulateChildren(XElement xml, KeePassRng rng)
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

        public bool MatchesQuery(string query)
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

            Debug.Assert(Uuid != null);
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

        public void Update(IKeePassEntry newEntry)
        {
            Debug.Assert(newEntry != null);
            if (newEntry == null)
            {
                throw new ArgumentNullException("entry");
            }

            if (History == null)
            {
                History = new KdbxHistory(_metadata);
            }
            History.Add(this);

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

            Fields = newEntry.Fields;

            Binaries = newEntry.Binaries;
            AutoType = newEntry.AutoType;

            Times.LastModificationTime = DateTime.Now;
        }
    }
}
