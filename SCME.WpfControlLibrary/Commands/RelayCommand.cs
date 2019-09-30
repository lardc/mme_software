using System;
using System.Windows.Input;

namespace SCME.WpfControlLibrary.Commands
{
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        
        public RelayCommand(Action<T> execute)
        {
            _execute = execute;
        }
        
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}