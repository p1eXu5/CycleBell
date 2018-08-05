using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DotnetperlsComAsync
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"HashCode current thread is {Thread.CurrentThread.GetHashCode()}");

            // Start the HandleFile method
            Task<int> task = HandleFileAsync();

            // Control returns here before HandleFileAsync returns
            // ... Prompt the user
            Console.WriteLine("Please wait patiently " +
                              "while I do something important");

            // Do something at the same time as the file is being read.
            Console.Write(">>> ");
            string line = Console.ReadLine();
            Console.WriteLine("You entered (asynchronous logic): " + line);

            // Wait for the HandleFile task to complete.
            // ... Display it's result
            task.Wait();
            var x = task.Result;
            Console.WriteLine("Count: " + x);

            Console.WriteLine("[DONE]");
            Console.ReadKey(true);
        }

        static async Task<int> HandleFileAsync()
        {
            string file = @"C:\Programs\enable1.txt";
            Console.WriteLine("HandleFile enter");
            int count = 0;

            Console.WriteLine($"HashCode current thread is {Thread.CurrentThread.GetHashCode()}");

            // Read in the specified file
            // ... Use async StreamReader method
            using (StreamReader reader = new StreamReader(file)) {

                string v = await reader.ReadToEndAsync();

                Console.WriteLine($"HashCode current thread is {Thread.CurrentThread.GetHashCode()}");
                Thread.Sleep(500);

                // ... Process the file data somehow.
                count += v.Length;

                // ... A slow-running omputation.
                //     Dummy code.
                for (int i = 0; i < 5_000_000; i++) {

                    int x = v.GetHashCode();
                    if (x == 0) {

                        count--;
                    }

                    if (i == 2_500_000) {
                        Console.WriteLine("50% done");
                    }
                }
            }

            Console.WriteLine("HandleFile exit");
            return count;
        }
    }
}
