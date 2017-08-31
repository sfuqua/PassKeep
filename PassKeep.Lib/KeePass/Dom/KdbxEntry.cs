// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.KeePass.IO;
using SariphLib.Diagnostics;
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

        private readonly bool isHistoryEntry;

        private Color? _foregroundColor;
        public Color? ForegroundColor
        {
            get { return this._foregroundColor; }
            private set { TrySetProperty(ref this._foregroundColor, value); }
        }

        private Color? _backgroundColor;
        public Color? BackgroundColor
        {
            get { return this._backgroundColor; }
            private set { TrySetProperty(ref this._backgroundColor, value); }
        }

        private string _overrideUrl;
        public string OverrideUrl
        {
            get { return this._overrideUrl; }
            set { TrySetProperty(ref this._overrideUrl, value); }
        }

        private string _tags;
        public string Tags
        {
            get { return this._tags; }
            set { TrySetProperty(ref this._tags, value); }
        }

        private ObservableCollection<IProtectedString> _fields;
        public ObservableCollection<IProtectedString> Fields
        {
            get { return this._fields; }
            private set { TrySetProperty(ref this._fields, value); }
        }

        private IProtectedString _username;
        public IProtectedString UserName
        {
            get { return this._username; }
            set { TrySetProperty(ref this._username, value); }
        }

        private IProtectedString _password;
        public IProtectedString Password
        {
            get { return this._password; }
            set { TrySetProperty(ref this._password, value); }
        }

        private IProtectedString _url;
        public IProtectedString Url
        {
            get { return this._url; }
            set { TrySetProperty(ref this._url, value); }
        }

        private ObservableCollection<IKeePassBinAttachment> _binaries;
        public ObservableCollection<IKeePassBinAttachment> Binaries
        {
            get { return this._binaries; }
            private set { TrySetProperty(ref this._binaries, value); }
        }

        public IKeePassAutoType AutoType
        {
            get;
            private set;
        }

        private IKeePassHistory history;
        public IKeePassHistory History
        {
            get
            {
                DebugHelper.Assert(this.history == null || !this.isHistoryEntry);
                return this.isHistoryEntry ? null : this.history;
            }
            set
            {
                DebugHelper.Assert(value == null || !this.isHistoryEntry);
                this.history = value;
            }
        }

        private KdbxMetadata _metadata;

        public KdbxEntry(IKeePassGroup parent, IRandomNumberGenerator rng, KdbxMetadata metadata)
            : this(false)
        {
            DebugHelper.Assert(parent != null);
            DebugHelper.Assert(rng != null);
            if (rng == null)
            {
                throw new ArgumentNullException("rng");
            }

            DebugHelper.Assert(metadata != null);
            if (metadata == null)
            {
                throw new ArgumentNullException("metadata");
            }

            Parent = parent ?? throw new ArgumentNullException("parent");
            Uuid = new KeePassUuid();
            IconID = KdbxEntry.DefaultIconId;
            Times = new KdbxTimes();
            History = new KdbxHistory(metadata);

            KdbxMemoryProtection memProtection = metadata.MemoryProtection;
            Title = new KdbxString("Title", string.Empty, rng, memProtection.ProtectTitle);
            string initialUsername = metadata.DefaultUserName ?? string.Empty;
            UserName = new KdbxString("UserName", initialUsername, rng, memProtection.ProtectUserName);
            Password = new KdbxString("Password", string.Empty, rng, memProtection.ProtectPassword);
            Url = new KdbxString("URL", string.Empty, rng, memProtection.ProtectUrl);
            Notes = new KdbxString("Notes", string.Empty, rng, memProtection.ProtectNotes);
            Tags = string.Empty;
            OverrideUrl = string.Empty;
            this._metadata = metadata;
        }

        private KdbxEntry(bool isHistoryEntry)
        {
            this.isHistoryEntry = isHistoryEntry;
            Fields = new ObservableCollection<IProtectedString>();
            Binaries = new ObservableCollection<IKeePassBinAttachment>();
        }

        /// <summary>
        /// Helper that deserializes an entry as a history entry (no parent).
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="rng"></param>
        /// <param name="metadata"></param>
        /// <param name="parameters"></param>
        public KdbxEntry(XElement xml, IRandomNumberGenerator rng, KdbxMetadata metadata, KdbxSerializationParameters parameters)
            : this(xml, null, rng, metadata, parameters)
        {
            this.isHistoryEntry = true;
            History = null;
        }

        public KdbxEntry(XElement xml, IKeePassGroup parent, IRandomNumberGenerator rng, KdbxMetadata metadata, KdbxSerializationParameters parameters)
            : base(xml, parameters)
        {
            Parent = parent;

            ForegroundColor = GetNullableColor("ForegroundColor");
            BackgroundColor = GetNullableColor("BackgroundColor");
            OverrideUrl = GetString("OverrideURL") ?? string.Empty;
            Tags = GetString("Tags") ?? string.Empty;
            
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

            IEnumerable<KdbxBinAttachment> binNodes = GetNodes(KdbxBinAttachment.RootName).Select(x => new KdbxBinAttachment(x, metadata, parameters));
            Binaries = new ObservableCollection<IKeePassBinAttachment>(binNodes);

            XElement autoTypeNode = GetNode(KdbxAutoType.RootName);
            if (autoTypeNode != null)
            {
                AutoType = new KdbxAutoType(autoTypeNode);
            }

            XElement historyElement = GetNode(KdbxHistory.RootName);
            if (historyElement != null)
            {
                History = new KdbxHistory(historyElement, rng, metadata, parameters);
            }
            else
            {
                History = new KdbxHistory(metadata);
            }

            this._metadata = metadata;
        }

        public static string RootName
        {
            get { return "Entry"; }
        }

        protected override string rootName
        {
            get { return RootName; }
        }

        public override void PopulateChildren(XElement xml, IRandomNumberGenerator rng, KdbxSerializationParameters parameters)
        {
            xml.Add(
                GetKeePassNode("UUID", Uuid, parameters),
                GetKeePassNode("IconID", IconID, parameters)
            );

            if (CustomIconUuid != null)
            {
                xml.Add(GetKeePassNode("CustomIconUUID", CustomIconUuid, parameters));
            }

            xml.Add(
                GetKeePassNode("ForegroundColor", ToKeePassColor(ForegroundColor), parameters),
                GetKeePassNode("BackgroundColor", ToKeePassColor(BackgroundColor), parameters),
                GetKeePassNode("OverrideURL", OverrideUrl, parameters),
                GetKeePassNode("Tags", Tags, parameters),
                Times.ToXml(rng, parameters)
            );

            foreach (IProtectedString str in Fields)
            {
                xml.Add(str.ToXml(rng, parameters));
            }

            if (Notes != null)
            {
                xml.Add(
                    Notes.ToXml(rng, parameters)
                );
            }

            xml.Add(
                Password.ToXml(rng, parameters),
                Title.ToXml(rng, parameters),
                Url.ToXml(rng, parameters),
                UserName.ToXml(rng, parameters)
            );

            foreach (IKeePassBinAttachment bin in Binaries)
            {
                xml.Add(bin.ToXml(rng, parameters));
            }

            if (AutoType != null)
            {
                xml.Add(AutoType.ToXml(rng, parameters));
            }

            if (History != null)
            {
                DebugHelper.Assert(!this.isHistoryEntry);
                xml.Add(History.ToXml(rng, parameters));
            }

            if (CustomData != null)
            {
                xml.Add(CustomData.ToXml(rng, parameters));
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

            DebugHelper.Assert(Uuid != null);
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
                DebugHelper.Assert(!this.isHistoryEntry);
                if (!History.Equals(other.History)) { return false; }
            }
            else
            {
                if (other.History != null) { return false; }
            }

            if (CustomData != null)
            {
                if (!CustomData.Equals(other.CustomData)) { return false; }
            }
            else
            {
                if (other.CustomData != null) { return false; }
            }

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public IKeePassEntry Clone(bool preserveHistory = true)
        {
            KdbxEntry clone = new KdbxEntry(!preserveHistory)
            {
                Parent = Parent,
                Uuid = Uuid.Clone(),
                IconID = IconID
            };
            if (CustomIconUuid != null)
            {
                clone.CustomIconUuid = CustomIconUuid.Clone();
            }
            else
            {
                clone.CustomIconUuid = null;
            }
            clone.ForegroundColor = ForegroundColor;
            clone.BackgroundColor = BackgroundColor;
            clone.OverrideUrl = OverrideUrl;
            clone.Tags = Tags;
            clone.Times = Times.Clone();
            if (Title != null)
            {
                clone.Title = Title.Clone();
            }
            else
            {
                clone.Title = null;
            }
            clone.Fields = new ObservableCollection<IProtectedString>(Fields.Select(f => f.Clone()));
            if (UserName != null)
            {
                clone.UserName = UserName.Clone();
            }
            else
            {
                clone.UserName = null;
            }
            if (Password != null)
            {
                clone.Password = Password.Clone();
            }
            else
            {
                clone.Password = null;
            }
            if (Url != null)
            {
                clone.Url = Url.Clone();
            }
            else
            {
                clone.Url = null;
            }
            if (Notes != null)
            {
                clone.Notes = Notes.Clone();
            }
            else
            {
                clone.Notes = null;
            }
            clone.Binaries = Binaries;
            clone.AutoType = AutoType;
            if (preserveHistory && History != null)
            {
                clone.History = History.Clone();
            }
            else
            {
                clone.History = null;
            }
            if (CustomData != null)
            {
                clone.CustomData = CustomData.Clone();
            }
            else
            {
                clone.CustomData = null;
            }
            clone._metadata = this._metadata;
            return clone;
        }

        public void SyncTo(IKeePassEntry newEntry, bool isUpdate = true)
        {
            DebugHelper.Assert(newEntry != null);
            if (newEntry == null)
            {
                throw new ArgumentNullException(nameof(newEntry));
            }

            if (isUpdate)
            {
                DebugHelper.Assert(!this.isHistoryEntry);
                if (!this.isHistoryEntry)
                {
                    if (History == null)
                    {
                        History = new KdbxHistory(this._metadata);
                    }

                    History.Add(this);
                }
            }

            IconID = newEntry.IconID;
            CustomIconUuid = newEntry.CustomIconUuid;
            ForegroundColor = newEntry.ForegroundColor;
            BackgroundColor = newEntry.BackgroundColor;
            OverrideUrl = newEntry.OverrideUrl;
            Tags = newEntry.Tags;

            Title = newEntry.Title?.Clone();
            UserName = newEntry.UserName?.Clone();
            Password = newEntry.Password?.Clone();
            Url = newEntry.Url?.Clone();
            Notes = newEntry.Notes?.Clone();

            /*Fields.Clear();
            foreach(IProtectedString str in newEntry.Fields.Select(f => f.Clone()))
            {
                Fields.Add(str);
            }*/
            Fields = new ObservableCollection<IProtectedString>(newEntry.Fields.Select(f => f.Clone()));

            Binaries = newEntry.Binaries;
            AutoType = newEntry.AutoType;

            Times.SyncTo(newEntry.Times);

            if (isUpdate)
            {
                Times.LastModificationTime = DateTime.Now;
            }
        }
    }
}
