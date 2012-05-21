using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CAS
{
    class NodeMath
    {
        public static Node add(params Node[] nodes)
        {
            List<Node> children = new List<Node>();
            foreach (Node child in nodes)
            {
                if (child == constant(0))
                {
                    continue;
                }
                else if (child.NodeType == Node.Type.Plus)
                {
                    children.AddRange(child.Children);
                }
                else
                {
                    children.Add(child);
                }
            }

            if (children.Count == 0)
            {
                return constant(0);
            }
            else if (children.Count == 1)
            {
                return children[0];
            }
            else
            {
                return new Node(Node.Type.Plus, children.ToArray());
            }
        }

        public static Node multiply(params Node[] nodes)
        {
            List<Node> children = new List<Node>();

            foreach (Node child in nodes)
            {
                if (child == constant(0))
                {
                    children.Clear();
                    children.Add(constant(0));
                    break;
                }
                else if (child == constant(1))
                {
                    continue;
                }
                else if (child.NodeType == Node.Type.Times)
                {
                    children.AddRange(child.Children);
                }
                else
                {
                    children.Add(child);
                }
            }

            if (children.Count == 0)
            {
                return constant(1);
            }
            else if (children.Count == 1)
            {
                return children[0];
            }
            else
            {
                return new Node(Node.Type.Times, children.ToArray());
            }
        }

        public static Node divide(Node num, Node den)
        {
            if (num == constant(0))
            {
                return num;
            }
            else if (den == constant(1))
            {
                return num;
            }
            else
            {
                return new Node(Node.Type.Divide, num, den);
            }
        }

        public static Node power(Node factor, Node exponent)
        {
            if (exponent == constant(1))
            {
                return factor;
            }
            else
            {
                return new Node(Node.Type.Power, factor, exponent);
            }
        }

        public static Node numerator(Node node)
        {
            if (node.NodeType == Node.Type.Divide)
            {
                return node.Children[0];
            }
            else
            {
                return node;
            }
        }

        public static Node denominator(Node node)
        {
            if (node.NodeType == Node.Type.Divide)
            {
                return node.Children[1];
            }
            else
            {
                return constant(1);
            }
        }

        public static Node exponent(Node node)
        {
            if (node.NodeType == Node.Type.Power)
            {
                return node.Children[1];
            }
            else
            {
                return constant(1);
            }
        }

        public static Node factor(Node node)
        {
            if (node.NodeType == Node.Type.Power)
            {
                return node.Children[0];
            }
            else
            {
                return node;
            }
        }

        public static Node[] terms(Node node)
        {
            if (node.NodeType == Node.Type.Plus)
            {
                return node.Children;
            }
            else
            {
                return new Node[] { node };
            }
        }

        public static Node[] factors(Node node)
        {
            if (node.NodeType == Node.Type.Times)
            {
                return node.Children;
            }
            else
            {
                return new Node[] { node };
            }
        }

        public static Node constant(int value)
        {
            return new Node(Node.Type.Constant, value);
        }

        public static Node constant(Rational rat)
        {
            return NodeMath.divide(NodeMath.constant(rat.Numerator), NodeMath.constant(rat.Denominator));
        }

        public static bool isConstant(Node node)
        {
            return node.NodeType == Node.Type.Constant;
        }

        public static int constantValue(Node node)
        {
            return (int)node.Data;
        }

        public static Node[] coefficientTerm(Node node)
        {
            Node coefficient = constant(1);
            Node term = constant(1);

            foreach (Node child in factors(node))
            {
                if (isConstant(child))
                {
                    coefficient = multiply(coefficient, child);
                }
                else
                {
                    term = multiply(term, child);
                }
            }

            return new Node[] { coefficient, term };
        }
    }
}
