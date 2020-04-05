using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;
using System.Reflection;
using System.Globalization;

namespace JetBrains_ILDebugger
{
    class ForTesting
    {
        Type[] types;
        DynamicMethod dd;
        ILGenerator g;
        ExprNode e = new ExprNode(TokenType.ASSIGN, "asd");
        public delegate int Res(int x, int y, int z);
        static public int _x_ = 101;
        static private string s = "It is a string";
        static public FieldInfo[] sf { get; private set; }
        static public MethodInfo[] sm { get; private set; }


        public ForTesting()
        {
            types = new Type[] { typeof(int), typeof(int), typeof(int) };
            //dd = new DynamicMethod("asddsa", typeof(int), types, typeof(void));
            dd = new DynamicMethod("asddsa", typeof(int), types, typeof(ForTesting).Module);
            g = dd.GetILGenerator();
            getFieldsMethodsInfo();
            var res = execute();
            int r = res.Invoke(1, 2, 3);
            Console.WriteLine(r);
            //_main();
            //res.Invoke(12);
            //object objRet = dd.Invoke(null, BindingFlags.ExactBinding, null, new object[] { 23 }, new CultureInfo("en-us"));

        }

        public void getFieldsMethodsInfo()
        {
            Type type = typeof(ForTesting);
            sf = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            sm = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
        }

        public MethodInfo getFunction(string name)
        {
            foreach(MethodInfo mi in sm)
            {
                if (mi.Name == name)
                    return mi;
            }
            return null;
        }

        public void _main()
        {
            FieldInfo[] myfieldsInfo;
            Type type = typeof(ForTesting);
            myfieldsInfo = type.GetFields(BindingFlags.Public| BindingFlags.NonPublic | BindingFlags.Static);
            foreach(FieldInfo fi in myfieldsInfo)
            {
                Console.WriteLine("Name " + fi.Name);
                Console.WriteLine("Type " + fi.DeclaringType);
                Console.WriteLine("is public ? " + fi.IsPublic);
                Console.WriteLine("member type " + fi.MemberType);
                Console.WriteLine("is family " + fi.IsFamily);
                Console.WriteLine("field type " + fi.FieldType);
                Console.WriteLine();
            }
        }


        public Res execute()
        {
            MethodInfo mm = getFunction("Computing");
            LocalBuilder x = g.DeclareLocal(typeof(int));
            g.Emit(OpCodes.Ldarg_0);
            g.Emit(OpCodes.Ldarg_1);
            //g.Emit(OpCodes.Ldarg_2);
            //g.Emit(OpCodes.Add);
            //g.Emit(OpCodes.Add);
            //g.Emit(OpCodes.Dup);
            //g.Emit(OpCodes.Stloc, x);
            //g.Emit(OpCodes.Call, typeof(Console).GetMethod("Write", new Type[] { typeof(long) }));
            //g.Emit(OpCodes.Ldc_I4, 2);
            //g.Emit(OpCodes.Ldc_I4, 12);
            //g.Emit(OpCodes.Mul);
            //g.Emit(OpCodes.Stloc,x);
            //g.EmitCall(OpCodes.Call, mm,new Type[] { typeof(long)});
            //g.EmitCall(OpCodes.Call,mm,null);
            g.Emit(OpCodes.Call, mm);
            //g.EmitWriteLine(x);
            Console.WriteLine(x);
            g.Emit(OpCodes.Ret);
            Res res = (Res)dd.CreateDelegate(typeof(Res));
            //dd.Invoke(null, new object[] { 1,2,3});
            res.Invoke(1, 2, 3);
            Console.WriteLine();
            return res;
        }

        public static int Computing(int a, int b)
        {
            return 2 * a + b;
        }

    }
}
