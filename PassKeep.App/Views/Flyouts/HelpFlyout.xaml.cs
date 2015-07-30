using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;

namespace PassKeep.Views.Flyouts
{
    public sealed partial class HelpFlyout
    {
        public HelpFlyout()
        {
            InitializeComponent();
        }

        private async void MailLink_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Uri mailUri = new Uri($"mailto:{this.contactEmail.Text}?subject=PassKeep Feedback");
            await Launcher.LaunchUriAsync(mailUri);
        }

        private async void SubredditLink_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Uri webUri = new Uri("https://www.reddit.com/r/PassKeep");
            await Launcher.LaunchUriAsync(webUri);
        }
    }
}
