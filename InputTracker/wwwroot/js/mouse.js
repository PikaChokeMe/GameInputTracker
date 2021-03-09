"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/mouseHub").build();

connection.on("RecieveKeyState", function (keyName, isActive) {
    //console.log("About to set #" + keyName + "to state: " + isActive);

    var key = $('#' + keyName);
    if (key) {
        if (isActive) {
            key.addClass('active');
        } else {
            key.removeClass('active');
        }
    }
});

connection.start().then(function () {
    console.log("Connecting...");
}).catch(function (err) {
    return console.error(err.toString());
});