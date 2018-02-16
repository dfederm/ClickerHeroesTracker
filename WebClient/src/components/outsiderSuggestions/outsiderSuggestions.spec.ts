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

// To generate these tests is to open https://driej.github.io/Clicker-Heroes-Outsiders/, open the console, and run test()
// tslint:disable-next-line:no-require-imports no-var-requires
export const testData: ITestData[] = require("./outsiderSuggestions.testdata.json");

interface ITestData {
    ancientSouls: number;
    useBeta: boolean;
    expectedLevels: [number, number, number, number, number, number, number, number, number];
    expectedRemaining: number;
    newHze: number;
    newLogHeroSouls: number;
    newAncientSouls: number;
    newTranscendentPower: number;
}

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

    for (let i = 0; i < testData.length; i++) {
        let test = testData[i];
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
            expect(getNormalizedTextContent(endOfTransTableRows[1].children[1])).toEqual(exponentialPipe.transform(Decimal.pow(10, test.newLogHeroSouls)));

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
