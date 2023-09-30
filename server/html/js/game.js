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

// a list of mapAnimations that should playonly one tmie and then becleared.
var visualEffects = [];

var lightningStrikes = [];

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
            addAllSoundAffects(data["frame"]);
            addAllFullMapSounds(data["frame"]);
            addAllVisualEffects(data["frame"]);
            addAllLightning(data["frame"]);
            addAllDamages(data["frame"]);
            addAllMonsters(data["frame"]);
        } else if (data.hasOwnProperty("mapName")) {
            loadMap(data);
        } else if (data.hasOwnProperty("monsterToLoad")) {
            loadMonsterType(data["monsterToLoad"]); // load a monster type
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

function addAllSoundAffects(data) {
    var soundsToLoad = data["soundAffects"];
    for (let i = 0; i < soundsToLoad.length; i++) {
        var snd = soundsToLoad[i];
        addSoundAffect(new MapSound(snd["path"], snd["repeat"], snd["x"], snd["y"], snd["fullRadius"], snd["fadeRadius"], true));
    }
}

function addAllFullMapSounds(data) {
    var soundsToLoad = data["fullMapSoundEffects"];
    for (let i = 0; i < soundsToLoad.length; i++) {
        var snd = soundsToLoad[i];
        addFullMapSound(new fullMapSound(snd["path"], snd["repeat"], snd["volume"]));
    }
}
function addAllLightning(data) {
    if ('lightning' in data) { 
        var lightning = data["lightning"];
        addLightning(new Lightning(lightning["amount"]));
    }
}

function addAllVisualEffects(data) {
    var visualsToLoad = data["visualEffects"];
    for (let i = 0; i < visualsToLoad.length; i++) {
        var vis = visualsToLoad[i];
        addVisualEffect(new MapAnimation(vis["path"], vis["x"], vis["y"], vis["frameCount"], vis["frameX"], vis["frameY"], vis["width"], vis["height"], vis["slowDown"], vis["horizontal"], 0, vis["drawOrder"]));
    }
}

function addAllDamages(data) {
    var damageToAdd = data["damages"];
    for (let i = 0; i < damageToAdd.length; i++) {
        var dmg = damageToAdd[i];
        damages.push(new Damage(dmg["x"], dmg["y"], dmg["amount"], dmg["r"], dmg["g"], dmg["b"]));
    }
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

/**
 * returns a random integer from low to high value.
 * @param {Number} high 
 * @param {Number} low 
 * @returns Number
 */
function randomInt(low, high) {
    return Math.floor(Math.random() * (high + 1 - low) + low);
}


function addVisualEffect(mapAnimation) {
    visualEffects.push(mapAnimation);
}

function checkAllVisualEffects() {
    visualEffects = visualEffects.filter(function (s) {
        return !s.finshed;
    });
}

function addLightning(lightning) {
    lightningStrikes.push(lightning);
}

function checkAllLightningStrikes() {
    lightningStrikes = lightningStrikes.filter(function (s) {
        return !s.finshed;
    });
}

function convertRange(oldMin, oldMax, newMin, newMax, oldValue) {
    let oldRange = oldMax - oldMin;
    let newRange = newMax - newMin;
    let newValue = (((oldValue - oldMin) * newRange) / oldRange) + newMin;
    if (newValue > newMax) {
        newValue = newMax;
    }
    if (newValue < newMin) {
        newValue = newMin;
    }
    return newValue;
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

let currentMap = null;
// player name to player objects for current map.
// name:Player
const playerLookup = new Map();

const mapImages = [];
const mapAnimations = [];
const damages = [];
const mapLights = [];

function loadMap(data) {
    //var worldbackground = new Image();
    //worldbackground.src = "./" + data["image"];
    var worldbackground = ImageLoader.GetImage("./" + data["image"]);
    var height = data["height"];
    var width = data["width"];
    currentMap = new GameMap(width, height, worldbackground);
    playerLookup.clear();
    mapImages.length = 0;
    mapAnimations.length = 0;
    damages.length = 0;
    clearMapSounds();
    visualEffects.length = 0;
    mapLights.length = 0;
    var imagesToLoad = data["mapImages"];
    for (let i = 0; i < imagesToLoad.length; i++) {
        var img = imagesToLoad[i];
        mapImages.push(new MapImage(img["width"], img["height"], img["path"], img["x"], img["y"], img["drawOrder"]));
    }
    var animaationsToLoad = data["mapAnimations"];
    for (let i = 0; i < animaationsToLoad.length; i++) {
        var img = animaationsToLoad[i];
        //(image, mapx, mapy, frames, imagex, imagey, width, height, slowdown, startFram = 0, draworder = 0)
        mapAnimations.push(new MapAnimation(img["path"], img["x"], img["y"], img["frameCount"], img["frameX"], img["frameY"], img["width"], img["height"], img["slowDown"], img["horizontal"], img["firstFrame"], img["drawOrder"]));
    } 
    var soundsToLoad = data["mapSounds"];
    for (let i = 0; i < soundsToLoad.length; i++) {
        var snd = soundsToLoad[i];
        addMapSound(new MapSound(snd["path"], snd["repeat"], snd["x"], snd["y"], snd["fullRadius"], snd["fadeRadius"]));
    }
    var lightsToLoad = data["mapLights"];
    for (let i = 0; i < lightsToLoad.length; i++) {
        var ml = lightsToLoad[i];
        mapLights.push(ml);
    }
}

function darken(x, y, w, h, darkenColor, amount) {
    c.fillStyle = darkenColor;
    c.globalAlpha = amount;
    c.fillRect(x, y, w, h);
    c.globalAlpha = 1;
}

function lighten(x, y, w, h, lightColor, amount) {
    c.save();
    c.globalAlpha = amount;
    c.globalCompositeOperation = 'lighter';
    c.fillStyle = lightColor;
    c.globalAlpha = amount;
    c.fillRect(x, y, w, h);
    c.restore();
}

function ligthenGradient(x, y, offsetx, offsety, radius, mainColor, midColor, amount) {
    var newx = x + offsetx;
    var newy = y + offsety;
    c.save();
    c.globalAlpha = amount;
    c.globalCompositeOperation = 'lighter';
    var rnd = 0.05 * Math.sin(1.1 * Date.now() / 1000);
    radius = radius * (1 + rnd);
    var radialGradient = c.createRadialGradient(newx, newy, 0, newx, newy, radius);
    radialGradient.addColorStop(0.0, mainColor);
    //radialGradient.addColorStop(0.2 + rnd, '#AA8');
    //radialGradient.addColorStop(0.7 + rnd, '#330');
    radialGradient.addColorStop(0.50, midColor);
    radialGradient.addColorStop(1, '#000');
    c.fillStyle = radialGradient;
    c.beginPath();
    c.arc(newx, newy, radius, 0, 2 * Math.PI);
    c.fill();
    c.restore();
}

//================================= Main animation ======================================
var stop = false;
var frameCount = 0;
var fps, fpsInterval, startTime, now, then, elapsed;
var topPlayerBar = new topbar();
var storm = new Storm();
storm.setRainAmount(0);
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
        if (players === undefined) {
            return;
        }
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
        currentMap.draw(c, offsetx, offsety);
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
            playerLookup.delete(playersToRemove[i]);
        }
        for (const m of mapMonsters.values()) {
            drawList.push(m);
        }
        // add map images to the draw list
        for (let i = 0; i < mapImages.length; i++) {
            drawList.push(mapImages[i]);
        }
        // add all animations.   
        for (let i = 0; i < mapAnimations.length; i++) {
            drawList.push(mapAnimations[i]);
        }
        // add all visual effects
        for (let i = 0; i < visualEffects.length; i++) {
            drawList.push(visualEffects[i]);
        }
        // add all damages
        for (let i = 0; i < damages.length; i++) {
            drawList.push(damages[i]);
        }
        // reorder the draw list
        drawList = drawList.sort((firstEl, secondEl) => {
            if (firstEl.drawOrder < secondEl.drawOrder) {
                return -1;
            }
            return 1;
        }); 
        // draw everything
        drawList.forEach((d) => {
            d.draw(c, offsetx, offsety);
        })
        // time of day
        // set how dark the sky is.
        darken(0, 0, canvas.width, canvas.height, lastUpdateFrame["sky"]['color'], lastUpdateFrame["sky"]['amount']);
        // add in all the map lights.
        for (let i = 0; i < mapLights.length; i++) {
            var mapLight = mapLights[i];
            var lightAmount = mapLight['amount'];
            if (lightAmount < 0) {
                lightAmount = lastUpdateFrame["sky"]['amount'];
            }
            ligthenGradient(mapLight['x'], mapLight['y'], offsetx, offsety, mapLight['radius'], mapLight['mainColor'], mapLight['midColor'], lightAmount);
        }
        storm.setRainAmount(lastUpdateFrame["storm"]['amount']);
        storm.draw(c, offsetx, offsety);
        for (let i = 0; i < lightningStrikes.length; i++) {
            var l = lightningStrikes[i];
            l.draw();
        }
        // draw all monster names
        for (const m of mapMonsters.values()) {
            m.drawName(c, offsetx, offsety);
        }
        // draw player names
        for (const p of playerLookup.values()) {
            p.drawName(c, offsetx, offsety);
        }
        // gui
        topPlayerBar.draw(curPlayer);
        // check for map sounds.
        checkAllMapSounds(curPlayer.X, curPlayer.Y);
        checkAllVisualEffects();
        checkAllLightningStrikes();
    }
}

//==============================================================================================

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
    const myRe = /Location \- \((.*)\)/g;
    const myArray = myRe.exec(serverMessage);
    if (myArray !== null) {
        navigator.clipboard.writeText(myArray[1]);
    }
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
    } else if (inputField.value.startsWith("/")) {
        sendData = {
            command: inputField.value
        };
    }
    //socket.emit("chat message", sendData,);
    
    myWebSocket.send(JSON.stringify(sendData));

    inputField.value = "";
});

// send a request for a monster type.
function requestMonster(type) {
    sendData = {
        monsterRequest: type
    };
    myWebSocket.send(JSON.stringify(sendData));
}

function tellUser(user) {
    inputField.focus();
    inputField.value = "";
    inputField.value = "/t " + user + ",";
}


connectToWS();
startAnimating(60);


