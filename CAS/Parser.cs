using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CAS
{
    class Parser
    {
        public Parser(Tokenizer tokenizer)
        {
            this.tokenizer = tokenizer;
        }

        public Expression Parse()
        {
            Expression e = GetExpression();
            return e;
        }

        Expression GetExpression()
        {
            Expression ret = GetTerm();
            while (tokenizer.NextToken.String == "+" || tokenizer.NextToken.String == "-")
            {
                Tokenizer.Token token = tokenizer.Consume();
                Expression.Type type = Expression.Type.Constant;

                if (token.String == "+")
                {
                    type = Expression.Type.Plus;
                }
                else if (token.String == "-")
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
                Expression.Type type = Expression.Type.Constant;

                if (token.String == "*")
                {
                    type = Expression.Type.Times;
                }
                else if (token.String == "/")
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

            if (tokenizer.NextToken.String == "(")
            {
                tokenizer.Consume();
                Expression e = GetExpression();
                tokenizer.Consume();
                return e;
            }

            return null;
        }

        Tokenizer tokenizer;
    }
}
