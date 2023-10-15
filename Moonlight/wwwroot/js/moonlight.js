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
    }
}