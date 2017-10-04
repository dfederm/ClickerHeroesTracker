import { Pipe, PipeTransform, ChangeDetectorRef } from "@angular/core";
// tslint:disable-next-line:import-name - The module is named "decimal.js"
import Decimal from "decimal.js";
import { SettingsService, IUserSettings } from "../services/settingsService/settingsService";

// Some shenanigans to wire up toFormat, which doesn't have typings.
// tslint:disable:no-require-imports
// tslint:disable:no-var-requires
require("toFormat")(Decimal);
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

    public transform(value: number | decimal.Decimal): string {
        if (!value) {
            value = 0;
        }

        if (typeof value === "number") {
            const useScientificNotation = this.settings && this.settings.useScientificNotation && Math.abs(value) > this.settings.scientificNotationThreshold;
            return useScientificNotation
                ? value.toExponential(3)
                : value.toLocaleString();
        }

        if (value instanceof Decimal) {
            const useScientificNotation = this.settings && this.settings.useScientificNotation && value.abs().greaterThan(this.settings.scientificNotationThreshold);
            return useScientificNotation
                ? value.toExponential(3)
                : value.toFormat();
        }

        throw new Error("Unexpected value passed to ExponentialPipe");
    }
}
