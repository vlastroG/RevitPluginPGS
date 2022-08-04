using System.Drawing;

int width = 360;
int height = 86;
int zero = 0;
float dpi = 144;

using (Bitmap b = new Bitmap(width, height))
{

    b.SetResolution(dpi, dpi);
    using (Graphics g = Graphics.FromImage(b))
    {
        Brush brush = new SolidBrush(Color.FromArgb(170, 100, 105));
        g.FillRectangle(brush, new Rectangle(zero, zero, width, height));
    }
    b.Save(@"green.png", System.Drawing.Imaging.ImageFormat.Png);
}

