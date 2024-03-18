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
        },
        create: function (id, text) {
            var toast = new ToastHelper("Progress", text, "secondary", 0);
            toast.showAlways();

            toast.domElement.setAttribute('data-ml-toast-id', id);
        },
        modify: function (id, text) {
            var toast = document.querySelector('[data-ml-toast-id="' + id + '"]');

            toast.getElementsByClassName("toast-body")[0].getElementsByTagName("span")[0].innerText = text;
        },
        remove: function (id) {
            var toast = document.querySelector('[data-ml-toast-id="' + id + '"]');
            bootstrap.Toast.getInstance(toast).hide();

            setTimeout(() => {
                toast.remove();
            }, 2);
        }
    },
    modals: {
        show: function (id, focus)
        {
            let modal = new bootstrap.Modal(document.getElementById(id), {
                focus: focus
            });
            
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
        getHelper: function (description, color, type, icon = "") {
            return new AlertHelper(description, color, type, icon);
        },
        info: function (title, description) {
            this.getHelper(title, "info", "", "bx-info-circle").ask(); // Ignore, as it should not be a blocking call
        },
        success: function (title, description) {
            this.getHelper(title, "success", "", "bx-check").ask(); // Ignore, as it should not be a blocking call
        },
        warning: function (title, description) {
            this.getHelper(title, "warning", "", "bx-error-circle").ask(); // Ignore, as it should not be a blocking call
        },
        error: function (title, description) {
            this.getHelper(title, "danger", "", "bx-error").ask(); // Ignore, as it should not be a blocking call
        },
        yesno: async function (title, yesText, noText) {
            return await this.getHelper(title, "secondary", "confirm", "bx-question-mark").ask();
        },
        text: async function (title, description, initialValue) {
            return await this.getHelper(title, "primary", "text", initialValue).ask();
        }
    },
    utils: {
        download: async function (fileName, contentStreamReference) {
            const arrayBuffer = await contentStreamReference.arrayBuffer();
            const blob = new Blob([arrayBuffer]);
            const url = URL.createObjectURL(blob);
            const anchorElement = document.createElement('a');
            anchorElement.href = url;
            anchorElement.download = fileName ?? '';
            anchorElement.click();
            anchorElement.remove();
            URL.revokeObjectURL(url);
        },
        vendo: function ()
        {
            try 
            {
                var request = new XMLHttpRequest();

                request.open("GET", "https://pagead2.googlesyndication.com/pagead/js/aidsbygoogle.js?client=ca-pub-1234567890123456", false);
                request.send();

                if(request.status === 404)
                    return false;

                return true;
            }
            catch (e) 
            {
                return false;
            }
        },
        registerUnload: function (dotNetObjRef) 
        {
            window.addEventListener("beforeunload", function()
            {
                dotNetObjRef.invokeMethodAsync('Unload');
            }, false);
        }
    },
    textEditor: {
        create: function(id)
        {
            BalloonEditor
                .create(document.getElementById(id), {
                    toolbar: [ 'heading', '|', 'bold', 'italic', 'link', 'bulletedList', 'numberedList', 'blockQuote' ],
                    heading: {
                        options: [
                            { model: 'paragraph', title: 'Paragraph', class: 'ck-heading_paragraph' },
                            { model: 'heading1', view: 'h1', title: 'Heading 1', class: 'ck-heading_heading1' },
                            { model: 'heading2', view: 'h2', title: 'Heading 2', class: 'ck-heading_heading2' }
                        ]
                    }
                })
                .catch(error => {
                    console.error(error);
                });
        },
        get: function (id)
        {
            let editor = document.getElementById(id).ckeditorInstance;
            return editor.getData();
        },
        set: function (id, data)
        {
            let editor = document.getElementById(id).ckeditorInstance;
            editor.setData(data);
        }
    },
    clipboard: {
        copy: function (text) {
            if (!navigator.clipboard) {
                var textArea = document.createElement("textarea");
                textArea.value = text;

                // Avoid scrolling to bottom
                textArea.style.top = "0";
                textArea.style.left = "0";
                textArea.style.position = "fixed";

                document.body.appendChild(textArea);
                textArea.focus();
                textArea.select();

                try {
                    var successful = document.execCommand('copy');
                    var msg = successful ? 'successful' : 'unsuccessful';
                } catch (err) {
                    console.error('Fallback: Oops, unable to copy', err);
                }

                document.body.removeChild(textArea);
                return;
            }
            navigator.clipboard.writeText(text).then(function () {
                },
                function (err) {
                    console.error('Async: Could not copy text: ', err);
                }
            );
        }
    },
    dropzone: {
        create: function (elementId, url) {
            var id = "#" + elementId;
            var dropzone = document.querySelector(id);

            // set the preview element template
            var previewNode = dropzone.querySelector(".dropzone-item");
            previewNode.id = "";
            var previewTemplate = previewNode.parentNode.innerHTML;
            previewNode.parentNode.removeChild(previewNode);

            var fileDropzone = new Dropzone(id, {
                url: url,
                maxFilesize: 100,
                previewTemplate: previewTemplate,
                previewsContainer: id + " .dropzone-items",
                clickable: ".dropzone-panel",
                createImageThumbnails: false,
                ignoreHiddenFiles: false,
                disablePreviews: false
            });

            fileDropzone.on("addedfile", function (file) {
                const dropzoneItems = dropzone.querySelectorAll('.dropzone-item');
                dropzoneItems.forEach(dropzoneItem => {
                    dropzoneItem.style.display = '';
                });

                // Create a progress bar for the current file
                var progressBar = dropzone.querySelector('.dropzone-item .progress-bar');
                progressBar.style.width = "0%";
            });

// Update the progress bar for each file
            fileDropzone.on("uploadprogress", function (file, progress, bytesSent) {
                var dropzoneItem = file.previewElement;
                var progressBar = dropzoneItem.querySelector('.progress-bar');
                progressBar.style.width = progress + "%";
            });

// Hide the progress bar for each file when the upload is complete
            fileDropzone.on("complete", function (file) {
                var dropzoneItem = file.previewElement;
                var progressBar = dropzoneItem.querySelector('.progress-bar');

                setTimeout(function () {
                    progressBar.style.opacity = "1";
                    progressBar.classList.remove("bg-primary");
                    progressBar.classList.add("bg-success");
                }, 300);
            });
        },
        updateUrl: function (elementId, url) {
            Dropzone.forElement("#" + elementId).options.url = url;
        }
    },
    editor: {
        instance: {},

        create: function (mount, theme, mode, initialContent, lines, fontSize) {
            this.instance = ace.edit(mount);

            this.instance.setTheme("ace/theme/" + theme);
            this.instance.session.setMode("ace/mode/" + mode);
            this.instance.setShowPrintMargin(false);
            this.instance.setOptions({
                minLines: lines,
                maxLines: lines
            });

            this.instance.setValue(initialContent);
            this.instance.setFontSize(fontSize);
        },

        setValue: function (content) {
            this.instance.setValue(content);
            this.instance.moveCursorTo(0);
        },

        getValue: function () {
            return this.instance.getValue();
        },

        setMode: function (mode) {
            this.instance.session.setMode("ace/mode/" + mode);
        }
    },
    hotkeys: {
        storage: {},

        registerHotkey: function (key, modifier, action, dotNetObjRef) {
            const hotkeyListener = (event) => {
                if (event.code === key && event[modifier + 'Key']) {
                    event.preventDefault();
                    dotNetObjRef.invokeMethodAsync('OnHotkeyPressed', action);
                }
            };
            this.storage[key + modifier] = hotkeyListener;
            window.addEventListener('keydown', hotkeyListener);
        },

        unregisterHotkey: function (key, modifier) {
            const listenerKey = key + modifier;
            if (this.storage[listenerKey]) {
                window.removeEventListener('keydown', this.hotkeys[listenerKey]);
                delete this.storage[listenerKey];
            }
        }
    }
}