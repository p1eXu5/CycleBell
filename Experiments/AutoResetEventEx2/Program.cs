using System;
using System.Threading;

namespace AutoResetEventEx
{
    class Program
    {
        private static AutoResetEvent event_1 = new AutoResetEvent(true);
        private static AutoResetEvent event_2 = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            Console.WriteLine("Press Enter to create three threads and start them.\r\n" +
                              "The threads wait on AutoResetEvent #1, which was created\r\n" +
                              "in the signaled state, so the first thread is released.\r\n" +
                              "This puts AutoResetEvent #1 into the unsignaled state.");
            Console.ReadLine();

            for (int i = 1; i < 4; i++) {
                Thread t = new Thread(ThreadProc);
                t.Name = "Thread_" + i;
                t.Start();
            }
            Thread.Sleep(250);

            for (int i = 0; i < 2; i++) {
                Console.WriteLine("Press Enter to release another thread: \tevent_1.Set()");
                Console.ReadLine();
                event_1.Set();
                Thread.Sleep(250);
            }

            Console.WriteLine("\r\nAll threads are now waiting on AutoResetEvent #2.\n");
            for (int i = 0; i < 3; i++) {
                Console.WriteLine("Press Enter to release a thread: \tevent_2.Set()");
                Console.ReadLine();
                event_2.Set();
                Thread.Sleep(250);
            }

            // Visual Studio: Uncomment the following line.
            //Console.Readline();
        }

        static void ThreadProc()
        {
            string name = Thread.CurrentThread.Name;

            Console.WriteLine($"{Thread.CurrentThread.Name} waits on AutoResetEvent #1.");
            event_1.WaitOne();
            Console.WriteLine($"{Thread.CurrentThread.Name} is released from AutoResetEvent #1.");

            Console.WriteLine($"{Thread.CurrentThread.Name} waits on AutoResetEvent #2.");
            event_2.WaitOne();
            Console.WriteLine($"{Thread.CurrentThread.Name} is released from AutoResetEvent #2.");

            Console.WriteLine($"{Thread.CurrentThread.Name} ends.");
        }

    }
}
