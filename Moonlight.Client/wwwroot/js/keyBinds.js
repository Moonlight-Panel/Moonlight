window.moonCoreKeyBinds = {
    storage: {},

    registerHotkey: function (key, modifier, action, dotNetObjRef) {

        const hotkeyListener = async (event) => {
            if (event.code === key && (!modifier || event[modifier + 'Key'])) {
                event.preventDefault();

                await dotNetObjRef.invokeMethodAsync("OnHotkeyPressed", action);
            }
        };
        
        this.storage[`${key}${modifier}`] = hotkeyListener;
        window.addEventListener('keydown', hotkeyListener);
    },

    unregisterHotkey: function (key, modifier) {
        const listenerKey = `${key}${modifier}`;
        if (this.storage[listenerKey]) {
            window.removeEventListener('keydown', this.storage[listenerKey]);
            delete this.storage[listenerKey];
        }
    }
}