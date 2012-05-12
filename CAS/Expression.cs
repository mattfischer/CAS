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

        public List<Expression> Children
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
            this.children = new List<Expression>();
            this.data = data;
        }

        public Expression(Type type, params Expression[] children)
        {
            this.type = type;
            this.children = new List<Expression>(children);
            this.data = null;
        }

        public Expression(Type type, List<Expression> children)
        {
            this.type = type;
            this.children = children;
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
        List<Expression> children;
        Object data;
    }
}
