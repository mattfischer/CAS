﻿using System;
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

            ex = flatten(ex, true);
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

        static Expression flatten(Expression expression, bool recursive)
        {
            Expression ret = expression;

            if(recursive) {
                ret = new Expression(expression.ExpressionType, expression.Data);
                foreach (Expression child in expression.Children)
                {
                    ret.Children.Add(flatten(child, recursive));
                }
            }

            switch (ret.ExpressionType)
            {
                case Expression.Type.Plus:
                case Expression.Type.Times:
                    {
                        Expression newRet = new Expression(ret.ExpressionType);
                        foreach (Expression child in ret.Children)
                        {
                            if (child.ExpressionType == ret.ExpressionType)
                            {
                                newRet.Children.AddRange(child.Children);
                            }
                            else
                            {
                                newRet.Children.Add(child);
                            }
                        }

                        if (newRet.Children.Count == 1)
                        {
                            ret = newRet.Children[0];
                        }
                        else
                        {
                            ret = newRet;
                        }

                        break;
                    }
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
                        Expression num = new Expression(Expression.Type.Plus);
                        Expression den = new Expression(Expression.Type.Times);
                        foreach (Expression child in ret.Children)
                        {
                            Expression childNum;
                            Expression childDen;

                            if (child.ExpressionType == Expression.Type.Divide)
                            {
                                childNum = child.Children[0];
                                childDen = child.Children[1];
                            }
                            else
                            {
                                childNum = child;
                                childDen = null;
                            }

                            if (childDen != null)
                            {
                                Expression newNum = new Expression(Expression.Type.Plus);
                                foreach (Expression numChild in num.Children)
                                {
                                    Expression times = new Expression(Expression.Type.Times, childDen, numChild);
                                    times = flatten(times, false);
                                    newNum.Children.Add(times);
                                }

                                num = newNum;
                            }

                            Expression newTerm;

                            if (den.Children.Count > 0)
                            {
                                newTerm = new Expression(Expression.Type.Times, den, childNum);
                                newTerm = flatten(newTerm, false);
                            }
                            else
                            {
                                newTerm = childNum;
                            }

                            num.Children.Add(newTerm);

                            if (childDen != null)
                            {
                                den.Children.Add(childDen);
                            }
                        }

                        if (den.Children.Count > 0)
                        {
                            num = flatten(num, false);
                            den = flatten(den, false);
                            ret = new Expression(Expression.Type.Divide, num, den);
                        }
                        else
                        {
                            ret = flatten(num, false);
                        }
                        break;
                    }

                case Expression.Type.Times:
                    {
                        Expression num = new Expression(Expression.Type.Times);
                        Expression den = new Expression(Expression.Type.Times);

                        foreach (Expression child in ret.Children)
                        {
                            if (child.ExpressionType == Expression.Type.Divide)
                            {
                                num.Children.Add(child.Children[0]);
                                den.Children.Add(child.Children[1]);
                            } else {
                                num.Children.Add(child);
                            }
                        }

                        if (den.Children.Count > 0)
                        {
                            num = flatten(num, false);
                            den = flatten(den, false);
                            ret = new Expression(Expression.Type.Divide, num, den);
                        }
                        else
                        {
                            ret = flatten(num, false);
                        }
                        break;
                    }

                case Expression.Type.Divide:
                    {
                        Expression num = new Expression(Expression.Type.Times);
                        Expression den = new Expression(Expression.Type.Times);
                        if (ret.Children[0].ExpressionType == Expression.Type.Divide)
                        {
                            num.Children.Add(ret.Children[0].Children[0]);
                            den.Children.Add(ret.Children[0].Children[1]);
                        }
                        else
                        {
                            num.Children.Add(ret.Children[0]);
                        }

                        if (ret.Children[1].ExpressionType == Expression.Type.Divide)
                        {
                            num.Children.Add(ret.Children[1].Children[1]);
                            den.Children.Add(ret.Children[1].Children[0]);
                        }
                        else
                        {
                            den.Children.Add(ret.Children[1]);
                        }

                        num = flatten(num, false);
                        den = flatten(den, false);
                        ret = new Expression(Expression.Type.Divide, num, den);
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
                        Expression newRet = new Expression(Expression.Type.Plus);
                        int result = 0;
                        foreach (Expression child in ret.Children)
                        {
                            if (child.ExpressionType == Expression.Type.Constant)
                            {
                                result += (int)child.Data;
                            }
                            else
                            {
                                newRet.Children.Add(child);
                            }
                        }
                        if (result != 0 || newRet.Children.Count == 0)
                        {
                            newRet.Children.Add(new Expression(Expression.Type.Constant, result));
                        }

                        if (newRet.Children.Count == 1)
                        {
                            ret = newRet.Children[0];
                        }
                        else
                        {
                            ret = newRet;
                        }

                        break;
                    }

                case Expression.Type.Times:
                    {
                        Expression newRet = new Expression(Expression.Type.Times);
                        int result = 1;
                        foreach (Expression child in ret.Children)
                        {
                            if (child.ExpressionType == Expression.Type.Constant)
                            {
                                result *= (int)child.Data;
                            }
                            else
                            {
                                newRet.Children.Add(child);
                            }
                        }

                        if (result == 0)
                        {
                            ret = new Expression(Expression.Type.Constant, result);
                        }
                        else
                        {
                            if (result != 1 || newRet.Children.Count == 0)
                            {
                                newRet.Children.Insert(0, new Expression(Expression.Type.Constant, result));
                            }

                            if (newRet.Children.Count == 1)
                            {
                                ret = newRet.Children[0];
                            }
                            else
                            {
                                ret = newRet;
                            }
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

            if (ret.ExpressionType == Expression.Type.Times)
            {
                Expression plus = new Expression(Expression.Type.Plus);
                foreach (Expression child in ret.Children)
                {
                    Expression factor;

                    if (child.ExpressionType == Expression.Type.Plus)
                    {
                        factor = child;
                    }
                    else
                    {
                        factor = new Expression(Expression.Type.Plus, child);
                    }

                    Expression newPlus = new Expression(Expression.Type.Plus);
                    foreach (Expression term in factor.Children)
                    {
                        if (plus.Children.Count == 0)
                        {
                            newPlus.Children.Add(term);
                        }
                        else
                        {
                            foreach (Expression oldTerm in plus.Children)
                            {
                                Expression times = new Expression(Expression.Type.Times, oldTerm, term);
                                times = flatten(times, false);
                                newPlus.Children.Add(times);
                            }
                        }
                    }
                    plus = newPlus;
                }
                ret = flatten(plus, false);
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
