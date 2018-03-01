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

    public get savedGame(): SavedGame {
        return this._savedGame;
    }

    @Input()
    public set savedGame(savedGame: SavedGame) {
        this._savedGame = savedGame;
        this.refresh();
    }

    public get useBeta(): boolean {
        return this._useBeta;
    }
    public set useBeta(value: boolean) {
        this._useBeta = value;
        this.refresh();
    }

    private _savedGame: SavedGame;
    private _useBeta = false;

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
    private refresh(): void {
        if (!this.savedGame) {
            return;
        }

        if (this.savedGame.data.outsiders && this.savedGame.data.outsiders.outsiders) {
            for (let i = 0; i < this.outsiders.length; i++) {
                let outsider = this.outsiders[i];
                let outsiderData = this.savedGame.data.outsiders.outsiders[outsider.id];
                if (outsiderData) {
                    outsider.currentLevel = outsiderData.level;
                }
            }
        }

        const latencyCounter = "OutsiderSuggestions";
        this.appInsights.startTrackEvent(latencyCounter);

        let ancientSouls = Number(this.savedGame.data.ancientSoulsTotal) || 0;
        let transcendentPower = (25 - 23 * Math.exp(-0.0003 * ancientSouls)) / 100;

        // Figure out goals for this transcendence
        if (ancientSouls < 100) {
            let a = ancientSouls + 42;
            this.newHze = (a / 5 - 6) * 51.8 * Math.log(1.25) / Math.log(1 + transcendentPower);
        } else if (ancientSouls < 10500) {
            this.newHze = (1 - Math.exp(-ancientSouls / 3900)) * 200000 + 4800;
        } else if (ancientSouls < 14500) {
            // ~ +8000 Ancient Souls
            this.newHze = ancientSouls * 10.32 + 90000;
        } else if (ancientSouls < 18000) {
            // 27k Ancient Souls
            this.newHze = 284000;
        } else if (ancientSouls < 27000) {
            // +59% Ancient Souls
            this.newHze = ancientSouls * 16.4;
        } else if (ancientSouls < 60000) {
            let b = this.spendAS(1, ancientSouls * 0.75);
            this.newHze = b * 5000 + (this.useBeta ? 0 : 46500);
        } else {
            let b = this.spendAS(1, ancientSouls - 15000);
            this.newHze = Math.min(5e6, b * 5000 + (this.useBeta ? 0 : 46500));
        }

        this.newHze = Math.floor(this.newHze);
        let newLogHeroSouls = Math.log10(1 + transcendentPower) * this.newHze / 5 + 6;

        // Ancient effects
        let ancientLevels = Math.floor(newLogHeroSouls / Math.log10(2) - Math.log(25) / Math.log(2)) + -1;
        let kuma = this.useBeta
            ? -8 * (1 - Math.exp(-0.025 * ancientLevels))
            : -100 * (1 - Math.exp(-0.0025 * ancientLevels));
        let atman = 75 * (1 - Math.exp(-0.013 * ancientLevels));
        let bubos = -5 * (1 - Math.exp(-0.002 * ancientLevels));
        let chronos = 30 * (1 - Math.exp(-0.034 * ancientLevels));
        let dora = 9900 * (1 - Math.exp(-0.002 * ancientLevels));

        // Unbuffed Stats
        let nerfs = Math.floor(this.newHze / 500) * 500 / 500;
        let unbuffedMonstersPerZone = 10 + nerfs * (this.useBeta ? 0.1 : 1);
        let unbuffedTreasureChestChance = Math.exp(-0.006 * nerfs);
        let unbuffedBossHealth = 10 + nerfs * 0.4;
        let unbuffedBossTimer = 30 - nerfs * 2;
        let unbuffedPrimalBossChance = 25 - nerfs * 2;

        // Outsider Caps
        let borbCap = Math.max(0, Math.ceil(((unbuffedMonstersPerZone - 2) / -kuma - 1) / (this.useBeta ? 0.125 : 0.1)));
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
        } else if (ancientSouls < 27000) {
            rhageistRatio = 0.2;
            kariquaRatio = 0.01;
            orphalasRatio = 0.05;
            senakhanRatio = 0.05;
        } else if (ancientSouls < 60000) {
            // TODO: Extrapolate from spreadsheets between 27k and 60k AS
            rhageistRatio = 0.2;
            kariquaRatio = 0.01;
            orphalasRatio = 0.05;
            senakhanRatio = 0.05;
        } else {
            // TODO: Extrapolate from spreadsheets between 60k and 420k AS
            rhageistRatio = 0.1;
            kariquaRatio = 0.005;
            orphalasRatio = 0.025;
            senakhanRatio = 0.025;
        }

        // Outsider Leveling
        this.remainingAncientSouls = ancientSouls;

        let borbLevel: number;
        if (this.useBeta) {
            let borb15 = Math.min(15, this.spendAS(0.5, this.remainingAncientSouls));
            let borb10pc = this.spendAS(0.1, this.remainingAncientSouls);
            let borbLate = this.remainingAncientSouls >= 10000 ? borbCap : 0;
            borbLevel = Math.max(borb15, borb10pc, borbLate);
        } else {
            borbLevel = Math.max((this.remainingAncientSouls >= 300) ? 15 : this.spendAS(0.4, this.remainingAncientSouls), borbCap);
        }

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

        newLogHeroSouls = Math.log10(1 + transcendentPower) * (this.newHze - 105) / 5 + Math.log10(ponyBonus + 1) + Math.log10(20 * series * pbcm);
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

        this.appInsights.stopTrackEvent(latencyCounter);
    }

    private nOS(ancientSouls: number, transcendentPower: number, zone: number): [number, number, number] {
        let hpMultiplier = Math.min(1.545, 1.145 + zone / 500000);
        let hsMultiplier = Math.pow(1 + transcendentPower, 0.2);
        let heroDamageMultiplier = (zone > 1.23e6) ? 1000 : ((zone > 168000) ? 4.5 : 4);
        let heroCostMultiplier = (zone > 1.23e6) ? 1.22 : 1.07;
        let goldToDps = Math.log10(heroDamageMultiplier) / Math.log10(heroCostMultiplier) / 25;
        let dpsToZones = Math.log10(hpMultiplier) - Math.log10(1.15) * goldToDps;
        let chor = 0;
        let phan = 0;
        let pony = 0;

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

            if (phan < 5) {
                phanBuff *= 1.3;
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
}
