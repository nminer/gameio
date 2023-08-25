using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server.mapObjects
{
    internal class Damage
    {


        public Int64 Amount;

        public Int64 R;

        public Int64 G;

        public Int64 B;

        public Point Position;

        /// <summary>
        /// Load a sound from its sound id in the database.
        /// </summary>
        /// <param name="imageId"></param>
        public Damage(Point position, Int64 amount, Int64 r, Int64 g, Int64 b)
        {
            this.Position = position;
            this.Amount = amount;
            this.R = r;
            this.G = g; 
            this.B = b;
        }

        public object? GetJsonDamageObject()
        {
            return new {x = Position.X, y = Position.Y, amount = Amount, r = R, g = G, b = B };
        }
    }
}
