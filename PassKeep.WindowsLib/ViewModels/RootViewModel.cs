using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using SariphLib.Mvvm;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// Serves as a ViewModel for the root container of the app.
    /// </summary>
    public class RootViewModel : BindableBase, IRootViewModel
    {
        private IAppSettingsService _settingsService;

        private ActivationMode _activationMode;
        /// <summary>
        /// How the app believes it was activated
        /// </summary>
        public ActivationMode ActivationMode
        {
            get { return this._activationMode; }
            set { SetProperty(ref this._activationMode, value); }
        }

        public RootViewModel(
            IAppSettingsService settingsService,
            ActivationMode activationMode = ActivationMode.Regular
            )
        {
            this._settingsService = settingsService;
            this.ActivationMode = activationMode;
        }
    }
}
