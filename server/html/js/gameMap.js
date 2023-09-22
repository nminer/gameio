//============================ Map ===========================
/**
 * Map class hold the back ground image for the world
 * Maps top left (0,0)
 * 
 */
class GameMap {

    /**
     * create a new world with a background.
     * @param {number} width
     * @param {number} height
     * @param {Image} image
     */
    constructor(width, height, image) {
        this.width = width;
        this.height = height;
        this.image = image;
    }

    /**
     * call to draw the map on the canvas
     * @param {number} x players x position
     * @param {number} y players y position
     */
    draw(c, x, y) {
        c.drawImage(this.image,
            x,
            y,
            this.width,
            this.height);
    }
}
