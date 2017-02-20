using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.KeePass.IO;
using System;
using System.Xml.Linq;
using Windows.UI;

namespace PassKeep.Lib.KeePass.Dom
{
    public class KdbxMetadata : KdbxPart
    {
        public static string RootName
        {
            get { return "Meta"; }
        }

        protected override string rootName
        {
            get { return RootName; }
        }

        public const string PKGenerator = "PassKeep";
        public string Generator
        {
            get;
            private set;
        }

        public string HeaderHash
        {
            get;
            set;
        }

        private string _databaseName;
        public string DatabaseName
        {
            get { return _databaseName; }
            private set
            {
                _databaseName = value;
                DatabaseNameChanged = DateTime.Now;
            }
        }

        public DateTime? DatabaseNameChanged
        {
            get;
            private set;
        }

        private string _databaseDescription;
        public string DatabaseDescription
        {
            get { return _databaseDescription; }
            private set
            {
                _databaseDescription = value;
                DatabaseDescriptionChanged = DateTime.Now;
            }
        }

        public DateTime? DatabaseDescriptionChanged
        {
            get;
            private set;
        }

        private string _defaultUserName;
        public string DefaultUserName
        {
            get { return _defaultUserName; }
            private set
            {
                _defaultUserName = value;
                DefaultUserNameChanged = DateTime.Now;
            }
        }

        public DateTime? DefaultUserNameChanged
        {
            get;
            private set;
        }

        public int MaintenanceHistoryDays
        {
            get;
            private set;
        }

        public Color? DbColor
        {
            get;
            private set;
        }

        public DateTime? MasterKeyChanged
        {
            get;
            private set;
        }

        public int MasterKeyChangeRec
        {
            get;
            private set;
        }

        public int MasterKeyChangeForce
        {
            get;
            private set;
        }

        public KdbxMemoryProtection MemoryProtection
        {
            get;
            private set;
        }

        public KdbxCustomIcons CustomIcons
        {
            get;
            private set;
        }

        public bool RecycleBinEnabled
        {
            get;
            private set;
        }

        public KeePassUuid RecycleBinUuid
        {
            get;
            private set;
        }

        public DateTime? RecycleBinChanged
        {
            get;
            private set;
        }

        private KeePassUuid _entryTemplatesGroup;
        public KeePassUuid EntryTemplatesGroup
        {
            get { return _entryTemplatesGroup; }
            private set
            {
                _entryTemplatesGroup = value;
                EntryTemplatesGroupChanged = DateTime.Now;
            }
        }

        public DateTime? EntryTemplatesGroupChanged
        {
            get;
            private set;
        }

        public int HistoryMaxItems
        {
            get;
            private set;
        }

        public int HistoryMaxSize
        {
            get;
            private set;
        }

        public KeePassUuid LastSelectedGroup
        {
            get;
            private set;
        }

        public KeePassUuid LastTopVisibleGroup
        {
            get;
            private set;
        }

        public KdbxBinaries Binaries
        {
            get;
            private set;
        }

        public KdbxCustomData CustomData
        {
            get;
            private set;
        }

        public KdbxMetadata(string databaseName)
        {
            Generator = PKGenerator;
            HeaderHash = null;
            DatabaseName = databaseName;
            DatabaseDescription = null;
            DefaultUserName = null;
            MaintenanceHistoryDays = 365;
            RecycleBinEnabled = false;
            RecycleBinUuid = new KeePassUuid(Guid.Empty);
            RecycleBinChanged = DateTime.Now;
            EntryTemplatesGroup = new KeePassUuid(Guid.Empty);
            EntryTemplatesGroupChanged = DateTime.Now;
            HistoryMaxItems = 10;
            HistoryMaxSize = -1;
            DbColor = null;
            MasterKeyChanged = DateTime.Now;
            MasterKeyChangeRec = -1;
            MasterKeyChangeForce = -1;
            MemoryProtection = new KdbxMemoryProtection();
            CustomIcons = null;
        }

        public KdbxMetadata(XElement xml, KdbxSerializationParameters parameters)
            : base(xml)
        {
            Generator = GetString("Generator");
            HeaderHash = GetString("HeaderHash");
            DatabaseName = GetString("DatabaseName");
            DatabaseNameChanged = GetDate("DatabaseNameChanged", parameters);
            DatabaseDescription = GetString("DatabaseDescription");
            DatabaseDescriptionChanged = GetDate("DatabaseDescriptionChanged", parameters);
            DefaultUserName = GetString("DefaultUserName");
            DefaultUserNameChanged = GetDate("DefaultUserNameChanged", parameters);
            MaintenanceHistoryDays = GetInt("MaintenanceHistoryDays");
            DbColor = GetNullableColor("Color");
            MasterKeyChanged = GetDate("MasterKeyChanged", parameters, false);
            MasterKeyChangeRec = GetInt("MasterKeyChangeRec", -1);
            MasterKeyChangeForce = GetInt("MasterKeyChangeForce", -1);
            MemoryProtection = new KdbxMemoryProtection(GetNode(KdbxMemoryProtection.RootName));

            XElement iconsElement = GetNode(KdbxCustomIcons.RootName);
            if (iconsElement != null)
            {
                CustomIcons = new KdbxCustomIcons(iconsElement);
            }
            else
            {
                CustomIcons = null;
            }

            RecycleBinEnabled = GetBool("RecycleBinEnabled");
            RecycleBinUuid = GetUuid("RecycleBinUUID");
            RecycleBinChanged = GetDate("RecycleBinChanged", parameters);
            EntryTemplatesGroup = GetUuid("EntryTemplatesGroup");
            EntryTemplatesGroupChanged = GetDate("EntryTemplatesGroupChanged", parameters);
            HistoryMaxItems = GetInt("HistoryMaxItems", -1);
            HistoryMaxSize = GetInt("HistoryMaxSize", -1);
            LastSelectedGroup = GetUuid("LastSelectedGroup");
            LastTopVisibleGroup = GetUuid("LastTopVisibleGroup");

            XElement binariesElement = GetNode(KdbxBinaries.RootName);
            if (binariesElement != null)
            {
                Binaries = new KdbxBinaries(binariesElement);
            }
            else
            {
                Binaries = null;
            }

            XElement customDataElement = GetNode(KdbxCustomData.RootName);
            if (customDataElement != null)
            {
                CustomData = new KdbxCustomData(customDataElement);
            }
            else
            {
                CustomData = null;
            }
        }

        public override void PopulateChildren(XElement xml, IRandomNumberGenerator rng, KdbxSerializationParameters parameters)
        {
            xml.Add(GetKeePassNode("Generator", Generator, parameters));
            if (parameters.UseXmlHeaderAuthentication && HeaderHash != null)
            {
                xml.Add(GetKeePassNode("HeaderHash", HeaderHash, parameters));
            }

            xml.Add(
                GetKeePassNode("DatabaseName", DatabaseName, parameters),
                GetKeePassNode("DatabaseNameChanged", DatabaseNameChanged, parameters),
                GetKeePassNode("DatabaseDescription", DatabaseDescription, parameters),
                GetKeePassNode("DatabaseDescriptionChanged", DatabaseDescriptionChanged, parameters),
                GetKeePassNode("DefaultUserName", DefaultUserName, parameters),
                GetKeePassNode("DefaultUserNameChanged", DefaultUserNameChanged, parameters),
                GetKeePassNode("MaintenanceHistoryDays", MaintenanceHistoryDays, parameters),
                GetKeePassNode("Color", DbColor, parameters),
                GetKeePassNode("MasterKeyChanged", MasterKeyChanged, parameters),
                GetKeePassNode("MasterKeyChangeRec", MasterKeyChangeRec, parameters),
                GetKeePassNode("MasterKeyChangeForce", MasterKeyChangeForce, parameters),
                MemoryProtection.ToXml(rng, parameters)
            );

            if (CustomIcons != null)
            {
                xml.Add(CustomIcons.ToXml(rng, parameters));
            }

            xml.Add(
                GetKeePassNode("RecycleBinEnabled", RecycleBinEnabled, parameters),
                GetKeePassNode("RecycleBinUUID", RecycleBinUuid, parameters),
                GetKeePassNode("RecycleBinChanged", RecycleBinChanged, parameters),
                GetKeePassNode("EntryTemplatesGroup", EntryTemplatesGroup, parameters),
                GetKeePassNode("EntryTemplatesGroupChanged", EntryTemplatesGroupChanged, parameters),
                GetKeePassNode("HistoryMaxItems", HistoryMaxItems, parameters),
                GetKeePassNode("HistoryMaxSize", HistoryMaxSize, parameters),
                GetKeePassNode("LastSelectedGroup", LastSelectedGroup, parameters),
                GetKeePassNode("LastTopVisibleGroup", LastTopVisibleGroup, parameters)
            );

            if (parameters.BinariesInXml && Binaries != null)
            {
                xml.Add(Binaries.ToXml(rng, parameters));
            }

            if (CustomData != null)
            {
                xml.Add(CustomData.ToXml(rng, parameters));
            }
        }

        public override bool Equals(object obj)
        {
            KdbxMetadata other = obj as KdbxMetadata;
            if (other == null)
            {
                return false;
            }

            if (Generator != other.Generator)
            {
                return false;
            }

            if (DatabaseName != other.DatabaseName || DatabaseDescription != other.DatabaseDescription)
            {
                return false;
            }

            if (DatabaseNameChanged != other.DatabaseNameChanged || DatabaseDescriptionChanged != other.DatabaseDescriptionChanged)
            {
                return false;
            }

            if (DefaultUserName != other.DefaultUserName || DefaultUserNameChanged != other.DefaultUserNameChanged)
            {
                return false;
            }

            if (MaintenanceHistoryDays != other.MaintenanceHistoryDays)
            {
                return false;
            }

            if (DbColor != other.DbColor)
            {
                return false;
            }

            if (MasterKeyChanged != other.MasterKeyChanged ||
                MasterKeyChangeRec != other.MasterKeyChangeRec ||
                MasterKeyChangeForce != other.MasterKeyChangeForce)
            {
                return false;
            }

            if (!MemoryProtection.Equals(other.MemoryProtection))
            {
                return false;
            }

            if (CustomIcons != null)
            {
                if (!CustomIcons.Equals(other.CustomIcons)) { return false; }
            }
            else
            {
                if (other.CustomIcons != null) { return false; }
            }

            if (RecycleBinEnabled != other.RecycleBinEnabled ||
                !RecycleBinUuid.Equals(other.RecycleBinUuid) ||
                RecycleBinChanged != other.RecycleBinChanged)
            {
                return false;
            }

            if (!EntryTemplatesGroup.Equals(other.EntryTemplatesGroup) || EntryTemplatesGroupChanged != other.EntryTemplatesGroupChanged)
            {
                return false;
            }

            if (HistoryMaxItems != other.HistoryMaxItems || HistoryMaxSize != other.HistoryMaxSize)
            {
                return false;
            }

            if (!LastSelectedGroup.Equals(other.LastSelectedGroup) || !LastTopVisibleGroup.Equals(other.LastTopVisibleGroup))
            {
                return false;
            }

            if (Binaries != null)
            {
                if (!Binaries.Equals(other.Binaries)) { return false; }
            }
            else
            {
                if (other.Binaries != null) { return false; }
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
    }
}
