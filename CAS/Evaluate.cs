using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CAS
{
    class Evaluate
    {
        public delegate void LogExpressionDelegate(Expression expression, string title);
        delegate Expression ExpressionOperationDelegate(Expression expression);

        public static Expression Eval(Expression ex, LogExpressionDelegate log)
        {
            ex = recurse(ex, removeMinus);
            log(ex, "RemoveMinus");

            ex = recurse(ex, flatten);
            log(ex, "Flatten");

            ex = recurse(ex, commonDenominator);
            log(ex, "Common Denominator");

            ex = recurse(ex, expand);
            log(ex, "Expand");

            ex = recurse(ex, fold);
            log(ex, "Fold");

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

                        if (children.Count == 0)
                        {
                            ret = null;
                        }
                        else if (children.Count == 1)
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

        static Expression commonDenominator(Expression expression)
        {
            Expression ret = expression;

            switch (ret.ExpressionType)
            {
                case Expression.Type.Plus:
                    {
                        Expression num = null;
                        Expression den = null;
                        foreach (Expression child in ret.Children)
                        {
                            if (num == null)
                            {
                                num = numerator(child);
                            }
                            else
                            {
                                num = add(multiply(denominator(child), num), multiply(den, numerator(child)));
                            }
                            den = multiply(den, denominator(child));
                        }

                        ret = divide(num, den);
                        break;
                    }

                case Expression.Type.Times:
                    {
                        Expression num = null;
                        Expression den = null;

                        foreach (Expression child in ret.Children)
                        {
                            num = multiply(num, numerator(child));
                            den = multiply(den, denominator(child));
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
                        Expression newRet = null;
                        int result = 0;
                        foreach (Expression child in ret.Children)
                        {
                            if (isConstant(child))
                            {
                                result += constantValue(child);
                            }
                            else
                            {
                                newRet = add(newRet, child);
                            }
                        }
                        if (result != 0 || newRet == null)
                        {
                            newRet = add(newRet, constant(result));
                        }

                        ret = newRet;

                        break;
                    }

                case Expression.Type.Times:
                    {
                        Expression newRet = null;
                        int result = 1;
                        foreach (Expression child in ret.Children)
                        {
                            if (isConstant(child))
                            {
                                result *= constantValue(child);
                            }
                            else
                            {
                                newRet = multiply(newRet, child);
                            }
                        }

                        if (result == 0)
                        {
                            ret = constant(0);
                        }
                        else
                        {
                            if (result != 1 || newRet == null)
                            {
                                newRet = multiply(constant(result), newRet);
                            }

                            ret = newRet;
                        }
                        break;
                    }

                case Expression.Type.Divide:
                    {
                        if (isConstant(numerator(ret)) && isConstant(denominator(ret)))
                        {
                            int num = constantValue(numerator(ret));
                            int den = constantValue(denominator(ret));
                            int gcd = greatestCommonDivisor(num, den);
                            num /= gcd;
                            den /= gcd;

                            ret = divide(constant(num), constant(den));
                        }

                        if (isConstant(numerator(ret)) && constantValue(numerator(ret)) == 0)
                        {
                            ret = constant(0);
                        }
                        else if (isConstant(denominator(ret)) && constantValue(denominator(ret)) == 1)
                        {
                            ret = numerator(ret);
                        }

                        break;
                    }
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
                        Expression plus = null;
                        foreach (Expression child in ret.Children)
                        {
                            Expression newPlus = null;
                            foreach (Expression term in terms(child))
                            {
                                if (plus == null)
                                {
                                    newPlus = add(newPlus, term);
                                }
                                else
                                {
                                    foreach (Expression oldTerm in terms(plus))
                                    {
                                        newPlus = add(newPlus, multiply(oldTerm, term));
                                    }
                                }
                            }
                            plus = newPlus;
                        }
                        ret = plus;
                        break;
                    }
            }

            return ret;
        }

        static Expression add(params Expression[] expressions)
        {
            return makeOp(Expression.Type.Plus, expressions);
        }

        static Expression multiply(params Expression[] expressions)
        {
            return makeOp(Expression.Type.Times, expressions);
        }

        static Expression makeOp(Expression.Type type, params Expression[] expressions)
        {
            List<Expression> children = new List<Expression>();
            foreach (Expression child in expressions)
            {
                if (child != null)
                {
                    children.Add(child);
                }
            }

            return flatten(new Expression(type, children.ToArray()));
        }

        static Expression divide(Expression num, Expression den)
        {
            if (den != null)
            {
                return new Expression(Expression.Type.Divide, num, den);
            }
            else
            {
                return num;
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
                return null;
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

        static Expression recurse(Expression expression, ExpressionOperationDelegate func)
        {
            List<Expression> children = new List<Expression>();
            if (expression.Children != null)
            {
                foreach (Expression child in expression.Children)
                {
                    children.Add(recurse(child, func));
                }
            }

            Expression ret = new Expression(expression.ExpressionType, expression.Data, children.ToArray());
            ret = func(ret);
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
