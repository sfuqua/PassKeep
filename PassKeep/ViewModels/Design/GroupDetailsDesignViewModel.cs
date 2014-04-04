using PassKeep.Common;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.KeePass.Dom;
using PassKeep.Lib.KeePass.Rng;
using System;

namespace PassKeep.ViewModels.Design
{
    public class GroupDetailsDesignViewModel : BindableBase, IGroupDetailsViewModel
    {
        private IKeePassGroup _group;
        public IKeePassGroup Group
        {
            get { return _group; }
            set { SetProperty(ref _group, value); }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public GroupDetailsDesignViewModel()
        {
            byte[] seed = new byte[32];
            new Random().NextBytes(seed);
            IRandomNumberGenerator rng = new Salsa20(seed);

            Group = new KdbxGroup(null);
            Group.Title =  new KdbxString("Name", "A Group", rng);
            Group.Notes = new KdbxString("Notes", "value", rng);
            Group.EnableSearching = true;
        }
    }
}
