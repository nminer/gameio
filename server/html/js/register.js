const canvas = document.querySelector('#mainGame');
const c = canvas.getContext('2d');

var bodyStyleSelect = document.getElementById("bodyStyle");
var bodyColorSelect = document.getElementById("bodyColor");
var noseStyleSelect = document.getElementById("noseStyle");
var earStyleSelect = document.getElementById("earStyle");
var eyeColorSelect = document.getElementById("eyeColor");
var wrinklesStyleSelect = document.getElementById("wrinklesStyle");
var hairStyleSelect = document.getElementById("hairStyle");
var hairColorSelect = document.getElementById("hairColor");
var beardStyleSelect = document.getElementById("beardStyle");
var beardColorSelect = document.getElementById("beardColor");


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
    constructor(image, frames, x, y, width, height, slowdown, after, eachfram) {
        this.image = image;
        this.frames = frames;
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
        this.currentFrame = 0;
        this.drawX = x;
        this.drawY = y;
        this.slowdown = slowdown;
        this.countSlowdown = 0;
        this.after = after;
        this.eachFram = eachfram;
    }

    /**
     * step to the next image/frame in the animation.
     */
    step() {
        this.countSlowdown += 1;
        if (this.countSlowdown >= this.slowdown.getSlowdown()) {
            this.currentFrame += 1;
            this.countSlowdown = 0;
            if (typeof this.eachFram !== 'undefined') {
                this.eachFram();
            }
        }
        if (this.currentFrame >= this.frames) {
            this.currentFrame = 0;
            this.after();
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
var animations;
function loadAnimation(avatar) {
    let body = avatar["body"];
    let bodyc = avatar["bodyc"];
    let playerImage = ImageLoader.GetImage("../img/player/body/" + body + "-" + bodyc + ".png");
    let images = [];
    images.push(playerImage);
    if (avatar["wrinkles"] != "00") {
        let wrinklesImage = ImageLoader.GetImage("../img/player/wrinkles/" + avatar["wrinkles"] + "-" + bodyc + ".png");
        images.push(wrinklesImage);
    }
    if (avatar["ears"] != "00") {
        let earsImage = ImageLoader.GetImage("../img/player/ears/" + avatar["ears"] + "-" + bodyc + ".png");
        images.push(earsImage);
    }
    if (avatar["eyec"] != "00") {
        let eyeImage = ImageLoader.GetImage("../img/player/eyes/" + avatar["eyec"] + ".png");
        images.push(eyeImage);
    }
    if (avatar["beard"] != "00" && avatar["beardc"] != "00") {
        let hairImage = ImageLoader.GetImage("../img/player/beard/" + avatar["beard"] + "-" + avatar["beardc"] + ".png");
        images.push(hairImage);
    }
    if (avatar["nose"] != "00") {
        let noseImage = ImageLoader.GetImage("../img/player/nose/" + avatar["nose"] + "-" + bodyc + ".png");
        images.push(noseImage);
    }
    if (avatar["hair"] != "00" && avatar["hairc"] != "00") {
        let hairImage = ImageLoader.GetImage("../img/player/hair/" + avatar["hair"] + "-" + avatar["hairc"] + ".png");
        images.push(hairImage);
    }
    let none = () => { };
    const idelSlow = { getSlowdown: function () { return 5; } };
    animations =
    {
        walkDown: new CharAnimation(images, 8, 64, 640, 64, 64, idelSlow, none),
        walkUp: new CharAnimation(images, 8, 64, 512, 64, 64, idelSlow, none),
        walkLeft: new CharAnimation(images, 8, 64, 576, 64, 64, idelSlow, none),
        walkRight: new CharAnimation(images, 8, 64, 704, 64, 63, idelSlow, none),
        standDown2: new CharAnimation(images, 2, 0, 128, 64, 64, idelSlow, none),
        standDown: new CharAnimation(images, 1, 0, 640, 64, 64, idelSlow, none),
        standUp: new CharAnimation(images, 1, 0, 512, 64, 64, idelSlow, none),
        standLeft: new CharAnimation(images, 1, 0, 576, 64, 64, idelSlow, none),
        standRight: new CharAnimation(images, 1, 0, 704, 64, 63, idelSlow, none),
        swingDown: new CharAnimation(images, 5, 64, 896, 64, 64, idelSlow, none),
        swingUp: new CharAnimation(images, 5, 64, 768, 64, 64, idelSlow, none),
        swingLeft: new CharAnimation(images, 5, 64, 832, 64, 64, idelSlow, none),
        swingRight: new CharAnimation(images, 5, 64, 960, 64, 63, idelSlow, none),
    }
}

//============================================================
var stop = false;
var frameCount = 0;
var fps, fpsInterval, startTime, now, then, elapsed;

// initialize the timer variables and start the animation

function startAnimating(fps) {
    fpsInterval = 1000 / fps;
    then = Date.now();
    startTime = then;
    animate();
}

function animate() {
    // request another frame
    requestAnimationFrame(animate);
    // calc elapsed time since last loop
    now = Date.now();
    elapsed = now - then;
    // if enough time has elapsed, draw the next frame
    if (elapsed > fpsInterval) {
        // Get ready for next frame by setting then=now, but also adjust for your
        // specified fpsInterval not being a multiple of RAF's interval (16.7ms)
        then = now - (elapsed % fpsInterval);

        c.fillStyle = 'black';
        c.fillRect(0, 0, canvas.width, canvas.height);
        // Put your drawing code here
        animations["walkDown"].draw(60, 0, 140, 140);
        animations["walkDown"].step();
    }
}

function intToStr(input) {
    return (input).toLocaleString('en-US', { minimumIntegerDigits: 2, useGrouping: false })
}

function buildAvatar() {
    avatar = {
        "body": intToStr(parseInt(bodyStyleSelect.value)),
        "bodyc": intToStr(parseInt(bodyColorSelect.value)), 
        "nose": intToStr(parseInt(noseStyleSelect.value)), 
        "ears": intToStr(parseInt(earStyleSelect.value)), 
        "eyec": intToStr(parseInt(eyeColorSelect.value)), 
        "wrinkles": intToStr(parseInt(wrinklesStyleSelect.value)), 
        "hair": intToStr(parseInt(hairStyleSelect.value)), 
        "hairc": intToStr(parseInt(hairColorSelect.value)), 
        "beard": intToStr(parseInt(beardStyleSelect.value)), 
        "beardc": intToStr(parseInt(beardColorSelect.value)), 
    }
    loadAnimation(avatar);
}
buildAvatar();
bodyStyleSelect.addEventListener("change", buildAvatar);
bodyColorSelect.addEventListener("change", buildAvatar);
noseStyleSelect.addEventListener("change", buildAvatar);
earStyleSelect.addEventListener("change", buildAvatar);
eyeColorSelect.addEventListener("change", buildAvatar);
wrinklesStyleSelect.addEventListener("change", buildAvatar);
hairStyleSelect.addEventListener("change", buildAvatar);
hairColorSelect.addEventListener("change", buildAvatar);
beardStyleSelect.addEventListener("change", buildAvatar);
beardColorSelect.addEventListener("change", buildAvatar);
startAnimating(60);

let townsounds =
    [
        { file: 'town1.wav', location: '../sounds/town/', duration: 3 },
        { file: 'town2.wav', location: '../sounds/town/', duration: 1 },
        { file: 'town3.wav', location: '../sounds/town/', duration: 1 },
        { file: 'town4.wav', location: '../sounds/town/', duration: 4 },
        { file: 'town5.wav', location: '../sounds/town/', duration: 2 },
        { file: 'town6.wav', location: '../sounds/town/', duration: 4 },
        { file: 'town7.wav', location: '../sounds/town/', duration: 6 },
        { file: 'town8.wav', location: '../sounds/town/', duration: 5 },
        { file: 'town9.wav', location: '../sounds/town/', duration: 5 },
        { file: 'town10.wav', location: '../sounds/town/', duration: 1 },
        { file: 'town11.wav', location: '../sounds/town/', duration: 1 },
        { file: 'town12.wav', location: '../sounds/town/', duration: 1 },
        { file: 'town13.wav', location: '../sounds/town/', duration: 1 },

    ];

let backgoundsounds =
    [
        { file: 'Rejoicing.mp3', location: '../sounds/town/', duration: 103 },
        { file: 'Ale-and-Anecdotes.mp3', location: '../sounds/town/', duration: 145 },
    ];

payListOfSounds(townsounds, -1, Array.apply(null, Array(townsounds.length)), true, true);
payListOfSounds(townsounds, -1, Array.apply(null, Array(townsounds.length)), true, true);
payListOfSounds(backgoundsounds, -1, Array.apply(null, Array(backgoundsounds.length)), true, false);