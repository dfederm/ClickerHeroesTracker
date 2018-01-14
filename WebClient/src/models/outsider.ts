import { Decimal } from "decimal.js";
import { IOutsiderData } from "./gameData";
import { isLinear, linear, isPercent, percent } from "./functions";

export class Outsider {
    constructor(
        private readonly definition: IOutsiderData,
        public level: Decimal,
    ) { }

    public getBonusAmount(): Decimal {
        if (isLinear(this.definition.levelAmountFormula)) {
            return linear(this.definition.levelAmountFormula, this.level);
        }

        if (isPercent(this.definition.levelAmountFormula)) {
            return percent(this.definition.levelAmountFormula, this.level);
        }

        switch (this.definition.levelAmountFormula) {
            case "ponyboyValue": {
                return new Decimal(this.level).pow(2).times(1000);
            }
            case "exponential": {
                return new Decimal(1.5).pow(this.level).plus(-1).times(100);
            }
        }

        throw new Error("Unexpected outsider levelAmountFormula: " + this.definition.levelAmountFormula);
    }
}
