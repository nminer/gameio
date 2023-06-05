using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server.mapObjects
{
    internal class Circle
    {
        public Point Center;

        public Double Radius;

        public Circle(Point center, double radius)
        {
            Center = center;
            Radius = radius;
        }
    }
}
