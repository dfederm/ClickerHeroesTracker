interface IHighchartsWrapper extends HighchartsStatic
{
    wrap: (obj: HighchartsStatic, funcName: string, callback: (orig: () => string) => string) => void;
}

namespace GraphConfig
{
    "use strict";

    const h = Highcharts as IHighchartsWrapper;
    h.wrap(Highcharts, "numberFormat", function (orig: () => string): string
    {
        const value = arguments[1] as number;
        return userSettings.useScientificNotation && Math.abs(value) > userSettings.scientificNotationThreshold
            ? value.toExponential(3)
            : value.toLocaleString();
    });
}
