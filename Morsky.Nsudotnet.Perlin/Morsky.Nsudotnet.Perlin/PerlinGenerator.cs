using System;
using System.Drawing;

namespace Morsky.Nsudotnet.Perlin
{
    class PerlinGenerator
    {
        public static Bitmap getImage(int size, int deep)
        {
            Bitmap image = new Bitmap(size, size);
            Grid gridR = new Grid(deep, new Tuple<int, int>(size, size));
            Grid gridG = new Grid(deep, new Tuple<int, int>(size, size));
            Grid gridB = new Grid(deep, new Tuple<int, int>(size, size));
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    image.SetPixel(x, y, Color.FromArgb(255, gridR.GetValue(new Point2D(x, y)), 
                        gridG.GetValue(new Point2D(x, y)), gridB.GetValue(new Point2D(x, y))));
                }
            }
            return image;
        }
    }
 
}
