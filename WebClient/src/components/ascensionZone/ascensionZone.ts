import { Component, Input } from "@angular/core";
import { AppInsightsService } from "@markpieszak/ng-application-insights";
import { gameData } from "../../models/gameData";
import { SavedGame } from "../../models/savedGame";
import { Decimal } from "decimal.js";
import { Outsiders } from "../../models/outsiders";
import { UserData } from "../../models/userData";
import { HeroCollection } from "../../models/heroCollection";
import { Ancients } from "../../models/ancients";
import { Hero } from "../../models/hero";
import { Ancient } from "../../models/ancient";
import { Outsider } from "../../models/outsider";
import { Upgrade } from "../../models/upgrade";
import { Attributes } from "../../models/attributes";

interface ICalculateAscensionZoneData {
    zone: number;
    hero: string;
    heroLevel: number;
    heroGilds: number;
    damage: Decimal;
    gold: Decimal;
}

@Component({
    selector: "ascensionZone",
    templateUrl: "./ascensionZone.html",
})
export class AscensionZoneComponent {
    public calculateAscensionZoneSteps: ICalculateAscensionZoneData[];

    public get savedGame(): SavedGame {
        return this._savedGame;
    }

    @Input()
    public set savedGame(savedGame: SavedGame) {
        this._savedGame = savedGame;
        this.calculateAscensionZoneSteps = null;
    }

    private _savedGame: SavedGame;

    constructor(
        private readonly appInsights: AppInsightsService,
    ) {
    }

    // tslint:disable-next-line:cyclomatic-complexity
    public calculateAscensionZone(): void {
        let startTime = Date.now();

        // There are some circular references, so create the maps as empty first
        let upgradesMap: { [id: string]: Upgrade } = {};
        let heroesMap: { [id: string]: Hero } = {};
        let ancientsMap: { [id: string]: Ancient } = {};
        let outsidersMap: { [id: string]: Outsider } = {};

        // Create the collection objects
        let heroCollection = new HeroCollection(heroesMap);
        let ancients = new Ancients(ancientsMap);
        let outsiders = new Outsiders(outsidersMap);
        let attributes = new Attributes(heroCollection);

        let userData = new UserData(heroCollection, ancients, attributes);

        // Populate the maps
        for (let id in gameData.upgrades) {
            let upgrade = gameData.upgrades[id];
            if (upgrade._live === "0") {
                continue;
            }

            upgradesMap[id] = new Upgrade(upgrade, gameData.heroes);
        }

        for (let id in gameData.heroes) {
            let hero = gameData.heroes[id];
            if (hero._live === "0") {
                continue;
            }

            heroesMap[id] = new Hero(hero, userData, ancients, outsiders);
        }

        let itemLevels: { [ancientId: string]: Decimal } = {};
        if (this.savedGame.data.items && this.savedGame.data.items.items && this.savedGame.data.items.slots) {
            for (let slotId in this.savedGame.data.items.slots) {
                let itemId = this.savedGame.data.items.slots[slotId];
                let item = this.savedGame.data.items.items[itemId];
                if (item) {
                    let bonuses = [
                        { type: item.bonusType1, level: item.bonus1Level },
                        { type: item.bonusType2, level: item.bonus2Level },
                        { type: item.bonusType3, level: item.bonus3Level },
                        { type: item.bonusType4, level: item.bonus4Level },
                    ];

                    for (let i = 0; i < bonuses.length; i++) {
                        let bonus = bonuses[i];
                        let bonusType = gameData.itemBonusTypes[bonus.type];
                        if (bonusType) {
                            itemLevels[bonusType.ancientId] = (itemLevels[bonusType.ancientId] || new Decimal(0)).plus(bonus.level);
                        }
                    }
                }
            }
        }

        for (let id in gameData.ancients) {
            let effectiveLevel: Decimal;
            let ancientData = this.savedGame.data.ancients.ancients[id];
            if (ancientData) {
                let ancientLevel = new Decimal(ancientData.level || 0);
                let itemLevel = itemLevels[id] || new Decimal(0);
                effectiveLevel = ancientLevel.plus(itemLevel).floor();
            } else {
                effectiveLevel = new Decimal(0);
            }

            ancientsMap[id] = new Ancient(gameData.ancients[id], outsiders, effectiveLevel);
        }

        for (let id in gameData.outsiders) {
            let outsiderLevel: Decimal;
            let outsiderData = this.savedGame.data.outsiders.outsiders[id];
            if (outsiderData) {
                outsiderLevel = new Decimal(outsiderData.level || 0);
            } else {
                outsiderLevel = new Decimal(0);
            }

            outsidersMap[id] = new Outsider(gameData.outsiders[id], outsiderLevel);
        }

        // Set current state.
        userData.heroSouls = new Decimal(this.savedGame.data.heroSouls || 0);
        userData.highestFinishedZonePersist = Number(this.savedGame.data.highestFinishedZonePersist);
        userData.totalAutoclickers = Number(this.savedGame.data.autoclickers) + Number(this.savedGame.data.dlcAutoclickers);
        userData.transcendent = this.savedGame.data.transcendent;
        userData.paidForRubyMultiplier = this.savedGame.data.paidForRubyMultiplier;

        // TODO: Handle purchasedGilds
        userData.epicHeroReceivedUpTo = userData.highestFinishedZonePersist - (userData.highestFinishedZonePersist % 10);
        let gilds = Math.max(0, userData.epicHeroReceivedUpTo / 10 - 9);

        let currentHeroId = 1; // Start with Cid
        let currentHero = heroCollection.getById(currentHeroId);
        currentHero.epicLevel = gilds; // Assume the current hero has all gilds initially.

        // Seed with gold from finishing the first zone
        userData.addGold(userData.getGoldFromFinishingZone(1));
        userData.currentZoneHeight = 2;

        this.calculateAscensionZoneSteps = [{
            zone: userData.currentZoneHeight,
            hero: this.getShortName(currentHero),
            heroLevel: currentHero.level,
            heroGilds: currentHero.epicLevel,
            damage: attributes.currentAttack,
            gold: userData.gold,
        }];

        let didMakeProgress = true;
        while (didMakeProgress) {
            didMakeProgress = false;

            let didCurrentHeroChange = true;
            while (didCurrentHeroChange) {
                didCurrentHeroChange = false;

                let allUpgradesPurchased = true;
                let anyUpgradePurchased = false;

                // Purchase as much as possible, starting with upgrades.
                for (let upgradeId in upgradesMap) {
                    let upgrade = upgradesMap[upgradeId];
                    if (userData.hasUpgrade(upgrade.id) || upgrade.heroId !== currentHeroId) {
                        continue;
                    }

                    // Level hero to where they can purchase the upgrade
                    if (upgrade.heroLevelRequired > currentHero.level) {
                        let cost = currentHero.getCostUpToLevel(upgrade.heroLevelRequired);
                        if (cost.greaterThan(userData.gold)) {
                            allUpgradesPurchased = false;
                            break;
                        }

                        userData.levelHero(currentHero, upgrade.heroLevelRequired - currentHero.level);
                    }

                    // Purchase upgrade
                    if (userData.canAffordUpgrade(upgrade)) {
                        userData.purchaseUpgrade(upgrade);
                        anyUpgradePurchased = true;
                    } else {
                        allUpgradesPurchased = false;
                        break;
                    }
                }

                // If we have all the upgrades and can purchase the next hero, do so.
                // TODO: Handle the weird interaction between Cadu and Ceus. Users should swap between them every upgrade.
                if (allUpgradesPurchased && userData.gold.greaterThanOrEqualTo(heroCollection.getById(currentHeroId + 1).getCostUpToLevel(1))) {
                    currentHeroId++;
                    didCurrentHeroChange = true;
                } else if (anyUpgradePurchased) {
                    // Special case Cadu and Ceus as they're intended to be swapped back and forth.
                    if (currentHeroId === 47) {
                        currentHeroId = 48;
                        didCurrentHeroChange = true;
                    } else if (currentHeroId === 48) {
                        currentHeroId = 47;
                        didCurrentHeroChange = true;
                    }
                }

                if (didCurrentHeroChange) {
                    currentHero = heroCollection.getById(currentHeroId);
                }
            }

            // Spend all the rest of the gold on the current hero. Based on "Q-Click" logic.
            let levelPurchaseAmount = 9999;
            let didLevel = true;
            while (didLevel) {
                didLevel = false;

                if (userData.gold.greaterThanOrEqualTo(currentHero.getCostUpToLevel(currentHero.level + levelPurchaseAmount))) {
                    didLevel = true;
                    userData.levelHero(currentHero, levelPurchaseAmount);
                } else if (userData.gold.greaterThanOrEqualTo(currentHero.getCostUpToLevel(currentHero.level + 1))) {
                    let loc5 = 1;
                    let loc6 = levelPurchaseAmount - 1;
                    while (loc5 <= loc6) {
                        let loc7 = Math.floor((loc5 + loc6) / 2);
                        if (userData.gold.greaterThan(currentHero.getCostUpToLevel(currentHero.level + loc7))) {
                            loc5 = loc7 + 1;
                        } else {
                            loc6 = loc7 - 1;
                        }
                    }

                    didLevel = true;
                    userData.levelHero(currentHero, Math.floor((loc5 + loc6) / 2));
                }
            }

            // Regild
            userData.moveAllGildsToHero(currentHeroId);
            attributes.recalculate();

            // Get how far the user can insta-kill with the current damage
            let instakillStopZone = userData.getInstakillStopZone();

            if (userData.currentZoneHeight < instakillStopZone) {
                // Accumulate gold from finishing these zones
                // As an optimization, only care about the last 100 zones. Beyond that the gold would be trivial.
                let startingZone = Math.max(userData.currentZoneHeight, instakillStopZone - 100);
                for (let i = startingZone; i < instakillStopZone; i++) {
                    userData.addGold(userData.getGoldFromFinishingZone(i));
                }

                userData.currentZoneHeight = instakillStopZone;

                // Open all new gilds and move them to the current hero
                if (userData.highestFinishedZonePersist < userData.currentZoneHeight) {
                    userData.highestFinishedZonePersist = userData.currentZoneHeight;
                    userData.openAllZoneGildedHeroes();
                    userData.moveAllGildsToHero(currentHeroId);
                    attributes.recalculate();
                }

                didMakeProgress = true;

                this.calculateAscensionZoneSteps.push({
                    zone: userData.currentZoneHeight,
                    hero: this.getShortName(currentHero),
                    heroLevel: currentHero.level,
                    heroGilds: currentHero.epicLevel,
                    damage: attributes.currentAttack,
                    gold: userData.gold,
                });
            }
        }

        this.appInsights.trackMetric("AscensionZone", Date.now() - startTime);
    }

    private getShortName(entity: { name: string }): string {
        let commaIndex = entity.name.indexOf(",");
        return commaIndex >= 0
            ? entity.name.substring(0, commaIndex)
            : entity.name;
    }
}
