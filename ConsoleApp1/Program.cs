using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    [Flags]
    public enum Foo : byte
    {
        One = 0x01,
        Two = 0x02,
        Tree = 0x04
    }

    class Program
    {
        static void Main(string[] args)
        {
            Foo _enum = (Foo)0x07;

            Console.WriteLine (_enum == Foo.Two);

            Console.ReadKey (true);
        }
    }
}
