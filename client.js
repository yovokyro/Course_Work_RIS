var webSocket = new WebSocket("ws://localhost:8888/")
var conneted = false;
const CHUNK_SIZE = 1024;

webSocket.onopen = function(event) {
    console.log('Client connects to server via websocket.');
    conneted = true;
};

webSocket.onclose = function(event) {
    let errorMessage;
    if (event.wasClean) {
        errorMessage = `Connection closed, code=${event.code} couse=${event.reason}.`;
    } else {
        errorMessage = 'Connection interrupted.';
    }

    console.error(errorMessage);
    conneted = false;
};

webSocket.onmessage = function(event) {
    loadOutputImg(event.data);
}
  
webSocket.onerror = function(error) {
    console.error(`Websocket error: ${error}`);
    conneted = false;
};

document.getElementById("img-form").addEventListener("submit", function(event) {
    event.preventDefault();
    if(conneted)
    {
        let fileInput = document.getElementById('file-input');
        let imageBox = document.getElementById('image-box');
        let file = fileInput.files[0];

        if (!file) {
            let errorMessage = 'File not selected or file is null.';
            console.error(errorMessage);
            output(errorMessage);
            return;
        }
        const reader = new FileReader();
    
        reader.onload = function(event) {
            if (event.target.readyState === FileReader.DONE) {
                const bytes = new Uint8Array(event.target.result);
                webSocket.send(bytes);
            }
        };  
        reader.readAsArrayBuffer(file);

        outputImageBox(imageBox);
        loadInputImg(file)
    }
    else
    {
        output("No connection with websocket. Try again later.")
    }
});

function output(message) {
    let error = document.getElementById('error');
    error.textContent = message
}

function outputImageBox(imageBox) {
    imageBox.innerHTML = '<div><label>Input:</label><img id="image-input" alt="Loading..."/></div><div><label>Output:</label><img id="image-output" alt="Loading..."/></div>';
}

function loadInputImg(img) {
    let image = document.getElementById('image-input');
    var imageUrl = URL.createObjectURL(img);
    
    if(image)
    {
        image.src = imageUrl;
    }
}

function loadOutputImg(img) {
    let image = document.getElementById('image-output');
    var imageUrl = URL.createObjectURL(img);

    if(image)
    {
        image.src = imageUrl;
    }
}