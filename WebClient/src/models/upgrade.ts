import { Decimal } from "decimal.js";
import { IUpgradeData, IHeroData } from "./gameData";

export class Upgrade {
    public static ASCENSION_ID = 106;

    public static ASCENSION2_ID = 132;

    private static readonly attributeMultipliers: { [attribute: number]: number } = {
        1: 10,
        2: 25,
        3: 100,
        4: 800,
        5: 8000,
        6: 40000,
        7: 400000,
    };

    constructor(
        private readonly definition: IUpgradeData,
        private readonly heroes: { [id: string]: IHeroData },
    ) { }

    public get id(): number {
        return this.definition.id;
    }

    public get heroId(): number {
        return this.definition.heroId;
    }

    public get upgradeFunction(): string {
        return this.definition.upgradeFunction;
    }

    public get upgradeParams(): string {
        return this.definition.upgradeParams;
    }

    public get heroLevelRequired(): number {
        return this.definition.heroLevelRequired;
    }

    public get cost(): Decimal {
        let hero = this.heroes[this.definition.heroId];
        let multiplier = Upgrade.attributeMultipliers[this.definition.attribute];
        return new Decimal(hero.baseCost).times(multiplier);
    }
}
