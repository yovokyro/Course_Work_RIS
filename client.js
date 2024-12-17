//var webSocket = new WebSocket("ws://127.0.0.1/")

var webSocket = new WebSocket("ws://192.168.100.14:8888/")

var isConneted = false;
var isProcessing = false;

var start_time = 0;
var result_time = 0;

var thread = false;

const CHUNK_SIZE = 1024;

//Ивенты сокеты

webSocket.onopen = function (event) {
    console.log('Client connects to server via websocket.');
    isConneted = true;
};

webSocket.onclose = function (event) {
    let errorMessage;
    if (event.wasClean) {
        errorMessage = `Connection closed, code=${event.code} couse=${event.reason}.`;
    } else {
        errorMessage = 'Connection interrupted.';
    }

    if(isProcessing)
    {
        setStatus(-1);  
        isProcessing = false;
    }

    outputError(errorMessage);
    console.error(errorMessage);
    isConneted = false;
};

webSocket.onmessage = function (event) {
    try
    {
        const packet = JSON.parse(event.data);
        const image = packet.image;
        const server_time = parseFloat(packet.time);

        const binaryString = window.atob(image);
        const uintArray = new Uint8Array(binaryString.length);
        for (let i = 0; i < binaryString.length; i++) {
            uintArray[i] = binaryString.charCodeAt(i);
        }

        const blob = new Blob([uintArray], { type: 'image/png' });

        loadImage(blob, 'image-output');
        setStatus(1);
    
        const result_time = performance.now() - start_time;
        setTime(result_time, 'otchet-time-client');
        setTime(server_time, 'otchet-time-server');
        getThread('otchet-thread')

    } catch(error) {
        console.error(error);
        setStatus(-1);
    }
    
    isProcessing = false;
}

webSocket.onerror = function (error) {
    console.error(`Websocket error: ${error}`);
    if(isProcessing)
    {
        setStatus(-1);  
        isProcessing = false;
    }
    
    isConneted = false;
};

//Отправка данных

document.getElementById("img-form").addEventListener("submit", function (event) {
    event.preventDefault();

    if (!isConneted) {
        outputError("No connection with websocket. Try again later.");
        return;
    }

    if (isProcessing) {
        outputError("The image is being processed, please wait.");
        return;
    }

    const fileInput = document.getElementById('file-input');
    const file = fileInput.files[0];

    thread = document.getElementById('thread').checked;

    if (!file) {
        const errorMessage = 'File not selected or file is null.';
        console.error(errorMessage);
        outputError(errorMessage);
        return;
    }

    isProcessing = true;

    outputError("");
    createUI();
    setStatus(0);
    loadImage(file, 'image-input');
    sendFileChunks(file);
});

async function sendFileChunks(file) {
    start_time = performance.now()

    const fileArrayBuffer = await file.arrayBuffer();
    const totalSize = new Uint32Array(1);
    totalSize[0] = fileArrayBuffer.byteLength;
    let offset = 0;

    const checkBoxValue = thread ? 1 : 0;
    const dataBuffer = new ArrayBuffer(5); 
    const dataView = new DataView(dataBuffer);
    
    dataView.setUint32(0, totalSize[0], true);
    dataView.setUint8(4, checkBoxValue);

    webSocket.send(dataBuffer);

    while (offset < totalSize[0]) {
        const chunk = fileArrayBuffer.slice(offset, offset + CHUNK_SIZE);
        webSocket.send(chunk);
        offset += CHUNK_SIZE;
    }
}

//Работа с интерфейсом

function outputError(message) {
    const error = document.getElementById('error');
    error.textContent = message
}

function createUI() {
    const imageBox = document.getElementById('image-box');
    const otchet = document.getElementById('otchet')

    if(imageBox)
    {
        imageBox.innerHTML = '<div><label>Input:</label><img id="image-input" onclick="openImg(this)" alt="Loading..."/></div><div><label>Output:</label><img id="image-output" onclick="openImg(this)" alt="Loading..."/></div>';
    }

    if(otchet)
    {
        otchet.innerHTML = '<p><label><b>ОТЧЕТ</b> </label></p>' +
            '<p><label>Статус: <label id="otchet-status"></label></label></p>' +
            '<p><label>Режим: <label id="otchet-thread">-</label></label></p>' +
            '<p><label>Время обработки изображения: <label id="otchet-time-server">-</label></label></p>' +
            '<p><label>Общее время: <label id="otchet-time-client">-</label></label></p>'
    }
}

function loadImage(img, elementId) {
    const image = document.getElementById(elementId);
    if (image) {
        const imageUrl = URL.createObjectURL(img);
        image.src = imageUrl;
    }
}

//Методы установки данных в отчет

function setStatus(code) {
    const status = document.getElementById('otchet-status')

    if(status)
    {
        switch (code) {
            case 0:
                status.innerText = "Ожидание...";
                status.style.color = "orange";
                break;
            case 1:
                status.innerText = "Готово!";
                status.style.color = "green";
                break;
            case -1:
                status.innerText = "Прервано.";
                status.style.color = "red";
                break;
            default:
                status.innerText = "-";
                status.style.color = "black";
        }
    }
}

function setTime(time, elementId) {
    const timeClient = document.getElementById(elementId)

    if(timeClient)
    {
        timeClient.innerText = getFormatTimeToString(time)
    }
}

function getFormatTimeToString(time) {
    if (time < 1000) {
        return time.toFixed(2) + ' мс.';
    } else if (time < 60000) {
        return (time / 1000).toFixed(2) + ' сек.';
    } else {
        var minutes = Math.floor(time / 60000);
        var seconds = Math.floor((time % 60000) / 1000);
        seconds = (seconds < 10) ? '0' + seconds : seconds;
        return minutes + ':' + seconds + ' мин.';
    }
}

function getThread(elementId) {
    const threadElement = document.getElementById(elementId)

    if(threadElement)
    {
        threadElement.innerText = thread ? "Многопоточное выполнение" : "Однопоточное выполнение"
    }
}