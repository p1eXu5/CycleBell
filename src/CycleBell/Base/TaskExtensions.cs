
using System;
using System.Threading.Tasks;

namespace CycleBell.Base
{
    public static class TaskExtensions
    {
        public static async void FireAndForgetSafeAsync ( this Task task, IErrorHandler handler = null )
        {
            try {
                await task;
            }
            catch( Exception ex ) {

                handler?.HandleError( ex );
            }
        }
    }
}
