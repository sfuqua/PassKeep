using PassKeep.Models;
using PassKeep.ViewBases;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace PassKeep.Views
{
    /// <summary>
    /// The primary View over the contents of a KeePass database.
    /// </summary>
    public sealed partial class DatabaseView : DatabaseViewBase
    {

        public DatabaseView()
            : base()
        {
            this.InitializeComponent();
        }
    }
}
