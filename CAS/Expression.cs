﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CAS
{
    public class Expression : IComparable
    {
        public enum Type
        {
            Constant,
            Variable,
            Plus,
            Minus,
            Times,
            Divide,
            Negative,
            Power,
            Function
        };

        public Type ExpressionType
        {
            get { return type; }
        }

        public Expression[] Children
        {
            get { return children; }
        }

        public Object Data
        {
            get { return data; }
        }

        public Expression(Type type, Object data = null)
        {
            this.type = type;
            this.children = null;
            this.data = data;
        }

        public Expression(Type type, Object data, params Expression[] children)
        {
            this.type = type;
            if (children.Length > 0)
            {
                this.children = children;
            }
            else
            {
                this.children = null;
            }
            this.data = data;
        }

        public Expression(Type type, params Expression[] children)
        {
            this.type = type;
            if (children.Length > 0)
            {
                this.children = children;
            }
            else
            {
                this.children = null;
            }
            this.data = null;
        }

        public int CompareTo(Object o)
        {
            if (o == null)
            {
                return 1;
            }

            if (o.GetType() != GetType())
            {
                return 1;
            }

            Expression b = (Expression)o;

            if (type != b.type)
            {
                return type.CompareTo(b.type);
            }

            if (data == null && b.data != null)
            {
                return 1;
            }

            int result;

            if (data != null)
            {
                result = ((IComparable)data).CompareTo((IComparable)b.data);
                if (result != 0)
                {
                    return result;
                }
            }

            if(children == null && b.children == null)
            {
                return 0;
            }

            if (children == null)
            {
                return -1;
            }
            else if (b.children == null)
            {
                return 1;
            }

            if (children.Length > b.children.Length)
            {
                return 1;
            }
            else if(children.Length < b.children.Length)
            {
                return -1;
            }

            for (int i = 0; i < children.Length; i++)
            {
                result = children[i].CompareTo(b.children[i]);
                if (result != 0)
                {
                    return result;
                }
            }

            return 0;
        }

        public override bool Equals(object obj)
        {
            return CompareTo(obj) == 0;
        }

        public static bool operator ==(Expression a, Expression b)
        {
            if (ReferenceEquals(a, null))
            {
                return ReferenceEquals(a, b);
            }

            return a.Equals(b);
        }

        public static bool operator !=(Expression a, Expression b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            int hash;

            hash = type.GetHashCode();
            if (data != null)
            {
                hash ^= data.GetHashCode();
            }

            if (children != null)
            {
                foreach (Expression child in children)
                {
                    hash ^= child.GetHashCode();
                }
            }

            return hash;
        }

        public override string ToString()
        {
            switch (type)
            {
                case Type.Plus:
                    return "+";
                case Type.Minus:
                case Type.Negative:
                    return "-";
                case Type.Times:
                    return "*";
                case Type.Divide:
                    return "/";
                case Type.Power:
                    return "^";
                default:
                    return data.ToString();
            }
        }

        Type type;
        Expression[] children;
        Object data;
    }
}
