using PassKeep.Models.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace PassKeep.ViewModels
{
    public class GroupDetailsViewModel : DetailsViewModelBase<IKeePassGroup>
    {
        public GroupDetailsViewModel(IKeePassGroup group, DatabaseViewModel databaseViewModel, ConfigurationViewModel appSettings, bool isReadOnly = true)
            : base(databaseViewModel, appSettings)
        {
            IsReadOnly = isReadOnly;
            Item = group;
        }

        public override IKeePassGroup GetBackup(out int index)
        {
            index = -1;

            if (Item.Parent != null)
            {
                for (int i = 0; i < Item.Parent.Children.Count; i++)
                {
                    if (Item.Parent.Children[i].Uuid.Equals(Item.Uuid))
                    {
                        index = i;
                        return (IKeePassGroup)Item.Parent.Children[i];
                    }
                }
            }
            else
            {
                return DatabaseViewModel.Document.Root.DatabaseGroup;
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
            IKeePassGroup backup = GetBackup(out i);

            if (Item.Parent != null)
            {
                // If this isn't a top-level node, find the original from the parent and update it.
                if (backup == null)
                {
                    Item.Parent.Groups.Add(Item);
                }
                else
                {
                    Item.Parent.Groups[i].SyncTo(Item);
                }
            }
            else
            {
                // Otherwise update the root DatabaseGroup itself.
                DatabaseViewModel.Document.Root.DatabaseGroup.SyncTo(Item);
            }

            if (await DatabaseViewModel.Commit())
            {
                // Successful save
                IKeePassGroup activeGroup = DatabaseViewModel.BreadcrumbViewModel.ActiveGroup;
                if (activeGroup != null && activeGroup.Uuid.Equals(Item.Uuid))
                {
                    DatabaseViewModel.BreadcrumbViewModel.Breadcrumbs.RemoveAt(
                        DatabaseViewModel.BreadcrumbViewModel.Breadcrumbs.Count - 1
                    );
                    DatabaseViewModel.BreadcrumbViewModel.Breadcrumbs.Add(Item);
                }
                return true;
            }
            else
            {
                // Restore pre-save state on cancel
                if (Item.Parent != null)
                {
                    // Revert this item based on its parent
                    if (backup == null)
                    {
                        Debug.Assert(Item.Parent.Groups[i].Uuid.Equals(Item.Uuid));
                        Item.Parent.Groups.RemoveAt(i);
                    }
                    else
                    {
                        Item.Parent.Groups[i] = backup;
                    }
                }
                else
                {
                    // Revert the DatabaseGroup
                    Debug.Assert(backup != null);
                    if (backup == null)
                    {
                        throw new InvalidOperationException();
                    }

                    // Update the DatabaseGroup without updating LastModificationTime - effectively a revert
                    DatabaseViewModel.Document.Root.DatabaseGroup.Update(backup, false);
                }

                return false;
            }
        }

        public override bool Revert()
        {
            int i;
            IKeePassGroup backup = GetBackup(out i);
            if (backup == null)
            {
                return false;
            }

            Item.Update(backup, false);
            return true;
        }
    }
}
