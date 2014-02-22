using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using SariphLib.Mvvm;

namespace PassKeep.Lib.ViewModels
{
    public class HelpViewModel : BindableBase, IHelpViewModel
    {
        private bool _showSampleBlurb;
        public bool ShowSampleBlurb
        {
            get { return _showSampleBlurb; }
            private set
            {
                SetProperty(ref _showSampleBlurb, value);
            }
        }

        public HelpViewModel(IAppSettingsService settingsService)
        {
            ShowSampleBlurb = settingsService.SampleEnabled;
        }
    }
}
