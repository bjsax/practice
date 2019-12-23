using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {

            for (int i = 1; i < 10; i++)
            {
                if (i == 4)
                    continue;
                Console.WriteLine(i);
                
            }
            int[] arr = new int[5] { 1, 2, 3, 4, 5 };

            foreach ( int elem in arr)
            {
                Console.WriteLine(elem);

            }
        }
    }
}
