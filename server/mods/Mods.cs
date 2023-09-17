using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

static class Mods
{

    private static Random rnd = new Random();

    /// <summary>
    /// takes in:
    /// old range min and max and value
    /// new range min and max
    /// gives back the new value in the new range while keeping the ratio.
    /// </summary>
    /// <param name="oldMin"></param>
    /// <param name="oldMax"></param>
    /// <param name="newMin"></param>
    /// <param name="newMax"></param>
    /// <param name="oldValue"></param>
    /// <returns></returns>
    public static double ConvertRange(double oldMin, double oldMax, double newMin, double newMax, double oldValue)
    {
        double oldRange = oldMax - oldMin;
        double newRange = newMax - newMin;
        double newValue = (((oldValue - oldMin) * newRange) / oldRange) + newMin;
        return newValue;
    }

    /// <summary>
    /// pass in percent amount as a double 0.0-1.0
    /// returns true percentTrue of the time.
    /// </summary>
    /// <param name="precentTrue"></param>
    /// <returns></returns>
    public static bool Chance(double percentTrue)
    {
        double testCheck = rnd.NextDouble();
        return percentTrue >= testCheck;
    }

    /// <summary>
    /// return a random int between min and max inclusive.
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static int IntBetween(int min, int max)
    {
        return rnd.Next(min, max + 1);
    }

    /// <summary>
    /// return a random double between min and max inclusive.
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static double DoubleBetween(double min, double max)
    {
        return rnd.NextDouble() * (max - min) + min;
    }
}