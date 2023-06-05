using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server.mapObjects
{
    class Solid : ISolid
    {
        /// <summary>
        /// used to keep points and lines list safe.
        /// </summary>
        private object threadLock = new object();

        /// <summary>
        /// all points are relitive to position.
        /// </summary>
        private Point position;

        /// <summary>
        /// gets set to true if lines have been built
        /// this is to keep us from building lines over and over again.
        /// </summary>
        private bool linesBuilt = false;

        /// <summary>
        // when set to true first and last point will be connected with a line.
        /// </summary>       
        public bool IsShape
        {
            get
            {
                return isShape;
            }
            set
            {
                isShape = value;
                linesBuilt = false;
            }
        }
        private bool isShape = true;

        /// <summary>
        /// hold the list of lines for this solid \.
        /// </summary>
        private List<Line> lines = new List<Line>();

        /// <summary>
        /// holds the list of points that make up this solid.
        /// </summary>
        private List<Point> points = new List<Point>();

        /// <summary>
        /// set the solids position on the map. 
        /// this is default 0 0 if left null.
        /// </summary>
        /// <param name="solidPosition"></param>
        public Solid(Point ?solidPosition = null)
        {
            lock(threadLock)
            {
                // set the default to 0, 0
                if (solidPosition == null)
                {
                    solidPosition = new Point(0, 0);
                }
                this.position = solidPosition;
            }
        }

        /// <summary>
        /// create a simple solid. 4 points.
        /// </summary>
        /// <param name="solidPosition"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        public Solid(Point solidPosition, double height, double width)
        {
            position = solidPosition;
            AddPoint(solidPosition);
            AddPoint(new Point(position.X + width, position.Y));
            AddPoint(new Point(position.X + width, position.Y + height));
            AddPoint(new Point(position.X, position.Y + height));
        }

        /// <summary>
        /// add a point to the solid.
        /// each pair of points in order create a line.
        /// </summary>
        /// <param name="point"></param>
        public Solid AddPoint(Point point)
        {
            lock (threadLock)
            {
                points.Add(point);
                linesBuilt = false;
            }
            return this;
        }

        /// <summary>
        /// returns a list of lines that make up the solid.
        /// </summary>
        /// <returns></returns>
        public Line[] Lines()
        {
            lock (threadLock)
            {
                // build the lines once for the solid.
                if (!linesBuilt)
                {
                    linesBuilt = true;
                    lines.Clear();
                    if (points.Count <= 1)
                    {
                        return lines.ToArray();
                    }
                    // connect each pair of points to build lines
                    for (int i = 0; i < points.Count && i+1 < points.Count; i++)
                    {
                        Point p1 = new Point(points[i].X + position.X, points[i].Y + position.Y);
                        Point p2 = new Point(points[i + 1].X + position.X, points[i + 1].Y + position.Y);
                        lines.Add(new Line(p1, p2));
                    }
                    // connect the last point to the first point if it is a shape
                    if (isShape)
                    {
                        Point p1 = new Point(points.Last().X + position.X, points.Last().Y + position.Y);
                        Point p2 = new Point(points.First().X + position.X, points.First().Y + position.Y);
                        lines.Add(new Line(p1, p2));
                    }
                }
                return lines.ToArray();
            }
        }
    }
}
