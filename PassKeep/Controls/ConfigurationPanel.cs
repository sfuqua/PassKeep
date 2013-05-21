using PassKeep.ViewModels;

namespace PassKeep.Controls
{
    public sealed class ConfigurationPanel : AppSettingsControl
    {
        public ConfigurationPanel(ConfigurationViewModel appSettings)
        {
            this.DefaultStyleKey = typeof(ConfigurationPanel);
            this.Content = appSettings;
        }
    }
}
