// Natalie's and Nathan's game
//import playerImg from '../img/player/maincharacter.png';

const inboxPeople = document.querySelector(".inbox__people");
const players = document.querySelector("#players");
const inputField = document.querySelector(".message_form__input");
const messageForm = document.querySelector(".message_form");
const messageBox = document.querySelector(".messages__history");
const fallback = document.querySelector(".fallback");
//the last user to private message.
let lastReply = "";
let lastTell = "";

// keep all the sent messages in.
let messages = [];
// message search posision.
let msgPos = 0;

const canvasWindow = document.querySelector('.game_page');
const canvas = document.querySelector('#mainGame');
const c = canvas.getContext('2d');


// set up our canvas to be full width and hight
canvas.width = innerWidth;
canvas.height = innerHeight;

var myWebSocket;

var sessionId = getCookie("sessionId");
var userName = "";

var lastSentKeys = "";

// the last frame from server.
var lastUpdateFrame = "";
var lastUpdateTime = 0;

function getCookie(cname) {
    let name = cname + "=";
    let decodedCookie = decodeURIComponent(document.cookie);
    let ca = decodedCookie.split(';');
    for (let i = 0; i < ca.length; i++) {
        let c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}

function SetSessionId() {
    var sendData = { setId: sessionId };
    myWebSocket.send(JSON.stringify(sendData));
}

function connectToWS() {
    var port = parseInt(document.location.port) + 2
    var endpoint = "ws://" + document.location.hostname + ":" + port;
    if (myWebSocket !== undefined) {
        myWebSocket.close()
    }

    myWebSocket = new WebSocket(endpoint);

    myWebSocket.onmessage = function (event) {
        var leng;
        if (event.data.size === undefined) {
            leng = event.data.length
        } else {
            leng = event.data.size
        }
        //console.log("onmessage. size: " + leng + ", content: " + event.data);
        var data = JSON.parse(event.data);
        if (data.hasOwnProperty("setUser")) {
            userName = data["setUser"];
            return;
        }
        Messages(data)
    }

    function Messages(data) {
        if (data.hasOwnProperty("privateMessage")) {
            addNewPrivateMessage(data);
        } else if (data.hasOwnProperty("serverMessage")) {
            addNewServerMessage(data);
        } else if (data.hasOwnProperty("userConnect")) {
            addToUsersBox(data["userConnect"]);
        } else if (data.hasOwnProperty("userDisconnect")) {
            var userName = data["userDisconnect"]
            document.querySelector(`.${userName}-userlist`).remove();
        } else if (data.hasOwnProperty("user")) {
            addNewMessage(data);
        } else if (data.hasOwnProperty("update")) {
            lastUpdateTime = data["update"];
            lastUpdateFrame = data["frame"];
        } else if (data.hasOwnProperty("mapName")) {
            loadMap(data);
        }
    }


    myWebSocket.onopen = function (evt) {
        console.log("onopen.");
        SetSessionId();
    };

    myWebSocket.onclose = function (evt) {
        console.log("onclose.");
        window.location = "./";
    };

    myWebSocket.onerror = function (evt) {
        console.log("Error!");
    };
}

//old test send
function sendMsg() {
    var message = document.getElementById("myMessage").value;
    myWebSocket.send(message);
}

function closeConn() {
    myWebSocket.close();
}


// set tot true when keys are down
const keys = {
    left: false,
    down: false,
    right: false,
    up: false,
    shift: false,
    hit: false,
    use: false,
}

addEventListener('keydown', (event) => {
    // console.log(keyCode);
    var keyCode = event.keyCode;
    console.log(keyCode);
    if (document.activeElement === inputField) {
        return;
    }
    switch (keyCode) {
        case 13: // enter
            inputField.focus();
            event.preventDefault();
            return;
        case 65: // left
            keys.left = true
            break;
        case 83: // down
            keys.down = true
            break;
        case 68: // right
            keys.right = true
            break;
        case 87:  // up
            keys.up = true
            break;
        case 16: //shift
            keys.shift = true
            break;
        case 70: //f , this will hit
            keys.hit = true
            break;
        case 69: //e ,  use objects
            keys.use = true
    }
    sendKeys();
});

addEventListener('keyup', (event) => {
    var keyCode = event.keyCode;
    if (document.activeElement === inputField) {
        return;
    }
    switch (keyCode) {
        case 65: // left
            keys.left = false
            break;
        case 83: // down
            keys.down = false
            break;
        case 68: // right
            keys.right = false
            break;
        case 87:  // up
            keys.up = false
            break;
        case 16: //shift
            keys.shift = false
            break;
        case 70: //f , this will hit
            keys.hit = false
            break;
        case 69: //e ,  use objects
            keys.use = false
    }
    sendKeys();
});

function sendKeys() {
    var newKeys = JSON.stringify(keys);
    if (newKeys != lastSentKeys) {
        sendData = {
            movement: keys
        };
        myWebSocket.send(JSON.stringify(sendData));
        lastSentKeys = newKeys;
    }
}

inputField.addEventListener("keyup", (e) => {
    var keyCode = e.keyCode || e.which;
    if (keyCode == '38') { //up key
        msgPos++;
        if (msgPos < messages.length && msgPos >= 0) {
            inputField.value = messages[msgPos];
        } else {
            msgPos--;
        }
    } else if (keyCode == '40') { //down key
        msgPos--;
        if (msgPos < messages.length && msgPos >= 0) {
            inputField.value = messages[msgPos];
        } else {
            msgPos++;
        }
    } else {
        msgPos = -1;
    }
});
//========================== Text class ==========================
class DisplayText {
    /**
     * 
     * @param {string} text the text to display
     * @param {int} r 0-255 color
     * @param {int} g 0-255 color
     * @param {int} b 0-255 color
     * @param {double} fade <= 1 how much to fade the text
     */
    constructor(text, fontsize, r, g, b, fade) {
        this.text = text;
        this.fontsize = fontsize;
        this.r = r;
        this.g = g;
        this.b = b;
        this.fade = fade;
    }

    draw(x, y) {
        c.font = this.fontsize + 'px Comic Sans MS';
        c.textAlign = "center";
        c.fillStyle = 'rgba(' + this.r + ', ' + this.g + ', ' + this.b + ', ' + this.fade + ')';

        c.fillText(this.text,  x, y);
    }
}

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
function convertRange(oldMin, oldMax, newMin, newMax, oldValue) {
    let oldRange = oldMax - oldMin;
    let newRange = newMax - newMin;
    let newValue = (((oldValue - oldMin) * newRange) / oldRange) + newMin;
    return newValue;
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
        if (this.running) {
            speedmod += 5;
        }
        return 10 - speedmod;
    }


    loadAnimation(avatar) {
        let body = avatar["body"];
        let bodyc = avatar["bodyc"];
        let playerImage = ImageLoader.GetImage("./img/player/body/" + body + "-" + bodyc + ".png");
        let none = () => { };
        const idelSlow = { getSlowdown: function () { return Math.random() * (50 - 20) + 20; } };
        this.animations =
        {
            walkDown: new CharAnimation(playerImage, 8, 64, 640, 64, 64, this, none),
            walkUp: new CharAnimation(playerImage, 8, 64, 512, 64, 64, this, none),
            walkLeft: new CharAnimation(playerImage, 8, 64, 576, 64, 64, this, none),
            walkRight: new CharAnimation(playerImage, 8, 64, 704, 64, 63, this, none),
            standDown2: new CharAnimation(playerImage, 2, 0, 128, 64, 64, idelSlow, none),
            standDown: new CharAnimation(playerImage, 1, 0, 640, 64, 64, this, none),
            standUp: new CharAnimation(playerImage, 1, 0, 512, 64, 64, this, none),
            standLeft: new CharAnimation(playerImage, 1, 0, 576, 64, 64, this, none),
            standRight: new CharAnimation(playerImage, 1, 0, 704, 64, 63, this, none),
            swingDown: new CharAnimation(playerImage, 5, 64, 896, 64, 64, this, none),
            swingUp: new CharAnimation(playerImage, 5, 64, 768, 64, 64, this, none),
            swingLeft: new CharAnimation(playerImage, 5, 64, 832, 64, 64, this, none),
            swingRight: new CharAnimation(playerImage, 5, 64, 960, 64, 63, this, none),
        }
    }

    // take in the fram from the server and update its stats.
    // and anminmation.
    updateFrame(frame) {
        this.X = frame["x"];
        this.Y = frame["y"];
        this.animation = this.animations[frame["animation"]];
        this.speed = frame["speed"]
        this.running = frame["running"];
    }

    draw(xOffset, yOffset) {
        //c.fillStyle = 'red';
        //c.fillRect(this.X + xOffset - 10, this.Y + yOffset - 40, 20, 50);
        this.name.draw(this.X + xOffset, this.Y + yOffset - 60);
        c.fillStyle = 'rgba(0,0,0,.2)';
        drawEllipseByCenter(c, this.X + xOffset, this.Y + yOffset + 10, 35, 15);
        this.animation.draw(this.X + xOffset - 40, this.Y + yOffset - 66, this.width, this.height);
        this.animation.step();
    }
}

function drawEllipseByCenter(ctx, cx, cy, w, h) {
    drawEllipse(ctx, cx - w / 2.0, cy - h / 2.0, w, h);
}

function drawEllipse(ctx, x, y, w, h) {
    var kappa = .5522848,
        ox = (w / 2) * kappa, // control point offset horizontal
        oy = (h / 2) * kappa, // control point offset vertical
        xe = x + w,           // x-end
        ye = y + h,           // y-end
        xm = x + w / 2,       // x-middle
        ym = y + h / 2;       // y-middle

    ctx.beginPath();
    ctx.moveTo(x, ym);
    ctx.bezierCurveTo(x, ym - oy, xm - ox, y, xm, y);
    ctx.bezierCurveTo(xm + ox, y, xe, ym - oy, xe, ym);
    ctx.bezierCurveTo(xe, ym + oy, xm + ox, ye, xm, ye);
    ctx.bezierCurveTo(xm - ox, ye, x, ym + oy, x, ym);
    ctx.closePath(); // not used correctly, see comments (use to close off open path)
    //ctx.stroke();
    //ctx.fi
    ctx.fill();
}

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
    draw(x, y) {
        c.drawImage(this.image,
            x,
            y,
            this.width,
            this.height);
    }
}
let currentMap = null;
// player name to player objects for current map.
// name:Player
const playerLookup = new Map();

function loadMap(data) {
    //var worldbackground = new Image();
    //worldbackground.src = "./" + data["image"];
    var worldbackground = ImageLoader.GetImage("./" + data["image"]);
    var height = data["height"];
    var width = data["width"];
    currentMap = new GameMap(width, height, worldbackground);
    playerLookup.clear();
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

window.addEventListener('resize', resizeCanvas, false);
function resizeCanvas() {
    canvas.width = canvasWindow.scrollWidth;
    canvas.height = canvasWindow.scrollHeight;
}
// the animation loop calculates time elapsed since the last loop
// and only draws if your specified fps interval is achieved
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
        // Put your drawing code here
        resizeCanvas();
        c.clearRect(0, 0, canvas.width, canvas.height);
        c.fillStyle = 'black';
        c.fillRect(0, 0, canvas.width, canvas.height);

        if (lastUpdateFrame == null) return;
        if (currentMap == null) return;
        
        // myWebSocket.send(JSON.stringify(keys));
        //find the current player
        var curPlayer = null;
        var players = lastUpdateFrame["players"];
        tempNames = [];
        for (let i = 0; i < players.length; i++) {
            let userFrame = players[i];
            tempNames.push(userFrame["username"]);
            if (!playerLookup.has(userFrame["username"])) {
                playerLookup.set(userFrame["username"], new Player(userFrame["username"], userFrame["avatar"]));
            }
            // update the players frame
            playerLookup.get(userFrame["username"]).updateFrame(userFrame);
            if (userFrame["username"] == userName) {
                curPlayer = playerLookup.get(userFrame["username"]);
                curPlayer.setAsPlayer();
            }
        }
        // lost the player???
        if (curPlayer == null) return;

        var centerx = canvas.width / 2;
        var centery = canvas.height / 2;
        var offsetx = centerx - curPlayer.X;
        var offsety = centery - curPlayer.Y;
        // get the map drawn
        currentMap.draw(offsetx, offsety);
        drawList = [];
        // draw the players
        var playersToRemove = [];
        for (const p of playerLookup.values()) {
            if (!tempNames.includes(p.Id)) {
                playersToRemove.push(p.Id);
                continue;
            }
            //p.draw(offsetx, offsety);
            drawList.push(p);
        };
        // remove players that are not on the map any more.
        for (let i = 0; i < playersToRemove.length; i++) {
            playerLookup.delete(playerLookup[i]);
        }
        // reorder the draw list
        drawList = drawList.sort((firstEl, secondEl) => {
            if (firstEl.Y < secondEl.Y) {
                return -1;
            }
            return 1;
        }); 
        // draw everythign
        drawList.forEach((d) => {
            d.draw(offsetx, offsety);
        })
    }
}

const addToUsersBox = (userName) => {
    if (!!document.querySelector(`.${userName}-userlist`)) {
        return;
    }

    const userBox = `<h5 class="chat_ib ${userName}-userlist" onclick="tellUser('${userName}')">${userName}</h5>`;
    players.innerHTML += userBox;
};

//===================== Messaging =======================
const addNewMessage = ({ user, message }) => {
    const time = new Date();
    const formattedTime = time.toLocaleString("en-US", { hour: "numeric", minute: "numeric" });

    const receivedMsg = `
  <div class="incoming__message">
    <div class="received__message">
      <div class="message__info">
        <span class="message__author">${user}: </span>
        <span class="time_date">${formattedTime}</span>
      </div>
      <p>${message}</p>
    </div>
  </div>`;

    const myMsg = `
  <div class="outgoing__message">
    <div class="sent__message">
      <div class="message__info">
        <span class="message__author">You: </span>
        <span class="time_date">${formattedTime}</span>
      </div>
      <p>${message}</p>
    </div>
  </div>`;

    messageBox.innerHTML += user === userName ? myMsg : receivedMsg;
};

const addNewPrivateMessage = ({ user, privateMessage }) => {
    const time = new Date();
    const formattedTime = time.toLocaleString("en-US", { hour: "numeric", minute: "numeric" });
    if (user != userName) {
        lastReply = user;
    }
    const receivedMsg = `
  <div class="private__message">
    <div class="received__message">
      <div class="message__info">
        <span class="message__author">${user}: </span>
        <span class="time_date">${formattedTime}</span>
      </div>
      <p>${privateMessage}</p>
    </div>
  </div>`;

    const myMsg = `
  <div class="private__message">
    <div class="sent__message">
      <div class="message__info">
        <span class="message__author">You think to yourself: </span>
        <span class="time_date">${formattedTime}</span>
      </div>
      <p>${privateMessage}</p>
    </div>
  </div>`;

    messageBox.innerHTML += user === userName ? myMsg : receivedMsg;
};

const addNewServerMessage = ({ user, serverMessage }) => {
    const time = new Date();
    const formattedTime = time.toLocaleString("en-US", { hour: "numeric", minute: "numeric" });

    const receivedMsg = `
  <div class="server__message">
    <div class="received__message">
      <div class="message__info">
        <span class="message__author">${user}: </span>
        <span class="time_date">${formattedTime}</span>
      </div>
      <p>${serverMessage}</p>
    </div>
  </div>`;

    messageBox.innerHTML += receivedMsg;
};

// send user message
messageForm.addEventListener("submit", (e) => {
    inputField.blur();
    e.preventDefault();
    if (!inputField.value) {
        return;
    }
    messages.unshift(inputField.value);
    msgPos = -1;
    if (messages.length > 50) {
        messages.pop();
    }

    let sendData = {
        message: inputField.value,
    };
    if (inputField.value.startsWith("/rt ")) {
        sendData = {
            privateMessage: inputField.value.slice(4),
            reciver: lastTell,
        };
    } else if (inputField.value.startsWith("/r ")) {
        sendData = {
            privateMessage: inputField.value.slice(3),
            reciver: lastReply,
        };
    } else if (inputField.value.startsWith("/t ")) {
        lastTell = inputField.value.substr(3, inputField.value.indexOf(',') - 3);
        sendData = {
            privateMessage: inputField.value.slice(inputField.value.indexOf(',') + 1).trimStart(),
            reciver: lastTell,
        };
    }
    //socket.emit("chat message", sendData,);
    
    myWebSocket.send(JSON.stringify(sendData));

    inputField.value = "";
});

function tellUser(user) {
    inputField.focus();
    inputField.value = "";
    inputField.value = "/t " + user + ",";
}






connectToWS();
startAnimating(60);


