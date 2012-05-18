using System;
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

                    region.Bitmap = renderer.Render(region.Expression);
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


        void logReplace(Expression oldExp, Expression newExp, string title)
        {
            if (ReferenceEquals(oldExp, topExpression))
            {
                topExpression = newExp;
                expressionReplacements.Clear();
                expressionReplacementsRev.Clear();
                if (topExpression != lastExpression)
                {
                    treeViewer.AddExpression(topExpression, title);
                    if (showIntermediates.Checked)
                    {
                        queueExpression(topExpression, DisplayRegion.LeftRight.Right);
                    }
                    lastExpression = topExpression;
                }
            }
            else
            {
                if (expressionReplacements.ContainsKey(oldExp))
                {
                    expressionReplacements[oldExp] = newExp;
                }
                else if (expressionReplacementsRev.ContainsKey(oldExp))
                {
                    Expression orig = expressionReplacementsRev[oldExp];
                    expressionReplacements[orig] = newExp;
                    expressionReplacementsRev.Remove(oldExp);
                }
                else
                {
                    expressionReplacements.Add(oldExp, newExp);
                }

                if (expressionReplacementsRev.ContainsKey(newExp))
                {
                    expressionReplacementsRev[newExp] = oldExp;
                }
                else
                {
                    expressionReplacementsRev.Add(newExp, oldExp);
                }

                Expression exp = constructExpression(topExpression);
                if (exp != lastExpression)
                {
                    treeViewer.AddExpression(exp, title);
                    if (showIntermediates.Checked)
                    {
                        queueExpression(exp, DisplayRegion.LeftRight.Right);
                    }
                    lastExpression = exp;
                }
            }
        }

        Expression constructExpression(Expression expression)
        {
            if (expressionReplacements.ContainsKey(expression))
            {
                return expressionReplacements[expression];
            }

            List<Expression> children = new List<Expression>();
            if (expression.Children != null)
            {
                foreach (Expression child in expression.Children)
                {
                    children.Add(constructExpression(child));
                }
            }

            return new Expression(expression.ExpressionType, expression.Data, children.ToArray());
        }

        void runCommand(string command)
        {
            Tokenizer tokenizer = new Tokenizer(command);
            Parser parser = new Parser(tokenizer);
            try
            {
                Expression expression = parser.Parse();
                treeViewer.ClearExpressions();

                topExpression = expression;
                lastExpression = expression;
                treeViewer.AddExpression(expression, "Start");
                queueExpression(expression, DisplayRegion.LeftRight.Left);

                Expression result = Evaluate.Eval(expression, logReplace);
                if (!showIntermediates.Checked)
                {
                    queueExpression(result, DisplayRegion.LeftRight.Right);
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

        void queueExpression(Expression expression, DisplayRegion.LeftRight leftRight)
        {
            DisplayRegion region = new DisplayRegion(0, expression, leftRight);
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
            public Expression Expression;
            public enum LeftRight
            {
                Left,
                Right
            };

            public LeftRight Side;

            public DisplayRegion(int Top, Expression Expression, LeftRight Side)
            {
                this.Top = Top;
                this.Expression = Expression;
                this.Side = Side;
                this.Bitmap = null;
            }

            public DisplayRegion(int Top, Bitmap Bitmap, LeftRight Side)
            {
                this.Top = Top;
                this.Expression = null;
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

        Expression topExpression = null;
        Expression lastExpression = null;
        class Comparer : IEqualityComparer<Expression>
        {
            public bool Equals(Expression a, Expression b)
            {
                return ReferenceEquals(a, b);
            }

            public int GetHashCode(Expression a)
            {
                return a.GetHashCode();
            }
        }

        Dictionary<Expression, Expression> expressionReplacements = new Dictionary<Expression, Expression>(new Comparer());
        Dictionary<Expression, Expression> expressionReplacementsRev = new Dictionary<Expression, Expression>(new Comparer());

        private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            exitRenderThread = true;
            renderEvent.Set();
            renderThread.Join();
        }
    }
}
