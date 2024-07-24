using System;
using System.Windows.Input;

namespace IMusicSync.Common
{
    public abstract class RelayCommandBase : ICommand
    {
        protected Func<bool> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_canExecute != null)
                    CommandManager.RequerySuggested += value;
            }

            remove
            {
                if (_canExecute != null)
                    CommandManager.RequerySuggested -= value;
            }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }

        public abstract void Execute(object parameter);
    }

    public class RelayCommand : RelayCommandBase
    {
        private Action _execute;

        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            _canExecute = canExecute;
            _execute = execute;
        }

        public RelayCommand(Action execute)
        {
            _execute = execute;
        }

        public override void Execute(object parameter)
        {
            if(CanExecute(parameter))
                _execute();
        }
    }

    public class RelayCommand<T> : RelayCommandBase
    {
        private Action<T> _execute;

        public RelayCommand(Action<T> execute, Func<bool> canExecute)
        {
            _canExecute = canExecute;
            _execute = execute;
        }

        public RelayCommand(Action<T> execute)
        {
            _execute = execute;
        }

        public override void Execute(object parameter)
        {
            if (CanExecute(parameter))
                _execute((T)parameter);
        }
    }
}
