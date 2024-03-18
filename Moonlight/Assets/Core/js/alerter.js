class AlertHelper {
    constructor(description, color, type, icon = "") {
        
        this.htmlHandle = buildAlert(description, color, type, icon, this)
        
        this.modal = new bootstrap.Modal(this.htmlHandle, {
            focus: true
        });

        this.ask = async function () {
            // This should wait until the modal has been submitted or cancled and the return the input value

            // Create a new promise that will be resolved when the modal is submitted or cancelled
            return new Promise((resolve, reject) => {
                this.resolvePromise = resolve;
                this.modal.show();

                // Handle modal close events
                this.modal._element.addEventListener('hidden.bs.modal', () => {

                    if(type === "confirm")
                        this.resolvePromise(false);
                    else
                        this.resolvePromise("");
                    
                    setTimeout(() => {
                        this.htmlHandle.remove();
                    }, 1000);
                });
            });
        }
    }
}

function buildAlert(description, color, type, icon, handle) {
    var modal = document.createElement("div");
    modal.classList.add("modal", "fade");
    modal.tabIndex = -1;

    var modalDialog = document.createElement("div");
    modalDialog.classList.add("modal-dialog", "modal-dialog-centered");

    var modalContent = document.createElement("div");
    modalContent.classList.add("modal-content");
    
    modalContent.appendChild(buildAlertBody(description, color, type, icon));
    modalContent.appendChild(buildAlertFooter(type, handle));
    
    modalDialog.appendChild(modalContent);
    
    modal.appendChild(modalDialog);
    
    return modal;
}

function buildAlertBody(description, color, type, icon) {
    var modalBody = document.createElement("modal-body");
    modalBody.classList.add("modal-body");

    if (type !== "text") {
        var flexBox = document.createElement("div");
        flexBox.classList.add("d-flex", "justify-content-center");

        var symbol = document.createElement("div");
        symbol.classList.add("symbol", "symbol-circle", "h-90px", "w-90px", "bg-" + color, "text-center", "d-flex", "justify-content-center", "align-items-center");

        var iconE = document.createElement("i");
        iconE.classList.add("bx", "bx-lg", icon, "text-white", "align-middle", "p-10");

        symbol.appendChild(iconE);
        flexBox.appendChild(symbol);

        modalBody.appendChild(flexBox);
    }

    var descE = document.createElement("p");
    descE.classList.add("mt-5", "text-gray-800", "fs-4", "fw-semibold", "text-center");
    descE.innerText = description;

    modalBody.appendChild(descE);

    if (type === "text") {
        var txtInput = document.createElement("input");

        txtInput.classList.add("form-control", "w-100", "text-center", "mt-2");
        txtInput.type = "text";
        txtInput.value = icon;

        modalBody.appendChild(txtInput);
    }

    return modalBody;
}

function buildAlertFooter(type, handle) {
    var modalFooter = document.createElement("modal-footer");
    modalFooter.classList.add("modal-footer");

    var buttonGroup = document.createElement("div");
    buttonGroup.classList.add("btn-group", "w-100");

    if (type === "confirm" || type === "text") {
        var submitButton = document.createElement("button");
        
        submitButton.onclick = function () {
            if(type === "confirm")
                handle.resolvePromise(true);
            else
                handle.resolvePromise(handle.htmlHandle.getElementsByTagName("input")[0].value);
            
            handle.modal.hide();
            
            setTimeout(() => {
                handle.htmlHandle.remove();
            }, 1000);
        };
        
        if(type === "confirm")
        {
            submitButton.innerText = "Yes";
            submitButton.classList.add("btn", "btn-primary", "w-50");
        }
        else
        {
            submitButton.innerText = "Submit";
            submitButton.classList.add("btn", "btn-primary", "w-50");
        }
        
        var cancelButton = document.createElement("button");
        cancelButton.onclick = function () {
            if(type === "confirm")
                handle.resolvePromise(false);
            else
                handle.resolvePromise("");

            handle.modal.hide();

            setTimeout(() => {
                handle.htmlHandle.remove();
            }, 1000);
        };
        
        if(type === "confirm")
        {
            cancelButton.innerText = "No";
            cancelButton.classList.add("btn", "btn-danger", "w-50");
        }
        else
        {
            cancelButton.innerText = "Cancel";
            cancelButton.classList.add("btn", "btn-secondary", "w-50");
        }
        
        buttonGroup.appendChild(submitButton);
        buttonGroup.appendChild(cancelButton);
    } else {
        var okButton = document.createElement("button");
        okButton.classList.add("btn", "btn-primary");
        okButton.innerText = "Cancel";
        okButton.setAttribute("data-bs-dismiss", "modal");

        buttonGroup.appendChild(okButton);
    }
    
    modalFooter.appendChild(buttonGroup);
    
    return modalFooter;
}