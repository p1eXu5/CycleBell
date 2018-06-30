using System;
using System.Threading;

namespace TimerExample
{
    class Program
    {
        static void Main (string[] args)
        {
            AutoResetEvent autoEvent = new AutoResetEvent (false);
            StatusChecker statusChecker = new StatusChecker (10);
            TimerCallback tcb = statusChecker.CheckStatus;

            // Create a timer that signals the delegate to invoke 
            // CheckStatus after one second, and every 1/4 second 
            // thereafter.
            Console.WriteLine ($"{DateTime.Now.ToString ("h: mm:ss.fff")} Creating timer.\n");
            Timer stateTimer = new Timer (tcb, autoEvent, 1000, 250);

            // When autoEvent signals, change the period to every
            // 1/2 second.
            autoEvent.WaitOne (5000, false);
            stateTimer.Change (0, 500);
            Console.WriteLine ("\nChanging period.\n");

            // When autoEvent signals the second time, dispose of 
            // the timer.
            autoEvent.WaitOne ();
            stateTimer.Dispose();
            Console.WriteLine ("\nDestroying timer.");

        }
    }

    class StatusChecker
    {
        private int invokeCount;
        private int maxCount;

        public StatusChecker (int count)
        {
            invokeCount = 0;
            maxCount = count;
        }

        // This method is called by the timer delegate.
        public void CheckStatus (Object stateInfo)
        {
            AutoResetEvent autoEvent = (AutoResetEvent) stateInfo;

            Console.WriteLine ($"{DateTime.Now.ToString ("h:mm:ss.fff")} Checking status {(++invokeCount).ToString(),2}.");

            if (invokeCount == maxCount) {
                // Reset the counter and signal Main.
                invokeCount = 0;
                autoEvent.Set();
                Console.WriteLine ($"{nameof(CheckStatus)} invoking autoEvent.Set()");
            }
        }
    }
}
