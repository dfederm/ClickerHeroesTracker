declare var userName: string;

namespace Progress
{
    "use strict";

    let titleElement = $("h2");
    let queryParams = Helpers.getQueryParameters();
    let progressUserName = queryParams["userName"] || userName;

    let now = Date.now();
    let end = new Date(now);
    let start = new Date(now);

    let range = queryParams["range"];
    switch (range) {
        case "1d":
            start.setDate(start.getDate() - 1);
            break;
        case "3d":
            start.setDate(start.getDate() - 3);
            break;
        case "1m":
            start.setMonth(start.getMonth() - 1);
            break;
        case "3m":
            start.setMonth(start.getMonth() - 3);
            break;
        case "1y":
            start.setFullYear(start.getFullYear() - 1);
            break;
        case "1w":
        default:
            start.setDate(start.getDate() - 7);
            break;
    }

    $.ajax({
        url: `/api/users/${progressUserName}/progress?start=${start.toISOString()}&end=${end.toISOString()}`,
    }).done((data: IProgressData) => {
        if (Object.keys(data.soulsSpentData).length === 0) {
            titleElement.text("This user has no uploaded data for that time period");
            return;
        }

        addGraph("Souls Spent", "col-md-6", data.soulsSpentData);
        addGraph("Titan Damage", "col-md-6", data.titanDamageData);
        addGraph("Hero Souls Sacrificed", "col-md-6", data.heroSoulsSacrificedData);
        addGraph("Total Ancient Souls", "col-md-6", data.totalAncientSoulsData);
        addGraph("Transcendent Power", "col-md-6", data.transcendentPowerData, 2);
        addGraph("Rubies", "col-md-6", data.rubiesData);
        addGraph("Highest Zone This Transcension", "col-md-6", data.highestZoneThisTranscensionData);
        addGraph("Highest Zone Lifetime", "col-md-6", data.highestZoneLifetimeData);
        addGraph("Ascensions This Transcension", "col-md-6", data.ascensionsThisTranscensionData);
        addGraph("Ascensions Lifetime", "col-md-6", data.ascensionsLifetimeData);

        for (let outsider in data.outsiderLevelData) {
            addGraph(outsider.toTitleCase(), "col-md-4", data.outsiderLevelData[outsider]);
        }

        for (let ancient in data.ancientLevelData) {
            addGraph(ancient.toTitleCase(), "col-md-4", data.ancientLevelData[ancient]);
        }
    }).fail((xhr: JQueryXHR) => {
        switch (xhr.status) {
            case 403:
                titleElement.text("That user does not have public uploads");
                break;
            case 404:
                titleElement.text("That user does not exist");
                break;
            case 500:
            default:
                titleElement.text("Oops! Something went wrong while fetching user progress");
                break;
        }
    });

    function addGraph(title: string, className: string, data: IMap<string>, numDecimals: number = 0): void
    {
        if (!data)
        {
            return;
        }

        let min = new Decimal(Infinity);
        let max = new Decimal(0);

        let seriesData: [number, number][] = [];
        for (let i in data)
        {
            let time = Date.parse(i);
            let value = new Decimal(data[i]);

            if (min.greaterThan(value)) {
                min = value;
            }

            if (max.lessThan(value)) {
                max = value;
            }

            seriesData.push([time, value.toNumber()]);
        }

        if (!seriesData.length) {
            return;
        }

        let lastTime = new Date().setMilliseconds(0); // Set milliseconds to 0 so they don't render
        seriesData.push([lastTime, seriesData[seriesData.length - 1][1]]);

        let yAxisType = userSettings.useLogarithmicGraphScale && max.minus(min).greaterThan(userSettings.logarithmicGraphScaleThreshold)
            ? "logarithmic"
            : "linear";

        let graph: Highcharts.Options = {
            chart:  {
                type: "line",
            },
            title: {
                text: title,
            },
            xAxis: {
                tickInterval: 24 * 3600 * 1000, // One day
                type: "datetime",
                tickWidth: 0,
                gridLineWidth: 1,
                labels: {
                    align: "left",
                    x: 3,
                    y: -3,
                    format: "{value:%m/%d}",
                },
            },
            yAxis: {
                labels: {
                    align: "left",
                    x: 3,
                    y: 16,
                    format: `{value:,.${numDecimals}f}`,
                },
                showFirstLabel: false,
                type: yAxisType,
            },
            legend: {
                enabled: false,
            },
            series: [{
                color: "#7cb5ec",
                data: seriesData,
            }],
        };

        let graphContainer = $("<div></div>");
        graphContainer.addClass(className);

        let graphElement = $("<div style='width:100 %; height: 400px;'><div>");

        graphContainer.append(graphElement);
        $("#graphs").append(graphContainer);

        GraphConfig.renderGraph(graphElement, graph);
    }
}
