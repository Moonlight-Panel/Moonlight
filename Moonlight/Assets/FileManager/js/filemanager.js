window.filemanager = {

    urlCache: new Map(),

    dropzone: {
        init: function (id, urlId, progressReporter) {
            function preventDefaults(e) {
                e.preventDefault();
                e.stopPropagation();
            }

            async function handleDrop(e) {
                e.preventDefault();

                if (e.dataTransfer.items && e.dataTransfer.items.length > 0) {

                    moonlight.toasts.create("dropZoneUploadProgress", "Preparing to upload");

                    await performUpload(e.dataTransfer.items);

                    moonlight.toasts.remove("dropZoneUploadProgress");
                    moonlight.toasts.success("", "Successfully uploaded files", 5000);

                    progressReporter.invokeMethodAsync("UpdateStatus");
                }

                //TODO: HANDLE UNSUPPORTED which would call else
            }

            async function performUpload(items) {
                const fileEntries = [];

                // Collect file entries from DataTransferItemList
                for (let i = 0; i < items.length; i++) {
                    if (items[i].kind === 'file') {
                        const entry = items[i].webkitGetAsEntry();
                        if (entry.isFile) {
                            fileEntries.push(entry);
                        } else if (entry.isDirectory) {
                            await readDirectory(entry, fileEntries);
                        }
                    }
                }

                // Upload files one by one
                for (const fileEntry of fileEntries) {
                    moonlight.toasts.modify("dropZoneUploadProgress", `Uploading '${fileEntry.name}'`);

                    await uploadFile(fileEntry);
                }
            }

            async function readDirectory(directoryEntry, fileEntries = []) {
                const directoryReader = directoryEntry.createReader();

                return new Promise(async (resolve, reject) => {
                    const readBatch = async () => {
                        directoryReader.readEntries(async function (entries) {
                            for (const entry of entries) {
                                if (entry.isFile) {
                                    fileEntries.push(entry);
                                } else if (entry.isDirectory) {
                                    await readDirectory(entry, fileEntries);
                                }
                            }

                            // If there are more entries to read, call readBatch again
                            if (entries.length === 100) {
                                await readBatch();
                            } else {
                                resolve();
                            }
                        }, reject);
                    };

                    // Start reading the first batch
                    await readBatch();
                });
            }

            async function uploadFile(file) {
                // Upload the file to the server
                let formData = new FormData();
                formData.append('file', await getFile(file));
                formData.append("path", file.fullPath);

                var url = filemanager.urlCache.get(urlId);

                // Create a new fetch request
                let request = new Request(url, {
                    method: 'POST',
                    body: formData
                });

                request.onprogress = function (event) {
                    if (event.lengthComputable) {
                        let percentComplete = (event.loaded / event.total) * 100;
                        console.log(`Upload progress: ${percentComplete.toFixed(2)}%`);
                        console.log(`Bytes transferred: ${event.loaded} of ${event.total}`);
                    }
                };

                try {
                    // Use the fetch API to send the request
                    var response = await fetch(request);

                    if (!response.ok) {
                        var errorText = await response.text();

                        moonlight.toasts.danger(`Failed to upload '${file.name}'`, errorText, 5000);
                    }
                } catch (error) {
                    moonlight.toasts.danger(`Failed to upload '${file.name}'`, error.toString(), 5000);
                }
            }

            async function getFile(fileEntry) {
                try {
                    return new Promise((resolve, reject) => fileEntry.file(resolve, reject));
                } catch (err) {
                    console.log(err);
                }
            }

            const dropArea = document.getElementById(id);

            ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
                dropArea.addEventListener(eventName, preventDefaults, false);
            });

            // Handle dropped files and folders
            dropArea.addEventListener('drop', handleDrop, false);
        }
    },
    fileselect: {
        init: function (id, urlId, progressReporter) {
            const inputElement = document.getElementById(id);

            inputElement.addEventListener("change", handleFiles, false);

            async function handleFiles() {
                const fileList = inputElement.files;

                moonlight.toasts.create("selectUploadProgress", "Preparing to upload");

                for (const file of fileList) {
                    moonlight.toasts.modify("selectUploadProgress", `Uploading '${file.name}'`);
                    await uploadFile(file);
                }

                moonlight.toasts.remove("selectUploadProgress");
                moonlight.toasts.success("", "Successfully uploaded files", 5000);

                progressReporter.invokeMethodAsync("UpdateStatus");
                
                inputElement.value = "";
            }

            async function uploadFile(file) {
                // Upload the file to the server
                let formData = new FormData();
                formData.append('file', file);
                formData.append("path", file.name);

                var url = filemanager.urlCache.get(urlId);

                // Create a new fetch request
                let request = new Request(url, {
                    method: 'POST',
                    body: formData
                });

                request.onprogress = function (event) {
                    if (event.lengthComputable) {
                        let percentComplete = (event.loaded / event.total) * 100;
                        console.log(`Upload progress: ${percentComplete.toFixed(2)}%`);
                        console.log(`Bytes transferred: ${event.loaded} of ${event.total}`);
                    }
                };

                try {
                    // Use the fetch API to send the request
                    var response = await fetch(request);

                    if (!response.ok) {
                        var errorText = await response.text();

                        moonlight.toasts.danger(`Failed to upload '${file.name}'`, errorText, 5000);
                    }
                } catch (error) {
                    moonlight.toasts.danger(`Failed to upload '${file.name}'`, error.toString(), 5000);
                }
            }
        }
    },
    updateUrl: function (urlId, url) {
        filemanager.urlCache.set(urlId, url);
    }
};