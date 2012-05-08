using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CAS
{
    class Parser
    {
        public class ParseException : System.ApplicationException
        {
            public ParseException(int position, string message)
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

        public Parser(Tokenizer tokenizer)
        {
            this.tokenizer = tokenizer;
        }

        public Expression Parse()
        {
            try
            {
                Expression e = GetExpression();
                if (tokenizer.Consume(Tokenizer.Token.Type.End, "<end>") == null)
                {
                    Tokenizer.Token next = tokenizer.NextToken;
                    throw new ParseException(next.Position, "Expected <end>, got " + next.String);
                }
                return e;
            }
            catch (Tokenizer.TokenException ex)
            {
                throw new ParseException(ex.Position, ex.Message);
            }
        }

        Expression GetExpression()
        {
            Expression ret = GetTerm();
            while (tokenizer.NextToken.String == "+" || tokenizer.NextToken.String == "-")
            {
                Tokenizer.Token token = tokenizer.Consume();
                Expression.Type type;

                if (token.String == "+")
                {
                    type = Expression.Type.Plus;
                }
                else
                {
                    type = Expression.Type.Minus;
                }

                Expression e = GetTerm();
                List<Expression> children = new List<Expression>();
                children.Add(ret);
                children.Add(e);
                ret = new Expression(type, children);
            }

            return ret;
        }

        Expression GetTerm()
        {
            Expression ret = GetFactor();
            while (tokenizer.NextToken.String == "*" || tokenizer.NextToken.String == "/")
            {
                Tokenizer.Token token = tokenizer.Consume();
                Expression.Type type;

                if (token.String == "*")
                {
                    type = Expression.Type.Times;
                }
                else
                {
                    type = Expression.Type.Divide;
                }

                Expression e = GetFactor();
                List<Expression> children = new List<Expression>();
                children.Add(ret);
                children.Add(e);
                ret = new Expression(type, children);
            }

            return ret;
        }

        Expression GetFactor()
        {
            if(tokenizer.NextToken.TokenType == Tokenizer.Token.Type.Number)
            {
                Tokenizer.Token token = tokenizer.Consume();
                return new Expression(Expression.Type.Constant, null, Int32.Parse(token.String));
            }

            if (tokenizer.Consume(Tokenizer.Token.Type.Symbol, "(") != null)
            {
                Expression e = GetExpression();
                if(tokenizer.Consume(Tokenizer.Token.Type.Symbol, ")") == null)
                {
                    Tokenizer.Token next = tokenizer.NextToken;
                    throw new ParseException(next.Position, "Expected ), got " + next.String);
                }
                return e;
            }

            return null;
        }

        Tokenizer tokenizer;
    }
}
