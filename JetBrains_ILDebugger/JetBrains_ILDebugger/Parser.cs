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
        public CallCompile tmp_compile;
        ArithmeticRules ar;

        enum PTypes
        {
            VAR, NUM, 
            MATH_OPERATION,
            FUNCTION
        }

        public Parser(List<Token> tokens, CallCompile tmp_compile)
        {
            this.tokens = tokens;
            tokens.Add(new Token("EOF", TokenType.EOF));
            curToken = tokens[0];
            symboltable = new SymbolTable();
            this.tmp_compile = tmp_compile;
            ar = new ArithmeticRules(this);
        }
        
        public ExprNode MainProcess()
        {
            return ar.ParseArithmeticExpression();
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
            switch (curToken.type)
            {
                case TokenType.IF: return ParseIfStmt();
                case TokenType.DECLARE_VAR: return ParseDeclaringVars();
            }
            return null;
        }

        public ExprNode ParseIfStmt()
        {
            ExprNode _cond, _body, _else = null;
            if (StepNext(1).type == TokenType.LPAR)
            {
                _cond = ar.ParseArithmeticExpression();
                _body = ParseBlock();
                if (StepNext(1).type == TokenType.ELSE)
                    _else = ParseBlock();
                return new IfNode(_cond, _body, _else);
            }
            return null;
        }
        // таблица симолов не будет ничего возвращять, в конструкторе класса VarNode() будет проверка и придание значения 
        public void ParseDeclaringVars()
        {
            if (getNextToken().type == TokenType.VAR) symboltable.Add(curToken.name);
        }

        public ExprNode ParseExpr()
        {
            return null;
        }

        public ExprNode ParseBlock()
        {
            ExprNode expr;
            if (getNextToken().type == TokenType.LBRA)
            {
                expr = ParseExpr();
                if (curToken.type == TokenType.RBRA)
                    return expr;
            }
            return null; // exception
        }

        /*
         Задачи на 04.04.2020 сб
         * добавить в грамматику логические операции
         
         */
        






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
