window.moonCore = {
    window: {
        getSize: function () {
            return [window.innerWidth, window.innerHeight];
        }
    },
    keyBinds: {
        storage: {},

        registerHotkey: function (key, modifier, action, dotNetObjRef) {

            const hotkeyListener = async (event) => {
                if (event.code === key && (!modifier || event[modifier + 'Key'])) {
                    event.preventDefault();

                    await dotNetObjRef.invokeMethodAsync("OnHotkeyPressed", action);
                }
            };

            moonCore.keyBinds.storage[`${key}${modifier}`] = hotkeyListener;
            window.addEventListener('keydown', hotkeyListener);
        },

        unregisterHotkey: function (key, modifier) {
            const listenerKey = `${key}${modifier}`;
            if (moonCore.keyBinds.storage[listenerKey]) {
                window.removeEventListener('keydown', moonCore.keyBinds.storage[listenerKey]);
                delete moonCore.keyBinds.storage[listenerKey];
            }
        }
    },
    downloadService: {
        download: async function (fileName, contentStreamReference, id, reportRef) {
            const promise = new Promise(async resolve => {
                const stream = await contentStreamReference.stream();
                const reader = stream.getReader();

                let lastReportTime = 0;
                let receivedLength = 0; // Track downloaded size
                let chunks = []; // Store downloaded chunks

                while (true) {
                    const {done, value} = await reader.read();
                    if (done) break;

                    chunks.push(value);
                    receivedLength += value.length;

                    if (reportRef) {
                        const now = Date.now();

                        if (now - lastReportTime >= 500) { // Only log once per second
                            await reportRef.invokeMethodAsync("ReceiveReport", id, receivedLength, -1, false);
                            lastReportTime = now;
                        }
                    }
                }

                // Combine chunks into a single Blob
                const blob = new Blob(chunks);

                this.downloadBlob(fileName, blob);

                if (reportRef)
                    await reportRef.invokeMethodAsync("ReceiveReport", id, receivedLength, -1, true);

                resolve();
            });

            await promise;
        },
        downloadUrl: async function (fileName, url, reportRef, id, headers) {
            const promise = new Promise(async resolve => {
                let loadRequest = new XMLHttpRequest();
                let lastReported = Date.now();

                loadRequest.open("GET", url, true);

                for(let headerKey in headers) {
                    loadRequest.setRequestHeader(headerKey, headers[headerKey]);
                }

                loadRequest.responseType = "blob";

                if (reportRef) {
                    loadRequest.onprogress = async ev => {
                        const now = Date.now();

                        if (now - lastReported >= 500) {
                            await reportRef.invokeMethodAsync("ReceiveReport", id, ev.loaded, ev.total, false);
                            lastReported = now;
                        }
                    };

                    loadRequest.onloadend = async ev => {
                        await reportRef.invokeMethodAsync("ReceiveReport", id, ev.loaded, ev.total, true);
                    }
                }

                loadRequest.onload = _ => {
                    this.downloadBlob(fileName, loadRequest.response);

                    resolve();
                }

                loadRequest.send();
            });

            await promise;
        },
        downloadBlob: function (fileName, blob)
        {
            const url = URL.createObjectURL(blob);

            // Trigger file download
            const anchor = document.createElement("a");
            anchor.href = url;
            anchor.download = fileName;
            document.body.appendChild(anchor);
            anchor.click();

            document.body.removeChild(anchor);
            URL.revokeObjectURL(url);
        }
    },
    fileManager: {
        uploadCache: [],
        addFilesToCache: async function(id) {
            let files = document.getElementById(id).files;


            for (let i = 0; i < files.length; i++) {
                moonCore.fileManager.uploadCache.push(files[i]);
            }

            await this.ref.invokeMethodAsync("TriggerUpload", moonCore.fileManager.uploadCache.length);
        },
        getNextFromCache: async function() {
            if(moonCore.fileManager.uploadCache.length === 0)
                return null;

            let nextItem = moonCore.fileManager.uploadCache.pop();

            if(!nextItem)
                return null;

            let file;
            let path;

            if(nextItem instanceof File)
            {
                file = nextItem;
                path = file.name;
            }
            else
            {
                file = await this.openFileEntry(nextItem);
                path = nextItem.fullPath;
            }

            if(file.size === 0)
            {
                return {
                    path: null,
                    stream: null,
                    left: moonCore.fileManager.uploadCache.length
                }
            }

            let stream = await this.createStreamRef(file);

            return {
                path: path,
                stream: stream,
                left: moonCore.fileManager.uploadCache.length
            };
        },
        openFileEntry: async function (fileEntry) {
            const promise = new Promise(resolve => {
                fileEntry.file(file => {
                    resolve(file);
                }, err => console.log(err));
            });

            return await promise;
        },
        createStreamRef: async function (processedFile) {
            // Prevent uploads of empty files
            if (processedFile.size <= 0) {
                console.log("Skipping upload of '" + processedFile.name + "' as its empty");
                return null;
            }

            const fileReader = new FileReader();

            const readerPromise = new Promise(resolve => {
                fileReader.addEventListener("loadend", ev => {
                    resolve(fileReader.result)
                });
            });

            fileReader.readAsArrayBuffer(processedFile);

            const arrayBuffer = await readerPromise;

            return DotNet.createJSStreamReference(arrayBuffer);
        },
        setup: function (id, callbackRef) {
            this.ref = callbackRef;

            // Check which features are supported by the browser
            const supportsFileSystemAccessAPI =
                'getAsFileSystemHandle' in DataTransferItem.prototype;
            const supportsWebkitGetAsEntry =
                'webkitGetAsEntry' in DataTransferItem.prototype;

            // This is the drag and drop zone.
            const elem = document.getElementById(id);

            // Prevent navigation.
            elem.addEventListener('dragover', (e) => {
                e.preventDefault();
            });

            elem.addEventListener('drop', async (e) => {
                // Prevent navigation.
                e.preventDefault();

                if (!supportsFileSystemAccessAPI && !supportsWebkitGetAsEntry) {
                    // Cannot handle directories.
                    console.log("Cannot handle directories");
                    return;
                }

                this.getAllWebkitFileEntries(e.dataTransfer.items).then(async value => {
                    value.forEach(a => moonCore.fileManager.uploadCache.push(a));
                    await this.ref.invokeMethodAsync("TriggerUpload", this.uploadCache.length);
                });
            });
        },
        getAllWebkitFileEntries: async function (dataTransferItemList) {
            function readAllEntries(reader) {
                return new Promise((resolve, reject) => {
                    const entries = [];
                    function readEntries() {
                        reader.readEntries((batch) => {
                            if (batch.length === 0) {
                                resolve(entries);
                            } else {
                                entries.push(...batch);
                                readEntries();
                            }
                        }, reject);
                    }
                    readEntries();
                });
            }

            async function traverseEntry(entry) {
                if (entry.isFile) {
                    return [entry];
                } else if (entry.isDirectory) {
                    const reader = entry.createReader();
                    const entries = await readAllEntries(reader);
                    const subEntries = await Promise.all(entries.map(traverseEntry));
                    return subEntries.flat();
                }
                return [];
            }

            const entries = [];

            // Convert DataTransferItemList to entries
            for (let i = 0; i < dataTransferItemList.length; i++) {
                const item = dataTransferItemList[i];
                const entry = item.webkitGetAsEntry();
                if (entry) {
                    entries.push(entry);
                }
            }

            // Traverse all entries and collect file entries
            const allFileEntries = await Promise.all(entries.map(traverseEntry));
            return allFileEntries.flat();
        }
    },
    codeEditor: {
        instances: new Map(),
        attach: function (id, options) {
            const editor = ace.edit(id, options);

            moonCore.codeEditor.instances.set(id, editor);
        },
        updateOptions: function (id, options) {
            const editor = moonCore.codeEditor.instances.get(id);

            editor.setOptions(options);
        },
        getValue: function (id) {
            const editor = moonCore.codeEditor.instances.get(id);

            return editor.getValue();
        },
        destroy: function (id){
            const editor = moonCore.codeEditor.instances.get(id);

            if(!editor)
                return;

            editor.destroy();
            editor.container.remove();

            moonCore.codeEditor.instances.delete(id);
        }
    }
}