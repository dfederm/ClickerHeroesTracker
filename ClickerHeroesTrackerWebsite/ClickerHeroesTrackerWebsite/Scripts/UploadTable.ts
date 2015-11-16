module UploadTable
{
    export function create(tableId: string): void
    {
        var table = document.getElementById(tableId);
        if (!table)
        {
            throw new Error("Element not found: " + tableId);
        }

        $.ajax({
            url: '/api/uploads'
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