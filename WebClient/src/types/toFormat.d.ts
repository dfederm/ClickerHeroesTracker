declare module 'toformat' {
    import Decimal from "decimal.js";
    function toFormat(ctor: Decimal.Constructor): void;
    export default toFormat;

    module "decimal.js" {
        interface Decimal {
            toFormat(decimalPlaces: number, rounding: Decimal.Rounding): string;
        }
    }
}
