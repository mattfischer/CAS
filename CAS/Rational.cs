using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CAS
{
    class Rational
    {
        public Rational(int numerator, int denominator)
        {
            this.numerator = numerator;
            this.denominator = denominator;

            reduce();
        }

        public Rational(int value)
        {
            this.numerator = value;
            this.denominator = 1;
        }

        public int Numerator
        {
            get { return numerator; }
        }

        public int Denominator
        {
            get { return denominator; }
        }

        public static Rational operator+(Rational a, Rational b)
        {
            return new Rational(a.Numerator*b.Denominator + a.Denominator*b.Numerator, a.Denominator*b.Denominator);
        }

        public static Rational operator -(Rational a, Rational b)
        {
            return new Rational(a.Numerator * b.Denominator - a.Denominator * b.Numerator, a.Denominator * b.Denominator);
        }

        public static Rational operator *(Rational a, Rational b)
        {
            return new Rational(a.Numerator * b.Numerator, a.Denominator * b.Denominator);
        }

        public static Rational operator /(Rational a, Rational b)
        {
            return new Rational(a.Numerator * b.Denominator, a.Denominator * b.Numerator);
        }

        public override bool Equals(Object o)
        {
            Rational a = (Rational)o;
            if (a == null)
            {
                return false;
            }

            return Numerator == a.Numerator && Denominator == a.Denominator;
        }

        public static bool operator ==(Rational a, Rational b)
        {
            if (ReferenceEquals(a, null))
            {
                return ReferenceEquals(b, null);
            }

            return a.Equals(b);
        }

        public static bool operator !=(Rational a, Rational b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return numerator ^ denominator;
        }

        public override string ToString()
        {
            if (denominator == 1)
            {
                return numerator.ToString();
            }
            else
            {
                return numerator.ToString() + "/" + denominator.ToString();
            }
        }

        void reduce()
        {
            int a = Math.Abs(Numerator);
            int b = Math.Abs(Denominator);

            while (b != 0)
            {
                int t = b;
                b = a % b;
                a = t;
            }

            numerator /= a;
            denominator /= a;

            if (denominator < 0)
            {
                numerator = -numerator;
                denominator = -denominator;
            }
        }

        int numerator;
        int denominator;
    }
}
