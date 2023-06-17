using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server.mods
{
    internal static class Mods
    {
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
    }
}
