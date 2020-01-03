using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;

namespace SetAlgorithm
{
    class Set
    {
        public static List<int> Converter(List<int> from)
        {
            from.Sort();

            var i = 0;
            var j = 0;
            while (true)
            {
                if (i + 1 == from.Count)
                    break;

                if (from[i] == from[i + 1])
                    from.Remove(from[i]);
                else
                    i++;

                if (j++ > 10000)
                    break;
            }

            return from;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            HashSet<int> set = new HashSet<int> { };
            List<int> list = new List<int> { };
            List<int> list2 = new List<int> { };
            var random = new Random();

            for (var i = 0; i < 100; i++)
            {
                var r = random.Next(1, 15);
                set.Add(r);
                list.Add(r);
                list2.Add(r);
            }

            var myset = Set.Converter(from: list2);
            Console.WriteLine("list length\t: " + list.Count);
            Console.WriteLine("existing set length : "+ set.Count);
            Console.WriteLine("my set length\t: " + myset.Count);

            foreach(var e in list)
                Console.Write(e+" ");

            Console.WriteLine("\n");

            foreach(var e in myset)
                Console.Write(e+" ");
        }
    }
}