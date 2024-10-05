document.getElementById('file-input').addEventListener('change', function() {
    let fileName;
    let text = document.querySelector('.input-file-text')

    try
    {
        fileName = this.files[0].name;
    }
    catch {
        fileName = ""
        console.log("File not selected")
    }

    if(text != null)
    {
        text.textContent = (text != null && fileName != null && fileName != "") ? fileName : "";
    }
});