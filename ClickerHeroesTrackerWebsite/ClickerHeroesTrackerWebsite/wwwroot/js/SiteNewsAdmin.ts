namespace SiteNewsAdmin
{
    "use strict";

    export function init(container: HTMLElement): void
    {
        if (!container)
        {
            throw new Error("Element not found: " + container);
        }

        const entries = container.children;
        for (let i = 0; i < entries.length; i++)
        {
            const entry = entries[i];
            const dateHeading = entry.querySelector("h3") as HTMLElement;

            const buttonContainer = document.createElement("div");
            buttonContainer.classList.add("pull-right");

            const editButton = document.createElement("button");
            editButton.appendChild(document.createTextNode("Edit"));
            editButton.addEventListener("click", editButtonClicked);
            buttonContainer.appendChild(editButton);

            const saveButton = document.createElement("button");
            saveButton.classList.add("hide");
            saveButton.appendChild(document.createTextNode("Save"));
            saveButton.addEventListener("click", saveButtonClicked);
            buttonContainer.appendChild(saveButton);

            const cancelButton = document.createElement("button");
            cancelButton.classList.add("hide");
            cancelButton.appendChild(document.createTextNode("Cancel"));
            cancelButton.addEventListener("click", cancelButtonClicked);
            buttonContainer.appendChild(cancelButton);

            const deleteButton = document.createElement("button");
            deleteButton.appendChild(document.createTextNode("Delete"));
            deleteButton.classList.add("pull-right");
            deleteButton.addEventListener("click", deleteButtonClicked);
            buttonContainer.appendChild(deleteButton);

            dateHeading.appendChild(buttonContainer);
        }

        const addButton = document.createElement("button");
        addButton.appendChild(document.createTextNode("Add"));
        addButton.addEventListener("click", addButtonClicked);
        container.insertBefore(addButton, container.firstChild);
    }

    function addButtonClicked(ev: MouseEvent): void
    {
        const addButton = ev.target as HTMLElement;

        const dateContainer = document.createElement("div");
        dateContainer.setAttribute("data-date", "");

        const dateHeading = document.createElement("h3") as HTMLElement;
        const headingInput = document.createElement("input");
        headingInput.classList.add("input-md");
        headingInput.value = new Date().toLocaleDateString();
        dateHeading.appendChild(headingInput);

        const buttonContainer = document.createElement("div");
        buttonContainer.classList.add("pull-right");

        const saveButton = document.createElement("button");
        saveButton.appendChild(document.createTextNode("Save"));
        saveButton.addEventListener("click", saveButtonClicked);
        buttonContainer.appendChild(saveButton);

        const cancelButton = document.createElement("button");
        cancelButton.appendChild(document.createTextNode("Cancel"));
        cancelButton.addEventListener("click", cancelButtonClicked);
        buttonContainer.appendChild(cancelButton);

        dateHeading.appendChild(buttonContainer);
        dateContainer.appendChild(dateHeading);

        const list = document.createElement("ul");
        const listItem = document.createElement("li");
        const input = document.createElement("textarea");
        input.classList.add("form-control");
        input.style.maxWidth = "none";
        input.addEventListener("blur", inputBlurred);
        listItem.appendChild(input);
        list.appendChild(listItem);

        dateContainer.appendChild(list);
        addButton.parentElement.insertBefore(dateContainer, addButton.nextSibling);
    }

    function editButtonClicked(ev: MouseEvent): void
    {
        const container = getNewsEntityContainer(ev.target as HTMLElement);
        if (!container)
        {
            return;
        }

        const heading = container.querySelector("h3");
        heading.setAttribute("data-original", heading.firstChild.nodeValue);
        const headingInput = document.createElement("input");
        headingInput.classList.add("input-md");
        headingInput.value = heading.firstChild.nodeValue;
        heading.replaceChild(headingInput, heading.firstChild);

        const list = container.querySelector("ul");
        const listItems = list.querySelectorAll("li");
        for (let i = 0; i < listItems.length; i++)
        {
            const listItem = listItems[i] as HTMLLIElement;
            listItem.setAttribute("data-original", listItem.innerHTML);

            const input = document.createElement("textarea");
            input.classList.add("form-control");
            input.style.maxWidth = "none";
            input.innerHTML = listItem.innerHTML;
            input.addEventListener("blur", inputBlurred);

            listItem.innerHTML = "";
            listItem.appendChild(input);
        }

        const listItem = document.createElement("li");
        const input = document.createElement("textarea");
        input.classList.add("form-control");
        input.style.maxWidth = "none";
        input.addEventListener("blur", inputBlurred);
        listItem.appendChild(input);
        list.appendChild(listItem);

        const buttons = container.querySelectorAll("button");
        for (let i = 0; i < buttons.length; i++)
        {
            buttons[i].classList.toggle("hide");
        }
    }

    function saveButtonClicked(ev: MouseEvent): void
    {
        const container = getNewsEntityContainer(ev.target as HTMLElement);
        if (!container)
        {
            return;
        }

        const buttons = container.querySelectorAll("button");
        const originalDateStr = container.getAttribute("data-date");

        const heading = container.querySelector("h3");
        const headingInput = heading.firstChild as HTMLInputElement;
        const milliseconds = Date.parse(headingInput.value);
        if (isNaN(milliseconds))
        {
            alert("Couldn't parse the date");
            return;
        }

        // The date will be parsed as local time, so we need to convert to UTC by subtracting out the timezone offset used on that day.
        const date = new Date(milliseconds);
        const dateUtcStr = new Date(date.getTime() - (date.getTimezoneOffset() * 60 * 1000)).toJSON();

        const headingText = document.createTextNode(date.toLocaleDateString());
        heading.replaceChild(headingText, headingInput);

        const messages: string[] = [];
        const listItems = container.querySelectorAll("li");
        for (let i = 0; i < listItems.length; i++)
        {
            const listItem = listItems[i];
            const input = listItem.firstChild as HTMLInputElement;
            const message = input.value.trim();
            if (message)
            {
                messages.push(message);
                listItem.innerHTML = input.value;
                listItem.removeAttribute("data-original");
            }
            else
            {
                listItem.remove();
            }
        }

        // If the date changed, we need to delete first
        if (originalDateStr && dateUtcStr !== originalDateStr)
        {
            for (let i = 0; i < buttons.length; i++)
            {
                buttons[i].setAttribute("disabled", "disabled");
            }

            $.ajax({
                type: "delete",
                url: "/api/news/" + originalDateStr.substring(0, 10),
            })
                .done((response: ISiteNewsEntryListResponse) =>
                {
                    // TODO. Need to wait for this before doing the below
                })
                .fail(() =>
                {
                    alert("Something bad happened when deleting the old entry");
                });
        }

        for (let i = 0; i < buttons.length; i++)
        {
            buttons[i].setAttribute("disabled", "disabled");
        }

        const data =
        {
            date: dateUtcStr.substring(0, 10),
            messages: messages,
        };

        $.ajax({
            data: data,
            type: "post",
            url: "/api/news",
        })
            .done((response: ISiteNewsEntryListResponse) =>
            {
                for (let i = 0; i < buttons.length; i++)
                {
                    buttons[i].removeAttribute("disabled");
                    buttons[i].classList.toggle("hide");
                }
            })
            .fail(() =>
            {
                alert("Something bad happened inserting the new entry");
            });
    }

    function cancelButtonClicked(ev: MouseEvent): void
    {
        const container = getNewsEntityContainer(ev.target as HTMLElement);
        if (!container)
        {
            return;
        }

        const heading = container.querySelector("h3");

        if (!heading.hasAttribute("data-original"))
        {
            container.remove();
            return;
        }

        const headingText = document.createTextNode(heading.getAttribute("data-original"));
        heading.replaceChild(headingText, heading.firstChild);

        const listItems = container.querySelectorAll("li");
        for (let i = 0; i < listItems.length; i++)
        {
            const listItem = listItems[i];
            if (!listItem.hasAttribute("data-original"))
            {
                listItem.remove();
                continue;
            }

            listItem.innerHTML = listItem.getAttribute("data-original");
            listItem.removeAttribute("data-original");
        }

        const buttons = container.querySelectorAll("button");
        for (let i = 0; i < buttons.length; i++)
        {
            buttons[i].classList.toggle("hide");
        }
    }

    function deleteButtonClicked(ev: MouseEvent): void
    {
        const container = getNewsEntityContainer(ev.target as HTMLElement);
        if (!container)
        {
            return;
        }

        const dateStr = container.getAttribute("data-date");

        const buttons = container.querySelectorAll("button");
        for (let i = 0; i < buttons.length; i++)
        {
            buttons[i].setAttribute("disabled", "disabled");
        }

        $.ajax({
            type: "delete",
            url: "/api/news/" + dateStr.substring(0, 10),
        })
            .done((response: ISiteNewsEntryListResponse) =>
            {
                container.remove();
            })
            .fail(() =>
            {
                alert("Something bad happened");
            });
    }

    function inputBlurred(ev: FocusEvent): void
    {
        const blurredInput = ev.target as HTMLTextAreaElement;
        const isLast = blurredInput.parentElement.nextSibling == null;
        const isEmpty = blurredInput.value.trim().length === 0;

        if (isLast && !isEmpty)
        {
            const listItem = document.createElement("li");
            const input = document.createElement("textarea");
            input.classList.add("form-control");
            input.style.maxWidth = "none";
            input.addEventListener("blur", inputBlurred);
            listItem.appendChild(input);
            blurredInput.parentElement.parentElement.appendChild(listItem);
        }
        else if (!isLast && isEmpty)
        {
            blurredInput.parentElement.remove();
        }
    }

    function getNewsEntityContainer(element: HTMLElement): HTMLElement
    {
        let container = element;
        while (container && !container.hasAttribute("data-date"))
        {
            container = container.parentElement;
        }

        return container;
    }
}
