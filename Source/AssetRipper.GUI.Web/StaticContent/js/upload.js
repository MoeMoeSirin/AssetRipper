document.getElementById('uploadButton').addEventListener('click', function () {
    document.getElementById('fileInput').click();
});

document.getElementById('fileInput').addEventListener('change', async function () {
    const file = this.files[0];

    if (file) {
        const formData = new FormData();
        formData.append('Path', file);

        const response = await fetch('/LoadFile', {
            method: 'POST',
            body: formData,
        })

        if (response.ok) {
            window.location.href = '/';
        }
    }
});