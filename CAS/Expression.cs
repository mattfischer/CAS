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
            Plus,
            Minus,
            Times,
            Divide
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

        public Expression(Type type, List<Expression> children = null, Object data = null)
        {
            this.type = type;
            this.children = children;
            this.data = data;
        }

        public override string ToString()
        {
            switch (type)
            {
                case Type.Constant:
                    return ((int)data).ToString();
                case Type.Plus:
                    return "+";
                case Type.Minus:
                    return "-";
                case Type.Times:
                    return "*";
                case Type.Divide:
                    return "/";
                default:
                    return "";
            }
        }

        Type type;
        List<Expression> children;
        Object data;
    }
}
