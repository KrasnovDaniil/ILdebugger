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

        static public void PrintText(long a)
        {
            Console.WriteLine(a + 1);
        }

        static void Main(string[] args)
        {
            ForTesting ft = new ForTesting();
            CallCompile cc = new CallCompile();
            string func_t = "1 + Counting(x, PrintText(z))";
            string func_long = "1 + Counting(x, t)";
            string normtext = @"(1+2)*8 - (3-9) + 4*5";
            string t = @"a*x + b*a ";
            string vartext = @"(1+2)*x - (3-y) + t+2*z"; // -1 wrong, 5 norm
            string proga = @"var a,b,c; 
                             a=2; b=3;
                             c = a*b;
                            if(a+b-c*2){
                            return a;}
                            else {
                            return b;
                            }";
            var result = cc.Compile(proga);
            Console.WriteLine(result.Invoke(1, 2, 3));

            Console.ReadKey();
        }
    }
}
