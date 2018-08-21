using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FunWithObservableCollection1
{
    class Program
    {
        static void Main(string[] args)
        {
            Foo foo = new Foo();
            ((INotifyCollectionChanged)foo.Coll).CollectionChanged += OnCollectionChanged;

            Console.WriteLine ("Enter: \n 'q' for quit, \n '1' for assign \"Reset\" to the foo[0], \n 'r' for clear collection, \n else - to add element.\n");
            while (true) {

                Console.Write ("\n:>");
                var res = Console.ReadLine();

                if (res == "q")
                    break;
                else if (res == "1")
                    foo[0] = "Reset";
                else if (res == "r")
                    foo.Clear();
                else if (res == "x") {
                    foo.ResetCollection();
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
