/*
 * Clear: Old is null, New is null
 * Add: Old is null, New isn't null
 * Remove: Old is not null, New is null
 */

using System;
using System.Collections.Specialized;
using System.Linq;

namespace FunWithObservableCollection1
{
    class Program
    {
        static void Main(string[] args)
        {
            Foo foo = new Foo();
            ((INotifyCollectionChanged)foo.Coll).CollectionChanged += OnCollectionChanged;

            string message = "Enter: \n" +
                             " 'q' for quit, \n" +
                             " '1' for assign test object to the foo[0], \n" +
                             " 'c' for clear collection, \n" +
                             " <else> - to add element.\n" +
                             " 'd' space <object> - for delete element \n" +
                             " '?' - for help";

            Console.WriteLine(message);

            while (true) {

                Console.Write ("\n:>");
                var res = Console.ReadLine();

                if (res == "q")
                    break;

                if (res == "1") {
                    foo[0] = "Reset";
                }
                else if (res == "c") {
                    foo.Clear();
                }
                else if (res == "x") {
                    foo.ResetCollection();
                }
                else if (res.Length > 0 && res[0] == 'd') {
                    string[] elements = res.Split(' ').Select(e => e.Trim()).ToArray();
                    for (int i = 1; i < elements.Length; ++i) {
                        foo.Remove(elements[i]);
                    }
                }
                else if (res == "?") {
                    Console.WriteLine($"\n{message}");
                }
                else {
                    foo.Add (res);
                    Console.WriteLine ($"\n{foo.Coll.GetType().Name} contains from:\n");
                    foo.Coll.ToList().ForEach (i => Console.Write($"{i} "));
                    Console.WriteLine ();
                }

            }
        }

        static void OnCollectionChanged(object s, NotifyCollectionChangedEventArgs e)
        {
            Console.WriteLine($"Old item is {e.OldItems?[0] ?? "null"}.\n New items {e.NewItems?[0] ?? "null"}");
        }
    }
}
