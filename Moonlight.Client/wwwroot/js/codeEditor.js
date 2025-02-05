window.moonCoreCodeEditor = {
    instances: new Map(),
    attach: function (id, options) {
        const editor = ace.edit(id, options);
        
        this.instances.set(id, editor);
    },
    updateOptions: function (id, options) {
        const editor = this.instances.get(id);
        
        editor.setOptions(options);
    },
    getValue: function (id) {
        const editor = this.instances.get(id);
        
        return editor.getValue();
    },
    destroy: function (id){
        const editor = this.instances.get(id);
        
        editor.destroy();
        editor.container.remove();
    }
}