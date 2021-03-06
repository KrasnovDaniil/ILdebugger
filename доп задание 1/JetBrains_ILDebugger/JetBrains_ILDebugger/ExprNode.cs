﻿using System;
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

        virtual public string printAll()
        {
            return name;
        }

        virtual public void CodeGen(CallCompile cc)
        {
        }
    }

    class VarNode : ExprNode
    {
        private ExprNode val;
        TokenType type;

        public VarNode(string name) : base(TokenType.VAR, name)
        {
            this.name = name;
        }

        public void setValue(ExprNode value) { val = value; }

        public override void CodeGen(CallCompile cc)
        {

            LocalBuilder x1 = cc.gen.DeclareLocal(typeof(long));

            long var = 0;
            if      (name == "x") cc.gen.Emit(OpCodes.Ldarg_0);
            else if (name == "y") cc.gen.Emit(OpCodes.Ldarg_1);
            else if (name == "z") cc.gen.Emit(OpCodes.Ldarg_2);
            else if (cc.seekVar(name, out var))
                cc.gen.Emit(OpCodes.Ldc_I8, var);
            else  
                Console.WriteLine("wrong variable");
        }
    }

    class NumberNode : ExprNode
    {
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

        public override void CodeGen(CallCompile cc)
        {

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
        { 
            GenerateArgs(cc);
            cc.gen.Emit(OpCodes.Call, selfMethod);
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

        public override void CodeGen(CallCompile cc)
        {
            base.CodeGen(cc);
        }
    }

    class ElseNode : ExprNode
    {
        private ExprNode body;
        public ElseNode(ExprNode body) : base(TokenType.ELSE, "else")
        {
            this.body = body;
        }
        public override void CodeGen(CallCompile cc)
        {
            base.CodeGen(cc);
        }
    }

    class ConnectingNode : ExprNode
    {
        public ExprNode leftPart, rightPart;
        public ConnectingNode(ExprNode left, ExprNode right) : base(TokenType.SEMICOLON, "Connection")
        {
            leftPart = left;
            rightPart = right;
        }

        public override void CodeGen(CallCompile cc)
        {
            if (leftPart != null) leftPart.CodeGen(cc);
            if (rightPart != null) rightPart.CodeGen(cc);
        }
    }

    class OperationNode : ExprNode
    {
        private TokenType type;
        private ExprNode expr;
        public OperationNode(TokenType type, ExprNode expr):base(type, null)
        {
            this.type = type;
            this.expr = expr;
        }

        public override void CodeGen(CallCompile cc)
        {
            base.CodeGen(cc);
        }

    }



}
