import { Component, Input } from "@angular/core";
import { AppInsightsService } from "@markpieszak/ng-application-insights";
import { gameData } from "../../models/gameData";
import { SavedGame } from "../../models/savedGame";
import { Decimal } from "decimal.js";

interface IOutsiderViewModel {
    id: number;
    name: string;
    currentLevel: number;
    suggestedLevel: number;
}

@Component({
    selector: "outsiderSuggestions",
    templateUrl: "./outsiderSuggestions.html",
})
export class OutsiderSuggestionsComponent {
    public outsiders: IOutsiderViewModel[] = [];
    public remainingAncientSouls = 0;

    public newHze: number;
    public newHeroSouls: Decimal;
    public newAncientSouls: number;
    public ancientSoulsDiff: number;
    public newTranscendentPower: number;

    private readonly simulatedAscensions = [
        4044232,
        4085089,
        4155170,
        4219902,
        4291233,
        4357121,
        4417981,
        4474197,
        4537681,
        4596321,
        4650485,
        4700517,
        4758279,
    ];

    public get savedGame(): SavedGame {
        return this._savedGame;
    }

    @Input()
    public set savedGame(savedGame: SavedGame) {
        this._savedGame = savedGame;
        this.refresh();
    }

    private _savedGame: SavedGame;

    private readonly outsidersByName: { [name: string]: IOutsiderViewModel } = {};

    constructor(
        private readonly appInsights: AppInsightsService,
    ) {
        for (const id in gameData.outsiders) {
            const outsiderDefinition = gameData.outsiders[id];

            // Skip the old Borb which is no longer in the game.
            // Unfotunately there's nothing in the game data that shows this, so hard-code it.
            if (id === "4") {
                continue;
            }

            let outsider: IOutsiderViewModel = {
                id: outsiderDefinition.id,
                name: outsiderDefinition.name,
                currentLevel: 0,
                suggestedLevel: 0,
            };

            this.outsiders.push(outsider);
            this.outsidersByName[outsider.name] = outsider;
        }

        this.outsiders = this.outsiders.sort((a, b) => a.id - b.id);
    }

    // tslint:disable-next-line:cyclomatic-complexity
    public refresh(): void {
        if (!this.savedGame) {
            return;
        }

        if (this.savedGame.data.outsiders && this.savedGame.data.outsiders.outsiders) {
            for (let i = 0; i < this.outsiders.length; i++) {
                let outsider = this.outsiders[i];
                let outsiderData = this.savedGame.data.outsiders.outsiders[outsider.id];
                outsider.currentLevel = outsiderData ? outsiderData.level : 0;
            }
        }

        let startTime = Date.now();

        let ancientSouls = Number(this.savedGame.data.ancientSoulsTotal) || 0;
        let transcendentPower = (25 - 23 * Math.exp(-0.0003 * ancientSouls)) / 100;

        // Figure out goals for this transcendence
        let nonBorb;
        let zonePush = 0;
        let zoneAdd = 0;
        if (ancientSouls < 100) {
            let a = ancientSouls + 42;
            this.newHze = (a / 5 - 6) * 51.8 * Math.log(1.25) / Math.log(1 + transcendentPower);
        } else if (ancientSouls < 10500) {
            this.newHze = (1 - Math.exp(-ancientSouls / 3900)) * 200000 + 4800;
        } else if (ancientSouls < 21000) {
            // 21k or +8000 Ancient Souls
            this.newHze = Math.max(225000, ancientSouls * 10.32 + 90000);
        } else if (ancientSouls < 333000) {
            // End Game
            if (ancientSouls < 27000) {
                nonBorb = 3000 + (27000 - ancientSouls) * 1.2;
            } else {
                nonBorb = 3000;
            }
            zonePush = ancientSouls / 16e4;
            let b = this.spendAS(1, ancientSouls - nonBorb);
            this.newHze = Math.min(5.46e6, b * 5000 + 500);
        } else if (ancientSouls < 530000) {
            // Post zone 4m
            nonBorb = 2000;
            zoneAdd = this.post4mStrategy(ancientSouls);
            let b = this.spendAS(1, ancientSouls - nonBorb);
            this.newHze = Math.min(5.46e6, b * 5000 + 500);
        } else {
            this.newHze = 5.46e6;
        }

        // Push beyond 2mpz
        let borbTarget = 0;
        if (ancientSouls >= 21000) {
            borbTarget = this.newHze;
            this.newHze = Math.min(5.5e6, (1 + zonePush / 100) * this.newHze + zoneAdd);
            this.newHze += 500 - this.newHze % 500;
        }

        this.newHze = Math.floor(this.newHze);
        let newLogHeroSouls = Math.log10(1 + transcendentPower) * this.newHze / 5 + 6;

        // Ancient effects
        let ancientLevels = Math.floor(newLogHeroSouls / Math.log10(2) - 3 / Math.log10(2)) - 1;
        let kuma = -8 * (1 - Math.exp(-0.025 * ancientLevels));
        let atman = 75 * (1 - Math.exp(-0.013 * ancientLevels));
        let bubos = -5 * (1 - Math.exp(-0.002 * ancientLevels));
        let chronos = 30 * (1 - Math.exp(-0.034 * ancientLevels));
        let dora = 9900 * (1 - Math.exp(-0.002 * ancientLevels));

        // Unbuffed Stats
        let nerfs = Math.floor(this.newHze / 500) * 500 / 500;
        let unbuffedMonstersPerZone = 10 + nerfs * 0.1;
        unbuffedMonstersPerZone = Math.round(unbuffedMonstersPerZone * 10) / 10;
        let unbuffedTreasureChestChance = Math.exp(-0.006 * nerfs);
        let unbuffedBossHealth = 10 + nerfs * 0.4;
        let unbuffedBossTimer = 30 - nerfs * 2;
        let unbuffedPrimalBossChance = 25 - nerfs * 2;

        // Outsider Caps
        let borbCap = borbTarget > 0
            ? Math.ceil((borbTarget - 500) / 5000)
            : ancientSouls >= 10500
                ? Math.ceil((this.newHze - 500) / 5000)
                : Math.max(0, Math.ceil(((unbuffedMonstersPerZone - 2.1) / - kuma - 1) / 0.125));
        let rhageistCap = Math.ceil(((100 - unbuffedPrimalBossChance) / atman - 1) / 0.25);
        let kariquaCap = Math.ceil(((unbuffedBossHealth - 5) / -bubos - 1) / 0.5);
        let orphalasCap = Math.max(1, Math.ceil(((2 - unbuffedBossTimer) / chronos - 1) / 0.75)) + 2;
        let senakhanCap = Math.max(1, Math.ceil((100 / unbuffedTreasureChestChance) / (dora / 100 + 1) - 1));

        // Outsider Ratios
        let rhageistRatio;
        let kariquaRatio;
        let orphalasRatio;
        let senakhanRatio;

        if (ancientSouls < 100) {
            let ratioChange = ancientSouls / 100;
            rhageistRatio = 0.2 * ratioChange;
            kariquaRatio = 0.01 * ratioChange;
            orphalasRatio = 0.05 * ratioChange;
            senakhanRatio = 0.05 * ratioChange;
        } else if (ancientSouls < 21000) {
            rhageistRatio = 0.2;
            kariquaRatio = 0.01;
            orphalasRatio = 0.05;
            senakhanRatio = 0.05;
        } else {
            rhageistRatio = 0;
            kariquaRatio = 0;
            orphalasRatio = 0;
            senakhanRatio = 0;
        }

        // Outsider Leveling
        this.remainingAncientSouls = ancientSouls;

        let borbFant = ancientSouls <= 2000
            ? Math.min(this.spendAS(0.35, this.remainingAncientSouls), this.getBorbFant(ancientSouls, transcendentPower))
            : 0;
        let borbHze = this.remainingAncientSouls >= 21000
            ? borbCap
            : Math.min(this.spendAS(0.5, this.remainingAncientSouls), borbCap + 1);
        let borbLevel = Math.max(borbFant, borbHze);

        if (this.getCostFromLevel(borbLevel) > (this.remainingAncientSouls - 5)) {
            borbLevel = this.spendAS(1, this.remainingAncientSouls - 5);
        }

        this.remainingAncientSouls -= this.getCostFromLevel(borbLevel);

        // Xyl sucks
        let xyliqilLevel = 0;
        this.remainingAncientSouls -= this.getCostFromLevel(xyliqilLevel);

        // Super outsiders
        let rhageistLevel = this.getCostFromLevel(rhageistCap) > (this.remainingAncientSouls * rhageistRatio)
            ? this.spendAS(rhageistRatio, this.remainingAncientSouls)
            : rhageistCap;
        let kariquaLevel = this.getCostFromLevel(kariquaCap) > (this.remainingAncientSouls * kariquaRatio)
            ? this.spendAS(kariquaRatio, this.remainingAncientSouls)
            : kariquaCap;
        let orphalasLevel = this.getCostFromLevel(orphalasCap) > (this.remainingAncientSouls * orphalasRatio)
            ? this.spendAS(orphalasRatio, this.remainingAncientSouls)
            : orphalasCap;
        let senakhanLevel = this.getCostFromLevel(senakhanCap) > (this.remainingAncientSouls * senakhanRatio)
            ? this.spendAS(senakhanRatio, this.remainingAncientSouls)
            : senakhanCap;

        this.remainingAncientSouls -= this.getCostFromLevel(rhageistLevel);
        this.remainingAncientSouls -= this.getCostFromLevel(kariquaLevel);
        this.remainingAncientSouls -= this.getCostFromLevel(orphalasLevel);
        this.remainingAncientSouls -= this.getCostFromLevel(senakhanLevel);

        // Chor, Phan, and Pony
        let [chorLevel, phanLevel, ponyLevel] = this.nOS(this.remainingAncientSouls, transcendentPower, this.newHze);

        this.remainingAncientSouls -= this.getCostFromLevel(chorLevel);
        this.remainingAncientSouls -= phanLevel;
        this.remainingAncientSouls -= this.getCostFromLevel(ponyLevel);

        // End of transcension estimates
        let ponyBonus = Math.pow(ponyLevel, 2) * 10;
        let series = 1 / (1 - 1 / (1 + transcendentPower));
        let buffedPrimalBossChance = Math.max(5, unbuffedPrimalBossChance + atman * (1 + rhageistLevel * 0.25));
        let pbcm = Math.min(buffedPrimalBossChance, 100) / 100;

        newLogHeroSouls = Math.log10(1 + transcendentPower) * (this.newHze - 100) / 5 + Math.log10(ponyBonus + 1) + Math.log10(20 * series * pbcm);
        this.newHeroSouls = Decimal.pow(10, newLogHeroSouls);
        this.newAncientSouls = Math.max(ancientSouls, Math.floor(newLogHeroSouls * 5));
        this.ancientSoulsDiff = this.newAncientSouls - ancientSouls;
        this.newTranscendentPower = (25 - 23 * Math.exp(-0.0003 * this.newAncientSouls)) / 100;

        // Update outtsider view models
        this.outsidersByName.Xyliqil.suggestedLevel = xyliqilLevel;
        this.outsidersByName["Chor'gorloth"].suggestedLevel = chorLevel;
        this.outsidersByName.Ponyboy.suggestedLevel = ponyLevel;
        this.outsidersByName.Phandoryss.suggestedLevel = phanLevel;
        this.outsidersByName.Borb.suggestedLevel = borbLevel;
        this.outsidersByName.Rhageist.suggestedLevel = rhageistLevel;
        this.outsidersByName["K'Ariqua"].suggestedLevel = kariquaLevel;
        this.outsidersByName.Orphalas.suggestedLevel = orphalasLevel;
        this.outsidersByName["Sen-Akhan"].suggestedLevel = senakhanLevel;

        this.appInsights.trackMetric("OutsiderSuggestions", Date.now() - startTime);
    }

    private nOS(ancientSouls: number, transcendentPower: number, zone: number): [number, number, number] {
        let pony = 0;
        if (ancientSouls > 20000) {
            ancientSouls -= 11325;
            pony = this.spendAS(0.88, ancientSouls);
            ancientSouls -= this.getCostFromLevel(pony);
            return [150, ancientSouls, pony];
        }
        let hpMultiplier = Math.min(1.545, 1.145 + zone / 500000);
        let hsMultiplier = Math.pow(1 + transcendentPower, 0.2);
        let heroDamageMultiplier = (zone > 1.2e6) ? 1000 : ((zone > 168000) ? 4.5 : 4);
        let heroCostMultiplier = (zone > 1.2e6) ? 1.22 : 1.07;
        let goldToDps = Math.log10(heroDamageMultiplier) / Math.log10(heroCostMultiplier) / 25;
        let dpsToZones = Math.log10(hpMultiplier) - Math.log10(1.15) * goldToDps;
        let chor = 0;
        let phan = 0;

        let chorBuff = 1 / 0.95;

        while (ancientSouls > 0) {
            if (pony < 1) {
                ancientSouls -= ++pony;
                continue;
            } else if (phan < 3) {
                phan++;
                ancientSouls--;
                continue;
            }

            let damageIncrease = (phan + 2) / (phan + 1);
            let zoneIncrease = Math.log10(damageIncrease) / dpsToZones;
            let phanBuff = Math.pow(hsMultiplier, zoneIncrease);

            // Boost Phandoryss for FANT
            if (phan < 50) {
                phanBuff *= Math.pow(1.1, 1 / phan);
            }

            if (chor < ancientSouls && chor < 150) {
                let chorBpAS = Math.pow(chorBuff, 1 / (chor + 1));
                if (chorBpAS >= phanBuff) {
                    if (pony < ancientSouls) {
                        let ponyBuff = (Math.pow(pony + 1, 2) * 10 + 1) / (Math.pow(pony, 2) * 10 + 1);
                        let ponyBpAS = Math.pow(ponyBuff, 1 / (pony + 1));
                        if (ponyBpAS >= chorBpAS) {
                            ancientSouls -= ++pony;
                            continue;
                        }
                    }
                    ancientSouls -= ++chor;
                    continue;
                }
            }

            if (pony < ancientSouls) {
                let ponyBuff = (Math.pow(pony + 1, 2) * 10 + 1) / (Math.pow(pony, 2) * 10 + 1);
                let ponyBpAS = Math.pow(ponyBuff, 1 / (pony + 1));
                if (ponyBpAS >= phanBuff) {
                    ancientSouls -= ++pony;
                    continue;
                }
            }

            phan++;
            ancientSouls--;

        }

        return [chor, phan, pony];
    }

    private getBorbFant(ancientSouls: number, transcendentPower: number): number {
        let [chor, , pony] = this.nOS(ancientSouls * 0.5, transcendentPower, 100);
        let ponyBonus = Math.pow(pony, 2) * 10 + 1;
        let chorBonus = Math.pow(1 / 0.95, chor);
        let tp = 1 + transcendentPower;
        let s = tp * tp;
        let a = s + s * s + s * s * s;
        let hsFant = 20 * ponyBonus * a;
        let logHSFant = Math.log10(Math.max(1, hsFant));
        logHSFant += Math.log10(chorBonus);
        let kumaFant = Math.max(1, Math.floor(logHSFant / Math.log10(2) - 3 / Math.log(2)) - 1);
        let kumaEffect = 8 * (1 - Math.exp(-0.025 * kumaFant));
        return Math.ceil((8 / kumaEffect - 1) * 8);
    }

    private getCostFromLevel(level: number): number {
        return (level + 1) * (level / 2);
    }

    private spendAS(ratio: number, as: number): number {
        let spendable = ratio * as;
        if (spendable < 1) {
            return 0;
        }

        return Math.floor(Math.sqrt(8 * spendable + 1) / 2 - 0.5);
    }

    private post4mStrategy(ancientSouls: number): number {
        let oldHZE = Math.round(ancientSouls / 0.09691) - 170;
        let borb = this.spendAS(1, ancientSouls - 2000);
        let borbLimit = borb * 5000 + 500;
        let zonesTraveled = this.getZonesTraveled(borbLimit, 0);
        let zonesGained = borbLimit - oldHZE;
        let efficiency = zonesGained / zonesTraveled;
        let zoneAdd = 0;
        let addMPZ = 256;
        let zoneAddA = 0;
        let efficiencyA = efficiency;
        let zoneAddB = 0;
        let efficiencyB = efficiency;
        do {
            zonesTraveled = this.getZonesTraveled(borbLimit, zoneAdd + addMPZ * 500);
            zonesGained = borbLimit + zoneAdd + addMPZ * 500 - oldHZE;
            let newEfficiency = zonesTraveled === Infinity
                ? - Infinity
                : zonesGained / zonesTraveled;
            if (newEfficiency > efficiency) {
                zoneAddA = zoneAddB;
                efficiencyA = efficiencyB;
                zoneAddB = zoneAdd;
                efficiencyB = efficiency;
                efficiency = newEfficiency;
                zoneAdd += addMPZ * 500;
            } else {
                zoneAdd = zoneAddA;
                efficiency = efficiencyA;
                zoneAddB = zoneAddA;
                efficiencyB = efficiencyA;
                if (addMPZ > 2) {
                    addMPZ /= 2;
                }
                addMPZ = Math.floor(addMPZ / 2);
            }
        } while (addMPZ > 0);
        return zoneAdd;
    }

    private getZonesTraveled(borbLimit: number, zoneAdd: number): number {
        let targetZone = borbLimit + zoneAdd;
        let zonesTraveled = 45000000;
        for (let i = 0; i < this.simulatedAscensions.length; i++) {
            let ascZone = this.simulatedAscensions[i];
            if (ascZone > targetZone) {
                zonesTraveled += targetZone - Math.round(ascZone * 0.9) + 30000;
                if (targetZone > borbLimit) {
                    let zonePush = targetZone - borbLimit;
                    zonesTraveled += Math.round(zonePush * zonePush / 10000);
                }
                break;
            } else {
                zonesTraveled += Math.round(ascZone / 10 + 30000);
                if (ascZone > borbLimit) {
                    let zonePush = ascZone - borbLimit;
                    zonesTraveled += Math.round(zonePush * zonePush / 10000);
                }
            }
        }
        if (targetZone >= 4811634) {
            let ascZone = 4811634;
            let zoneIncrease = 49283;
            do {
                if (ascZone > targetZone) {
                    zonesTraveled += targetZone - Math.round(ascZone * 0.9) + 30000;
                    if (targetZone > borbLimit) {
                        let zonePush = targetZone - borbLimit;
                        zonesTraveled += Math.round(zonePush * zonePush / 10000);
                    }
                } else {
                    zonesTraveled += Math.round(ascZone / 10 + 30000);
                    if (ascZone > borbLimit) {
                        let zonePush = ascZone - borbLimit;
                        zonesTraveled += Math.round(zonePush * zonePush / 10000);
                    }
                }
                ascZone += zoneIncrease;
                zoneIncrease = Math.round(zoneIncrease * 0.923989);
                if (zoneIncrease < 10) {
                    return Infinity;
                }
            } while (ascZone <= targetZone);
        }
        return zonesTraveled;
    }
}
