using PassKeep.Contracts.ViewModels;

namespace PassKeep.Views.Flyouts
{
    public sealed partial class AppSettingsFlyout
    {
        public AppSettingsFlyout()
        {
            InitializeComponent();
        }

        public IAppSettingsViewModel ViewModel
        {
            get;
            set;
        }
    }
}
