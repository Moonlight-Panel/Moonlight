window.moonCoreXmlHttpRequest = {
    storage: {},
    initialize: function (trackingId, refObject) {
        const req = new XMLHttpRequest();

        req.addEventListener("timeout", async ev => {
            await refObject.invokeMethodAsync("TriggerTimeoutEvent", {
                "loaded": ev.loaded,
                "total": ev.total
            });
        });

        req.addEventListener("progress", async ev => {
            await refObject.invokeMethodAsync("TriggerDownloadProgressEvent", {
                "loaded": ev.loaded,
                "total": ev.total
            });
        });

        req.upload.addEventListener("progress", async ev => {
            await refObject.invokeMethodAsync("TriggerUploadProgressEvent", {
                "loaded": ev.loaded,
                "total": ev.total
            });
        });

        req.addEventListener("loadend", async ev => {
            await refObject.invokeMethodAsync("TriggerLoadedEvent", ev);
        });

        req.addEventListener("readystatechange", async _ => {
            await refObject.invokeMethodAsync("TriggerReadyStateChangeEvent", req.readyState);
        });

        this.storage[trackingId] = req;

        return req;
    },
    setProperty: function (trackingId, property, value) {
        this.storage[trackingId][property] = value;
        console.log(this.storage[trackingId]);
    },
    getProperty: function (trackingId, property) {
        return this.storage[trackingId][property];
    },
    sendStream: async function (trackingId, streamRef) {
        const stream = await streamRef.stream();
        const blob = await this.streamToBlob(stream);
        this.storage[trackingId].send(blob);
    },
    sendFile: async function (trackingId, formName, fileName, streamRef) {
        const stream = await streamRef.stream();
        const blob = await this.streamToBlob(stream);

        const formData = new FormData();
        formData.append(formName, blob, fileName);

        this.storage[trackingId].send(formData);
    },
    getResponseStream: function (trackingId) {
        return this.storage[trackingId].response;
    },
    dispose: function (trackingId) {
        this.storage[trackingId] = undefined;
    },
    streamToBlob: async function (stream) {
        const reader = stream.getReader();

        let chunks = [];

        while (true) {
            const {done, value} = await reader.read();
            if (done) break;

            chunks.push(value);
        }

        return new Blob(chunks);
    }
}