using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CAS
{
    public partial class TreeViewer : Form
    {
        class LayoutNode
        {
            public Expression expression;
            public List<LayoutNode> children;
            public Point position;
            public LayoutNode(Expression expression, List<LayoutNode> children, Point position)
            {
                this.expression = expression;
                this.children = children;
                this.position = position;
            }
        };

        const int BORDER = 50;
        const int VERTICAL_SPACE = 100;
        const int HORIZONTAL_SPACE = 115;
        const int CIRCLE_RADIUS = 20;
        static Pen EdgePen = new Pen(Color.DarkOliveGreen);
        static Pen CirclePen = new Pen(Color.DarkBlue, 2);
        static Brush CircleBrush = new SolidBrush(Color.White);
        static Font TextFont = new Font(FontFamily.GenericSansSerif, 12);
        static Brush TextBrush = new SolidBrush(Color.Black);

        public TreeViewer()
        {
            InitializeComponent();
        }

        private void TreeViewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        void moveNode(LayoutNode node, int delta)
        {
            node.position.X += delta;
            if (node.children != null)
            {
                foreach (LayoutNode child in node.children)
                {
                    moveNode(child, delta);
                }
            }
        }

        LayoutNode layoutExpression(Expression expression, List<LayoutNode> rightmost, int depth)
        {
            List<LayoutNode> children = null;
            int x, y;

            x = BORDER;
            y = BORDER + depth * VERTICAL_SPACE;
            if (expression.Children.Count > 0)
            {
                if (rightmost.Count == depth + 1)
                {
                    rightmost.Add(null);
                }

                children = new List<LayoutNode>();
                foreach (Expression child in expression.Children)
                {
                    LayoutNode childNode = layoutExpression(child, rightmost, depth + 1);
                    children.Add(childNode);
                }

                x = (children[0].position.X + children[children.Count - 1].position.X) / 2;
            }

            LayoutNode node = new LayoutNode(expression, children, new Point(x, y));

            if (rightmost[depth] != null)
            {
                int nextPosition = rightmost[depth].position.X + HORIZONTAL_SPACE;
                if (nextPosition > node.position.X)
                {
                    moveNode(node, nextPosition - node.position.X);
                }
            }

            rightmost[depth] = node;

            return node;
        }

        void layout()
        {
            int width, height;
            width = 0;
            height = 0;
            if (active != -1)
            {
                List<LayoutNode> rightmost = new List<LayoutNode>();
                rightmost.Add(null);
                layoutRoot = layoutExpression(expressions[active], rightmost, 0);


                width = 0;
                height = (rightmost.Count - 1) * VERTICAL_SPACE + 2 * BORDER;
                foreach (LayoutNode node in rightmost)
                {
                    if (node != null)
                    {
                        width = Math.Max(width, node.position.X + BORDER);
                    }
                }
            }

            DisplayPanel.AutoScrollMinSize = new Size(width, height);
            DisplayPanel.Invalidate();
        }

        List<Expression> expressions = new List<Expression>();
        int active = 0;
        LayoutNode layoutRoot = null;
        public void ClearExpressions()
        {
            expressions.Clear();
            SelectBox.Items.Clear();
            active = -1;
            Invalidate();
        }

        public void AddExpression(Expression expression, string title)
        {
            expressions.Add(expression);
            SelectBox.Items.Add(title);
            SelectBox.SelectedIndices.Clear();
            SelectBox.SelectedIndices.Add(expressions.Count - 1);
        }

        void drawNode(LayoutNode node, Graphics g)
        {
            if (node.children != null)
            {
                foreach (LayoutNode child in node.children)
                {
                    g.DrawLine(EdgePen, node.position, child.position);
                    drawNode(child, g);
                }
            }

            g.FillEllipse(CircleBrush, node.position.X - CIRCLE_RADIUS, node.position.Y - CIRCLE_RADIUS, CIRCLE_RADIUS * 2, CIRCLE_RADIUS * 2);
            g.DrawEllipse(CirclePen, node.position.X - CIRCLE_RADIUS, node.position.Y - CIRCLE_RADIUS, CIRCLE_RADIUS * 2, CIRCLE_RADIUS * 2);
            SizeF size = g.MeasureString(node.expression.ToString(), TextFont);
            g.DrawString(node.expression.ToString(), TextFont, TextBrush, node.position.X - size.Width / 2, node.position.Y - (int)size.Height / 2);
        }

        private void Display_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.TranslateTransform(DisplayPanel.AutoScrollPosition.X, DisplayPanel.AutoScrollPosition.Y);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            if (layoutRoot != null)
            {
                drawNode(layoutRoot, g);
            }
        }

        private void SelectBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectBox.SelectedIndices.Count > 0)
            {
                active = SelectBox.SelectedIndices[0];
            }
            else
            {
                active = -1;
            }
            layout();
        }
    }
}
