using System;
using System.Windows.Input;

namespace MusicDownloader.Common
{
    /// <summary>
    ///     Represents RelayCommand that uses the WPF CommandManager
    /// </summary>
    public sealed class RelayCommand : ICommand
    {
        private readonly Func<Boolean> canExecuteMethod;
        private readonly Action executeMethod;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelayCommand" /> class.
        /// </summary>
        /// <param name="executeMethod">The execute method.</param>
        /// <param name="canExecuteMethod">The can execute method.</param>
        public RelayCommand(Action executeMethod, Func<Boolean> canExecuteMethod = null)
        {
            if (executeMethod == null)
                throw new ArgumentNullException("executeMethod");

            this.executeMethod = executeMethod;
            this.canExecuteMethod = canExecuteMethod;
        }

        /// <summary>
        ///     Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        ///     Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">
        ///     Data used by the command.  If the command does not require data to be passed, this object can
        ///     be set to null.
        /// </param>
        /// <returns>
        ///     true if this command can be executed; otherwise, false.
        /// </returns>
        public Boolean CanExecute(Object parameter)
        {
            return canExecuteMethod == null || canExecuteMethod();
        }

        /// <summary>
        ///     Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">
        ///     Data used by the command.  If the command does not require data to be passed, this object can
        ///     be set to null.
        /// </param>
        public void Execute(Object parameter)
        {
            if (executeMethod != null)
                executeMethod();
        }
    }

    public sealed class RelayCommand<T> : ICommand
    {
        private readonly Predicate<T> canExecuteMethod;
        private readonly Action<T> executeMethod;

        public RelayCommand(Action<T> executeMethod, Predicate<T> canExecuteMethod = null)
        {
            if (executeMethod == null)
                throw new ArgumentNullException("executeMethod");

            this.executeMethod = executeMethod;
            this.canExecuteMethod = canExecuteMethod;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        ///     Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">
        ///     Data used by the command.  If the command does not require data to be passed, this object can
        ///     be set to null.
        /// </param>
        /// <returns>
        ///     true if this command can be executed; otherwise, false.
        /// </returns>
        public Boolean CanExecute(Object parameter)
        {
            return canExecuteMethod == null || canExecuteMethod((T) parameter);
        }

        /// <summary>
        ///     Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">
        ///     Data used by the command.  If the command does not require data to be passed, this object can
        ///     be set to null.
        /// </param>
        public void Execute(Object parameter)
        {
            executeMethod((T) parameter);
        }
    }
}