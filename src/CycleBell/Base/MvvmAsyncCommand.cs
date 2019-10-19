
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CycleBell.Base
{
    public class MvvmAsyncCommand : IAsyncCommand
    {
        private readonly Func< object, Task > _execute;
        private readonly Predicate< object > _canExecute;
        private readonly IErrorHandler _errorHandler;

        private bool _isExecuting;

        public MvvmAsyncCommand ( Func< object, Task > execute, Predicate< object > canExecute = null, IErrorHandler errorHandler = null )
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute), @"execute cannot be null.");
            _canExecute = canExecute;
            _errorHandler = errorHandler;
        }

        public event EventHandler CanExecuteChanged;



        public bool CanExecute ( object parameter )
        {
            return !_isExecuting && _canExecute?.Invoke( parameter ) == null;
        }


        public async Task ExecuteAsync ( object parameter )
        {
            if ( CanExecute( parameter ) ) {
                try {
                    _isExecuting = true;
                    await _execute( parameter );
                }
                finally {
                    _isExecuting = false;
                }
            }
        }

        public void RiseCanExecuteChanged ()
        {
            CanExecuteChanged?.Invoke( this, EventArgs.Empty );
        }

        public async Task ExecuteAsync ()
        {
            await ExecuteAsync( null );
        }


        #region Explicit Implementation

        void ICommand.Execute ( object parameter )
        {
            ExecuteAsync( parameter ).FireAndForgetSafeAsync( _errorHandler );
        }

        #endregion
    }
}
