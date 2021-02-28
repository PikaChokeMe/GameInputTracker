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
    var key = $('#' + keyName);
    if (key) {
        if (isActive) {
            key.addClass('active');
        } else {
            key.removeClass('active');
        }
    }
}

$.connection.mouseHub.client.highlightKey = function (keyName, isActive) {
    var key = $('#' + keyName);
    if (key) {
        if (isActive) {
            key.addClass('active');
        } else {
            key.removeClass('active');
        }
    }
}

$.connection.mouseHub.client.setAngle = function (angle) {
    console.log('set angles to:' + angle);
    $('#pointer').css('transform', `rotate(${angle}deg)`)
}