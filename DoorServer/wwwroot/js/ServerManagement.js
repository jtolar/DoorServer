"use strict";

var connection = new signalR
    .HubConnectionBuilder()
    .configureLogging(signalR.LogLevel.Information)
    .withUrl("/servermanagement")
    .build();

//Disable the send button until connection is established.
document.getElementById("StartStopButton").disabled = true;

connection.on("ServerStatusUpdate", function (message) {
    document.getElementById("rloginStatus").textContent = message;
    // We can assign user-supplied strings to an element's textContent because it
    // is not interpreted as markup. If you're assigning in any other way, you 
    // should be aware of possible script injection concerns.
    //li.textContent = `${user} says ${message}`;
});

connection.on("RloginServerStatus", function (status) {
    if (status == "Started") {
        document.getElementById("StartStopButton").textContent = "Stop Server";
    }
    else {
        document.getElementById("StartStopButton").textContent = "Start Server";
    };
})

connection.start().then(function () {
    document.getElementById("StartStopButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("StartStopButton").addEventListener("click", function (event) {
    connection.invoke("ToggleServerStartStop").catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});