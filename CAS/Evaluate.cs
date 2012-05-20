using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CAS
{
    class Evaluate
    {
        public delegate void LogNodeDelegate(Node oldNode, Node newNode, string title);
        delegate Node NodeOperationDelegate(Node node);

        static LogNodeDelegate log = null;
        public static Node Eval(Node node, LogNodeDelegate lg)
        {
            log = lg;

            node = recurse(node, call, "Call");
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
                    ret = add(ret.Children[0], multiply(constant(-1), ret.Children[1]));
                    break;

                case Node.Type.Negative:
                    ret = multiply(constant(-1), ret.Children[0]);
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
                        Node num = constant(0);
                        Node den = constant(1);
                        foreach (Node term in terms(ret))
                        {
                            if (denominator(term) == den)
                            {
                                num = add(num, numerator(term));
                            }
                            else
                            {
                                num = add(multiply(denominator(term), num), multiply(den, numerator(term)));
                                den = multiply(den, denominator(term));
                            }
                        }

                        ret = divide(num, den);
                        break;
                    }

                case Node.Type.Times:
                    {
                        Node num = constant(1);
                        Node den = constant(1);

                        foreach (Node factor in factors(ret))
                        {
                            num = multiply(num, numerator(factor));
                            den = multiply(den, denominator(factor));
                        }

                        ret = divide(num, den);
                        break;
                    }

                case Node.Type.Divide:
                    {
                        Node num = multiply(numerator(numerator(ret)), denominator(denominator(ret)));
                        Node den = multiply(denominator(numerator(ret)), numerator(denominator(ret)));

                        ret = divide(num, den);
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
                        Node rest = constant(0);
                        int result = 0;
                        foreach (Node term in terms(ret))
                        {
                            if (isConstant(term))
                            {
                                result += constantValue(term);
                            }
                            else
                            {
                                rest = add(rest, term);
                            }
                        }

                        ret = add(rest, constant(result));
                        break;
                    }

                case Node.Type.Times:
                    {
                        Node rest = constant(1);
                        int result = 1;
                        foreach (Node factor in factors(ret))
                        {
                            if (isConstant(factor))
                            {
                                result *= constantValue(factor);
                            }
                            else
                            {
                                rest = multiply(rest, factor);
                            }
                        }

                        ret = multiply(constant(result), rest);
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
                        Node expansion = constant(1);
                        foreach (Node factor in factors(ret))
                        {
                            Node newExpansion = constant(0);
                            foreach (Node factorTerm in terms(factor))
                            {
                                foreach (Node term in terms(expansion))
                                {
                                    newExpansion = add(newExpansion, multiply(term, factorTerm));
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

                        foreach (Node child in terms(node))
                        {
                            Node[] coeffTerm = coefficientTerm(child);
                            Node coefficient = coeffTerm[0];
                            Node term = coeffTerm[1];

                            term = recurse(term, sort, "Sort");
                            if (dict.ContainsKey(term))
                            {
                                dict[term] = add(dict[term], coefficient);
                            }
                            else
                            {
                                dict.Add(term, coefficient);
                            }
                        }

                        ret = constant(0);
                        foreach (Node term in dict.Keys)
                        {
                            ret = add(ret, multiply(fold(dict[term]), term));
                        }
                        break;
                    }

                case Node.Type.Times:
                    {
                        Dictionary<Node, Node> dict = new Dictionary<Node, Node>();

                        foreach (Node child in factors(node))
                        {
                            Node fact = factor(child);
                            Node exp = exponent(child);

                            fact = recurse(fact, sort, "Sort");
                            if (dict.ContainsKey(fact))
                            {
                                dict[fact] = add(dict[fact], exp);
                            }
                            else
                            {
                                dict.Add(fact, exp);
                            }
                        }

                        ret = constant(1);
                        foreach (Node factor in dict.Keys)
                        {
                            ret = multiply(ret, power(factor, fold(dict[factor])));
                        }
                        break;
                    }
            }

            return ret;
        }

        static Node add(params Node[] nodes)
        {
            List<Node> children = new List<Node>();
            foreach (Node child in nodes)
            {
                if (child != constant(0))
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
                return flatten(new Node(Node.Type.Plus, children.ToArray()));
            }
        }

        static Node multiply(params Node[] nodes)
        {
            List<Node> children = new List<Node>();
            bool zero = false;
            foreach (Node child in nodes)
            {
                if (child == constant(0))
                {
                    zero = true;
                    break;
                }

                if (child != constant(1))
                {
                    children.Add(child);
                }
            }

            if (zero)
            {
                return constant(0);
            }
            else if (children.Count == 0)
            {
                return constant(1);
            }
            else if (children.Count == 1)
            {
                return children[0];
            }
            else
            {
                return flatten(new Node(Node.Type.Times, children.ToArray()));
            }
        }

        static Node divide(Node num, Node den)
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

        static Node power(Node factor, Node exponent)
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

        static Node numerator(Node node)
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

        static Node denominator(Node node)
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

        static Node exponent(Node node)
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

        static Node factor(Node node)
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

        static Node[] terms(Node node)
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

        static Node[] factors(Node node)
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

        static Node constant(int value)
        {
            return new Node(Node.Type.Constant, value);
        }

        static bool isConstant(Node node)
        {
            return node.NodeType == Node.Type.Constant;
        }

        static int constantValue(Node node)
        {
            return (int)node.Data;
        }

        static Node[] coefficientTerm(Node node)
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

        static Polynomial parsePolynomial(Node node)
        {
            Dictionary<int, Rational> coeffs = new Dictionary<int, Rational>();
            Node variable = null;
            foreach (Node term in terms(node))
            {
                Node[] coeffTerm = coefficientTerm(term);
                Node coefficient = coeffTerm[0];
                Node var = coeffTerm[1];
                Node fact = factor(var);
                Node exp = exponent(var);

                if (!isConstant(coefficient) || !isConstant(exp))
                {
                    return null;
                }

                switch(fact.NodeType) {
                    case Node.Type.Variable:
                        {
                            if (variable != null && (string)variable.Data != (string)fact.Data)
                            {
                                return null;
                            }
                            else
                            {
                                variable = fact;
                            }

                            coeffs.Add((int)exp.Data, new Rational((int)coefficient.Data));
                            break;
                        }

                    case Node.Type.Constant:
                        {
                            coeffs.Add(0, new Rational((int)coefficient.Data));
                            break;
                        }
                }
            }

            Polynomial poly = new Polynomial(variable, coeffs);
            return poly;
        }

        static Node rationalPoly(Node node)
        {
            Node ret = node;

            if (node.NodeType == Node.Type.Divide)
            {
                Node num = numerator(ret);
                Node den = denominator(ret);
                Polynomial numPoly = parsePolynomial(num);
                Polynomial denPoly = parsePolynomial(den);

                if (numPoly != null && denPoly != null)
                {
                    Polynomial gcd = Polynomial.gcd(numPoly, denPoly);

                    num = makePolynomial(Polynomial.divide(numPoly, gcd)[0]);
                    den = makePolynomial(Polynomial.divide(denPoly, gcd)[0]);

                    ret = divide(num, den);
                    log(node, ret, "RationalPoly");
                    ret = recurse(ret, rationalize, "Rationalize");
                    ret = recurse(ret, fold, "Fold");
                }
            }

            return ret;
        }

        struct FunctionEntry
        {
            public string name;
            public NodeOperationDelegate func;

            public FunctionEntry(string name, NodeOperationDelegate func)
            {
                this.name = name;
                this.func = func;
            }
        };

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

        static FunctionEntry[] functions = { new FunctionEntry("substitute", substitute) };

        static Node call(Node node)
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

        static Node makePolynomial(Polynomial poly)
        {
            Node ret = constant(0);
            for (int i = 0; i < poly.Degree + 1; i++)
            {
                if (i == 0)
                {
                    ret = add(ret, rational(poly.Coefficients[i]));
                } else {
                    ret = add(ret, multiply(rational(poly.Coefficients[i]), power(poly.Variable, constant(i))));
                }
            }

            return ret;
        }

        static Node rational(Rational rat)
        {
            return divide(constant(rat.Numerator), constant(rat.Denominator));
        }

        static int greatestCommonDivisor(int a, int b)
        {
            a = Math.Abs(a);
            b = Math.Abs(b);

            while (b != 0)
            {
                int t = b;
                b = a % b;
                a = t;
            }

            return a;
        }
    }
}
