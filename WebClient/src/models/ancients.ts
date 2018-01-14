import { Decimal } from "decimal.js";
import { Ancient } from "./ancient";

export class Ancients {
    constructor(
        private readonly ancients: { [id: number]: Ancient },
    ) { }

    public get idleGoldPercent(): Decimal {
        // Libertas
        return this.getAncientBonus(4);
    }

    public get idleDpsPercent(): Decimal {
        // Siyalatas
        return this.getAncientBonus(5);
    }

    public get goldPercent(): Decimal {
        // Mammon
        return this.getAncientBonus(8);
    }

    public get treasureChestGoldPercent(): Decimal {
        // Mimzee
        return this.getAncientBonus(9);
    }

    public get goldenClicksPercent(): Decimal {
        // Pluto
        return this.getAncientBonus(10);
    }

    public get heroLevelCostPercent(): Decimal {
        // Dogcog
        return this.getAncientBonus(11);
    }

    public get tenXGoldChance(): Decimal {
        // Fortuna
        return this.getAncientBonus(12);
    }

    public get primalBossSpawnPercent(): Decimal {
        // Atman
        return this.getAncientBonus(13);
    }

    public get treasureChestSpawnPercent(): Decimal {
        // Dora
        return this.getAncientBonus(14);
    }

    public get criticalClickMultiplierPercent(): Decimal {
        // Bhaal
        return this.getAncientBonus(15);
    }

    public get dpsPercent(): Decimal {
        // Morg
        return this.getAncientBonus(16);
    }

    public get bossTimerSeconds(): Decimal {
        // Chronos
        return this.getAncientBonus(17);
    }

    public get bossLifePercent(): Decimal {
        // Bubos
        return this.getAncientBonus(18);
    }

    public get clickDamagePercent(): Decimal {
        // Fragsworth
        return this.getAncientBonus(19);
    }

    public get skillCooldownPercent(): Decimal {
        // Vaagur
        return this.getAncientBonus(20);
    }

    public get monsterLevelRequirement(): Decimal {
        // Kuma
        return this.getAncientBonus(21);
    }

    public get clickstormSeconds(): Decimal {
        // Chawedo
        return this.getAncientBonus(22);
    }

    public get superClicksSeconds(): Decimal {
        // Hecatoncheir
        return this.getAncientBonus(23);
    }

    public get powersurgeSeconds(): Decimal {
        // Berserker
        return this.getAncientBonus(24);
    }

    public get luckyStrikesSeconds(): Decimal {
        // Sniperino
        return this.getAncientBonus(25);
    }

    public get goldenClicksSeconds(): Decimal {
        // Kleptos
        return this.getAncientBonus(26);
    }

    public get metalDetectorSeconds(): Decimal {
        // Energon
        return this.getAncientBonus(27);
    }

    public get gildedDamageBonusPercent(): Decimal {
        // Argaiv
        return this.getAncientBonus(28);
    }

    public get comboClickPercent(): Decimal {
        // Juggernaut
        return this.getAncientBonus(29);
    }

    public get doubleRubyPercent(): Decimal {
        // Revolc
        return this.getAncientBonus(31);
    }

    public get idleUnassignedAutoclickerBonusPercent(): Decimal {
        // Nogardnit
        return this.getAncientBonus(32);
    }

    private getAncientBonus(id: number): Decimal {
        return this.ancients[id].getBonusAmount();
    }
}
