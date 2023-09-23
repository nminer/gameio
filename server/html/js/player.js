//========================== Animation ==========================
/**
 * class for animating sprite sheets.
 */
class CharAnimation {
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

//========================== Player ==========================
class Player {
    /**
     * the Player will have an x and y coord.
     * current animation / action
     * 
     */
    constructor(id, avatar) {
        this.Id = id;
        this.width = 80; // hight the sprite is displayed
        this.height = 80; // width the sprite is displayed
        this.loadAnimation(avatar);
        this.animation = this.animations.standDown
        this.speed = 1;
        this.name = new DisplayText(this.Id, 14, 35, 163, 255, .7);
        this.animationName = "stand";
        this.health = 0;
        this.maxHealth = 0;
        this.stamina = 0;
        this.maxStamina = 0;
        this.mana = 0;
        this.maxMana = 0;
    }

    setAsPlayer() {
        this.name = new DisplayText(this.Id, 14, 145, 191, 224, .7);
    }

    /**
     * linked with speed this on is used in the animations.
     */
    getSlowdown() {
        //return 10 - this.speed/2;
        let speedmod = convertRange(0, 100, 2, 10, this.speed)
        if (this.animationName.includes("walk") && this.running) {
            speedmod += 5;
        }
        return 10 - speedmod;
    }

    loadAnimation(avatar) {
        let body = avatar["body"];
        let bodyc = avatar["bodyc"];
        let playerImage = ImageLoader.GetImage("./img/player/body/" + body + "-" + bodyc + ".png");
        let images = [];
        images.push(playerImage);
        if (avatar["wrinkles"] != "00") {
            let wrinklesImage = ImageLoader.GetImage("./img/player/wrinkles/" + avatar["wrinkles"] + "-" + bodyc + ".png");
            images.push(wrinklesImage);
        }
        if (avatar["ears"] != "00") {
            let earsImage = ImageLoader.GetImage("./img/player/ears/" + avatar["ears"] + "-" + bodyc + ".png");
            images.push(earsImage);
        }
        if (avatar["eyec"] != "00") {
            let eyeImage = ImageLoader.GetImage("./img/player/eyes/" + avatar["eyec"] + ".png");
            images.push(eyeImage);
        }
        if (avatar["beard"] != "00" && avatar["beardc"] != "00") {
            let hairImage = ImageLoader.GetImage("./img/player/beard/" + avatar["beard"] + "-" + avatar["beardc"] + ".png");
            images.push(hairImage);
        }
        if (avatar["nose"] != "00") {
            let noseImage = ImageLoader.GetImage("./img/player/nose/" + avatar["nose"] + "-" + bodyc + ".png");
            images.push(noseImage);
        }
        if (avatar["hair"] != "00" && avatar["hairc"] != "00") {
            let hairImage = ImageLoader.GetImage("./img/player/hair/" + avatar["hair"] + "-" + avatar["hairc"] + ".png");
            images.push(hairImage);
        }
        let none = () => { };
        const idelSlow = { getSlowdown: function () { return Math.random() * (50 - 20) + 20; } };
        let standDownAnimation = new CharAnimation(images, 1, 0, 640, 64, 64, this, this);
        let standUpAnimation = new CharAnimation(images, 1, 0, 512, 64, 64, this, this);
        let standLeftAnimation = new CharAnimation(images, 1, 0, 576, 64, 64, this, this);
        let standRightAnimatin = new CharAnimation(images, 1, 0, 704, 64, 63, this, this);
        let deadDownAnimation = new CharAnimation(images, 1, 320, 1280, 64, 64, this, this);
        this.animations =
        {
            walkDown: new CharAnimation(images, 8, 64, 640, 64, 64, this, this),
            walkUp: new CharAnimation(images, 8, 64, 512, 64, 64, this, this),
            walkLeft: new CharAnimation(images, 8, 64, 576, 64, 64, this, this),
            walkRight: new CharAnimation(images, 8, 64, 704, 64, 63, this, this),
            standDown2: new CharAnimation(images, 2, 0, 128, 64, 64, idelSlow),
            standDown: standDownAnimation,
            standUp: standUpAnimation,
            standLeft: standLeftAnimation,
            standRight: standRightAnimatin,
            swingDown: new CharAnimation(images, 5, 64, 896, 64, 64, this, this, standDownAnimation),
            swingUp: new CharAnimation(images, 5, 64, 768, 64, 64, this, this, standUpAnimation),
            swingLeft: new CharAnimation(images, 5, 64, 832, 64, 64, this, this, standLeftAnimation),
            swingRight: new CharAnimation(images, 5, 64, 960, 64, 63, this, this, standRightAnimatin),
            castDown: new CharAnimation(images, 7, 0, 128, 64, 64, this, this, standDownAnimation),
            castUp: new CharAnimation(images, 7, 0, 0, 64, 64, this, this, standUpAnimation),
            castLeft: new CharAnimation(images, 7, 0, 64, 64, 64, this, this, standLeftAnimation),
            castRight: new CharAnimation(images, 7, 0, 192, 64, 64, this, this, standRightAnimatin),
            dieingDown: new CharAnimation(images, 5, 0, 1280, 64, 64, this, this, deadDownAnimation),
        }
    }

    // take in the frame from the server and update its stats.
    // and animation.
    updateFrame(frame) {
        this.X = frame["x"];
        this.Y = frame["y"];
        this.drawOrder = frame["y"];
        if (frame["coolDown"] !== undefined) {
            if (this.CoolDownCount < frame["coolDownCount"]) {
                this.animationName = "";
            }
            this.CoolDown = frame["coolDown"];
            this.CoolDownCount = frame["coolDownCount"];
            if (this.coolDownCount == 0) {
                this.animationName = "";
                this.CoolDown = 0;
            }
        } else if (this.CoolDown !== undefined && this.CoolDown > 0) {
            this.CoolDown = 0;
            this.CoolDownCount = 0;
            this.animationName = "";
        }
        if (this.animationName != frame["animation"]) {
            console.log(frame["animation"])
            this.animation = this.animations[frame["animation"]];
            this.animationName = frame["animation"];
            if (this.animationName.includes("swing")) {
                this.animation.currentFrame = 0;
            }
        }
        this.speed = frame["speed"]
        this.running = frame["running"];
        this.health = frame["health"];
        this.maxHealth = frame["maxHealth"];
        this.stamina = frame["stamina"];
        this.maxStamina = frame["maxStamina"];
        this.mana = frame["mana"];
        this.maxMana = frame["maxMana"];
    }

    draw(c, xOffset, yOffset) {
        //c.fillStyle = 'red';
        //c.fillRect(this.X + xOffset - 10, this.Y + yOffset - 40, 20, 50);
        //this.name.draw(this.X + xOffset, this.Y + yOffset - 60);
        c.fillStyle = 'rgba(0,0,0,.2)';
        if (this.animationName != "dieingDown") {
            drawEllipseByCenter(c, this.X + xOffset, this.Y + yOffset + 10, 35, 15);
        }
        this.animation.draw(this.X + xOffset - 40, this.Y + yOffset - 66, this.width, this.height);
        this.animation.step();
    }

    drawName(c, xOffset, yOffset) {
        this.name.draw(c, this.X + xOffset, this.Y + yOffset - 60);
    }
}