namespace Browser.Selector.Cli.UI
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Linq;
    using System.Windows.Forms;
    using static System.Net.Mime.MediaTypeNames;

    public class PieButton : Button
    {
        private readonly int InnerDiameter = 30;
        private readonly int OuterDiameter = 100;

        private GraphicUtils GraphicUtils = new GraphicUtils();

        private int TotalPartCount;

        private Color BackgroundColor = Color.FromArgb(20, Color.DarkGray);
        private Color BackgroundColorHighlight = Color.FromArgb(10, Color.DarkGray);

        public new string Text;

        public PieButton(int position, int totalParts, System.Drawing.Image image)
        {
            Image = image;
            TotalPartCount = totalParts;
            Region = CreateRegion(position);

            BackColor = BackgroundColor;
            ForeColor = Color.LightGray;

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;

            Width = Height = OuterDiameter * 2;
            AutoSize = false;
        }

        private Region CreateRegion(int position) => GraphicUtils.CreateRegion(position, TotalPartCount, OuterDiameter, InnerDiameter);
        private Point GetImageDrawPoint(int position) { 
            Point point = GraphicUtils.GetAnglePointPosition(
                position,
                TotalPartCount,
                new Point(OuterDiameter, OuterDiameter),
                (OuterDiameter - InnerDiameter) / 2 + InnerDiameter
            );

            point.Offset(-Image.Width/2, -Image.Height/2);
            return point;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // well, weird, but better to paint the logo to each segment (even those not within the region) than guessing where probably that should be
            foreach (var i in Enumerable.Range(1, TotalPartCount))
            {
                e.Graphics.DrawImage(Image, GetImageDrawPoint(i));
            }

            e.Graphics.TranslateTransform(10, 10);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            BackColor = BackgroundColor;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            BackColor = BackgroundColorHighlight;
        }
    }
}
