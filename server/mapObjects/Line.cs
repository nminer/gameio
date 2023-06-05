using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server.mapObjects
{

    class Line : ISolid
    {
        private Point point1;
        private Point point2;

        public double X1 
        { get { return point1.X; } }

        public double Y1
        { get { return point1.Y; } }

        public double X2
        { get { return point2.X; } }

        public double Y2
        { get { return point2.Y; } }

        public Point Point1 { get { return point1; } }

        public Point Point2 { get { return point2; } }

        public Line(double x1, double y1, double x2, double y2)
        {
            point1 = new Point(x1, y1);
            point2 = new Point(x2, y2);
        }

        public Line(Point firstPoint, Point secondPoint) {
            point1 = firstPoint;
            point2 = secondPoint;
        }

        public Line[] Lines()
        {
            return new Line[] { this };
        }
    }
}
