window.moonCoreDownloadService = {
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
                        await reportRef.invokeMethodAsync("ReceiveReport", id, receivedLength, false);
                        lastReportTime = now;
                    }
                }
            }

            // Combine chunks into a single Blob
            const blob = new Blob(chunks);

            this.downloadBlob(fileName, blob);

            if (reportRef)
                await reportRef.invokeMethodAsync("ReceiveReport", id, receivedLength, true);
            
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
                        await reportRef.invokeMethodAsync("ReceiveReport", id, ev.loaded, false);
                        lastReported = now;
                    }
                };

                loadRequest.onloadend = async ev => {
                    await reportRef.invokeMethodAsync("ReceiveReport", id, ev.loaded, true);
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
}