using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeSpanLogical
{
    class Program
    {
        static void Main(string[] args)
        {
            TimeSpan tNeg = TimeSpan.FromMinutes(-1);

            if (tNeg < TimeSpan.Zero)
                Console.WriteLine("Less");

            Console.ReadKey(true);
        }
    }
}
