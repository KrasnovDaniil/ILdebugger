using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;
using System.Reflection;
using System.Globalization;
using System.Collections;


namespace JetBrains_ILDebugger
{
    class CallCompile
    {
        public DynamicMethod dyn { get; private set; }
        public ILGenerator gen { get; private set; }
        public SymbolTable symboltable;
        public delegate long CompileResult(long x, long y, long z);
        static public FieldInfo[] sFields { get; private set; }
        static public MethodInfo[] sMethods { get; private set; }
        
        public CallCompile()
        {
            symboltable = new SymbolTable();
            Type[] types_xyz = new Type[] { typeof(long), typeof(long), typeof(long) };
            dyn = new DynamicMethod("Compile", typeof(long), types_xyz, typeof(CallCompile).Module);
            gen = dyn.GetILGenerator();
            getFieldsMethodsInfo();
        }


        public void showTopOfStack()
        {
            LocalBuilder x = gen.DeclareLocal(typeof(long));
            gen.Emit(OpCodes.Stloc, x);
            gen.EmitWriteLine(x);
        }
        
        public CompileResult CompileAllExprNodes(ExprNode root) // тот самый метод Compile()
        {
            root.CodeGen(this);
            gen.Emit(OpCodes.Ret);
            CompileResult res = (CompileResult)dyn.CreateDelegate(typeof(CompileResult));
            return res;
        }

        public CompileResult Compile(string expression)
        {
            Lexer lexer = new Lexer(expression);
            List<Token> toks = lexer.Tokenization();
            Parser parser = new Parser(toks, this);
            ExprNode root = parser.MainProcess();
            var res = CompileAllExprNodes(root);
            return res;
        }

        public void getFieldsMethodsInfo()
        {
            Type type = typeof(Program);
            sFields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            sMethods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
        }

        public bool seekVar(string name, out long result)
        {
            result = 0;
            foreach (FieldInfo fi in sFields)
                if (fi.Name == name)
                {
                    result = (long)fi.GetValue(null);
                    return true;
                }
            return false;
        }

        public bool seekFunc(FunctionNode func, out MethodInfo res_func)
        {
            res_func = null;
            foreach (MethodInfo mi in sMethods)
            {   // нужно обработать ошибку args = null, она возникает, когда один из аргументов имеет тип void
                if (mi.Name == func.name && mi.GetParameters().Length == func.args.Count)
                {
                    res_func = mi;
                    return true;
                }
            }
            return false;

            foreach (MethodInfo fi in sMethods)
            {
                Console.WriteLine(fi.Name);
                Console.WriteLine("return type " + fi.ReturnType);
                foreach (ParameterInfo pi in fi.GetParameters()) // вывод параметров статического метода 
                {
                    Console.WriteLine(pi.ParameterType); // могу сделать проверку сигнатур (достаточно списока параметров)
                    // объявленного метода и вызванного метода в expressions 
                }
            }
            return false;
        }

        /*
         * Задачи на 31.03.2020
          Сделать доп задание 1 - внедрение функций:
            * проверить на соответствие кол-во (и типы параметров) у объявленных и вызываемых методов
            * добавить обработку этих же функций в парсер: оперделять их на этапе лексинга или парсинга?
          другое:
            исправить грамматику: чтобы создавала правильное АСТ
         */



    }
}
