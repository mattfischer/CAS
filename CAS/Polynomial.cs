using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CAS
{
    class Polynomial
    {
        public Polynomial(string variable, int[] coefficients)
        {
            this.variable = variable;
            this.coefficients = coefficients;
        }

        public Polynomial(string variable, Dictionary<int, int> coefficients)
        {
            this.variable = variable;
            int degree = -1;
            foreach (int exp in coefficients.Keys)
            {
                degree = Math.Max(degree, exp);
            }
            this.coefficients = new int[degree + 1];
            for (int i = 0; i <= degree; i++)
            {
                if (coefficients.ContainsKey(i))
                {
                    this.coefficients[i] = coefficients[i];
                }
            }
        }

        public int[] Coefficients
        {
            get { return coefficients; }
        }

        public string Variable
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
                quotient = new Polynomial(dividend.Variable, new int[] { 0 });
                remainder = dividend;
                return new Polynomial[] { quotient, remainder };
            }

            int[] coeffs = new int[dividend.Degree + 1];
            Array.Copy(dividend.Coefficients, coeffs, dividend.Degree + 1);
            for (int i = dividend.Degree; i >= divisor.Degree; i--)
            {
                coeffs[i] /= divisor.Coefficients[divisor.Degree];
                for (int j = 1; j <= divisor.Degree; j++)
                {
                    coeffs[i - j] -= coeffs[i] * divisor.Coefficients[divisor.Degree - j];
                }
            }

            int[] quotientCoeffs = new int[dividend.Degree - divisor.Degree + 1];
            int[] remainderCoeffs = new int[divisor.Degree];
            Array.Copy(coeffs, remainderCoeffs, remainderCoeffs.Length);
            Array.Copy(coeffs, remainderCoeffs.Length, quotientCoeffs, 0, quotientCoeffs.Length);

            quotient = new Polynomial(dividend.Variable, quotientCoeffs);
            remainder = new Polynomial(dividend.Variable, remainderCoeffs);
            return new Polynomial[] { quotient, remainder };
        }

        public static Polynomial gcd(Polynomial a, Polynomial b)
        {
            while (b.Degree > 0 || b.Coefficients[0] != 0)
            {
                Polynomial t = b;
                Polynomial[] result = divide(a, b);
                b = result[1];
                a = t;
            }

            return a;
        }

        int[] coefficients;
        string variable;
    }
}
