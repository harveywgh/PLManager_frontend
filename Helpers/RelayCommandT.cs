using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WPFModernVerticalMenu.Helpers
{
    public class RelayCommand<T> : ICommand
    {
        private readonly Func<T, Task> _executeAsync;
        private readonly Action<T> _executeSync;
        private readonly Predicate<T> _canExecute;

        public RelayCommand(Func<T, Task> executeAsync, Predicate<T> canExecute = null)
        {
            _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
            _canExecute = canExecute;
        }

        public RelayCommand(Action<T> executeSync, Predicate<T> canExecute = null)
        {
            _executeSync = executeSync ?? throw new ArgumentNullException(nameof(executeSync));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute((T)parameter);
        }

        public async void Execute(object parameter)
        {
            if (_executeAsync != null)
                await _executeAsync((T)parameter);
            else
                _executeSync((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
