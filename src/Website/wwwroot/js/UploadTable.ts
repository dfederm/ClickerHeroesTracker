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
                        const paginationList = Pagination.create(pagination, count, currentPage);

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
