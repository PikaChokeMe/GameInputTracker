$.connection.hub.start()
    .done((done) => {
        console.log('Connection Online')

        // $.connection.keyboardHub.server.
    })
    .fail((err) => console.log('Connection Error!' + err));

$.connection.keyboardHub.client.heartbeat = function () {
    console.log('beep');
}

$.connection.keyboardHub.client.highlightKey = function (keyName, isActive) {
    //console.log(keyName);
    //console.log('key ' + keyName + ' is ' + (isActive ? 'up' : 'down'));

    var key = $('#' + keyName);
    if (key) {
        if (isActive) {
            key.addClass('active');
        } else {
            key.removeClass('active');
        }
    }
}