XtermBlazor.registerAddons({"xterm-addon-fit": new FitAddon.FitAddon()});

window.serverTerminal = {
    registerResize: function (id) {
        window.serverTerminal.terminalId = id;
        
        window.addEventListener("resize", this.handle);
    },
    handle: function(a){
        XtermBlazor.invokeAddonFunction(window.serverTerminal.terminalId, "xterm-addon-fit", "fit");
    },
    unregisterResize: function (id) {
        window.removeEventListener("resize", this.handle);
    }
}