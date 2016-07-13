namespace AjaxLoader {
    "use strict";

    // Get the loading div as variable
    const loadingElement = $("#loadingElement").hide();

    // Show and hide loading spinner on Ajax start/stop.
    $(document)
        .ajaxStart(function (): void
        {
            loadingElement.show();
        })
        .ajaxStop(function (): void
        {
            loadingElement.hide();
        });
}
