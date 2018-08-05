using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunWithAsyncAwait
{
    class AsyncAwaitMain
    {
        static async Task Main(string[] args)
        {
            SomeService ss = new SomeService();

            Console.WriteLine("Start...\n");
            await ss.GetterAsync();

            Console.WriteLine("Wait for a work will be done");
            Console.ReadKey(true);
        }
    }
}
