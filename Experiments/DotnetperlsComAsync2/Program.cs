using System;
using System.Threading;
using System.Threading.Tasks;

namespace DotnetperlsComAsync2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"HashCode current thread is {Thread.CurrentThread.GetHashCode()}");

            while (true) {

                // Start computation.
                Example();

                // Handle user input.
                string result = Console.ReadLine();
                Console.WriteLine("You typed: " + result);
            }
        }

        static async void Example()
        {
            Console.WriteLine($"HashCode current thread is {Thread.CurrentThread.GetHashCode()}");

            // This method runs asynchronously.
            int t = await Task.Run(() => Allocate());
            Console.WriteLine("Compute: " + t);
        }

        static int Allocate()
        {
            // Compute total count of digit in strings.
            int size = 0;
            for (int z = 0; z < 100; ++z) {
                for (int i = 0; i < 1_000_000; i++) {
                    string value = i.ToString();
                    size += value.Length;
                }
            }

            return size;
        }
    }
}
