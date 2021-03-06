﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace CAS
{
    public partial class MainWindow : Form
    {
        delegate void UpdateOutputDelegate();

        const int BORDER = 5;

        public MainWindow()
        {
            InitializeComponent();
            ActiveControl = CommandBox;

            history.Add("");
            renderThread = new Thread(renderMain);
            renderThread.Start();
        }

        void renderMain()
        {
            while (true)
            {
                renderEvent.WaitOne();

                if (exitRenderThread)
                {
                    break;
                }

                while(renderQueue.Count > 0)
                {
                    DisplayRegion region = renderQueue.First();
                    renderQueue.RemoveAt(0);

                    region.Bitmap = renderer.Render(region.Node);
                    if (regions.Count > 0)
                    {
                        region.Top = regions.Last().Top + regions.Last().Bitmap.Size.Height + 2 * BORDER;
                    }
                    else
                    {
                        region.Top = BORDER;
                    }
                    regions.Add(region);
                    BeginInvoke(new UpdateOutputDelegate(updateOutput));
                }
            }
        }

        void updateOutput()
        {
            DisplayRegion region = regions.Last();
            Size size = OutputPanel.AutoScrollMinSize;
            size.Height = region.Top + region.Bitmap.Height + BORDER;
            OutputPanel.AutoScrollMinSize = size;
            OutputPanel.Invalidate();
            OutputPanel.AutoScrollPosition = new Point(0, size.Height - OutputPanel.Size.Height);
        }


        void logReplace(Node oldNode, Node newNode, string title)
        {
            if (ReferenceEquals(oldNode, topNode))
            {
                topNode = newNode;
                nodeReplacements.Clear();
                nodeReplacementsRev.Clear();
                if (topNode != lastNode)
                {
                    treeViewer.AddNode(topNode, title);
                    if (showIntermediates.Checked)
                    {
                        queueNode(topNode, DisplayRegion.LeftRight.Right);
                    }
                    lastNode = topNode;
                }
            }
            else
            {
                if (nodeReplacements.ContainsKey(oldNode))
                {
                    nodeReplacements[oldNode] = newNode;
                }
                else if (nodeReplacementsRev.ContainsKey(oldNode))
                {
                    Node orig = nodeReplacementsRev[oldNode];
                    nodeReplacements[orig] = newNode;
                    nodeReplacementsRev.Remove(oldNode);
                }
                else
                {
                    nodeReplacements.Add(oldNode, newNode);
                }

                if (nodeReplacementsRev.ContainsKey(newNode))
                {
                    nodeReplacementsRev[newNode] = oldNode;
                }
                else
                {
                    nodeReplacementsRev.Add(newNode, oldNode);
                }

                Node exp = constructNode(topNode);
                if (exp != lastNode)
                {
                    treeViewer.AddNode(exp, title);
                    if (showIntermediates.Checked)
                    {
                        queueNode(exp, DisplayRegion.LeftRight.Right);
                    }
                    lastNode = exp;
                }
            }
        }

        Node constructNode(Node node)
        {
            if (nodeReplacements.ContainsKey(node))
            {
                return nodeReplacements[node];
            }

            List<Node> children = new List<Node>();
            if (node.Children != null)
            {
                foreach (Node child in node.Children)
                {
                    children.Add(constructNode(child));
                }
            }

            return new Node(node.NodeType, node.Data, children.ToArray());
        }

        void runCommand(string command)
        {
            Tokenizer tokenizer = new Tokenizer(command);
            Parser parser = new Parser(tokenizer);
            try
            {
                Node node = parser.Parse();
                treeViewer.ClearNodes();

                topNode = node;
                lastNode = node;
                treeViewer.AddNode(node, "Start");
                queueNode(node, DisplayRegion.LeftRight.Left);

                Node result = Simplify.Eval(node, logReplace);
                if (!showIntermediates.Checked)
                {
                    queueNode(result, DisplayRegion.LeftRight.Right);
                }
            }
            catch (Parser.ParseException ex)
            {
                Bitmap bitmap = new Bitmap(300, 50);
                Graphics g = Graphics.FromImage(bitmap);
                g.Clear(Color.White);
                Font font = new Font(FontFamily.GenericSansSerif, 10);
                Brush blackBrush = new SolidBrush(Color.Black);
                Brush redBrush = new SolidBrush(Color.Red);
                Size size = TextRenderer.MeasureText(command, font);
                g.DrawString(command, font, blackBrush, new Point(0, 0));
                g.DrawString("Error, column " + ex.Position + ": " + ex.Message, font, redBrush, new Point(0, size.Height));

                DisplayRegion region = new DisplayRegion(0, bitmap, DisplayRegion.LeftRight.Left);
                regions.Add(region);
                updateOutput();
            }
        }

        void queueNode(Node node, DisplayRegion.LeftRight leftRight)
        {
            DisplayRegion region = new DisplayRegion(0, node, leftRight);
            renderQueue.Add(region);
            renderEvent.Set();
        }

        void CommandBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
            {
                int line = CommandBox.GetLineFromCharIndex(CommandBox.SelectionStart);
                if (line == 0 && historyIndex > 0)
                {
                    history[historyIndex] = CommandBox.Text;
                    historyIndex--;
                    CommandBox.Text = history[historyIndex];
                    CommandBox.SelectionStart = history[historyIndex].Length;
                    CommandBox.SelectionLength = 0;
                    e.SuppressKeyPress = true;
                }
            }

            if (e.KeyCode == Keys.Down)
            {
                int line = CommandBox.GetLineFromCharIndex(CommandBox.SelectionStart);
                if (line == CommandBox.Lines.Length - 1 && historyIndex < history.Count - 1)
                {
                    history[historyIndex] = CommandBox.Text;
                    historyIndex++;
                    CommandBox.Text = history[historyIndex];
                    CommandBox.SelectionStart = history[historyIndex].Length;
                    CommandBox.SelectionLength = 0;
                    e.SuppressKeyPress = true;
                }
            }

            if (e.KeyCode == Keys.Return)
            {
                string command = CommandBox.Text;
                CommandBox.Text = "";
                e.SuppressKeyPress = true;

                history[history.Count - 1] = command;
                history.Add("");
                historyIndex = history.Count - 1;

                runCommand(command);
            }
        }

        void OutputWindow_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.TranslateTransform(OutputPanel.AutoScrollPosition.X, OutputPanel.AutoScrollPosition.Y);
            foreach (DisplayRegion region in regions)
            {
                int x = 0;
                switch (region.Side)
                {
                    case DisplayRegion.LeftRight.Left:
                        x = BORDER;
                        break;
                    case DisplayRegion.LeftRight.Right:
                        x = OutputPanel.Size.Width - System.Windows.Forms.SystemInformation.VerticalScrollBarWidth - region.Bitmap.Width - BORDER;
                        break;
                }
                e.Graphics.DrawImage(region.Bitmap, new Point(x, region.Top));
            }
        }

        void OutputWindow_SizeChanged(object sender, EventArgs e)
        {
            OutputPanel.AutoScrollPosition = new Point(0, OutputPanel.AutoScrollMinSize.Height - OutputPanel.Size.Height);
            OutputPanel.Invalidate();
        }

        void OutputWindow_MouseClick(object sender, MouseEventArgs e)
        {
            treeViewer.Show();
            treeViewer.BringToFront();
        }

        struct DisplayRegion
        {
            public int Top;
            public Bitmap Bitmap;
            public Node Node;
            public enum LeftRight
            {
                Left,
                Right
            };

            public LeftRight Side;

            public DisplayRegion(int Top, Node Node, LeftRight Side)
            {
                this.Top = Top;
                this.Node = Node;
                this.Side = Side;
                this.Bitmap = null;
            }

            public DisplayRegion(int Top, Bitmap Bitmap, LeftRight Side)
            {
                this.Top = Top;
                this.Node = null;
                this.Side = Side;
                this.Bitmap = Bitmap;
            }
        };

        List<DisplayRegion> regions = new List<DisplayRegion>();

        List<string> history = new List<string>();
        int historyIndex = 0;

        TeXRenderer renderer = new TeXRenderer();
        Thread renderThread = null;
        AutoResetEvent renderEvent = new AutoResetEvent(false);
        List<DisplayRegion> renderQueue = new List<DisplayRegion>();
        bool exitRenderThread = false;

        TreeViewer treeViewer = new TreeViewer();

        Node topNode = null;
        Node lastNode = null;
        class Comparer : IEqualityComparer<Node>
        {
            public bool Equals(Node a, Node b)
            {
                return ReferenceEquals(a, b);
            }

            public int GetHashCode(Node a)
            {
                return a.GetHashCode();
            }
        }

        Dictionary<Node, Node> nodeReplacements = new Dictionary<Node, Node>(new Comparer());
        Dictionary<Node, Node> nodeReplacementsRev = new Dictionary<Node, Node>(new Comparer());

        private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            exitRenderThread = true;
            renderEvent.Set();
            renderThread.Join();
        }
    }
}
