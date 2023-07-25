using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server.mapObjects
{
    internal class ModCollision
    {
        /// <summary>
        /// returns true if the line intercept the circle.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="circle"></param>
        /// <returns></returns>
        public static bool DoesLineInterceptCircle(Line line, Circle circle)
        {
            double dist;
            double v1x = line.X2 - line.X1;
            double v1y = line.Y2 - line.Y1;
            double v2x = circle.Center.X - line.X1;
            double v2y = circle.Center.Y - line.Y1;

            // get the unit distance along the line of the closest point to
            // circle center
            double u = (v2x * v1x + v2y * v1y) / (v1y * v1y + v1x * v1x);


            // if the point is on the line segment get the distance squared
            // from that point to the circle center
            if (u >= 0 && u <= 1)
            {
                dist = Math.Pow((line.X1 + v1x * u - circle.Center.X), 2) + Math.Pow((line.Y1 + v1y * u - circle.Center.Y), 2);
            }
            else
            {
                // if closest point not on the line segment
                // use the unit distance to determine which end is closest
                // and get dist square to circle
                if (u < 0)
                {
                    dist = Math.Pow((line.X1 - circle.Center.X), 2) + Math.Pow((line.Y1 - circle.Center.Y), 2);
                } else
                {
                    dist = Math.Pow((line.X2 - circle.Center.X), 2) + Math.Pow((line.Y2 - circle.Center.Y), 2);
                }
            }
            return dist < circle.Radius * circle.Radius;
        }


    }
}
