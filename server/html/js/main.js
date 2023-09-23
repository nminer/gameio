const socket = io();

const inboxPeople = document.querySelector(".inbox__people");
const players = document.querySelector("#players");
const inputField = document.querySelector(".message_form__input");
const messageForm = document.querySelector(".message_form");

const fallback = document.querySelector(".fallback");

// your user name.
let userName = "";

//the last user to private message.
let lastReply = "";
let lastTell = "";

// keep all the sent messages in.
let messages = [];
// message search posision.
let msgPos = 0;

const addToUsersBox = (userName) => {
    if (!!document.querySelector(`.${userName}-userlist`)) {
        return;
    }

    const userBox = `<h5 class="chat_ib ${userName}-userlist" onclick="tellUser('${userName}')">${userName}</h5>`;
    players.innerHTML += userBox;
};

//////////////////////////////////////////////////////////////////////
socket.on("new user", function (data) {
    data.map((user) => addToUsersBox(user));
});

socket.on("user disconnected", function (userName) {
    document.querySelector(`.${userName}-userlist`).remove();
});

socket.on("chat message", function (data) {
    addNewMessage({ user: data.user, message: data.message });
});

socket.on("private message", function (data) {
    if (data.nick != userName) {
        lastReply = data.user;
    }
    addNewPrivateMessage({ user: data.user, message: data.message });
});

socket.on("server message", function (data) {
    addNewServerMessage({ user: data.user, message: data.message });
});

socket.on("typing", function (data) {
    const { isTyping, nick } = data;

    if (!isTyping) {
        fallback.innerHTML = "";
        return;
    }

    fallback.innerHTML = `<p>${nick} is typing...</p>`;
});

// server will ahve closed socket connection so need to reload page.
// this will force user to relog in.
socket.on("login", function (data) {
    location.reload();
});

socket.on("connected", function (data) {
    userName = data.user;
});

let townsounds =
    [
        { file: 'town1.wav', location: 'sounds/town/', duration: 3 },
        { file: 'town2.wav', location: 'sounds/town/', duration: 1 },
        { file: 'town3.wav', location: 'sounds/town/', duration: 1 },
        { file: 'town4.wav', location: 'sounds/town/', duration: 4 },
        { file: 'town5.wav', location: 'sounds/town/', duration: 2 },
        { file: 'town6.wav', location: 'sounds/town/', duration: 4 },
        { file: 'town7.wav', location: 'sounds/town/', duration: 6 },
        { file: 'town8.wav', location: 'sounds/town/', duration: 5 },
        { file: 'town9.wav', location: 'sounds/town/', duration: 5 },
        { file: 'town10.wav', location: 'sounds/town/', duration: 1 },
        { file: 'town11.wav', location: 'sounds/town/', duration: 1 },
        { file: 'town12.wav', location: 'sounds/town/', duration: 1 },
        { file: 'town13.wav', location: 'sounds/town/', duration: 1 },

    ];

let backgoundsounds =
    [
        { file: 'Rejoicing.mp3', location: 'sounds/town/', duration: 103 },
        { file: 'Ale-and-Anecdotes.mp3', location: 'sounds/town/', duration: 145 },
    ];

payListOfSounds(townsounds, -1, true, true);
payListOfSounds(townsounds, -1, true, true);
payListOfSounds(backgoundsounds, -1, true, false);

function tellUser(user) {
    inputField.focus();
    inputField.value = "";
    inputField.value = "/t " + user + ",";   
}