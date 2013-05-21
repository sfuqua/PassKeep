using System;
using System.Windows.Input;

namespace PassKeep.Common
{
    public class DelegateCommand : ICommand
    {
        public static readonly DelegateCommand NoOp;
        static DelegateCommand()
        {
            NoOp = new DelegateCommand(() => { });
        }

        private Func<bool> canExecute;
        private Action methodToExecute;

        public DelegateCommand(Func<bool> canExecute, Action methodToExecute)
        {
            this.canExecute = canExecute;
            this.methodToExecute = methodToExecute;
            RaiseCanExecuteChanged();
        }

        public DelegateCommand(Action methodToExecute)
            : this(() => true, methodToExecute)
        { }

        public bool CanExecute(object parameter)
        {
            return canExecute();
        }

        public event EventHandler CanExecuteChanged;
        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }

        public void Execute(object parameter)
        {
            methodToExecute();
        }
    }
}
