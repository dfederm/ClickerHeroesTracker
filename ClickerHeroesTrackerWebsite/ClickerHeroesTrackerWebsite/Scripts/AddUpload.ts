$('#addUpload').submit(function ()
{
    var form = <HTMLFormElement>this;
    if ($(form).valid())
    {
        $('#addUploadSubmit').attr('disabled', 'disabled');
        $.ajax({
            url: form.action,
            type: form.method,
            data: $(form).serialize(),
            success: (result: string) =>
            {
                var uploadId = parseInt(result);
                if (uploadId)
                {
                    // redirect to the calculator page
                    window.location.href = '/calculator/view?uploadId=' + uploadId;
                }
            },
            error: (xhr) =>
            {
                var statusCode = xhr.status;
                var errorMessage;
                if (statusCode >= 400 && statusCode < 500)
                {
                    errorMessage = 'The uploaded save was not valid';
                }
                else
                {
                    errorMessage = 'An unknown error occurred';
                }

                // Custom error message
                var container = $('span[data-valmsg-for="EncodedSaveData"]');
                container.removeClass('field-validation-valid').addClass('field-validation-error');
                container.text(errorMessage);

                $('#addUploadSubmit').removeAttr('disabled');
            },
        });
    }

    return false;
});
