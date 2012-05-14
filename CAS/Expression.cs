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
