let nightsounds =
    [
        { file: 'bird.wav', location: 'sounds/login/' },
        { file: 'bird2.wav', location: 'sounds/login/' },
        { file: 'crow.wav', location: 'sounds/login/' },
        { file: 'night1.wav', location: 'sounds/login/' },
        { file: 'night2.wav', location: 'sounds/login/' },
        { file: 'night3.wav', location: 'sounds/login/' },
        { file: 'thunder.wav', location: 'sounds/login/' },
    ];

let nightsounds2 =
    [
        { file: 'night3.wav', location: 'sounds/login/' },
        { file: 'night2.wav', location: 'sounds/login/' },
        { file: 'frog.wav', location: 'sounds/login/' },
        { file: 'night2.wav', location: 'sounds/login/' },
    ];

let backgoundsounds =
    [
        { file: 'campfire.mp3', location: 'sounds/login/' },
        { file: 'campfire.mp3', location: 'sounds/login/' },
    ];

payListOfSounds(nightsounds, -1, Array.apply(null, Array(nightsounds.length)), true, true, .3);
payListOfSounds(nightsounds2, -1, Array.apply(null, Array(nightsounds2.length)), true, true, .2);
payListOfSounds(backgoundsounds, -1, Array.apply(null, Array(backgoundsounds.length)), true, false);