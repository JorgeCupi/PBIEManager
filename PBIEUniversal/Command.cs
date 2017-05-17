using System;
using System.Windows.Input;

namespace PBIEMobileSDK
{
    internal class Command : ICommand
    {
        private Action execute;
        private Func<object, bool> canExecute;

        public Command(Action execute)
        {
            this.execute = execute;
            this.canExecute = (x) => { return true; };
        }

        public bool CanExecute(object parameter)
        {
            return canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged;
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Execute(object parameter)
        {
            execute();
        }
    }
}