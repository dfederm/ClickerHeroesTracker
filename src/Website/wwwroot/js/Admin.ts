declare var staleUploads: string[];

$("#deleteStaleUploads").click(function (event: JQueryEventObject): boolean
{
    $("#deleteStaleUploads").attr("disabled", "true");

    let progressBar = $("#deleteStaleUploadsProgress");
    let totalStaleUploads = staleUploads ? staleUploads.length : 0;

    function deleteNextUpload(): void
    {
        if (!staleUploads || staleUploads.length === 0)
        {
            return;
        }

        let uploadId = staleUploads.shift();
        $.ajax({
            method: "DELETE",
            url: "/api/uploads/" + uploadId,
        })
        .done(() =>
        {
            updateProgress();
            deleteNextUpload();
        });
    }

    function updateProgress(): void
    {
        let percentDone = 100 - (100 * staleUploads.length / totalStaleUploads);
        progressBar.css("width", percentDone.toFixed(2) + "%");
        progressBar.html(percentDone.toFixed(2) + "%");
    }

    const numParallelDeletes = 4;
    for (let i = 0; i < numParallelDeletes; i++)
    {
        deleteNextUpload();
    }

    return false;
});
