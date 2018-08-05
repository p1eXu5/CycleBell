/*
 * Использование state в обработчике таймера. Для отслеживания изменений, происходящих с объектом state, он
 * должен быть ссылочным типом. Если это строка или структура, то необходима обёртка в класс.
 */

using System;
using System.Threading;

namespace TimerStateObject
{
    class Foo
    {
        public string str = "a";
        public TimeSpan ts = TimeSpan.Zero;
    }
    class Program
    {
        private static Timer _timer;
        private static int i;

        static void Main(string[] args)
        {
            Program p = new Program();
            p.Run();

            Console.ReadKey(true);

            if (_timer != null) {
                Console.WriteLine(_timer);
                _timer.Dispose();
            }

            Console.ReadKey(true);
        }

        void Run()
        {
            TimeSpan? t = TimeSpan.Zero + TimeSpan.FromMinutes(2);
            Foo foo = new Foo();
            foo.str = "a";

            _timer = new Timer(TimerHandle, foo, 0, Timeout.Infinite);

            Thread.Sleep(2000);
            foo.ts = TimeSpan.Zero;
            foo.str = "b";
            Console.WriteLine($"\nfoo.ts changed in main thread to {foo.ts}");
            Console.WriteLine($"foo.str changed in main thread to {foo.str}\n");
        }

        void TimerHandle(Object state)
        {
            Foo f = state as Foo;

            if (f.ts > TimeSpan.FromMinutes(3)) {
                _timer.Dispose();
                Console.WriteLine("Dispose");
                return;
            }

            ++i;
            f.str = f.str + "a";
            f.ts = f.ts + TimeSpan.FromMinutes(1);
            Console.WriteLine($"Итерация {i}:");
            Console.WriteLine(f.str);
            Console.WriteLine(f.ts);
            _timer.Change(1000, 0);
        }
    }
}
