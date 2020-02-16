
window.addEventListener("load", () => {
    var s = new EventSource("/events");

    s.onmessage = e => {
        console.log(e);
    };

    window.addEventListener("beforeunload", () => {
        s.close();
    });
});
