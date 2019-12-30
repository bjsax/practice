using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// 숫자로 구성된 두 문자열을 더하는 함수와 빼는 함수를 작성하시오
namespace ConsoleApp1
{   
    class Accumulator
    {
        public Accumulator()
        {
        }

        

        // TODO: 큰수와 작은 수를 생각하지 않기
        public string Add(string s1, string s2)
        {
            string result = "";
            var diff  = Math.Abs(s1.Length - s2.Length);

            if (s1.Length > s2.Length)
            {
                s2 = append(s2, "0", diff);
            }
            else if (s1.Length < s2.Length)
            {
                s1 = append(s1, "0", diff);
            }

            int carry = 0;
            var len = s1.Length;
            for (var i = len - 1; i >= 0; i--)
            {
                int res = (s1[i] - '0') + (s2[i] - '0') + carry;
                carry = res / 10;
                res   %= 10;

                result += res;
            }

            if (carry == 1)
                result += "1";

            return this.reverse(result);
        }

        // TODO: s1이 큰 수라고 가정하고 짜기.
        // TODO: ch - '0' 확인하기
        public string Subtract(string s1, string s2)
        {
            if (int.Parse(s2) > int.Parse(s1))
            {
                return "-" + Subtract(s2, s1);
            }

            string result = "";
            var diff = Math.Abs(s1.Length - s2.Length);

            if (s1.Length > s2.Length)
            {
                s2 = append(s2, "0", diff);
            }
            else if (s1.Length < s2.Length)
            {
                s1 = append(s1, "0", diff);
            }

            int borrow = 0;
            var len = s1.Length;
            for (var i = len - 1; i >= 0; i--)
            {
                int res = (s1[i] - '0') - (s2[i] - '0') - borrow;
                if (res < 0)
                {
                    borrow = 1;
                    res += 10;
                }
                else
                    borrow = 0;

             
                result += res;
            }

            return int.Parse(this.reverse(result)).ToString();
        }

        // refactor again
        private string reverse(string inputString)
        {
            var output_string = "";
            for (int i = inputString.Length; i > 0; i--)
            {
                output_string += inputString[i - 1];
            }

            return output_string;
        }
        private string append(string org, string toAppend, int no)
        {
            for (var i = 0; i < no; i++)
                org = toAppend + org;

            return org;
        }
    }

    class StringAccumulator
    {
        // TODO random number
        static void Main(string[] args)
        {
            var random = new Random();
            for (int i = 0; i < 100000; i++)
            {
                var s1 = (long)random.Next();
                var s2 = (long)random.Next();
                var expected1 = (s1 + s2).ToString();
                var expected2 = (s1 - s2).ToString();

                var accumulator = new Accumulator();

                var s3 = accumulator.Add(s1.ToString(), s2.ToString());
                var s4 = accumulator.Subtract(s1.ToString(), s2.ToString());

                Debug.Assert(s3 == expected1);
                Debug.Assert(s4 == expected2);
            }
        }
    }
}
