using PassKeep.Common;
using PassKeep.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace PassKeep.ViewModels
{
    public class FileOpenViewModel : ViewModelBase
    {
        private StorageFile _file;
        public StorageFile File
        {
            get { return _file; }
            set { SetProperty(ref _file, value); }
        }

        private DatabaseViewModel _databaseViewModel;
        public DatabaseViewModel DatabaseViewModel
        {
            get { return _databaseViewModel; }
            set { SetProperty(ref _databaseViewModel, value); }
        }

        public FileOpenViewModel(ConfigurationViewModel appSettings, StorageFile file)
            : base(appSettings)
        {
            File = file;
        }
    }
}
