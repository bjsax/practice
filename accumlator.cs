using System;
using System.Diagnostics;

namespace Calculation
{
    class Accumulator
    {
        public string Add(string s1, string s2)
        {
            normalizeInput(ref s1, ref s2);

            var len = s1.Length;
            var result = "";
            var carry = 0;

            for (var i = len - 1; i >= 0; i--)
            {
                var res = (s1[i] - '0') + (s2[i] - '0') + carry;
                carry = res / 10;
                res %= 10;

                result += res;
            }

            if (carry == 1)
                result += "1";

            return this.reverse(result);
        }

        public string Subtract(string s1, string s2)
        {
            if (int.Parse(s2) > int.Parse(s1))
            {
                return "-" + Subtract(s2, s1);
            }

            normalizeInput(ref s1, ref s2);

            var len = s1.Length;
            var result = "";
            var borrow = 0;

            for (var i = len - 1; i >= 0; i--)
            {
                var res = (s1[i] - '0') - (s2[i] - '0') - borrow;
                if (res < 0)
                {
                    borrow = 1;
                    res += 10;
                }
                else
                {
                    borrow = 0;
                }

                result += res;
            }

            return int.Parse(this.reverse(result)).ToString();
        }

        private void normalizeInput(ref string s1, ref string s2)
        {
            var diff = Math.Abs(s1.Length - s2.Length);

            if (s1.Length > s2.Length)
            {
                s2 = append(s2, "0", diff);
            }
            else if (s1.Length < s2.Length)
            {
                s1 = append(s1, "0", diff);
            }
        }

        public string reverse(string org)
        {
            var len = org.Length;

            for (var i = 0; i < len / 2; i++)
            {
                var tempchar = org[0 + i];
                org = org.Remove(0 + i, 1).Insert(0 + i, org[len - 1 - i].ToString());
                org = org.Remove(len - 1 - i, 1).Insert(len - 1 - i, tempchar.ToString());
            }

            return org;
        }

        private string append(string org, string toAppend, int no)
        {
            for (var i = 0; i < no; i++)
            {
                org = toAppend + org;
            }

            return org;
        }
    }

    class StringAccumulator
    {
        static void Main(string[] args)
        {
            var random = new Random();
            var accumulator = new Accumulator();

            for (int i = 0; i < 100000; i++)
            {            
                var s1 = (long)random.Next();
                var s2 = (long)random.Next();
                var expected1 = (s1 + s2).ToString();
                var expected2 = (s1 - s2).ToString();

                var s3 = accumulator.Add(s1.ToString(), s2.ToString());
                var s4 = accumulator.Subtract(s1.ToString(), s2.ToString());
                
                Debug.Assert(s3 == expected1);
                Debug.Assert(s4 == expected2);
            }
        }
    }
}