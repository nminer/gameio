// Natalie's and Nathan's game
//import playerImg from '../img/player/maincharacter.png';
const canvas = document.querySelector('#mainGame');
const c = canvas.getContext('2d');

// set up our canvas to be full width and hight
canvas.width = innerWidth;
canvas.height = innerHeight;

var myWebSocket;

function connectToWS() {
    var endpoint = "ws://localhost:9000/";
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
        console.log("onmessage. size: " + leng + ", content: " + event.data);
    }

    myWebSocket.onopen = function (evt) {
        console.log("onopen.");
    };

    myWebSocket.onclose = function (evt) {
        console.log("onclose.");
    };

    myWebSocket.onerror = function (evt) {
        console.log("Error!");
    };
}

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
    switch (keyCode) {
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
});

addEventListener('keyup', (event) => {
    var keyCode = event.keyCode;
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
            //keys.hit = false
            break;
        case 69: //e ,  use objects
            keys.use = false
    }
});


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
        myWebSocket.send(JSON.stringify(keys));

    }
}

connectToWS();
startAnimating(60);