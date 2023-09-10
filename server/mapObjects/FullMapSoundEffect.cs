using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server.mapObjects
{
    class FullMapSoundEffect
    {

        public int Repeat;

        public string SoundPath;

        /// <summary>
        /// Load a sound from its sound id in the database.
        /// </summary>
        /// <param name="imageId"></param>
        public FullMapSoundEffect(string path, int repeat)
        {
            this.SoundPath = path;
            this.Repeat = repeat;
        }

        public object? GetJsonSoundObject()
        {
            return new {path = SoundPath, repeat = Repeat};
        }
    }
}
