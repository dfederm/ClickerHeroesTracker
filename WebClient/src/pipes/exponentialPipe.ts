import { Pipe, PipeTransform, ChangeDetectorRef } from "@angular/core";
// tslint:disable-next-line:import-name - The module is named "decimal.js"
import Decimal from "decimal.js";
import { SettingsService, IUserSettings } from "../services/settingsService/settingsService";

// Some shenanigans to wire up toformat, which doesn't have typings.
// tslint:disable:no-require-imports
// tslint:disable:no-var-requires
require("toformat")(Decimal);
// tslint:enable:no-require-imports
// tslint:enable:no-var-requires

@Pipe({ name: "exponential" })
export class ExponentialPipe implements PipeTransform {
    private settings: IUserSettings;

    constructor(
        settingsService: SettingsService,
        changeDetectorRef: ChangeDetectorRef,
    ) {
        settingsService
            .settings()
            .subscribe(settings => {
                this.settings = settings;
                changeDetectorRef.markForCheck();
            });
    }

    public static formatNumber(value: number | decimal.Decimal, settings: IUserSettings): string {
        if (!value) {
            value = 0;
        }

        if (typeof value === "number") {
            // Account for floating point inaccuracy
            value = Math.round(value);

            const useScientificNotation = settings && settings.useScientificNotation && Math.abs(value) > settings.scientificNotationThreshold;
            return useScientificNotation
                ? value.toExponential(3)
                : value.toLocaleString();
        }

        if (value instanceof Decimal) {
            // Account for floating point inaccuracy
            value = value.round();

            const useScientificNotation = settings && settings.useScientificNotation && value.abs().greaterThan(settings.scientificNotationThreshold);
            return useScientificNotation
                ? value.toExponential(3)
                : value.toFormat();
        }

        throw new Error("Unexpected value passed to ExponentialPipe");
    }

    public transform(value: number | decimal.Decimal): string {
        return ExponentialPipe.formatNumber(value, this.settings);
    }
}
