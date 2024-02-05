class ToastHelper {
    constructor(title, description, color, timeout) {
        var toastElement = buildToast(title, description, color);
        var toastWrapper = getOrCreateToastWrapper();
        toastWrapper.append(toastElement);
        this.bootstrapToast = new bootstrap.Toast(toastElement, {
            autohide: false
        });
        this.domElement = toastElement;

        this.show = function () {
            this.bootstrapToast.show();

            if (timeout && typeof timeout === 'number') {
                setTimeout(() => {
                    this.hide();
                    toastElement.remove();
                }, timeout);
            }
        }

        this.showAlways = function () {
            this.bootstrapToast.show();
        }

        this.hide = function () {
            this.bootstrapToast.hide();
        }

        this.dispose = function () {
            this.bootstrapToast.dispose();
        }
    }
}

function getOrCreateToastWrapper() {
    var toastWrapper = document.querySelector('body > [data-toast-wrapper]');

    if (!toastWrapper) {
        toastWrapper = document.createElement('div');
        toastWrapper.style.zIndex = 11;
        toastWrapper.style.position = 'fixed';
        toastWrapper.style.bottom = 0;
        toastWrapper.style.right = 0;
        toastWrapper.style.padding = '1rem';
        toastWrapper.setAttribute('data-toast-wrapper', '');
        document.body.append(toastWrapper);
    }

    return toastWrapper;
}

function buildToastHeader(title, color) {
    var toastHeader = document.createElement('div');

    if(title !== "")
    {
        toastHeader.setAttribute('class', 'toast-header');

        var strong = document.createElement('strong');
        strong.setAttribute('class', 'me-auto text-white');
        strong.textContent = title;

        var closeButton = document.createElement('button');
        closeButton.setAttribute('type', 'button');
        closeButton.setAttribute('class', 'btn-close');
        closeButton.setAttribute('data-bs-dismiss', 'toast');
        closeButton.setAttribute('data-label', 'Close');

        toastHeader.append(strong);
        toastHeader.append(closeButton);
    }

    return toastHeader;
}

function buildToastBody(title, description, color) {
    var toastBody = document.createElement('div');

    if(title !== "")
        toastBody.setAttribute('class', 'toast-body fs-5');
    else
        toastBody.setAttribute('class', 'toast-body fs-5 fw-bold '   + (color ? 'text-' + color : ''));

    toastBody.textContent = description;

    return toastBody;
}

function buildToast(title, description, color) {
    var toast = document.createElement('div');
    toast.setAttribute('class', 'toast my-2');
    toast.setAttribute('role', 'alert');
    toast.setAttribute('aria-live', 'assertive');
    toast.setAttribute('aria-atomic', 'true');

    var toastHeader = buildToastHeader(title, color);
    var toastBody = buildToastBody(title, description, color);

    toast.append(toastHeader);
    toast.append(toastBody);

    return toast;
}