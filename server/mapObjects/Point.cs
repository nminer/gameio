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

        public Point():this(0,0)
        {
        }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// our directions is 0 when going down 
        /// this is due to top left being 0,0 and y going up as we move down and x going up as we move right.
        /// the 0 direction is set by vector1.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public static double Direction(Point source, Point destination)
        {
            var vector2 = destination - source;
            var vector1 = new Point(0, 1); // 12 o'clock == 0°, assuming that y goes from bottom to top
            double angleInRadians = Math.Atan2(vector2.Y, vector2.X) - Math.Atan2(vector1.Y, vector1.X);
            double degrees = (180 / Math.PI) * angleInRadians;
            if (degrees < 0)
            {
                degrees = 360 + degrees;
            }
            return degrees;
        }

        public double Direction(Point Destination)
        {
            return Point.Direction(this, Destination);
        }

        public static double Distance(Point p1, Point p2)
        {
            double distance = Math.Sqrt(Math.Pow((p2.X - p1.X), 2) + Math.Pow((p2.Y - p1.Y), 2));
            return distance;
        }

        public double Distance(Point pointToCheck)
        {
            return Point.Distance(pointToCheck, this);
        }

        public override bool Equals(object obj) => this.Equals(obj as Point);

        public bool Equals(Point p)
        {
            if (p is null)
            {
                return false;
            }
            // Optimization for a common success case.
            if (Object.ReferenceEquals(this, p))
            {
                return true;
            }
            // If run-time types are not exactly the same, return false.
            if (this.GetType() != p.GetType())
            {
                return false;
            }
            // Return true if the fields match.
            // Note that the base class is not invoked because it is
            // System.Object, which defines Equals as reference equality.
            return (X == p.X) && (Y == p.Y);
        }

        public override int GetHashCode() => (X, Y).GetHashCode();

        public static bool operator ==(Point lhs, Point rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                {
                    return true;
                }
                // Only the left side is null.
                return false;
            }
            // Equals handles case of null on right side.
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Point lhs, Point rhs) => !(lhs == rhs);

        public static Point operator +(Point a, Point b)
        {
            Point newPoint = new Point(a.X + b.X, a.Y + b.Y);
            return newPoint;
        }

        public static Point operator -(Point a, Point b)
        {
            Point newPoint = new Point(a.X - b.X, a.Y - b.Y);
            return newPoint;
        }
    }
}
