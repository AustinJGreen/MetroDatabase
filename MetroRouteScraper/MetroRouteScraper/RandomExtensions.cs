using System;

namespace MetroRouteScraper
{
    public static class RandomExtensions
    {
        public static double NextNormal(this Random rng, double mean, double stdDev)
        {
            double u1 = rng.NextDouble();
            double u2 = rng.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                         Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            double randNormal = mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
            return randNormal;
        }
    }
}
