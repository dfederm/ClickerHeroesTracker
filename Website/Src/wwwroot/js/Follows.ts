declare var userName: string;

namespace Follows {
    "use strict";

    let element = $("#follows");

    function handleData(data: IFollowsData): void {
        if (!data || !data.follows || !data.follows.length) {
            let message = $("<p>You are currently not following any users. This feature is coming soon.</p>");
            element.append(message);
            return;
        }

        let table = $("<table class='table'></table>");

        for (let i = 0; i < data.follows.length; i++) {
            let follow = data.follows[i];

            let row = $(`<tr><td>${follow}</td><td class="text-right"><a href="/dashboard/compare?userName1=${userName}&userName2=${follow}">Compare</a></td></tr>`);
            table.append(row);
        }

        element.append(table);
    }

    function handleError(): void {
        let error = $("<p class='text-danger'>There was a problem fetching followed users.</p>");
        element.append(error);
    }

    let queryParams = Helpers.getQueryParameters();
    let progressUserName = queryParams["userName"] || userName;

    $.ajax({
        url: `/api/users/${progressUserName}/follows`,
    }).done(handleData)
    .fail(handleError);
}
