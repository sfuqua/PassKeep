using PassKeep.Common;
using PassKeep.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PassKeep.KeePassLib;

namespace PassKeep.ViewModels.Design
{
    public class EntryDetailsDesignViewModel : BindableBase
    {
        private KdbxEntry _item;
        public KdbxEntry Item
        {
            get { return _item; }
            set { SetProperty(ref _item, value); }
        }

        private bool isReadOnly;
        public bool IsReadOnly
        {
            get { return isReadOnly; }
            set { SetProperty(ref isReadOnly, value); }
        }

        private object viewModel;
        public object ViewModel
        {
            get { return viewModel; }
            set { SetProperty(ref viewModel, value); }
        }

        public DatabaseViewModel DatabaseViewModel
        {
            get;
            set;
        }

        private PasswordGenViewModel passwordGenViewModel;
        public PasswordGenViewModel PasswordGenViewModel
        {
            get { return passwordGenViewModel; }
            set { SetProperty(ref passwordGenViewModel, value); }
        }

        public EntryDetailsDesignViewModel()
        {
            byte[] seed = new byte[32];
            new Random().NextBytes(seed);
            KeePassRng rng = new Salsa20Rng(seed);

            Item = new KdbxEntry(new KdbxGroup(null), rng, new KdbxMetadata("test"));
            Item.Title.ClearValue = "An Entry";
            Item.Password.ClearValue = "Foobar";
            Item.UserName.ClearValue = "Some user";

            Item.Fields.Add(new KdbxString("some multiline xkey", "line a1\r\nline 2\r\nline 3 is long long long long long\r\n", rng));
            Item.Fields.Add(new KdbxString("some multiline protected", "line 1\r\nline 2\r\nline 3 is long long long long long\r\n", rng, true));
            Item.Fields.Add(new KdbxString("some long key", "xxsomevalueaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa\r\n\r\n", rng));
            Item.Fields.Add(new KdbxString("some protected key", "some protected value", rng, true));
            Item.Fields.Add(new KdbxString("really long really long  really long  really long  really long  really long  really long  key", "value", rng));

            ViewModel = new { IsReadOnly = true };
            IsReadOnly = true;
            PasswordGenViewModel = new PasswordGenViewModel(null);
        }
    }
}
