using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PassKeep.Models.Abstraction;

namespace PassKeep.ViewModels
{
    public class DatabaseClipboardViewModel : ViewModelBase
    {
        public DatabaseClipboardViewModel(ConfigurationViewModel appSettings)
            : base(appSettings)
        {

        }

        public void AddToClipboard(IKeePassNode entity)
        {

        }
    }
}
