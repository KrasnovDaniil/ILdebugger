﻿using System;
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
        static long asd = 4;

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
            CallCompile cc = new CallCompile();
            string text = @"a*(x+y)+z*b*asd"; // = 13
            string vartext = @"(1+2)*x - (3-y) + t+2*z"; // = 18 
            
            var result = cc.Compile(vartext);
            Console.WriteLine(result.Invoke(1, 2, 3));

            Console.ReadKey();
        }
    }

}   