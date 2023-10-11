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

        public static Point FindPointOnCur(Circle circle, double degrees)
        {
            double x_oncircle = circle.Center.X + circle.Radius * Math.Cos(degrees * Math.PI / 180);
            double y_oncircle = circle.Center.Y + circle.Radius * Math.Sin(degrees * Math.PI / 180);
            return new Point(x_oncircle, y_oncircle);
        }
    }
}