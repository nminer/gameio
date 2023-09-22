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

class MonsterAnimation {
    /**
     * call step to go to next image in the animations.
     * call draw to draw the image/frame to canvas.
     * @param {Image} image sprite sheet
     * @param {number} frames number of frames to be animated.
     * @param {number} x x for where to start the animation from
     * @param {number} y y for where to start the animation from
     * @param {number} width width of each frame
     * @param {number} height height of each frame
     * @param {any} slowdown this is how much to slow the animation down(loops before next image.)
     */
    constructor(image, frames, x, y, width, height, slowdown, beingAnimated, after, startFram = 0) {
        this.image = image;
        this.frames = frames;
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
        this.currentFrame = startFram;
        this.drawX = x;
        this.drawY = y;
        this.slowdown = slowdown;
        this.countSlowdown = 0;
        this.after = after;
        this.animated = beingAnimated;
    }

    /**
     * step to the next image/frame in the animation.
     */
    step() {
        this.countSlowdown += 1;
        if (this.countSlowdown >= this.slowdown.getSlowdown()) {
            this.currentFrame += 1;
            this.countSlowdown = 0;
        }
        if (this.currentFrame >= this.frames) {
            this.currentFrame = 0;
            if (this.after !== undefined && this.animated !== undefined) {
                this.animated.animation = this.after;
            }
        }
        this.drawX = this.x + (this.width * this.currentFrame);
    }

    /**
     * draw the frame to canvas.
     * at the given x,y and at the passed in height and width.
     * @param {number} x
     * @param {number} y
     * @param {number} width
     * @param {number} height
     */
    draw(x, y, width, height) {
        if (Array.isArray(this.image)) {
            for (var i = 0; i < this.image.length; i++) {
                let img = this.image[i];
                c.drawImage(img,
                    this.drawX,
                    this.drawY,
                    this.width,
                    this.height,
                    x,
                    y,
                    width, height);
            }
        } else {
            c.drawImage(this.image,
                this.drawX,
                this.drawY,
                this.width,
                this.height,
                x,
                y,
                width, height);
        }
    }
}