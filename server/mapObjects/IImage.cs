namespace server.mapObjects
{
    interface IImage
    {
        /// <summary>
        /// return true if IImage has an image to return.
        /// </summary>
        /// <returns></returns>
        public bool HasImage();

        /// <summary>
        /// output an object to be used as json return.
        /// return null if no image object.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public object? GetJsonImageObject(Point? position = null);
    }
}
