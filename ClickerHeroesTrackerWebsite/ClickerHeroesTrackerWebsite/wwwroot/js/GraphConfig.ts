/* tslint:disable:interface-name:We don't own this interface name, just extending it */
interface HighchartsStatic
/* tslint:enable:interface-name */
{
    wrap: (obj: HighchartsStatic, funcName: string, callback: (orig: () => string) => string) => void;
}

namespace GraphConfig
{
    "use strict";

    Highcharts.setOptions({
        global: {
            useUTC: false,
        },
    });

    Highcharts.wrap(Highcharts, "numberFormat", function (orig: () => string): string
    {
        const value = arguments[1] as number;
        return userSettings.useScientificNotation && Math.abs(value) > userSettings.scientificNotationThreshold
            ? value.toExponential(3)
            : value.toLocaleString();
    });
}
