import { Component, OnInit } from "@angular/core";
import { ActivatedRoute, Router, Params } from "@angular/router";
import { UploadService, IUpload } from "../../services/uploadService/uploadService";
import { AuthenticationService } from "../../services/authenticationService/authenticationService";
import Decimal from "decimal.js";

import "rxjs/add/operator/switchMap";

// tslint:disable-next-line:no-require-imports no-var-requires
const gameData: IGameData = require("../../../../Website/src/wwwroot/data/GameData.json");

// tslint:disable-next-line:no-require-imports no-var-requires
const optimalOutsiderLevels: [number, number, number, number, number][] = require("../../../../Website/src/wwwroot/data/OptimalOutsiderLevels.json");

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
    suggestedLevel?: decimal.Decimal;
}

@Component({
    selector: "upload",
    templateUrl: "./upload.html",
    styleUrls: ["./upload.css"],
})
export class UploadComponent implements OnInit {
    private static exponentialRegex = /^(\d+(\.\d+)?)e\+?(\d+)$/i;

    public isLoggedIn: boolean;
    public errorMessage: string;

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
    public maxTranscendentPrimalReward: decimal.Decimal = new Decimal(0);
    public bossLevelToTranscendentPrimalCap: decimal.Decimal = new Decimal(0);
    public titanDamage: decimal.Decimal = new Decimal(0);
    public highestZoneThisTranscension: decimal.Decimal = new Decimal(0);
    public highestZoneLifetime: decimal.Decimal = new Decimal(0);
    public ascensionsThisTranscension: decimal.Decimal = new Decimal(0);
    public ascensionsLifetime: decimal.Decimal = new Decimal(0);
    public rubies: decimal.Decimal = new Decimal(0);

    public showLowAncientSoulWarning: boolean;
    public showMissingSimulationWarning: boolean;

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

    // TODO get the user's real settings
    private userSettings =
    {
        areUploadsPublic: true,
        hybridRatio: 1,
        logarithmicGraphScaleThreshold: 1000000,
        playStyle: "hybrid",
        scientificNotationThreshold: 100000,
        useEffectiveLevelForSuggestions: false,
        useExperimentalStats: true,
        useLogarithmicGraphScale: true,
        useReducedSolomonFormula: false,
        useScientificNotation: true,
    };

    private _suggestionType = "AvailableSouls";
    private _useSoulsFromAscension = true;

    private ancientsByName: { [name: string]: IAncientViewModel } = {};
    private outsidersByName: { [name: string]: IOutsiderViewModel } = {};

    private uploadId: number;
    private heroSouls: decimal.Decimal;
    private ancientCostMultiplier: decimal.Decimal;

    constructor(
        private authenticationService: AuthenticationService,
        private route: ActivatedRoute,
        private router: Router,
        private uploadService: UploadService,
    ) {
        for (const id in gameData.ancients) {
            const ancientDefinition = gameData.ancients[id];

            // Skip ancients no longer in the game.
            if (ancientDefinition.nonTranscendent) {
                continue;
            }

            let commaIndex = ancientDefinition.name.indexOf(",");
            let ancientShortName = commaIndex >= 0
                ? ancientDefinition.name.substring(0, commaIndex)
                : ancientDefinition.name;

            let ancient: IAncientViewModel = {
                name: ancientShortName,
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
            .isLoggedIn()
            .subscribe(isLoggedIn => this.isLoggedIn = isLoggedIn);

        this.route.params
            .switchMap((params: Params) => this.uploadService.get(+params.id))
            .subscribe(upload => this.handleData(upload), () => this.handleError("There was a problem getting that upload"));
    }

    public deleteUpload(): void {
        this.errorMessage = null;
        this.uploadService.delete(this.uploadId)
            .then(() => this.router.navigate(["/dashboard"]))
            .catch(() => this.handleError("There was a problem deleting that upload"));
    }

    private static normalizeName(name: string): string {
        return name.replace(/[^\w]/gi, "");
    }

    // tslint:disable-next-line:cyclomatic-complexity
    private handleData(upload: IUpload): void {
        this.errorMessage = null;
        this.uploadId = upload.id;

        this.userName = upload.user
            ? upload.user.name
            : null;

        this.uploadTime = upload.timeSubmitted;
        this.playStyle = upload.playStyle;
        this.uploadContent = upload.uploadContent;

        let stats: { [key: string]: decimal.Decimal } = {};
        if (upload.stats) {
            for (let statType in upload.stats) {
                stats[statType] = new Decimal(upload.stats[statType]);
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
        this.maxTranscendentPrimalReward = stats.maxTranscendentPrimalReward || new Decimal(0);
        this.bossLevelToTranscendentPrimalCap = stats.bossLevelToTranscendentPrimalCap || new Decimal(0);
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

        this.calculateOutsiderSuggestions();
    }

    private handleError(errorMessage: string): void {
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
        appInsights.startTrackEvent(availableSoulsSuggestionsLatency);

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

        appInsights.stopTrackEvent(
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
        suggestedLevels.Solomon = currentPrimaryAncientLevel.isZero()
            ? new Decimal(0)
            : this.transcendentPower.isZero()
                ? this.getPreTranscendentSuggestedSolomonLevel(currentPrimaryAncientLevel, this.playStyle)
                : currentPrimaryAncientLevel.pow(0.8).dividedBy(alpha.pow(0.4));

        // Math per play style
        switch (this.playStyle) {
            case "idle":
                suggestedLevels.Libertas = suggestedLevels.Mammon;
                suggestedLevels.Nogardnit = suggestedLevels.Libertas.pow(0.8);
                break;
            case "hybrid":
                const hybridRatioReciprocal = 1 / this.userSettings.hybridRatio;
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

        // Normalize the values
        for (let ancient in suggestedLevels) {
            suggestedLevels[ancient] = Decimal.max(suggestedLevels[ancient].ceil(), new Decimal(0));
        }

        return suggestedLevels;
    }

    private calculateOutsiderSuggestions(): void {
        let ancientSouls = this.totalAncientSouls;

        this.showLowAncientSoulWarning = false;
        this.showMissingSimulationWarning = false;

        let suggestedXyl = new Decimal(0);
        let suggestedChor = new Decimal(0);
        let suggestedPhan = new Decimal(0);
        let suggestedBorb = new Decimal(0);
        let suggestedPony = new Decimal(0);

        if (ancientSouls.isZero()) {
            // If the user has no ancient souls, all the suggestions should remain 0.
        } else if (ancientSouls.lessThan(30)) {
            // Less ancient souls than the simulation data supported. We can try to guess though.
            // Our guess just alternates leveling Xyl and Pony until Xylk hits 7 and then dump into Pony unti lit matches the 30 AS simulation data.
            this.showLowAncientSoulWarning = true;
            appInsights.trackEvent("LowAncientSouls", { ancientSouls: ancientSouls.toString() });
            if (ancientSouls.lessThan(14)) {
                suggestedXyl = ancientSouls.dividedBy(2).ceil();
                suggestedPony = ancientSouls.dividedBy(2).floor();
            } else {
                suggestedXyl = new Decimal(7);
                suggestedPony = ancientSouls.minus(7);
            }
        } else {
            if (ancientSouls.greaterThan(210)) {
                this.showMissingSimulationWarning = true;
                appInsights.trackEvent("MissingSimulationData", { ancientSouls: ancientSouls.toString() });
                if (ancientSouls.greaterThanOrEqualTo(1500)) {
                    ancientSouls = new Decimal(1500);
                } else if (ancientSouls.greaterThanOrEqualTo(500)) {
                    ancientSouls = ancientSouls.minus(ancientSouls.modulo(100));
                } else {
                    ancientSouls = ancientSouls.minus(ancientSouls.modulo(10));
                }
            }

            const outsiderLevels = optimalOutsiderLevels[ancientSouls.toNumber()];
            if (outsiderLevels === null) {
                // Should not happen.
                throw Error("Could not look up optimal outsider levels for " + ancientSouls + " ancient souls. Raw ancient souls: " + this.totalAncientSouls);
            }

            suggestedXyl = new Decimal(outsiderLevels[0]);
            suggestedChor = new Decimal(outsiderLevels[1]);
            suggestedPhan = new Decimal(outsiderLevels[2]);
            suggestedBorb = new Decimal(outsiderLevels[3]);
            suggestedPony = new Decimal(outsiderLevels[4]);
        }

        for (let i = 0; i < this.outsiders.length; i++) {
            let outsider = this.outsiders[i];
            switch (outsider.name) {
                case "Xyliqil":
                    outsider.suggestedLevel = suggestedXyl;
                    break;
                case "Chor'gorloth":
                    outsider.suggestedLevel = suggestedChor;
                    break;
                case "Phandoryss":
                    outsider.suggestedLevel = suggestedPhan;
                    break;
                case "Borb":
                    outsider.suggestedLevel = suggestedBorb;
                    break;
                case "Ponyboy":
                    outsider.suggestedLevel = suggestedPony;
                    break;
            }
        }
    }

    private getAncientLevel(ancientName: string): decimal.Decimal {
        let ancient = this.ancientsByName[ancientName];
        return ancient
            ? this.userSettings.useEffectiveLevelForSuggestions
                ? ancient.effectiveLevel
                : ancient.ancientLevel
            : new Decimal(0);
    }

    private getPreTranscendentSuggestedSolomonLevel(currentPrimaryAncientLevel: decimal.Decimal, playStyle: string): decimal.Decimal {
        let solomonMultiplier1: number;
        let solomonMultiplier2: number;
        switch (playStyle) {
            case "idle":
                solomonMultiplier1 = 1.15;
                solomonMultiplier2 = 3.25;
                break;
            case "hybrid":
                solomonMultiplier1 = 1.32;
                solomonMultiplier2 = 4.65;
                break;
            case "active":
                solomonMultiplier1 = 1.21;
                solomonMultiplier2 = 3.73;
                break;
        }

        return Decimal.min(
            currentPrimaryAncientLevel,
            currentPrimaryAncientLevel.pow(2).times(solomonMultiplier2).ln().pow(0.4).times(currentPrimaryAncientLevel.pow(0.8)).times(solomonMultiplier1));
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

            let commaIndex = ancient.name.indexOf(",");
            let ancientShortName = commaIndex >= 0
                ? ancient.name.substring(0, commaIndex)
                : ancient.name;

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

            ancientCosts[ancientShortName] = ancientCost;
        }

        return ancientCosts;
    }
}
