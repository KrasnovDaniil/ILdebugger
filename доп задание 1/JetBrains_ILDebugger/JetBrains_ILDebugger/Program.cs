using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;
using System.Reflection;

namespace JetBrains_ILDebugger
{

    class Program
    {
        static long t = 10;
        static long a = 3;
        static long b = 2;

        static public long Counting(long a, long b)
        {
            return a + b * 2;
        }

        static public long OtherFunc(long a)
        {
            return a * a + 1;
        }

        static public void PrintText(long a)
        {
            Console.WriteLine(a + 1);
        }

        static void Main(string[] args)
        {
            CallCompile cc = new CallCompile();
            string func_t = "1 + Counting(x, OtherFunc(3)) * Counting(t,y) + 4"; // = 299
            string func_long = "1 + Counting(x, t)"; // = 22
            string ar_gram = @"10+3-4-2-5"; // = 2
            string text = @"3*(1+2)+4"; // = 13
            string vartext = @"(1+2)*x - 3 - y + t+2*z"; // = 14

            var result = cc.Compile(func_t);
            Console.WriteLine(result.Invoke(1, 2, 3));


            Console.ReadKey();
        }
    }

}

    