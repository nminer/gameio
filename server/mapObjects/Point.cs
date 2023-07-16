using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server.mapObjects
{
    internal class Point
    {
        /// <summary>
        /// used to keep x and y thread safe.
        /// </summary>
        private object fieldLock = new object();

        private double _x;
        public double X
        {
            get
            {
                lock (fieldLock)
                {
                    return _x;
                }
            }
            set
            {
                lock (fieldLock)
                {
                    _x = value;
                }
            }
        }
        private double _y;
        public double Y
        {
            get
            {
                lock(fieldLock)
                {
                    return _y;
                }
            }
            set
            {
                lock(fieldLock)
                {
                    _y = value;
                }
            }
        }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static double Distance(Point p1, Point p2)
        {
            double distance = Math.Sqrt(Math.Pow((p2.X - p1.X), 2) + Math.Pow((p2.Y - p1.Y), 2));
            return distance;
        }
    }
}
