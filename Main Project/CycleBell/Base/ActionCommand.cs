using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CycleBell.Base
{
    public class ActionCommand : ICommand
    {
        private readonly Action<object> action;
        private readonly Predicate<object> predicate;

        public ActionCommand (Action<object> action, Predicate<object> predicate = null)
        {
            this.action = action ?? throw new ArgumentNullException (nameof(action), "Action parameter is null.");
            this.predicate = predicate;
        }

        public bool CanExecute (object parameter)
        {
            return predicate?.Invoke (parameter) ?? true;
        }

        public void Execute (object parameter)
        {
            if (predicate == null || predicate.Invoke (parameter))
                action(parameter);
        }

        public virtual void Execute()
        {
            Execute(null);
        }

        public event EventHandler CanExecuteChanged;
    }
}
