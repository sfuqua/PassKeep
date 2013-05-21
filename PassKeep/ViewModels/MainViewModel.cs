using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassKeep.ViewModels
{
    public enum ActivationMode
    {
        Regular,
        Search,
        File
    }

    public class MainViewModel : ViewModelBase
    {
        private ActivationMode _mode;
        public ActivationMode Mode
        {
            get { return _mode; }
            set { SetProperty(ref _mode, value); }
        }

        private EntrySearchViewModel _searchViewModel;
        public EntrySearchViewModel SearchViewModel
        {
            get { return _searchViewModel; }
            set { SetProperty(ref _searchViewModel, value); }
        }

        private FileOpenViewModel _fileOpenViewModel;
        public FileOpenViewModel FileOpenViewModel
        {
            get { return _fileOpenViewModel; }
            set { SetProperty(ref _fileOpenViewModel, value); }
        }

        public MainViewModel(ConfigurationViewModel appSettings, ActivationMode mode = ActivationMode.Regular)
            : base(appSettings)
        {
            Mode = mode;
        }
    }
}
