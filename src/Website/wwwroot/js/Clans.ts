namespace Clans
{
    "use strict";

    const leaderboardCount = 10;

    function buildClanInformation(response: IClanData, textStatus: string, xhr: JQueryXHR): void
    {
        const clanMemebersTable = document.getElementById("clan-members-table") as HTMLTableElement;

        if (xhr.status === 204)
        {
            document.getElementById("clan-name").appendChild(document.createTextNode("Please join a clan to view the clan's data"));
            clanMemebersTable.classList.add("hidden");
            return;
        }

        document.getElementById("clan-name").appendChild(document.createTextNode(response.clanName));
        document.getElementById("clan-messages").classList.remove("hidden");

        const bodyElement = clanMemebersTable.tBodies[0];
        for (let i = 0; i < response.guildMembers.length; i++)
        {
            const guildMember = response.guildMembers[i];

            const nameCell = document.createElement("td");
            nameCell.appendChild(document.createTextNode(guildMember.nickname));

            const highestZoneCell = document.createElement("td");
            highestZoneCell.appendChild(document.createTextNode(guildMember.highestZone.toString()));

            const row = document.createElement("tr");
            row.appendChild(nameCell);
            row.appendChild(highestZoneCell);

            bodyElement.appendChild(row);
        }

        const minuteInMilliSeconds = 1000 * 60;
        const hourInMilliSeconds = minuteInMilliSeconds * 60;
        const dayInMilliSeconds = hourInMilliSeconds * 24;
        const clanMessagesElement = document.getElementById("clan-messages");
        for (let i = 0; i < response.messages.length; i++)
        {
            const message = response.messages[i];

            const messageDate = new Date(message.date);
            const nowDate = new Date();
            const timeDiff = Math.abs(nowDate.getTime() - messageDate.getTime());
            const diffDays = Math.ceil(timeDiff / dayInMilliSeconds);

            const messageElement = document.createElement("div");
            messageElement.classList.add("col-xs-12", "clan-message");

            const messageHeaderElement = document.createElement("p");

            let displayTimeDiff: string;
            if (timeDiff > dayInMilliSeconds)
            {
                displayTimeDiff = diffDays + " days";
            }
            else if (timeDiff > hourInMilliSeconds)
            {
                displayTimeDiff = Math.ceil(timeDiff / hourInMilliSeconds) + " hours";
            }
            else if (timeDiff > minuteInMilliSeconds)
            {
                displayTimeDiff = Math.ceil(timeDiff / minuteInMilliSeconds) + " minutes";
            }
            else
            {
                displayTimeDiff = Math.ceil(timeDiff / 1000) + " seconds";
            }

            let username = message.username;
            if (username === null)
            {
                username = "(Unknown)";
                messageHeaderElement.classList.add("text-muted");
            }

            messageHeaderElement.appendChild(document.createTextNode("(" + displayTimeDiff + " ago) " + username));
            messageElement.appendChild(messageHeaderElement);

            const messageContentElement = document.createElement("p");
            messageContentElement.appendChild(document.createTextNode(message.content));
            messageElement.appendChild(messageContentElement);

            clanMessagesElement.appendChild(messageElement);
        }

        const clanNameHiddenElement = document.createElement("input");
        clanNameHiddenElement.type = "hidden";
        clanNameHiddenElement.name = "clanName";
        clanNameHiddenElement.value = response.clanName;
        document.getElementById("sendMessage").appendChild(clanNameHiddenElement);
    }

    function clearTable(table: HTMLElement): void
    {
        const tableBody = table.querySelector("tbody");
        if (tableBody)
        {
            tableBody.parentNode.removeChild(tableBody);
        }

        const tableFoot = table.querySelector("tfoot");
        if (tableFoot)
        {
            tableFoot.parentNode.removeChild(tableFoot);
        }
    }

    function updateLeaderboard(table: HTMLElement, userClanRow: HTMLTableRowElement): void
    {
        const queryParameters = Helpers.getQueryParameters();
        const currentPage = parseInt(queryParameters["page"]) || 1;
        $.ajax({
            url: "/api/clans/leaderboard?page=" + currentPage + "&count=" + leaderboardCount,
        }).done((response: ILeaderboardSummaryListResponse): void =>
        {
            const tablebody = document.createElement("tbody");

            for (let i = 0; i < response.leaderboardClans.length; i++)
            {
                const clan = response.leaderboardClans[i];

                const row = document.createElement("tr");
                if (clan.isUserClan)
                {
                    row.classList.add("highlighted-clan");
                }

                const rankCell = document.createElement("td");
                rankCell.appendChild(document.createTextNode(clan.rank.toString()));
                row.appendChild(rankCell);

                const nameCell = document.createElement("td");
                nameCell.appendChild(document.createTextNode(clan.name));
                row.appendChild(nameCell);

                const memberCountCell = document.createElement("td");
                if (clan.memberCount > 0)
                {
                    memberCountCell.appendChild(document.createTextNode(clan.memberCount.toString()));
                }

                row.appendChild(memberCountCell);

                const currentRaidLevelCell = document.createElement("td");
                currentRaidLevelCell.appendChild(document.createTextNode(clan.currentRaidLevel.toString()));
                row.appendChild(currentRaidLevelCell);

                tablebody.appendChild(row);
            }

            if (userClanRow !== null && userClanRow.firstChild !== null)
            {
                const rank = parseInt(userClanRow.firstChild.textContent);
                if (rank < (currentPage * leaderboardCount - leaderboardCount) || rank > (currentPage * leaderboardCount))
                {
                    tablebody.appendChild(userClanRow);
                }
            }

            table.appendChild(tablebody);

            const pagination = response.pagination;
            if (pagination)
            {
                const tableFoot = document.createElement("tfoot");
                const row = document.createElement("tr");
                const paginationCell = document.createElement("td");
                paginationCell.colSpan = 4;

                const paginationList = Pagination.create(pagination, leaderboardCount, currentPage);

                paginationCell.appendChild(paginationList);
                row.appendChild(paginationCell);
                tableFoot.appendChild(row);
                table.appendChild(tableFoot);
            }
        });
    }

    $.ajax({
        url: "/api/clans",
    })
        .done(buildClanInformation)
        .fail((jqXHR: JQueryXHR, ajaxOptions: string, error: string) =>
        {
            if (jqXHR.status === 404)
            {
                const clanMemebersElement = document.getElementById("clan-members");
                while (clanMemebersElement.hasChildNodes())
                {
                    clanMemebersElement.removeChild(clanMemebersElement.firstChild);
                }

                const errorElement = document.createElement("h2");
                errorElement.appendChild(document.createTextNode("Please upload a save to view your clan data"));
                clanMemebersElement.appendChild(errorElement);
            }
        });

    const table = document.getElementById("leaderboard-table");

    $.ajax({
        url: "/api/clans/userClan",
    }).done((response: ILeaderboardClan, textStatus: string, xhr: JQueryXHR) =>
    {
        let userClanRow: HTMLTableRowElement = null;
        if (xhr.status !== 204)
        {
            userClanRow = document.createElement("tr");
            userClanRow.classList.add("highlighted-clan");

            const rankCell = document.createElement("td");
            rankCell.appendChild(document.createTextNode(response.rank.toString()));
            userClanRow.appendChild(rankCell);

            const nameCell = document.createElement("td");
            nameCell.appendChild(document.createTextNode(response.name));
            userClanRow.appendChild(nameCell);

            const memberCountCell = document.createElement("td");
            if (response.memberCount > 0)
            {
                memberCountCell.appendChild(document.createTextNode(response.memberCount.toString()));
            }

            userClanRow.appendChild(memberCountCell);

            const currentRaidLevelCell = document.createElement("td");
            currentRaidLevelCell.appendChild(document.createTextNode(response.currentRaidLevel.toString()));
            userClanRow.appendChild(currentRaidLevelCell);
        }

        updateLeaderboard(table, userClanRow);

        window.addEventListener("hashchange", () =>
        {
            window.scrollTo(0, 0);
            clearTable(table);
            updateLeaderboard(table, userClanRow);
        });
    });

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
