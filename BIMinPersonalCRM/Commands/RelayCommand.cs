using System;
using System.Windows.Input;

namespace BIMinPersonalCRM.Commands
{
    /// <summary>
    ///     A simple implementation of <see cref="ICommand"/> that delegates the
    ///     <see cref="Execute(object?)"/> and <see cref="CanExecute(object?)"/> logic to
    ///     supplied delegates. This class makes it easy to wire up commands in
    ///     view models without creating separate classes for each action.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// </summary>
        /// <param name="execute">The action to execute when the command is invoked.</param>
        /// <param name="canExecute">An optional predicate that determines whether the command can execute.</param>
        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <inheritdoc />
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <inheritdoc />
        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute(parameter);

        /// <inheritdoc />
        public void Execute(object? parameter) => _execute(parameter);
    }
}
