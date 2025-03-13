window.moonCoreFileManager = {
    uploadCache: [],
    addFilesToCache: async function(id) {
        let files = document.getElementById(id).files;


        for (let i = 0; i < files.length; i++) {
            this.uploadCache.push(files[i]);
        }

        await this.ref.invokeMethodAsync("TriggerUpload", this.uploadCache.length);
    },
    getNextFromCache: async function() {
        if(this.uploadCache.length === 0)
            return null;

        let nextItem = this.uploadCache.pop();

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
                left: this.uploadCache.length
            }
        }
        
        let stream = await this.createStreamRef(file);
        
        return {
            path: path,
            stream: stream,
            left: this.uploadCache.length
        };
    },
    readCache: async function (index) {
        const item = this.uploadCache.pop();
        console.log(item);
        const streamRefData = await this.createStreamRef(item);

        if(streamRefData == null)
            return {path: item.fullPath, streamRef: null};
        
        return {
            path: item.fullPath,
            streamRef: streamRefData.stream,
            size: streamRefData.size
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
                value.forEach(a => this.uploadCache.push(a));
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
}