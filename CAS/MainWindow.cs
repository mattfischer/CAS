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
        private delegate void FinishRender();
        private const int BORDER = 5;

        public MainWindow()
        {
            InitializeComponent();
            ActiveControl = CommandBox;
        }

        private void finishRender()
        {
            renderThread.Join();
            renderThread = null;

            DisplayRegion newRegion = Regions[Regions.Count - 1];

            Size size = OutputWindow.AutoScrollMinSize;
            size.Height = newRegion.Top + newRegion.Bitmap.Height + BORDER;
            OutputWindow.AutoScrollMinSize = size;
            OutputWindow.Invalidate();
            OutputWindow.AutoScrollPosition = new Point(0, size.Height - OutputWindow.Size.Height);
        }

        private void renderCommand(object command)
        {
            int nextTop = BORDER;
            if (Regions.Count > 0)
            {
                DisplayRegion lastRegion = Regions[Regions.Count - 1];
                nextTop = lastRegion.Top + lastRegion.Bitmap.Height + 2 * BORDER;
            }

            Bitmap bitmap = Renderer.Render((string)command);
            DisplayRegion newRegion = new DisplayRegion(nextTop, bitmap, DisplayRegion.LeftRight.Left);
            Regions.Add(newRegion);

            BeginInvoke(new FinishRender(this.finishRender));
        }

        private void CommandBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                if (renderThread != null)
                {
                    renderThread.Join();
                }
                renderThread = new Thread(this.renderCommand);
                renderThread.Start(CommandBox.Text);
                CommandBox.Text = "";
                e.SuppressKeyPress = true;
            }
        }

        private void OutputWindow_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.TranslateTransform(OutputWindow.AutoScrollPosition.X, OutputWindow.AutoScrollPosition.Y);
            foreach (DisplayRegion region in Regions)
            {
                int x = 0;
                switch (region.Side)
                {
                    case DisplayRegion.LeftRight.Left:
                        x = BORDER;
                        break;
                    case DisplayRegion.LeftRight.Right:
                        x = OutputWindow.Size.Width - System.Windows.Forms.SystemInformation.VerticalScrollBarWidth - region.Bitmap.Width - BORDER;
                        break;
                }
                e.Graphics.DrawImage(region.Bitmap, new Point(x, region.Top));
            }
        }

        private void OutputWindow_SizeChanged(object sender, EventArgs e)
        {
            OutputWindow.AutoScrollPosition = new Point(0, OutputWindow.AutoScrollMinSize.Height - OutputWindow.Size.Height);
            OutputWindow.Invalidate();
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
        TeXRenderer Renderer = new TeXRenderer();
        Thread renderThread = null;
    }
}
