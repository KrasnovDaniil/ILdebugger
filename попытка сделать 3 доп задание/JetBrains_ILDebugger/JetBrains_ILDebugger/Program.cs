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

        /*
         Дополнительное задание 3 не сделано до конца
         Программа только строит АСТ, без генерации кода (ещё нет обработки логических выражений,
         например для if (boolExpr)
         */

        static void Main(string[] args)
        {
            CallCompile cc = new CallCompile();
            string textOfProgram = 
                @"var a,b,c; 
                a=2; b=3;
                c = a*b;
                if(a+b-c*2){
                    return a;
                }
                else {
                    return b;
                }";
            var result = cc.Compile(textOfProgram);
            Console.WriteLine(result.Invoke(1, 2, 3));

            Console.ReadKey();
        }
    }
}
