const xhr = new XMLHttpRequest();
const fd = new FormData();


function post() {
    const input = document.getElementById("file");
    fd.append("file", input.files);
    xhr.open('POST',"http://127.0.0.1:5000/file");
    xhr.setRequestHeader("Content-Type", "multipart/form-data")
    xhr.send(fd);
    xhr.addEventListener('readystatechange', () => {

		if( xhr.readyState === 4 && xhr.status === 200) {
			console.log(xhr.response);
        }
    })
}