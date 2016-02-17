module UploadTable
{
    export function create(tableId: string, count: number, usePagination: boolean): void
    {
        var table = document.getElementById(tableId);
        if (!table)
        {
            throw new Error("Element not found: " + tableId);
        }

        updateTable(table, count, usePagination);

        // Upon a hash change, re-create the table
        window.addEventListener('hashchange', () =>
        {
            window.scrollTo(0, 0);
            clearTable(table);
            updateTable(table, count, usePagination);
        });
    }

    function clearTable(table: HTMLElement)
    {
        var tableBody = table.querySelector('tbody');
        if (tableBody)
        {
            tableBody.remove();
        }

        var tableFoot = table.querySelector('tfoot');
        if (tableFoot)
        {
            tableFoot.remove();
        }
    }

    function updateTable(table: HTMLElement, count: number, usePagination: boolean)
    {
        var queryParameters = getQueryParameters();
        var currentPage = parseInt(queryParameters['page']) || 1;
        $.ajax({
            url: '/api/uploads?page=' + currentPage + '&count=' + count
        })
            .done((response: IUploadSummaryListResponse) =>
            {
                if (!response)
                {
                    displayFailure(table);
                }

                var uploads = response.uploads;
                if (uploads)
                {
                    var tableBody = document.createElement('tbody');

                    for (var i = 0; i < uploads.length; i++)
                    {
                        var upload = uploads[i];
                        var row = document.createElement('tr');

                        var uploadTimeCell = document.createElement('td');
                        var uploadTime = new Date(upload.timeSubmitted).toLocaleString();
                        uploadTimeCell.appendChild(document.createTextNode(uploadTime));
                        row.appendChild(uploadTimeCell);

                        var viewCell = document.createElement('td');
                        viewCell.classList.add("text-right");

                        var viewLink = document.createElement('a');
                        viewLink.setAttribute('href', '/Calculator/View?uploadId=' + upload.id);
                        viewLink.appendChild(document.createTextNode('View'));

                        viewCell.appendChild(viewLink);
                        row.appendChild(viewCell);
                        tableBody.appendChild(row);
                    }

                    table.appendChild(tableBody);

                    var pagination = response.pagination;
                    if (usePagination && pagination)
                    {
                        var tableFoot = document.createElement('tfoot');
                        var row = document.createElement('tr');
                        var paginationCell = document.createElement('td');
                        paginationCell.colSpan = 2;
                        var paginationList = document.createElement('ul');
                        paginationList.classList.add('pagination');

                        var minPage = 1;
                        var maxPage = Math.ceil(pagination.count / count);

                        var previousListItem = document.createElement('li');
                        var previousLink = document.createElement('a');
                        var previousPage = currentPage - 1;
                        previousLink.appendChild(document.createTextNode('«'));
                        if (previousPage < minPage)
                        {
                            previousListItem.classList.add('disabled');
                        }
                        else
                        {
                            previousLink.href = '#page=' + previousPage;
                        }

                        previousListItem.appendChild(previousLink);
                        paginationList.appendChild(previousListItem);

                        var startPage = Math.max(minPage, Math.min(maxPage - 4, currentPage - 2));
                        var numPages = Math.min(5, maxPage - minPage);
                        for (var i = 0; i < numPages; i++)
                        {
                            var page = startPage + i;
                            var pageListItem = document.createElement('li');
                            if (currentPage == page)
                            {
                                pageListItem.classList.add('active');
                            }

                            var pageLink = document.createElement('a');
                            pageLink.href = '#page=' + page;
                            pageLink.appendChild(document.createTextNode(page.toString()));
                            pageListItem.appendChild(pageLink);
                            paginationList.appendChild(pageListItem);
                        }

                        var nextListItem = document.createElement('li');
                        var nextLink = document.createElement('a');
                        var nextPage = currentPage + 1;
                        nextLink.appendChild(document.createTextNode('»'));
                        if (nextPage > maxPage)
                        {
                            nextListItem.classList.add('disabled');
                        }
                        else
                        {
                            nextLink.href = '#page=' + nextPage;
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
    }
}