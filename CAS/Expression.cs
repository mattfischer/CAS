using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CAS
{
    public class Expression
    {
        public enum Type
        {
            Constant,
            Variable,
            Plus,
            Minus,
            Times,
            Divide,
            Negative
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

        public override bool Equals(object o)
        {
            if (o == null)
            {
                return false;
            }

            if (o.GetType() != GetType())
            {
                return false;
            }

            Expression b = (Expression)o;

            if (type != b.type)
            {
                return false;
            }

            if((data == null && b.data != null) || (data != null && b.data == null)) {
                return false;
            }

            if(data != null && !data.Equals(b.data)) {
                return false;
            }

            if(children == null && b.children == null)
            {
                return true;
            }

            if (children == null || b.children == null)
            {
                return false;
            }

            if (children.Length != b.children.Length)
            {
                return false;
            }

            for (int i = 0; i < children.Length; i++)
            {
                if (!children[i].Equals(b.children[i]))
                {
                    return false;
                }
            }

            return true;
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
                default:
                    return data.ToString();
            }
        }

        Type type;
        Expression[] children;
        Object data;
    }
}
