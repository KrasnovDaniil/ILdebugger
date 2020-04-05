using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetBrains_ILDebugger
{

    class ArithmeticRules
    {
        /*
            Stmt -> Expr_plus| e
            Expr_plus -> Expr_minus| Expr_minus + Expr_plus| e
            Expr_minus -> Term_mul| Expr_minus - Term_mul| e
            Term_mul -> Term_div| Term_div * Term_div| e
            Term_div -> Fact| Term_div / Term_mul| e
            Fact -> NUM| ( Expr_plus )| VAR| Function
        */

        Parser parser;
        public ArithmeticRules(Parser parser)
        {
            this.parser = parser;
        }

        public ExprNode ParseArithmeticExpression()
        {
            return Stmt(parser.curToken);
        }

        public ExprNode Stmt(Token token)
        {
            ExprNode res = null;
            switch (token.type)
            {
                case TokenType.LPAR:
                case TokenType.NUM:
                case TokenType.VAR:
                    // expr может начинаться с открывающейся скобки или числа 			
                    res = Expr_plus(token);
                    break;
                default:
                    break;
            }
            return res;
        }

        public ExprNode Expr_plus(Token token)
        {
            ExprNode leftBranch;
            ExprNode rightBranch;
            switch (token.type)
            {
                case TokenType.NUM:
                case TokenType.VAR:
                case TokenType.LPAR:
                    leftBranch = Expr_minus(token);

                    Token op = parser.getNextToken();

                    switch (op.type)
                    {
                        case TokenType.PLUS:
                            token = parser.getNextToken();
                            rightBranch = Expr_plus(token);
                            return new BinaryOperation(op.type, leftBranch, rightBranch);
                        case TokenType.RPAR:
                            parser.StepBack();
                            // Если токен равен закрывающейся скобке, возвращаем его назад в строку
                            break;
                        default:
                            return leftBranch;
                    }

                    break;
                default:
                    return null; // error
            }
            return leftBranch;

        }

        public ExprNode Expr_minus(Token token)
        {
            ExprNode leftBranch;
            ExprNode rightBranch;
            switch (token.type)
            {
                case TokenType.NUM:
                case TokenType.VAR:
                case TokenType.LPAR:
                    leftBranch = Term_mul(token);

                    Token op = parser.getNextToken();

                    switch (op.type)
                    {
                        case TokenType.MINUS:
                            // посмотрю сколько минусов впереди
                            if (parser.StepNext(2).type == TokenType.MINUS)
                            {
                                Token t_past = parser.StepBack();
                                rightBranch = expr_minus(t_past, leftBranch);
                            }
                            else
                                return new BinaryOperation(op.type, leftBranch, Term_mul(parser.getNextToken()));
                            return rightBranch;
                        case TokenType.RPAR:
                            parser.StepBack();
                            break;
                        default:
                            parser.StepBack();
                            return leftBranch;
                    }

                    break;
                default:
                    return null; // ERROR handlig
            }
            return leftBranch; // error too

        }



        public ExprNode Term_mul(Token token)
        {
            Token op;
            ExprNode leftB = null, rightB;

            switch (token.type)
            {
                case TokenType.LPAR:
                case TokenType.VAR:
                case TokenType.NUM:
                    leftB = Term_div(token);
                    op = parser.getNextToken();
                    switch (op.type)
                    {
                        case TokenType.MUL:
                            rightB = Term_mul(parser.getNextToken());
                            return new BinaryOperation(op.type, leftB, rightB);
                        case TokenType.PLUS:
                        case TokenType.MINUS:
                        case TokenType.RPAR:
                            parser.StepBack(); // вернулись назад если не получилось раскрыть правило
                            break;
                    }
                    break;
            }
            return leftB; // error handling

        }

        public ExprNode Term_div(Token token)
        {
            Token op;
            ExprNode leftBranch = null, rightBranch;

            switch (token.type)
            {
                case TokenType.LPAR:
                case TokenType.VAR:
                case TokenType.NUM:
                    leftBranch = Fact(token);
                    op = parser.getNextToken();
                    switch (op.type)
                    {
                        case TokenType.DIV:
                            if (parser.StepNext(2).type == TokenType.DIV)
                            {
                                Token t_past = parser.StepBack();
                                rightBranch = term_div(t_past, leftBranch);
                            }
                            else
                                return new BinaryOperation(op.type, leftBranch, Term_mul(parser.getNextToken()));
                            return rightBranch;
                        case TokenType.PLUS:
                        case TokenType.MUL:
                        case TokenType.MINUS:
                        case TokenType.RPAR:
                            parser.StepBack();
                            break;
                    }
                    break;
            }
            return leftBranch; // error handling
        }


        public ExprNode Fact(Token token)
        {
            Token tmp_token;

            switch (token.type)
            {
                case TokenType.NUM:
                    return new NumberNode(token.type, token.name, token.val);
                case TokenType.VAR:
                    if (parser.getNextToken().type == TokenType.LPAR)
                    {
                        FunctionNode fn = (FunctionNode)parser.parseFunct(token);
                        fn.funcIdentification(parser._compile);
                        return fn;
                    }
                    else
                    {
                        parser.StepBack();
                        VarNode var = new VarNode(token.name);
                        return var;
                    }
                case TokenType.LPAR:
                    tmp_token = parser.getNextToken();
                    ExprNode res = Expr_plus(tmp_token);
                    tmp_token = parser.getNextToken();
                    if (tmp_token.type != TokenType.RPAR)
                        Console.WriteLine(") exected here");
                    return res;
            }
            return null; // error handling
        }



        public ExprNode expr_minus(Token token, ExprNode op)
        {
            BinaryOperation bo;
            ExprNode en = null;
            if (parser.getNextToken().type == TokenType.MINUS && parser.StepNext(2).type == TokenType.MINUS)
            {
                if (op == null)
                    bo = new BinaryOperation(TokenType.MINUS, Term_mul(token), Term_mul(parser.getNextToken()));
                else
                    bo = new BinaryOperation(TokenType.MINUS, op, Term_mul(parser.getNextToken()));
                en = expr_minus(parser.curToken, bo);
            }
            else if (parser.curToken.type == TokenType.MINUS)
                if (op != null)
                    en = new BinaryOperation(TokenType.MINUS, op, Term_mul(parser.getNextToken()));
                else
                    en = new BinaryOperation(TokenType.MINUS, Term_mul(token), Term_mul(parser.getNextToken()));
            return en;
        }

        public ExprNode term_div(Token token, ExprNode op)
        {
            BinaryOperation bo;
            ExprNode en = null;
            if (parser.getNextToken().type == TokenType.DIV && parser.StepNext(2).type == TokenType.DIV)
            {
                if (op == null)
                    bo = new BinaryOperation(TokenType.DIV, Fact(token), Fact(parser.getNextToken()));
                else
                    bo = new BinaryOperation(TokenType.DIV, op, Fact(parser.getNextToken()));
                en = term_div(parser.curToken, bo);
            }
            else if (parser.curToken.type == TokenType.DIV)
                if (op != null)
                    en = new BinaryOperation(TokenType.DIV, op, Fact(parser.getNextToken()));
                else
                    en = new BinaryOperation(TokenType.DIV, Fact(token), Fact(parser.getNextToken()));
            return en;
        }

    }
}