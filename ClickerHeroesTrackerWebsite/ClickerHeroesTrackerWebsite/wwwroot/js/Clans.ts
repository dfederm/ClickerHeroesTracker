namespace Clans {
    "use strict";

    $.ajax({
        url: "/api/clans/leaderboard",
    }).done((response: Array<ILeaderboardClan>) => {
        $("#leaderboard-table").prepend("<tr><th>Rank</th><th>Name</th><th>Members</th><th>Current Raid Level</th></tr>");
        for (let index = 0; index < response.length; ++index)
        {
            const clan = response[index];

            if (clan.memberCount > 0)
            {
                $("#leaderboard-table-body").append("<tr><td>" + (index + 1) + "</td><td>" + clan.name + "</td><td>" + clan.memberCount + "</td><td>" + clan.currentRaidLevel + "</td></tr>");
            }
            else
            {
                $("#leaderboard-table-body").append("<tr><td>" + (index + 1) + "</td><td>" + clan.name + "</td><td></td><td>" + clan.currentRaidLevel + "</td></tr>");

            }
        }
    });

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

            if (timeDiff > dayInMilliSeconds)
            {
                $(".timeName").last().text("(" + diffDays + " days ago) " + message.username);
            }
            else if (timeDiff > hourInMilliSeconds)
            {
                $(".timeName").last().text("(" + Math.ceil(timeDiff / hourInMilliSeconds) + " hours ago) " + message.username);
            }
            else if (timeDiff > minuteInMilliSeconds)
            {
                $(".timeName").last().text("(" + Math.ceil(timeDiff / minuteInMilliSeconds) + " minutes ago) " + message.username);
            }
            else
            {
                $(".timeName").last().text("(" + Math.ceil(timeDiff / 1000) + " seconds ago) " + message.username);
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
