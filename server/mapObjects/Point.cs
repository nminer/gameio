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

        public static double Distance(Point p1, Point p2)
        {
            double distance = Math.Sqrt(Math.Pow((p2.X - p1.X), 2) + Math.Pow((p2.Y - p1.Y), 2));
            return distance;
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
    }
}
