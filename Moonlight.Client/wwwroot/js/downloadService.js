window.moonCoreDownloadService = {
    download: async function (fileName, contentStreamReference, id, reportRef) {

        const stream = await contentStreamReference.stream();
        const reader = stream.getReader();

        let lastReportTime = 0;
        let receivedLength = 0; // Track downloaded size
        let chunks = []; // Store downloaded chunks

        while (true) {
            const { done, value } = await reader.read();
            if (done) break;

            chunks.push(value);
            receivedLength += value.length;

            if(reportRef)
            {
                const now = Date.now();

                if (now - lastReportTime >= 500) { // Only log once per second
                    await reportRef.invokeMethodAsync("ReceiveReport", id, receivedLength, false);
                    lastReportTime = now;
                }
            }
        }

        // Combine chunks into a single Blob
        const blob = new Blob(chunks);
        const url = URL.createObjectURL(blob);

        // Trigger file download
        const anchor = document.createElement("a");
        anchor.href = url;
        anchor.download = fileName;
        document.body.appendChild(anchor);
        anchor.click();

        document.body.removeChild(anchor);
        URL.revokeObjectURL(url);

        if(reportRef)
            await reportRef.invokeMethodAsync("ReceiveReport", id, receivedLength, true);
    }
}