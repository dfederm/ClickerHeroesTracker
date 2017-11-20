import { Outsider } from "./outsider";

export class Outsiders {
    constructor(
        private outsiders: { [id: number]: Outsider },
    ) { }

    public get idleBonus(): decimal.Decimal {
        // Xyl
        return this.getOutsiderBonus(1);
    }

    public get solomonPercent(): decimal.Decimal {
        // Ponyboy
        return this.getOutsiderBonus(5);
    }

    public get ancientSoulDamageBonus(): decimal.Decimal {
        // Phan
        return this.getOutsiderBonus(3);
    }

    public get ancientCostModifier(): decimal.Decimal {
        // Chor
        return new Decimal(1).minus(this.getOutsiderBonus(2).times(0.01));
    }

    public get kumaBonus(): decimal.Decimal {
        // Borb
        return this.getOutsiderBonus(6);
    }

    public get atmanBonus(): decimal.Decimal {
        // Rhageist
        return this.getOutsiderBonus(7);
    }

    public get bubosBonus(): decimal.Decimal {
        // K'Ariqua
        return this.getOutsiderBonus(8);
    }

    public get chronosBonus(): decimal.Decimal {
        // Orphalas
        return this.getOutsiderBonus(9);
    }

    public get doraBonus(): decimal.Decimal {
        // Sen-Akhan
        return this.getOutsiderBonus(10);
    }

    private getOutsiderBonus(id: number): decimal.Decimal {
        return this.outsiders[id].getBonusAmount();
    }
}
