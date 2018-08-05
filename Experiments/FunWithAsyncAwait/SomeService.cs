using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FunWithAsyncAwait
{
    class SomeService
    {
        public async Task<int> GetterAsync()
        {
            Console.WriteLine("Start Getter");

            Task<string> worker = GetStringAsync();

            DoIndependentWork();

            string res = await worker;

            return res.Length;
        }

        private async Task<string> GetStringAsync()
        {
            Console.WriteLine($"Working in {Thread.CurrentThread.Name}");

            Thread.Sleep(5000);

            return Thread.CurrentThread.Name;
        }

        private void DoIndependentWork()
        {
            Console.WriteLine("Do work...");
        }
    }
}
