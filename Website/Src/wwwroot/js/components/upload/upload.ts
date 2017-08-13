import { Component } from "@angular/core";
import { ActivatedRoute, Router, Params } from "@angular/router";
import { UploadService } from "../../services/uploadService/uploadService";
import { AuthenticationService } from "../../services/authenticationService/authenticationService";

import "rxjs/add/operator/switchMap";

// tslint:disable-next-line:no-require-imports no-var-requires
const gameData: IGameData = require("../../../data/GameData.json");

// tslint:disable-next-line:no-require-imports no-var-requires
const optimalOutsiderLevels: [number, number, number, number, number][] = require("../../../data/OptimalOutsiderLevels.json");

interface IGameData
{
    ancients: { [id: string]: { name: string, nonTranscendent: boolean, levelCostFormula: string } };
    outsiders: { [id: string]: { id: number, name: string } };
}

interface IAncientViewModel
{
    name: string;
    ancientLevel: number;
    itemLevel: number;
    effectiveLevel: number;
    suggestedLevel?: number;
    diffValue?: number;
    diffCopyValue?: string;
    isBase?: boolean;
}

interface IOutsiderViewModel
{
    id: number;
    name: string;
    currentLevel: number;
    suggestedLevel?: number;
}

@Component({
    selector: "upload",
    templateUrl: "./js/components/upload/upload.html",
    styleUrls: ["./js/components/upload/upload.css"],
})
export class UploadComponent
{
    private static exponentialRegex = /^(\d+(\.\d+)?)e\+?(\d+)$/i;

    public isLoggedIn: boolean;
    public errorMessage: string;

    public userName: string;
    public uploadTime: string;
    public playStyle: string;
    public uploadContent: string;

    public ancients: IAncientViewModel[] = [];
    public outsiders: IOutsiderViewModel[] = [];

    public pendingSouls: number;
    public heroSoulsSpent: number;
    public heroSoulsSacrificed: number;
    public totalAncientSouls: number;
    public transcendentPower: number;
    public maxTranscendentPrimalReward: number;
    public bossLevelToTranscendentPrimalCap: number;
    public titanDamage: number;
    public highestZoneThisTranscension: number;
    public highestZoneLifetime: number;
    public ascensionsThisTranscension: number;
    public ascensionsLifetime: number;
    public rubies: number;

    public showLowAncientSoulWarning: boolean;
    public showMissingSimulationWarning: boolean;

    public get suggestionType(): string
    {
        return this._suggestionType;
    }
    public set suggestionType(value: string)
    {
        this._suggestionType = value;
        this.hydrateAncientSuggestions();
    }

    public get useSoulsFromAscension(): boolean
    {
        return this._useSoulsFromAscension;
    }
    public set useSoulsFromAscension(value: boolean)
    {
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
    private heroSouls: number;

    private static normalizeName(name: string): string
    {
        return name.replace(/[^\w]/gi, "");
    }

    constructor(
        private authenticationService: AuthenticationService,
        private route: ActivatedRoute,
        private router: Router,
        private uploadService: UploadService,
    )
    {
        for (const id in gameData.ancients)
        {
            const ancientDefinition = gameData.ancients[id];

            // Skip ancients no longer in the game.
            if (ancientDefinition.nonTranscendent)
            {
                continue;
            }

            let commaIndex = ancientDefinition.name.indexOf(",");
            let ancientShortName = commaIndex >= 0
                ? ancientDefinition.name.substring(0, commaIndex)
                : ancientDefinition.name;

            let ancient: IAncientViewModel = {
                name: ancientShortName,
                ancientLevel: 0,
                itemLevel: 0,
                effectiveLevel: 0,
            };

            this.ancients.push(ancient);
            this.ancientsByName[ancient.name] = ancient;
        }

        this.ancients = this.ancients.sort((a, b) => a.name < b.name ? -1 : 1);

        for (const id in gameData.outsiders)
        {
            const outsiderDefinition = gameData.outsiders[id];
            let outsider: IOutsiderViewModel = {
                id: outsiderDefinition.id,
                name: outsiderDefinition.name,
                currentLevel: 0,
            };

            this.outsiders.push(outsider);
            this.outsidersByName[outsider.name] = outsider;
        }

        this.outsiders = this.outsiders.sort((a, b) => a.id - b.id);
    }

    public ngOnInit(): void
    {
        this.authenticationService
            .isLoggedIn()
            .subscribe(isLoggedIn => this.isLoggedIn = isLoggedIn);

        this.route.params
            .switchMap((params: Params) => this.uploadService.get(+params["id"]))
            .subscribe(upload => this.handleData(upload), () => this.handleError("There was a problem getting that upload"));
    }

    public deleteUpload(): void
    {
        this.errorMessage = null;
        this.uploadService.delete(this.uploadId)
            .then(() => this.router.navigate(["/dashboard"]))
            .catch(() => this.handleError("There was a problem deleting that upload"));
    }

    private handleData(upload: IUpload): void
    {
        this.errorMessage = null;
        this.uploadId = upload.id;

        if (upload.user)
        {
            this.userName = upload.user.name;
        }

        this.uploadTime = upload.timeSubmitted;
        this.playStyle = upload.playStyle;
        this.uploadContent = upload.uploadContent;

        if (upload.stats)
        {
            for (let i = 0; i < this.ancients.length; i++)
            {
                let ancient = this.ancients[i];
                let ancientName = UploadComponent.normalizeName(ancient.name);
                ancient.ancientLevel = upload.stats["ancient" + ancientName] || 0;
                ancient.itemLevel = upload.stats["item" + ancientName] || 0;
                ancient.effectiveLevel = Math.floor(ancient.ancientLevel + ancient.itemLevel);
            }

            for (let i = 0; i < this.outsiders.length; i++)
            {
                let outsider = this.outsiders[i];
                let outsiderName = UploadComponent.normalizeName(outsider.name);
                outsider.currentLevel = upload.stats["outsider" + outsiderName] || 0;
            }

            this.pendingSouls = upload.stats["pendingSouls"];
            this.heroSouls = upload.stats["heroSouls"];
            this.heroSoulsSpent = upload.stats["heroSoulsSpent"];
            this.heroSoulsSacrificed = upload.stats["heroSoulsSacrificed"];
            this.totalAncientSouls = upload.stats["totalAncientSouls"];
            this.transcendentPower = upload.stats["transcendentPower"];
            this.maxTranscendentPrimalReward = upload.stats["maxTranscendentPrimalReward"];
            this.bossLevelToTranscendentPrimalCap = upload.stats["bossLevelToTranscendentPrimalCap"];
            this.titanDamage = upload.stats["titanDamage"];
            this.highestZoneThisTranscension = upload.stats["highestZoneThisTranscension"];
            this.highestZoneLifetime = upload.stats["highestZoneLifetime"];
            this.ascensionsThisTranscension = upload.stats["ascensionsThisTranscension"];
            this.ascensionsLifetime = upload.stats["ascensionsLifetime"];
            this.rubies = upload.stats["rubies"];

            this.hydrateAncientSuggestions();

            this.calculateOutsiderSuggestions();
        }
    }

    private handleError(errorMessage: string): void
    {
        this.errorMessage = errorMessage;
    }

    private formatForClipboard(num: number): string
    {
        // The game can't handle pasting in decimal points, so we'll just use an altered sci-not form that excludes the decimal (eg. 1.234e5 => 1234e2)
        if (num >= 1e6)
        {
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
        }
        else
        {
            return num.toFixed(0);
        }
    }

    private hydrateAncientSuggestions(): void
    {
        const availableSoulsSuggestionsLatency = "AncientSuggestions";
        appInsights.startTrackEvent(availableSoulsSuggestionsLatency);

        let suggestedLevels: IMap<number>;

        if (this.suggestionType === "AvailableSouls")
        {
            let availableSouls = this.heroSouls;
            if (this.useSoulsFromAscension)
            {
                availableSouls += this.pendingSouls;
            }

            /*
                As an optimization, instead of incrementing only by 1 each time,
                we increment by increasing by a power of 2 until we can no longer afford it.
                Then we back off by a power of 2 until we're back to trying just 1. In this
                way we only calculate the suggestions log(n) times instead of n times where n
                is the optimial primary ancient level.
            */
            let power = 0;
            let primaryAncientLevel = 0;
            let powerChange = 1;

            // Seed with some values in case nothing can be afforded
            suggestedLevels = this.calculateAncientSuggestions(primaryAncientLevel);

            while (power >= 0)
            {
                // If we're on our way up we're trying to find the limit, so use the power.
                let newPrimaryAncientLevel = Math.pow(2, power);
                if (powerChange < 0)
                {
                    // If we're on the way down we're filling the space, so add the last affordable value.
                    newPrimaryAncientLevel += primaryAncientLevel;
                }

                const newSuggestedLevels = this.calculateAncientSuggestions(newPrimaryAncientLevel);
                if (availableSouls >= this.getTotalAncientCost(newSuggestedLevels))
                {
                    primaryAncientLevel = newPrimaryAncientLevel;
                    suggestedLevels = newSuggestedLevels;
                }
                else if (powerChange > 0)
                {
                    // We found the limit, so reverse the direction to try and fill.
                    powerChange = -1;
                }

                power += powerChange;
            }

            // Ensure we don't suggest removing levels
            for (let ancient in suggestedLevels)
            {
                suggestedLevels[ancient] = Math.max(suggestedLevels[ancient], this.getAncientLevel(ancient));
            }
        }
        else
        {
            suggestedLevels = this.calculateAncientSuggestions();

            const baseAncient = this.playStyle === "active"
                ? "Fragsworth"
                : "Siyalatas";
            this.ancientsByName[baseAncient].isBase = true;
        }

        for (let ancientName in suggestedLevels)
        {
            let suggestedLevel = suggestedLevels[ancientName];
            let ancient = this.ancientsByName[ancientName];
            if (ancient)
            {
                ancient.suggestedLevel = suggestedLevel;
                ancient.diffValue = ancient.suggestedLevel - this.getAncientLevel(ancientName);
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

    private calculateAncientSuggestions(currentPrimaryAncientLevel?: number): IMap<number>
    {
        const suggestedLevels: IMap<number> = {};

        const primaryAncient = this.playStyle === "active" ? "Fragsworth" : "Siyalatas";
        if (currentPrimaryAncientLevel === undefined)
        {
            // Use the current level, but don't use it in the suggestions.
            currentPrimaryAncientLevel = this.getAncientLevel(primaryAncient);
        }
        else
        {
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

        const highestZone = this.highestZoneThisTranscension;

        const lnPrimary = Math.log(currentPrimaryAncientLevel);
        const hpScale = 1.145 + (0.005 * Math.floor(highestZone / 500));
        const alpha = this.transcendentPower === 0 ? 0 : 1.4067 * Math.log(1 + this.transcendentPower) / Math.log(hpScale);
        const lnAlpha = this.transcendentPower === 0 ? 0 : Math.log(alpha);

        // Common formulas across play styles
        suggestedLevels["Argaiv"] = currentPrimaryAncientLevel;
        suggestedLevels["Atman"] = (2.832 * lnPrimary) - (1.416 * lnAlpha) - (1.416 * Math.log((4 / 3) - Math.pow(Math.E, -0.013 * currentAtmanLevel))) - 6.613;
        suggestedLevels["Bubos"] = (2.8 * lnPrimary) - (1.4 * Math.log(1 + Math.pow(Math.E, -0.02 * currentBubosLevel))) - 5.94;
        suggestedLevels["Chronos"] = (2.75 * lnPrimary) - (1.375 * Math.log(2 - Math.pow(Math.E, -0.034 * currentChronosLevel))) - 5.1;
        suggestedLevels["Dogcog"] = (2.844 * lnPrimary) - (1.422 * Math.log((1 / 99) + Math.pow(Math.E, -0.01 * currentDogcogLevel))) - 7.232;
        suggestedLevels["Dora"] = (2.877 * lnPrimary) - (1.4365 * Math.log((100 / 99) - Math.pow(Math.E, -0.002 * currentDoraLevel))) - 9.63;
        suggestedLevels["Fortuna"] = (2.875 * lnPrimary) - (1.4375 * Math.log((10 / 9) - Math.pow(Math.E, -0.0025 * currentFortunaLevel))) - 9.3;
        suggestedLevels["Kumawakamaru"] = (2.844 * lnPrimary) - (1.422 * lnAlpha) - (1.422 * Math.log(0.25 + Math.pow(Math.E, -0.001 * currentKumaLevel))) - 7.014;
        suggestedLevels["Mammon"] = suggestedLevels["Mimzee"] = currentPrimaryAncientLevel * 0.926;
        suggestedLevels["Morgulis"] = currentPrimaryAncientLevel * currentPrimaryAncientLevel;
        suggestedLevels["Solomon"] = this.transcendentPower > 0
            ? Math.pow(currentPrimaryAncientLevel, 0.8) / Math.pow(alpha, 0.4)
            : this.getPreTranscendentSuggestedSolomonLevel(currentPrimaryAncientLevel, this.playStyle);

        // Math per play style
        switch (this.playStyle)
        {
            case "idle":
                suggestedLevels["Libertas"] = suggestedLevels["Mammon"];
                suggestedLevels["Nogardnit"] = Math.pow(suggestedLevels["Libertas"], 0.8);
                break;
            case "hybrid":
                const hybridRatioReciprocal = 1 / this.userSettings.hybridRatio;
                suggestedLevels["Bhaal"] = suggestedLevels["Fragsworth"] = hybridRatioReciprocal * currentPrimaryAncientLevel;
                suggestedLevels["Juggernaut"] = Math.pow(suggestedLevels["Fragsworth"], 0.8);
                suggestedLevels["Libertas"] = suggestedLevels["Mammon"];
                suggestedLevels["Nogardnit"] = Math.pow(suggestedLevels["Libertas"], 0.8);
                break;
            case "active":
                suggestedLevels["Bhaal"] = currentPrimaryAncientLevel;
                suggestedLevels["Juggernaut"] = Math.pow(currentPrimaryAncientLevel, 0.8);
                break;
        }

        // Normalize the values
        for (let ancient in suggestedLevels)
        {
            suggestedLevels[ancient] = Math.max(Math.round(suggestedLevels[ancient]), 0);
        }

        return suggestedLevels;
    }

    private calculateOutsiderSuggestions(): void
    {
        let ancientSouls = this.totalAncientSouls;
        if (ancientSouls === 0)
        {
            return;
        }

        this.showLowAncientSoulWarning = false;
        this.showMissingSimulationWarning = false;

        let suggestedXyl = 0;
        let suggestedChor = 0;
        let suggestedPhan = 0;
        let suggestedBorb = 0;
        let suggestedPony = 0;

        // Less ancient souls than the simulation data supported. We can try to guess though.
        // Our guess just alternates leveling Xyl and Pony until Xylk hits 7 and then dump into Pony unti lit matches the 30 AS simulation data.
        if (ancientSouls < 30)
        {
            this.showLowAncientSoulWarning = true;
            appInsights.trackEvent("LowAncientSouls", { ancientSouls: ancientSouls.toString() });
            if (ancientSouls < 14)
            {
                suggestedXyl = Math.ceil(ancientSouls / 2);
                suggestedPony = Math.floor(ancientSouls / 2);
            }
            else
            {
                suggestedXyl = 7;
                suggestedPony = ancientSouls - 7;
            }
        }
        else
        {
            if (ancientSouls > 210)
            {
                this.showMissingSimulationWarning = true;
                appInsights.trackEvent("MissingSimulationData", { ancientSouls: ancientSouls.toString() });
                if (ancientSouls >= 1500)
                {
                    ancientSouls = 1500;
                }
                else if (ancientSouls >= 500)
                {
                    ancientSouls -= ancientSouls % 100;
                }
                else
                {
                    ancientSouls -= ancientSouls % 10;
                }
            }

            const outsiderLevels = optimalOutsiderLevels[ancientSouls];
            if (outsiderLevels === null)
            {
                // Should not happen.
                throw Error("Could not look up optimal outsider levels for " + ancientSouls + " ancient souls. Raw ancient souls: " + this.totalAncientSouls);
            }

            suggestedXyl = outsiderLevels[0];
            suggestedChor = outsiderLevels[1];
            suggestedPhan = outsiderLevels[2];
            suggestedBorb = outsiderLevels[3];
            suggestedPony = outsiderLevels[4];
        }

        for (let i = 0; i < this.outsiders.length; i++)
        {
            let outsider = this.outsiders[i];
            switch (outsider.name)
            {
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

    private getAncientLevel(ancientName: string): number
    {
        let ancient = this.ancientsByName[ancientName];
        return ancient
            ? this.userSettings.useEffectiveLevelForSuggestions
                ? ancient.effectiveLevel
                : ancient.ancientLevel
            : 0;
    }

    private getPreTranscendentSuggestedSolomonLevel(currentPrimaryAncientLevel: number, playStyle: string): number
    {
        let solomonMultiplier1: number;
        let solomonMultiplier2: number;
        switch (playStyle)
        {
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

        const solomonLogFunction = this.userSettings.useReducedSolomonFormula
            ? Math.log10
            : Math.log;
        return currentPrimaryAncientLevel < 100
            ? currentPrimaryAncientLevel
            : Math.round(solomonMultiplier1 * Math.pow(solomonLogFunction(solomonMultiplier2 * Math.pow(currentPrimaryAncientLevel, 2)), 0.4) * Math.pow(currentPrimaryAncientLevel, 0.8));
    }

    private getTotalAncientCost(suggestedLevels: IMap<number>): number
    {
        let cost = 0;
        const chorgorloth = this.outsidersByName["Chor'gorloth"];
        const chorgorlothLevel = chorgorloth ? chorgorloth.currentLevel : 0;
        const ancientCostMultiplier = Math.pow(0.95, chorgorlothLevel);

        for (let ancient in suggestedLevels)
        {
            const suggestedLevel = suggestedLevels[ancient];
            const currentLevel = this.getAncientLevel(ancient);

            // If the ancient is over-leveled, no cost
            if (suggestedLevel < currentLevel)
            {
                continue;
            }

            const costFormula = this.ancientCostFormulas[ancient];
            if (!costFormula)
            {
                continue;
            }

            cost += Math.ceil((costFormula(suggestedLevel) - costFormula(currentLevel)) * ancientCostMultiplier);
        }

        return cost;
    }

    private getAncientCostFormulas(): IMap<(level: number) => number>
    {
        const ancientCosts: IMap<(level: number) => number> = {};

        for (const ancientId in gameData.ancients)
        {
            const ancient = gameData.ancients[ancientId];

            let commaIndex = ancient.name.indexOf(",");
            let ancientShortName = commaIndex >= 0
                ? ancient.name.substring(0, commaIndex)
                : ancient.name;

            let ancientCost: (level: number) => number;
            switch (ancient.levelCostFormula)
            {
                case "one":
                    ancientCost = (n: number) => n;
                    break;
                case "linear":
                    ancientCost = (n: number) => n * (n + 1) / 2;
                    break;
                case "polynomial1_5":
                    ancientCost = (n: number) =>
                    {
                        // Approximate above a certain level for perf
                        // Formula taken from https://github.com/superbob/clicker-heroes-1.0-hsoptimizer/blob/335f13b7304627065a4e515edeb3fb3c4e08f8ad/src/app/components/maths/maths.service.js
                        if (n > 100)
                        {
                            return Math.ceil(2 * Math.pow(n, 2.5) / 5 + Math.pow(n, 1.5) / 2 + Math.pow(n, 0.5) / 8 + Math.pow(n, - 1.5) / 1920);
                        }

                        let cost = 0;
                        for (let i = 1; i <= n; i++)
                        {
                            cost += Math.pow(i, 1.5);
                        }

                        return Math.ceil(cost);
                    };
                    break;
                case "exponential":
                    ancientCost = (n: number) => Math.pow(2, n + 1) - 1;
                    break;
                default:
                    ancientCost = () => 0;
            }

            ancientCosts[ancientShortName] = ancientCost;
        }

        return ancientCosts;
    }
}
