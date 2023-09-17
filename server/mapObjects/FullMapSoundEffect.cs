using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server.mapObjects
{
    class FullMapSoundEffect
    {

        public bool Repeat;

        public string SoundPath;

        public double Volume;

        /// <summary>
        /// Load a sound from its sound id in the database.
        /// </summary>
        /// <param name="imageId"></param>
        public FullMapSoundEffect(string path, bool repeat, double volume = 1)
        {
            this.SoundPath = path;
            this.Repeat = repeat;
            Volume = volume;
        }

        public object? GetJsonSoundObject()
        {
            return new {path = SoundPath, repeat = Repeat, volume = Volume};
        }
    }
}
