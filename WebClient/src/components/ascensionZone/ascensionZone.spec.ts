import { ComponentFixture, TestBed } from "@angular/core/testing";
import { AscensionZoneComponent } from "./ascensionZone";
import { By } from "@angular/platform-browser";
import { ISavedGameData, SavedGame } from "../../models/savedGame";
import { DebugElement, ChangeDetectorRef } from "@angular/core";
import { ExponentialPipe } from "../../pipes/exponentialPipe";
import { AppInsightsService } from "@markpieszak/ng-application-insights";
import { SettingsService } from "../../services/settingsService/settingsService";
import { BehaviorSubject } from "rxjs";

describe("AscensionZoneComponent", () => {
    let fixture: ComponentFixture<AscensionZoneComponent>;
    let component: AscensionZoneComponent;

    const settings = SettingsService.defaultSettings;
    let settingsSubject = new BehaviorSubject(settings);

    // Based on upload 566437, but with a few different values to exercise tests better.
    let savedGameData: Partial<ISavedGameData> = {
        heroSouls: "2.8409502254938617e494",
        highestFinishedZonePersist: 25621,
        transcendent: true,
        autoclickers: 10,
        paidForRubyMultiplier: true,
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
                1: { level: 20, id: 1 },
                2: { level: 150, id: 2 },
                3: { level: 3918, id: 3 },
                4: { level: 0, id: 4 },
                5: { level: 80, id: 5 },
                6: { level: 100, id: 6 },
                7: { level: 50, id: 7 },
                8: { level: 20, id: 8 },
                9: { level: 20, id: 9 },
                10: { level: 20, id: 10 },
            },
        },
    };
    let savedGame = new SavedGame(null, false);
    savedGame.data = savedGameData as ISavedGameData;

    beforeEach(done => {
        let appInsights = {
            trackMetric: (): void => void 0,
        };
        let settingsService = { settings: () => settingsSubject };
        let changeDetectorRef = { markForCheck: (): void => void 0 };

        TestBed.configureTestingModule(
            {
                declarations: [
                    AscensionZoneComponent,
                    ExponentialPipe,
                ],
                providers: [
                    { provide: AppInsightsService, useValue: appInsights },
                    { provide: SettingsService, useValue: settingsService },
                    { provide: ChangeDetectorRef, useValue: changeDetectorRef },
                ],
            })
            .compileComponents()
            .then(() => {
                fixture = TestBed.createComponent(AscensionZoneComponent);
                component = fixture.componentInstance;
            })
            .then(done)
            .catch(done.fail);
    });

    it("should show data after clicking the calculate button", () => {
        component.savedGame = savedGame;
        fixture.detectChanges();

        // Not showing the table yet
        let table = fixture.debugElement.query(By.css("table"));
        expect(table).toBeNull();

        let buttons = fixture.debugElement.queryAll(By.css("button"));
        let calculateButton: DebugElement;
        for (let i = 0; i < buttons.length; i++) {
            if (getNormalizedTextContent(buttons[i]) === "Calculate") {
                calculateButton = buttons[i];
            }
        }

        expect(calculateButton).toBeDefined("Could not find the 'Calculate' button");

        calculateButton.nativeElement.click();
        fixture.detectChanges();

        // The new table exists
        table = fixture.debugElement.query(By.css("table"));
        expect(table).not.toBeNull();

        // Don't make specific assertions about the rows since there is some level of randomness in the calculation.
        let rows = table.queryAll(By.css("tbody tr"));
        expect(rows.length).toBeGreaterThan(0);
    });

    function getNormalizedTextContent(element: DebugElement): string {
        let nativeElement: HTMLElement = element.nativeElement;
        return nativeElement.textContent.trim().replace(/\s\s+/g, " ");
    }
});
