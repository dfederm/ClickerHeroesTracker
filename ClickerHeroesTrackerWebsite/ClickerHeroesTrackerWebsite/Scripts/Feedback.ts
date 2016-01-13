// On hiding the modal, reset it.
$('#feedbackModal').on('hidden.bs.modal', (e) =>
{
    $('#feedbackComments').val('');
    $('#feedbackSubmit').removeAttr('disabled');

    // Clear any errors
    var container = $('span[data-valmsg-for="feedbackComments"]');
    container.addClass('field-validation-valid').removeClass('field-validation-error');
    container.text('');
});

$('#feedbackForm').submit(function ()
{
    var form = <HTMLFormElement>this;
    if ($(form).valid())
    {
        $('#feedbackSubmit').attr('disabled', 'disabled');
        $.ajax({
            url: form.action,
            type: form.method,
            data: $(form).serialize(),
            success: (result: string) =>
            {
                $('#feedbackModal').modal('hide');
            },
            error: (xhr) =>
            {
                var statusCode = xhr.status;

                // Custom error message
                var container = $('span[data-valmsg-for="feedbackComments"]');
                container.removeClass('field-validation-valid').addClass('field-validation-error');
                container.text('Something went wrong. Please try again later.');

                $('#feedbackSubmit').removeAttr('disabled');
            },
        });
    }

    return false;
});
