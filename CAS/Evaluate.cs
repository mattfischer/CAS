using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CAS
{
    class Evaluate
    {
        public delegate void LogExpressionDelegate(Expression oldExp, Expression newExp, string title);
        delegate Expression ExpressionOperationDelegate(Expression expression);

        static LogExpressionDelegate log = null;
        public static Expression Eval(Expression ex, LogExpressionDelegate lg)
        {
            log = lg;

            ex = recurse(ex, removeMinus, "RemoveMinus");
            ex = recurse(ex, flatten, "Flatten");
            ex = recurse(ex, rationalize, "Rationalize");
            ex = recurse(ex, expand, "Expand");
            ex = recurse(ex, fold, "Fold");
            ex = recurse(ex, collect, "Collect");

            return ex;
        }

        static Expression removeMinus(Expression expression)
        {
            Expression ret = expression;

            switch (ret.ExpressionType)
            {
                case Expression.Type.Minus:
                    ret = add(ret.Children[0], multiply(constant(-1), ret.Children[1]));
                    break;

                case Expression.Type.Negative:
                    ret = multiply(constant(-1), ret.Children[0]);
                    break;
            }

            return ret;
        }

        static Expression flatten(Expression expression)
        {
            Expression ret = expression;

            switch (ret.ExpressionType)
            {
                case Expression.Type.Plus:
                case Expression.Type.Times:
                    {
                        List<Expression> children = new List<Expression>();
                        if (ret.Children != null)
                        {
                            foreach (Expression child in ret.Children)
                            {
                                if (child.ExpressionType == ret.ExpressionType)
                                {
                                    children.AddRange(child.Children);
                                }
                                else
                                {
                                    children.Add(child);
                                }
                            }
                        }

                        if (children.Count == 1)
                        {
                            ret = children[0];
                        }
                        else
                        {
                            ret = new Expression(ret.ExpressionType, children.ToArray());
                        }
                        break;
                    }
            }

            return ret;
        }

        static Expression rationalize(Expression expression)
        {
            Expression ret = expression;

            switch (ret.ExpressionType)
            {
                case Expression.Type.Plus:
                    {
                        Expression num = constant(0);
                        Expression den = constant(1);
                        foreach (Expression term in terms(ret))
                        {
                            if (denominator(term) == den)
                            {
                                num = add(num, numerator(term));
                            }
                            else
                            {
                                num = add(multiply(denominator(term), num), multiply(den, numerator(term)));
                                den = multiply(den, denominator(term));
                            }
                        }

                        ret = divide(num, den);
                        break;
                    }

                case Expression.Type.Times:
                    {
                        Expression num = constant(1);
                        Expression den = constant(1);

                        foreach (Expression factor in factors(ret))
                        {
                            num = multiply(num, numerator(factor));
                            den = multiply(den, denominator(factor));
                        }

                        ret = divide(num, den);
                        break;
                    }

                case Expression.Type.Divide:
                    {
                        Expression num = multiply(numerator(numerator(ret)), denominator(denominator(ret)));
                        Expression den = multiply(denominator(numerator(ret)), numerator(denominator(ret)));

                        ret = divide(num, den);
                        break;
                    }
            }

            return ret;
        }

        static Expression fold(Expression expression)
        {
            Expression ret = expression;

            switch (ret.ExpressionType)
            {
                case Expression.Type.Plus:
                    {
                        Expression rest = constant(0);
                        int result = 0;
                        foreach (Expression term in terms(ret))
                        {
                            if (isConstant(term))
                            {
                                result += constantValue(term);
                            }
                            else
                            {
                                rest = add(rest, term);
                            }
                        }

                        ret = add(rest, constant(result));
                        break;
                    }

                case Expression.Type.Times:
                    {
                        Expression rest = constant(1);
                        int result = 1;
                        foreach (Expression factor in factors(ret))
                        {
                            if (isConstant(factor))
                            {
                                result *= constantValue(factor);
                            }
                            else
                            {
                                rest = multiply(rest, factor);
                            }
                        }

                        ret = multiply(constant(result), rest);
                        break;
                    }

                case Expression.Type.Divide:
                    if (isConstant(numerator(ret)) && isConstant(denominator(ret)))
                    {
                        int num = constantValue(numerator(ret));
                        int den = constantValue(denominator(ret));
                        int gcd = greatestCommonDivisor(num, den);
                        num /= gcd;
                        den /= gcd;

                        ret = divide(constant(num), constant(den));
                    }
                    break;
            }

            return ret;
        }

        static Expression expand(Expression expression)
        {
            Expression ret = expression;

            switch (ret.ExpressionType)
            {
                case Expression.Type.Plus:
                    ret = flatten(ret);
                    break;

                case Expression.Type.Times:
                    {
                        Expression expansion = constant(1);
                        foreach (Expression factor in factors(ret))
                        {
                            Expression newExpansion = constant(0);
                            foreach (Expression factorTerm in terms(factor))
                            {
                                foreach (Expression term in terms(expansion))
                                {
                                    newExpansion = add(newExpansion, multiply(term, factorTerm));
                                }
                            }
                            expansion = newExpansion;
                        }
                        ret = expansion;
                        break;
                    }
            }

            return ret;
        }

        static Expression collect(Expression expression)
        {
            Expression ret = expression;

            switch (ret.ExpressionType)
            {
                case Expression.Type.Plus:
                    {
                        Dictionary<Expression, Expression> dict = new Dictionary<Expression, Expression>();

                        foreach (Expression child in terms(expression))
                        {
                            Expression[] coeffTerm = coefficientTerm(child);
                            Expression coefficient = coeffTerm[0];
                            Expression term = coeffTerm[1];

                            term = recurse(term, sort, "Sort");
                            if (dict.ContainsKey(term))
                            {
                                dict[term] = add(dict[term], coefficient);
                            }
                            else
                            {
                                dict.Add(term, coefficient);
                            }
                        }

                        ret = constant(0);
                        foreach (Expression term in dict.Keys)
                        {
                            ret = add(ret, multiply(fold(dict[term]), term));
                        }
                        break;
                    }

                case Expression.Type.Times:
                    {
                        Dictionary<Expression, Expression> dict = new Dictionary<Expression, Expression>();

                        foreach (Expression child in factors(expression))
                        {
                            Expression fact = factor(child);
                            Expression exp = exponent(child);

                            fact = recurse(fact, sort, "Sort");
                            if (dict.ContainsKey(fact))
                            {
                                dict[fact] = add(dict[fact], exp);
                            }
                            else
                            {
                                dict.Add(fact, exp);
                            }
                        }

                        ret = constant(1);
                        foreach (Expression factor in dict.Keys)
                        {
                            ret = multiply(ret, power(factor, fold(dict[factor])));
                        }
                        break;
                    }
            }

            return ret;
        }

        static Expression add(params Expression[] expressions)
        {
            List<Expression> children = new List<Expression>();
            foreach (Expression child in expressions)
            {
                if (child != constant(0))
                {
                    children.Add(child);
                }
            }

            if (children.Count == 0)
            {
                return constant(0);
            }
            else if (children.Count == 1)
            {
                return children[0];
            }
            else
            {
                return flatten(new Expression(Expression.Type.Plus, children.ToArray()));
            }
        }

        static Expression multiply(params Expression[] expressions)
        {
            List<Expression> children = new List<Expression>();
            bool zero = false;
            foreach (Expression child in expressions)
            {
                if (child == constant(0))
                {
                    zero = true;
                    break;
                }

                if (child != constant(1))
                {
                    children.Add(child);
                }
            }

            if (zero)
            {
                return constant(0);
            }
            else if (children.Count == 0)
            {
                return constant(1);
            }
            else if (children.Count == 1)
            {
                return children[0];
            }
            else
            {
                return flatten(new Expression(Expression.Type.Times, children.ToArray()));
            }
        }

        static Expression divide(Expression num, Expression den)
        {
            if (num == constant(0))
            {
                return num;
            }
            else if (den == constant(1))
            {
                return num;
            }
            else
            {
                return new Expression(Expression.Type.Divide, num, den);
            }
        }

        static Expression power(Expression factor, Expression exponent)
        {
            if (exponent == constant(1))
            {
                return factor;
            }
            else
            {
                return new Expression(Expression.Type.Power, factor, exponent);
            }
        }

        static Expression numerator(Expression expression)
        {
            if (expression.ExpressionType == Expression.Type.Divide)
            {
                return expression.Children[0];
            }
            else
            {
                return expression;
            }
        }

        static Expression denominator(Expression expression)
        {
            if (expression.ExpressionType == Expression.Type.Divide)
            {
                return expression.Children[1];
            }
            else
            {
                return constant(1);
            }
        }

        static Expression exponent(Expression expression)
        {
            if (expression.ExpressionType == Expression.Type.Power)
            {
                return expression.Children[1];
            }
            else
            {
                return constant(1);
            }
        }

        static Expression factor(Expression expression)
        {
            if (expression.ExpressionType == Expression.Type.Power)
            {
                return expression.Children[0];
            }
            else
            {
                return expression;
            }
        }

        static Expression[] terms(Expression expression)
        {
            if (expression.ExpressionType == Expression.Type.Plus)
            {
                return expression.Children;
            }
            else
            {
                return new Expression[] { expression };
            }
        }

        static Expression[] factors(Expression expression)
        {
            if (expression.ExpressionType == Expression.Type.Times)
            {
                return expression.Children;
            }
            else
            {
                return new Expression[] { expression };
            }
        }

        static Expression constant(int value)
        {
            return new Expression(Expression.Type.Constant, value);
        }

        static bool isConstant(Expression expression)
        {
            return expression.ExpressionType == Expression.Type.Constant;
        }

        static int constantValue(Expression expression)
        {
            return (int)expression.Data;
        }

        static Expression[] coefficientTerm(Expression expression)
        {
            Expression coefficient = constant(1);
            Expression term = constant(1);

            foreach (Expression child in factors(expression))
            {
                if (isConstant(child))
                {
                    coefficient = multiply(coefficient, child);
                }
                else
                {
                    term = multiply(term, child);
                }
            }

            return new Expression[] { coefficient, term };
        }

        static Expression recurse(Expression expression, ExpressionOperationDelegate func, string logTitle)
        {
            List<Expression> children = new List<Expression>();
            if (expression.Children != null)
            {
                foreach (Expression child in expression.Children)
                {
                    Expression newChild = recurse(child, func, logTitle);
                    log(child, newChild, logTitle);
                    children.Add(newChild);
                }
            }

            Expression ret = new Expression(expression.ExpressionType, expression.Data, children.ToArray());
            ret = func(ret);
            log(expression, ret, logTitle);
            return ret;
        }

        static Expression sort(Expression expression)
        {
            Expression ret = expression;

            switch(ret.ExpressionType)
            {
                case Expression.Type.Plus:
                case Expression.Type.Times:
                    {
                        Expression[] array = new Expression[ret.Children.Length];
                        Array.Copy(ret.Children, array, ret.Children.Length);
                        Array.Sort(array);

                        ret = new Expression(ret.ExpressionType, ret.Data, array);
                        break;
                    }
            }

            return ret;
        }

        static int greatestCommonDivisor(int a, int b)
        {
            a = Math.Abs(a);
            b = Math.Abs(b);

            if (a == 0)
            {
                return b;
            }
            while (b > 0)
            {
                if (a > b)
                {
                    a = a - b;
                }
                else
                {
                    b = b - a;
                }
            }
            return a;
        }
    }
}
