using PassKeep.Common;
using PassKeep.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassKeep.ViewModels
{
    public class EntrySearchViewModel : ViewModelBase
    {
        private string _query;
        public string Query
        {
            get { return _query; }
            set { SetProperty(ref _query, value); }
        }

        private DatabaseViewModel _databaseViewModel;
        public DatabaseViewModel DatabaseViewModel
        {
            get { return _databaseViewModel; }
            set { SetProperty(ref _databaseViewModel, value); }
        }

        public EntrySearchViewModel(ConfigurationViewModel appSettings, string query)
            : base(appSettings)
        {
            Query = query;
        }
    }
}
