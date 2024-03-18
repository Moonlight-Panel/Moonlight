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

        var titleE = document.createElement('div');
        titleE.setAttribute('class', 'me-auto');
        
        var iconE = document.createElement("i");
        
        var iconTag = "bx-info-circle";
        
        switch (color)
        {
            case "info":
                iconTag = "bx-info-circle";
                break

            case "success":
                iconTag = "bx-check-circle";
                break

            case "warning":
                iconTag = "bx-error-circle";
                break

            case "danger":
                iconTag = "bx-error";
                break
        }
        
        iconE.setAttribute("class", "align-middle me-2 bx bx-sm " + iconTag + " text-" + (color === "secondary" ? "primary" : color));
        
        var titleText = document.createElement("span");
        titleText.setAttribute("class", "text-white fs-6 fw-bold align-middle");
        titleText.innerText = title;
        
        titleE.appendChild(iconE);
        titleE.appendChild(titleText);
        
        var closeButton = document.createElement('button');
        closeButton.setAttribute('type', 'button');
        closeButton.setAttribute('class', 'btn-close');
        closeButton.setAttribute('data-bs-dismiss', 'toast');
        closeButton.setAttribute('data-label', 'Close');

        toastHeader.append(titleE);
        toastHeader.append(closeButton);
    }

    return toastHeader;
}

function buildToastBody(title, description, color) {
    var toastBody = document.createElement('div');
    toastBody.setAttribute("class", "toast-body")

    if(title === "")
    {

        var iconE = document.createElement("i");

        var iconTag = "bx-info-circle";

        switch (color)
        {
            case "info":
                iconTag = "bx-info-circle";
                break

            case "success":
                iconTag = "bx-check-circle";
                break

            case "warning":
                iconTag = "bx-error-circle";
                break

            case "danger":
                iconTag = "bx-error";
                break
        }

        iconE.setAttribute("class", "align-middle me-2 bx bx-sm " + iconTag + " text-" + (color === "secondary" ? "primary" : color));

        toastBody.appendChild(iconE);
    }
    
    var contentSpan = document.createElement("span");
    contentSpan.setAttribute("class", "fs-5 align-middle text-white")
    contentSpan.innerText = description;
    
    toastBody.appendChild(contentSpan);

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