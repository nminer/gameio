using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Permissions;

namespace server.mapObjects
{
    internal class SoundAffect
    {


        public Int64 FullVolumeRadius;

        public Int64 FadeVolumeRadius;

        public Int64 DelayMax;

        public Int64 DelayMin;

        public bool Repeat;

        public bool HasDelay;

        /// <summary>
        /// Returns true if the min and max delay are the same.
        /// </summary>
        public bool RandomDelay
        {
            get
            {
                return DelayMax == DelayMin;
            }
        }

        public string SoundPath;

        public Point Position;

        /// <summary>
        /// Load a sound from its sound id in the database.
        /// </summary>
        /// <param name="imageId"></param>
        public SoundAffect(string path, bool repeat, Point position, Int64 fullRadius, Int64 fadeRadius)
        {
            this.SoundPath = path;
            this.Repeat = repeat;
            this.FadeVolumeRadius = fadeRadius;
            this.FullVolumeRadius = fullRadius;
            this.Position = position;
        }

        public object? GetJsonSoundObject()
        {
            return new { path = SoundPath, repeat = Repeat, x = Position.X, y = Position.Y, fullRadius = FullVolumeRadius, fadeRadius = FadeVolumeRadius };
        }
    }
}
