using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetBrains_ILDebugger
{

    // вобще для реализации кодогенерации можно было использовать шаблон проектирования Visitor,
    // но так как мы всегда будем генерировать IL ( и пока я не вижу причин для такого абстрагирования),
    // то не будем его использовать, а просто напишем внутри каждого узла метод для генерации IL кода


    /*
    Заметка: можно оставить текущюю грамматку. Проблема была в том, что грамматика строила неправильное дерево разбора - 
    "минус" был в вершине дерева: 1-2+3 = -4, а не 2
    Прикол в том, что можно не менять грамматику, так как мы будем генерировать IL во время построения дерева, и это не будет противоречить 
    принципам компиляции, так как это всего лишь генерация, а не исполнение
    
    */    

    class ArithmeticRules
    {
        /*
            stmt -> expr| e
            expr -> term| term + expr| term - expr | e
            term -> fact| fact * term| fact / term| e
            fact -> NUM| ( expr )| VAR
        */

        List<Token> expression;
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
                     
                    switch (op.type) // look ahead
                    {
                        case TokenType.PLUS:
                            token = parser.getNextToken();
                            rightBranch = Expr_plus(token);
                            return new BinaryOperation(op.type, leftBranch, rightBranch); // expr -> term + expr| term - expr
                        case TokenType.RPAR:
                            parser.StepBack();
                            // Если токен равен закрывающейся скобке, возвращаем его назад в строку
                            break;
                        default:
                            return leftBranch; // if expr -> term
                    }

                    break;
                default:
                    return null; // ERROR handlig
            }
            return leftBranch; // error too

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

                    switch (op.type) // look ahead
                    {
                        case TokenType.MINUS:
                            // посмотрю сколько минусов впереди
                            if (parser.StepNext(2).type == TokenType.MINUS)
                            {
                                Token t_past = parser.StepBack();
                                rightBranch = expr_minus(t_past, null);
                            }
                            else
                                return new BinaryOperation(op.type, leftBranch, Term_mul(parser.getNextToken()));
                            return rightBranch;
                            //BinaryOperation bo = new BinaryOperation(op.type, leftBranch, rightBranch); // expr -> term + expr| term - expr
                            //return new BinaryOperation(op.type, bo, Expr_minus(token));
                        case TokenType.RPAR:
                            parser.StepBack();
                            // Если токен равен закрывающейся скобке, возвращаем его назад в строку
                            break;
                        default:
                            parser.StepBack();
                            return leftBranch; // if expr -> term
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
                    leftB = Fact(token);
                    op = parser.getNextToken();
                    switch (op.type)
                    {
                        case TokenType.MUL:
                        case TokenType.DIV:
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

        public ExprNode Fact(Token token)
        {
            Token tmp_token;

            switch (token.type)
            {
                case TokenType.NUM:
                    return new NumberNode(token.type, token.name, token.val); // можно реализовать так, чтобы конструктор 
                                                                              // принимал сам ТОКЕН и по нему создавал вершину 
                case TokenType.VAR:
                    if (parser.getNextToken().type == TokenType.LPAR)
                    {
                        FunctionNode fn = (FunctionNode)parseFunct(token);
                        fn.funcIdentification(parser.tmp_compile);
                        return fn;
                    }
                    else
                    {
                        parser.StepBack();
                        VarNode var = new VarNode(token.type, token.name);
                        // should be handling exceptions
                        parser.symboltable.Add(var);
                        return var;
                    }
                case TokenType.LPAR:
                    tmp_token = parser.getNextToken();
                    ExprNode res = Expr_plus(tmp_token);
                    tmp_token = parser.getNextToken();   // Считываем закрывающуюся скобку
                    if(tmp_token.type != TokenType.RPAR)
                        Console.WriteLine(") exected here");
                    return res;
            }
            return null; // error handling
        }

        public ExprNode parseFunct(Token token)
        {
            List<ExprNode> args = parseFunctionArgs();
            return new FunctionNode(token.name, args: args);
        }

        public List<ExprNode> parseFunctionArgs()
        { // в аргументах тоже не должно быть void-ов
            Token tok = parser.getNextToken();
            List<ExprNode> args = new List<ExprNode>();
            while (tok.type == TokenType.NUM || tok.type == TokenType.VAR)
            {
                ExprNode curNode = Fact(tok);
                if (curNode is FunctionNode o && o.return_type == TokenType.VOID)
                {
                    // exception handling 
                    Console.WriteLine("Here can't be argument with void type");
                    return null; 
                }
                args.Add(curNode);
                parser.getNextToken();
                if (parser.curToken.type == TokenType.COMMA)
                    parser.getNextToken();
                tok = parser.curToken;
                // else return ERROR;
            }
            return args;
        }

        public ExprNode expr_minus(Token token, ExprNode op)
        {
            BinaryOperation bo;
            ExprNode en = null;
            if (parser.getNextToken().type == TokenType.MINUS && parser.StepNext(2).type == TokenType.MINUS)
            {
                if (op == null)
                    bo = new BinaryOperation(TokenType.MINUS, Fact(token), Fact(parser.getNextToken()));
                else
                    bo = new BinaryOperation(TokenType.MINUS, op, Fact(parser.getNextToken()));
                en = expr_minus(parser.curToken, bo);
            }
            else if (parser.curToken.type == TokenType.MINUS)
                if (op != null)
                    en = new BinaryOperation(TokenType.MINUS, op, Fact(parser.getNextToken()));
                else
                    en = new BinaryOperation(TokenType.MINUS, Fact(token), Fact(parser.getNextToken()));
            return en;
        }

    }
}
