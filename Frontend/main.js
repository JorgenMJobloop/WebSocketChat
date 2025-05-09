// required JavaScript DOM elements
const chat = document.getElementById("chat");
const message = document.getElementById("message");
const sendButton = document.getElementById("send-message");

// default connection for the Websocket server running on the backend.
const websocket = new WebSocket("ws://localhost:5000/chat");


websocket.onmessage = (event) => {
    printMessageInFrontendChat(event);
};

function printMessageInFrontendChat(event) {
    const element = document.createElement("div");
    element.textContent = event.data;
    chat.appendChild(element);
    chat.scrollTop = chat.scrollHeight;
}

sendButton.addEventListener("click", () => {
    if (message.value.trim()) {
        websocket.send(message.value);
        message.value = "";
    }
});

message.addEventListener("keyup", (e) => {
    if (e.key == "Enter") {
        sendButton.click();
    }
});