window.scrollToElement = function (id)
{
    let e = document.getElementById(id);
    e.scrollTop = e.scrollHeight;
};

window.showModal = function (name)
{
    $('#' + name).modal('show');
}

window.hideModal = function (name)
{
    $('#' + name).modal('hide');
}

window.attachFileUploadWebkit = function (id)
{
    document.getElementById(id).addEventListener("change", function(e) {
        var files = [];

        Array.from(e.target.files).forEach(file => {
            files.push(file.webkitRelativePath);
        });

        window.webkitStorage = files;
    });
};

window.getWebkitStorage = function ()
{
  return window.webkitStorage;
};

window.triggerResizeEvent = function ()
{
    window.dispatchEvent(new Event('resize'));
}

window.showNotification = function (title, text, img) {
    let notification = new Notification(title, { body: text, icon: img });
}