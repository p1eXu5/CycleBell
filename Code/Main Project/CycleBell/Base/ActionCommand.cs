using System;
using System.Windows.Input;

namespace CycleBell.Base
{
    public class ActionCommand : ICommand
    {
        private readonly Action<object> _action;
        private readonly Predicate<object> _predicate;

        public ActionCommand (Action<object> action, Predicate<object> predicate = null)
        {
            _action = action ?? throw new ArgumentNullException (nameof(action), @"Action parameter is null.");
            _predicate = predicate;
        }

        public bool CanExecute (object parameter)
        {
            return _predicate?.Invoke (parameter) ?? true;
        }

        public void Execute (object parameter)
        {
            if (_predicate == null || _predicate.Invoke (parameter))
                _action(parameter);
        }

        public virtual void Execute()
        {
            Execute(null);
        }

        
        public event EventHandler CanExecuteChanged;

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke (this, EventArgs.Empty);
        }
        
    }
}
