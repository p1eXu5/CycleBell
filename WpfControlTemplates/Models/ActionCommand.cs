using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WpfControlTemplates.Models
{
    public class ActionCommand : ICommand
    {
        private readonly Action<object> _action;
        private readonly Predicate<object> _predicate;

        public ActionCommand (Action<object> action, Predicate<object> predicate = null)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action), "Action parameter is null");
            _predicate = predicate;
        }

        public bool CanExecute (object parameter)
        {
            if (_predicate == null) return true;

            return _predicate (parameter);
        }

        public void Execute()
        {
            Execute(null);
        }

        public void Execute (object parameter)
        {
            if (CanExecute (parameter))
                _action.Invoke (parameter);
        }

        public event EventHandler CanExecuteChanged;
    }
}
