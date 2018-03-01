import { Decimal } from "decimal.js";

const linearStr = "linear";
const percentStr = "percent";

export function isLinear(levelAmountFormula: string): boolean {
    return levelAmountFormula.startsWith(linearStr);
}

export function linear(levelAmountFormula: string, level: Decimal): Decimal {
    /*
    Examples:
        linear0_25 => 0.25
        linear01 => 0.01
        linear5 => 5
    */
    let linearSuffix = levelAmountFormula.substring(linearStr.length);

    // Match 01 => 0.01 but not 0_25 => 0.25
    if (linearSuffix[0] === "0" && linearSuffix[1] !== "_") {
        linearSuffix = "0." + linearSuffix;
    }

    linearSuffix = linearSuffix.replace("_", ".");

    return level.times(linearSuffix);
}

export function isPercent(levelAmountFormula: string): boolean {
    return levelAmountFormula.startsWith(percentStr);
}

export function percent(levelAmountFormula: string, level: Decimal): Decimal {
    let percentSuffix = levelAmountFormula.substring(percent.length);

    // Eg. percent5 => 0.95 (95%)
    let percentValue = 1 - (Number(percentSuffix) / 100);

    // Basically percent5 is "take 5% off each time" or "95% of the last value"
    return new Decimal(1).minus(new Decimal(percentValue).pow(level)).times(100);
}
