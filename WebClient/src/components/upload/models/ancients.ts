import { Ancient } from "./ancient";

export class Ancients {
    constructor(
        private ancients: { [id: number]: Ancient },
    ) { }

    public get idleGoldPercent(): decimal.Decimal {
        // Libertas
        return this.getAncientBonus(4);
    }

    public get idleDpsPercent(): decimal.Decimal {
        // Siyalatas
        return this.getAncientBonus(5);
    }

    public get goldPercent(): decimal.Decimal {
        // Mammon
        return this.getAncientBonus(8);
    }

    public get treasureChestGoldPercent(): decimal.Decimal {
        // Mimzee
        return this.getAncientBonus(9);
    }

    public get goldenClicksPercent(): decimal.Decimal {
        // Pluto
        return this.getAncientBonus(10);
    }

    public get heroLevelCostPercent(): decimal.Decimal {
        // Dogcog
        return this.getAncientBonus(11);
    }

    public get tenXGoldChance(): decimal.Decimal {
        // Fortuna
        return this.getAncientBonus(12);
    }

    public get primalBossSpawnPercent(): decimal.Decimal {
        // Atman
        return this.getAncientBonus(13);
    }

    public get treasureChestSpawnPercent(): decimal.Decimal {
        // Dora
        return this.getAncientBonus(14);
    }

    public get criticalClickMultiplierPercent(): decimal.Decimal {
        // Bhaal
        return this.getAncientBonus(15);
    }

    public get dpsPercent(): decimal.Decimal {
        // Morg
        return this.getAncientBonus(16);
    }

    public get bossTimerSeconds(): decimal.Decimal {
        // Chronos
        return this.getAncientBonus(17);
    }

    public get bossLifePercent(): decimal.Decimal {
        // Bubos
        return this.getAncientBonus(18);
    }

    public get clickDamagePercent(): decimal.Decimal {
        // Fragsworth
        return this.getAncientBonus(19);
    }

    public get skillCooldownPercent(): decimal.Decimal {
        // Vaagur
        return this.getAncientBonus(20);
    }

    public get monsterLevelRequirement(): decimal.Decimal {
        // Kuma
        return this.getAncientBonus(21);
    }

    public get clickstormSeconds(): decimal.Decimal {
        // Chawedo
        return this.getAncientBonus(22);
    }

    public get superClicksSeconds(): decimal.Decimal {
        // Hecatoncheir
        return this.getAncientBonus(23);
    }

    public get powersurgeSeconds(): decimal.Decimal {
        // Berserker
        return this.getAncientBonus(24);
    }

    public get luckyStrikesSeconds(): decimal.Decimal {
        // Sniperino
        return this.getAncientBonus(25);
    }

    public get goldenClicksSeconds(): decimal.Decimal {
        // Kleptos
        return this.getAncientBonus(26);
    }

    public get metalDetectorSeconds(): decimal.Decimal {
        // Energon
        return this.getAncientBonus(27);
    }

    public get gildedDamageBonusPercent(): decimal.Decimal {
        // Argaiv
        return this.getAncientBonus(28);
    }

    public get comboClickPercent(): decimal.Decimal {
        // Juggernaut
        return this.getAncientBonus(29);
    }

    public get doubleRubyPercent(): decimal.Decimal {
        // Revolc
        return this.getAncientBonus(31);
    }

    public get idleUnassignedAutoclickerBonusPercent(): decimal.Decimal {
        // Nogardnit
        return this.getAncientBonus(32);
    }

    private getAncientBonus(id: number): decimal.Decimal {
        return this.ancients[id].getBonusAmount();
    }
}
