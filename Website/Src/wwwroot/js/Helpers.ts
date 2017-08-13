namespace Helpers
{
    "use strict";

    export function getQueryParameters(): IMap<string>
    {
        const ret: IMap<string> = {};

        let rawParameters = "";

        // Process query string
        const queryString = location.search;
        if (queryString && queryString.length > 0)
        {
            // Remove the '?'
            rawParameters += queryString.substring(1);
        }

        // Process hash
        const hash = location.hash;
        if (hash && hash.length > 0)
        {
            // Remove the '#'
            rawParameters += "&" + hash.substring(1);
        }

        rawParameters
            .split("&")
            .forEach((entry: string): void =>
            {
                const parts = entry.split("=");
                const key = decodeURIComponent(parts[0]);
                const value = decodeURIComponent(parts[1]);

                if (key in ret)
                {
                    ret[key] += "," + value;
                }
                else
                {
                    ret[key] = value;
                }
            });

        return ret;
    }

    export function getElementsByDataType(dataType: string): HTMLElement[]
    {
        const nodes = document.querySelectorAll("[data-type='" + dataType + "']");
        let elements: HTMLElement[] = [];
        for (let i = 0; i < nodes.length; i++)
        {
            elements.push(nodes[i] as HTMLElement);
        }

        return elements;
    }

    export function copyToClipboard(value: string): void
    {
        appInsights.trackEvent("copyToClipboard", { value: value });

        // Set up temp field to copy from
        const temp = $("<input>");
        $("body").append(temp);
        temp.val(value).select();
        document.execCommand("copy");
        temp.remove();

        // Show copied alert
        showMessage("Value copied to clipboard", "success");
    }

    export function showMessage(message: string, type: string): void
    {
        type = ".alert-" + type;
        console.log(type);
        $(type).find("strong").text(message);
        $(type).fadeIn(200);
        $(type).delay(2000).slideUp(600, function (): void {
            $(type).hide();
        });
    }
}
