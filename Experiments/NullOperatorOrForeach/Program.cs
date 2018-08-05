using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NullOperatorOrForeach
{
    class foo
    {
        public List<int> list = null;
    }

    class Program
    {
        static void Main(string[] args)
        {
            #region FunWithTimeSpan

            var t = new TimeSpan();

            if (t == TimeSpan.Zero)
                Console.WriteLine(t);

            var ct = DateTime.Now.TimeOfDay;
            var nt = new TimeSpan(9, 50, 0);

            Console.WriteLine((nt - ct));

            #endregion

            #region FunWithQueue

            Console.WriteLine("FunWithQueue");

            var q = new Queue<int>(new []{ 1, 2, 3, 4, 5 });
            q.Enqueue(6);

            Console.WriteLine();

            foreach (var i in q) {
                
                Console.Write($"{i} ");
            }

            Console.WriteLine();

            Console.WriteLine(q.Peek());
            for (int i = 0; i < 10; i++) {
                q.Enqueue(q.Dequeue());
                Console.WriteLine(q.Peek());
            }

            #endregion

            #region Check ~-operation -----------------------

            Console.WriteLine("Check ~-operation");

            byte k = 0;
            Action l = () => Console.WriteLine($"k = {k}; ~k = {(byte)~k}");

            l();
            k = (byte)~k;
            l();
            k = (byte)~k;
            l();

            #endregion

            #region Check ?-operation -------------------------------

            foo foo = null;

            if (foo?.list == null || foo.list.Count == 0)
                Console.WriteLine("true");

            #endregion

            #region Check foreach speed -----------------------------

            int n = 1_000_000_000;
            List<int> list = new List<int>();

            Stopwatch sw = new Stopwatch();
            sw.Start();

            // first measure
            for (int i = 0; i < n; ++i) {
                
                if (list.Count != 0) { }
            }

            Console.WriteLine($"if(Count) invoked for {sw.Elapsed}"); // 3,... sec.

            sw.Restart();

            // second measure
            for (int i = 0; i < n; ++i) {

                foreach (var i1 in list) { }
            }

            Console.WriteLine($"<foreach> invoked for {sw.Elapsed}"); // 20,... sec.

            #endregion

            Console.ReadKey(true);
        }
    }
}
