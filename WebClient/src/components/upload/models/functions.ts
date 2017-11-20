const linearStr = "linear";
const percentStr = "percent";

export function isLinear(levelAmountFormula: string): boolean {
    return levelAmountFormula.startsWith(linearStr);
}

export function linear(levelAmountFormula: string, level: decimal.Decimal): decimal.Decimal {
    let linearSuffix = levelAmountFormula.substring(linearStr.length);
    let linearCoefficient: number;
    if (linearSuffix[0] === "0") {
        if (linearSuffix[1] === "_") {
            // Eg. linear0_25 => 0.25
            linearCoefficient = Number(linearSuffix.replace("_", "."));
        } else {
            // Eg. linear01 => 0.21
            linearCoefficient = Number("0." + linearSuffix);
        }
    } else {
        // Eg. linear5 => 5
        linearCoefficient = Number(linearSuffix);
    }
    return level.times(linearCoefficient);
}

export function isPercent(levelAmountFormula: string): boolean {
    return levelAmountFormula.startsWith(percentStr);
}

export function percent(levelAmountFormula: string, level: decimal.Decimal): decimal.Decimal {
    let percentSuffix = levelAmountFormula.substring(percent.length);

    // Eg. percent5 => 0.95 (95%)
    let percentValue = 1 - (Number(percentSuffix) / 100);

    // Basically percent5 is "take 5% off each time" or "95% of the last value"
    return new Decimal(1).minus(new Decimal(percentValue).pow(level)).times(100);
}
