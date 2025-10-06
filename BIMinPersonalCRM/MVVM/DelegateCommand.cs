using System.Windows.Input;

namespace BIMinPersonalCRM.Commands
{
    /// <summary>
    /// Простая реализация <see cref="ICommand"/> для делегирования логики Execute и CanExecute.
    /// </summary>
    public class DelegateCommand : ICommand
    {
        private readonly Action _func;
        private readonly Predicate<object?>? _canExecute;

        public DelegateCommand(Action func, Predicate<object?>? canExecute = null)
        {
            _func = func;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object parameter) => _func.Invoke();

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// Принудительный вызов события CanExecuteChanged для обновления состояния команд.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public class DelegateCommand<T> : ICommand
    {
        private readonly Action<T> _func;
        private readonly Predicate<object?>? _canExecute;

        public DelegateCommand(Action<T> func, Predicate<object?>? canExecute = null)
        {
            _func = func;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object parameter) => _func.Invoke((T)parameter);

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// Принудительный вызов события CanExecuteChanged для обновления состояния команд.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
