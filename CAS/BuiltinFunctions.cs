using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CAS
{
    class BuiltinFunctions
    {
        delegate Node BuiltinFunctionDelegate(Node node);

        struct FunctionEntry
        {
            public string name;
            public BuiltinFunctionDelegate func;

            public FunctionEntry(string name, BuiltinFunctionDelegate func)
            {
                this.name = name;
                this.func = func;
            }
        };

        static FunctionEntry[] functions = { new FunctionEntry("substitute", substitute) };

        public static Node call(Node node)
        {
            Node ret = node;

            if (ret.NodeType == Node.Type.Function)
            {
                foreach (FunctionEntry entry in functions)
                {
                    if (entry.name == (string)ret.Data)
                    {
                        ret = entry.func(ret);
                        break;
                    }
                }
            }
            return ret;
        }

        static Node replace(Node source, Node var, Node val)
        {
            Node ret = source;

            if (source == var)
            {
                ret = val;
            }
            else
            {
                List<Node> children = new List<Node>();
                if (source.Children != null)
                {
                    foreach (Node child in source.Children)
                    {
                        children.Add(replace(child, var, val));
                    }
                }
                ret = new Node(source.NodeType, source.Data, children.ToArray());
            }

            return ret;
        }

        static Node substitute(Node node)
        {
            Node source = node.Children[0];
            Node var = node.Children[1];
            Node val = node.Children[2];

            Node ret = replace(source, var, val);

            return ret;
        }
    }
}
