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
        /// all points are relative to position.
        /// </summary>
        private Point shapePosition;

        private Shape shape;

        /// <summary>
        /// set the solids position on the map. 
        /// this is default 0 0 if left null.
        /// </summary>
        /// <param name="shapePosition"></param>
        public Solid(Shape shape, Point ? shapePosition = null)
        {
            lock (threadLock)
            {
                // set the default to 0, 0
                if (shapePosition == null)
                {
                    shapePosition = new Point(0, 0);
                }
                this.shapePosition = shapePosition;
                this.shape = shape;
            }
        }

        /// <summary>
        /// create a simple solid. 4 points.
        /// </summary>
        /// <param name="shapePosition"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        public Solid(Point shapePosition, double height, double width)
        {
            this.shapePosition = shapePosition;
            shape = new Shape();
            shape.AddPoint(0, 0).AddPoint(width, 0).AddPoint(width, height).AddPoint(0, height);
        }

        /// <summary>
        /// returns a list of lines that make up the solid.
        /// </summary>
        /// <returns></returns>
        public Line[] Lines(Point? position = null)
        {
            if (position is null)
            {
                position = new Point(0, 0);
            }     
            lock (threadLock)
            {
                return shape.Lines(shapePosition + position);
            }
        }
    }
}
