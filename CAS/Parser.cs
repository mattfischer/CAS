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

        public Node Parse()
        {
            try
            {
                Node e = GetExpression();
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

        Node GetExpression()
        {
            Node ret = GetTerm();
            while (tokenizer.NextToken.String == "+" || tokenizer.NextToken.String == "-")
            {
                Tokenizer.Token token = tokenizer.Consume();
                Node.Type type;

                if (token.String == "+")
                {
                    type = Node.Type.Plus;
                }
                else
                {
                    type = Node.Type.Minus;
                }

                Node e = GetTerm();
                ret = new Node(type, ret, e);
            }

            return ret;
        }

        Node GetTerm()
        {
            Node ret = GetFactor();
            while (tokenizer.NextToken.String == "*" || tokenizer.NextToken.String == "/")
            {
                Tokenizer.Token token = tokenizer.Consume();
                Node.Type type;

                if (token.String == "*")
                {
                    type = Node.Type.Times;
                }
                else
                {
                    type = Node.Type.Divide;
                }

                Node e = GetFactor();
                ret = new Node(type, ret, e);
            }

            return ret;
        }

        Node GetFactor()
        {
            Node ret = GetAtom();
            if (tokenizer.NextToken.TokenType == Tokenizer.Token.Type.Symbol && tokenizer.NextToken.String == "^")
            {
                tokenizer.Consume();
                Node exponent = GetFactor();
                ret = new Node(Node.Type.Power, ret, exponent);
            }

            return ret;
        }

        Node GetAtom()
        {
            Tokenizer.Token token = tokenizer.Consume();

            switch (token.TokenType)
            {
                case Tokenizer.Token.Type.Number:
                    return new Node(Node.Type.Constant, Int32.Parse(token.String));

                case Tokenizer.Token.Type.Identifier:
                    {
                        if (tokenizer.NextToken.String == "(")
                        {
                            tokenizer.Consume();
                            List<Node> args = new List<Node>();
                            while (tokenizer.NextToken.String != ")")
                            {
                                Node arg = GetExpression();
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

                            return new Node(Node.Type.Function, token.String, args.ToArray());
                        }
                        else
                        {
                            return new Node(Node.Type.Variable, token.String);
                        }
                    }
                case Tokenizer.Token.Type.Symbol:
                    if (token.String == "-")
                    {
                        Node e = GetFactor();
                        return new Node(Node.Type.Negative, e);
                    }

                    if (token.String == "(")
                    {
                        Node e = GetExpression();
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
