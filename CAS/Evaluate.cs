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
            ex = flatten(ex, true);
            log(ex, "Flatten");

            ex = commonDenominator(ex);
            log(ex, "Common Denominator");

            ex = fold(ex);
            log(ex, "Fold");

            return ex;
        }

        static Expression flatten(Expression ex, bool recursive)
        {
            List<Expression> children = ex.Children;
            if(recursive && ex.Children != null) {
                children = new List<Expression>();
                foreach (Expression child in ex.Children)
                {
                    children.Add(flatten(child, recursive));
                }
            }

            switch (ex.ExpressionType)
            {
                case Expression.Type.Plus:
                    {
                        Expression plus = new Expression(Expression.Type.Plus, new List<Expression>());
                        foreach (Expression child in children)
                        {
                            if (child.ExpressionType == Expression.Type.Plus)
                            {
                                plus.Children.AddRange(child.Children);
                            }
                            else
                            {
                                plus.Children.Add(child);
                            }
                        }

                        if (plus.Children.Count == 1)
                        {
                            plus = plus.Children[0];
                        }

                        return plus;
                    }

                case Expression.Type.Times:
                    {
                        Expression times = new Expression(Expression.Type.Times, new List<Expression>());
                        foreach (Expression child in children)
                        {
                            if (child.ExpressionType == Expression.Type.Times)
                            {
                                times.Children.AddRange(child.Children);
                            }
                            else
                            {
                                times.Children.Add(child);
                            }
                        }

                        if (times.Children.Count == 1)
                        {
                            times = times.Children[0];
                        }

                        return times;
                    }

                case Expression.Type.Divide:
                    {
                        Expression num = new Expression(Expression.Type.Times, new List<Expression>());
                        Expression den = new Expression(Expression.Type.Times, new List<Expression>());
                        if (children[0].ExpressionType == Expression.Type.Divide)
                        {
                            num.Children.Add(children[0].Children[0]);
                            den.Children.Add(children[0].Children[1]);
                        }
                        else
                        {
                            num.Children.Add(children[0]);
                        }

                        if (children[1].ExpressionType == Expression.Type.Divide)
                        {
                            num.Children.Add(children[1].Children[1]);
                            den.Children.Add(children[1].Children[0]);
                        }
                        else
                        {
                            den.Children.Add(children[1]);
                        }

                        Expression div = new Expression(Expression.Type.Divide, new List<Expression>());
                        div.Children.Add(flatten(num, false));
                        div.Children.Add(flatten(den, false));

                        return div;
                    }

                default:
                    return new Expression(ex.ExpressionType, children, ex.Data);
            }
        }

        static Expression commonDenominator(Expression ex)
        {
            List<Expression> children = null;
            if (ex.Children != null)
            {
                children = new List<Expression>();
                foreach (Expression child in ex.Children)
                {
                    children.Add(commonDenominator(child));
                }
            }

            switch (ex.ExpressionType)
            {
                case Expression.Type.Plus:
                    {
                        Expression num = new Expression(Expression.Type.Plus, new List<Expression>());
                        Expression den = new Expression(Expression.Type.Times, new List<Expression>());
                        foreach (Expression child in children)
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
                                Expression newNum = new Expression(Expression.Type.Plus, new List<Expression>());
                                foreach (Expression numChild in num.Children)
                                {
                                    Expression numTimes = new Expression(Expression.Type.Times, new List<Expression>());
                                    numTimes.Children.Add(childDen);
                                    numTimes.Children.Add(numChild);
                                    numTimes = flatten(numTimes, false);
                                    newNum.Children.Add(numTimes);
                                }

                                num = newNum;
                            }

                            Expression newTerm;

                            if (den.Children.Count > 0)
                            {
                                newTerm = new Expression(Expression.Type.Times, new List<Expression>());
                                newTerm.Children.Add(den);
                                newTerm.Children.Add(childNum);
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
                            Expression div = new Expression(Expression.Type.Divide, new List<Expression>());
                            div.Children.Add(flatten(num, false));
                            div.Children.Add(flatten(den, false));
                            div = flatten(div, false);
                            return div;
                        }
                        else
                        {
                            return flatten(num, false);
                        }
                    }

                case Expression.Type.Times:
                    {
                        Expression num = new Expression(Expression.Type.Times, new List<Expression>());
                        Expression den = new Expression(Expression.Type.Times, new List<Expression>());

                        foreach (Expression child in children)
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
                            Expression div = new Expression(Expression.Type.Divide, new List<Expression>());
                            div.Children.Add(flatten(num, false));
                            div.Children.Add(flatten(den, false));
                            div = flatten(div, false);
                            return div;
                        }
                        else
                        {
                            return flatten(num, false);
                        }
                    }

                case Expression.Type.Divide:
                    {
                        Expression div = new Expression(Expression.Type.Divide, children);
                        div = flatten(div, false);
                        return div;
                    }

                default:
                    return new Expression(ex.ExpressionType, children, ex.Data);
            }
        }

        static Expression fold(Expression ex)
        {
            List<Expression> children = null;
            if(ex.Children != null) {
                children = new List<Expression>();
                foreach(Expression child in ex.Children)
                {
                    children.Add(fold(child));
                }
            }

            switch (ex.ExpressionType)
            {
                case Expression.Type.Plus:
                    {
                        Expression plus = new Expression(Expression.Type.Plus, new List<Expression>());
                        int result = 0;
                        foreach (Expression child in children)
                        {
                            if (child.ExpressionType == Expression.Type.Constant)
                            {
                                result += (int)child.Data;
                            }
                            else
                            {
                                plus.Children.Add(child);
                            }
                        }
                        plus.Children.Add(new Expression(Expression.Type.Constant, null, result));
                        if (plus.Children.Count == 1)
                        {
                            plus = plus.Children[0];
                        }
                        return plus;
                    }

                case Expression.Type.Times:
                    {
                        Expression times = new Expression(Expression.Type.Times, new List<Expression>());
                        int result = 1;
                        foreach (Expression child in children)
                        {
                            if (child.ExpressionType == Expression.Type.Constant)
                            {
                                result *= (int)child.Data;
                            }
                            else
                            {
                                times.Children.Add(child);
                            }
                        }

                        times.Children.Add(new Expression(Expression.Type.Constant, null, result));
                        if (times.Children.Count == 1)
                        {
                            times = times.Children[0];
                        }
                        return times;
                    }

                case Expression.Type.Divide:
                    {
                        Expression div = new Expression(Expression.Type.Divide, new List<Expression>());
                        div.Children.Add(children[0]);
                        div.Children.Add(children[1]);

                        if (div.Children[0].ExpressionType == Expression.Type.Constant && div.Children[1].ExpressionType == Expression.Type.Constant)
                        {
                            int num = (int)div.Children[0].Data;
                            int den = (int)div.Children[1].Data;
                            int gcd = greatestCommonDivisor(num, den);
                            num /= gcd;
                            den /= gcd;
                            div.Children[0] = new Expression(Expression.Type.Constant, null, num);
                            div.Children[1] = new Expression(Expression.Type.Constant, null, den);
                        }

                        if (div.Children[0].ExpressionType == Expression.Type.Constant && (int)div.Children[0].Data == 0)
                        {
                            div = div.Children[0];
                        }
                        else if (div.Children[1].ExpressionType == Expression.Type.Constant && (int)div.Children[1].Data == 1)
                        {
                            div = div.Children[0];
                        }

                        return div;
                    }

                default:
                    return new Expression(ex.ExpressionType, children, ex.Data);
            }
        }

        static int greatestCommonDivisor(int a, int b)
        {
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
