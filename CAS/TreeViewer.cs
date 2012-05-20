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
            public Node node;
            public List<LayoutNode> children;
            public Point position;
            public LayoutNode(Node node, List<LayoutNode> children, Point position)
            {
                this.node = node;
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

        LayoutNode layout(Node node, List<LayoutNode> rightmost, int depth)
        {
            List<LayoutNode> children = null;
            int x, y;

            x = BORDER;
            y = BORDER + depth * VERTICAL_SPACE;
            if (node.Children != null)
            {
                if (rightmost.Count == depth + 1)
                {
                    rightmost.Add(null);
                }

                children = new List<LayoutNode>();
                foreach (Node child in node.Children)
                {
                    LayoutNode childNode = layout(child, rightmost, depth + 1);
                    children.Add(childNode);
                }

                x = (children.First().position.X + children.Last().position.X) / 2;
            }

            LayoutNode layoutNode = new LayoutNode(node, children, new Point(x, y));

            if (rightmost[depth] != null)
            {
                int nextPosition = rightmost[depth].position.X + HORIZONTAL_SPACE;
                if (nextPosition > layoutNode.position.X)
                {
                    moveNode(layoutNode, nextPosition - layoutNode.position.X);
                }
            }

            rightmost[depth] = layoutNode;

            return layoutNode;
        }

        void doLayout()
        {
            int width, height;
            width = 0;
            height = 0;
            if (active != -1)
            {
                List<LayoutNode> rightmost = new List<LayoutNode>();
                rightmost.Add(null);
                layoutRoot = layout(expressions[active], rightmost, 0);


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

        List<Node> expressions = new List<Node>();
        int active = 0;
        LayoutNode layoutRoot = null;
        public void ClearNodes()
        {
            expressions.Clear();
            SelectBox.Items.Clear();
            active = -1;
            Invalidate();
        }

        public void AddNode(Node expression, string title)
        {
            expressions.Add(expression);
            SelectBox.Items.Add(title);
            SelectBox.SelectedIndices.Clear();
            SelectBox.SelectedIndices.Add(expressions.Count - 1);
        }

        void drawNode(LayoutNode layoutNode, Graphics g)
        {
            if (layoutNode.children != null)
            {
                foreach (LayoutNode child in layoutNode.children)
                {
                    g.DrawLine(EdgePen, layoutNode.position, child.position);
                    drawNode(child, g);
                }
            }

            g.FillEllipse(CircleBrush, layoutNode.position.X - CIRCLE_RADIUS, layoutNode.position.Y - CIRCLE_RADIUS, CIRCLE_RADIUS * 2, CIRCLE_RADIUS * 2);
            g.DrawEllipse(CirclePen, layoutNode.position.X - CIRCLE_RADIUS, layoutNode.position.Y - CIRCLE_RADIUS, CIRCLE_RADIUS * 2, CIRCLE_RADIUS * 2);
            SizeF size = g.MeasureString(layoutNode.node.ToString(), TextFont);
            g.DrawString(layoutNode.node.ToString(), TextFont, TextBrush, layoutNode.position.X - size.Width / 2, layoutNode.position.Y - (int)size.Height / 2);
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
            doLayout();
        }
    }
}
