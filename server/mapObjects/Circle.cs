using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server.mapObjects
{
    public class Circle
    {
        public Point Center;

        public Double Radius;

        public Circle(Point center, double radius)
        {
            Center = center;
            Radius = radius;
        }

        public static double Distance(Circle c1, Circle c2)
        {
            return Point.Distance(c1.Center, c2.Center) - c1.Radius - c2.Radius;
        }

        public double Distance(Circle circleToCheck)
        {
            return Distance(circleToCheck, this);
        }

        public bool DoesInterceptCircle(Circle circleToCheck)
        {
            return Distance(circleToCheck) < 0;
        }
    }
}