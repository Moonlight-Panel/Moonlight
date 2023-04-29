window.showInfoToast = function (msg) {
    toastr['info'](msg);
}

window.showErrorToast = function (msg) {
    toastr['error'](msg);
}

window.showSuccessToast = function (msg) {
    toastr['success'](msg);
}

window.showWarningToast = function (msg) {
    toastr['warning'](msg);
}

window.createToast = function (id, text) {
    var toast = toastr.success(text, '',
        {
            closeButton: true,
            progressBar: false,
            tapToDismiss: false,
            timeOut: 0,
            extendedTimeOut: 0,
            positionClass: "toastr-bottom-right",
            preventDuplicates: false,
            onclick: function () {
                toastr.clear(toast);
            }
        });
    var toastElement = toast[0];
    toastElement.setAttribute('data-toast-id', id);
    toastElement.classList.add("bg-secondary");
}

window.modifyToast = function (id, newText) {
    var toast = document.querySelector('[data-toast-id="' + id + '"]');
    
    if (toast) {
        var toastMessage = toast.lastChild;
        if (toastMessage) {
            toastMessage.innerHTML = newText;
        }
    }
}

window.removeToast = function (id) {
    var toast = document.querySelector('[data-toast-id="' + id + '"]');
    if (toast) {
        toast.childNodes.item(1).click();
    }
}