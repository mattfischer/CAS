using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CAS
{
    class Polynomial
    {
        public Polynomial(Rational constant)
        {
            this.variable = null;
            this.coefficients = new Rational[] { constant };
        }

        public Polynomial(Node variable, Rational[] coefficients)
        {
            this.variable = variable;
            if (coefficients.Length > 0)
            {
                int max = 0;
                for (int i = 0; i < coefficients.Length; i++)
                {
                    if (coefficients[i] != new Rational(0))
                    {
                        max = i;
                    }
                }

                this.coefficients = new Rational[max + 1];
                Array.Copy(coefficients, this.coefficients, max + 1);
            }
            else
            {
                this.coefficients = new Rational[] { new Rational(0) };
            }
        }

        public Polynomial(Node variable, Dictionary<int, Rational> coefficients)
        {
            this.variable = variable;
            int degree = -1;
            foreach (int exp in coefficients.Keys)
            {
                if (coefficients[exp] != new Rational(0))
                {
                    degree = Math.Max(degree, exp);
                }
            }
            this.coefficients = new Rational[degree + 1];
            for (int i = 0; i <= degree; i++)
            {
                if (coefficients.ContainsKey(i))
                {
                    this.coefficients[i] = coefficients[i];
                }
                else
                {
                    this.coefficients[i] = new Rational(0);
                }
            }
        }

        public Rational[] Coefficients
        {
            get { return coefficients; }
        }

        public Node Variable
        {
            get { return variable; }
        }

        public int Degree
        {
            get { return coefficients.Length - 1; }
        }

        public static Polynomial[] divide(Polynomial dividend, Polynomial divisor)
        {
            Polynomial quotient;
            Polynomial remainder;
            if (divisor.Degree > dividend.Degree)
            {
                quotient = new Polynomial(dividend.Variable, new Rational[] { new Rational(0) });
                remainder = dividend;
                return new Polynomial[] { quotient, remainder };
            }

            Rational[] coeffs = new Rational[dividend.Degree + 1];
            Array.Copy(dividend.Coefficients, coeffs, dividend.Degree + 1);
            for (int i = dividend.Degree; i >= divisor.Degree; i--)
            {
                coeffs[i] /= divisor.Coefficients[divisor.Degree];
                for (int j = 1; j <= divisor.Degree; j++)
                {
                    coeffs[i - j] -= coeffs[i] * divisor.Coefficients[divisor.Degree - j];
                }
            }

            Rational[] quotientCoeffs = new Rational[dividend.Degree - divisor.Degree + 1];
            Rational[] remainderCoeffs = new Rational[divisor.Degree];
            Array.Copy(coeffs, remainderCoeffs, remainderCoeffs.Length);
            Array.Copy(coeffs, remainderCoeffs.Length, quotientCoeffs, 0, quotientCoeffs.Length);

            quotient = new Polynomial(dividend.Variable, quotientCoeffs);
            remainder = new Polynomial(dividend.Variable, remainderCoeffs);
            return new Polynomial[] { quotient, remainder };
        }

        public static Polynomial gcd(Polynomial a, Polynomial b)
        {
            while (b.Degree > 0 || b.coefficients[0] != new Rational(0))
            {
                Polynomial t = b;
                Polynomial[] result = divide(a, b);
                b = result[1];
                a = t;
            }

            return a;
        }

        public static Polynomial FromNode(Node node)
        {
            Dictionary<int, Rational> coeffs = new Dictionary<int, Rational>();
            Node variable = null;
            foreach (Node term in NodeMath.terms(node))
            {
                Node[] coeffTerm = NodeMath.coefficientTerm(term);
                Node coefficient = coeffTerm[0];
                Node var = coeffTerm[1];
                Node fact = NodeMath.factor(var);
                Node exp = NodeMath.exponent(var);

                if (!NodeMath.isConstant(coefficient) || !NodeMath.isConstant(exp))
                {
                    return null;
                }

                switch (fact.NodeType)
                {
                    case Node.Type.Variable:
                        {
                            if (variable != null && (string)variable.Data != (string)fact.Data)
                            {
                                return null;
                            }
                            else
                            {
                                variable = fact;
                            }

                            coeffs.Add((int)exp.Data, new Rational((int)coefficient.Data));
                            break;
                        }

                    case Node.Type.Constant:
                        {
                            coeffs.Add(0, new Rational((int)coefficient.Data));
                            break;
                        }
                }
            }

            Polynomial poly = new Polynomial(variable, coeffs);
            return poly;
        }

        public Node ToNode()
        {
            Node ret = NodeMath.constant(0);
            for (int i = 0; i < Degree + 1; i++)
            {
                if (i == 0)
                {
                    ret = NodeMath.add(ret, NodeMath.constant(Coefficients[i]));
                }
                else
                {
                    ret = NodeMath.add(ret, NodeMath.multiply(NodeMath.constant(Coefficients[i]), NodeMath.power(Variable, NodeMath.constant(i))));
                }
            }

            return ret;
        }

        Rational[] coefficients;
        Node variable;
    }
}
