using PassKeep.Lib.Contracts.Services;
using PassKeep.ViewModels;

namespace PassKeep.Controls
{
    public sealed class ConfigurationPanel : AppSettingsControl
    {
        public ConfigurationPanel(IAppSettingsService appSettings)
        {
            this.DefaultStyleKey = typeof(ConfigurationPanel);
            this.Content = appSettings;
        }
    }
}
