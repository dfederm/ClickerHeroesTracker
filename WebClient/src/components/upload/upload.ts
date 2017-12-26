import { Component, OnInit } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { UploadService } from "../../services/uploadService/uploadService";
import { AuthenticationService, IUserInfo } from "../../services/authenticationService/authenticationService";
import { Decimal } from "decimal.js";
import { AppInsightsService } from "@markpieszak/ng-application-insights";
import { NgbModal } from "@ng-bootstrap/ng-bootstrap";
import { switchMap } from "rxjs/operators";

import { SettingsService, IUserSettings } from "../../services/settingsService/settingsService";
import { IUpload } from "../../models";
import { gameData } from "./models/gameData";
import { Outsiders } from "./models/outsiders";
import { UserData } from "./models/userData";
import { HeroCollection } from "./models/heroCollection";
import { Ancients } from "./models/ancients";
import { Hero } from "./models/hero";
import { Ancient } from "./models/ancient";
import { Outsider } from "./models/outsider";
import { Upgrade } from "./models/upgrade";
import { Attributes } from "./models/attributes";
import { SavedGame } from "./models/savedGame";

interface IAncientViewModel {
    id: string;
    name: string;
    ancientLevel: Decimal;
    itemLevel: Decimal;
    effectiveLevel: Decimal;
    suggestedLevel?: Decimal;
    diffValue?: Decimal;
    diffCopyValue?: string;
    isBase?: boolean;
}

interface IOutsiderViewModel {
    id: number;
    name: string;
    currentLevel: Decimal;
    suggestedLevel: Decimal;
}

interface ICalculateAscensionZoneData {
    zone: number;
    hero: string;
    heroLevel: number;
    heroGilds: number;
    damage: Decimal;
    gold: Decimal;
}

@Component({
    selector: "upload",
    templateUrl: "./upload.html",
    styleUrls: ["./upload.css"],
})
export class UploadComponent implements OnInit {
    private static exponentialRegex = /^(\d+(\.\d+)?)e\+?(\d+)$/i;

    // These all have effective caps where additional levels add less than floating point numbers can handle.
    private static ancientLevelCaps: { [ancientName: string]: number } = {
        Atman: 2880,
        Bubos: 18715,
        Chronos: 1101,
        Dogcog: 3743,
        Dora: 18715,
        Fortuna: 14972,
        Kumawakamaru: 14972,
        Revolc: 3743,
        Vaagur: 1440,
    };

    public userInfo: IUserInfo;
    public errorMessage: string;
    public isLoading: boolean;

    public userId: string;
    public userName: string;
    public uploadTime: string;
    public playStyle: string;
    public savedGame: SavedGame;

    public ancients: IAncientViewModel[] = [];
    public outsiders: IOutsiderViewModel[] = [];

    public pendingSouls: Decimal = new Decimal(0);
    public heroSoulsSpent: Decimal = new Decimal(0);
    public heroSoulsSacrificed: Decimal = new Decimal(0);
    public totalAncientSouls: Decimal = new Decimal(0);
    public transcendentPower: Decimal = new Decimal(0);
    public titanDamage: Decimal = new Decimal(0);
    public highestZoneThisTranscension: Decimal = new Decimal(0);
    public highestZoneLifetime: Decimal = new Decimal(0);
    public ascensionsThisTranscension: Decimal = new Decimal(0);
    public ascensionsLifetime: Decimal = new Decimal(0);
    public rubies: Decimal = new Decimal(0);
    public autoclickers: Decimal = new Decimal(0);
    public remainingAncientSouls: Decimal = new Decimal(0);

    public calculateAscensionZoneSteps: ICalculateAscensionZoneData[];

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

    public get focusBorb(): boolean {
        return this._focusBorb;
    }
    public set focusBorb(value: boolean) {
        this._focusBorb = value;
        this.hydrateOutsiderSuggestions();
    }

    // An index for quick lookup of ancient cost formulas.
    // Each formula gets the sum of the cost of the ancient from 1 to N.
    private ancientCostFormulas = this.getAncientCostFormulas();

    private _suggestionType = "AvailableSouls";
    private _useSoulsFromAscension = true;
    private _focusBorb = true;

    private ancientsByName: { [name: string]: IAncientViewModel } = {};
    private outsidersByName: { [name: string]: IOutsiderViewModel } = {};

    private uploadId: number;
    private heroSouls: Decimal;
    private ancientCostMultiplier: Decimal;

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
                id,
                name: this.getShortName(ancientDefinition),
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
                suggestedLevel: new Decimal(0),
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

    // tslint:disable-next-line:cyclomatic-complexity
    public calculateAscensionZone(): void {
        const latencyCounter = "AscensionZone";
        this.appInsights.startTrackEvent(latencyCounter);

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

        for (let id in gameData.ancients) {
            let ancient = gameData.ancients[id];
            if (ancient.nonTranscendent) {
                continue;
            }

            let ancientViewModel = this.ancientsByName[this.getShortName(ancient)];
            ancientsMap[id] = new Ancient(ancient, outsiders, ancientViewModel.effectiveLevel);
        }

        for (let id in gameData.outsiders) {
            let outsider = gameData.outsiders[id];
            let outsiderViewModel = this.outsidersByName[this.getShortName(outsider)];
            outsidersMap[id] = new Outsider(gameData.outsiders[id], outsiderViewModel.currentLevel);
        }

        // Set current state.
        userData.heroSouls = this.heroSouls;
        userData.highestFinishedZonePersist = this.highestZoneThisTranscension.toNumber();
        userData.totalAutoclickers = this.autoclickers.toNumber();

        // TODO: Don't assume these
        userData.transcendent = true;
        userData.paidForRubyMultiplier = true;

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
                    } else {
                        allUpgradesPurchased = false;
                        break;
                    }
                }

                // If we have all the upgrades and can purchase the next hero, do so.
                if (allUpgradesPurchased && userData.gold.greaterThanOrEqualTo(heroCollection.getById(currentHeroId + 1).getCostUpToLevel(1))) {
                    currentHeroId++;
                    currentHero = heroCollection.getById(currentHeroId);
                    didCurrentHeroChange = true;
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
                for (let i = userData.currentZoneHeight; i < instakillStopZone; i++) {
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

        this.appInsights.stopTrackEvent(latencyCounter);
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
        this.calculateAscensionZoneSteps = null;
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
        this.savedGame = new SavedGame(this.upload.content, this.upload.isScrubbed);

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

        let heroSoulsSpent = new Decimal(0);
        if (this.savedGame.data.ancients && this.savedGame.data.ancients.ancients) {
            for (let i = 0; i < this.ancients.length; i++) {
                let ancient = this.ancients[i];
                let ancientData = this.savedGame.data.ancients.ancients[ancient.id];
                if (ancientData) {
                    ancient.ancientLevel = new Decimal(ancientData.level || 0);
                    ancient.itemLevel = itemLevels[ancient.id] || new Decimal(0);
                    ancient.effectiveLevel = ancient.ancientLevel.plus(ancient.itemLevel).floor();
                    heroSoulsSpent = heroSoulsSpent.plus(ancientData.spentHeroSouls);
                }
            }
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

        this.pendingSouls = new Decimal(this.savedGame.data.primalSouls || 0);
        this.heroSouls = new Decimal(this.savedGame.data.heroSouls || 0);
        this.heroSoulsSpent = heroSoulsSpent;
        this.heroSoulsSacrificed = new Decimal(this.savedGame.data.heroSoulsSacrificed || 0);
        this.totalAncientSouls = new Decimal(this.savedGame.data.ancientSoulsTotal || 0);
        this.transcendentPower = this.savedGame.data.transcendent
            ? new Decimal((2 + (23 * (1 - Math.pow(Math.E, -0.0003 * this.totalAncientSouls.toNumber())))) / 100)
            : new Decimal(0);
        this.titanDamage = new Decimal(this.savedGame.data.titanDamage || 0);
        this.highestZoneThisTranscension = new Decimal(this.savedGame.data.highestFinishedZonePersist || 0);
        this.highestZoneLifetime = Decimal.max(this.savedGame.data.pretranscendentHighestFinishedZone || 0, this.savedGame.data.transcendentHighestFinishedZone || 0, this.highestZoneThisTranscension);
        this.ascensionsLifetime = new Decimal(this.savedGame.data.numWorldResets || 0);
        this.ascensionsThisTranscension = this.savedGame.data.transcendent
            ? new Decimal(this.savedGame.data.numAscensionsThisTranscension || 0)
            : this.ascensionsLifetime;
        this.rubies = new Decimal(this.savedGame.data.rubies || 0);
        this.autoclickers = new Decimal(this.savedGame.data.autoclickers || 0);

        // Ancient cost discount multiplier
        const chorgorloth = this.outsidersByName["Chor'gorloth"];
        const chorgorlothLevel = chorgorloth ? chorgorloth.currentLevel : 0;
        this.ancientCostMultiplier = Decimal.pow(0.95, chorgorlothLevel);

        this.hydrateAncientSuggestions();
        this.hydrateOutsiderSuggestions();
    }

    private handleError(errorMessage: string): void {
        this.isLoading = false;
        this.errorMessage = errorMessage;
    }

    private formatForClipboard(num: Decimal): string {
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

    private hydrateOutsiderSuggestions(): void {
        if (this.totalAncientSouls.isZero()) {
            for (let outsider of this.outsiders) {
                outsider.suggestedLevel = new Decimal(0);
            }

            return;
        }

        const latencyCounter = "OursiderSuggestions";
        this.appInsights.startTrackEvent(latencyCounter);

        let isMoreThan30000 = this.totalAncientSouls.greaterThan(30000);
        let isLessThan100 = this.totalAncientSouls.lessThan(100);

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

        let halfAncientSouls = this.totalAncientSouls.dividedBy(2).floor();

        // Pony and Chor
        if (isMoreThan30000) {
            ponyLevel = new Decimal(0);
            chorLevel = new Decimal(0);
        } else if (isLessThan100) {
            // Get highest buyable level for Pony
            ponyLevel = costToLevelFunc(this.totalAncientSouls);

            // If what's remaining is less than 5, go 1 less
            if (this.totalAncientSouls.minus(levelToCostFunc(ponyLevel)).lessThan(5)) {
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
            let soulsMinusPonyCost = this.totalAncientSouls.minus(ponyCost);

            borbLevel = soulsMinusPonyCost.greaterThan(4) ? new Decimal(2) : new Decimal(1);
            phanLevel = soulsMinusPonyCost.minus(levelToCostFunc(borbLevel));
        } else {
            let borbCostCap: Decimal;
            if (isMoreThan30000) {
                phanLevel = new Decimal(0);
                borbCostCap = this.totalAncientSouls.times(0.9);
            } else {
                phanLevel = this.totalAncientSouls.times(0.15).floor();

                let superOutsiderLevel = costToLevelFunc(this.totalAncientSouls.dividedBy(20).floor());

                rhageistLevel = Decimal.min(superOutsiderLevel, 50);
                kariquaLevel = Decimal.min(superOutsiderLevel, 10);
                orphalasLevel = Decimal.min(superOutsiderLevel, 10);
                senAkhanLevel = Decimal.min(superOutsiderLevel, 20);

                borbCostCap = this.totalAncientSouls.times(0.04);
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

        this.remainingAncientSouls = this.totalAncientSouls
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

    private hydrateAncientSuggestions(): void {
        const latencyCounter = "AncientSuggestions";
        this.appInsights.startTrackEvent(latencyCounter);

        const baseAncient = this.playStyle === "active"
            ? "Fragsworth"
            : "Siyalatas";

        let suggestedLevels: { [key: string]: Decimal };

        if (this.suggestionType === "AvailableSouls") {
            let availableSouls = this.heroSouls;
            if (this.useSoulsFromAscension) {
                availableSouls = availableSouls.plus(this.pendingSouls);
            }

            let baseLevel = this.getAncientLevel(baseAncient);
            let left = baseLevel.times(-1);
            let right: Decimal;
            let mid: Decimal;
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

            let spentHS: Decimal;

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
            latencyCounter,
            {
                suggestionType: this.suggestionType,
                useSoulsFromAscension: this.useSoulsFromAscension.toString(),
            });
    }

    private calculateAncientSuggestions(currentPrimaryAncientLevel?: Decimal): { [key: string]: Decimal } {
        const suggestedLevels: { [key: string]: Decimal } = {};

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
        suggestedLevels.Fortuna = lnPrimary.times(2.875).minus(new Decimal(10).div(9).minus(currentFortunaLevel.times(-0.0025).exp()).ln().times(1.4375)).minus(9.3);
        suggestedLevels.Kumawakamaru = lnPrimary.times(2.844).minus(lnAlpha.times(1.422)).minus(new Decimal(1).div(4).plus(currentKumaLevel.times(-0.01).exp()).ln().times(1.422)).minus(7.014);
        suggestedLevels.Mammon = suggestedLevels.Mimzee = currentPrimaryAncientLevel.times(0.926);
        suggestedLevels.Morgulis = currentPrimaryAncientLevel.pow(2);

        // Math per play style
        switch (this.playStyle) {
            case "idle":
                suggestedLevels.Libertas = suggestedLevels.Mammon;
                suggestedLevels.Nogardnit = this.autoclickers.isZero() ? new Decimal(0) : suggestedLevels.Libertas.pow(0.8);
                break;
            case "hybrid":
                const hybridRatioReciprocal = 1 / this.settings.hybridRatio;
                suggestedLevels.Bhaal = suggestedLevels.Fragsworth = currentPrimaryAncientLevel.times(hybridRatioReciprocal);
                suggestedLevels.Juggernaut = suggestedLevels.Fragsworth.pow(0.8);
                suggestedLevels.Pluto = suggestedLevels.Libertas = suggestedLevels.Mammon;
                suggestedLevels.Nogardnit = this.autoclickers.isZero() ? new Decimal(0) : suggestedLevels.Libertas.pow(0.8);
                break;
            case "active":
                suggestedLevels.Bhaal = currentPrimaryAncientLevel;
                suggestedLevels.Juggernaut = currentPrimaryAncientLevel.pow(0.8);
                suggestedLevels.Pluto = suggestedLevels.Mammon;
                break;
        }

        // Skill ancients
        if (this.settings.shouldLevelSkillAncients) {
            let skillAncientBaseAncient = gameData.ancients[this.settings.skillAncientBaseAncient];
            let skillAncientBaseAncientShortName = this.getShortName(skillAncientBaseAncient);
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
        for (let ancientName in UploadComponent.ancientLevelCaps) {
            if (suggestedLevels[ancientName]) {
                let maxLevel = UploadComponent.ancientLevelCaps[ancientName];
                suggestedLevels[ancientName] = Decimal.min(maxLevel, suggestedLevels[ancientName]);
            }
        }

        // Normalize the values
        for (let ancient in suggestedLevels) {
            suggestedLevels[ancient] = Decimal.max(suggestedLevels[ancient].ceil(), new Decimal(0));
        }

        return suggestedLevels;
    }

    private getAncientLevel(ancientName: string): Decimal {
        let ancient = this.ancientsByName[ancientName];
        return ancient
            ? this.settings.useEffectiveLevelForSuggestions
                ? ancient.effectiveLevel
                : ancient.ancientLevel
            : new Decimal(0);
    }

    private getTotalAncientCost(suggestedLevels: { [key: string]: Decimal }): Decimal {
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

            let ancientShortName = this.getShortName(ancient);
            ancientCosts[ancientShortName] = ancientCost;
        }

        return ancientCosts;
    }

    private getShortName(entity: { name: string }): string {
        let commaIndex = entity.name.indexOf(",");
        return commaIndex >= 0
            ? entity.name.substring(0, commaIndex)
            : entity.name;
    }
}
