let playing = true; // set to true once sounds are playing.

// sounds that are placed on a map.
let mapSounds = [];

// a list of sound affects to play.
// sound affects play one time and then are removed.
let soundAffects = [];

let fullMapSounds = [];

/**
 * stop and removes all the map sounds.
 */
function clearMapSounds() {
    mapSounds.forEach((s) => {
         s.pause();
    });
    mapSounds.length = 0;
    soundAffects.forEach((s) => {
        s.pause();
    });
    soundAffects.length = 0;
    fullMapSounds.forEach((s) => {
        s.pause();
    });
    fullMapSounds.length = 0;
}

function addMapSound(mapSoundToAdd) {
    mapSounds.push(mapSoundToAdd);
}

function addSoundAffect(mapSoundToAdd) {
    if (playing) {
        mapSoundToAdd.loadSound();
        soundAffects.push(mapSoundToAdd);
    }
}

function addFullMapSound(fullMapSound) {
    if (playing) {
        fullMapSound.loadSound();
        fullMapSounds.push(mapSounds);
    }
}

class MapSound {

    constructor(soundFileStr, repeat, mapX, mapY, fullVolumeRadius, fadeVolumeRadius, soundAffect = false) {
        this.soundFileStr = soundFileStr;
        this.repeatLoop = repeat;
        this.mapX = mapX;
        this.mapY = mapY;
        this.fullVolumeRadius = fullVolumeRadius;
        this.fadeVolumeRadius = fadeVolumeRadius;
        this.startLoading = false;
        this.soundAffect = soundAffect;
    }

    setSound(audioObject) {
        this.sound = audioObject;
        this.sound.loop = this.repeatLoop;
        if (this.soundAffect) {
            if (playing) {
                this.play();
            } else {
                this.pause();
            }
        }
    }

    pause() {
        if (this.sound !== undefined) {
            this.sound.pause();
        }       
    }

    play() {
        if (this.sound !== undefined && playing) {
            this.sound.play();
        }
    }

    isPlaying() {
        if (this.sound !== undefined) {
            return !this.sound.paused;
        }
        return false;
    }

    // returns true if the sound was loaded and finished playing
    finishedPlaying() {
        return this.sound !== undefined && this.sound.paused;
    }

    loadSound() {
        if (this.sound === undefined && !this.startLoading) {
            this.startLoading = true;
            SoundLoader.GetSound(this.soundFileStr, this);
        }
    }

    checkDistance(x, y) {
        if (!playing) { return; }
        let d = this.dist(x, y, this.mapX, this.mapY);
        if (this.sound === undefined && !this.startLoading && d <= this.fadeVolumeRadius + 70) {
            this.loadSound();
        }       
        if (this.sound === undefined) {
            return;
        }      
        if (d <= this.fullVolumeRadius) {
            this.sound.volume = 1;
        } else if (d < this.fadeVolumeRadius) {
            let first = d - this.fullVolumeRadius;
            let second = this.fadeVolumeRadius - this.fullVolumeRadius;
            let temp = first / second;
            let volume = 1 - temp;
            this.sound.volume = volume;
        } else {
            this.sound.volume = 0;
        }
        if (d <= this.fullVolumeRadius || d < this.fadeVolumeRadius) {
            if (!this.isPlaying()) {
                this.sound.play()
            }
        } else if (!this.soundAffect) {
            if (this.isPlaying()) {
                this.sound.pause()
            }
        }
    }

    dist(x1, y1, x2, y2) {
        //console.log(x1+","+y1+" "+x2+","+y2)
        const re = Math.sqrt((x1 - x2) ** 2 + (y1 - y2) ** 2);
        //console.log(re);
        return Math.floor(re);
    }
}

function checkAllMapSounds(playerX, playerY) {
    for (let i = 0; i < mapSounds.length; i++) {
        let sound = mapSounds[i];
        sound.checkDistance(playerX, playerY);
    }
    soundAffects = soundAffects.filter(function (s) {
        return !s.finishedPlaying()
    });
    for (let i = 0; i < soundAffects.length; i++) {
        let sound = soundAffects[i];
        sound.checkDistance(playerX, playerY);
    }
    fullMapSounds = fullMapSounds.filter(function (s) {
        return !s.finishedPlaying()
    });
}
//========================== full map sound ===============================
class fullMapSound {

    constructor(soundFileStr, repeat, volume = 1) {
        this.soundFileStr = soundFileStr;
        this.repeatLoop = repeat;
        this.volume = volume;
        this.startLoading = false;
    }

    setSound(audioObject) {
        this.sound = audioObject;
        this.sound.loop = this.repeatLoop;
        this.sound.volume = this.volume;
        if (playing) {       
            this.play();
        } else {
            this.pause();
        }
    }

    pause() {
        if (this.sound !== undefined) {
            this.sound.pause();
        }
    }

    play() {
        if (this.sound !== undefined && playing) {
            this.sound.play();
        }
    }

    isPlaying() {
        if (this.sound !== undefined) {
            return !this.sound.paused;
        }
        return false;
    }

    // returns true if the sound was loaded and finished playing
    finishedPlaying() {
        return this.sound !== undefined && this.sound.paused;
    }

    loadSound() {
        if (this.sound === undefined && !this.startLoading) {
            this.startLoading = true;
            SoundLoader.GetSound(this.soundFileStr, this);
        }
    }
}

//======================== stormSounds ===================================
class StormSounds {
    constructor() {
        this.startStopSound = new MapSound('sounds/outside/stop-rain.mp3', true, 0, 0, 0, 0);
        this.startStopSound.loadSound();
        this.lightSound = new MapSound('sounds/outside/light-rain.mp3', true, 0, 0, 0);
        this.lightSound.loadSound();
        this.midSound = new MapSound('sounds/outside/mid-rain.mp3', true, 0, 0, 0, 0);
        this.midSound.loadSound();
        this.heavySound = new MapSound('sounds/outside/heavy-rain.mp3', true, 0, 0, 0, 0);
        this.heavySound.loadSound();
    }

    setStorm(amount) {
        if (amount < 10) {
            // start or stopping rain.
            this.startStopSound.play();
            this.lightSound.pause();
            this.midSound.pause();
            this.heavySound.pause();
        } else if (amount >= 1 && amount < 60) {
            // light rain
            this.startStopSound.pause();
            this.lightSound.play();
            this.midSound.pause();
            this.heavySound.pause();
        } else if (amount >= 60 && amount < 100) { 
            //mid rain.
            this.startStopSound.pause();
            this.lightSound.pause();
            this.midSound.play();
            this.heavySound.pause();
        } else if (amount >= 100) {
            // heavy rain.
            this.startStopSound.pause();
            this.lightSound.pause();
            this.midSound.pause();
            this.heavySound.play();
        }
        
    }
}
let stormSounds = null;
function setStormSounds(amount) {
    if (stormSounds == null) {
        stormSounds = new StormSounds()
    }
    stormSounds.setStorm(amount);
}
//=============================== baackground sounds =====================
// this is for back ground sounds.
let playingList = [];


/**
 * 
 * Takes a array of sounds {file, location, duration}
 * and plays them randomly and repetedly
 * 
 * @param {any} sounds sounds to play
 * @param {any} lastPlayed last played index. start with -1 
 * @param {any} repeat  true if want to keep calling random noises.
 * @param {any} randomVolume true if want random value for noise files.
 */
function payListOfSounds(sounds, lastPlayed, loaddedsounds, repeat, randomVolume, sacaleVolume = -1) {
    let max = sounds.length;
    let r = Math.floor(Math.random() * max);
    while (r == lastPlayed) {
        r = Math.floor(Math.random() * max);
    }
    lastPlayed = r;
    if (loaddedsounds[r] == null) {
        let name = sounds[r].location + sounds[r].file;
        let audio = new Audio(name);
        loaddedsounds[r] = audio;
        if (repeat) {
            audio.addEventListener("ended", function () {
                payListOfSounds(sounds, lastPlayed, loaddedsounds, repeat, randomVolume, sacaleVolume);
                let index = playingList.indexOf(audio);
                if (index > -1) {
                    playingList.splice(index, 1);
                }
            });
        }
    }
    let audio = loaddedsounds[r];
    audio
    if (sacaleVolume != -1) {
        audio.volume = sacaleVolume;
    }
    if (randomVolume) {
        if (sacaleVolume != -1) {
            audio.volume = Math.random() * sacaleVolume;
        } else {
            audio.volume = Math.random();
        }
    }
    audio.currentTime = 0;
    let p = audio.play();
    if (p !== undefined) {
        if (!playing) {
            audio.pause();
        }
        playingList.push(audio);
        p.catch(_ => {
            setTimeout(function () { payListOfSounds(sounds, lastPlayed, loaddedsounds, repeat, randomVolume, sacaleVolume); }, 1000);
            let index = playingList.indexOf(audio);
            if (index > -1) {
                playingList.splice(index, 1);
            }
        });
    }
}

function soundonoff(saveSetting = true) {
    playing = !playing;
    playingList.forEach((s) => {
        if (!playing) {
            s.pause();
        } else {
            s.play();
        }
    });
    mapSounds.forEach((s) => {
        if (!playing) {
            s.pause();
        } else {
            s.play();
        }
    });
    soundAffects.forEach((s) => {
        if (!playing) {
            s.pause();
        } else {
            s.play();
        }
    });
    fullMapSounds.forEach((s) => {
        if (!playing) {
            s.pause();
        } else {
            s.play();
        }
    });
    const soff = document.getElementById("soundoff");
    const son = document.getElementById("soundon");
    if (!playing) {
        soff.style.display = 'block';
        son.style.display = 'none';
    } else {
        son.style.display = 'block';
        soff.style.display = 'none';
    }
}
//==================== sound loader =================================
/**
 * the image loader holds all the loaded sounds
 */
class SoundLoader {
    /**
     * key path to image, value is the loaded image.
     */
    static LoadedSounds = new Map();

    /**
     * 
     * @param {string} path the path to the image
     */
    static GetSound(path, toSetSoundOn) {
        if (SoundLoader.LoadedSounds.has(path)) {
            let temp = new Audio()
            temp.src = SoundLoader.LoadedSounds.get(path);
            toSetSoundOn.setSound(temp);
            return;
        }
        var xhr = new XMLHttpRequest();
        xhr.responseType = "blob";//xhr.responseType = "ArrayBuffer";
        xhr.open("GET", path);
        xhr.send();
        xhr.addEventListener("load", function () {
            SoundLoader.LoadedSounds.set(path, URL.createObjectURL(xhr.response));
            let temp = new Audio()
            temp.src = SoundLoader.LoadedSounds.get(path);
            toSetSoundOn.setSound(temp);
        });
    }
}



