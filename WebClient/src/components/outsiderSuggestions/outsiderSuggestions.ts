import { Component, Input } from "@angular/core";
import { AppInsightsService } from "@markpieszak/ng-application-insights";
import { gameData } from "../../models/gameData";
import { SavedGame } from "../../models/savedGame";
import { Decimal } from "decimal.js";

interface IOutsiderViewModel {
    id: number;
    name: string;
    currentLevel: Decimal;
    suggestedLevel: Decimal;
}

@Component({
    selector: "outsiderSuggestions",
    templateUrl: "./outsiderSuggestions.html",
})
export class OutsiderSuggestionsComponent {
    public outsiders: IOutsiderViewModel[] = [];
    public remainingAncientSouls: Decimal = new Decimal(0);

    public get savedGame(): SavedGame {
        return this._savedGame;
    }

    @Input()
    public set savedGame(savedGame: SavedGame) {
        this._savedGame = savedGame;
        this.refresh();
    }

    public get focusBorb(): boolean {
        return this._focusBorb;
    }
    public set focusBorb(value: boolean) {
        this._focusBorb = value;
        this.refresh();
    }

    private _savedGame: SavedGame;
    private _focusBorb = true;

    private outsidersByName: { [name: string]: IOutsiderViewModel } = {};

    constructor(
        private appInsights: AppInsightsService,
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
                currentLevel: new Decimal(0),
                suggestedLevel: new Decimal(0),
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
                    outsider.currentLevel = new Decimal(outsiderData.level || 0);
                }
            }
        }

        let totalAncientSouls = new Decimal(this.savedGame.data.ancientSoulsTotal || 0);
        if (totalAncientSouls.isZero()) {
            for (let outsider of this.outsiders) {
                outsider.suggestedLevel = new Decimal(0);
            }

            this.remainingAncientSouls = new Decimal(0);
            return;
        }

        const latencyCounter = "OutsiderSuggestions";
        this.appInsights.startTrackEvent(latencyCounter);

        let isMoreThan30000 = totalAncientSouls.greaterThan(30000);
        let isLessThan100 = totalAncientSouls.lessThan(100);

        let ponyLevel: Decimal;
        let chorLevel: Decimal;
        let phanLevel: Decimal;
        let borbLevel: Decimal;
        let rhageistLevel: Decimal;
        let kariquaLevel: Decimal;
        let orphalasLevel: Decimal;
        let senAkhanLevel: Decimal;

        // For all but Phan
        let costToLevelFunc = (cost: Decimal) => cost.times(8).plus(1).squareRoot().minus(1).dividedBy(2).floor();
        let levelToCostFunc = (level: Decimal) => level.plus(1).times(level).dividedBy(2);

        let halfAncientSouls = totalAncientSouls.dividedBy(2).floor();

        // Pony and Chor
        if (isMoreThan30000) {
            ponyLevel = new Decimal(0);
            chorLevel = new Decimal(0);
        } else if (isLessThan100) {
            // Get highest buyable level for Pony
            ponyLevel = costToLevelFunc(totalAncientSouls);

            // If what's remaining is less than 5, go 1 less
            if (totalAncientSouls.minus(levelToCostFunc(ponyLevel)).lessThan(5)) {
                ponyLevel = ponyLevel.minus(1);
            }

            chorLevel = new Decimal(0);
        } else {
            // Spend at most half on Pony and Chor
            let maxPonyChorLevel = costToLevelFunc(halfAncientSouls);

            let ponyBenefitFunc = (level: Decimal) => level.times(level).times(10).plus(1);

            let chorReduction = new Decimal(1).dividedBy(0.95);
            let chorBenefitFunc = (level: Decimal) => chorReduction.pow(level);

            // Pony-favored leveling
            let ponyFavoredMaxTotalBenefit = new Decimal(0);
            let ponyFavoredPonyLevel = new Decimal(0);
            let ponyFavoredChorLevel = new Decimal(0);
            ponyLevel = maxPonyChorLevel;
            chorLevel = new Decimal(0);
            while (ponyLevel.greaterThan(chorLevel)) {
                let ponyCost = levelToCostFunc(ponyLevel);
                let ponyBenefit = ponyBenefitFunc(ponyLevel);

                chorLevel = Decimal.min(costToLevelFunc(halfAncientSouls.minus(ponyCost)), 150);
                let chorBenefit = chorBenefitFunc(chorLevel);

                let totalBenefit = ponyBenefit.times(chorBenefit);
                if (totalBenefit.greaterThan(ponyFavoredMaxTotalBenefit)) {
                    ponyFavoredMaxTotalBenefit = totalBenefit;
                    ponyFavoredPonyLevel = ponyLevel;
                    ponyFavoredChorLevel = chorLevel;
                }

                ponyLevel = ponyLevel.minus(1);
            }

            // Chor-favored leveling
            let chorFavoredMaxTotalBenefit = new Decimal(0);
            let chorFavoredPonyLevel = new Decimal(0);
            let chorFavoredChorLevel = new Decimal(0);
            chorLevel = Decimal.min(maxPonyChorLevel, 150);
            ponyLevel = new Decimal(0);
            while (chorLevel.greaterThan(ponyLevel)) {
                let chorCost = levelToCostFunc(chorLevel);
                let chorBenefit = chorBenefitFunc(chorLevel);

                ponyLevel = costToLevelFunc(halfAncientSouls.minus(chorCost));
                let ponyBenefit = ponyBenefitFunc(ponyLevel);

                let totalBenefit = ponyBenefit.times(chorBenefit);
                if (totalBenefit.greaterThan(chorFavoredMaxTotalBenefit)) {
                    chorFavoredMaxTotalBenefit = totalBenefit;
                    chorFavoredPonyLevel = ponyLevel;
                    chorFavoredChorLevel = chorLevel;
                }

                chorLevel = chorLevel.minus(1);
            }

            // Choose the better Chor/Pony approach
            if (ponyFavoredMaxTotalBenefit.greaterThan(chorFavoredMaxTotalBenefit)) {
                ponyLevel = ponyFavoredPonyLevel;
                chorLevel = ponyFavoredChorLevel;
            } else {
                ponyLevel = chorFavoredPonyLevel;
                chorLevel = chorFavoredChorLevel;
            }
        }

        if (isMoreThan30000 || isLessThan100) {
            rhageistLevel = new Decimal(0);
            kariquaLevel = new Decimal(0);
            orphalasLevel = new Decimal(0);
            senAkhanLevel = new Decimal(0);
        }

        // Borb, Phan, amd super outsiders
        if (isLessThan100) {
            let ponyCost = levelToCostFunc(ponyLevel);
            let soulsMinusPonyCost = totalAncientSouls.minus(ponyCost);

            borbLevel = soulsMinusPonyCost.greaterThan(4) ? new Decimal(2) : new Decimal(1);
            phanLevel = soulsMinusPonyCost.minus(levelToCostFunc(borbLevel));
        } else {
            let borbCostCap: Decimal;
            if (isMoreThan30000) {
                phanLevel = new Decimal(0);
                borbCostCap = totalAncientSouls.times(0.9);
            } else {
                phanLevel = totalAncientSouls.times(0.15).floor();

                let superOutsiderLevel = costToLevelFunc(totalAncientSouls.dividedBy(20).floor());

                rhageistLevel = Decimal.min(superOutsiderLevel, 50);
                kariquaLevel = Decimal.min(superOutsiderLevel, 10);
                orphalasLevel = Decimal.min(superOutsiderLevel, 10);
                senAkhanLevel = Decimal.min(superOutsiderLevel, 20);

                borbCostCap = totalAncientSouls.times(0.04);
                if (this.focusBorb) {
                    let extra = levelToCostFunc(superOutsiderLevel)
                        .times(4)
                        .minus(levelToCostFunc(rhageistLevel))
                        .minus(levelToCostFunc(kariquaLevel))
                        .minus(levelToCostFunc(orphalasLevel))
                        .minus(levelToCostFunc(senAkhanLevel));
                    borbCostCap = borbCostCap.plus(extra);
                }
            }

            borbLevel = Decimal.min(costToLevelFunc(borbCostCap), 245);
        }

        this.outsidersByName["Chor'gorloth"].suggestedLevel = chorLevel;
        this.outsidersByName.Ponyboy.suggestedLevel = ponyLevel;
        this.outsidersByName.Phandoryss.suggestedLevel = phanLevel;
        this.outsidersByName.Borb.suggestedLevel = borbLevel;
        this.outsidersByName.Rhageist.suggestedLevel = rhageistLevel;
        this.outsidersByName["K'Ariqua"].suggestedLevel = kariquaLevel;
        this.outsidersByName.Orphalas.suggestedLevel = orphalasLevel;
        this.outsidersByName["Sen-Akhan"].suggestedLevel = senAkhanLevel;

        this.remainingAncientSouls = totalAncientSouls
            .minus(levelToCostFunc(chorLevel))
            .minus(levelToCostFunc(ponyLevel))
            .minus(phanLevel)
            .minus(levelToCostFunc(borbLevel))
            .minus(levelToCostFunc(rhageistLevel))
            .minus(levelToCostFunc(kariquaLevel))
            .minus(levelToCostFunc(orphalasLevel))
            .minus(levelToCostFunc(senAkhanLevel));

        this.appInsights.stopTrackEvent(latencyCounter);
    }
}
