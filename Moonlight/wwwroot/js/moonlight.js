window.moonlight = {
    toasts: {
        success: function(title, message, timeout)
        {
            this.show(title, message, timeout, "success");
        },
        danger: function(title, message, timeout)
        {
            this.show(title, message, timeout, "danger");
        },
        warning: function(title, message, timeout)
        {
            this.show(title, message, timeout, "warning");
        },
        info: function(title, message, timeout)
        {
            this.show(title, message, timeout, "info");
        },
        show: function(title, message, timeout, color)
        {
            var toast = new ToastHelper(title, message, color, timeout);
            toast.show();
        }
    },
    modals: {
        show: function (id)
        {
            let modal = new bootstrap.Modal(document.getElementById(id));
            modal.show();
        },
        hide: function (id)
        {
            let element = document.getElementById(id)
            let modal = bootstrap.Modal.getInstance(element)
            modal.hide()
        }
    },
    alerts: {
        getHelper: function(){
            return Swal.mixin({
                customClass: {
                    confirmButton: 'btn btn-success',
                    cancelButton: 'btn btn-danger',
                    denyButton: 'btn btn-danger',
                    htmlContainer: 'text-white'
                },
                buttonsStyling: false
            });
        },
        info: function (title, description) {
            this.getHelper().fire(
                title,
                description,
                'info'
            )
        },
        success: function (title, description) {
            this.getHelper().fire(
                title,
                description,
                'success'
            )
        },
        warning: function (title, description) {
            this.getHelper().fire(
                title,
                description,
                'warning'
            )
        },
        error: function (title, description) {
            this.getHelper().fire(
                title,
                description,
                'error'
            )
        },
        yesno: function (title, yesText, noText) {
            return this.getHelper().fire({
                title: title,
                showDenyButton: true,
                confirmButtonText: yesText,
                denyButtonText: noText,
            }).then((result) => {
                if (result.isConfirmed) {
                    return true;
                } else if (result.isDenied) {
                    return false;
                }
            })
        },
        text: function (title, description) {
            const {value: text} = this.getHelper().fire({
                title: title,
                input: 'text',
                inputLabel: description,
                inputValue: "",
                showCancelButton: false,
                inputValidator: (value) => {
                    if (!value) {
                        return 'You need to enter a value'
                    }
                }
            })

            return text;
        }
    }
}