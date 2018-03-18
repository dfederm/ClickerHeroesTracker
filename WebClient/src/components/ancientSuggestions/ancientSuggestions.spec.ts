import { ComponentFixture, TestBed } from "@angular/core/testing";
import { AncientSuggestionsComponent } from "./ancientSuggestions";
import { By } from "@angular/platform-browser";
import { FormsModule } from "@angular/forms";
import { ExponentialPipe } from "../../pipes/exponentialPipe";
import { NO_ERRORS_SCHEMA, DebugElement, ChangeDetectorRef } from "@angular/core";
import { AppInsightsService } from "@markpieszak/ng-application-insights";
import { BehaviorSubject } from "rxjs";
import { SettingsService } from "../../services/settingsService/settingsService";
import { SavedGame, ISavedGameData } from "../../models/savedGame";
import { gameData } from "../../models/gameData";
import { Decimal } from "decimal.js";
import { UploadService } from "../../services/uploadService/uploadService";
import { NgbModal } from "@ng-bootstrap/ng-bootstrap";
import { Router } from "@angular/router";

describe("AncientSuggestionsComponent", () => {
    let fixture: ComponentFixture<AncientSuggestionsComponent>;
    let component: AncientSuggestionsComponent;

    const settings = SettingsService.defaultSettings;
    let settingsSubject = new BehaviorSubject(settings);

    let ancientIdByName: { [name: string]: string } = {};
    for (let ancientId in gameData.ancients) {
        let ancient = gameData.ancients[ancientId];
        let name = AncientSuggestionsComponent.getShortName(ancient);
        ancientIdByName[name] = ancientId;
    }

    // Based on upload 566437, but with a few different values to exercise tests better.
    // To help regenerate expected value, put a breakpoint at the end of ancientSuggestions.hydrateAncientSuggestions and run:
    // Object.keys(suggestedLevels).sort().forEach(name => { console.log(name + ": " + suggestedLevels[name].toString()) })
    let savedGameData: Partial<ISavedGameData> = {
        primalSouls: "1e750",
        heroSouls: "1e500",
        highestFinishedZonePersist: 25621,
        ancientSoulsTotal: 25648,
        transcendent: true,
        autoclickers: 10,
        ancients: {
            ancients: {
                4: { spentHeroSouls: "0", level: "9.64831e251", id: 4, purchaseTime: 0 },
                5: { spentHeroSouls: "0", level: "8.93434e251", id: 5, purchaseTime: 0 },
                8: { spentHeroSouls: "1.8181752812377077e500", level: "8.93434e251", id: 8, purchaseTime: 0 },
                9: { spentHeroSouls: "1.8181752812377077e500", level: "8.93434e251", id: 9, purchaseTime: 0 },
                10: { spentHeroSouls: "1.8181752812377077e500", level: "8.93434e251", id: 10, purchaseTime: 0 },
                11: { spentHeroSouls: "1.1676255774597208e496", level: "1657.9999999999998", id: 11, purchaseTime: 0 },
                12: { spentHeroSouls: "2.335251154919442e496", level: "1659.9999999999998", id: 12, purchaseTime: 0 },
                13: { spentHeroSouls: "2.919063943649302e495", level: "1655.9999999999995", id: 13, purchaseTime: 0 },
                14: { spentHeroSouls: "9.34100461967777e496", level: "1661", id: 14, purchaseTime: 0 },
                15: { spentHeroSouls: "2.12037802879293e500", level: "9.64831e251", id: 15, purchaseTime: 0 },
                16: { spentHeroSouls: "4.240761257468004e500", level: "9.309e503", id: 16, purchaseTime: 0 },
                17: { spentHeroSouls: "2.4751186069689943e328", level: "1101", id: 17, purchaseTime: 0 },
                18: { spentHeroSouls: "2.335251154919442e496", level: "1659.0000000000005", id: 18, purchaseTime: 0 },
                19: { spentHeroSouls: "2.12037802879293e500", level: "9.64831e251", id: 19, purchaseTime: 0 },
                20: { spentHeroSouls: "2.771816943112655e430", level: "1440", id: 20, purchaseTime: 0 },
                21: { spentHeroSouls: "7.297659859123254e494", level: "1654.0000000000005", id: 21, purchaseTime: 0 },
                22: { spentHeroSouls: "8.908276195218814e490", level: "1641", id: 22, purchaseTime: 0 },
                23: { spentHeroSouls: "8.908276195218814e490", level: "1641", id: 23, purchaseTime: 0 },
                24: { spentHeroSouls: "8.908276195218814e490", level: "1641.999999999999", id: 24, purchaseTime: 0 },
                25: { spentHeroSouls: "8.908276195218814e490", level: "1641", id: 25, purchaseTime: 0 },
                26: { spentHeroSouls: "8.908276195218814e490", level: "1641", id: 26, purchaseTime: 0 },
                27: { spentHeroSouls: "8.908276195218814e490", level: "1641", id: 27, purchaseTime: 0 },
                28: { spentHeroSouls: "2.12037802879293e500", level: "9.64831e251", id: 28, purchaseTime: 0 },
                29: { spentHeroSouls: "1.6962981656040397e500", level: "3.86866e201", id: 29, purchaseTime: 0 },
                31: { spentHeroSouls: "8.908276195218814e490", level: "1641", id: 31, purchaseTime: 0 },
                32: { spentHeroSouls: "0", level: "3.86866e201", id: 32, purchaseTime: 0 },
            },
        },
        items: {
            items: {
                1: { bonus1Level: "2", bonus2Level: "0", bonus3Level: "0", bonusType1: 18, bonus4Level: "0", bonusType2: null, bonusType3: null, bonusType4: null },
                2: { bonus1Level: "1", bonus2Level: "0", bonus3Level: "0", bonusType1: 12, bonus4Level: "0", bonusType2: null, bonusType3: null, bonusType4: null },
                3: { bonus1Level: "3", bonus2Level: "1", bonus3Level: "2", bonusType1: 9, bonus4Level: "0", bonusType2: 10, bonusType3: 18, bonusType4: null },
                4: { bonus1Level: "67", bonus2Level: "5", bonus3Level: "0", bonusType1: 11, bonus4Level: "0", bonusType2: 20, bonusType3: null, bonusType4: null },
            },
            slots: {
                1: 1,
                2: 2,
                3: 3,
                4: 4,
            },
        },
        outsiders: {
            outsiders: {
                2: {
                    id: 2,
                    level: 150,
                },
            },
        },
    };
    let savedGame = new SavedGame(null, false);
    savedGame.data = savedGameData as ISavedGameData;

    let ancientsWithItems: { [name: string]: true } = {};
    for (let slotId in savedGameData.items.slots) {
        let itemId = savedGameData.items.slots[slotId];
        let item = savedGameData.items.items[itemId];
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
                    ancientsWithItems[bonusType.ancientId] = true;
                }
            }
        }
    }

    beforeEach(done => {
        let appInsights = {
            startTrackEvent: (): void => void 0,
            stopTrackEvent: (): void => void 0,
        };
        let settingsService = { settings: () => settingsSubject };
        let changeDetectorRef = { markForCheck: (): void => void 0 };

        let modalService = { open: (): void => void 0 };
        let router = { navigate: (): void => void 0 };
        let uploadService = { create: (): void => void 0 };

        TestBed.configureTestingModule(
            {
                imports: [FormsModule],
                declarations: [
                    AncientSuggestionsComponent,
                    ExponentialPipe,
                ],
                providers: [
                    { provide: AppInsightsService, useValue: appInsights },
                    { provide: SettingsService, useValue: settingsService },
                    { provide: ChangeDetectorRef, useValue: changeDetectorRef },
                    ExponentialPipe,
                    { provide: NgbModal, useValue: modalService },
                    { provide: Router, useValue: router },
                    { provide: UploadService, useValue: uploadService },
                ],
                schemas: [NO_ERRORS_SCHEMA],
            })
            .compileComponents()
            .then(() => {
                fixture = TestBed.createComponent(AncientSuggestionsComponent);
                component = fixture.componentInstance;
            })
            .then(done)
            .catch(done.fail);
    });

    describe("Suggestion Types", () => {
        beforeEach(done => {
            component.savedGame = savedGame;
            fixture.detectChanges();

            // NgModel is async
            fixture.whenStable()
                .then(done)
                .catch(done.fail);
        });

        it("should default to available souls not including souls from ascension", () => {
            let suggestionTypes = fixture.debugElement.queryAll(By.css("input[type='radio']"));
            expect(suggestionTypes.length).toEqual(2);

            expect((suggestionTypes[0].nativeElement as HTMLInputElement).checked).toEqual(true);
            expect((suggestionTypes[1].nativeElement as HTMLInputElement).checked).toEqual(false);

            let useSoulsFromAscension = fixture.debugElement.query(By.css("input[name='useSoulsFromAscension']"));
            expect(useSoulsFromAscension).not.toBeNull("Couldn't find the 'Use souls from ascension' checkbox");
            expect(useSoulsFromAscension.nativeElement.checked).toEqual(false);
        });

        it("should hide souls from ascension after selecting the Rules of Thumb", () => {
            let suggestionTypes = fixture.debugElement.queryAll(By.css("input[type='radio']"));
            expect(suggestionTypes.length).toEqual(2);

            suggestionTypes[1].nativeElement.click();
            fixture.detectChanges();

            let useSoulsFromAscension = fixture.debugElement.query(By.css("input[name='useSoulsFromAscension']"));
            expect(useSoulsFromAscension).toBeNull("Unexpectedly found the 'Use souls from ascension' checkbox");
        });

        it("should re-show souls from ascension after selecting Available Souls", () => {
            let suggestionTypes = fixture.debugElement.queryAll(By.css("input[type='radio']"));
            expect(suggestionTypes.length).toEqual(2);

            suggestionTypes[1].nativeElement.click();
            fixture.detectChanges();

            suggestionTypes[0].nativeElement.click();
            fixture.detectChanges();

            let useSoulsFromAscension = fixture.debugElement.query(By.css("input[name='useSoulsFromAscension']"));
            expect(useSoulsFromAscension).not.toBeNull("Couldn't find the 'Use souls from ascension' checkbox");
        });
    });

    describe("Ancient Levels", () => {
        it("should display data based on hybrid playstyle and Available Souls using souls from ascension", () => {
            component.savedGame = savedGame;
            component.playStyle = "hybrid";
            component.suggestionType = "AvailableSouls";
            component.useSoulsFromAscension = true;

            let expectedValues: { name: string, suggested?: string }[] =
                [
                    { name: "Argaiv", suggested: "2.3176969355757257304e+376" },
                    { name: "Atman", suggested: "2448" },
                    { name: "Berserker" },
                    { name: "Bhaal", suggested: "1.1588484677878628652e+376" },
                    { name: "Bubos", suggested: "2421" },
                    { name: "Chawedo" },
                    { name: "Chronos", suggested: "1101" },
                    { name: "Dogcog", suggested: "2464" },
                    { name: "Dora", suggested: "2484" },
                    { name: "Energon" },
                    { name: "Fortuna", suggested: "2483" },
                    { name: "Fragsworth", suggested: "1.1588484677878628652e+376" },
                    { name: "Hecatoncheir" },
                    { name: "Juggernaut", suggested: "7.0993946787669440805e+300" },
                    { name: "Kleptos" },
                    { name: "Kumawakamaru", suggested: "2460" },
                    { name: "Libertas", suggested: "2.1461873623431220264e+376" },
                    { name: "Mammon", suggested: "2.1461873623431220264e+376" },
                    { name: "Mimzee", suggested: "2.1461873623431220264e+376" },
                    { name: "Morgulis", suggested: "5.3717190851771097468e+752" },
                    { name: "Nogardnit", suggested: "1.1623424703097457586e+301" },
                    { name: "Pluto", suggested: "1.0730936811715610132e+376" },
                    { name: "Revolc" },
                    { name: "Siyalatas", suggested: "2.3176969355757257304e+376" },
                    { name: "Sniperino" },
                    { name: "Vaagur" },
                ];

            verify(expectedValues, "-9.9999352075945758335e+749");
        });

        it("should display data based on hybrid playstyle and Available Souls without using souls from ascension", () => {
            component.savedGame = savedGame;
            component.playStyle = "hybrid";
            component.suggestionType = "AvailableSouls";
            component.useSoulsFromAscension = false;

            let expectedValues: { name: string, suggested?: string }[] =
                [
                    { name: "Argaiv", suggested: "9.922212356733026065e+251" },
                    { name: "Atman", suggested: "1655.9999999999995" },
                    { name: "Berserker" },
                    { name: "Bhaal", suggested: "9.64831e+251" },
                    { name: "Bubos", suggested: "1659.0000000000005" },
                    { name: "Chawedo" },
                    { name: "Chronos", suggested: "1101" },
                    { name: "Dogcog", suggested: "1657.9999999999998" },
                    { name: "Dora", suggested: "1661" },
                    { name: "Energon" },
                    { name: "Fortuna", suggested: "1659.9999999999998" },
                    { name: "Fragsworth", suggested: "9.64831e+251" },
                    { name: "Hecatoncheir" },
                    { name: "Juggernaut", suggested: "3.86866e+201" },
                    { name: "Kleptos" },
                    { name: "Kumawakamaru", suggested: "1654.0000000000005" },
                    { name: "Libertas", suggested: "9.64831e+251" },
                    { name: "Mammon", suggested: "9.1879686423347821362e+251" },
                    { name: "Mimzee", suggested: "9.1879686423347821362e+251" },
                    { name: "Morgulis", suggested: "9.8450298052105551295e+503" },
                    { name: "Nogardnit", suggested: "3.86866e+201" },
                    { name: "Pluto", suggested: "8.93434e+251" },
                    { name: "Revolc" },
                    { name: "Siyalatas", suggested: "9.922212356733026065e+251" },
                    { name: "Sniperino" },
                    { name: "Vaagur" },
                ];

            verify(expectedValues, "-9.9997843328756843346e+499");
        });

        it("should display data based on hybrid playstyle and the Rules of Thumb", () => {
            component.savedGame = savedGame;
            component.playStyle = "hybrid";
            component.suggestionType = "RulesOfThumb";

            // Use rules of thumb
            let suggestionTypes = fixture.debugElement.queryAll(By.css("input[type='radio']"));
            suggestionTypes[1].nativeElement.click();

            let expectedValues: { name: string, suggested?: string, isPrimary?: boolean }[] =
                [
                    { name: "Argaiv", suggested: "8.93434e+251" },
                    { name: "Atman", suggested: "1637" },
                    { name: "Berserker" },
                    { name: "Bhaal", suggested: "4.46717e+251" },
                    { name: "Bubos", suggested: "1619" },
                    { name: "Chawedo" },
                    { name: "Chronos", suggested: "1101" },
                    { name: "Dogcog", suggested: "1650" },
                    { name: "Dora", suggested: "1660" },
                    { name: "Energon" },
                    { name: "Fortuna", suggested: "1659" },
                    { name: "Fragsworth", suggested: "4.46717e+251" },
                    { name: "Hecatoncheir" },
                    { name: "Juggernaut", suggested: "2.0894211381190353384e+201" },
                    { name: "Kleptos" },
                    { name: "Kumawakamaru", suggested: "1645" },
                    { name: "Libertas", suggested: "8.27319884e+251" },
                    { name: "Mammon", suggested: "8.27319884e+251" },
                    { name: "Mimzee", suggested: "8.27319884e+251" },
                    { name: "Morgulis", suggested: "7.98224312356e+503" },
                    { name: "Nogardnit", suggested: "3.4208873250310610443e+201" },
                    { name: "Pluto", suggested: "4.13659942e+251" },
                    { name: "Revolc" },
                    { name: "Siyalatas", isPrimary: true },
                    { name: "Sniperino" },
                    { name: "Vaagur" },
                ];

            verify(expectedValues);
        });

        it("should display data based on idle playstyle and Available Souls without using souls from ascension", () => {
            component.savedGame = savedGame;
            component.playStyle = "idle";
            component.suggestionType = "AvailableSouls";
            component.useSoulsFromAscension = false;

            let expectedValues: { name: string, suggested?: string }[] =
                [
                    { name: "Argaiv", suggested: "9.922212356733026065e+251" },
                    { name: "Atman", suggested: "1655.9999999999995" },
                    { name: "Berserker" },
                    { name: "Bhaal" },
                    { name: "Bubos", suggested: "1659.0000000000005" },
                    { name: "Chawedo" },
                    { name: "Chronos", suggested: "1101" },
                    { name: "Dogcog", suggested: "1657.9999999999998" },
                    { name: "Dora", suggested: "1661" },
                    { name: "Energon" },
                    { name: "Fortuna", suggested: "1659.9999999999998" },
                    { name: "Fragsworth" },
                    { name: "Hecatoncheir" },
                    { name: "Juggernaut" },
                    { name: "Kleptos" },
                    { name: "Kumawakamaru", suggested: "1654.0000000000005" },
                    { name: "Libertas", suggested: "9.64831e+251" },
                    { name: "Mammon", suggested: "9.1879686423347821362e+251" },
                    { name: "Mimzee", suggested: "9.1879686423347821362e+251" },
                    { name: "Morgulis", suggested: "9.8450298052105551295e+503" },
                    { name: "Nogardnit", suggested: "3.86866e+201" },
                    { name: "Pluto" },
                    { name: "Revolc" },
                    { name: "Siyalatas", suggested: "9.922212356733026065e+251" },
                    { name: "Sniperino" },
                    { name: "Vaagur" },
                ];

            verify(expectedValues, "-9.9997843328756843346e+499");
        });

        it("should display data based on idle playstyle and the Rules of Thumb", () => {
            component.savedGame = savedGame;
            component.playStyle = "idle";
            component.suggestionType = "RulesOfThumb";

            let expectedValues: { name: string, suggested?: string, isPrimary?: boolean }[] =
                [
                    { name: "Argaiv", suggested: "8.93434e+251" },
                    { name: "Atman", suggested: "1637" },
                    { name: "Berserker" },
                    { name: "Bhaal" },
                    { name: "Bubos", suggested: "1619" },
                    { name: "Chawedo" },
                    { name: "Chronos", suggested: "1101" },
                    { name: "Dogcog", suggested: "1650" },
                    { name: "Dora", suggested: "1660" },
                    { name: "Energon" },
                    { name: "Fortuna", suggested: "1659" },
                    { name: "Fragsworth" },
                    { name: "Hecatoncheir" },
                    { name: "Juggernaut" },
                    { name: "Kleptos" },
                    { name: "Kumawakamaru", suggested: "1645" },
                    { name: "Libertas", suggested: "8.27319884e+251" },
                    { name: "Mammon", suggested: "8.27319884e+251" },
                    { name: "Mimzee", suggested: "8.27319884e+251" },
                    { name: "Morgulis", suggested: "7.98224312356e+503" },
                    { name: "Nogardnit", suggested: "3.4208873250310610443e+201" },
                    { name: "Pluto" },
                    { name: "Revolc" },
                    { name: "Siyalatas", isPrimary: true },
                    { name: "Sniperino" },
                    { name: "Vaagur" },
                ];

            verify(expectedValues);
        });

        it("should display data based on active playstyle and Available Souls without using souls from ascension", () => {
            component.savedGame = savedGame;
            component.playStyle = "active";
            component.suggestionType = "AvailableSouls";
            component.useSoulsFromAscension = false;

            let expectedValues: { name: string, suggested?: string }[] =
                [
                    { name: "Argaiv", suggested: "9.916295976653634376e+251" },
                    { name: "Atman", suggested: "1655.9999999999995" },
                    { name: "Berserker" },
                    { name: "Bhaal", suggested: "9.916295976653634376e+251" },
                    { name: "Bubos", suggested: "1659.0000000000005" },
                    { name: "Chawedo" },
                    { name: "Chronos", suggested: "1101" },
                    { name: "Dogcog", suggested: "1657.9999999999998" },
                    { name: "Dora", suggested: "1661" },
                    { name: "Energon" },
                    { name: "Fortuna", suggested: "1659.9999999999998" },
                    { name: "Fragsworth", suggested: "9.916295976653634376e+251" },
                    { name: "Hecatoncheir" },
                    { name: "Juggernaut", suggested: "3.9543907786902441483e+201" },
                    { name: "Kleptos" },
                    { name: "Kumawakamaru", suggested: "1654.0000000000005" },
                    { name: "Libertas" },
                    { name: "Mammon", suggested: "9.1824900743812654322e+251" },
                    { name: "Mimzee", suggested: "9.1824900743812654322e+251" },
                    { name: "Morgulis", suggested: "9.8332925896597056441e+503" },
                    { name: "Nogardnit" },
                    { name: "Pluto", suggested: "9.1824900743812654322e+251" },
                    { name: "Revolc" },
                    { name: "Siyalatas" },
                    { name: "Sniperino" },
                    { name: "Vaagur" },
                ];

            verify(expectedValues, "-9.9986729745924893928e+499");
        });

        it("should display data based on active playstyle and the Rules of Thumb", () => {
            component.savedGame = savedGame;
            component.playStyle = "active";
            component.suggestionType = "RulesOfThumb";

            let expectedValues: { name: string, suggested?: string, isPrimary?: boolean }[] =
                [
                    { name: "Argaiv", suggested: "9.64831e+251" },
                    { name: "Atman", suggested: "1637" },
                    { name: "Berserker" },
                    { name: "Bhaal", suggested: "9.64831e+251" },
                    { name: "Bubos", suggested: "1619" },
                    { name: "Chawedo" },
                    { name: "Chronos", suggested: "1101" },
                    { name: "Dogcog", suggested: "1650" },
                    { name: "Dora", suggested: "1660" },
                    { name: "Energon" },
                    { name: "Fortuna", suggested: "1659" },
                    { name: "Fragsworth", isPrimary: true },
                    { name: "Hecatoncheir" },
                    { name: "Juggernaut", suggested: "3.8686638838780241295e+201" },
                    { name: "Kleptos" },
                    { name: "Kumawakamaru", suggested: "1646" },
                    { name: "Libertas" },
                    { name: "Mammon", suggested: "8.93433506e+251" },
                    { name: "Mimzee", suggested: "8.93433506e+251" },
                    { name: "Morgulis", suggested: "9.30898858561e+503" },
                    { name: "Nogardnit" },
                    { name: "Pluto", suggested: "8.93433506e+251" },
                    { name: "Revolc" },
                    { name: "Siyalatas" },
                    { name: "Sniperino" },
                    { name: "Vaagur" },
                ];

            verify(expectedValues);
        });

        function verify(expectedValues: { name: string, suggested?: string, isPrimary?: boolean }[], expectedSpentSoulsStr?: string): void {
            fixture.detectChanges();

            let exponentialPipe = TestBed.get(ExponentialPipe) as ExponentialPipe;

            let table = fixture.debugElement.query(By.css("table"));
            expect(table).not.toBeNull();

            let rows = table.queryAll(By.css("tbody tr"));
            expect(rows.length).toEqual(expectedValues.length);

            for (let i = 0; i < rows.length; i++) {
                let cells = rows[i].children;
                let expected = expectedValues[i];

                let expectedName = expected.name + ":";
                expect(getNormalizedTextContent(cells[0])).toEqual(expectedName, "Unexpected ancient name");

                let ancientId = ancientIdByName[expected.name];

                let expectedCurrentLevel = new Decimal(savedGameData.ancients.ancients[ancientId].level);
                let expectedCurrentLevelText = exponentialPipe.transform(expectedCurrentLevel);
                if (ancientsWithItems[ancientId]) {
                    expectedCurrentLevelText += " (*)";
                }
                expect(getNormalizedTextContent(cells[1])).toEqual(expectedCurrentLevelText, `Unexpected current level for ${expected.name}`);

                let expectedSuggestedLevel = new Decimal(expected.suggested || 0);
                let expectedSuggestedLevelText = expected.isPrimary
                    ? "N/A (*)"
                    : expected.suggested === undefined
                        ? "-"
                        : exponentialPipe.transform(expectedSuggestedLevel);
                expect(getNormalizedTextContent(cells[2])).toEqual(expectedSuggestedLevelText, `Unexpected suggested level for ${expected.name}`);

                let expectedDifference = expectedSuggestedLevel.minus(expectedCurrentLevel);
                if (expectedDifference.log().floor().minus(expectedCurrentLevel.log().floor()).lessThan(-4)) {
                    expectedDifference = new Decimal(0);
                }

                let expectedDifferenceText = expected.isPrimary || expected.suggested === undefined
                    ? "-"
                    : exponentialPipe.transform(expectedDifference);
                expect(getNormalizedTextContent(cells[3])).toEqual(expectedDifferenceText, `Unexpected difference in levels for ${expected.name}`);
            }

            let heroSoulsRow = table.query(By.css("tfoot tr"));
            if (component.suggestionType === "RulesOfThumb") {
                expect(heroSoulsRow).toBeNull();
                expect(expectedSpentSoulsStr).toBeUndefined();
            } else {
                expect(heroSoulsRow).not.toBeNull();
                expect(expectedSpentSoulsStr).toBeDefined();

                let heroSoulsCells = heroSoulsRow.children;
                expect(heroSoulsCells.length).toEqual(4);

                let expectedAvailableSouls = new Decimal(savedGame.data.heroSouls);
                if (component.useSoulsFromAscension) {
                    expectedAvailableSouls = expectedAvailableSouls.plus(savedGame.data.primalSouls);
                }

                let expectedAvailableSoulsText = exponentialPipe.transform(expectedAvailableSouls);
                expect(getNormalizedTextContent(heroSoulsCells[1])).toEqual(expectedAvailableSoulsText);
                expect(expectedAvailableSouls).toEqual(component.availableSouls);

                let expectedSpentSouls = new Decimal(expectedSpentSoulsStr);
                let expectedSpentSoulsText = exponentialPipe.transform(expectedSpentSouls);
                expect(getNormalizedTextContent(heroSoulsCells[2])).toEqual(expectedSpentSoulsText);
                expect(expectedSpentSouls).toEqual(component.spentSouls);
                expect(expectedSpentSouls.isPositive()).toEqual(false);

                let expectedRemainingSouls = expectedAvailableSouls.plus(expectedSpentSouls);
                let expectedRemainingSoulsText = exponentialPipe.transform(expectedRemainingSouls);
                expect(getNormalizedTextContent(heroSoulsCells[3])).toEqual(expectedRemainingSoulsText);
                expect(expectedRemainingSouls).toEqual(component.remainingSouls);
                expect(expectedRemainingSouls.isNegative()).toEqual(false);
            }
        }
    });

    describe("Autolevel", () => {
        describe("Button", () => {
            it("should display when using available souls and not using souls from ascension", () => {
                component.savedGame = savedGame;
                component.suggestionType = "AvailableSouls";
                component.useSoulsFromAscension = false;

                fixture.detectChanges();

                let autolevelButton = fixture.debugElement.query(By.css("button"));
                expect(autolevelButton).not.toBeNull();
                expect(autolevelButton.properties.disabled).toEqual(false);
            });

            it("should not display when using available souls and using souls from ascension", () => {
                component.savedGame = savedGame;
                component.suggestionType = "AvailableSouls";
                component.useSoulsFromAscension = true;

                fixture.detectChanges();

                let autolevelButton = fixture.debugElement.query(By.css("button"));
                expect(autolevelButton).not.toBeNull();
                expect(autolevelButton.properties.disabled).toEqual(true);
            });

            it("should not display when using rules of thumb", () => {
                component.savedGame = savedGame;
                component.suggestionType = "RulesOfThumb";

                fixture.detectChanges();

                let autolevelButton = fixture.debugElement.query(By.css("button"));
                expect(autolevelButton).not.toBeNull();
                expect(autolevelButton.properties.disabled).toEqual(true);
            });
        });

        describe("Modal", () => {
            it("should show the modal when clicked", () => {
                let modalService = TestBed.get(NgbModal) as NgbModal;
                spyOn(modalService, "open").and.returnValue({ result: Promise.resolve() });

                component.savedGame = savedGame;
                component.suggestionType = "AvailableSouls";
                component.useSoulsFromAscension = false;

                fixture.detectChanges();

                let autolevelButton = fixture.debugElement.query(By.css("button"));
                expect(autolevelButton).not.toBeNull();
                expect(autolevelButton.properties.disabled).toEqual(false);

                autolevelButton.nativeElement.click();
                expect(modalService.open).toHaveBeenCalled();
            });

            it("should add to progress", async () => {
                let modal = jasmine.createSpyObj("modal", ["close"]);
                let modalService = TestBed.get(NgbModal) as NgbModal;
                spyOn(modalService, "open").and.returnValue(modal);

                let uploadService = TestBed.get(UploadService) as UploadService;
                spyOn(uploadService, "create").and.returnValue(Promise.resolve<number>(123));

                let router = TestBed.get(Router) as Router;
                spyOn(router, "navigate").and.returnValue({ result: Promise.resolve() });

                component.savedGame = savedGame;
                component.suggestionType = "AvailableSouls";
                component.useSoulsFromAscension = false;

                fixture.detectChanges();

                let autolevelButton = fixture.debugElement.query(By.css("button"));
                expect(autolevelButton).not.toBeNull();
                expect(autolevelButton.properties.disabled).toEqual(false);

                autolevelButton.nativeElement.click();
                expect(modalService.open).toHaveBeenCalled();

                component.saveAutolevel();

                // Wait for stability from the uploadService promise
                fixture.detectChanges();
                await fixture.whenStable();
                fixture.detectChanges();

                expect(uploadService.create).toHaveBeenCalledWith(component.autoLeveledSavedGame.content, true, component.playStyle);
                expect(router.navigate).toHaveBeenCalledWith(["/uploads", 123]);
                expect(modal.close).toHaveBeenCalled();
                expect(component.modalErrorMessage).toBeNull();
            });

            it("should show an error when the upload fails", async () => {
                let modal = jasmine.createSpyObj("modal", ["close"]);
                let modalService = TestBed.get(NgbModal) as NgbModal;
                spyOn(modalService, "open").and.returnValue(modal);

                let uploadService = TestBed.get(UploadService) as UploadService;
                spyOn(uploadService, "create").and.returnValue(Promise.reject({}));

                let router = TestBed.get(Router) as Router;
                spyOn(router, "navigate");

                component.savedGame = savedGame;
                component.suggestionType = "AvailableSouls";
                component.useSoulsFromAscension = false;

                fixture.detectChanges();

                let autolevelButton = fixture.debugElement.query(By.css("button"));
                expect(autolevelButton).not.toBeNull();
                expect(autolevelButton.properties.disabled).toEqual(false);

                autolevelButton.nativeElement.click();
                expect(modalService.open).toHaveBeenCalled();

                component.saveAutolevel();

                // Wait for stability from the uploadService promise
                fixture.detectChanges();
                await fixture.whenStable();
                fixture.detectChanges();

                expect(uploadService.create).toHaveBeenCalledWith(component.autoLeveledSavedGame.content, true, component.playStyle);
                expect(router.navigate).not.toHaveBeenCalled();
                expect(modal.close).not.toHaveBeenCalled();
                expect(component.modalErrorMessage).not.toBeNull();
            });
        });
    });

    function getNormalizedTextContent(element: DebugElement): string {
        let nativeElement: HTMLElement = element.nativeElement;
        return nativeElement.textContent.trim().replace(/\s\s+/g, " ");
    }
});
