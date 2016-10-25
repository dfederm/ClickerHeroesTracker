namespace SiteNews
{
    "use strict";

    export function create(containerId: string, isFull: boolean): void
    {
        const container = document.getElementById(containerId);
        if (!container)
        {
            throw new Error("Element not found: " + container);
        }

        $.ajax({
            url: "/api/news",
        })
            .done((response: ISiteNewsEntryListResponse) =>
            {
                if (!response)
                {
                    displayFailure(container);
                }

                const entries = response.entries;
                if (entries)
                {
                    const maxEntries = 3;
                    let numEntries = 0;

                    // Put the dates in an array so we can enumerate backwards
                    const dates: string[] = [];
                    for (let dateStr in entries)
                    {
                        dates.push(dateStr);
                    }

                    let currentDateContainer: HTMLDivElement = null;
                    let currentList: HTMLUListElement = null;
                    for (let i = dates.length - 1; i >= 0; i--)
                    {
                        const dateStr = dates[i];

                        // The date comes back as a UTC time at midnight of the date. We need to adjust for the user's local timezone offset or the date may move back by a day.
                        const dateUtc = new Date(dateStr);
                        const date = new Date(dateUtc.getUTCFullYear(), dateUtc.getUTCMonth(), dateUtc.getUTCDate()).toLocaleDateString();

                        if (isFull || !currentList)
                        {
                            if (currentList)
                            {
                                currentDateContainer.appendChild(currentList);
                            }

                            currentList = document.createElement("ul");
                        }

                        if (isFull || !currentDateContainer)
                        {
                            if (currentDateContainer)
                            {
                                container.appendChild(currentDateContainer);
                            }

                            currentDateContainer = document.createElement("div");
                            if (isFull)
                            {
                                currentDateContainer.setAttribute("data-date", dateStr);
                            }
                        }

                        if (isFull)
                        {
                            const dateHeading = document.createElement("h3");
                            dateHeading.appendChild(document.createTextNode(date));
                            currentDateContainer.appendChild(dateHeading);
                        }

                        const messages = entries[dateStr];
                        for (let j = 0; j < messages.length; j++)
                        {
                            const listItem = document.createElement("li");
                            listItem.innerHTML = messages[j];
                            currentList.appendChild(listItem);

                            numEntries++;
                            if (!isFull && numEntries === maxEntries)
                            {
                                break;
                            }
                        }

                        if (!isFull && numEntries === maxEntries)
                        {
                            break;
                        }
                    }

                    if (currentDateContainer)
                    {
                        currentDateContainer.appendChild(currentList);
                        container.appendChild(currentDateContainer);
                    }
                }

                if (typeof SiteNewsAdmin !== "undefined")
                {
                    SiteNewsAdmin.init(container);
                }
            })
            .fail((): void =>
            {
                displayFailure(container);
            });
    }

    function displayFailure(container: HTMLElement): void
    {
        // BUGBUG 51: Create Loading and Failure states for ajax loading
    }
}
