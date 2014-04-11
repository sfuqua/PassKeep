using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.UI;
using PassKeep.KeePassLib;

namespace PassKeep.Models
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
            DbColor = null;
            MasterKeyChanged = DateTime.Now;
            MasterKeyChangeRec = -1;
            MasterKeyChangeForce = -1;
            MemoryProtection = new KdbxMemoryProtection();
            CustomIcons = null;
        }

        public KdbxMetadata(XElement xml)
            : base(xml)
        {
            Generator = GetString("Generator");
            HeaderHash = GetString("HeaderHash");
            DatabaseName = GetString("DatabaseName");
            DatabaseNameChanged = GetDate("DatabaseNameChanged");
            DatabaseDescription = GetString("DatabaseDescription");
            DatabaseDescriptionChanged = GetDate("DatabaseDescriptionChanged");
            DefaultUserName = GetString("DefaultUserName");
            DefaultUserNameChanged = GetDate("DefaultUserNameChanged");
            MaintenanceHistoryDays = GetInt("MaintenanceHistoryDays");
            DbColor = GetNullableColor("Color");
            MasterKeyChanged = GetDate("MasterKeyChanged", false);
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
            RecycleBinChanged = GetDate("RecycleBinChanged");
            EntryTemplatesGroup = GetUuid("EntryTemplatesGroup");
            EntryTemplatesGroupChanged = GetDate("EntryTemplatesGroupChanged");
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

        public override void PopulateChildren(XElement xml, KeePassRng rng)
        {
            xml.Add(GetKeePassNode("Generator", Generator));
            if (HeaderHash != null)
            {
                xml.Add(GetKeePassNode("HeaderHash", HeaderHash));
            }

            xml.Add(
                GetKeePassNode("DatabaseName", DatabaseName),
                GetKeePassNode("DatabaseNameChanged", DatabaseNameChanged),
                GetKeePassNode("DatabaseDescription", DatabaseDescription),
                GetKeePassNode("DatabaseDescriptionChanged", DatabaseDescriptionChanged),
                GetKeePassNode("DefaultUserName", DefaultUserName),
                GetKeePassNode("DefaultUserNameChanged", DefaultUserNameChanged),
                GetKeePassNode("MaintenanceHistoryDays", MaintenanceHistoryDays),
                GetKeePassNode("Color", DbColor),
                GetKeePassNode("MasterKeyChanged", MasterKeyChanged),
                GetKeePassNode("MasterKeyChangeRec", MasterKeyChangeRec),
                GetKeePassNode("MasterKeyChangeForce", MasterKeyChangeForce),
                MemoryProtection.ToXml(rng)
            );

            if (CustomIcons != null)
            {
                xml.Add(CustomIcons.ToXml(rng));
            }

            xml.Add(
                GetKeePassNode("RecycleBinEnabled", RecycleBinEnabled),
                GetKeePassNode("RecycleBinUUID", RecycleBinUuid),
                GetKeePassNode("RecycleBinChanged", RecycleBinChanged),
                GetKeePassNode("EntryTemplatesGroup", EntryTemplatesGroup),
                GetKeePassNode("EntryTemplatesGroupChanged", EntryTemplatesGroupChanged),
                GetKeePassNode("HistoryMaxItems", HistoryMaxItems),
                GetKeePassNode("HistoryMaxSize", HistoryMaxSize),
                GetKeePassNode("LastSelectedGroup", LastSelectedGroup),
                GetKeePassNode("LastTopVisibleGroup", LastTopVisibleGroup)
            );

            if (Binaries != null)
            {
                xml.Add(Binaries.ToXml(rng));
            }

            if (CustomData != null)
            {
                xml.Add(CustomData.ToXml(rng));
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
