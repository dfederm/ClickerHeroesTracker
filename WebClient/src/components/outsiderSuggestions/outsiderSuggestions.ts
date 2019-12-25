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

        let borbToZone = [];
        borbToZone[184] = null;
        borbToZone = borbToZone.concat([966409, 979782, 979782, 984260, 984260, 984260, 988749, 988749, 988749, 988749, 993248, 993248, 993248, 993248, 993248, 993248, 993248, 993248, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1021386, 1309598, 1309598, 1309598, 1309598, 1309598, 1309598, 1325760, 1325760, 1340689, 1340689, 1340689, 1340689, 1340689, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 1367215, 2115108, 2115108, 2115108, 2134603, 2134603, 2134603, 2134603, 2134603, 2162796, 2180849, 2180849, 2180849, 2180849, 2180849, 2180849, 2180849, 2197526, 2197526, 2197526, 2197526, 2197526, 2212929, 2227158, 2227158, 2227158, 2227158, 2227158, 2227158, 2227158, 2227158, 2227158, 2227158, 2227158, 2227158, 2227158, 2227158, 2227158, 2227158, 2227158, 2227158, 2227158, 2227158, 2227158, 2227158, 2227158, 2227158, 2227158, 2227158, 2227158, 2227158, 2227158, 2227158, 2227158, 2227158, 2227158, 2227158, 2227158, 2227158, 2227158, 2227158, 2429081, 2459157, 2459157, 2459157, 2459157, 2459157, 2459157, 2459157, 2459157, 2459157, 2486938, 2486938, 2486938, 2486938, 2486938, 2486938, 2486938, 2486938, 2547841, 2547841, 2547841, 2547841, 2547841, 2547841, 2547841, 2547841, 2547841, 2580393, 2580393, 2580393, 2580393, 2580393, 2580393, 2610461, 2610461, 2610461, 2610461, 2610461, 2610461, 2610461, 2638235, 2638235, 2638235, 2638235, 2638235, 2638235, 2638235, 2638235, 2638235, 2638235, 2638235, 2663889, 2663889, 2663889, 2663889, 2663889, 2663889, 2663889, 2663889, 2663889, 2741250, 2741250, 2741250, 2741250, 2741250, 2741250, 2741250, 2741250, 2741250, 2741250, 2741250, 2741250, 2741250, 2741250, 2741250, 2741250, 2741250, 2741250, 2741250, 2741250, 2741250, 2741250, 2741250, 2741250, 2822756, 2822756, 2822756, 2822756, 2822756, 2867255, 2867255, 2867255, 2867255, 2867255, 2886991, 2886991, 2886991, 2886991, 2886991, 2886991, 2886991, 2886991, 2886991, 2886991, 2886991, 2886991, 2886991, 2984324, 2984324, 2984324, 2984324, 2984324, 2984324, 2984324, 2984324, 2984324, 3067321, 3067321, 3067321, 3067321, 3067321, 3115671, 3115671, 3115671, 3115671, 3115671, 3115671, 3115671, 3115671, 3115671, 3115671, 3115671, 3115671, 3115671, 3160331, 3160331, 3201583, 3201583, 3201583, 3201583, 3201583, 3201583, 3201583, 3201583, 3201583, 3201583, 3239688, 3239688, 3239688, 3239688, 3239688, 3239688, 3239688, 3239688, 3239688, 3274884, 3274884, 3274884, 3274884, 3274884, 3274884, 3274884, 3307395, 3307395, 3307395, 3307395, 3307395, 3307395, 3307395, 3307395, 3307395, 3348983, 3348983, 3348983, 3348983, 3348983, 3348983, 3348983, 3348983, 3348983, 3348983, 3348983, 3348983, 3387398, 3387398, 3387398, 3387398, 3387398, 3387398, 3455657, 3455657, 3455657, 3455657, 3455657, 3455657, 3455657, 3455657, 3497480, 3497480, 3497480, 3497480, 3497480, 3497480, 3497480, 3497480, 3497480, 3497480, 3497480, 3556905, 3556905, 3556905, 3556905, 3556905, 3556905, 3556905, 3556905, 3556905, 3556905, 3556905, 3611794, 3611794, 3611794, 3611794, 3611794, 3611794, 3611794, 3611794, 3611794, 3611794, 3611794, 3611794, 3662496, 3662496, 3662496, 3662496, 3662496, 3662496, 3662496, 3662496, 3662496, 3662496, 3662496, 3662496, 3662496, 3662496, 3720867, 3720867, 3720867, 3720867, 3720867, 3720867, 3720867, 3720867, 3720867, 3720867, 3720867, 3720867, 3720867, 3720867, 3774784, 3774784, 3774784, 3774784, 3774784, 3774784, 3824586, 3824586, 3824586, 3824586, 3824586, 3824586, 3824586, 3824586, 3824586, 3870588, 3870588, 3870588, 3870588, 3870588, 3870588, 3870588, 3870588, 3870588, 3870588, 3870588, 3870588, 3924638, 3924638, 3924638, 3924638, 3974563, 4020679, 4020679, 4020679, 4020679, 4020679, 4020679, 4020679, 4020679, 4020679, 4020679, 4020679, 4020679, 4063275, 4063275, 4102621, 4102621, 4102621, 4102621, 4102621, 4102621, 4102621, 4102621, 4102621, 4102621, 4102621, 4171306, 4171306, 4171306, 4171306, 4171306, 4171306, 4171306, 4171306, 4171306, 4171306, 4171306, 4171306, 4171306, 4171306, 4171306, 4171306, 4171306, 4171306, 4234750, 4234750, 4304891, 4304891, 4304891, 4304891, 4304891, 4304891, 4304891, 4304891, 4304891, 4304891, 4304891, 4304891, 4304891, 4304891, 4304891, 4304891, 4304891, 4304891, 4369680, 4369680, 4369680, 4369680, 4369680, 4369680, 4369680, 4369680, 4369680, 4369680, 4369680, 4369680, 4369680, 4369680, 4369680, 4369680, 4369680, 4369680, 4369680, 4369680, 4369680, 4429524, 4429524, 4429524, 4429524, 4429524, 4429524, 4429524, 4496360, 4496360, 4496360, 4496360, 4496360, 4496360, 4496360, 4496360, 4496360, 4496360, 4496360, 4496360, 4496360, 4496360, 4496360, 4558096, 4558096, 4667794, 4667794, 4667794, 4667794, 4667794, 4667794, 4667794, 4667794, 4727997, 4727997, 4727997, 4727997, 4727997, 4727997, 4727997, 4727997, 4727997, 4727997, 4727997, 4727997, 4727997, 4727997, 4727997, 4727997, 4727997, 4727997, 4727997, 4727997, 4727997, 4727997, 4834971, 4834971, 4834971, 4834971, 4834971, 4834971, 4834971, 4834971, 4834971, 4834971, 4834971, 4834971, 4834971, 4834971, 4834971, 4834971, 4834971, 4834971, 4834971, 4834971, 4834971, 4834971, 4882417, 4882417, 4882417, 4882417, 4882417, 4882417, 4882417, 4926243, 4926243, 4926243, 4926243, 4926243, 4926243, 4926243, 4926243, 4926243, 4926243, 4926243, 5070557, 5100026, 5100026, 5100026, 5100026, 5100026, 5100026, 5100026, 5100026, 5100026, 5100026, 5100026, 5100026, 5100026, 5100026, 5100026, 5100026, 5100026, 5152389, 5152389, 5152389, 5152389, 5152389, 5152389, 5152389, 5152389, 5152389, 5152389, 5152389, 5152389, 5152389, 5152389, 5152389, 5152389, 5152389, 5175613, 5175613, 5175613, 5175613, 5319130, 5319130, 5329630, 5329630, 5339329, 5339329, 5339329, 5348289, 5348289, 5348289, 5348289, 5348289, 5348289, 5348289, 5348289, 5348289, 5348289, 5348289, 5348289, 5348289, 5348289, 5348289, 5348289, 5348289, 5348289, 5348289, 5452205, 5452205, 5452205, 5452205, 5452205, 5452205, 5452205, 5452205, 5452205, 5452205, 5452205, 5452205, 5452205, 5452205, 5452205, 5452205, 5452205]);

        let ancientSouls = Number(this.savedGame.data.ancientSoulsTotal) || 0;
        let transcendentPower = (25 - 23 * Math.exp(-0.0003 * ancientSouls)) / 100;

        // Figure out goals for this transcendence
        let borbTarget = 0;
        if (ancientSouls < 100) {
            let a = ancientSouls + 42;
            this.newHze = (a / 5 - 6) * 51.8 * Math.log(1.25) / Math.log(1 + transcendentPower);
        } else if (ancientSouls < 10500) {
            this.newHze = (1 - Math.exp(-ancientSouls / 3900)) * 200000 + 4800;
        } else if (ancientSouls < 21000) {
            let x = 8000 + (10500 - ancientSouls) / 10500 * 4000;
            this.newHze = ancientSouls * 10.32 + x * 12;
        } else {
            let nonBorb = ancientSouls > 433000 ? 500 : 1000;
            let b = this.spendAS(1, ancientSouls - nonBorb);
            borbTarget = b * 5000;
            if (b > 1026) {
                this.newHze = 5.46e6;
            } else if (borbToZone[b]) {
                this.newHze = Math.max(borbTarget + 500, borbToZone[b]);
            } else {
                this.newHze = borbTarget + 500;
            }
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
            }

            if (phan < 3) {
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
}
