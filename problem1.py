## 숫자로 구성된 두 문자열을 더하는 함수와 빼는 함수를 작성하시오
def string_sort(string_list):
    if int(string_list[1]) > int(string_list[0])  :
        string_list[0], string_list[1] = string_list[1], string_list[0]
    return string_list

def string_reverse(input_string):
    output_string =''
    for i in range(1,len(input_string)+1):
        output_string += input_string[-i]
    return output_string

def add_between(input_string):
    string_list = input_string.split()
    string_list = string_sort(string_list)

    carry = 0
    N = len(string_list[0]) ; n = len(string_list[1])
    output = ''

    for i in range(1,N+1):
        temp = int(string_list[0][-i]) + carry
        carry = 0
        if i<=n :
            temp += int(string_list[1][-i])
        if temp > 9:
            temp -= 10
            carry = 1
        output += "%d" % temp

    if(carry == 1):
        output += '1';

    print(string_reverse(output))

def subtract_between(input_string):
    string_list = input_string.split()
    string_list = string_sort(string_list)

    carry = 0
    N = len(string_list[0]);
    n = len(string_list[1])
    output = ''

    for i in range(1, N + 1):
        temp = int(string_list[0][-i]) - carry
        carry = 0
        if i <= n:
            temp -= int(string_list[1][-i])
        if temp < 0:
            temp += 10
            carry = 1
        output += "%d" % temp

    if (carry == 1):
        output = output[:-1]

    print(string_reverse(output))



def main() :
    input_string = input("숫자를 2개를 띄어쓰기로 입력하세요: ")
    add_between(input_string)
    subtract_between(input_string)

main()