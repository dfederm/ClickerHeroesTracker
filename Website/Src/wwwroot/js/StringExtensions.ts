// tslint:disable-next-line:interface-name - We don't own this interface name, just extending it
interface String
{
    toTitleCase(): string;
}

namespace StringExtensions
{
    "use strict";

    // tslint:disable:no-invalid-this - Prototype functions have a valid case to use "this"
    String.prototype.toTitleCase = function (): string
    {
        if (this.length === 0)
        {
            return this;
        }

        return this[0].toUpperCase() + this.substring(1);
    };
    // tslint:enable:no-invalid-this
}
