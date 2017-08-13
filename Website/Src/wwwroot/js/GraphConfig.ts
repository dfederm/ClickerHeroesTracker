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
}
