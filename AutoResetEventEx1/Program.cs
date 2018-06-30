using System;
using System.Threading;

namespace AutoResetEventEx1
{
    class Program
    {
        //Initially not signaled.
        const int numIterations = 100;
        static AutoResetEvent myResetEvent = new AutoResetEvent(false);
        static int number;

        static void Main(string[] args)
        {
            Thread myReadThread = new Thread (new ThreadStart(MyReadThreadProc));
            myReadThread.Name = "ReaderThread";
            myReadThread.Start();

            for (int i = 0; i < numIterations; ++i) {

                Console.WriteLine ($"Writer thread writing value: {i}");
                number = i;

                myResetEvent.Set();

                Thread.Sleep (1);
            }

            myReadThread.Abort();
        }

        static void MyReadThreadProc()
        {
            while (true) {
                myResetEvent.WaitOne();
                Console.WriteLine ($"{Thread.CurrentThread.Name} reading value {number}");
            }
        }
    }
}
