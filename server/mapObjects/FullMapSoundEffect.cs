using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server.mapObjects
{
    /// <summary>
    /// this is a sound effect to send to client that will be played for everyone on a map.
    /// this sound effect is not affected by where the player is on the map.
    /// </summary>
    class FullMapSoundEffect
    {
        // set to true if the sound should be repeated none stop.
        public bool Repeat;

        /// <summary>
        /// the path in the html to the sound file to be played.
        /// </summary>
        public string SoundPath;

        /// <summary>
        /// the volume to the play the sound at.
        /// 0.0 to 1.0.
        /// </summary>
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

        /// <summary>
        /// return an object to be used for json to send to client.
        /// </summary>
        /// <returns></returns>
        public object? GetJsonSoundObject()
        {
            return new {path = SoundPath, repeat = Repeat, volume = Volume};
        }
    }
}
