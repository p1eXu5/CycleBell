using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SortedList
{
    class Program
    {
        static void Main(string[] args)
        {
            Random rnd = new Random();

            SortedDictionary<int, int> dict = new SortedDictionary<int, int>();

            for (int i = 0; i < 100; ++i)
                dict[rnd.Next(100)] = i;

            foreach (int dictKey in dict.Keys) {
                Console.Write($"{dictKey} ");
            }

            Console.WriteLine();

            DictsComp(10);

            Console.ReadKey();
        }

        static void DictsComp(int count)
        {
            SortedList<int, int> sortedList = new SortedList<int, int>();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int i = 0; i < count; ++i) {
                sortedList[i] = i;
            }

            stopwatch.Stop();
            Console.WriteLine($"SortedList is filled in {stopwatch.Elapsed}");
            stopwatch.Reset();

            SortedDictionary<int, int> sdict = new SortedDictionary<int, int>();

            stopwatch.Start();

            for (int i = 0; i < count; ++i) {
                sdict[i] = i;
            }

            stopwatch.Stop();
            Console.WriteLine($"SortedDictionary is filled in {stopwatch.Elapsed}");
            stopwatch.Reset();

            Dictionary<int, int> dict = new Dictionary<int, int>();

            stopwatch.Start();

            for (int i = 0; i < count; ++i) {
                dict[i] = i;
            }

            stopwatch.Stop();
            Console.WriteLine($"Dictionary is filled in {stopwatch.Elapsed}");

            Console.WriteLine();

            stopwatch.Reset();

            int j = 0;

            stopwatch.Start();

            for (int i = 0; i < count; ++i) {
                if (sortedList[i] == i) j = sortedList[i];
            }

            stopwatch.Stop();
            Console.WriteLine($"SortedList lookup in {stopwatch.Elapsed}");
            stopwatch.Reset();

            stopwatch.Start();

            for (int i = 0; i < count; ++i) {
                if (sdict[i] == i) j = sdict[i];
            }

            stopwatch.Stop();
            Console.WriteLine($"SortedDictionary lookup in {stopwatch.Elapsed}");
            stopwatch.Reset();

            stopwatch.Start();

            for (int i = 0; i < count; ++i) {
                if (dict[i] == i) j = dict[i];
            }

            stopwatch.Stop();
            Console.WriteLine($"Dictionary lookup in {stopwatch.Elapsed}");

        }
    }
}
