window.initMonacoTheme = function ()
{
    monaco.editor.defineTheme('moonlight-theme', {
        base: 'vs-dark',
        inherit: true,
        rules: [
        ],
        colors: {
            'editor.background': '#000000'
        }
    });
}