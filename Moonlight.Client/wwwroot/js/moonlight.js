window.moonlight = {
    window: {
        open: function (url, title, w, h) {
            let height = w;
            let width = h;
            var left = (screen.width - width) / 2;
            var top = (screen.height - height) / 2;
            var newWindow = window.open(url, title, 'resizable = yes, width=' + width + ', height=' + height + ', top=' + top + ', left=' + left);

            if (window.focus) newWindow.focus();
        },
        closeCurrent() {
            window.close();
        }
    }
};