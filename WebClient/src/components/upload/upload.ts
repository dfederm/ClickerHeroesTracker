import { Component, OnInit } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { UploadService } from "../../services/uploadService/uploadService";
import { AuthenticationService, IUserInfo } from "../../services/authenticationService/authenticationService";
import Decimal from "decimal.js";
import { AppInsightsService } from "@markpieszak/ng-application-insights";
import { NgbModal } from "@ng-bootstrap/ng-bootstrap";
import { switchMap } from "rxjs/operators";

import { SettingsService, IUserSettings } from "../../services/settingsService/settingsService";
import { IUpload } from "../../models";

// tslint:disable-next-line:no-require-imports no-var-requires
const gameData: IGameData = require("../../../../Website/src/wwwroot/data/GameData.json");

interface IGameData {
    ancients: { [id: string]: { name: string, nonTranscendent: boolean, levelCostFormula: string } };
    outsiders: { [id: string]: { id: number, name: string } };
}

interface IAncientViewModel {
    name: string;
    ancientLevel: decimal.Decimal;
    itemLevel: decimal.Decimal;
    effectiveLevel: decimal.Decimal;
    suggestedLevel?: decimal.Decimal;
    diffValue?: decimal.Decimal;
    diffCopyValue?: string;
    isBase?: boolean;
}

interface IOutsiderViewModel {
    id: number;
    name: string;
    currentLevel: decimal.Decimal;
}

@Component({
    selector: "upload",
    templateUrl: "./upload.html",
    styleUrls: ["./upload.css"],
})
export class UploadComponent implements OnInit {
    private static exponentialRegex = /^(\d+(\.\d+)?)e\+?(\d+)$/i;

    public userInfo: IUserInfo;
    public errorMessage: string;
    public isLoading: boolean;

    public userId: string;
    public userName: string;
    public uploadTime: string;
    public playStyle: string;
    public uploadContent: string;

    public ancients: IAncientViewModel[] = [];
    public outsiders: IOutsiderViewModel[] = [];

    public pendingSouls: decimal.Decimal = new Decimal(0);
    public heroSoulsSpent: decimal.Decimal = new Decimal(0);
    public heroSoulsSacrificed: decimal.Decimal = new Decimal(0);
    public totalAncientSouls: decimal.Decimal = new Decimal(0);
    public transcendentPower: decimal.Decimal = new Decimal(0);
    public titanDamage: decimal.Decimal = new Decimal(0);
    public highestZoneThisTranscension: decimal.Decimal = new Decimal(0);
    public highestZoneLifetime: decimal.Decimal = new Decimal(0);
    public ascensionsThisTranscension: decimal.Decimal = new Decimal(0);
    public ascensionsLifetime: decimal.Decimal = new Decimal(0);
    public rubies: decimal.Decimal = new Decimal(0);

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

    // An index for quick lookup of ancient cost formulas.
    // Each formula gets the sum of the cost of the ancient from 1 to N.
    private ancientCostFormulas = this.getAncientCostFormulas();

    private _suggestionType = "AvailableSouls";
    private _useSoulsFromAscension = true;

    private ancientsByName: { [name: string]: IAncientViewModel } = {};
    private outsidersByName: { [name: string]: IOutsiderViewModel } = {};

    private uploadId: number;
    private heroSouls: decimal.Decimal;
    private ancientCostMultiplier: decimal.Decimal;

    private upload: IUpload;
    private settings: IUserSettings;

    constructor(
        private authenticationService: AuthenticationService,
        private route: ActivatedRoute,
        private router: Router,
        private uploadService: UploadService,
        private settingsService: SettingsService,
        private appInsights: AppInsightsService,
        private modalService: NgbModal,
    ) {
        for (const id in gameData.ancients) {
            const ancientDefinition = gameData.ancients[id];

            // Skip ancients no longer in the game.
            if (ancientDefinition.nonTranscendent) {
                continue;
            }

            let ancient: IAncientViewModel = {
                name: this.getAncientShortName(ancientDefinition),
                ancientLevel: new Decimal(0),
                itemLevel: new Decimal(0),
                effectiveLevel: new Decimal(0),
            };

            this.ancients.push(ancient);
            this.ancientsByName[ancient.name] = ancient;
        }

        this.ancients = this.ancients.sort((a, b) => a.name < b.name ? -1 : 1);

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
            };

            this.outsiders.push(outsider);
            this.outsidersByName[outsider.name] = outsider;
        }

        this.outsiders = this.outsiders.sort((a, b) => a.id - b.id);
    }

    public ngOnInit(): void {
        this.authenticationService
            .userInfo()
            .subscribe(userInfo => this.userInfo = userInfo);

        this.settingsService
            .settings()
            .subscribe(settings => this.handleSettings(settings));

        this.route.params.pipe(
            switchMap(params => {
                this.isLoading = true;
                return this.uploadService.get(+params.id);
            }),
        ).subscribe(upload => this.handleUpload(upload), () => this.handleError("There was a problem getting that upload"));
    }

    public openModal(modal: {}): void {
        this.errorMessage = null;
        this.modalService
            .open(modal)
            .result
            .then(() => {
                // Noop on close as the modal is expected to handle its own stuff.
            })
            .catch(() => {
                // Noop on dismissal
            });
    }

    public deleteUpload(closeModal: () => void): void {
        this.isLoading = true;
        this.uploadService.delete(this.uploadId)
            .then(() => {
                this.isLoading = false;
                this.router.navigate([`/users/${this.userName}`]);
            })
            .catch(() => this.handleError("There was a problem deleting that upload"))
            .then(closeModal);
    }

    private static normalizeName(name: string): string {
        return name.replace(/[^\w]/gi, "");
    }

    private handleSettings(settings: IUserSettings): void {
        this.settings = settings;
        this.refresh();
    }

    private handleUpload(upload: IUpload): void {
        this.upload = upload;
        this.refresh();
        this.isLoading = false;
    }

    // tslint:disable-next-line:cyclomatic-complexity
    private refresh(): void {
        // Only render when we have both
        if (!this.settings || !this.upload) {
            return;
        }

        this.errorMessage = null;
        this.uploadId = this.upload.id;

        if (this.upload.user) {
            this.userId = this.upload.user.id;
            this.userName = this.upload.user.name;
        } else {
            this.userId = null;
            this.userName = null;
        }

        this.uploadTime = this.upload.timeSubmitted;
        this.playStyle = this.upload.playStyle;
        this.uploadContent = this.upload.uploadContent;

        let stats: { [key: string]: decimal.Decimal } = {};
        if (this.upload.stats) {
            for (let statType in this.upload.stats) {
                stats[statType] = new Decimal(this.upload.stats[statType]);
            }
        }

        for (let i = 0; i < this.ancients.length; i++) {
            let ancient = this.ancients[i];
            let ancientName = UploadComponent.normalizeName(ancient.name);
            ancient.ancientLevel = stats["ancient" + ancientName] || new Decimal(0);
            ancient.itemLevel = stats["item" + ancientName] || new Decimal(0);
            ancient.effectiveLevel = ancient.ancientLevel.plus(ancient.itemLevel).floor();
        }

        for (let i = 0; i < this.outsiders.length; i++) {
            let outsider = this.outsiders[i];
            let outsiderName = UploadComponent.normalizeName(outsider.name);
            outsider.currentLevel = stats["outsider" + outsiderName] || new Decimal(0);
        }

        this.pendingSouls = stats.pendingSouls || new Decimal(0);
        this.heroSouls = stats.heroSouls || new Decimal(0);
        this.heroSoulsSpent = stats.heroSoulsSpent || new Decimal(0);
        this.heroSoulsSacrificed = stats.heroSoulsSacrificed || new Decimal(0);
        this.totalAncientSouls = stats.totalAncientSouls || new Decimal(0);
        this.transcendentPower = stats.transcendentPower || new Decimal(0);
        this.titanDamage = stats.titanDamage || new Decimal(0);
        this.highestZoneThisTranscension = stats.highestZoneThisTranscension || new Decimal(0);
        this.highestZoneLifetime = stats.highestZoneLifetime || new Decimal(0);
        this.ascensionsThisTranscension = stats.ascensionsThisTranscension || new Decimal(0);
        this.ascensionsLifetime = stats.ascensionsLifetime || new Decimal(0);
        this.rubies = stats.rubies || new Decimal(0);

        // Ancient cost discount multiplier
        const chorgorloth = this.outsidersByName["Chor'gorloth"];
        const chorgorlothLevel = chorgorloth ? chorgorloth.currentLevel : 0;
        this.ancientCostMultiplier = Decimal.pow(0.95, chorgorlothLevel);

        this.hydrateAncientSuggestions();
    }

    private handleError(errorMessage: string): void {
        this.isLoading = false;
        this.errorMessage = errorMessage;
    }

    private formatForClipboard(num: decimal.Decimal): string {
        // The game can't handle pasting in decimal points, so we'll just use an altered sci-not form that excludes the decimal (eg. 1.234e5 => 1234e2)
        if (num.greaterThanOrEqualTo(1e6)) {
            let str = num.toExponential();
            let groups = UploadComponent.exponentialRegex.exec(str);
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

    private hydrateAncientSuggestions(): void {
        const availableSoulsSuggestionsLatency = "AncientSuggestions";
        this.appInsights.startTrackEvent(availableSoulsSuggestionsLatency);

        const baseAncient = this.playStyle === "active"
            ? "Fragsworth"
            : "Siyalatas";

        let suggestedLevels: { [key: string]: decimal.Decimal };

        if (this.suggestionType === "AvailableSouls") {
            let availableSouls = this.heroSouls;
            if (this.useSoulsFromAscension) {
                availableSouls = availableSouls.plus(this.pendingSouls);
            }

            let baseLevel = this.getAncientLevel(baseAncient);
            let left = baseLevel.times(-1);
            let right: decimal.Decimal;
            let mid: decimal.Decimal;
            if (availableSouls.greaterThan(0)) {
                /*
                  If all hs were to be spent on Siya (or Frags), we would have the following cost equation,
                  where bf and bi are the final and current level of Siya (or Frags) respectively:
                  (1/2 bf^2 - 1/2 bi^2) * multiplier = hs. Solve for bf and you get the following equation:
                */
                right = availableSouls.dividedBy(this.ancientCostMultiplier).times(2).plus(baseLevel.pow(2)).sqrt().ceil();
            } else {
                right = new Decimal(0);
            }

            let spentHS: decimal.Decimal;

            /*
              Iterate until we have converged, or until we are very close to convergence.
              Converging exactly has run-time complexity in O(log(hs)), which, though sub-
              polynomial in hs, is still very slow (as hs is basically exponential
              in play-time). As such, we'll make do with an approximation.
            */
            let initialDiff = right.minus(left);
            while (right.minus(left).greaterThan(1) && right.minus(left).dividedBy(initialDiff).greaterThan(0.00001)) {
                if (spentHS === undefined) {
                    mid = right.plus(left).dividedBy(2).floor();
                } else {
                    let fitIndicator = spentHS.dividedBy(availableSouls).ln();
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
                if (spentHS.lessThan(availableSouls)) {
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

        for (let ancientName in suggestedLevels) {
            let suggestedLevel = suggestedLevels[ancientName];
            let ancient = this.ancientsByName[ancientName];
            if (ancient) {
                ancient.suggestedLevel = suggestedLevel;
                ancient.diffValue = ancient.suggestedLevel.minus(this.getAncientLevel(ancientName));
                ancient.diffCopyValue = this.formatForClipboard(ancient.diffValue);
            }
        }

        this.appInsights.stopTrackEvent(
            availableSoulsSuggestionsLatency,
            {
                suggestionType: this.suggestionType,
                useSoulsFromAscension: this.useSoulsFromAscension.toString(),
            });
    }

    private calculateAncientSuggestions(currentPrimaryAncientLevel?: decimal.Decimal): { [key: string]: decimal.Decimal } {
        const suggestedLevels: { [key: string]: decimal.Decimal } = {};

        const primaryAncient = this.playStyle === "active" ? "Fragsworth" : "Siyalatas";
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
        suggestedLevels.Fortuna = lnPrimary.times(2.875).minus(Decimal(10).div(9).minus(currentFortunaLevel.times(-0.0025).exp()).ln().times(1.4375)).minus(9.3);
        suggestedLevels.Kumawakamaru = lnPrimary.times(2.844).minus(lnAlpha.times(1.422)).minus(new Decimal(1).div(4).plus(currentKumaLevel.times(-0.01).exp()).ln().times(1.422)).minus(7.014);
        suggestedLevels.Mammon = suggestedLevels.Mimzee = currentPrimaryAncientLevel.times(0.926);
        suggestedLevels.Morgulis = currentPrimaryAncientLevel.pow(2);

        // Math per play style
        switch (this.playStyle) {
            case "idle":
                suggestedLevels.Libertas = suggestedLevels.Mammon;
                suggestedLevels.Nogardnit = suggestedLevels.Libertas.pow(0.8);
                break;
            case "hybrid":
                const hybridRatioReciprocal = 1 / this.settings.hybridRatio;
                suggestedLevels.Bhaal = suggestedLevels.Fragsworth = currentPrimaryAncientLevel.times(hybridRatioReciprocal);
                suggestedLevels.Juggernaut = suggestedLevels.Fragsworth.pow(0.8);
                suggestedLevels.Libertas = suggestedLevels.Mammon;
                suggestedLevels.Nogardnit = suggestedLevels.Libertas.pow(0.8);
                break;
            case "active":
                suggestedLevels.Bhaal = currentPrimaryAncientLevel;
                suggestedLevels.Juggernaut = currentPrimaryAncientLevel.pow(0.8);
                break;
        }

        // Skill ancients
        if (this.settings.shouldLevelSkillAncients) {
            let skillAncientBaseAncient = gameData.ancients[this.settings.skillAncientBaseAncient];
            let skillAncientBaseAncientShortName = this.getAncientShortName(skillAncientBaseAncient);
            let skillAncientBaseAncientLevel = suggestedLevels[skillAncientBaseAncientShortName];
            let suggestedSkillAncientLevel = skillAncientBaseAncientLevel.plus(this.settings.skillAncientLevelDiff);

            // These two have effective caps where additional levels add less than floating point numbers can handle.
            suggestedLevels.Vaagur = Decimal.min(1440, suggestedSkillAncientLevel);
            suggestedLevels.Revolc = Decimal.min(3743, suggestedSkillAncientLevel);

            suggestedLevels.Chawedo = suggestedSkillAncientLevel;
            suggestedLevels.Hecatoncheir = suggestedSkillAncientLevel;
            suggestedLevels.Berserker = suggestedSkillAncientLevel;
            suggestedLevels.Sniperino = suggestedSkillAncientLevel;
            suggestedLevels.Kleptos = suggestedSkillAncientLevel;
            suggestedLevels.Energon = suggestedSkillAncientLevel;
        }

        // Normalize the values
        for (let ancient in suggestedLevels) {
            suggestedLevels[ancient] = Decimal.max(suggestedLevels[ancient].ceil(), new Decimal(0));
        }

        return suggestedLevels;
    }

    private getAncientLevel(ancientName: string): decimal.Decimal {
        let ancient = this.ancientsByName[ancientName];
        return ancient
            ? this.settings.useEffectiveLevelForSuggestions
                ? ancient.effectiveLevel
                : ancient.ancientLevel
            : new Decimal(0);
    }

    private getTotalAncientCost(suggestedLevels: { [key: string]: decimal.Decimal }): decimal.Decimal {
        let cost = new Decimal(0);
        for (let ancient in suggestedLevels) {
            const suggestedLevel = suggestedLevels[ancient];
            const currentLevel = this.getAncientLevel(ancient);

            // If the ancient is over-leveled, no cost
            if (suggestedLevel.lessThan(currentLevel)) {
                continue;
            }

            const costFormula = this.ancientCostFormulas[ancient];
            if (!costFormula) {
                continue;
            }

            cost = cost.plus((costFormula(suggestedLevel).minus(costFormula(currentLevel))).times(this.ancientCostMultiplier).ceil());
        }

        return cost;
    }

    private getAncientCostFormulas(): { [key: string]: (level: decimal.Decimal) => decimal.Decimal } {
        const ancientCosts: { [key: string]: (level: decimal.Decimal) => decimal.Decimal } = {};

        for (const ancientId in gameData.ancients) {
            const ancient = gameData.ancients[ancientId];

            let ancientCost: (level: decimal.Decimal) => decimal.Decimal;
            switch (ancient.levelCostFormula) {
                case "one":
                    ancientCost = (n: decimal.Decimal) => n;
                    break;
                case "linear":
                    ancientCost = (n: decimal.Decimal) => n.times(n.plus(1)).dividedBy(2);
                    break;
                case "polynomial1_5":
                    ancientCost = (n: decimal.Decimal) => {
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
                    ancientCost = (n: decimal.Decimal) => Decimal.pow(2, n.plus(1)).minus(1);
                    break;
                default:
                    ancientCost = () => new Decimal(0);
            }

            let ancientShortName = this.getAncientShortName(ancient);
            ancientCosts[ancientShortName] = ancientCost;
        }

        return ancientCosts;
    }

    private getAncientShortName(ancient: { name: string }): string {
        let commaIndex = ancient.name.indexOf(",");
        return commaIndex >= 0
            ? ancient.name.substring(0, commaIndex)
            : ancient.name;
    }
}
