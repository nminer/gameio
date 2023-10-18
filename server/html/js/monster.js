const monsterLookup = new Map();

const mapMonsters = new Map();
//========================== MonsterType ========================
class MonsterType {
    constructor(type, animations) {

    }
}

class NotLoadedAnimation {
    constructor(name, image, frames, x, y, width, height, drawWidth, drawHeight, after, slowdown, startFrame, horizontal) {
        this.name = name;
        this.image = image;
        this.frames = frames;
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
        this.drawWidth = drawWidth;
        this.drawHeight = drawHeight;
        this.after = after;
        this.slowDown = slowdown;
        this.startFrame = startFrame;
        this.horizontal = horizontal;
    }
}
//========================== Animation ==========================
/**
 * class for animating sprite sheets.
 */
class MonsterAnimation {
    /**
     * call step to go to next image in the animations.
     * call draw to draw the image/frame to canvas.
     * @param {Image} image sprite sheet or a list/array of sprite sheets.
     * @param {number} frames number of frames to be animated.
     * @param {number} x x for where to start the animation from
     * @param {number} y y for where to start the animation from
     * @param {number} width width of each frame
     * @param {number} height height of each frame
     * @param {any} slowdown this is how much to slow the animation down(loops before next image.)
     */
    constructor(image, frames, x, y, width, height, drawWidth, drawHeight, baseSlowdown, slowdown, beingAnimated, afterAnimationName, startFrame = 0, horizontal = true) {
        this.image = image;
        this.frames = frames;
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
        this.drawWidth = drawWidth;
        this.drawHeight = drawHeight;
        this.currentFrame = startFrame;
        this.drawX = x;
        this.drawY = y;
        this.baseSlowdown = baseSlowdown;
        this.slowdown = slowdown;
        this.countSlowdown = 0;
        this.afterAnimationName = afterAnimationName;
        this.animated = beingAnimated;
        this.stepHorizontal = horizontal;
        this.startFrameSet = false;
    }

    /**
     * step to the next image/frame in the animation.
     */
    step() {
        this.countSlowdown += 1;
        if (this.countSlowdown >= this.slowdown.getSlowdown()) {
            this.currentFrame += 1;
            this.countSlowdown = 0;
            this.setDrawXYAndFrame();
        }
    }

    setDrawXYAndFrame() {
        if (this.currentFrame >= this.frames) {
            this.currentFrame = 0;
            if (this.afterAnimationName !== undefined && this.afterAnimationName != "" && this.animated !== undefined) {
                this.animated.animation = this.animated.animations.get(this.afterAnimationName);
            }
        }
        if (this.currentFrame == 0) {
            this.drawX = this.x;
            this.drawY = this.y;
            return;
        }
        if (this.stepHorizontal) {
            if ((this.drawX + this.width + this.width) > this.image.width) {
                this.drawX = this.x;
                this.drawY = this.drawY + this.height;
            } else {
                this.drawX = this.drawX + this.width;
            }
        } else {
            if ((this.drawY + this.height + this.height) > this.image.height) {
                this.drawY = this.y;
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
    draw(x, y) {
        if (!this.image.complete) {
            // make sure the image is loaded.
            return;
        }
        if (!this.startFrameSet) {
            this.setStartFrame()
        }
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
                    this.drawWidth,
                    this.DrawHeight);
            }
        } else {
            c.drawImage(this.image,
                this.drawX,
                this.drawY,
                this.width,
                this.height,
                x,
                y,
                this.drawWidth,
                this.drawHeight);
        }
    }
}

//========================== Monster ==========================
class Monster {
    /**
     * the monsters will have an x and y coord.
     * current animation / action
     * 
     */
    constructor(id, name, animations, slowdown) {
        this.Id = id;
        this.animations = new Map();
        this.loadAnimation(animations);
        this.animation = this.animations.get("standDown");
        this.speed = 1;
        this.name = new DisplayText(name, 14, 245, 39, 39, .7);
        this.animationName = "standDown";       
        this.slowdown = slowdown
    }

    /**
     * linked with speed this on is used in the animations.
     */
    getSlowdown() {
        //return 10 - this.speed/2;
        return this.slowdown;
    }

    loadAnimation(animations) {
        for (let i = 0; i < animations.length; i++) {
            let toadd = animations[i];
            this.animations.set(toadd.name, new MonsterAnimation(toadd.image, toadd.frames, toadd.x, toadd.y, toadd.width, toadd.height, toadd.drawWidth, toadd.drawHeight, toadd.slowdown, this, this, toadd.after, toadd.startFrame, toadd.horizontal));
        }
    }

    // take in the frame from the server and update its stats.
    // and animation.
    updateFrame(frame) {
        this.X = frame["x"];
        this.Y = frame["y"];
        this.drawOrder = frame["y"];
        if (this.animationName != frame["animation"]) {
            console.log("monster:" + frame["animation"])
            this.animationName = frame["animation"];
            if (this.animationName.includes("swing")) {
                this.animations.get(frame["animation"]).currentFrame = 0;
                this.animations.get(frame["animation"]).setDrawXYAndFrame();
            }
            this.animation = this.animations.get(frame["animation"]);
        }
        this.slowdown = frame["slowdown"];
        this.health = frame["health"];
        this.maxHealth = frame["maxHealth"];
    }

    draw(c, xOffset, yOffset) {
        c.fillStyle = 'rgba(0,0,0,.2)';
        if (this.animationName != "dieingDown") {
            drawEllipseByCenter(c, this.X + xOffset, this.Y + yOffset + 10, 35, 15);
        }
        if (!this.animation) {
            return;
        }
        this.animation.draw(this.X + xOffset - (this.animation.drawWidth / 2), this.Y + yOffset - (this.animation.drawHeight - 15));
        this.animation.step();
    }

    drawHealthBar(c, xOffset, yOffset) {
        if (!this.animation) {
            return;
        }
        if (this.health != this.maxHealth && this.health > 0) {
            c.fillStyle = 'rgba(0,0,0,.4)';
            c.fillRect(this.X + xOffset - (this.animation.drawWidth * .3) - 2, this.Y + yOffset - (this.animation.drawHeight * .8) - 2 + 4, (this.animation.drawWidth * .6) + 4, 10);
            c.fillStyle = 'rgba(155,0,0,.8)';
            const p = this.health / this.maxHealth;
            c.fillRect(this.X + xOffset - (this.animation.drawWidth * .3), this.Y + yOffset - (this.animation.drawHeight * .8) + 4, (this.animation.drawWidth * .6) * p, 6);
            c.fillStyle = 'rgba(255,255,255,.2)';
            c.fillRect(this.X + xOffset - (this.animation.drawWidth * .3), this.Y + yOffset - (this.animation.drawHeight * .8) + 1 + 4, (this.animation.drawWidth * .6) * p, 1);
            c.fillStyle = 'rgba(0,0,0,.2)';
            c.fillRect(this.X + xOffset - (this.animation.drawWidth * .3), this.Y + yOffset - (this.animation.drawHeight * .8) + 5 + 4, (this.animation.drawWidth * .6) * p, 1);
        }
    }

    drawName(c, xOffset, yOffset) {
        if (!this.animation) {
            return;
        }
        this.drawHealthBar(c, xOffset, yOffset);
        this.name.draw(c, this.X + xOffset, this.Y + yOffset - (this.animation.drawHeight * .8));
    }
}

// ========================== monster factory =======================================

// messages being passed in frames will have: monster id, monster type, name, x, y, animation, slowdown
function addAllMonsters(data) {
    var monstersToLoad = data["monsters"];
    var monstIds = [];
    for (let i = 0; i < monstersToLoad.length; i++) {
        var mnst = monstersToLoad[i];
        monstIds.push(mnst['id']);
        if (mapMonsters.has(mnst['id'])) {
            // monster all loaded and in map monsters.
            mapMonsters.get(mnst['id']).updateFrame(mnst);
        } else if (monsterLookup.has(mnst['type'])) {
            // have the monster type loaded but not this monster. so make a new monster.
            let savedMonst = monsterLookup.get(mnst['type']);
            mapMonsters.set(mnst['id'], new Monster(mnst['id'], mnst['name'], savedMonst, mnst['slowdown']));
            mapMonsters.get(mnst['id']).updateFrame(mnst);
        } else {
            // don't have the monster type loaded to load the type.
            requestMonster(mnst['type']);
        }
    }
    var toRemoveIds = [];
    for (const mId of mapMonsters.keys()) {
        if (!monstIds.includes(mId)) {
            toRemoveIds.push(mId);
        }
    };
    for (let i = 0; i < toRemoveIds.length; i++) {
        mapMonsters.delete(toRemoveIds[i]);
    }
}

function loadMonsterType(data) {
    let monsterAnimations = data['animations'];
    let toAdd = [];
    for (i = 0; i < monsterAnimations.length; i++) {
        let ma = monsterAnimations[i];
        let img = ImageLoader.GetImage("./" + ma["image"]);
        notanimation = new NotLoadedAnimation(ma['name'], img, ma['frames'], ma['x'], ma['y'], ma['width'], ma['height'], ma['drawWidth'], ma['drawHeight'], ma['after'], ma['slowdown'], ma['startFrame'], ma['horizontal']);
        toAdd.push(notanimation);
    }
    monsterLookup.set(data['type'], toAdd);
}

function clearMonsters() {
    mapMonsters.length = 0;
}