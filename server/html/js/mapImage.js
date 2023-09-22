class MapImage {
    /**
     * create a new image for a map.
     * @param {number} width
     * @param {number} height
     * @param {Image} image
     */
    constructor(width, height, path, x, y, draworder) {
        this.X = x;
        this.Y = y;
        this.drawOrder = draworder;
        this.width = width;
        this.height = height;
        this.image = ImageLoader.GetImage("./" + path);
    }

    /**
     * call to draw the map on the canvas
     * @param {number} x players x position
     * @param {number} y players y position
     */
    draw(c, xOffset, yOffset) {
        c.drawImage(this.image,
            xOffset + this.X,
            yOffset + this.Y,
            this.width,
            this.height);
    }
}