namespace Pagination
{
    "use strict";

    export function create(pagination: IPaginationMetadata, count: number, currentPage: number): HTMLElement
    {
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
        const numPages = Math.min(5, maxPage - minPage + 1);
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

        return paginationList;
    }
}
