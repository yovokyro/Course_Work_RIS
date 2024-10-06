document.getElementById('file-input').addEventListener('change', function() {
    const fileName = this.files[0] ? this.files[0].name : "";
    const text = document.querySelector('.input-file-text');

    if (text) {
        text.textContent = fileName || "";
        return;
    }

    console.log("File not selected")
});