import { Decimal } from "decimal.js";
import { UserData } from "./userData";
import { Ancients } from "./ancients";

export class Monster {
    private static readonly BOSS_ZONE_MODULUS = 5;

    private static readonly earlyBase = 1.55;

    private static readonly lateBase = 1.145;

    private static readonly levelWhereScaleChanges = 140;

    private static readonly increasePerFiveHundredLevels = 0.001;

    private static readonly healthAt200k = new Decimal("1.240e25409");

    private static readonly cachedMonsterLife: { [level: number]: Decimal } = Monster.createCachedMonsterLife();

    constructor(
        private readonly userData: UserData,
        private readonly ancients: Ancients,
        private readonly level: number,
    ) { }

    public get maxLife(): Decimal {
        return this.monsterLifeFormula1();
    }

    public get goldReward(): Decimal {
        return this.monsterGoldFormula1();
    }

    public get isBoss(): boolean {
        return this.level % Monster.BOSS_ZONE_MODULUS === 0;
    }

    private static createCachedMonsterLife(): { [level: number]: Decimal } {
        let cachedMonsterLife: { [level: number]: Decimal } = {};
        cachedMonsterLife[Monster.levelWhereScaleChanges] = new Decimal(Monster.earlyBase).pow(Monster.levelWhereScaleChanges - 1).times(10);
        cachedMonsterLife[500] = new Decimal(Monster.lateBase).pow(500 - Monster.levelWhereScaleChanges).times(cachedMonsterLife[Monster.levelWhereScaleChanges]);
        return cachedMonsterLife;
    }

    private monsterLifeFormula1(): Decimal {
        let loc3: Decimal = null;
        let loc4: number;
        let loc6: number;
        let loc7: number;
        let loc8: number;
        let loc9: number;
        let loc10: Decimal = null;

        if (!this.userData.transcendent && this.userData.highestFinishedZonePersist > 500) {
            return this.oldMonsterLifeFormula1();
        }

        if (this.level <= Monster.levelWhereScaleChanges) {
            loc3 = new Decimal(Monster.earlyBase).pow(this.level - 1);
            loc4 = this.level;
        } else if (this.level <= 500) {
            loc3 = new Decimal(Monster.lateBase).pow(this.level - Monster.levelWhereScaleChanges).times(Monster.cachedMonsterLife[Monster.levelWhereScaleChanges]);
            loc4 = Monster.levelWhereScaleChanges;
        } else if (this.level > 200000) {
            loc3 = Monster.healthAt200k.times(new Decimal(1.545).pow(this.level - 200001));

            // This isn't set in game code but it seems to handle it somehow. The effect is negligable anyway at this level, so this guess doesn't really matter
            loc4 = Monster.levelWhereScaleChanges;
        } else {
            loc6 = Math.floor(this.level / 500);
            loc7 = loc6 * 500;
            loc8 = this.level - loc7;
            loc9 = Monster.lateBase + loc6 * Monster.increasePerFiveHundredLevels;
            loc10 = this.monsterLifeByFiveHundreds(loc7);
            loc3 = new Decimal(loc9).pow(loc8).times(loc10);
            loc4 = Monster.levelWhereScaleChanges;
        }

        loc3 = loc3.plus((loc4 - 1) * 10);
        if (this.isBoss) {
            loc3 = loc3.times(this.userData.getBossHpMultiplier(this.level));
        }

        loc3 = loc3.ceil();
        return loc3;
    }

    private monsterGoldFormula1(): Decimal {
        let loc4: Decimal = new Decimal(1);
        let loc5 = 1;
        if (this.level > 75) {
            loc5 = Math.min(3, Math.pow(1.025, this.level - 75));
        }

        /*
        // Is treasure chest
        if (this.id === 86) {
            loc4 = this.userData.getTreasureChestMultiplier();
        }
        */

        return this.oldMonsterLifeFormula1()
            .dividedBy(15)
            .times(this.userData.goldMultiplier)
            // Metal Detector: .times(param2 ? 1 : this.userData.getSkillBonus("skillGoldBonus") + 1);
            .times(loc4)
            .times(this.ancients.goldPercent.times(0.01).plus(1))
            .times(loc5)
            .ceil();
    }

    private oldMonsterLifeFormula1(): Decimal {
        let loc5: number;
        let loc6: number;
        let loc3 = 1.6;
        let loc4 = 1.15;
        let loc7 = 140;

        if (this.level < loc7) {
            loc5 = this.level;
            loc6 = 0;
        } else {
            loc5 = loc7;
            loc6 = this.level - loc7;
        }

        let loc9 = new Decimal(loc3);
        loc9 = loc9.pow(loc5 - 1);

        let loc2 = new Decimal(1);
        loc2 = loc2.times(loc9);
        loc2 = loc2.plus((loc5 - 1) * 10);

        if (this.isBoss) {
            loc2 = loc2.times(this.userData.getBossHpMultiplier(this.level));
        }

        if (loc6) {
            let loc10 = new Decimal(loc4);
            loc10 = loc10.pow(loc6);
            loc2 = loc2.times(loc10);
        }

        loc2 = loc2.ceil();
        return loc2;
    }

    private monsterLifeByFiveHundreds(param1: number): Decimal {
        if (Monster.cachedMonsterLife[param1] != null) {
            return Monster.cachedMonsterLife[param1];
        } else {
            let loc3 = Math.floor(param1 / 500);
            let loc4 = Monster.lateBase + (loc3 - 1) * Monster.increasePerFiveHundredLevels;
            let loc2 = new Decimal(loc4).pow(500).times(this.monsterLifeByFiveHundreds(param1 - 500));
            Monster.cachedMonsterLife[param1] = loc2;
            return loc2;
        }
    }
}
