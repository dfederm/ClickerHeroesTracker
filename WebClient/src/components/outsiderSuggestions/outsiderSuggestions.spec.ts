import { ComponentFixture, TestBed } from "@angular/core/testing";
import { OutsiderSuggestionsComponent } from "./outsiderSuggestions";
import { By } from "@angular/platform-browser";
import { FormsModule } from "@angular/forms";
import { SettingsService } from "../../services/settingsService/settingsService";
import { BehaviorSubject } from "rxjs";
import { ISavedGameData, SavedGame } from "../../models/savedGame";
import { DebugElement, ChangeDetectorRef } from "@angular/core";
import { AppInsightsService } from "@markpieszak/ng-application-insights";
import { gameData } from "../../models/gameData";
import { ExponentialPipe } from "../../pipes/exponentialPipe";
import { PercentPipe } from "@angular/common";
import { Decimal } from "decimal.js";

describe("OutsiderSuggestionsComponent", () => {
    let fixture: ComponentFixture<OutsiderSuggestionsComponent>;
    let component: OutsiderSuggestionsComponent;

    const settings = SettingsService.defaultSettings;
    let settingsSubject = new BehaviorSubject(settings);

    let outsiderIdByName: { [name: string]: string } = {};
    for (let id in gameData.outsiders) {
        let outsider = gameData.outsiders[id];
        outsiderIdByName[outsider.name] = id;
    }

    // Based on upload 566437, but with a few different values to exercise tests better.
    let savedGameData: Partial<ISavedGameData> = {
        ancientSoulsTotal: 25648,
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

    const outsiderOrder = ["Xyliqil", "Chor'gorloth", "Phandoryss", "Ponyboy", "Borb", "Rhageist", "K'Ariqua", "Orphalas", "Sen-Akhan"];

    beforeEach(done => {
        let appInsights = {
            startTrackEvent: (): void => void 0,
            stopTrackEvent: (): void => void 0,
        };
        let settingsService = { settings: () => settingsSubject };
        let changeDetectorRef = { markForCheck: (): void => void 0 };

        TestBed.configureTestingModule(
            {
                imports: [FormsModule],
                declarations: [
                    OutsiderSuggestionsComponent,
                    ExponentialPipe,
                ],
                providers: [
                    { provide: AppInsightsService, useValue: appInsights },
                    { provide: SettingsService, useValue: settingsService },
                    { provide: ChangeDetectorRef, useValue: changeDetectorRef },
                    ExponentialPipe,
                    PercentPipe,
                ],
            })
            .compileComponents()
            .then(() => {
                fixture = TestBed.createComponent(OutsiderSuggestionsComponent);
                component = fixture.componentInstance;
            })
            .then(done)
            .catch(done.fail);
    });

    // Easy way to generate these tests is to open https://driej.github.io/Clicker-Heroes-Outsiders/, put a breakpoint at the end of calculateClick, and run the following:
    // JSON.stringify({ ancientSouls: inputAS, useBeta: e11, expectedLevels: [outsiders.xyliqil, outsiders.chor, outsiders.phan, outsiders.pony, outsiders.borb, outsiders.rhageist, outsiders.kariqua, outsiders.orphalas, outsiders.senakhan], expectedRemaining: inputAS-totalAS, newHze: HZE, newHeroSouls: Math.pow(10, logHS).toExponential(3), newAncientSouls: AS, newTranscendentPower: newTP })
    const tests: {
        ancientSouls: number,
        useBeta: boolean,
        expectedLevels: [number, number, number, number, number, number, number, number, number],
        expectedRemaining: number,
        newHze: number,
        newHeroSouls: string,
        newAncientSouls: number,
        newTranscendentPower: number,
    }[] = [
            // Patch e10
            { ancientSouls: 0, useBeta: false, expectedLevels: [0, 0, 0, 0, 0, 0, 0, 0, 0], expectedRemaining: 0, newHze: 1400, newHeroSouls: "342788", newAncientSouls: 27, newTranscendentPower: 2.185547518071864 },
            { ancientSouls: 1, useBeta: false, expectedLevels: [0, 0, 0, 1, 0, 0, 0, 0, 0], expectedRemaining: 0, newHze: 1512, newHeroSouls: "6.141e+6", newAncientSouls: 33, newTranscendentPower: 2.2265765952919843 },
            { ancientSouls: 10, useBeta: false, expectedLevels: [0, 0, 4, 2, 2, 0, 0, 0, 0], expectedRemaining: 0, newHze: 2483, newHeroSouls: "1.367e+9", newAncientSouls: 45, newTranscendentPower: 2.308413524692149 },
            { ancientSouls: 100, useBeta: false, expectedLevels: [0, 1, 5, 8, 8, 5, 1, 2, 2], expectedRemaining: 0, newHze: 9863, newHeroSouls: "1.274e+28", newAncientSouls: 140, newTranscendentPower: 2.9459950468328557 },
            { ancientSouls: 1000, useBeta: false, expectedLevels: [0, 22, 57, 29, 15, 11, 6, 9, 2], expectedRemaining: 0, newHze: 50035, newHeroSouls: "3.704e+338", newAncientSouls: 1692, newTranscendentPower: 11.155414522699742 },
            { ancientSouls: 10000, useBeta: false, expectedLevels: [0, 104, 312, 63, 29, 41, 23, 34, 9], expectedRemaining: 0, newHze: 189402, newHeroSouls: "1.788e+3524", newAncientSouls: 17621, newTranscendentPower: 24.883609665499616 },
            { ancientSouls: 100000, useBeta: false, expectedLevels: [0, 133, 382, 72, 400, 99, 33, 52, 44], expectedRemaining: 0, newHze: 2041500, newHeroSouls: "2.182e+39574", newAncientSouls: 197871, newTranscendentPower: 25 },

            // Patch e11
            { ancientSouls: 0, useBeta: true, expectedLevels: [0, 0, 0, 0, 0, 0, 0, 0, 0], expectedRemaining: 0, newHze: 1400, newHeroSouls: "355674", newAncientSouls: 27, newTranscendentPower: 2.185547518071864 },
            { ancientSouls: 1, useBeta: true, expectedLevels: [0, 0, 0, 1, 0, 0, 0, 0, 0], expectedRemaining: 0, newHze: 1512, newHeroSouls: "1.174e+6", newAncientSouls: 30, newTranscendentPower: 2.2060712882236757 },
            { ancientSouls: 10, useBeta: true, expectedLevels: [0, 0, 4, 2, 2, 0, 0, 0, 0], expectedRemaining: 0, newHze: 2483, newHeroSouls: "1.745e+8", newAncientSouls: 41, newTranscendentPower: 2.2811672764423783 },
            { ancientSouls: 100, useBeta: true, expectedLevels: [0, 1, 5, 8, 8, 5, 1, 2, 2], expectedRemaining: 0, newHze: 9863, newHeroSouls: "1.292e+27", newAncientSouls: 135, newTranscendentPower: 2.9128892162375024 },
            { ancientSouls: 1000, useBeta: true, expectedLevels: [0, 23, 62, 30, 10, 11, 7, 9, 2], expectedRemaining: 0, newHze: 50035, newHeroSouls: "3.968e+337", newAncientSouls: 1687, newTranscendentPower: 11.13463206153463 },
            { ancientSouls: 10000, useBeta: true, expectedLevels: [0, 107, 310, 64, 10, 41, 23, 34, 9], expectedRemaining: 0, newHze: 189402, newHeroSouls: "1.846e+3523", newAncientSouls: 17616, newTranscendentPower: 24.883434948993248 },
            { ancientSouls: 100000, useBeta: true, expectedLevels: [0, 150, 8705, 348, 46, 107, 76, 117, 71], expectedRemaining: 0, newHze: 2716000, newHeroSouls: "7.382e+52647", newAncientSouls: 263239, newTranscendentPower: 25 },
        ];
    for (let i = 0; i < tests.length; i++) {
        let test = tests[i];
        it(`should show correct suggestions for ${test.ancientSouls} ancient souls for patch ${test.useBeta ? "e11" : "e10"}`, () => {
            // Make a copy to avoid affecting other tests
            component.savedGame = JSON.parse(JSON.stringify(savedGame));
            component.savedGame.data.ancientSoulsTotal = test.ancientSouls;
            component.useBeta = test.useBeta;
            fixture.detectChanges();

            let exponentialPipe = TestBed.get(ExponentialPipe) as ExponentialPipe;
            let percentPipe = TestBed.get(PercentPipe) as PercentPipe;

            let tables = fixture.debugElement.queryAll(By.css("table"));
            expect(tables.length).toEqual(2);

            let outsiderTable = tables[0];
            let outsiderRows = outsiderTable.queryAll(By.css("tbody tr"));
            expect(outsiderRows.length).toEqual(test.expectedLevels.length);
            expect(outsiderRows.length).toEqual(outsiderOrder.length);

            for (let j = 0; j < outsiderRows.length; j++) {
                let expectedName = outsiderOrder[j];
                let outsiderId = outsiderIdByName[expectedName];

                let cells = outsiderRows[j].children;
                expect(cells.length).toEqual(3);
                expect(getNormalizedTextContent(cells[0])).toEqual(expectedName + ":");
                expect(getNormalizedTextContent(cells[1])).toEqual(exponentialPipe.transform(component.savedGame.data.outsiders.outsiders[outsiderId].level));
                expect(getNormalizedTextContent(cells[2])).toEqual(exponentialPipe.transform(test.expectedLevels[j]));
            }

            let outsiderFooter = outsiderTable.queryAll(By.css("tfoot tr"));
            expect(outsiderFooter.length).toEqual(1);

            let outsiderFooterCells = outsiderFooter[0].children;
            expect(outsiderFooterCells.length).toEqual(3);
            expect(getNormalizedTextContent(outsiderFooterCells[2])).toEqual(exponentialPipe.transform(test.expectedRemaining));

            let endOfTransTable = tables[1];
            let endOfTransTableRows = endOfTransTable.queryAll(By.css("tbody tr"));
            expect(endOfTransTableRows.length).toEqual(4);

            expect(endOfTransTableRows[0].children.length).toEqual(2);
            expect(getNormalizedTextContent(endOfTransTableRows[0].children[1])).toEqual(exponentialPipe.transform(test.newHze));

            expect(endOfTransTableRows[1].children.length).toEqual(2);
            expect(getNormalizedTextContent(endOfTransTableRows[1].children[1])).toEqual(exponentialPipe.transform(new Decimal(test.newHeroSouls)));

            expect(endOfTransTableRows[2].children.length).toEqual(2);
            let expectedNewAncientSoulsText = `${exponentialPipe.transform(test.newAncientSouls)} (+${exponentialPipe.transform(test.newAncientSouls - test.ancientSouls)})`;
            expect(getNormalizedTextContent(endOfTransTableRows[2].children[1])).toEqual(expectedNewAncientSoulsText);

            expect(endOfTransTableRows[3].children.length).toEqual(2);
            expect(getNormalizedTextContent(endOfTransTableRows[3].children[1])).toEqual(percentPipe.transform(test.newTranscendentPower / 100, "1.1-3"));
        });
    }

    function getNormalizedTextContent(element: DebugElement): string {
        let nativeElement: HTMLElement = element.nativeElement;
        return nativeElement.textContent.trim().replace(/\s\s+/g, " ");
    }
});
