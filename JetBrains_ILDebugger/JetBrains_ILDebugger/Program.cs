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

        static public long Counting (long a, long b)
        {
            return a + b * 2;
        }

        static public void PrintText(long a)
        {
            Console.WriteLine(a+1);
        }

        static void Main(string[] args)
        {
            ForTesting ft = new ForTesting();
            CallCompile cc = new CallCompile();
            string func_t = "1 + Counting(x, PrintText(z))";
            string func_long = "1 + Counting(x, t)";
            string ar_gram = @"10+3-4+2+5";
            string text = @"3*(1+2)+4";
            string text1 = @"123+043*14001";
            string normtext = @"(1+2)*8 - (3-9) + 4*5";
            string t = @"a*x + b*a ";
            string vartext = @"(1+2)*x - (3-y) + t+2*z"; // -1 wrong, 5 norm
            string proga = @"var a,b,c; 
                             a=2; b=3;
                             c = a*b;";
            //Lexer lexer = new Lexer(vartext);
            //List<Token> toks = lexer.Tokenization();
            //Parser parser = new Parser(toks);
            //ExprNode root = parser.MainProcess(); // works clear
            //string output = root.printAll();
            //Console.WriteLine(output);
            //cc.seekFunc("asd");
            //var result = cc.Compile(func_long);
            var result = cc.Compile(normtext);
            Console.WriteLine(result.Invoke(1,2,3));
           
            // here I will call delegate of DynamicMethod in ExprNode

            Console.ReadKey();
        }
    }

}


/*
    02.04.2020 чт
    Завершил 1 доп задание, но есть небольшой недочёт - могу вызывать только public методы, private уже не могу

    Задачи на 03.04.2020 пт
        
 */















namespace ILGenerationsExercises
{

    // ex_1
    //  int x, y;
    //  x=123;
    //  y=x+23;
    //  Console.WriteLine(y);

    //ex_2
    //  int x, y;
    //  x=24;
    //  y=x*2;
    //  if(x>y){
    //      x++;
    //  } 
    //  else{
    //      y+=x*3;
    //  }
    //  Console.WriteLine(x+y);

    class TestILGeneration
    {
        public void Execute()
        {
            Type[] types = new Type[] { typeof(int), typeof(string) };

            DynamicMethod dyn = new DynamicMethod("NewGeneratedMethod", null, null, typeof(void));
            ILGenerator ilgen = dyn.GetILGenerator();
            LocalBuilder x = ilgen.DeclareLocal(typeof(int));
            LocalBuilder y = ilgen.DeclareLocal(typeof(int));
            Label _then = ilgen.DefineLabel();
            Label _else = ilgen.DefineLabel();
            Label _jumpOverThen = ilgen.DefineLabel();

            // x = 24
            ilgen.Emit(OpCodes.Ldc_I4, 24);
            ilgen.Emit(OpCodes.Stloc, x);

            // y = x * 2;
            ilgen.Emit(OpCodes.Ldloc, x);
            ilgen.Emit(OpCodes.Ldc_I4, 2);
            ilgen.Emit(OpCodes.Mul);
            ilgen.Emit(OpCodes.Stloc, y);

            ilgen.Emit(OpCodes.Ldloc, y);
            ilgen.Emit(OpCodes.Ldloc, x);

            // if( x > y )
            ilgen.Emit(OpCodes.Blt, _then); // if x > y => _then

            //else y += x*3
            ilgen.MarkLabel(_else);
            ilgen.Emit(OpCodes.Ldloc, x);
            ilgen.Emit(OpCodes.Ldc_I4, 3);
            ilgen.Emit(OpCodes.Mul);
            ilgen.Emit(OpCodes.Ldloc, y);
            ilgen.Emit(OpCodes.Add);
            ilgen.Emit(OpCodes.Stloc, y);
            // create new label for jump over _then block
            ilgen.Emit(OpCodes.Br, _jumpOverThen);


            //then x++
            ilgen.MarkLabel(_then);
            ilgen.Emit(OpCodes.Ldc_I4, 1);
            ilgen.Emit(OpCodes.Ldloc, x);
            ilgen.Emit(OpCodes.Add);
            ilgen.Emit(OpCodes.Stloc, x);

            // here we avoid execution _then block
            ilgen.MarkLabel(_jumpOverThen);

            ilgen.Emit(OpCodes.Ldloc, x);
            ilgen.Emit(OpCodes.Ldloc, y);
            ilgen.Emit(OpCodes.Add);
            ilgen.Emit(OpCodes.Stloc, x);

            ilgen.EmitWriteLine(x);
            ilgen.Emit(OpCodes.Ret);
            dyn.Invoke(null, null);


            Console.ReadKey();
        }

        public void Example_2()
        {
            // ex3
            /*  int x, y;
                x = 20;
                y = 2 * x;
                int counter = 0;
                while (y > x)
                {
                    x += 3;
                    counter++;
                }
                Console.WriteLine(x);
                Console.WriteLine(counter);
            */
            DynamicMethod dyn = new DynamicMethod("Ex_3", null, null, typeof(void));
            ILGenerator gen = dyn.GetILGenerator();
            LocalBuilder x = gen.DeclareLocal(typeof(int));
            LocalBuilder y = gen.DeclareLocal(typeof(int));
            LocalBuilder counter = gen.DeclareLocal(typeof(int));

            Label _while = gen.DefineLabel();
            Label _endOfWhile = gen.DefineLabel();

            // x = 20;
            gen.Emit(OpCodes.Ldc_I4, 20);
            gen.Emit(OpCodes.Stloc, x);
            // y = 2*x;
            gen.Emit(OpCodes.Ldc_I4, 2);
            gen.Emit(OpCodes.Ldloc, x);
            gen.Emit(OpCodes.Mul);
            gen.Emit(OpCodes.Stloc, y);
            // counter = 0;
            gen.Emit(OpCodes.Ldc_I4, 0);
            gen.Emit(OpCodes.Stloc, counter);

            gen.MarkLabel(_while);
            gen.Emit(OpCodes.Ldloc, x);
            gen.Emit(OpCodes.Ldloc, y);

            gen.Emit(OpCodes.Bge, _endOfWhile); // if x >= y => _endOfWhile
            gen.Emit(OpCodes.Ldc_I4, 3);
            gen.Emit(OpCodes.Ldloc, x);
            gen.Emit(OpCodes.Add);
            gen.Emit(OpCodes.Stloc, x);
            gen.Emit(OpCodes.Ldc_I4, 1);
            gen.Emit(OpCodes.Ldloc, counter);
            gen.Emit(OpCodes.Add);
            gen.Emit(OpCodes.Stloc, counter);
            gen.Emit(OpCodes.Br, _while);

            gen.MarkLabel(_endOfWhile);
            gen.EmitWriteLine(x);
            gen.EmitWriteLine(counter);

            gen.Emit(OpCodes.Ret);
            dyn.Invoke(null, null);

            Console.ReadKey();
        }
    }
}





namespace RememberingKnowledge
{
    class Program1
    {
        delegate bool ValidatorType(int x, int limit);
        delegate bool ValOne(int a);
        static void Main1(string[] args)
        {
            int[] a = new int[] { 1, 2, 12, 43, 8, 5, -2 };
            int res = getSum(a, (x) => x > 5, 5);
            IEnumerable<int> b = a.Where((x) => x < 5);
            foreach (int i in b)
            {
                Console.WriteLine("&: " + i);
            }

            Console.WriteLine("\nanswer is " + res + "\n");
            string S = "Hello World, how are you doing?";
            int amount = S.CountChars('h');
            Console.WriteLine("\namount = " + amount);


            Console.ReadKey();
        }

        static int getSum(int[] arr, ValOne Validator, int limit)
        {
            int res = 0;
            for (int i = 0; i < arr.Length; ++i)
                if (getRes(arr[i], limit))
                    res += arr[i];
            return res;
        }

        public static bool getRes(int a, int b) => a > b;

        static bool isValid(int x, int limit)
        {
            if (x > limit) return true;
            return false;
        }

    }

    public static class ExtentionClass
    {
        public static int CountChars(this string str, char letter)
        {
            int counter = 0;
            for (int i = 0; i < str.Length; ++i)
                if (str[i] == letter) counter++;
            return counter;
        }
    }
}