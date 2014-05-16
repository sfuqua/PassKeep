using System.Collections.ObjectModel;
using PassKeep.Common;

namespace PassKeep.ViewModels.Design
{
    public class UintFieldDesignViewModel : BindableBase
    {
        private uint _value;
        public uint Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }

        private string _description;
        public string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value); }
        }

        private bool _enabled;
        public bool Enabled
        {
            get { return _enabled; }
            set { SetProperty(ref _enabled, value); }
        }

        public UintFieldDesignViewModel()
        {
            Value = 256;
            Description = "Some description of some property";
            Enabled = true;
        }
    }
}
