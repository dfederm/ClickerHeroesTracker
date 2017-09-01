declare var userName: string;

namespace ProgressSummary
{
    "use strict";

    let element = $("#progressSummary");

    function addGraph(title: string, data: IMap<string>): void
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
                    format: `{value:,.0f}`,
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

        let graphElement = $("<div style='width:100 %; height: 400px;'></div>");
        element.append(graphElement);

        GraphConfig.renderGraph(graphElement, graph);
    }

    let queryParams = Helpers.getQueryParameters();
    let progressUserName = queryParams["userName"] || userName;

    let now = Date.now();
    let end = new Date(now);
    let start = new Date(now);
    start.setDate(start.getDate() - 7);

    $.ajax({
        url: `/api/users/${progressUserName}/progress?start=${start.toISOString()}&end=${end.toISOString()}`,
    }).done((data: IProgressData) => {
        if (Object.keys(data.soulsSpentData).length === 0)
        {
            element.text("No uploads in the last week!");
            element.addClass("text-warning");
            return;
        }

        addGraph("Souls Spent", data.soulsSpentData);
    }).fail((xhr: JQueryXHR) => {
        switch (xhr.status) {
            case 403:
                element.text("That user does not have public uploads");
                break;
            case 404:
                element.text("That user does not exist");
                break;
            case 500:
            default:
                element.text("Oops! Something went wrong while fetching user progress");
                break;
        }
        element.addClass("text-warning");
    });
}
