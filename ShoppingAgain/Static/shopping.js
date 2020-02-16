
window.addEventListener("load", () => {

    function $(a, b) {
        if (b == null) {
            return document.querySelector(a);
        }
        return a.querySelector(b);
    }

    function $$(a, b) {
        if (b == null) {
            return Array.from(document.querySelectorAll(a));
        }
        return Array.from(a.querySelectorAll(b));
    }

    function textNode(text) {
        return document.createTextNode(text);
    }

    function listCreated(e) {
        const template = $("#lists-li-template").cloneNode(true);
        template.id = e.listId;
        template.hidden = false;

        const link = $(template, "a");
        link.href = "/l/" + e.listId;
        link.appendChild(textNode(e.listName));

        const lists = $$("#lists li");
        if (lists.length === 0) {
            return;
        }

        for (let i = 0; i < lists.length; i += 1) {
            const l = lists[i];
            const name = $(l, "a").innerText;
            if (e.listName < name) {
                $("#lists").insertBefore(template, l);
                return;
            }
        }

        // Not inserted, either because lists is empty
        // or list.name is after all the existing entries.
        // Either way, just stick it at the end.
        $("lists").appendChild(template);
    }

    function itemCreated(e) {
        const template = $("#list-tr-template").cloneNode(true);
        template.id = e.itemId;
    }


    const s = new EventSource("/events");
    s.onmessage = e => {

        let payload;
        try {
            payload = JSON.parse(e.data);
        } catch (ex) {
            console.warn("Can't parse payload from event");
            return;
        }

        switch (payload.eventType) {
            case "ListCreated":
                listCreated(payload);
                break;
            case "ItemCreated":
                itemCreated(payload);
        }

    };

    window.addEventListener("beforeunload", () => {
        s.close();
    });
});
