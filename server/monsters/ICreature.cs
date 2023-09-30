using server.mapObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server.monsters
{
    internal interface ICreature
    {
        public Double X_Coord
        {
            get;
            set;
        }
        public Double Y_Coord
        {
            get;
            set;
        }

        public Point GetNetMoveAmount();

        public bool TakeDamage(long damageAmount);


        public SoundAffect GetTakeHitSound(bool critacalHi);

        public Circle Solid
        {
            get;
        }

    }
}
