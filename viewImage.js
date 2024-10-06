function closeImg() {
    const big_image = document.getElementById('big-image');

    if (big_image) {
        big_image.innerHTML = "";
        big_image.style.display = "none";
    }
}

function openImg(image) {
    var big_image = document.getElementById('big-image');

    if (big_image && image.src) {
        const imgElement = document.createElement('img');
        imgElement.src = image.src;
        big_image.innerHTML = "";
        big_image.appendChild(imgElement);
        big_image.style.display = "flex";
    }
}