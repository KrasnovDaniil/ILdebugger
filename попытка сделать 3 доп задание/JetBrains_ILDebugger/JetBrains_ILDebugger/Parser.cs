using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetBrains_ILDebugger
{
    class Parser
    {
        private List<Token> tokens;
        public Token curToken { get; private set; }
        int _i = 0;
        public SymbolTable symboltable { get; private set; }
        public CallCompile _compile;
        ArithmeticRules ar;

        public Parser(List<Token> tokens, CallCompile tmp_compile)
        {
            this.tokens = tokens;
            tokens.Add(new Token("EOF", TokenType.EOF));
            curToken = tokens[0];
            symboltable = new SymbolTable();
            _compile = tmp_compile;
            ar = new ArithmeticRules(this);
        }
        
        public ExprNode MainProcess()
        {
            ParseDeclaringVars();
            return startParsing();
        }

        public List<Token> ParseNumeralExpression()
        {
            int res = 0;
            List<Token> expr = new List<Token>();
            TokenType tokentype;
            do
            {
                expr.Add(curToken);
                tokentype = getNextToken().type;
            }
            while (curToken.type == TokenType.NUM  ||
                   curToken.type == TokenType.VAR  ||
                   curToken.type == TokenType.LPAR ||
                   curToken.type == TokenType.RPAR ||
                   curToken.isArithmOp()); 
            return expr;
        }

        public ExprNode startParsing()
        {
            return new ConnectingNode(ParseExpr(curToken), ParseExpr(curToken));
        }

        public ExprNode ParseIfStmt()
        {
            ExprNode _cond, _body, _else = null;
            if (getNextToken().type == TokenType.LPAR)
            {
                getNextToken();
                _cond = ar.ParseArithmeticExpression();
                if (getNextToken().type == TokenType.RPAR) getNextToken();
                _body = ParseBlock();
                if (curToken.type == TokenType.ELSE)
                {
                    getNextToken();
                    _else = ParseBlock();
                }
                return new IfNode(_cond, _body, _else);
            }
            return null;
        }
        // >> VARIABLES
        public void ParseDeclaringVars()
        {
            if (curToken.type == TokenType.DECLARE_VAR)
                while (getNextToken().type == TokenType.VAR)
                {
                    symboltable.Add(curToken.name);
                    if (getNextToken().type == TokenType.SEMICOLON) { getNextToken(); break; }
                    if (curToken.type != TokenType.COMMA)
                    {
                        Console.WriteLine("wrong enumeration of variables");
                        return; // error
                    }
                }
        }
        // << VARTABLE

        // >> IDENTIFICATION VARS & FUNCTIONS
        public ExprNode ParseIdentification(Token tok)
        {
            if (tok.type != TokenType.VAR) return null; // error
            if (getNextToken().type == TokenType.LPAR) return parseFunct(tok);
            else if (curToken.type == TokenType.ASSIGN)
            {
                VarNode var = new VarNode(tok.name);
                getNextToken();
                ExprNode value = ar.ParseArithmeticExpression();
                var.setValue(value);
                if (curToken.type == TokenType.SEMICOLON) getNextToken();
                return var;
            }
            else return new VarNode(tok.name);
        }
        // << IDENTIFICATION VARS & FUNCTIONS
        // << VARIABLES


        // >> FUNCTION PARSING
        public ExprNode parseFunct(Token token)
        {
            List<ExprNode> args = parseFunctionArgs();
            return new FunctionNode(token.name, args: args);
        }
        /// <summary>
        /// парсинг аргументов функций
        /// </summary>
        public List<ExprNode> parseFunctionArgs()
        { 
            Token tok = getNextToken();
            List<ExprNode> args = new List<ExprNode>();
            while (tok.type == TokenType.NUM || tok.type == TokenType.VAR)
            {
                ExprNode curNode = ar.Expr_plus(tok);
                if (curNode is FunctionNode o && o.return_type == TokenType.VOID)
                {
                    // exception handling 
                    Console.WriteLine("Here can't be argument with void type");
                    return null;
                }
                args.Add(curNode);
                getNextToken();
                if (curToken.type == TokenType.COMMA)
                    getNextToken();
                tok = curToken;
            }
            return args;
        }
        // << FUNCTION PARSING

        /// <summary>
        ///  парсинг выражения 
        /// </summary>
        public ExprNode parseSmallExpr(Token tok)
        {
            ExprNode expr;
            switch (tok.type)
            {
                case TokenType.NUM: expr = ar.ParseArithmeticExpression(); break;
                case TokenType.VAR: expr = ParseIdentification(tok); break;
                case TokenType.EOF: return null; // end
                case TokenType.RBRA: getNextToken(); return null;
                default: expr = null; getNextToken(); break;
            }
            return expr;
        }


        // >> EXPR PARSING
        /// <summary>
        /// основной метод парснга - разбор 
        /// </summary>
        public ExprNode ParseExpr(Token tok)
        {
            ConnectingNode expr;
            ExprNode leftRes;

            switch (tok.type)
            {
                case TokenType.IF: leftRes = ParseIfStmt(); break;
                case TokenType.VAR: leftRes = ParseIdentification(curToken); break;
                case TokenType.NUM: leftRes = ar.ParseArithmeticExpression(); break;
                case TokenType.RETURN: leftRes = new OperationNode(TokenType.RETURN, parseSmallExpr(getNextToken())); break;
                case TokenType.BREAK: leftRes = new OperationNode(TokenType.BREAK, null); break;
                case TokenType.EOF: return null; // end
                case TokenType.RBRA: getNextToken(); return null;
                default: leftRes = null; getNextToken(); break;
            }
            if (leftRes == null) return null;
            return expr = new ConnectingNode(leftRes, ParseExpr(curToken));
        }
        // << EXPR PARSING

        // >> BLOCK PARSING
        /// <summary>
        /// разбор выражения в блоке "{ expr }", после if и else
        /// </summary>
        public ExprNode ParseBlock()
        {
            ExprNode expr;
            if (curToken.type == TokenType.LBRA)
            {
                expr = ParseExpr(getNextToken());
                if (curToken.type == TokenType.RBRA)
                {
                    getNextToken();
                    return expr;
                }
            }
            return null; // exception
        }
        // << BLOCK PARSING


        // Возвращение следующего токена и сдвиг указателя на 1 право
        public Token getNextToken()
        {
            if (curToken.type == TokenType.EOF) return curToken;
            _i++;
            return curToken = StepNext(0);
        }

        // просмотр следующего токена на step шагов от curToken
        public Token StepNext(int step)
        {
            if (ConfirmNextStep(step))
                return tokens[_i + step];
            return null;
        }

        // шаг назад на 1 от curToken
        public Token StepBack()
        {
            if (_i > 0)
                return curToken = tokens[--_i];
            return null;
        }

        // условие, чтобы указатель не выходил за пределы массива токенов
        public bool ConfirmNextStep(int step) => _i + step < tokens.Count;

    }
}
