using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
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
            Foo _enum = (Foo)0x07;  // false

            Console.WriteLine (_enum == Foo.Two);

            // Сравнение скоростей распаковки логических, ссылочных и типов значений.

            Check(ObjConverterBool, true);      // supper slow
            Check(ObjConverterString, "True");  // normal
            Check(ObjConverterInt, 1);          // slow

            Console.ReadKey (true);
        }

        static void Check<T>(Func<object> func, T res)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < 10000; i++) {

                if (((T)func()).Equals(res)) {
                }
            }

            sw.Stop();
            Console.WriteLine($"ObjConverter{typeof(T)}(): {sw.Elapsed}");
        }

        static object ObjConverterBool()
        {
            return true;
        }

        static object ObjConverterString()
        {
            return "True";
        }

        static object ObjConverterInt()
        {
            return 1;
        }
    }
}
