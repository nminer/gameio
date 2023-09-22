class MapAnimation {
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
    constructor(imagepath, mapx, mapy, frames, imagex, imagey, width, height, slowdown, horizontal = true, startFrame = 0, draworder = 0) {
        this.image = ImageLoader.GetImage("./" + imagepath);
        this.mapX = mapx;
        this.mapY = mapy;
        this.frames = frames;
        this.imageX = imagex;
        this.imageY = imagey;
        this.width = width;
        this.height = height;
        this.stepHorizontal = horizontal;
        this.startFrame = startFrame;
        this.currentFrame = 0;
        this.slowdown = slowdown;
        this.countSlowdown = 0;
        this.drawOrder = draworder;
        this.startFrameSet = false;
        this.drawX = this.imageX;
        this.drawY = this.imageY;
        this.finshed = false; // set to true when currentFrame = frames count - 1.
    }

    /**
     * step to the next image/frame in the animation.
     */
    step() {
        this.countSlowdown += 1;
        if (this.countSlowdown >= this.slowdown) {
            this.currentFrame += 1;
            this.countSlowdown = 0;
            this.setDrawXYAndFrame();
        }
    }

    setDrawXYAndFrame() {
        if (this.currentFrame >= this.frames) {
            this.currentFrame = 0;
        }
        if (this.currentFrame == 0) {
            this.drawX = this.imageX;
            this.drawY = this.imageY;
            return;
        }
        if (this.stepHorizontal) {
            if ((this.drawX + this.width + this.width) > this.image.width) {
                this.drawX = this.imageX;
                this.drawY = this.drawY + this.height;
            } else {
                this.drawX = this.drawX + this.width;
            }
        } else {
            if ((this.drawY + this.height + this.height) > this.image.height) {
                this.drawY = this.imageY;
                this.drawX = this.drawX + this.width;
            }
        }
    }

    setStartFrame() {
        this.startFrameSet = true;
        while (this.currentFrame < this.startFrame) {
            this.setDrawXYAndFrame();
            this.currentFrame += 1;
        }
    }

    /**
     * draw the frame to canvas.
     * at the given x,y and at the passed in height and width.
     * @param {number} x
     * @param {number} y
     * @param {number} width
     * @param {number} height
     */
    draw(c, offsetx, offsety) {
        if (!this.image.complete) {
            // make sure the image is loaded.
            return;
        }
        if (!this.startFrameSet) {
            this.setStartFrame()
        }
        c.drawImage(this.image,
            this.drawX,
            this.drawY,
            this.width,
            this.height,
            offsetx + this.mapX,
            offsety + this.mapY,
            this.width, this.height);
        this.finshed = this.currentFrame == this.frames - 1
        this.step();
    }
}