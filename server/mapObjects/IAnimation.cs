
namespace server.mapObjects
{
    interface IAnimation
    {
        /// <summary>
        /// return true if IAnimation has an animation to return.
        /// </summary>
        /// <returns></returns>
        public bool HasAnimation();

        /// <summary>
        /// output an object to be used as json return.
        /// return null if no animation object.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public object? GetJsonAnimationObject(Point? position = null);
    }
}
