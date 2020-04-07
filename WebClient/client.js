connection = new WebSocket('ws://127.0.0.1:9090/ws');  
 
connection.onopen = function () { 
    console.log("Connected"); 
 };


 connection.onclose = function (event) {
    if (event.wasClean) {
        console.log('Disconnected.');
    } else {
        console.log('Connection lost.'); 
    }
    console.log('Code: ' + event.code + '. Reason: ' + event.reason);
};

connection.onmessage = function (event) {
    console.log("Data received: " + event.data);


};

   
 connection.onerror = function (err) { 
    console.log("Got error", err); 
 };
 


var messageInput = document.querySelector('#messageInput'); 


sendButton.addEventListener("click", function(event) {
  
    var message = messageInput.value;
    send(message);
});


function send(message) { 
   
    connection.send(JSON.stringify(message)); 
 };