declare var graphs: { [id: string]: Highcharts.Options };

namespace GraphConfig
{
    "use strict";

    Highcharts.setOptions({
        global: {
            useUTC: false,
        },
    });

    Highcharts.wrap(Highcharts, "numberFormat", function (): string
    {
        const value = arguments[1] as number;

        // Special-case 0.1 which is the special value we use to plot a 0 value on a log scale.
        if (userSettings.useScientificNotation && value === 0.1)
        {
            return "0";
        }

        return userSettings.useScientificNotation && Math.abs(value) > userSettings.scientificNotationThreshold
            ? value.toExponential(3)
            : value.toLocaleString();
    });

    $(() => {
        for (let id in graphs)
        {
            let graph = graphs[id];
            let isLogarithmic = (graph.yAxis as Highcharts.AxisOptions).type === "logarithmic";
            let hasFiniteValues = true;

            for (let i = 0; i < graph.series.length; i++)
            {
                let series = graph.series[i];
                for (let j = 0; j < series.data.length; j++)
                {
                    let point = series.data[j] as [string, string | number];
                    let value = new Decimal(point[1]);
                    let numberValue = value.toNumber();

                    hasFiniteValues = hasFiniteValues && isFinite(numberValue);

                    // If we're using a log scale, hack around the inability to plot a 0 value by changing it to 0.1 (1e-1) or "one below" 1 (1e0).
                    if (isLogarithmic && numberValue === 0)
                    {
                        numberValue = 0.1;
                    }

                    point[1] = numberValue;
                }
            }

            let graphElement = $("#" + id);
            if (hasFiniteValues)
            {
                graphElement.highcharts(graph);
            }
            else
            {
                graphElement.css("text-align", "center");
                graphElement.html(`
                    <p style="color:#333333;font-size:18px;padding-bottom: 100px">${graph.title.text}</p>
                    <p>This graph contains values which are currently too large to graph.</p>
                    <p>This will be <a href="https://github.com/dfederm/ClickerHeroesTracker/issues/113" target="_blank">fixed</a> soon.</p>
                `);
            }
        }
    });

    // ContentManager.RegisterRawScript("$(function(){$('#" + Model.Id + "').highcharts(" + Model.Data.ToJsonString() + ");});");
}
