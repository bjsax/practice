using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;

namespace StackAlgorithm
{
    struct Vector
    {
        public int X;
        public int Y;
        public int value;

        public Vector(int x, int y, int v)
        {
            this.X = x;
            this.Y = y;
            this.value = v;
        }

        public override string ToString()
        {
            return "X: " + X + ", Y = " + Y + ", Value = " + value;
        }
    }

    public enum MazeState
    {   Wall   = 0,
        Road   = 1,
        Passed = 2,
        Start  = 3,
        End    = 4
    }

    class Stack
    {
        private Vector[] stack;
        private int sp = -1;
        private int size;

        public Stack(int size)
        {
            this.size = size;
            stack = new Vector[size];
        }

        public bool Push(int x, int y, int v)
        {
            if (IsFull())
                return false;

            stack[++sp] = new Vector(x, y, v);
            return true;
        }

        public void Pop()
        {
            if (!IsEmpty())
                sp--;
        }

        public Vector Top()
        {
            return IsEmpty() 
                ? new Vector(-1, -1, -1) 
                : stack[sp];
        }

        public bool IsEmpty()
        {
            return sp < 0;
        }

        public bool IsFull()
        {
            return sp >= size - 1;
        }
    }

    class MazeSolver
    {
        int r, p, s, e;
        public MazeSolver()
        {
            r = (int)MazeState.Road;
            p = (int)MazeState.Passed;
            s = (int)MazeState.Start;
            e = (int)MazeState.End;
        }

        public List<string> Solve(int[,] map)
        {
            var stack = new Stack(map.Length);
            stack = initialize(stack, map);

            int iter = 0;
            while (true)
            {
                if (stack.Top().value == r)
                {
                    findFourNeighbors(stack, map);
                }
                else if (stack.Top().value == e)
                {
                    break;
                }
                else
                {
                    stack.Pop();

                    if (stack.IsEmpty())
                        break;
                }

                if (iter++ > 100000)
                    break;
            }
            
            return stackToList(stack);
        }
        
        public int[,] TextToMap(string txt)
        {
            var stringArray = txt.Replace("\r\n", " ").Split();

            var length = (int)Math.Sqrt(stringArray.Length);
            var map = new int[length, length];

            var k = 0;
            for (int i = 0; i < length; i++)
                for (int j = 0; j < length; j++)
                    map[i, j] = int.Parse(stringArray[k++]);

            return map;
        }

        private Stack initialize(Stack stack, int[,] map)
        {
            var x = startPointOf(map)[0];
            var y = startPointOf(map)[1];

            if (!stack.Push(x, y, map[x, y]))
                return null;

            stack = findFourNeighbors(stack, map);

            return stack;
        }

        private int[] startPointOf(int[,] map)
        {
            var length = Math.Sqrt(map.Length);

            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    if (map[i, j] == s)
                        return new int[2] { i, j };
                }
            }

            return new int[2] { -1, -1 };
        }

        private Stack findFourNeighbors(Stack stack, int[,] map)
        {

            var x = stack.Top().X;
            var y = stack.Top().Y;
            var length = Math.Sqrt(map.Length);

            stack.Pop();
            stack.Push(x, y, p);
            map[stack.Top().X, stack.Top().Y] = p;

            if (x - 1 >= 0 && (map[x - 1, y] == r || map[x - 1, y] == e))
                stack.Push(x - 1, y, map[x - 1, y]);

            if (y - 1 >= 0 && (map[x, y - 1] == r || map[x, y - 1] == e))
                stack.Push(x, y - 1, map[x, y - 1]);

            if (x + 1 < length && (map[x + 1, y] == r || map[x + 1, y] == e))
                stack.Push(x + 1, y, map[x + 1, y]);

            if (y + 1 < length && (map[x, y + 1] == r || map[x, y + 1] == e))
                stack.Push(x, y + 1, map[x, y + 1]);

            return stack;
        }

        private List<string> stackToList(Stack stack)
        {
            var list = new List<string>();

            while (!stack.IsEmpty())
            {
                if (stack.Top().value == p || stack.Top().value == e)
                    list.Add(stack.Top().ToString());

                stack.Pop();
            }
            list.Reverse();

            return list;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var solver = new MazeSolver();

            var mapText = File.ReadAllText(@"C:\Users\HancomGMD\Desktop\maze.txt");

            var map = solver.TextToMap(mapText);
            
           /* var map = new int[5, 5]
            {
                { 3, 1, 1, 1, 1},
                { 1, 0, 0, 1, 0},
                { 1, 1, 1, 1, 1},
                { 0, 1, 0, 0, 1},
                { 1, 1, 1, 0, 4}
            };*/
            
            var solution = solver.Solve(map);

            foreach (var str in solution)
                Console.WriteLine(str);
        }
    }
}