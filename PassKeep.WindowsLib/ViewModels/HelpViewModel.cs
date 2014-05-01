﻿using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using SariphLib.Mvvm;

namespace PassKeep.Lib.ViewModels
{
    public class HelpViewModel : BindableBase, IHelpViewModel
    {
        private bool _showSampleBlurb;
        public bool ShowSampleBlurb
        {
            get { return this._showSampleBlurb; }
            private set
            {
                TrySetProperty(ref this._showSampleBlurb, value);
            }
        }

        public HelpViewModel(IAppSettingsService settingsService)
        {
            ShowSampleBlurb = settingsService.SampleEnabled;
        }
    }
}
