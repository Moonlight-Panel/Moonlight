window.moonlight = {
    window: {
        open: function (url, title, h, w) {
            const dualScreenLeft = window.screenLeft !== undefined ? window.screenLeft : window.screenX;
            const dualScreenTop = window.screenTop !== undefined ? window.screenTop : window.screenY;

            const width = window.innerWidth ? window.innerWidth : document.documentElement.clientWidth ? document.documentElement.clientWidth : screen.width;
            const height = window.innerHeight ? window.innerHeight : document.documentElement.clientHeight ? document.documentElement.clientHeight : screen.height;

            const systemZoom = width / window.screen.availWidth;
            const left = (width - w) / 2 / systemZoom + dualScreenLeft
            const top = (height - h) / 2 / systemZoom + dualScreenTop
            const newWindow = window.open(url, title,
                `
      scrollbars=yes,
      width=${w / systemZoom}, 
      height=${h / systemZoom}, 
      top=${top}, 
      left=${left}
      `
            )

            if (window.focus) newWindow.focus();
        },
        closeCurrent() {
            window.close();
        }
    },
    assets: {
        loadCss: function (url) {
            let linkElement = document.createElement('link');

            linkElement.href = url;
            linkElement.rel = 'stylesheet';
            linkElement.type = 'text/css';

            (document.head || document.documentElement).appendChild(linkElement);
        },
        loadJavascript: function (url) {
            let scriptElement = document.createElement('script');

            scriptElement.src = url;
            scriptElement.type = 'text/javascript';

            (document.head || document.documentElement).appendChild(scriptElement);
        }
    }
};