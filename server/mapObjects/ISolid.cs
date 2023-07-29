using server.mapObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server.mapObjects
{
    interface ISolid
    {
        public Line[] Lines(Point? position = null);
    }
}
