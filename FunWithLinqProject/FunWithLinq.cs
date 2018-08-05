using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FunWithLinqProject
{
    class TempProjectionItem : IComparable<TempProjectionItem>
    {
        public string Original;
        public string Vowelless;

        public int CompareTo(TempProjectionItem other)
        {
            return String.Compare(Vowelless, other.Vowelless, StringComparison.Ordinal);
        }

        public override string ToString()
        {
            return Vowelless;
        }
    }
    class FunWithLinq
    {
        static void Main(string[] args)
        {
            string[] names = {"Tom", "Alex", "Marry", "Lee", "Po", "Andrew", "Joe"};

            IEnumerable<string> res1 = names
                       .Select(n => n.Replace("a", "").Replace("e", "").Replace("i", "").Replace("o", "")
                                     .Replace("u", "")).Where(l => l.Length > 2).OrderBy(n => n);


            Action<object> lambda = (o) =>
                                    {
                                        Console.WriteLine(o.GetType().Name + " are:");
                                        ((IEnumerable<string>) o).ToList().ForEach(n => Console.Write(n + " "));
                                        Console.WriteLine();
                                    };

            Action<object> lambda2 = (o) =>
                                    {
                                        Console.WriteLine(o.GetType().Name + " are:");
                                        ((IEnumerable<TempProjectionItem>)o).ToList().ForEach(n => Console.Write(n + " "));
                                        Console.WriteLine();
                                    };
            lambda(res1);

            var res2 = names
                       .Select(n => Regex.Replace(n, "[aeiou]","")).OrderBy(n => n).Where(n => n.Length > 2);

            lambda(res2);

            var query = from n in names
                        /*where n.Length > 2
                        orderby n*/
                        select n.Replace("a", "").Replace("e", "").Replace("i", "").Replace("o", "")
                                .Replace("u", "");

            lambda(query);

            var query2 = from n in query
                         where n.Length > 2
                         orderby n
                         select n;

            lambda(query2);

            var query3 = from n in names
                         select n.Replace("a", "").Replace("e", "").Replace("i", "").Replace("o", "")
                                 .Replace("u", "")
                         into r
                         where r.Length > 2
                         orderby r
                         select r;

            Console.WriteLine("\nWith into\n");
            lambda(query3);

            var query4 = from n in
                             (
                                 from n in names
                                 select n.Replace("a", "").Replace("e", "").Replace("i", "").Replace("o", "")
                                         .Replace("u", "")
                             )
                         where n.Length > 2
                         orderby n
                         select n;

            lambda(query4);

            var query5 = from n in
                             (
                                 from n in names
                                 select new TempProjectionItem
                                 { 
                                     Original = n,
                                     Vowelless = n.Replace("a", "").Replace("e", "").Replace("i", "").Replace("o", "")
                                         .Replace("u", "")
                                 }
                             )
                         where n.Vowelless.Length > 2
                         orderby n
                         select n;

            lambda2(query5);

            var query6 = from n in
                             (
                                 from n in names
                                 select new
                                 {
                                     Original = n,
                                     Vowelless = n.Replace("a", "").Replace("e", "").Replace("i", "").Replace("o", "")
                                                  .Replace("u", "")
                                 }
                             )
                         where n.Vowelless.Length > 2
                         orderby n.Original
                         select n.Original;

            lambda(query6);

            var query7 = from n in names
                         let vowelless = n.Replace("a", "").Replace("e", "").Replace("i", "").Replace("o", "")
                                        .Replace("u", "")
                         where vowelless.Length > 2
                         orderby n
                         select n;

            Console.WriteLine("\nWith let\n");
            lambda(query7);

            Console.ReadKey(true);
        }
    }
}
