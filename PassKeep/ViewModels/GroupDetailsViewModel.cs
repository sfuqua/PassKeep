using PassKeep.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace PassKeep.ViewModels
{
    public class GroupDetailsViewModel : DetailsViewModelBase<KdbxGroup>
    {
        public GroupDetailsViewModel(KdbxGroup group, DatabaseViewModel databaseViewModel, ConfigurationViewModel appSettings, bool isReadOnly = true)
            : base(databaseViewModel, appSettings)
        {
            IsReadOnly = isReadOnly;
            Item = group;
        }

        public override KdbxGroup GetBackup(out int index)
        {
            index = -1;

            for (int i = 0; i < Item.Parent.Groups.Count; i++)
            {
                if (Item.Parent.Groups[i].Uuid.Equals(Item.Uuid))
                {
                    index = i;
                    return Item.Parent.Groups[i];
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
            KdbxGroup backup = GetBackup(out i);

            if (backup == null)
            {
                Item.Parent.Groups.Add(Item);
            }
            else
            {
                Item.Parent.Groups[i].Update(Item);
            }

            if (await DatabaseViewModel.Commit())
            {
                // Successful save
                int bc = DatabaseViewModel.Breadcrumbs.Count;
                if (bc > 1 && DatabaseViewModel.Breadcrumbs[bc - 1].Uuid.Equals(Item.Uuid))
                {
                    DatabaseViewModel.Breadcrumbs.RemoveAt(bc - 1);
                    DatabaseViewModel.Breadcrumbs.Add(Item);
                }
                return true;
            }
            else
            {
                // Restore pre-save state on cancel
                if (backup == null)
                {
                    Debug.Assert(Item.Parent.Groups[i].Uuid.Equals(Item.Uuid));
                    Item.Parent.Groups.RemoveAt(i);
                }
                else
                {
                    Item.Parent.Groups[i] = backup;
                }

                return false;
            }
        }

        public override bool Revert()
        {
            int i;
            KdbxGroup backup = GetBackup(out i);
            if (backup == null)
            {
                return false;
            }
            Item = backup.Clone();
            return true;
        }
    }
}
