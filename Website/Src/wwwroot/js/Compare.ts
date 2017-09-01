namespace Compare
{
    "use strict";

    let titleElement = $("h2");
    let userData1: IProgressData;
    let userData2: IProgressData;

    let queryParams = Helpers.getQueryParameters();
    let userName1 = queryParams["userName1"];
    let userName2 = queryParams["userName2"];

    if (!userName1 || !userName2) {
        titleElement.text("Two users are required to compare");
    }

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
        url: `/api/users/${userName1}/progress?start=${start.toISOString()}&end=${end.toISOString()}`,
    }).done((data: IProgressData) => {
        userData1 = data;
        handleData();
    }).fail((xhr: JQueryXHR) => {
        switch (xhr.status) {
            case 403:
                titleElement.text(`${userName1}'s data is private and may not be viewed`);
                break;
            case 404:
                titleElement.text(`User ${userName1} does not exist`);
                break;
            case 500:
            default:
                titleElement.text("Oops! Something went wrong while fetching user progress");
                break;
        }
    });
    $.ajax({
        url: `/api/users/${userName2}/progress?start=${start.toISOString()}&end=${end.toISOString()}`,
    }).done((data: IProgressData) => {
        userData2 = data;
        handleData();
    }).fail((xhr: JQueryXHR) => {
        switch (xhr.status) {
            case 403:
                titleElement.text(`${userName2}'s data is private and may not be viewed`);
                break;
            case 404:
                titleElement.text(`User ${userName2} does not exist`);
                break;
            case 500:
            default:
                titleElement.text("Oops! Something went wrong while fetching user progress");
                break;
        }
    });

    function addGraph(title: string, className: string, data1: IMap<string>, data2: IMap<string>, numDecimals: number = 0): void
    {
        if (!data1 && !data2)
        {
            return;
        }

        let min = new Decimal(Infinity);
        let max = new Decimal(0);
        let lastTime = new Date().setMilliseconds(0); // Set milliseconds to 0 so they don't render

        let series: Highcharts.IndividualSeriesOptions[] = [];

        let series1Data: [number, number][] = [];
        for (let i in data1) {
            let time = Date.parse(i);
            let value = new Decimal(data1[i]);

            if (min.greaterThan(value)) {
                min = value;
            }

            if (max.lessThan(value)) {
                max = value;
            }

            series1Data.push([time, value.toNumber()]);
        }

        if (series1Data.length) {
            series1Data.push([lastTime, series1Data[series1Data.length - 1][1]]);
            series.push({
                name: userName1,
                color: "#7cb5ec",
                data: series1Data,
            });
        }

        let series2Data: [number, number][] = [];
        for (let i in data2) {
            let time = Date.parse(i);
            let value = new Decimal(data2[i]);

            if (min.greaterThan(value)) {
                min = value;
            }

            if (max.lessThan(value)) {
                max = value;
            }

            series2Data.push([time, value.toNumber()]);
        }

        if (series2Data.length) {
            series2Data.push([lastTime, series2Data[series2Data.length - 1][1]]);
            series.push({
                name: userName2,
                color: "#a94442",
                data: series2Data,
            });
        }

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
            series: series,
        };

        let graphContainer = $("<div></div>");
        graphContainer.addClass(className);

        let graphElement = $("<div style='width:100 %; height: 400px;'><div>");

        graphContainer.append(graphElement);
        $("#graphs").append(graphContainer);

        GraphConfig.renderGraph(graphElement, graph);
    }

    function handleData(): void
    {
        if (!userData1 || !userData2) {
            return;
        }

        if (Object.keys(userData1.soulsSpentData).length === 0
            && Object.keys(userData2.soulsSpentData).length === 0) {
            titleElement.text("These users have no uploaded data for that time period");
            return;
        }

        addGraph("Souls Spent", "col-md-6", userData1.soulsSpentData, userData2.soulsSpentData);
        addGraph("Titan Damage", "col-md-6", userData1.titanDamageData, userData2.titanDamageData);
        addGraph("Hero Souls Sacrificed", "col-md-6", userData1.heroSoulsSacrificedData, userData2.heroSoulsSacrificedData);
        addGraph("Total Ancient Souls", "col-md-6", userData1.totalAncientSoulsData, userData2.totalAncientSoulsData);
        addGraph("Transcendent Power", "col-md-6", userData1.transcendentPowerData, userData2.transcendentPowerData, 2);
        addGraph("Rubies", "col-md-6", userData1.rubiesData, userData2.rubiesData);
        addGraph("Highest Zone This Transcension", "col-md-6", userData1.highestZoneThisTranscensionData, userData2.highestZoneThisTranscensionData);
        addGraph("Highest Zone Lifetime", "col-md-6", userData1.highestZoneLifetimeData, userData2.highestZoneLifetimeData);
        addGraph("Ascensions This Transcension", "col-md-6", userData1.ascensionsThisTranscensionData, userData2.ascensionsThisTranscensionData);
        addGraph("Ascensions Lifetime", "col-md-6", userData1.ascensionsLifetimeData, userData2.ascensionsLifetimeData);

        for (let outsider in userData1.outsiderLevelData) {
            addGraph(outsider.toTitleCase(), "col-md-4", userData1.outsiderLevelData[outsider], userData2.outsiderLevelData[outsider]);
        }

        for (let ancient in userData1.ancientLevelData) {
            addGraph(ancient.toTitleCase(), "col-md-4", userData1.ancientLevelData[ancient], userData2.ancientLevelData[ancient]);
        }
    }
}
