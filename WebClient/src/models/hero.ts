import { Decimal } from "decimal.js";
import { IHeroData } from "./gameData";
import { Ancients } from "./ancients";
import { Outsiders } from "./outsiders";
import { UserData } from "./userData";

export class Hero {
    private static readonly baseAttackMultipliers: { [heroId: number]: Decimal } = {
        27: new Decimal("5e-4"),
        28: new Decimal("5e-6"),
        29: new Decimal("5e-8"),
        30: new Decimal("5e-10"),
        31: new Decimal("5e-12"),
        32: new Decimal("5e-14"),
        33: new Decimal("5e-16"),
        34: new Decimal("5e-18"),
        35: new Decimal("5e-20"),
        36: new Decimal("5e-22"),
        37: new Decimal("5e-24"),
        38: new Decimal("5e-26"),
        39: new Decimal("5e-28"),
        40: new Decimal("5e-30"),
        41: new Decimal("5e-69"),
        42: new Decimal("5e-148"),
        43: new Decimal("5e-315"),
        44: new Decimal("5e-660"),
        45: new Decimal("5e-1363"),
        46: new Decimal("5e-2312"),
        47: new Decimal("5e951"),
        48: new Decimal("5e951"),
        49: new Decimal("5e16237"),
        50: new Decimal("5e26699"),
    };

    public level = 0;
    public damageMultiplier = new Decimal(1);
    public epicLevel = 0;
    public upgradeDamageMultiplier = new Decimal(1);

    constructor(
        private readonly definition: IHeroData,
        private readonly userData: UserData,
        private readonly ancients: Ancients,
        private readonly outsiders: Outsiders,
    ) { }

    public get id(): number {
        return this.definition.id;
    }

    public get name(): string {
        return this.definition.name;
    }

    public recalculateDamageMultiplier(): void {
        this.damageMultiplier = this.upgradeDamageMultiplier.times(this.getMultiplierForHeroLevel());
    }

    public get currentClickDamage(): Decimal {
        if (this.definition.attackFormula !== "heroClickDamageFormula1") {
            return new Decimal(0);
        }

        return new Decimal(this.definition.baseClickDamage)
            .times(this.level)
            .times(this.damageMultiplier)
            .times(this.userData.clickMultiplier)
            .times(this.userData.allDpsMultiplier)
            .times(this.userData.getHeroSoulWorldDamageBonus().times(0.01).plus(1))
            .times(this.outsiders.ancientSoulDamageBonus.times(0.01).plus(1));
    }

    public get currentAttack(): Decimal {
        if (this.definition.attackFormula !== "heroAttackFormula1") {
            return new Decimal(0);
        }

        return this.baseAttack
            .times(this.level)
            .times(this.damageMultiplier)
            .times(this.userData.allDpsMultiplier)
            .times(this.userData.getHeroSoulWorldDamageBonus().times(0.01).plus(1))
            .times(this.outsiders.ancientSoulDamageBonus.times(0.01).plus(1))
            .times(this.getEpicBonus())
            .times(this.userData.getRubyDamageMultiple());
    }

    public addLevel(param1: number): void {
        this.level = this.level + param1;
    }

    public getCostUpToLevel(toLevel: number): Decimal {
        return this.processHeroFormula(this.definition.costFormula, this.level, toLevel);
    }

    private get baseAttack(): Decimal {
        if (this.definition.id === 1) {
            return new Decimal(0);
        }

        let baseAttack = new Decimal(this.definition.baseCost).dividedBy(10).times(Math.pow(1 - 0.0188 * Math.min(this.definition.id, 14), this.definition.id)).ceil();
        let baseAttackMultiplier = Hero.baseAttackMultipliers[this.definition.id] || new Decimal(1);
        return baseAttack.times(baseAttackMultiplier);
    }

    private getMultiplierForHeroLevel(): Decimal {
        let loc7 = NaN;
        let loc3: Decimal = new Decimal(1);
        let loc4 = 7;
        let loc5: number = Math.min(Math.floor(this.level / 1000), 8);
        let loc6: number = Math.floor(this.level / 25) - loc5 - loc4;
        if (this.definition.id >= 27 && this.definition.id <= 45) {
            if (this.level >= 525) {
                loc7 = Math.min(Math.floor((this.level - 500) / 25), 9);
                loc3 = loc3.times(new Decimal(5).pow(loc7));
                loc6 = loc6 - loc7;
            }
        }
        if (this.level >= 200) {
            loc3 = loc3.times(new Decimal(this.getMultiplierFor1000Levels()).pow(loc5));
            loc3 = loc3.times(new Decimal(this.getMultiplierFor25Levels()).pow(loc6));
        }
        return loc3;
    }

    private getMultiplierFor25Levels(): number {
        if (this.definition.id === 1) {
            return 1;
        }

        if (this.definition.id > 1 && this.definition.id < 46) {
            return 4;
        }

        return 4.5;
    }

    private getMultiplierFor1000Levels(): number {
        if (this.definition.id > 1 && this.definition.id < 46) {
            return 10;
        }

        return this.getMultiplierFor25Levels();
    }

    private getEpicBonus(): Decimal {
        return this.ancients.gildedDamageBonusPercent.times(0.01).plus(0.5).times(this.epicLevel).plus(1);
    }

    private processHeroFormula(param1: string, fromLevel: number = 0, toLevel: number = 1): Decimal {
        if (fromLevel === 0) {
            fromLevel = this.level;
        }

        if (param1 === "heroCostFormula1") {
            return this.heroCostFormula1(fromLevel, toLevel);
        }

        if (param1 === "heroCostFormula46") {
            return this.heroCostFormula46(fromLevel, toLevel);
        }

        return new Decimal(0);
    }

    private heroCostFormula1(fromLevel: number, toLevel: number): Decimal {
        let loc5: Decimal = null;
        let loc7: Decimal = null;
        let loc9 = 0;
        let loc10: Decimal;
        let loc11: number;
        let loc12: Decimal;
        let loc13: Decimal;
        let loc4 = 1.07;
        let loc6: number = 1 - this.ancients.heroLevelCostPercent.times(0.01).toNumber();
        if (this.definition.id === 1) {
            if (fromLevel <= 15) {
                loc5 = new Decimal(0);
                if (toLevel <= 15) {
                    loc9 = fromLevel;
                    while (loc9 < toLevel) {
                        loc5 = loc5.plus(new Decimal((5 + loc9) * Math.pow(loc4, loc9) * loc6));
                        loc9++;
                    }
                    return loc5.floor();
                }
                loc9 = fromLevel;
                while (loc9 < 15) {
                    loc5 = loc5.plus(new Decimal((5 + loc9) * Math.pow(loc4, loc9) * loc6));
                    loc9++;
                }
                loc5 = loc5.plus(new Decimal(20 * ((Math.pow(loc4, toLevel) - Math.pow(loc4, 15)) / (loc4 - 1)) * loc6));
                return loc5.floor();
            }
            loc5 = new Decimal(loc6 * 20);
            loc7 = new Decimal(loc4);
            loc7 = loc7.pow(toLevel);
            loc10 = new Decimal(loc4);
            loc11 = fromLevel;
            loc10 = loc10.pow(loc11);
            loc12 = loc7.minus(loc10);
            loc13 = new Decimal(loc4 - 1);
            loc12 = loc12.dividedBy(loc13);
            loc5 = loc5.times(loc12);
            return loc5.floor();
        }
        loc7 = new Decimal(this.definition.baseCost);
        let loc8 = new Decimal(loc4).pow(toLevel);
        loc8 = loc8.minus(new Decimal(loc4).pow(fromLevel));
        loc8 = loc8.dividedBy(loc4 - 1);
        loc7 = loc7.times(loc8);
        loc7 = loc7.times(loc6);
        loc7 = loc7.floor();
        return loc7;
    }

    private heroCostFormula46(fromLevel: number, toLevel: number): Decimal {
        let loc4 = 1.07;
        let loc6 = 1 - this.ancients.heroLevelCostPercent.times(0.01).toNumber();
        let loc7 = new Decimal(this.definition.baseCost);
        let loc8 = new Decimal(loc4).pow(toLevel);
        loc8 = loc8.minus(new Decimal(loc4).pow(fromLevel));
        loc8 = loc8.dividedBy(loc4 - 1);
        loc7 = loc7.times(loc8);
        loc7 = loc7.times(loc6);
        loc7 = loc7.floor();
        return loc7;
    }
}
