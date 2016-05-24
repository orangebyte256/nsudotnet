using System;
using System.Collections.Generic;

namespace Morsky.Nsudotnet.Perlin
{
    class Point2D
    {
        private int x;
        private int y;
        public int X
        {
            get { return x; }
            set { x = value; }
        }
        public int Y
        {
            get { return y; }
            set { y = value; }
        }

        public Point2D(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static Point2D operator+(Point2D first, Point2D second)
        {
            Point2D result = new Point2D(first.X, first.Y);
            result.X += second.X;
            result.Y += second.Y;
            return result;
        }

        public static Point2D operator/(Point2D first, int value)
        {
            Point2D result = new Point2D(first.X, first.Y);
            result.X /= value;
            result.Y /= value;
            return result;
        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            Point2D p = obj as Point2D;
            if (p == null)
            {
                return false;
            }

            return (x == p.x) && (y == p.y);
        }

        public bool Equals(Point2D p)
        {
            // If parameter is null return false:
            if (p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (x == p.x) && (y == p.y);
        }

        public override int GetHashCode()
        {
            return x ^ y;
        }
    }


    class Grid
    {
        List<Dictionary<Point2D, int>> levelValues = new List<Dictionary<Point2D, int>>();
        int deep;
        Tuple<int, int> size = new Tuple<int, int>(0, 0);
        public Grid(int deep, Tuple<int, int> size)
        {
            this.deep = deep;
            this.size = size;
            Random random = new Random();
            for (int i = 0; i < deep; i++)
            {
                Dictionary<Point2D, int> dict = new Dictionary<Point2D, int>();
                int tmpSize = (int)Math.Pow(2, i) + 1;
                for(int y = 0; y < tmpSize; y++)
                {
                    for(int x = 0; x < tmpSize; x++)
                    {
                        dict.Add(new Point2D(x, y), random.Next(0,255));
                    }
                }
                levelValues.Add(dict);
            }
        }

        Tuple<Point2D, Tuple<double, double>> GetLocalCoord(Point2D point, int level)
        {
            double tmpSize = (int)Math.Pow(2, level);
            double tmpSizeX = size.Item1 / tmpSize;
            double tmpSizeY = size.Item2 / tmpSize;
            Point2D integerPart = new Point2D((int)(point.X / tmpSizeX), (int)(point.Y / tmpSizeY));
            Tuple<double, double> restPart = new Tuple<double, double>((point.X - integerPart.X * tmpSizeX) / (double)tmpSizeX, (point.Y - integerPart.Y * tmpSizeY) / (double)tmpSizeY);
            return new Tuple<Point2D, Tuple<double, double>>(integerPart, restPart);
        }

        double SmoothNoize(Point2D point, int level)
        {
            double result = 0;
            int[] countForDiv = new int[3];
            int maxLength = (int)Math.Round(Math.Pow(2.0, level));
            for (int y = -1; y < 2; y++)
            {
                for (int x = -1; x < 2; x++)
                {
                    if ((point.X + x) >= 0 && (point.X + x) <= maxLength && (point.Y + y) >= 0 && (point.Y + y) <= maxLength)
                    {
                        countForDiv[Math.Abs(y) + Math.Abs(x)]++;
                    }
                }
            }
            for (int y = -1; y < 2; y++)
            {
                for (int x = -1; x < 2; x++)
                {
                    int absval = Math.Abs(y) + Math.Abs(x);
                    int tmp;
                    levelValues[level].TryGetValue(new Point2D(point.X + x, point.Y + y), out tmp);
                    double cur = tmp;
                    switch (absval)
                    {
                        case 2:
                            cur /= Math.Max(1, countForDiv[2]) * 4.0;
                            break;
                        case 1:
                            cur /= Math.Max(1, countForDiv[1]) * 2.0;
                            break;
                        case 0:
                            cur /= 4.0;
                            break;
                    }
                    result += cur;
                }
            }
            return result;
        }

        int LinearInterpolate(Point2D point, int level)
        {
            Tuple<Point2D, Tuple<double, double>> coord = GetLocalCoord(point, level);

            double lt = SmoothNoize(new Point2D(coord.Item1.X, coord.Item1.Y), level);
            double rt = SmoothNoize(new Point2D(coord.Item1.X + 1, coord.Item1.Y), level);
            double ld = SmoothNoize(new Point2D(coord.Item1.X, coord.Item1.Y + 1), level);
            double rd = SmoothNoize(new Point2D(coord.Item1.X + 1, coord.Item1.Y + 1), level);

            double mt = lt * (1 - coord.Item2.Item1) + rt * coord.Item2.Item1;
            double md = ld * (1 - coord.Item2.Item1) + rd * coord.Item2.Item1;

            return (int)(mt * (1 - coord.Item2.Item2) + md * coord.Item2.Item2);
        }

        public int GetValue(Point2D point)
        {
            int result = 0;
            for (int i = 0; i < deep; i++)
            {
                result += LinearInterpolate(point, i);
            }
            return Math.Min(255, (int)(result / (double)deep));
        }
    }
}
