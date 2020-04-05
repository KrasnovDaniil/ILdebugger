using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;
using System.Reflection;
using System.Globalization;
using System.Collections;


/*
 Задачи (на завтра):
    1) Разобраться с возвращаемым делегатом, то есть определить тип, кол-во параметров CompileRes Compile(string expr) / long CompileRes(x,y,z);
    1.1) чтобы метод принимал 3 переменные x, y, z и возвращал делегат с посчитанным значением
    2) Использование внешних статических переменных (вне expression) 
    3) Создание таблицы символов для x,y,z и не только для них

    Доп.Задание 1
        1) Сделать возможность использования статических методов текущего класса в expression
    Доп.Задание 2
        1) Сделать аналогичный доступ к переменным кроме x,y,z
    Доп.ЗАдание 3
        # организовать компиляцию целой программы из нескольких выражений
        1) Добавить объявление переменных (любых) в expression
        2) Добавить операцию присваивания а = 3
        3) Добавить операторы if(...) else
        4) Добавить true, false бинарные операторы >, <, >=, <=, ==, !=, ||, && и !
        5) Добавить return
        6) Сделать return для любого метода **
        7) для void **
     */

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
        //public Hashtable mapFields;
        
        public CallCompile()
        {
            symboltable = new SymbolTable();
            Type[] types_xyz = new Type[] { typeof(long), typeof(long), typeof(long) };
            dyn = new DynamicMethod("Compile", typeof(long), types_xyz, typeof(CallCompile).Module);
            // delegate long CompileResult(long x, long y, long z);
            // CompileResult Compile(string expression);
            // var result = Compile( @"2 + x * y / z + 4");
            // result.Invoke(x, y, z);
            //dyn = new DynamicMethod("Compile", typeof(int), null, typeof(void));
            //dyn = new DynamicMethod("Compile", null, null, typeof(void));
            gen = dyn.GetILGenerator();
            getFieldsMethodsInfo();
        }


        public void showTopOfStack()
        {
            LocalBuilder x = gen.DeclareLocal(typeof(long));
            gen.Emit(OpCodes.Stloc, x);
            gen.EmitWriteLine(x);
        }

         /*
         Получилось устранить ошибку с аргументами (попробовал разобраться в классе ForTesting)
         Теперь надо исправить эту ошибку здесь
         res.Invoke() выбрасывает исключение, надо рабозраться почему
         мб это связано со значением на стеке, нужно будет проверить как скомпилировался IL
         [Разобрался]
         */

        public CompileResult CompileAllExprNodes(ExprNode root) // тот самый метод Compile()
        {
            root.CodeGen(this);
            //gen.Emit(OpCodes.Dup);
            //LocalBuilder x1 = gen.DeclareLocal(typeof(long));
            //gen.Emit(OpCodes.Stloc, x1);
            //gen.EmitWriteLine(x1);
            gen.Emit(OpCodes.Ret);
            object[] objs = new object[] { 1, 23, 34 };
            //object objRet = dyn.Invoke(null, BindingFlags.ExactBinding, null, new object[] { 1, 23, 34 }, new CultureInfo("en-us"));
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
