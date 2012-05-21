using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CAS
{
    class Simplify
    {
        public delegate void LogNodeDelegate(Node oldNode, Node newNode, string title);
        delegate Node NodeOperationDelegate(Node node);

        static LogNodeDelegate log = null;
        public static Node Eval(Node node, LogNodeDelegate lg)
        {
            log = lg;

            node = recurse(node, BuiltinFunctions.call, "Call");
            node = recurse(node, removeMinus, "RemoveMinus");
            node = recurse(node, flatten, "Flatten");
            node = recurse(node, rationalize, "Rationalize");
            node = recurse(node, expand, "Expand");
            node = recurse(node, fold, "Fold");
            node = recurse(node, collect, "Collect");
            node = recurse(node, rationalPoly, "RationalPoly");

            return node;
        }

        static Node removeMinus(Node node)
        {
            Node ret = node;

            switch (ret.NodeType)
            {
                case Node.Type.Minus:
                    ret = NodeMath.add(ret.Children[0], NodeMath.multiply(NodeMath.constant(-1), ret.Children[1]));
                    break;

                case Node.Type.Negative:
                    ret = NodeMath.multiply(NodeMath.constant(-1), ret.Children[0]);
                    break;
            }

            return ret;
        }

        static Node flatten(Node node)
        {
            Node ret = node;

            switch (ret.NodeType)
            {
                case Node.Type.Plus:
                case Node.Type.Times:
                    {
                        List<Node> children = new List<Node>();
                        if (ret.Children != null)
                        {
                            foreach (Node child in ret.Children)
                            {
                                if (child.NodeType == ret.NodeType)
                                {
                                    children.AddRange(child.Children);
                                }
                                else
                                {
                                    children.Add(child);
                                }
                            }
                        }

                        if (children.Count == 1)
                        {
                            ret = children[0];
                        }
                        else
                        {
                            ret = new Node(ret.NodeType, children.ToArray());
                        }
                        break;
                    }
            }

            return ret;
        }

        static Node rationalize(Node node)
        {
            Node ret = node;

            switch (ret.NodeType)
            {
                case Node.Type.Plus:
                    {
                        Node num = NodeMath.constant(0);
                        Node den = NodeMath.constant(1);
                        foreach (Node term in NodeMath.terms(ret))
                        {
                            if (NodeMath.denominator(term) == den)
                            {
                                num = NodeMath.add(num, NodeMath.numerator(term));
                            }
                            else
                            {
                                num = NodeMath.add(NodeMath.multiply(NodeMath.denominator(term), num), NodeMath.multiply(den, NodeMath.numerator(term)));
                                den = NodeMath.multiply(den, NodeMath.denominator(term));
                            }
                        }

                        ret = NodeMath.divide(num, den);
                        break;
                    }

                case Node.Type.Times:
                    {
                        Node num = NodeMath.constant(1);
                        Node den = NodeMath.constant(1);

                        foreach (Node factor in NodeMath.factors(ret))
                        {
                            num = NodeMath.multiply(num, NodeMath.numerator(factor));
                            den = NodeMath.multiply(den, NodeMath.denominator(factor));
                        }

                        ret = NodeMath.divide(num, den);
                        break;
                    }

                case Node.Type.Divide:
                    {
                        Node num = NodeMath.multiply(NodeMath.numerator(NodeMath.numerator(ret)), NodeMath.denominator(NodeMath.denominator(ret)));
                        Node den = NodeMath.multiply(NodeMath.denominator(NodeMath.numerator(ret)), NodeMath.numerator(NodeMath.denominator(ret)));

                        ret = NodeMath.divide(num, den);
                        break;
                    }
            }

            return ret;
        }

        static Node fold(Node node)
        {
            Node ret = node;

            switch (ret.NodeType)
            {
                case Node.Type.Plus:
                    {
                        Node rest = NodeMath.constant(0);
                        int result = 0;
                        foreach (Node term in NodeMath.terms(ret))
                        {
                            if (NodeMath.isConstant(term))
                            {
                                result += NodeMath.constantValue(term);
                            }
                            else
                            {
                                rest = NodeMath.add(rest, term);
                            }
                        }

                        ret = NodeMath.add(rest, NodeMath.constant(result));
                        break;
                    }

                case Node.Type.Times:
                    {
                        Node rest = NodeMath.constant(1);
                        int result = 1;
                        foreach (Node factor in NodeMath.factors(ret))
                        {
                            if (NodeMath.isConstant(factor))
                            {
                                result *= NodeMath.constantValue(factor);
                            }
                            else
                            {
                                rest = NodeMath.multiply(rest, factor);
                            }
                        }

                        ret = NodeMath.multiply(NodeMath.constant(result), rest);
                        break;
                    }
            }

            return ret;
        }

        static Node expand(Node node)
        {
            Node ret = node;

            switch (ret.NodeType)
            {
                case Node.Type.Plus:
                    ret = flatten(ret);
                    break;

                case Node.Type.Times:
                    {
                        Node expansion = NodeMath.constant(1);
                        foreach (Node factor in NodeMath.factors(ret))
                        {
                            Node newExpansion = NodeMath.constant(0);
                            foreach (Node factorTerm in NodeMath.terms(factor))
                            {
                                foreach (Node term in NodeMath.terms(expansion))
                                {
                                    newExpansion = NodeMath.add(newExpansion, NodeMath.multiply(term, factorTerm));
                                }
                            }
                            expansion = newExpansion;
                        }
                        ret = expansion;
                        break;
                    }
            }

            return ret;
        }

        static Node collect(Node node)
        {
            Node ret = node;

            switch (ret.NodeType)
            {
                case Node.Type.Plus:
                    {
                        Dictionary<Node, Node> dict = new Dictionary<Node, Node>();

                        foreach (Node child in NodeMath.terms(node))
                        {
                            Node[] coeffTerm = NodeMath.coefficientTerm(child);
                            Node coefficient = coeffTerm[0];
                            Node term = coeffTerm[1];

                            term = recurse(term, sort, "Sort");
                            if (dict.ContainsKey(term))
                            {
                                dict[term] = NodeMath.add(dict[term], coefficient);
                            }
                            else
                            {
                                dict.Add(term, coefficient);
                            }
                        }

                        ret = NodeMath.constant(0);
                        foreach (Node term in dict.Keys)
                        {
                            ret = NodeMath.add(ret, NodeMath.multiply(fold(dict[term]), term));
                        }
                        break;
                    }

                case Node.Type.Times:
                    {
                        Dictionary<Node, Node> dict = new Dictionary<Node, Node>();

                        foreach (Node child in NodeMath.factors(node))
                        {
                            Node fact = NodeMath.factor(child);
                            Node exp = NodeMath.exponent(child);

                            fact = recurse(fact, sort, "Sort");
                            if (dict.ContainsKey(fact))
                            {
                                dict[fact] = NodeMath.add(dict[fact], exp);
                            }
                            else
                            {
                                dict.Add(fact, exp);
                            }
                        }

                        ret = NodeMath.constant(1);
                        foreach (Node factor in dict.Keys)
                        {
                            ret = NodeMath.multiply(ret, NodeMath.power(factor, fold(dict[factor])));
                        }
                        break;
                    }
            }

            return ret;
        }

        static Node recurse(Node node, NodeOperationDelegate func, string logTitle)
        {
            List<Node> children = new List<Node>();
            if (node.Children != null)
            {
                foreach (Node child in node.Children)
                {
                    Node newChild = recurse(child, func, logTitle);
                    log(child, newChild, logTitle);
                    children.Add(newChild);
                }
            }

            Node ret = new Node(node.NodeType, node.Data, children.ToArray());
            log(node, ret, logTitle);
            node = ret;
            ret = func(ret);
            log(node, ret, logTitle);
            return ret;
        }

        static Node sort(Node node)
        {
            Node ret = node;

            switch(ret.NodeType)
            {
                case Node.Type.Plus:
                case Node.Type.Times:
                    {
                        Node[] array = new Node[ret.Children.Length];
                        Array.Copy(ret.Children, array, ret.Children.Length);
                        Array.Sort(array);

                        ret = new Node(ret.NodeType, ret.Data, array);
                        break;
                    }
            }

            return ret;
        }

        static Node rationalPoly(Node node)
        {
            Node ret = node;

            if (node.NodeType == Node.Type.Divide)
            {
                Node num = NodeMath.numerator(ret);
                Node den = NodeMath.denominator(ret);
                Polynomial numPoly = Polynomial.FromNode(num);
                Polynomial denPoly = Polynomial.FromNode(den);

                if (numPoly != null && denPoly != null)
                {
                    Polynomial gcd = Polynomial.gcd(numPoly, denPoly);

                    num = Polynomial.divide(numPoly, gcd)[0].ToNode();
                    den = Polynomial.divide(denPoly, gcd)[0].ToNode();

                    ret = NodeMath.divide(num, den);
                    log(node, ret, "RationalPoly");
                    ret = recurse(ret, rationalize, "Rationalize");
                    ret = recurse(ret, fold, "Fold");
                }
            }

            return ret;
        }
    }
}
