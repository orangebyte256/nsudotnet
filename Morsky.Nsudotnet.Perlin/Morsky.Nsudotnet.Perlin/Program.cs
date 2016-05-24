using System;

namespace Morsky.Nsudotnet.Perlin
{
    class Program
    {
        static void Main(string[] args)
        {
            PerlinGenerator.getImage(Convert.ToInt32(args[0]), (int)Math.Log(Convert.ToInt32(args[0]), 2)).Save(args[1]);
        }
    }
}
