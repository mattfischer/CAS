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
        private delegate void AddBitmapDelegate(Bitmap bitmap);
        private const int BORDER = 5;

        public MainWindow()
        {
            InitializeComponent();
            ActiveControl = CommandBox;
            History.Add("");
        }

        private void addBitmap(Bitmap bitmap)
        {
            if (renderThread != null)
            {
                renderThread.Join();
                renderThread = null;
            }

            int nextTop = BORDER;
            if (Regions.Count > 0)
            {
                DisplayRegion lastRegion = Regions[Regions.Count - 1];
                nextTop = lastRegion.Top + lastRegion.Bitmap.Height + 2 * BORDER;
            }

            DisplayRegion newRegion = new DisplayRegion(nextTop, bitmap, DisplayRegion.LeftRight.Left);
            Regions.Add(newRegion);

            Size size = OutputPanel.AutoScrollMinSize;
            size.Height = newRegion.Top + newRegion.Bitmap.Height + BORDER;
            OutputPanel.AutoScrollMinSize = size;
            OutputPanel.Invalidate();
            OutputPanel.AutoScrollPosition = new Point(0, size.Height - OutputPanel.Size.Height);
        }

        private void renderCommand(object expression)
        {
            Bitmap bitmap = Renderer.Render((Expression)expression);

            object[] args = { bitmap };
            BeginInvoke(new AddBitmapDelegate(this.addBitmap), args);
        }

        private void CommandBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
            {
                int line = CommandBox.GetLineFromCharIndex(CommandBox.SelectionStart);
                if (line == 0 && HistoryIndex > 0)
                {
                    History[HistoryIndex] = CommandBox.Text;
                    HistoryIndex--;
                    CommandBox.Text = History[HistoryIndex];
                    CommandBox.SelectionStart = History[HistoryIndex].Length;
                    CommandBox.SelectionLength = 0;
                    e.SuppressKeyPress = true;
                }
            }

            if (e.KeyCode == Keys.Down)
            {
                int line = CommandBox.GetLineFromCharIndex(CommandBox.SelectionStart);
                if (line == CommandBox.Lines.Length - 1 && HistoryIndex < History.Count - 1)
                {
                    History[HistoryIndex] = CommandBox.Text;
                    HistoryIndex++;
                    CommandBox.Text = History[HistoryIndex];
                    CommandBox.SelectionStart = History[HistoryIndex].Length;
                    CommandBox.SelectionLength = 0;
                    e.SuppressKeyPress = true;
                }
            }

            if (e.KeyCode == Keys.Return)
            {
                string command = CommandBox.Text;
                CommandBox.Text = "";
                e.SuppressKeyPress = true;

                History[History.Count - 1] = command;
                History.Add("");
                HistoryIndex = History.Count - 1;

                Tokenizer tokenizer = new Tokenizer(command);
                Parser parser = new Parser(tokenizer);
                try
                {
                    Expression expression = parser.Parse();
                    treeViewer.AddExpression(expression, command);
                    treeViewer.Show();
                    treeViewer.BringToFront();

                    if (renderThread != null)
                    {
                        renderThread.Join();
                    }
                    renderThread = new Thread(this.renderCommand);
                    renderThread.Start(expression);
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

                    addBitmap(bitmap);
                }
            }
        }

        private void OutputWindow_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.TranslateTransform(OutputPanel.AutoScrollPosition.X, OutputPanel.AutoScrollPosition.Y);
            foreach (DisplayRegion region in Regions)
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

        private void OutputWindow_SizeChanged(object sender, EventArgs e)
        {
            OutputPanel.AutoScrollPosition = new Point(0, OutputPanel.AutoScrollMinSize.Height - OutputPanel.Size.Height);
            OutputPanel.Invalidate();
        }

        struct DisplayRegion
        {
            public int Top;
            public Bitmap Bitmap;
            public enum LeftRight
            {
                Left,
                Right
            };

            public LeftRight Side;

            public DisplayRegion(int Top, Bitmap Bitmap, LeftRight Side)
            {
                this.Top = Top;
                this.Bitmap = Bitmap;
                this.Side = Side;
            }
        };

        List<DisplayRegion> Regions = new List<DisplayRegion>();

        List<string> History = new List<string>();
        int HistoryIndex = 0;

        TeXRenderer Renderer = new TeXRenderer();
        Thread renderThread = null;
        TreeViewer treeViewer = new TreeViewer();

        private void OutputWindow_MouseClick(object sender, MouseEventArgs e)
        {
            treeViewer.Show();
            treeViewer.BringToFront();
        }
    }
}
