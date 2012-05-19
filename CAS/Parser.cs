﻿using System;
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
                ret = new Expression(type, ret, e);
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
                ret = new Expression(type, ret, e);
            }

            return ret;
        }

        Expression GetFactor()
        {
            Expression ret = GetAtom();
            if (tokenizer.NextToken.TokenType == Tokenizer.Token.Type.Symbol && tokenizer.NextToken.String == "^")
            {
                tokenizer.Consume();
                Expression exponent = GetFactor();
                ret = new Expression(Expression.Type.Power, ret, exponent);
            }

            return ret;
        }

        Expression GetAtom()
        {
            Tokenizer.Token token = tokenizer.Consume();

            switch (token.TokenType)
            {
                case Tokenizer.Token.Type.Number:
                    return new Expression(Expression.Type.Constant, Int32.Parse(token.String));

                case Tokenizer.Token.Type.Identifier:
                    {
                        if (tokenizer.NextToken.String == "(")
                        {
                            tokenizer.Consume();
                            List<Expression> args = new List<Expression>();
                            while (tokenizer.NextToken.String != ")")
                            {
                                Expression arg = GetExpression();
                                args.Add(arg);

                                if (tokenizer.NextToken.String == ",")
                                {
                                    tokenizer.Consume();
                                }
                                else if (tokenizer.NextToken.String != ")")
                                {
                                    throw new ParseException(tokenizer.NextToken.Position, "Expected , or ), got " + tokenizer.NextToken.String);
                                }
                            }
                            tokenizer.Consume();

                            return new Expression(Expression.Type.Function, token.String, args.ToArray());
                        }
                        else
                        {
                            return new Expression(Expression.Type.Variable, token.String);
                        }
                    }
                case Tokenizer.Token.Type.Symbol:
                    if (token.String == "-")
                    {
                        Expression e = GetFactor();
                        return new Expression(Expression.Type.Negative, e);
                    }

                    if (token.String == "(")
                    {
                        Expression e = GetExpression();
                        token = tokenizer.Consume();
                        if (token.String != ")")
                        {
                            throw new ParseException(token.Position, "Expected ), got " + token.String);
                        }
                        return e;
                    }
                    break;
            }

            throw new ParseException(token.Position, "Unexpected token " + token.String);
        }

        Tokenizer tokenizer;
    }
}
