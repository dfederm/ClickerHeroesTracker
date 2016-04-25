$("#addUpload").submit(function (event: JQueryEventObject): boolean
{
    function handleSuccess(result: string): void
    {
        const uploadId = parseInt(result);
        if (uploadId)
        {
            // Redirect to the calculator page
            window.location.href = "/calculator/view?uploadId=" + uploadId;
        }
    }

    function handleError(xhr: JQueryXHR): void
    {
        const statusCode = xhr.status;
        let errorMessage: string;
        if (statusCode >= 400 && statusCode < 500)
        {
            errorMessage = "The uploaded save was not valid";
        }
        else
        {
            errorMessage = "An unknown error occurred";
        }

        // Custom error message
        const container = $("span[data-valmsg-for='EncodedSaveData']");
        container.removeClass("field-validation-valid").addClass("field-validation-error");
        container.text(errorMessage);

        $("#addUploadSubmit").removeAttr("disabled");
    }

    const form = event.target as HTMLFormElement;
    if ($(form).valid())
    {
        $("#addUploadSubmit").attr("disabled", "disabled");
        $.ajax({
            data: $(form).serialize(),
            error: handleError,
            success: handleSuccess,
            type: form.method,
            url: form.action,
        });
    }

    return false;
});
