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
                quotient = new Polynomial(new Rational(0));
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
            while (!b.Equals(new Rational(0)))
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
            int degree = -1;
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

                degree = Math.Max(degree, NodeMath.constantValue(exp));

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

                            coeffs.Add(NodeMath.constantValue(exp), new Rational((int)coefficient.Data));
                            break;
                        }

                    case Node.Type.Constant:
                        {
                            coeffs.Add(0, new Rational(NodeMath.constantValue(coefficient)));
                            break;
                        }
                }
            }

            Rational[] coefficients = new Rational[degree + 1];
            for (int i = 0; i <= degree; i++)
            {
                if (coeffs.ContainsKey(i))
                {
                    coefficients[i] = coeffs[i];
                }
                else
                {
                    coefficients[i] = new Rational(0);
                }
            }

            Polynomial poly = new Polynomial(variable, coefficients);
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

        public override bool Equals(object obj)
        {
            Polynomial poly;
            if (obj.GetType() == typeof(Rational))
            {
                poly = new Polynomial((Rational)obj);
            }
            else if (obj.GetType() == typeof(Polynomial))
            {
                poly = (Polynomial)obj;
            }
            else
            {
                return false;
            }

            if (variable != null && poly.variable != null && variable != poly.variable)
            {
                return false;
            }

            if (coefficients.Length != poly.coefficients.Length)
            {
                return false;
            }

            for (int i = 0; i < coefficients.Length; i++)
            {
                if (coefficients[i] != poly.coefficients[i])
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            int ret = 0;

            if(variable != null)
            {
                ret ^= variable.GetHashCode();
            }

            foreach (Rational coefficient in coefficients)
            {
                ret ^= coefficient.GetHashCode();
            }

            return ret;
        }

        Rational[] coefficients;
        Node variable;
    }
}
