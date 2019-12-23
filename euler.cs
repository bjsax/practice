using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{

// 1000보다 작은 자연수 중에서 3또는 5의 배수의 합을 구하시오
    class euler
    {
        static void Main(string[] args)
        {
            int sum = 0;

            for (int i = 1; i <= 1000; i++)
            {
                if (i % 3 == 0 | i % 5 == 0)
                {
                    sum += i;
                }
            }
            Console.WriteLine(sum);
        }
    }
}