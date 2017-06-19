using PassKeep.Common;
using PassKeep.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PassKeep.KeePassLib;
using PassKeep.KeePassLib.Crypto;

namespace PassKeep.ViewModels.Design
{
    public class GroupDetailsDesignViewModel : BindableBase
    {
        private KdbxGroup _group;
        public KdbxGroup Group
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
