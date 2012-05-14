using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CAS
{
    class Evaluate
    {
        public delegate void LogExpressionDelegate(Expression expression, string title);

        public static Expression Eval(Expression ex, LogExpressionDelegate log)
        {
            ex = removeMinus(ex);
            log(ex, "RemoveMinus");

            ex = flatten(ex);
            log(ex, "Flatten");

            ex = commonDenominator(ex);
            log(ex, "Common Denominator");

            ex = expand(ex);
            log(ex, "Expand");

            ex = fold(ex);
            log(ex, "Fold");

            return ex;
        }

        static Expression removeMinus(Expression expression)
        {
            Expression ret = new Expression(expression.ExpressionType, expression.Data);
            foreach (Expression child in expression.Children)
            {
                ret.Children.Add(removeMinus(child));
            }

            Expression minusOne = new Expression(Expression.Type.Constant, -1);
            switch (ret.ExpressionType)
            {
                case Expression.Type.Minus:
                    {
                        Expression times = new Expression(Expression.Type.Times, minusOne, ret.Children[1]);
                        ret = new Expression(Expression.Type.Plus, ret.Children[0], times);
                        break;
                    }

                case Expression.Type.Negative:
                    {
                        ret = new Expression(Expression.Type.Times, minusOne, ret.Children[0]);
                        break;
                    }
            }

            return ret;
        }

        static Expression flatten(Expression expression)
        {
            Expression ret = new Expression(expression.ExpressionType, expression.Data);
            foreach (Expression child in expression.Children)
            {
                ret.Children.Add(flatten(child));
            }

            switch (ret.ExpressionType)
            {
                case Expression.Type.Plus:
                case Expression.Type.Times:
                    ret = flattenNode(ret);
                    break;
            }

            return ret;
        }

        static Expression commonDenominator(Expression expression)
        {
            Expression ret = new Expression(expression.ExpressionType, expression.Data);
            foreach (Expression child in expression.Children)
            {
                ret.Children.Add(commonDenominator(child));
            }

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
                        Expression num = multiply(numerator(ret.Children[0]), denominator(ret.Children[1]));
                        Expression den = multiply(denominator(ret.Children[0]), numerator(ret.Children[1]));

                        ret = divide(num, den);
                        break;
                    }
            }

            return ret;
        }

        static Expression fold(Expression expression)
        {
            Expression ret = new Expression(expression.ExpressionType, expression.Data);
            foreach(Expression child in expression.Children)
            {
                ret.Children.Add(fold(child));
            }

            switch (ret.ExpressionType)
            {
                case Expression.Type.Plus:
                    {
                        Expression newRet = null;
                        int result = 0;
                        foreach (Expression child in ret.Children)
                        {
                            if (child.ExpressionType == Expression.Type.Constant)
                            {
                                result += (int)child.Data;
                            }
                            else
                            {
                                newRet = add(newRet, child);
                            }
                        }
                        if (result != 0 || newRet == null)
                        {
                            newRet = add(newRet, new Expression(Expression.Type.Constant, result));
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
                            if (child.ExpressionType == Expression.Type.Constant)
                            {
                                result *= (int)child.Data;
                            }
                            else
                            {
                                newRet = multiply(newRet, child);
                            }
                        }

                        if (result == 0)
                        {
                            ret = new Expression(Expression.Type.Constant, result);
                        }
                        else
                        {
                            if (result != 1 || newRet == null)
                            {
                                newRet = multiply(new Expression(Expression.Type.Constant, result), newRet);
                            }

                            ret = newRet;
                        }
                        break;
                    }

                case Expression.Type.Divide:
                    {
                        if (ret.Children[0].ExpressionType == Expression.Type.Constant && ret.Children[1].ExpressionType == Expression.Type.Constant)
                        {
                            int num = (int)ret.Children[0].Data;
                            int den = (int)ret.Children[1].Data;
                            int gcd = greatestCommonDivisor(num, den);
                            num /= gcd;
                            den /= gcd;
                            ret = new Expression(ret.ExpressionType, new Expression(Expression.Type.Constant, num), new Expression(Expression.Type.Constant, den));
                        }

                        if (ret.Children[0].ExpressionType == Expression.Type.Constant && (int)ret.Children[0].Data == 0)
                        {
                            ret = ret.Children[0];
                        }
                        else if (ret.Children[1].ExpressionType == Expression.Type.Constant && (int)ret.Children[1].Data == 1)
                        {
                            ret = ret.Children[0];
                        }

                        break;
                    }
            }

            return ret;
        }

        static Expression expand(Expression expression)
        {
            Expression ret = new Expression(expression.ExpressionType, expression.Data);
            foreach (Expression child in expression.Children)
            {
                ret.Children.Add(expand(child));
            }

            switch (ret.ExpressionType)
            {
                case Expression.Type.Plus:
                    ret = flattenNode(ret);
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
            Expression plus = new Expression(Expression.Type.Plus);
            foreach (Expression child in expressions)
            {
                if (child != null)
                {
                    plus.Children.Add(child);
                }
            }

            return flattenNode(plus);
        }

        static Expression multiply(params Expression[] expressions)
        {
            Expression plus = new Expression(Expression.Type.Times);
            foreach (Expression child in expressions)
            {
                if (child != null)
                {
                    plus.Children.Add(child);
                }
            }

            return flattenNode(plus);
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

        static List<Expression> terms(Expression expression)
        {
            if (expression.ExpressionType == Expression.Type.Plus)
            {
                return expression.Children;
            }
            else
            {
                List<Expression> list = new List<Expression>();
                list.Add(expression);
                return list;
            }
        }

        static Expression flattenNode(Expression expression)
        {
            Expression ret = new Expression(expression.ExpressionType);
            foreach (Expression child in expression.Children)
            {
                if (child.ExpressionType == ret.ExpressionType)
                {
                    ret.Children.AddRange(child.Children);
                }
                else
                {
                    ret.Children.Add(child);
                }
            }

            if (ret.Children.Count == 0)
            {
                ret = null;
            }
            else if (ret.Children.Count == 1)
            {
                ret = ret.Children[0];
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
