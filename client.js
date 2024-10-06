var webSocket = new WebSocket("ws://localhost:8888/")
var isConneted = false;
var isProcessing = false;
const CHUNK_SIZE = 1024;

webSocket.onopen = function(event) {
    console.log('Client connects to server via websocket.');
    isConneted = true;
};

webSocket.onclose = function(event) {
    let errorMessage;
    if (event.wasClean) {
        errorMessage = `Connection closed, code=${event.code} couse=${event.reason}.`;
    } else {
        errorMessage = 'Connection interrupted.';
    }

    console.error(errorMessage);
    isConneted = false;
};

webSocket.onmessage = function(event) {
    loadOutputImg(event.data);
    isProcessing = false;
}
  
webSocket.onerror = function(error) {
    console.error(`Websocket error: ${error}`);
    isConneted = false;
};

document.getElementById("img-form").addEventListener("submit", function(event) {
    event.preventDefault();
    if(isConneted && !isProcessing)
    {
        output("")
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
                isProcessing = true;
            }
        };  
        reader.readAsArrayBuffer(file);

        outputImageBox(imageBox);
        loadInputImg(file)
    }
    else if (!isConneted)
    {
        output("No connection with websocket. Try again later.")
    }
    else
    {
        output("The image is being processed, please wait.")
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