namespace UploadTable
{
    "use strict";

    export function create(tableId: string, count: number, usePagination: boolean): void
    {
        const table = document.getElementById(tableId);
        if (!table)
        {
            throw new Error("Element not found: " + tableId);
        }

        updateTable(table, count, usePagination);

        // Upon a hash change, re-create the table
        window.addEventListener("hashchange", () =>
        {
            window.scrollTo(0, 0);
            clearTable(table);
            updateTable(table, count, usePagination);
        });
    }

    function clearTable(table: HTMLElement): void
    {
        const tableBody = table.querySelector("tbody");
        if (tableBody)
        {
            tableBody.remove();
        }

        const tableFoot = table.querySelector("tfoot");
        if (tableFoot)
        {
            tableFoot.remove();
        }
    }

    function updateTable(table: HTMLElement, count: number, usePagination: boolean): void
    {
        const queryParameters = Helpers.getQueryParameters();
        const currentPage = parseInt(queryParameters["page"]) || 1;
        $.ajax({
            url: "/api/uploads?page=" + currentPage + "&count=" + count,
        })
            .done((response: IUploadSummaryListResponse) =>
            {
                if (!response)
                {
                    displayFailure(table);
                }

                const uploads = response.uploads;
                if (uploads)
                {
                    const tableBody = document.createElement("tbody");

                    for (let i = 0; i < uploads.length; i++)
                    {
                        const upload = uploads[i];
                        const row = document.createElement("tr");

                        const uploadTimeCell = document.createElement("td");
                        const uploadTime = new Date(upload.timeSubmitted).toLocaleString();
                        uploadTimeCell.appendChild(document.createTextNode(uploadTime));
                        row.appendChild(uploadTimeCell);

                        const viewCell = document.createElement("td");
                        viewCell.classList.add("text-right");

                        const viewLink = document.createElement("a");
                        viewLink.setAttribute("href", "/Calculator/View?uploadId=" + upload.id);
                        viewLink.appendChild(document.createTextNode("View"));

                        viewCell.appendChild(viewLink);
                        row.appendChild(viewCell);
                        tableBody.appendChild(row);
                    }

                    table.appendChild(tableBody);

                    const pagination = response.pagination;
                    if (usePagination && pagination)
                    {
                        const tableFoot = document.createElement("tfoot");
                        const row = document.createElement("tr");
                        const paginationCell = document.createElement("td");
                        paginationCell.colSpan = 2;
                        const paginationList = document.createElement("ul");
                        paginationList.classList.add("pagination");

                        const minPage = 1;
                        const maxPage = Math.ceil(pagination.count / count);

                        const previousListItem = document.createElement("li");
                        const previousLink = document.createElement("a");
                        const previousPage = currentPage - 1;
                        previousLink.appendChild(document.createTextNode("«"));
                        if (previousPage < minPage)
                        {
                            previousListItem.classList.add("disabled");
                        }
                        else
                        {
                            previousLink.href = "#page=" + previousPage;
                        }

                        previousListItem.appendChild(previousLink);
                        paginationList.appendChild(previousListItem);

                        const startPage = Math.max(minPage, Math.min(maxPage - 4, currentPage - 2));
                        const numPages = Math.min(5, maxPage - minPage);
                        for (let i = 0; i < numPages; i++)
                        {
                            const page = startPage + i;
                            const pageListItem = document.createElement("li");
                            if (currentPage === page)
                            {
                                pageListItem.classList.add("active");
                            }

                            const pageLink = document.createElement("a");
                            pageLink.href = "#page=" + page;
                            pageLink.appendChild(document.createTextNode(page.toString()));
                            pageListItem.appendChild(pageLink);
                            paginationList.appendChild(pageListItem);
                        }

                        const nextListItem = document.createElement("li");
                        const nextLink = document.createElement("a");
                        const nextPage = currentPage + 1;
                        nextLink.appendChild(document.createTextNode("»"));
                        if (nextPage > maxPage)
                        {
                            nextListItem.classList.add("disabled");
                        }
                        else
                        {
                            nextLink.href = "#page=" + nextPage;
                        }

                        nextListItem.appendChild(nextLink);
                        paginationList.appendChild(nextListItem);

                        paginationCell.appendChild(paginationList);
                        row.appendChild(paginationCell);
                        tableFoot.appendChild(row);
                        table.appendChild(tableFoot);
                    }
                }
            })
            .fail(() =>
            {
                displayFailure(table);
            });
    }

    function displayFailure(table: HTMLElement): void
    {
        // BUGBUG 51: Create Loading and Failure states for ajax loading
    }
}
