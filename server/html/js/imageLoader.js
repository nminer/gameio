//========================== Image loading ==========================
/**
 * the image loader holds all the loaded images
 */
class ImageLoader {
    /**
     * key path to image, value is the loaded image.
     */
    static LoadedImages = new Map();

    /**
     * 
     * @param {string} path the path to the image
     */
    static GetImage(path) {
        if (ImageLoader.LoadedImages.has(path)) {
            return ImageLoader.LoadedImages.get(path);
        }
        var imageToAdd = new Image();
        imageToAdd.src = path;
        ImageLoader.LoadedImages.set(path, imageToAdd);
        return imageToAdd;
    }
}
