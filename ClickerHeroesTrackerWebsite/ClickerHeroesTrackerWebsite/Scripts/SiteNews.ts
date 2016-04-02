module SiteNews
{
    export function create(containerId: string, isFull: boolean): void
    {
        var container = document.getElementById(containerId);
        if (!container)
        {
            throw new Error("Element not found: " + container);
        }

        $.ajax({
            url: '/api/news'
        })
            .done((response: ISiteNewsEntryListResponse) =>
            {
                if (!response)
                {
                    displayFailure(container);
                }

                var entries = response.entries;
                if (entries)
                {
                    var maxEntries = 3;
                    var numEntries = 0;

                    // put the dates in an array so we can enumerate backwards
                    var dates: string[] = [];
                    for (var dateStr in entries)
                    {
                        dates.push(dateStr);
                    }

                    var currentDateContainer: HTMLDivElement = null;
                    var currentList: HTMLUListElement = null;
                    for (var i = dates.length - 1; i >= 0; i--)
                    {
                        var dateStr = dates[i];

                        // The date comes back as a UTC time at midnight of the date. We need to adjust for the user's local
                        // timezone offset or the date may move back by a day.
                        var dateUtc = new Date(dateStr);
                        var date = new Date(dateUtc.getUTCFullYear(), dateUtc.getUTCMonth(), dateUtc.getUTCDate()).toLocaleDateString();

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
                            var dateHeading = document.createElement("h3");
                            dateHeading.appendChild(document.createTextNode(date));
                            currentDateContainer.appendChild(dateHeading);
                        }

                        var messages = entries[dateStr];
                        for (var j = 0; j < messages.length; j++)
                        {
                            var listItem = document.createElement("li");
                            listItem.innerHTML = messages[j];
                            currentList.appendChild(listItem);

                            numEntries++;
                            if (!isFull && numEntries == maxEntries)
                            {
                                break;
                            }
                        }

                        if (!isFull && numEntries == maxEntries)
                        {
                            break;
                        }
                    }

                    container.appendChild(currentDateContainer);
                    currentDateContainer.appendChild(currentList);
                }

                if (typeof SiteNewsAdmin != "undefined")
                {
                    SiteNewsAdmin.init(container);
                }
            })
            .fail(() =>
            {
                displayFailure(container);
            });
    }

    function displayFailure(container: HTMLElement): void
    {
    }
}