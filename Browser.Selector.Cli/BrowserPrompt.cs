namespace Browser.Selector.Cli
{
    using Browser.Selector.Cli.UI;
    using Browser.Selector.Lib;
    using Browser.Selector.Lib.Interfaces;
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;

    public class BrowserPrompt
    {
        public string Url { get; set; }
        public string ProcessName { get; set; }

        private IBrowser TargetBrowser { get; set; }

        private GraphicUtils GraphicUtils { get; } = new GraphicUtils();

        private readonly Form Prompt = new DismissibleForm()
        {
            Width = 200,
            Height = 200,
            AutoSize = false,
            StartPosition = FormStartPosition.Manual,
            BackColor = Color.Black,
            TransparencyKey = Color.Black,
            ControlBox = false,
            FormBorderStyle = FormBorderStyle.None
        };

        private readonly ToolTip ToolTip = new ToolTip()
        {
            AutoPopDelay = 5000,
            InitialDelay = 500,
            ReshowDelay = 250,
            ShowAlways = true,
            BackColor = Color.Black,
            ForeColor = Color.White
        };

        public BrowserPrompt() : base()
        {}

        private Icon BuildIcon(BrowserList browsers)
        {
            int size = 50;
            Bitmap bitmap = new Bitmap(size, size);
            using (Graphics graphics = Graphics.FromImage(bitmap)) {
                var i = 0;
                foreach (IBrowser browser in browsers)
                {
                    i++;
                    Bitmap browserIco = Icon.ExtractAssociatedIcon(browser.ExecutablePath).ToBitmap();
                    Point location = GraphicUtils.GetAnglePointPosition(i, browsers.Count(), new Point(size / 2, size / 2), size / 5);
                    location.Offset(-browserIco.Width/2, -browserIco.Height/2);
                    graphics.DrawImage(browserIco, location);
                }
            }

            return Icon.FromHandle(bitmap.GetHicon());
        }

        public IBrowser Show(BrowserList browserList)
        {
            Prompt.Text = $"{ProcessName} ⇢ {Url}";
            Prompt.Icon = BuildIcon(browserList);

            Prompt.SetDesktopLocation(Cursor.Position.X - 100, Cursor.Position.Y - 100);
            
            Prompt.KeyPress += (object sender, KeyPressEventArgs e) => {
                if (e.KeyChar == (char)27)
                {
                    Prompt.Close();
                }
            };

            int i = 0;
            foreach (IBrowser browser in browserList)
            {
                ++i;
                Program.Console.WriteLine(browser.ExecutablePath);
                var button = new PieButton(i, browserList.Count(), Icon.ExtractAssociatedIcon(browser.ExecutablePath).ToBitmap()) { Text = browser.Name, DialogResult = DialogResult.Yes };
                button.Click += (sender, e) => {
                    TargetBrowser = browser;
                    Prompt.Close();
                };
                ToolTip.SetToolTip(button, $"{browser.Name}\n{ProcessName}\n{Url}");
                Prompt.Controls.Add(button);
            }

            var result = Prompt.ShowDialog();

            if (result != DialogResult.Yes)
            {
                throw new Exception("Nothing selected");
            }

            return TargetBrowser;
        }
    }

    public class GraphicUtils
    {
        public Region CreateRegion(int partNumber, int totalPartCount, int outerDiameter, int innerDiameter)
        {
            float angle = 360 / totalPartCount;
            float startingAngle = (partNumber - 1) * angle;

            var wantedshape = new GraphicsPath();
            var rectangle = new Rectangle(0, 0, outerDiameter * 2, outerDiameter * 2);
            wantedshape.AddPie(rectangle, startingAngle, angle);

            var innerShape = new GraphicsPath();
            var innerBoundary = new Rectangle(outerDiameter - innerDiameter, outerDiameter - innerDiameter, innerDiameter * 2, innerDiameter * 2);
            innerShape.AddPie(innerBoundary, startingAngle, angle);
            var innerRegion = new Region(innerShape);

            var region = new Region(wantedshape);
            region.Exclude(innerRegion);

            return region;
        }

        public Point GetAnglePointPosition(int partNumber, int totalPartCount, Point centralPoint, int distanceFromCenter)
        {
            double angle = 360 / totalPartCount;
            double startingAngle = (partNumber - 0.5) * angle;
            double startingAngleRad = Math.PI * startingAngle / 180.0;

            double x = centralPoint.X + distanceFromCenter * Math.Cos(startingAngleRad);
            double y = centralPoint.Y + distanceFromCenter * Math.Sin(startingAngleRad);

            return new Point((int)x, (int)y);
        }
    }
}
