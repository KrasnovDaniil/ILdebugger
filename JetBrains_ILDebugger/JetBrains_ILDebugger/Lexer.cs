using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetBrains_ILDebugger
{
    class Lexer
    {
        private char curCh;
        private int _i = 0;
        private string textOfProgram;
        private TokenType types;
        private List<Token> tokens;
        string arOps = "-*+/=()";

        public Lexer(string textOfProgram)
        {
            this.textOfProgram = textOfProgram + '\0';
            tokens = new List<Token>();
            types = new TokenType();

        }

        public char getNextChar()
        {
            return curCh = textOfProgram[_i++];
        }

        public List<Token> Tokenization()
        {
            StringBuilder word = new StringBuilder();
            getNextChar();
            while (inRange())
            {
                if (curCh == ' ' || curCh == '\r' || curCh == '\t' || curCh == '\n') { getNextChar(); continue; }
                else if (Char.IsNumber(curCh)) tokenizeNumber();
                else if (Char.IsLetter(curCh)) tokenizeWord();
                else if (curCh == '(') { tokens.Add(new Token("(", TokenType.LPAR)); getNextChar(); }
                else if (curCh == ')') { tokens.Add(new Token(")", TokenType.RPAR)); getNextChar(); }
                else if (curCh == '{') { tokens.Add(new Token("{", TokenType.LBRA)); getNextChar(); }
                else if (curCh == '}') { tokens.Add(new Token("}", TokenType.RBRA)); getNextChar(); }
                else if (curCh == ',') { tokens.Add(new Token(",", TokenType.COMMA)); getNextChar(); }
                else if (curCh == ';') { tokens.Add(new Token(";", TokenType.SEMICOLON)); getNextChar(); }
                else
                    try
                    {
                        if (arOps.Contains(curCh)) tokenizeOperation();
                    }
                    catch
                    {
                        Console.WriteLine("Unexpected symbol " + curCh);
                        return null;
                    }
            }
            return tokens;
        }

        public void tokenizeNumber()
        {
            StringBuilder word = new StringBuilder();
            Token thisToken = null;
            while (Char.IsNumber(curCh) && inRange())
            {
                word.Append(curCh);
                getNextChar();
            }
            thisToken = new Token(word.ToString(), TokenType.NUM);
            tokens.Add(thisToken);
        }

        public void tokenizeOperation()
        {
            Token thisToken = null;
            switch (curCh)
            {
                case '+':
                    thisToken = new Token("+", TokenType.PLUS);
                    break;
                case '-':
                    thisToken = new Token("-", TokenType.MINUS);
                    break;
                case '*':
                    thisToken = new Token("*", TokenType.MUL);
                    break;
                case '/':
                    thisToken = new Token("/", TokenType.DIV);
                    break;
                case '=':
                    thisToken = new Token("=", TokenType.ASSIGN);
                    break;
                case '(':
                    thisToken = new Token("(", TokenType.LPAR);
                    break;
                case ')':
                    thisToken = new Token(")", TokenType.RPAR);
                    break;
                default:
                    Console.WriteLine("error in operations");
                    break; // here should be handling errors
            }
            getNextChar();
            tokens.Add(thisToken);

        }

        public void tokenizeWord()
        {
            StringBuilder word = new StringBuilder();
            Token thisToken = null;
            while (Char.IsLetter(curCh) && inRange())
            {
                word.Append(curCh);
                getNextChar();
            }
            string wordS = word.ToString();
            if (wordS == "if") thisToken = new Token(wordS, TokenType.IF);
            else if (wordS == "else") thisToken = new Token(wordS, TokenType.ELSE);
            else if (wordS == "return") thisToken = new Token(wordS, TokenType.RETURN);
            else if (wordS == "var") thisToken = new Token(wordS, TokenType.DECLARE_VAR);
            else if (wordS == "break") thisToken = new Token(wordS, TokenType.BREAK);
            else thisToken = new Token(wordS, TokenType.VAR);
            tokens.Add(thisToken);
        }


        public bool inRange() => textOfProgram[_i - 1] != '\0';

    }

    class Token
    {
        public string name { get; private set; }
        public TokenType type { get; private set; }
        public int val { get; private set; }

        public Token(string name, TokenType type)
        {
            this.name = name;
            this.type = type;
            if (type == TokenType.NUM) ComputeValue();
        }

        public int ComputeValue() => val = Convert.ToInt32(name);

        public bool isArithmOp()
        {
            switch (type)
            {
                case TokenType.PLUS: return true;
                case TokenType.MUL: return true;
                case TokenType.DIV: return true;
                case TokenType.MINUS: return true;
            }
            return false;
        }

    }

    enum TokenType
    {
        NUM, VAR, FUNCTION,
        LPAR, RPAR,
        PLUS, MINUS, MUL, DIV,
        ASSIGN,

        DECLARE_VAR,
  
        SEMICOLON, COMMA,

        LONG, VOID,

        IF, ELSE,

        RETURN, BREAK, 
        LBRA, RBRA,

        EOF
    }
}
