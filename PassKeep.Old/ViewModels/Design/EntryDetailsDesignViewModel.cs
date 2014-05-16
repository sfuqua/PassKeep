﻿using PassKeep.Common;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.KeePass.Dom;
using PassKeep.Lib.KeePass.Rng;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public IDatabaseViewModel DatabaseViewModel
        {
            get;
            set;
        }

        public EntryDetailsDesignViewModel()
        {
            byte[] seed = new byte[32];
            new Random().NextBytes(seed);
            IRandomNumberGenerator rng = new Salsa20(seed);

            Item = new KdbxEntry(new KdbxGroup(null), rng, new KdbxMetadata("test"));
            Item.Title.ClearValue = "An Entry";
            Item.Password.ClearValue = "Foobar";
            Item.UserName.ClearValue = "Some user";

            Item.Fields.Add(new KdbxString("some multiline xkey", "line a1\r\nline 2\r\nline 3 is long long long long long\r\n", rng));
            Item.Fields.Add(new KdbxString("Multi Line Field (Protected)", "line 1\r\nline 2\r\nline 3 is long long long long long\r\n", rng, true));
            Item.Fields.Add(new KdbxString("some long key", "xxsomevalueaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa\r\n\r\n", rng));
            Item.Fields.Add(new KdbxString("some protected key", "some protected value", rng, true));
            Item.Fields.Add(new KdbxString("really long really long  really long  really long  really long  really long  really long  key", "value", rng));

            ViewModel = new { IsReadOnly = true };
            IsReadOnly = true;
        }
    }
}