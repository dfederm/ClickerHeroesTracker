namespace Clans {
    "use strict";

    $.ajax({
        url: "/api/clans",
    }).done((response: IClanData, textStatus: string, jqXHR: JQueryXHR) =>
    {
        if (jqXHR.status === 204) {
            $("#clan-members").html("<h2>Please join a clan to view this data</h2>");
            return;
        }

        $("#clan-name").html(response.clanName);
        $("#clan-members-table").prepend("<tr><th>Name</th> <th>Highest Zone</th></tr>");
        $("#clan-messages").removeClass("hidden");

        for (let index = 0; index < Object.keys(response.guildMembers).length; ++index)
        {
            $("#clan-members-table-body").append("<tr><td>" + response.guildMembers[index].nickname + "</td><td>" + response.guildMembers[index].highestZone + "</td></tr>");
        }

        for (let index = 0; index < response.messages.length; ++index)
        {
            const message = response.messages[index];
            const minuteInMilliSeconds = 1000 * 60;
            const hourInMilliSeconds = minuteInMilliSeconds * 60;
            const dayInMilliSeconds = hourInMilliSeconds * 24;

            let date1 = new Date(message.date);
            let date2 = new Date();
            let timeDiff = Math.abs(date2.getTime() - date1.getTime());
            let diffDays = Math.ceil(timeDiff / dayInMilliSeconds);

            $("#clan-messages").append("<div class='col-xs-12 clan-message'><p class='timeName'></p><p class='messageContent'></p></div>");

            let username = "";
            let timeName = $(".timeName").last();

            if (message.username == null)
            {
                username = "(Unknown)";
                timeName.addClass("text-muted");
            }
            else
            {
                username = message.username;
            }

            if (timeDiff > dayInMilliSeconds)
            {
                timeName.text("(" + diffDays + " days ago) " + username);
            }
            else if (timeDiff > hourInMilliSeconds)
            {
                timeName.text("(" + Math.ceil(timeDiff / hourInMilliSeconds) + " hours ago) " + username);
            }
            else if (timeDiff > minuteInMilliSeconds)
            {
                timeName.text("(" + Math.ceil(timeDiff / minuteInMilliSeconds) + " minutes ago) " + username);
            }
            else
            {
                timeName.text("(" + Math.ceil(timeDiff / 1000) + " seconds ago) " + username);
            }

            $(".messageContent").last().text(message.content);
        }

        $("form").append("<input type='hidden' name='clanName' value='" + response.clanName + "' />");
    }).fail((jqXHR: JQueryXHR, ajaxOptions: string, error: string) => {
        if (jqXHR.status === 404) {
            $("#clan-members").html("<h2>Please upload a save to view this data</h2>");
            return;
        }
    });

    export function createLeaderboard(count: number, tableId: string): void
    {
        const table = document.getElementById(tableId);
        if (!table) {
            throw new Error("Element not found: " + tableId);
        }

        updateLeaderboard(count, table);

        // Upon a hash change, re-create the table
        window.addEventListener("hashchange", () => {
            window.scrollTo(0, 0);
            clearTable(table);
            updateLeaderboard(count, table);
        });
    }

    function clearTable(table: HTMLElement): void {
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

    function updateLeaderboard(count: number, table: HTMLElement): void
    {
        const queryParameters = Helpers.getQueryParameters();
        const currentPage = parseInt(queryParameters["page"]) || 1;

        $.ajax({
            url: "/api/clans/leaderboard?page=" + currentPage + "&count=" + count,
        }).done((response: ILeaderboardSummaryListResponse) =>
        {
            $("#leaderboard-table").append("<tbody class='leaderboard-table-body' id='leaderboard-table-body'></tbody>");
            const tablebody = document.getElementById("leaderboard-table-body");

            for (let index = 0; index < response.leaderboardClans.length; ++index)
            {
                const clan = response.leaderboardClans[index];

                const row = document.createElement("tr");
                if (clan.isUserClan) {
                    row.classList.add("highlighted-clan");
                }

                const rankCell = document.createElement("td");
                rankCell.appendChild(document.createTextNode(clan.rank.toString()));
                row.appendChild(rankCell);

                const nameCell = document.createElement("td");
                nameCell.appendChild(document.createTextNode(clan.name));
                row.appendChild(nameCell);

                const memberCountCell = document.createElement("td");
                if (clan.memberCount > 0) {
                    memberCountCell.appendChild(document.createTextNode(clan.memberCount.toString()));
                }

                row.appendChild(memberCountCell);

                const currentRaidLevelCell = document.createElement("td");
                currentRaidLevelCell.appendChild(document.createTextNode(clan.currentRaidLevel.toString()));
                row.appendChild(currentRaidLevelCell);

                tablebody.appendChild(row);
            }

            const pagination = response.pagination;

            if (pagination)
            {
                const tableFoot = document.createElement("tfoot");
                const row = document.createElement("tr");
                const paginationCell = document.createElement("td");
                paginationCell.colSpan = 4;

                const paginationList = Pagination.create(pagination, count, currentPage);

                paginationCell.appendChild(paginationList);
                row.appendChild(paginationCell);
                tableFoot.appendChild(row);
                table.appendChild(tableFoot);
            }
        });
    }

    $("#sendMessage").submit(function (event: JQueryEventObject): boolean
    {
        function handleSuccess(result: string): void
        {
            Helpers.showMessage("Message successfully sent to clan", "success");
        }

        function handleError(xhr: JQueryXHR): void
        {
            Helpers.showMessage("Could not send message to clan", "error");
        }

        const form = event.target as HTMLFormElement;
        console.log($(form).serialize());
        if ($(form).valid())
        {
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
}
