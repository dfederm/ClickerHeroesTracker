import Decimal from "decimal.js";
import { IAncientData } from "./gameData";
import { Outsiders } from "./outsiders";
import { isLinear, linear, isPercent, percent } from "./functions";

export class Ancient {
    private static libAndSiyaVariables = [
        { threshold: 10, minus: 0, times: 25, plus: 0 },
        { threshold: 20, minus: 9, times: 24, plus: 225 },
        { threshold: 30, minus: 19, times: 23, plus: 465 },
        { threshold: 40, minus: 29, times: 22, plus: 695 },
        { threshold: 50, minus: 39, times: 21, plus: 915 },
        { threshold: 60, minus: 49, times: 20, plus: 1125 },
        { threshold: 70, minus: 59, times: 19, plus: 1325 },
        { threshold: 80, minus: 69, times: 18, plus: 1515 },
        { threshold: 90, minus: 79, times: 17, plus: 1695 },
        { threshold: 100, minus: 89, times: 16, plus: 1865 },
        { threshold: null, minus: 99, times: 15, plus: 2025 },
    ];

    constructor(
        private definition: IAncientData,
        private outsiders: Outsiders,
        public level: decimal.Decimal,
    ) { }

    public getBonusAmount(): decimal.Decimal {
        if (isLinear(this.definition.levelAmountFormula)) {
            return linear(this.definition.levelAmountFormula, this.level);
        }

        if (isPercent(this.definition.levelAmountFormula)) {
            return percent(this.definition.levelAmountFormula, this.level);
        }

        switch (this.definition.levelAmountFormula) {
            case "libAndSiy": {
                let xylMultiplier = this.outsiders.idleBonus.times(0.01).plus(1);
                for (let i = 0; i < Ancient.libAndSiyaVariables.length; i++) {
                    let variables = Ancient.libAndSiyaVariables[i];
                    if (variables.threshold === null || this.level.lessThan(variables.threshold)) {
                        return this.level.minus(variables.minus).times(variables.times).plus(variables.plus).times(xylMultiplier);
                    }
                }

                throw new Error("Unexpected error when calculating libAndSiy bonus amount");
            }
            case "diminishingReturns": {
                return this.diminishingReturns();
            }
            case "atmanValue": {
                return this.diminishingReturns().times(this.outsiders.atmanBonus.times(0.01).plus(1));
            }
            case "bubosValue": {
                return this.diminishingReturns().times(this.outsiders.bubosBonus.times(0.01).plus(1));
            }
            case "chronosValue": {
                return this.diminishingReturns().times(this.outsiders.chronosBonus.times(0.01).plus(1));
            }
            case "doraValue": {
                return this.diminishingReturns().times(this.outsiders.doraBonus.times(0.01).plus(1));
            }
            case "kumaValue": {
                return this.diminishingReturns().times(this.outsiders.kumaBonus.times(0.01).plus(1));
            }
            case "nogardnitValue": {
                return this.level.times(10).times(this.outsiders.idleBonus.times(0.01).plus(1));
            }
        }

        throw new Error("Unexpected ancient levelAmountFormula: " + this.definition.levelAmountFormula);
    }

    private diminishingReturns(): decimal.Decimal {
        let params = this.definition.levelAmountParams.split(",");
        let param1 = Number(params[0]);
        let param2 = Number(params[1]);
        return new Decimal(param1 * (1 - Math.exp(param2 * this.level.toNumber())));
    }
}
