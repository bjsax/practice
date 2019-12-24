using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// 숫자로 구성된 두 문자열을 더하는 함수와 빼는 함수를 작성하시오

namespace ConsoleApp1
{
    class Two_Strings
    {
        private string large_value, small_value = null;

        public Two_Strings(string input_string)
        {
            string[] token;
            token = input_string.Split(' ');
            if (Convert.ToInt32(token[0]) > Convert.ToInt32(token[1]))
            {
                small_value = token[1]; large_value = token[0];}
            else
            {
                small_value = token[0]; large_value = token[1];}
        }

        public string reverse(string input_string)
        {
            string output_string = null;
            for (int i = input_string.Length; i > 0; i--)
            {
                output_string += input_string[i - 1];
            }
            return output_string;
        }
        
        public void add()
        {
            string result = null;
            int carry = 0;
            int large_len = large_value.Length;
            int small_len = small_value.Length;
            double temp = 0;
            
            for (int i=1;i<large_len+1;i++)
            {
                if ( small_len -i >=0)
                    temp = Char.GetNumericValue(large_value[large_len-i]) + Char.GetNumericValue(small_value[small_len - i]) + carry;
                else
                    temp = Char.GetNumericValue(large_value[large_len - i]) + carry;
                carry = 0;

                if (temp > 9)
                {
                    temp -= 10;
                    carry = 1;
                }
                result += temp;

            }    
            if (carry == 1)
                result += 1;

            result = this.reverse(result);
            Console.WriteLine(result);

        }

        public void subtract()
        {
            string result = null;
            int carry = 0;
            int large_len = large_value.Length;
            int small_len = small_value.Length;
            double temp = 0;

            for (int i = 1; i < large_len + 1; i++)
            {
                if (small_len - i >= 0)
                    temp = Char.GetNumericValue(large_value[large_len - i]) - Char.GetNumericValue(small_value[small_len - i]) - carry;
                else
                    temp = Char.GetNumericValue(large_value[large_len - i]) - carry;
                carry = 0;

                if (temp < 0)
                {
                    temp += 10;
                    carry = 1;
                }
                result += temp;
            }
            if (carry == 1)
                result = result.Substring(0, result.Length - 1);

            result = this.reverse(result);
            Console.WriteLine(result);
        }

        

    }
    class problem1
    {
        static void Main(string[] args)
        {
            string input_string = null;

            Console.Write("숫자를 2개를 띄어쓰기로 입력하세요: ");
            input_string = Console.ReadLine();

            Two_Strings my_strings = new Two_Strings(input_string);
            my_strings.add();
            my_strings.subtract();
        }
    }
}
