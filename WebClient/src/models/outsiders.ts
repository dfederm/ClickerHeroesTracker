import { Decimal } from "decimal.js";
import { Outsider } from "./outsider";

export class Outsiders {
    constructor(
        private readonly outsiders: { [id: number]: Outsider },
    ) { }

    public get idleBonus(): Decimal {
        // Xyl
        return this.getOutsiderBonus(1);
    }

    public get solomonPercent(): Decimal {
        // Ponyboy
        return this.getOutsiderBonus(5);
    }

    public get ancientSoulDamageBonus(): Decimal {
        // Phan
        return this.getOutsiderBonus(3);
    }

    public get ancientCostModifier(): Decimal {
        // Chor
        return new Decimal(1).minus(this.getOutsiderBonus(2).times(0.01));
    }

    public get kumaBonus(): Decimal {
        // Borb
        return this.getOutsiderBonus(6);
    }

    public get atmanBonus(): Decimal {
        // Rhageist
        return this.getOutsiderBonus(7);
    }

    public get bubosBonus(): Decimal {
        // K'Ariqua
        return this.getOutsiderBonus(8);
    }

    public get chronosBonus(): Decimal {
        // Orphalas
        return this.getOutsiderBonus(9);
    }

    public get doraBonus(): Decimal {
        // Sen-Akhan
        return this.getOutsiderBonus(10);
    }

    private getOutsiderBonus(id: number): Decimal {
        return this.outsiders[id].getBonusAmount();
    }
}
