import Decimal from "decimal.js";
import { HeroCollection } from "./heroCollection";

export class Attributes {
    private _currentAttack = new Decimal(0);

    private _currentClickDamage = new Decimal(0);

    constructor(
        private heroCollection: HeroCollection,
    ) { }

    public get currentAttack(): decimal.Decimal {
        /*
        Powersurge
        let loc1 = this.userData.getSkillBonus("skillDpsMultiplier") + 1;
        return this._currentAttack.times(loc1);
        */
        return this._currentAttack;
    }

    public get currentAttackUnmodified(): decimal.Decimal {
        return this._currentAttack;
    }

    public get currentClickDamage(): decimal.Decimal {
        return this._currentClickDamage.plus(1);
    }

    public set currentClickDamage(param1: decimal.Decimal) {
        this._currentClickDamage = param1;
    }

    public recalculate(): void {
        let attack = new Decimal(0);
        let clickDamage = new Decimal(0);

        for (let heroId in this.heroCollection.heroes) {
            let hero = this.heroCollection.heroes[heroId];

            attack = attack.plus(hero.currentAttack);
            clickDamage = clickDamage.plus(hero.currentClickDamage);
        }

        this._currentAttack = attack;
        this._currentClickDamage = clickDamage;
    }
}
