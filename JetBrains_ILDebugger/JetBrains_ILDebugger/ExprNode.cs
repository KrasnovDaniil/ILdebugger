using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;
using System.Reflection;

namespace JetBrains_ILDebugger
{
    class ExprNode
    {
        TokenType type;
        public string name { get; protected set; }
        public long val;

        public ExprNode(TokenType type, string name)
        {
            this.type = type;
            this.name = name;
        }


        //static public void print()
        //{
        //    LocalBuilder result = comp.gen.DeclareLocal(typeof(int));
        //    comp.gen.Emit(OpCodes.Stloc, result);
        //    comp.gen.EmitWriteLine(result);
        //    comp.gen.Emit(OpCodes.Ret);
        //    comp.dyn.Invoke(null, null);
        //}

        //static public object result()
        //{
        //    comp.gen.Emit(OpCodes.Ret);
        //    return dyn.Invoke(null, null);
        //}

        virtual public string printAll()
        {
            return name;
        }

        virtual public void CodeGen(CallCompile cc)
        {
             // here will be IL code comp.generation...(the most interesting part)
        }
    }

    class VarNode : ExprNode
    {
        
        TokenType type;
        public VarNode(TokenType type, string name) : base(type, name)
        {
            this.type = type;
            this.name = name;
            //val = Convert.ToInt32(name);
        }

        public override void CodeGen(CallCompile cc)
        {
            /* 18:10  30.03.2020
            Доп задание 2
            Реализовал работу со статическими переменными класса Program так, чтобы они использовались в вычилениях
            так же как и обычные переменные
            */

            LocalBuilder x1 = cc.gen.DeclareLocal(typeof(long));

            long var = 0;
            if      (name == "x") cc.gen.Emit(OpCodes.Ldarg_0);
            else if (name == "y") cc.gen.Emit(OpCodes.Ldarg_1);
            else if (name == "z") cc.gen.Emit(OpCodes.Ldarg_2);
            else if (cc.seekVar(name, out var))
                cc.gen.Emit(OpCodes.Ldc_I8, var);
            else  // нужно поместить вторым аргументом поле из массива из getFieldsInfo() 30.03.2020
                  //else cc.gen.Emit(OpCodes.Ldsfld,);
                Console.WriteLine("wrong variable");
        }
    }

    class NumberNode : ExprNode
    {
        //public int val { get; private set; }
        public NumberNode(TokenType type, string name, long val) : base(type, name)
        {
            this.val = val;
        }
        public override string printAll()
        {
            return val.ToString();
        }
        public override void CodeGen(CallCompile cc)
        {
            cc.gen.Emit(OpCodes.Ldc_I8,val);
        }
    }

    class BinaryOperation :ExprNode
    {
        TokenType opName;
        ExprNode op1, op2;
        public BinaryOperation(TokenType opName, ExprNode op1, ExprNode op2):base(opName, null)
        {
            this.opName = opName;
            this.op1 = op1;
            this.op2 = op2;
        }

        public override string printAll()
        {
            return "(" + op1.printAll() + " " + opName.ToString() + " " + op2.printAll() + ")";
        }

        public void ComputingValue()
        {
            /// computiong: val = op1 opName op2
        }

        public override void CodeGen(CallCompile cc)
        {

            // можно убрать проверку на null, так как исключение бцдет выброшено на этапе парсинга
            if (!(op1 is FunctionNode obj1 && obj1.return_type == TokenType.VOID)) op1.CodeGen(cc);
            else
            {
                // exception hadling
                Console.WriteLine("Wrong type of 1 operand");
                return;
            }
            if (!(op2 is FunctionNode obj2 && obj2.return_type == TokenType.VOID)) op2.CodeGen(cc);
            else
            {
                // exception handling
                Console.WriteLine("Wrong type of 2 operand");
                return;
            }
            GenILOps(cc);
        }
        public void GenILOps(CallCompile cc)
        {
            switch (opName)
            {
                case TokenType.PLUS:
                    cc.gen.Emit(OpCodes.Add); break;
                case TokenType.MINUS:
                    cc.gen.Emit(OpCodes.Sub); break;
                case TokenType.MUL:
                    cc.gen.Emit(OpCodes.Mul); break;
                case TokenType.DIV:
                    cc.gen.Emit(OpCodes.Div); break;
            }
        }   
    }

    // Идея. Проверять функцию на этапе парсинга таким образом: если среди аргументов встретился тип void (функция типа void), то
    // выбрасывать исключение о неправильном параметре
    class FunctionNode : ExprNode
    {
        public List<ExprNode> args { get; private set; }
        public TokenType return_type { get; private set; }
        public MethodInfo selfMethod { get; private set; }
        public FunctionNode(string name, List<ExprNode> args, TokenType retType = TokenType.LONG): base(TokenType.FUNCTION, name)
        {
            this.args = args;
            return_type = retType;
        }

        public void funcIdentification(CallCompile cc)
        {
            // тут опеределим её тип и кол- во параметров (сигнатутру)
            // и найдём соответствующий ей метод в классе
            MethodInfo mi;
            bool isExist = cc.seekFunc(this, out mi);
            selfMethod = mi;
            if (isExist && selfMethod.ReturnType == typeof(void)) return_type = TokenType.VOID;
            else if (isExist && selfMethod.ReturnType == typeof(long)) return_type = TokenType.LONG;
            else
            {
                // exception handling
                Console.WriteLine("this function doesn't exist in this scope");
                return;
            }

        }

        // Найти эту функцию в списке объявленных статических методов класса Program.cs
        public override void CodeGen(CallCompile cc)
        { /* метод CodeGen(CallCompile cc) генерирует IL-код ПРАВИЛЬНОЙ функции, то есть ту, которая не имеет во входных 
             параметрах агрумент типа void
          */
            GenerateArgs(cc);
            //selfMethod.Invoke(p, new object[] { , (long)args[1].val });
            cc.gen.Emit(OpCodes.Call, selfMethod);

            //cc.gen.Emit(OpCodes.Ldc_I8,args[0].val);
            //cc.gen.Emit(OpCodes.Ldc_I8,args[1].val);
            //cc.gen.Emit(OpCodes.Add);
        }

        public void GenerateArgs(CallCompile cc)
        {
            foreach (ExprNode en in args)
                en.CodeGen(cc);
        }

    }

    class IfNode : ExprNode
    {
        private ExprNode cond, body, _else;
        public IfNode(ExprNode cond, ExprNode body, ExprNode _else) : base(TokenType.IF, "if")
        {
            this.cond = cond;
            this.body = body;
            this._else = _else;
        }
    }

    class ElseNode : ExprNode
    {
        private ExprNode body;
        public ElseNode(ExprNode body) : base(TokenType.ELSE, "else")
        {
            this.body = body;
        }
    }

}
