import { Pipe, PipeTransform } from "@angular/core";
// tslint:disable-next-line:import-name - The module is named "decimal.js"
import Decimal from "decimal.js";

// Some shenanigans to wire up toFormat, which doesn't have typings.
// tslint:disable:no-require-imports
// tslint:disable:no-var-requires
require("toFormat")(Decimal);
// tslint:enable:no-require-imports
// tslint:enable:no-var-requires

@Pipe({ name: "exponential" })
export class ExponentialPipe implements PipeTransform {
    // TODO get the user's real settings
    private userSettings =
    {
        areUploadsPublic: true,
        hybridRatio: 1,
        logarithmicGraphScaleThreshold: 1000000,
        playStyle: "hybrid",
        scientificNotationThreshold: 100000,
        useEffectiveLevelForSuggestions: false,
        useLogarithmicGraphScale: true,
        useScientificNotation: true,
    };

    public transform(value: number | decimal.Decimal): string {
        if (!value) {
            value = 0;
        }

        if (typeof value === "number") {
            const useScientificNotation = this.userSettings.useScientificNotation && Math.abs(value) > this.userSettings.scientificNotationThreshold;
            return useScientificNotation
                ? value.toExponential(3)
                : value.toLocaleString();
        }

        if (value instanceof Decimal) {
            const useScientificNotation = this.userSettings.useScientificNotation && value.abs().greaterThan(this.userSettings.scientificNotationThreshold);
            return useScientificNotation
                ? value.toExponential(3)
                : value.toFormat();
        }

        throw new Error("Unexpected value passed to ExponentialPipe");
    }
}
