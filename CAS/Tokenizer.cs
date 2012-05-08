using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CAS
{
    class Tokenizer
    {
        public struct Token
        {
            public enum Type
            {
                Number,
                Symbol,
                End
            };

            Type type;
            String str;

            public Type TokenType
            {
                get { return type; }
            }

            public String String
            {
                get { return str; }
            }

            public Token(Type type, string str)
            {
                this.type = type;
                this.str = str;
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
                return new Token(Token.Type.End, "");
            }

            string rest = input.Substring(position);

            if (rest[0] >= '0' && rest[0] <= '9')
            {
                int i;
                for (i = 0; i < rest.Length; i++)
                {
                    if (rest[i] < '0' || rest[i] > '9')
                    {
                        break;
                    }
                }
                string str = rest.Substring(0, i);
                position += i;
                return new Token(Token.Type.Number, str);
            }

            string[] symbols = { "+", "-", "*", "/", "(", ")" };
            foreach (string symbol in symbols)
            {
                if (rest.StartsWith(symbol))
                {
                    position += symbol.Length;
                    return new Token(Token.Type.Symbol, symbol);
                }
            }

            return new Token(Token.Type.End, "");
        }

        public Token Consume()
        {
            Token ret = nextToken;
            nextToken = GetNext();

            return ret;
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
