import { Decimal } from "decimal.js";
import { HeroCollection } from "./heroCollection";
import { Ancients } from "./ancients";
import { Hero } from "./hero";
import { Upgrade } from "./upgrade";
import { Monster } from "./monster";
import { Attributes } from "./attributes";

export class UserData {
    private static readonly BOSS_HP_MULTIPLIER = 10;

    private static readonly MINIMUM_BOSS_HP_MULTIPLIER = 5;

    private static readonly BOSS_HEALTH_INCREASE_PER_ZONE_INTERVAL = 0.04;

    private static readonly BOSS_HEALTH_INCREASE_ZONE_INTERVAL = 500;

    private static readonly TREASURE_CHANCE_DECREASE_ZONE_INTERVAL = 500;

    private static readonly TREASURE_MULTIPLIER = 10;

    private static readonly MINIMUM_TREASURE_CHANCE_MULTIPLIER = 0.01;

    private static readonly CHANCE_OF_TREASURE_MONSTER = 0.01;

    public clickMultiplier = 1.0;

    public allDpsMultiplier = 1.0;

    public goldMultiplier = 1.0;

    public heroSouls = new Decimal(0);

    public paidForRubyMultiplier = false;

    public transcendent = false;

    public highestFinishedZonePersist = 0;

    public gold = new Decimal(0);

    public currentZoneHeight = 1;

    public totalAutoclickers = 0;

    public epicHeroReceivedUpTo = 0;

    private readonly purchasedUpgrades: { [id: number]: boolean } = {};

    constructor(
        private readonly heroCollection: HeroCollection,
        private readonly ancients: Ancients,
        private readonly attributes: Attributes,
    ) { }

    public upgradeClickPercent(params: string): void {
        let percentIncrease = Number(params);
        this.clickMultiplier = this.clickMultiplier * (1 + percentIncrease / 100);
    }

    public getHeroSoulWorldDamageBonus(): Decimal {
        return this.heroSouls.times(10).plus(this.ancients.dpsPercent);
    }

    public getRubyDamageMultiple(): number {
        return this.paidForRubyMultiplier
            ? 2
            : 1;
    }

    public getTreasureChestMultiplier(): Decimal {
        return this.ancients.treasureChestGoldPercent.times(0.01).plus(1).times(UserData.TREASURE_MULTIPLIER);
    }

    public getBossHpMultiplier(level: number): number {
        return Math.max(this.getUncappedBossHpMultiplier(level), UserData.MINIMUM_BOSS_HP_MULTIPLIER);
    }

    public addGold(param1: Decimal): Decimal {
        this.gold = this.gold.plus(param1);
        return this.gold;
    }

    public levelHero(hero: Hero, addedLevels: number): void {
        this.addGold(hero.getCostUpToLevel(hero.level + addedLevels).times(-1));
        hero.addLevel(addedLevels);
        hero.recalculateDamageMultiplier();
        this.attributes.recalculate();
    }

    public purchaseUpgrade(upgrade: Upgrade): void {
        this.addGold(upgrade.cost.negated());
        this.purchasedUpgrades[upgrade.id] = true;

        switch (upgrade.upgradeFunction) {
            case "upgradeHeroPercent": {
                this.upgradeHeroPercent(upgrade.upgradeParams);
                break;
            }
            case "upgradeEveryonePercent": {
                this.upgradeEveryonePercent(upgrade.upgradeParams);
                break;
            }
            case "upgradeGoldFoundPercent": {
                this.upgradeGoldFoundPercent(upgrade.upgradeParams);
                break;
            }
        }

        this.attributes.recalculate();
    }

    public hasUpgrade(upgradeId: number): boolean {
        return this.purchasedUpgrades[upgradeId];
    }

    public canAffordUpgrade(upgrade: Upgrade): boolean {
        return this.gold.greaterThanOrEqualTo(upgrade.cost);
    }

    public getInstakillStopZone(): number {
        let zone = this.currentZoneHeight;
        let monster = new Monster(this, this.ancients, zone);

        /*
        // TODO: Don't assume idle.
        if (this.isIdle()) {
            ...
        } else if (this.comboer.isClickComboing()) {
            dpsMultiplier = this.comboer.getDpsBonus().plus(1);
        }
        */
        let dpsMultiplier = this.ancients.idleDpsPercent.times(0.01).plus(1)
            .times(this.ancients.idleUnassignedAutoclickerBonusPercent.times(this.getUnassignedAutoClickerMultiplier(this.totalAutoclickers)).times(0.01).plus(1));

        let damagePerFrame = this.attributes.currentAttack
            .times(dpsMultiplier)
            .dividedBy(30); // 30 fps
        while (monster.maxLife.lessThanOrEqualTo(damagePerFrame)) {
            zone++;
            monster = new Monster(this, this.ancients, zone);
        }

        return zone;
    }

    public getTreasureChestChance(param1: number): number {
        let loc2 = this.ancients.treasureChestSpawnPercent.times(0.01).plus(1).toNumber();
        let loc3 = 0.0099999999 * (1 - Math.exp(-0.006 * Math.floor(param1 / UserData.TREASURE_CHANCE_DECREASE_ZONE_INTERVAL)));
        return Math.max((UserData.CHANCE_OF_TREASURE_MONSTER - loc3) * loc2, UserData.MINIMUM_TREASURE_CHANCE_MULTIPLIER);
    }

    public getGoldFromFinishingZone(level: number): Decimal {
        return new Monster(this, this.ancients, level).goldReward
            .times(this.getZoneMonsterRequirement(level))
            .times(this.getTreasureChestMultiplier().plus(-1).times(this.getTreasureChestChance(level)).plus(1))
            .times(this.ancients.idleGoldPercent.times(0.01).plus(1))
            .times(this.ancients.tenXGoldChance.times(0.09).plus(1));
    }

    public addSouls(addedSouls: Decimal): Decimal {
        this.heroSouls = this.heroSouls.plus(addedSouls);
        return this.heroSouls;
    }

    public getRandomGoldenHeroes(uncollectedEpicHeroes: number, excludedHeroId: number = 0): number[] {
        let heroGilds: number[] = [];

        /*
        // TODO: Do this right, although it at best saves a few hero souls
        let loc5: number = this.getHighestHeroSeen();
        if (loc5 < 5) {
            loc5 = 5;
        }
        */
        let highestHeroSeen = 5;

        let loc6 = 1;
        while (loc6 <= highestHeroSeen) {
            heroGilds[loc6] = 0;
            loc6++;
        }

        let loc7 = uncollectedEpicHeroes;
        if (uncollectedEpicHeroes > 1000) {
            let loc8 = 0;
            let loc9 = Math.floor(loc7 / (highestHeroSeen - 1));
            loc6 = 2;
            while (loc6 <= highestHeroSeen) {
                loc7 = loc7 - loc9;
                heroGilds[loc6] = loc9;
                loc8 = loc8 + loc9;
                loc6++;
            }
        }

        loc6 = 0;
        while (loc6 < loc7) {
            let heroId = excludedHeroId;
            while (heroId === excludedHeroId) {
                // Random number between 2 and loc5, Math.floor(Math.random() * (max - min + 1) + min);
                heroId = Math.floor((Math.random() * (highestHeroSeen - 1)) + 2);
            }

            heroGilds[heroId] = heroGilds[heroId] + 1;
            loc6++;
        }

        return heroGilds;
    }

    public openAllZoneGildedHeroes(): void {
        let uncollectedEpicHeroes = this.numberOfUncollectedEpicHeroes;
        this.collectEpicHeroGift(uncollectedEpicHeroes, false);

        /*
        // TODO restorePurchasedGilds
        let i = 0;
        while (i < uncollectedEpicHeroes) {
            this.restorePurchasedGilds();
            i++;
        }
        */

        this.attributes.recalculate();
    }

    public get numberOfUncollectedEpicHeroes(): number {
        let loc1 = this.epicHeroReceivedUpTo;
        let loc2 = 0;
        while (this.highestFinishedZonePersist >= 100 && (loc1 < 100 || loc1 + 10 <= this.highestFinishedZonePersist)) {
            if (loc1 < 100) {
                loc1 = 100;
            } else {
                loc1 = loc1 + 10;
            }
            loc2++;
        }
        return loc2;
    }

    public collectEpicHeroGift(uncollectedEpicHeroes: number = 1, shouldRecalculate: boolean = true): {}[] {
        let gildLevels = this.getRandomGoldenHeroes(uncollectedEpicHeroes);
        this.heroCollection.addGildLevels(gildLevels);

        if (this.epicHeroReceivedUpTo < 100) {
            this.epicHeroReceivedUpTo = 100;
            uncollectedEpicHeroes--;
        }

        this.epicHeroReceivedUpTo = this.epicHeroReceivedUpTo + 10 * uncollectedEpicHeroes;
        if (shouldRecalculate) {
            this.attributes.recalculate();
        }

        return gildLevels;
    }

    public moveAllGildsToHero(heroId: number): void {
        let hero = this.heroCollection.getById(heroId);
        let epicLevels = this.heroCollection.getTotalEpicLevels();
        let epicLevelsToMove = epicLevels - hero.epicLevel;
        let heroSoulCost = new Decimal(80 * epicLevelsToMove);
        if (this.heroSouls.greaterThanOrEqualTo(heroSoulCost)) {
            // Remove gils from all other heroes
            for (let id in this.heroCollection.heroes) {
                this.heroCollection.heroes[id].epicLevel = 0;
            }

            hero.epicLevel = epicLevels;
            this.addSouls(new Decimal(-heroSoulCost));
        }
    }

    private getUnassignedAutoClickerMultiplier(num: number): Decimal {
        if (num === 0) {
            return new Decimal(0);
        }

        if (num >= 1 && num <= 4) {
            return new Decimal(num);
        }

        return new Decimal(1.5).pow(num - 1);
    }

    private getZoneMonsterRequirement(zone: number): number {
        return Math.max(this.getUncappedZoneMonsterRequirement(zone), 2);
    }

    private getUncappedZoneMonsterRequirement(zone: number): number {
        let monsterLevelRequirement = this.ancients.monsterLevelRequirement.toNumber();
        let monstersRemovedThisZone = Math.floor(monsterLevelRequirement);

        // Uses random... might want a deterministic way to do this.
        let chanceToRemove = monsterLevelRequirement - monstersRemovedThisZone;
        if (Math.random() < chanceToRemove) {
            monstersRemovedThisZone++;
        }

        return 10 + 1 * Math.floor(zone / 500) - monstersRemovedThisZone;
    }

    private upgradeHeroPercent(params: string): void {
        let pieces = params.split(",");
        let heroId = Number(pieces[0]);
        let percentIncrease = new Decimal(pieces[1].trim());
        let hero = this.heroCollection.getById(heroId);
        hero.upgradeDamageMultiplier = hero.upgradeDamageMultiplier.times(percentIncrease.times(0.01).plus(1));
        hero.recalculateDamageMultiplier();
    }

    private upgradeEveryonePercent(params: string): void {
        let percentIncrease = Number(params);
        this.allDpsMultiplier = this.allDpsMultiplier * (1 + percentIncrease / 100);
    }

    private upgradeGoldFoundPercent(params: string): void {
        let percentIncrease = Number(params);
        this.goldMultiplier = this.goldMultiplier * (1 + percentIncrease / 100);
    }

    private getUncappedBossHpMultiplier(level: number): number {
        let zoneMultiplier = 1 + Math.floor(level / UserData.BOSS_HEALTH_INCREASE_ZONE_INTERVAL) * UserData.BOSS_HEALTH_INCREASE_PER_ZONE_INTERVAL;
        return zoneMultiplier * UserData.BOSS_HP_MULTIPLIER - this.ancients.bossLifePercent.toNumber();
    }
}
