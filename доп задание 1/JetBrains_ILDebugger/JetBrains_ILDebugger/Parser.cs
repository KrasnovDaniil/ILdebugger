using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetBrains_ILDebugger
{
    class Parser
    {
        private List<Token> tokens; // список токенов от лексера
        public Token curToken { get; private set; } 
        int _i = 0; // указатель на текущий токен в списке
        public SymbolTable symboltable { get; private set; } // таблица символов (не используется здесь)
        public CallCompile _compile; // класс компиляторора
        ArithmeticRules ar; // отдельный класс для парсинга арифметических выражений


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
            return ar.ParseArithmeticExpression(); // основной метод парсинга
        }
        
   
        // >> FUNCTION PARSING
        public ExprNode parseFunct(Token token)
        {
            List<ExprNode> args = parseFunctionArgs();
            return new FunctionNode(token.name, args: args);
        }

        public List<ExprNode> parseFunctionArgs()
        { // в аргументах тоже не должно быть void-ов
            Token tok = getNextToken();
            List<ExprNode> args = new List<ExprNode>();
            while (tok.type == TokenType.NUM || tok.type == TokenType.VAR)
            {
                ExprNode curNode = ar.Fact(tok);
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
                // else return ERROR;
            }
            return args;
        }
        // << FUNCTION PARSING


        public Token getNextToken()
        {
            if (curToken.type == TokenType.EOF) return curToken;
            _i++;
            return curToken = StepNext(0);
        }

        public Token StepNext(int step)
        {
            if (ConfirmNextStep(step))
                return tokens[_i + step];
            return null;
        }

        public Token StepBack()
        {
            if (_i > 0)
                return curToken = tokens[--_i];
            return null;
        }

        public bool ConfirmNextStep(int step) => _i + step < tokens.Count;

    }
}
