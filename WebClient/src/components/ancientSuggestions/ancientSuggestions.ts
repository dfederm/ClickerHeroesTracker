import { Component, OnInit, Input } from "@angular/core";
import { AppInsightsService } from "@markpieszak/ng-application-insights";
import { gameData } from "../../models/gameData";
import { SettingsService, IUserSettings } from "../../services/settingsService/settingsService";
import { SavedGame } from "../../models/savedGame";
import { Decimal } from "decimal.js";
import { NgbModal, NgbModalRef } from "@ng-bootstrap/ng-bootstrap";
import { FeedbackDialogComponent } from "../feedbackDialog/feedbackDialog";
import { UploadService } from "../../services/uploadService/uploadService";
import { Router } from "@angular/router";
import { HttpErrorResponse } from "@angular/common/http";

interface IAncientViewModel {
    id: string;
    name: string;
    ancientLevel: Decimal;
    itemLevel: Decimal;
    effectiveLevel: Decimal;
    suggestedLevel?: Decimal;
    diffValue?: Decimal;
    diffCopyValue?: string;
    costToLevel?: Decimal;
    isBase?: boolean;
    purchaseTime: number;
}

@Component({
    selector: "ancientSuggestions",
    templateUrl: "./ancientSuggestions.html",
    styleUrls: ["./ancientSuggestions.css"],
})
export class AncientSuggestionsComponent implements OnInit {
    private static readonly exponentialRegex = /^(\d+(\.\d+)?)e\+?(\d+)$/i;

    // These all have effective caps where additional levels add less than floating point numbers can handle.
    private static readonly ancientLevelCaps: { [ancientName: string]: number } = {
        Atman: 2880,
        Bubos: 18715,
        Chronos: 1101,
        Dogcog: 3743,
        Dora: 18715,
        Fortuna: 14972,
        Kumawakamaru: 1498,
        Revolc: 3743,
        Vaagur: 1440,
    };

    public FeedbackDialogComponent = FeedbackDialogComponent;

    public ancients: IAncientViewModel[] = [];

    public availableSouls: Decimal = new Decimal(0);
    public spentSouls: Decimal = new Decimal(0);
    public remainingSouls: Decimal = new Decimal(0);
    public pendingSouls: Decimal = new Decimal(0);

    public autoLeveledSavedGame: SavedGame;
    public modalErrorMessage: string;
    public isModalLoading: boolean;

    public get playStyle(): string {
        return this._playStyle;
    }

    @Input()
    public set playStyle(playStyle: string) {
        this._playStyle = playStyle;
        this.hydrateAncientSuggestions();
    }

    public get savedGame(): SavedGame {
        return this._savedGame;
    }

    @Input()
    public set savedGame(savedGame: SavedGame) {
        this._savedGame = savedGame;
        this.handleSavedGame();
    }

    public get suggestionType(): string {
        return this._suggestionType;
    }
    public set suggestionType(value: string) {
        this._suggestionType = value;
        this.hydrateAncientSuggestions();
    }

    public get useSoulsFromAscension(): boolean {
        return this._useSoulsFromAscension;
    }
    public set useSoulsFromAscension(value: boolean) {
        this._useSoulsFromAscension = value;
        this.hydrateAncientSuggestions();
    }

    private _playStyle: string;
    private _savedGame: SavedGame;
    private _suggestionType = "AvailableSouls";
    private _useSoulsFromAscension = false;

    // An index for quick lookup of ancient cost formulas.
    // Each formula gets the sum of the cost of the ancient from 1 to N.
    private readonly ancientCostFormulas = this.getAncientCostFormulas();

    private readonly ancientsByName: { [name: string]: IAncientViewModel } = {};

    private settings: IUserSettings;
    private ancientCostMultiplier: Decimal;

    private heroSouls: Decimal = new Decimal(0);
    private highestZoneThisTranscension: Decimal = new Decimal(0);
    private totalAncientSouls: Decimal = new Decimal(0);
    private transcendentPower: Decimal = new Decimal(0);
    private autoclickers: Decimal = new Decimal(0);

    private autolevelModal: NgbModalRef;

    constructor(
        private readonly appInsights: AppInsightsService,
        private readonly settingsService: SettingsService,
        private readonly modalService: NgbModal,
        private readonly router: Router,
        private readonly uploadService: UploadService,
    ) {
        for (const id in gameData.ancients) {
            const ancientDefinition = gameData.ancients[id];

            // Skip ancients no longer in the game.
            if (ancientDefinition.nonTranscendent) {
                continue;
            }

            let ancient: IAncientViewModel = {
                id,
                name: AncientSuggestionsComponent.getShortName(ancientDefinition),
                ancientLevel: new Decimal(0),
                itemLevel: new Decimal(0),
                effectiveLevel: new Decimal(0),
                purchaseTime: 0,
            };

            this.ancients.push(ancient);
            this.ancientsByName[ancient.name] = ancient;
        }

        this.sortAncients();
    }

    public static getShortName(entity: { name: string }): string {
        let commaIndex = entity.name.indexOf(",");
        return commaIndex >= 0
            ? entity.name.substring(0, commaIndex)
            : entity.name;
    }

    public ngOnInit(): void {
        this.settingsService
            .settings()
            .subscribe(settings => {
                this.settings = settings;
                this.hydrateAncientSuggestions();
            });
    }

    public openAutolevelModal(modal: {}): void {
        this.autoLeveledSavedGame = this.savedGame.clone();

        function formatForSavedGameData(num: Decimal): string {
            return num.toExponential().replace("+", "");
        }

        for (let i = 0; i < this.ancients.length; i++) {
            let ancientViewModel = this.ancients[i];
            if (ancientViewModel.suggestedLevel) {
                let ancient = this.autoLeveledSavedGame.data.ancients.ancients[ancientViewModel.id.toString()];
                ancient.level = formatForSavedGameData(ancientViewModel.suggestedLevel);
                ancient.spentHeroSouls = formatForSavedGameData(new Decimal(ancient.spentHeroSouls).plus(ancientViewModel.costToLevel));
            }
        }

        this.autoLeveledSavedGame.data.heroSouls = formatForSavedGameData(this.remainingSouls);
        this.autoLeveledSavedGame.updateContent();

        this.autolevelModal = this.modalService.open(modal);
        this.appInsights.trackEvent("Autolevel");
    }

    public saveAutolevel(): void {
        this.modalErrorMessage = null;
        this.isModalLoading = true;
        this.uploadService.create(this.autoLeveledSavedGame.content, true, this.playStyle)
            .then(uploadId => {
                this.appInsights.trackEvent("SaveAutolevel");
                return this.router.navigate(["/uploads", uploadId]);
            })
            .then(() => {
                this.autolevelModal.close();
                this.isModalLoading = false;
            })
            .catch((error: HttpErrorResponse) => {
                this.modalErrorMessage = error.status >= 400 && error.status < 500
                    ? "The uploaded save was not valid"
                    : "An unknown error occurred";
            });
    }

    // tslint:disable-next-line:cyclomatic-complexity
    private handleSavedGame(): void {
        if (!this.savedGame) {
            return;
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

        if (this.savedGame.data.ancients && this.savedGame.data.ancients.ancients) {
            for (let i = 0; i < this.ancients.length; i++) {
                let ancient = this.ancients[i];
                let ancientData = this.savedGame.data.ancients.ancients[ancient.id];
                if (ancientData) {
                    ancient.ancientLevel = new Decimal(ancientData.level || 0);
                    ancient.itemLevel = itemLevels[ancient.id] || new Decimal(0);
                    ancient.effectiveLevel = ancient.ancientLevel.plus(ancient.itemLevel).floor();
                    ancient.purchaseTime = ancientData.purchaseTime;
                }
            }
        }

        this.pendingSouls = new Decimal(this.savedGame.data.primalSouls || 0);
        this.heroSouls = new Decimal(this.savedGame.data.heroSouls || 0);
        this.highestZoneThisTranscension = new Decimal(this.savedGame.data.highestFinishedZonePersist || 0);
        this.totalAncientSouls = new Decimal(this.savedGame.data.ancientSoulsTotal || 0);
        this.transcendentPower = this.savedGame.data.transcendent
            ? new Decimal((2 + (23 * (1 - Math.pow(Math.E, -0.0003 * this.totalAncientSouls.toNumber())))) / 100)
            : new Decimal(0);
        this.autoclickers = new Decimal(this.savedGame.data.autoclickers || 0)
            .plus(this.savedGame.data.dlcAutoclickers || 0);

        // Ancient cost discount multiplier
        // TODO: Use Outsiders.ancientCostModifier
        const chorgorloth = this.savedGame.data.outsiders && this.savedGame.data.outsiders.outsiders["2"];
        const chorgorlothLevel = chorgorloth ? chorgorloth.level : 0;
        this.ancientCostMultiplier = Decimal.pow(0.95, chorgorlothLevel);

        this.hydrateAncientSuggestions();

        this.sortAncients();
    }

    // tslint:disable-next-line:cyclomatic-complexity
    private hydrateAncientSuggestions(): void {
        // Only render when we have both
        if (!this.settings || !this.savedGame) {
            return;
        }

        // Clear out existing suggestions
        for (let i = 0; i < this.ancients.length; i++) {
            let ancient = this.ancients[i];
            ancient.isBase = false;
            ancient.suggestedLevel = undefined;
            ancient.diffValue = undefined;
            ancient.diffCopyValue = undefined;
            ancient.costToLevel = undefined;
        }

        let startTime = Date.now();

        const isHybridRatioActiveFocused = this.settings.hybridRatio < 1;
        const baseAncient = this.playStyle === "active" || (this.playStyle === "hybrid" && isHybridRatioActiveFocused)
            ? "Fragsworth"
            : "Siyalatas";

        let suggestedLevels: { [key: string]: Decimal };

        if (this.suggestionType === "AvailableSouls") {
            this.availableSouls = this.heroSouls;
            if (this.useSoulsFromAscension) {
                this.availableSouls = this.availableSouls.plus(this.pendingSouls);
            }

            let baseLevel = this.getAncientLevel(baseAncient);
            let left = baseLevel.times(-1);
            let right: Decimal;
            let mid: Decimal;
            if (this.availableSouls.greaterThan(0)) {
                /*
                  If all hs were to be spent on Siya (or Frags), we would have the following cost equation,
                  where bf and bi are the final and current level of Siya (or Frags) respectively:
                  (1/2 bf^2 - 1/2 bi^2) * multiplier = hs. Solve for bf and you get the following equation:
                */
                right = this.availableSouls.dividedBy(this.ancientCostMultiplier).times(2).plus(baseLevel.pow(2)).sqrt().ceil();
            } else {
                right = new Decimal(0);
            }

            let spentHS: Decimal;

            /*
              Iterate until we have converged, or until we are very close to convergence.
              Converging exactly has run-time complexity in O(log(hs)), which, though sub-
              polynomial in hs, is still very slow (as hs is basically exponential
              in play-time). As such, we'll make do with an approximation.
            */
            let initialDiff = right.minus(left);
            while (right.minus(left).greaterThan(1) && right.minus(left).dividedBy(initialDiff).greaterThan(1e-5)) {
                if (spentHS === undefined) {
                    mid = right.plus(left).dividedBy(2).floor();
                } else {
                    let fitIndicator = spentHS.dividedBy(this.availableSouls).ln();
                    let interval = right.minus(left);

                    // If the (log of) the number of the percentage of spent hero souls is very large or very small, place the new search point off-center.
                    if (fitIndicator.lessThan(-0.1)) {
                        mid = left.plus(interval.dividedBy(1.25)).floor();
                    } else if (fitIndicator.greaterThan(0.1)) {
                        mid = left.plus(interval.dividedBy(4)).floor();
                    } else {
                        mid = right.plus(left).dividedBy(2).floor();
                    }
                }

                // Level according to RoT and calculate new cost
                const newSuggestedLevels = this.calculateAncientSuggestions(baseLevel.plus(mid));
                spentHS = this.getTotalAncientCost(newSuggestedLevels);
                if (spentHS.lessThan(this.availableSouls)) {
                    left = mid;
                } else {
                    right = mid;
                }
            }

            suggestedLevels = this.calculateAncientSuggestions(baseLevel.plus(left));

            // Ensure we don't suggest removing levels
            for (let ancient in suggestedLevels) {
                suggestedLevels[ancient] = Decimal.max(suggestedLevels[ancient], this.getAncientLevel(ancient));
            }
        } else {
            suggestedLevels = this.calculateAncientSuggestions();
            this.ancientsByName[baseAncient].isBase = true;
        }

        this.spentSouls = new Decimal(0);
        let ancientCosts = this.getAncientCosts(suggestedLevels);

        for (let ancientName in suggestedLevels) {
            let suggestedLevel = suggestedLevels[ancientName];
            let ancient = this.ancientsByName[ancientName];
            let cost = ancientCosts[ancientName];
            this.spentSouls = this.spentSouls.minus(cost);
            if (ancient) {
                ancient.suggestedLevel = suggestedLevel;
                ancient.diffValue = ancient.suggestedLevel.minus(this.getAncientLevel(ancientName));
                ancient.diffCopyValue = this.formatForClipboard(ancient.diffValue);
                ancient.costToLevel = cost;
            }
        }

        this.remainingSouls = this.availableSouls.plus(this.spentSouls);

        this.appInsights.trackMetric(
            "AncientSuggestions",
            Date.now() - startTime,
            null,
            null,
            null,
            {
                suggestionType: this.suggestionType,
                useSoulsFromAscension: this.useSoulsFromAscension.toString(),
            });
    }

    private calculateAncientSuggestions(currentPrimaryAncientLevel?: Decimal): { [key: string]: Decimal } {
        const suggestedLevels: { [key: string]: Decimal } = {};

        const isHybridRatioActiveFocused = this.settings.hybridRatio < 1;
        const hybridRatio = isHybridRatioActiveFocused
            ? this.settings.hybridRatio
            : 1 / this.settings.hybridRatio;

        const primaryAncient = this.playStyle === "active" || (this.playStyle === "hybrid" && isHybridRatioActiveFocused)
            ? "Fragsworth"
            : "Siyalatas";

        if (currentPrimaryAncientLevel === undefined) {
            // Use the current level, but don't use it in the suggestions.
            currentPrimaryAncientLevel = this.getAncientLevel(primaryAncient);
        } else {
            // When provided, add it to the suggestions
            suggestedLevels[primaryAncient] = currentPrimaryAncientLevel;
        }

        const currentBubosLevel = this.getAncientLevel("Bubos");
        const currentChronosLevel = this.getAncientLevel("Chronos");
        const currentDoraLevel = this.getAncientLevel("Dora");
        const currentDogcogLevel = this.getAncientLevel("Dogcog");
        const currentFortunaLevel = this.getAncientLevel("Fortuna");
        const currentAtmanLevel = this.getAncientLevel("Atman");
        const currentKumaLevel = this.getAncientLevel("Kumawakamaru");

        const lnPrimary = currentPrimaryAncientLevel.ln();
        const hpScale = this.highestZoneThisTranscension.dividedBy(500).floor().times(0.005).plus(1.145);
        const alpha = this.transcendentPower.isZero() ? new Decimal(0) : this.transcendentPower.plus(1).ln().times(1.4067).dividedBy(hpScale.ln());
        const lnAlpha = this.transcendentPower.isZero() ? new Decimal(0) : alpha.ln();

        // Common formulas across play styles
        suggestedLevels.Argaiv = currentPrimaryAncientLevel;
        suggestedLevels.Atman = lnPrimary.times(2.832).minus(lnAlpha.times(1.416)).minus(new Decimal(4).div(3).minus(currentAtmanLevel.times(-0.013).exp()).ln().times(1.416)).minus(6.613);
        suggestedLevels.Bubos = lnPrimary.times(2.8).minus(new Decimal(1).plus(currentBubosLevel.times(-0.02).exp()).ln().times(1.4)).minus(5.94);
        suggestedLevels.Chronos = lnPrimary.times(2.75).minus(new Decimal(2).minus(currentChronosLevel.times(-0.034).exp()).ln().times(1.375)).minus(5.1);
        suggestedLevels.Dogcog = lnPrimary.times(2.844).minus(new Decimal(1).div(99).plus(currentDogcogLevel.times(-0.01).exp()).ln().times(1.422)).minus(7.232);
        suggestedLevels.Dora = lnPrimary.times(2.877).minus(new Decimal(100).div(99).minus(currentDoraLevel.times(-0.002).exp()).ln().times(1.4365)).minus(9.63);
        suggestedLevels.Fortuna = lnPrimary.times(2.875).minus(new Decimal(10).div(9).minus(currentFortunaLevel.times(-0.0025).exp()).ln().times(1.4375)).minus(9.3);
        suggestedLevels.Kumawakamaru = lnPrimary.times(2.844).minus(lnAlpha.times(1.422)).minus(new Decimal(1).div(4).plus(currentKumaLevel.times(-0.01).exp()).ln().times(1.422)).minus(7.014);
        suggestedLevels.Mammon = suggestedLevels.Mimzee = currentPrimaryAncientLevel.times(0.926);
        suggestedLevels.Morgulis = currentPrimaryAncientLevel.pow(2);

        // Math per play style
        switch (this.playStyle) {
            case "active":
                suggestedLevels.Bhaal = currentPrimaryAncientLevel;
                suggestedLevels.Juggernaut = currentPrimaryAncientLevel.pow(0.8);
                suggestedLevels.Pluto = suggestedLevels.Mammon;
                break;
            case "idle":
                suggestedLevels.Libertas = suggestedLevels.Mammon;
                suggestedLevels.Nogardnit = this.autoclickers.isZero() ? new Decimal(0) : currentPrimaryAncientLevel.pow(0.8);
                break;
            case "hybrid":
                if (isHybridRatioActiveFocused) {
                    // Active-focused
                    suggestedLevels.Bhaal = currentPrimaryAncientLevel;
                    suggestedLevels.Juggernaut = currentPrimaryAncientLevel.pow(0.8);
                    suggestedLevels.Pluto = suggestedLevels.Mammon;

                    suggestedLevels.Siyalatas = currentPrimaryAncientLevel.times(hybridRatio);
                    suggestedLevels.Libertas = suggestedLevels.Mammon.times(hybridRatio);
                    suggestedLevels.Nogardnit = this.autoclickers.isZero() ? new Decimal(0) : suggestedLevels.Siyalatas.pow(0.8);
                } else {
                    // Idle-focused
                    suggestedLevels.Libertas = suggestedLevels.Mammon;
                    suggestedLevels.Nogardnit = this.autoclickers.isZero() ? new Decimal(0) : currentPrimaryAncientLevel.pow(0.8);

                    suggestedLevels.Fragsworth = currentPrimaryAncientLevel.times(hybridRatio);
                    suggestedLevels.Bhaal = suggestedLevels.Fragsworth;
                    suggestedLevels.Juggernaut = suggestedLevels.Fragsworth.pow(0.8);
                    suggestedLevels.Pluto = suggestedLevels.Mammon.times(hybridRatio);
                }

                break;
        }

        // Skill ancients
        if (this.settings.shouldLevelSkillAncients) {
            let skillAncientBaseAncient = gameData.ancients[this.settings.skillAncientBaseAncient];
            let skillAncientBaseAncientShortName = AncientSuggestionsComponent.getShortName(skillAncientBaseAncient);
            let skillAncientBaseAncientLevel = suggestedLevels[skillAncientBaseAncientShortName];
            let suggestedSkillAncientLevel = skillAncientBaseAncientLevel.plus(this.settings.skillAncientLevelDiff);

            suggestedLevels.Berserker = suggestedSkillAncientLevel;
            suggestedLevels.Chawedo = suggestedSkillAncientLevel;
            suggestedLevels.Energon = suggestedSkillAncientLevel;
            suggestedLevels.Hecatoncheir = suggestedSkillAncientLevel;
            suggestedLevels.Kleptos = suggestedSkillAncientLevel;
            suggestedLevels.Revolc = suggestedSkillAncientLevel;
            suggestedLevels.Sniperino = suggestedSkillAncientLevel;
            suggestedLevels.Vaagur = suggestedSkillAncientLevel;
        }

        // Handle ancients with caps
        for (let ancientName in AncientSuggestionsComponent.ancientLevelCaps) {
            if (suggestedLevels[ancientName]) {
                let maxLevel = AncientSuggestionsComponent.ancientLevelCaps[ancientName];
                suggestedLevels[ancientName] = Decimal.min(maxLevel, suggestedLevels[ancientName]);
            }
        }

        // Normalize the values
        for (let ancient in suggestedLevels) {
            suggestedLevels[ancient] = Decimal.max(suggestedLevels[ancient].ceil(), new Decimal(0));

            // The game only lets you buy 4 exponents less than what you currently have, so clip any suggestions less than that.
            const minimumLevelingThreshold = -4;
            if (suggestedLevels[ancient].minus(this.ancientsByName[ancient].ancientLevel).log().floor().minus(this.ancientsByName[ancient].ancientLevel.log().floor()).lessThan(minimumLevelingThreshold)) {
                suggestedLevels[ancient] = this.ancientsByName[ancient].ancientLevel;
            }
        }

        return suggestedLevels;
    }

    private formatForClipboard(num: Decimal): string {
        // The game can't handle pasting in decimal points, so we'll just use an altered sci-not form that excludes the decimal (eg. 1.234e5 => 1234e2)
        if (num.greaterThanOrEqualTo(1e6)) {
            let str = num.toExponential();
            let groups = AncientSuggestionsComponent.exponentialRegex.exec(str);
            let n = parseFloat(groups[1]);
            let exponent = parseInt(groups[3]);

            n *= 1e5;
            n = Math.floor(n);
            exponent -= 5;

            return exponent === 0
                ? n.toFixed()
                : (n.toFixed() + "e" + exponent);
        } else {
            return num.toFixed(0);
        }
    }

    private getAncientLevel(ancientName: string): Decimal {
        let ancient = this.ancientsByName[ancientName];
        return ancient
            ? ancient.ancientLevel
            : new Decimal(0);
    }

    private getAncientCosts(suggestedLevels: { [key: string]: Decimal }): { [key: string]: Decimal } {
        let costs: { [key: string]: Decimal } = {};
        for (let ancient in suggestedLevels) {
            const suggestedLevel = suggestedLevels[ancient];
            const currentLevel = this.getAncientLevel(ancient);

            // If the ancient is over-leveled, no cost
            if (suggestedLevel.lessThan(currentLevel)) {
                costs[ancient] = new Decimal(0);
                continue;
            }

            const costFormula = this.ancientCostFormulas[ancient];
            if (!costFormula) {
                costs[ancient] = new Decimal(0);
                continue;
            }

            costs[ancient] = costFormula(suggestedLevel).minus(costFormula(currentLevel)).times(this.ancientCostMultiplier).ceil();
        }

        return costs;
    }

    private getTotalAncientCost(suggestedLevels: { [key: string]: Decimal }): Decimal {
        let costs = this.getAncientCosts(suggestedLevels);

        let totalCost = new Decimal(0);
        for (let ancient in costs) {
            totalCost = totalCost.plus(costs[ancient]);
        }

        return totalCost;
    }

    private getAncientCostFormulas(): { [key: string]: (level: Decimal) => Decimal } {
        const ancientCosts: { [key: string]: (level: Decimal) => Decimal } = {};

        for (const ancientId in gameData.ancients) {
            const ancient = gameData.ancients[ancientId];

            let ancientCost: (level: Decimal) => Decimal;
            switch (ancient.levelCostFormula) {
                case "one":
                    ancientCost = (n: Decimal) => n;
                    break;
                case "linear":
                    ancientCost = (n: Decimal) => n.times(n.plus(1)).dividedBy(2);
                    break;
                case "polynomial1_5":
                    ancientCost = (n: Decimal) => {
                        // Approximate above a certain level for perf
                        // Formula taken from https://github.com/superbob/clicker-heroes-1.0-hsoptimizer/blob/335f13b7304627065a4e515edeb3fb3c4e08f8ad/src/app/components/maths/maths.service.js
                        if (n.greaterThan(100)) {
                            return new Decimal(2).div(5).times(n.pow(new Decimal(5).div(2)))
                                .plus(new Decimal(1).div(2).times(n.pow(new Decimal(3).div(2))))
                                .plus(new Decimal(1).div(8).times(n.pow(new Decimal(1).div(2))))
                                .plus(new Decimal(1).div(1920).times(n.pow(new Decimal(-3).div(2)))).ceil();

                        }

                        let num = n.toNumber();
                        let cost = new Decimal(0);
                        for (let i = 1; i <= num; i++) {
                            cost = cost.plus(Decimal.pow(i, 1.5));
                        }

                        return cost.ceil();
                    };
                    break;
                case "exponential":
                    ancientCost = (n: Decimal) => Decimal.pow(2, n.plus(1)).minus(1);
                    break;
                default:
                    ancientCost = () => new Decimal(0);
            }

            let ancientShortName = AncientSuggestionsComponent.getShortName(ancient);
            ancientCosts[ancientShortName] = ancientCost;
        }

        return ancientCosts;
    }

    private sortAncients(): void {
        // For mobile, sort by purchase time, then by name.
        if (this.savedGame && this.savedGame.data && this.savedGame.data.saveOrigin === "mobile") {
            this.ancients = this.ancients.sort((a, b) => {
                // If both are not purchased, sort by name (at the bottom)
                if (a.purchaseTime === 0 && b.purchaseTime === 0) {
                    return a.name < b.name ? -1 : 1;
                }

                // Bring unpurchased to the bottom
                if (a.purchaseTime === 0) {
                    return 1;
                }

                if (b.purchaseTime === 0) {
                    return -1;
                }

                // Sort by date
                let timeDiff = a.purchaseTime - b.purchaseTime;
                if (timeDiff !== 0) {
                    return timeDiff;
                }

                // Finally, sort by id
                return parseInt(a.id) - parseInt(b.id);
            });
        } else {
            // Sort by name
            this.ancients = this.ancients.sort((a, b) => a.name < b.name ? -1 : 1);
        }
    }
}
