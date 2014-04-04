using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.ViewModels;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PassKeep.ViewModels
{
    public class EntryDetailsViewModel : DetailsViewModelBase<IKeePassEntry>, IEntryDetailsViewModel
    {
        public EntryDetailsViewModel(IKeePassEntry entry, DatabaseViewModel databaseViewModel, ConfigurationViewModel appSettings, bool isReadOnly = true)
            : base(databaseViewModel, appSettings)
        {
            IsReadOnly = isReadOnly;
            Item = entry;
        }

        public override IKeePassEntry GetBackup(out int index)
        {
            index = -1;

            for (int i = 0; i < Item.Parent.Entries.Count; i++)
            {
                if (Item.Parent.Entries[i].Uuid.Equals(Item.Uuid))
                {
                    index = i;
                    return Item.Parent.Entries[i];
                }
            }

            return null;
        }

        public async override Task<bool> Save()
        {
            if (IsReadOnly)
            {
                return true;
            }

            int i;
            IKeePassEntry backup = GetBackup(out i);

            if (backup == null)
            {
                Item.Parent.Entries.Add(Item);
            }
            else
            {
                Item.Parent.Entries[i].Update(Item);
            }

            if (await DatabaseViewModel.Commit())
            {
                // Successful save
                if (DatabaseViewModel.BreadcrumbViewModel.ActiveLeaf != null && DatabaseViewModel.BreadcrumbViewModel.ActiveLeaf.Uuid.Equals(Item.Uuid))
                {
                    DatabaseViewModel.BreadcrumbViewModel.ActiveLeaf.Update(Item);
                }
                return true;
            }
            else
            {
                // Restore pre-save state on cancel
                if (backup == null)
                {
                    Debug.Assert(Item.Parent.Entries[i].Uuid.Equals(Item.Uuid));
                    Item.Parent.Entries.RemoveAt(i);
                }
                else
                {
                    Item.Parent.Entries[i] = backup;
                }

                return false;
            }
        }

        public override bool Revert()
        {
            int i;
            IKeePassEntry backup = GetBackup(out i);
            if (backup == null)
            {
                return false;
            }
            Item = backup.Clone();
            return true;
        }
    }
}
