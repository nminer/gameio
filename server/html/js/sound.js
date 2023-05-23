let playing = true; // set to true once sounds are playing.
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