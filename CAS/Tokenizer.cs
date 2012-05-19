using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CAS
{
    class Tokenizer
    {
        public class TokenException : System.ApplicationException
        {
            public TokenException(int position, string message)
                : base(message)
            {
                this.position = position;
            }

            public int Position
            {
                get { return position; }
            }

            int position;
        };

        public class Token
        {
            public enum Type
            {
                Number,
                Identifier,
                Symbol,
                End
            };

            Type type;
            String str;
            int pos;

            public Type TokenType
            {
                get { return type; }
            }

            public String String
            {
                get { return str; }
            }

            public int Position
            {
                get { return pos; }
            }

            public Token(Type type, string str, int pos)
            {
                this.type = type;
                this.str = str;
                this.pos = pos;
            }
        };

        public Tokenizer(string input)
        {
            this.input = input;
            this.nextToken = GetNext();
        }

        Token GetNext()
        {
            while (position < input.Length && input[position] == ' ')
            {
                position++;
            }

            if (position == input.Length)
            {
                return new Token(Token.Type.End, "<end>", position);
            }

            string rest = input.Substring(position);

            if (Char.IsDigit(rest[0]))
            {
                int i;
                for (i = 0; i < rest.Length && Char.IsDigit(rest[i]); i++) {}

                string str = rest.Substring(0, i);
                Token ret = new Token(Token.Type.Number, str, position);
                position += i;
                return ret;
            }

            if(Char.IsLetter(rest[0]))
            {
                int i;
                for (i = 0; i < rest.Length && Char.IsLetterOrDigit(rest[i]); i++) {}

                string str = rest.Substring(0, i);
                Token ret = new Token(Token.Type.Identifier, str, position);
                position += i;
                return ret;
            }

            string[] symbols = { "+", "-", "*", "/", "(", ")", "^", "," };
            foreach (string symbol in symbols)
            {
                if (rest.StartsWith(symbol))
                {
                    Token ret = new Token(Token.Type.Symbol, symbol, position);
                    position += symbol.Length;
                    return ret;
                }
            }

            throw new TokenException(position, "Found illegal character " + rest[0]);
        }

        public Token Consume(Token.Type type, string str)
        {
            Token token = nextToken;

            if (token.TokenType == type && token.String == str)
            {
                nextToken = GetNext();
                return token;
            }
            else
            {
                return null;
            }
        }

        public Token Consume()
        {
            Token token = nextToken;
            nextToken = GetNext();
            return token;
        }

        public Token NextToken
        {
            get { return nextToken; }
        }

        string input;
        int position = 0;
        Token nextToken;
    }
}
