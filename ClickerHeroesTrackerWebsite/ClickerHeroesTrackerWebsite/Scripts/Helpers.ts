interface IMap<TValue>
{
    [key: string]: TValue;
}

function getQueryParameters(): IMap<string>
{
    var ret: IMap<string> = {};

    var rawParameters = '';

    // Process query string
    var queryString = location.search;
    if (queryString && queryString.length > 0)
    {
        // Remove the '?'
        rawParameters += queryString.substring(1);
    }

    // Process hash
    var hash = location.hash;
    if (hash && hash.length > 0)
    {
        // Remove the '#'
        rawParameters += '&' + hash.substring(1);
    }

    rawParameters
        .split('&')
        .forEach((entry: string) =>
        {
            var parts = entry.split("=");
            var key = decodeURIComponent(parts[0]);
            var value = decodeURIComponent(parts[1]);

            if (key in ret)
            {
                ret[key] += ',' + value;
            }
            else
            {
                ret[key] = value;
            }
        });

    return ret;
}