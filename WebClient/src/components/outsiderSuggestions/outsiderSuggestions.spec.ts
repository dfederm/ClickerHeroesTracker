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

    const tests: { ancientSouls: number, focusBorb: boolean, expectedLevels: [number, number, number, number, number, number, number, number, number], expectedRemaining: number }[] = [
        { ancientSouls: 0, focusBorb: false, expectedLevels: [0, 0, 0, 0, 0, 0, 0, 0, 0], expectedRemaining: 0 },
        { ancientSouls: 0, focusBorb: true, expectedLevels: [0, 0, 0, 0, 0, 0, 0, 0, 0], expectedRemaining: 0 },
        { ancientSouls: 1, focusBorb: false, expectedLevels: [0, 0, 0, 0, 1, 0, 0, 0, 0], expectedRemaining: 0 },
        { ancientSouls: 1, focusBorb: true, expectedLevels: [0, 0, 0, 0, 1, 0, 0, 0, 0], expectedRemaining: 0 },
        { ancientSouls: 10, focusBorb: false, expectedLevels: [0, 0, 3, 3, 1, 0, 0, 0, 0], expectedRemaining: 0 },
        { ancientSouls: 10, focusBorb: true, expectedLevels: [0, 0, 3, 3, 1, 0, 0, 0, 0], expectedRemaining: 0 },
        { ancientSouls: 100, focusBorb: false, expectedLevels: [0, 2, 15, 9, 2, 2, 2, 2, 2], expectedRemaining: 22 },
        { ancientSouls: 100, focusBorb: true, expectedLevels: [0, 2, 15, 9, 2, 2, 2, 2, 2], expectedRemaining: 22 },
        { ancientSouls: 1000, focusBorb: false, expectedLevels: [0, 15, 150, 27, 8, 9, 9, 9, 9], expectedRemaining: 136 },
        { ancientSouls: 1000, focusBorb: true, expectedLevels: [0, 15, 150, 27, 8, 9, 9, 9, 9], expectedRemaining: 136 },
        { ancientSouls: 10000, focusBorb: false, expectedLevels: [0, 82, 1500, 56, 27, 31, 10, 10, 20], expectedRemaining: 2307 },
        { ancientSouls: 10000, focusBorb: true, expectedLevels: [0, 82, 1500, 56, 55, 31, 10, 10, 20], expectedRemaining: 1145 },
        { ancientSouls: 100000, focusBorb: false, expectedLevels: [0, 0, 0, 0, 245, 0, 0, 0, 0], expectedRemaining: 69865 },
        { ancientSouls: 100000, focusBorb: true, expectedLevels: [0, 0, 0, 0, 245, 0, 0, 0, 0], expectedRemaining: 69865 },
    ];
    for (let i = 0; i < tests.length; i++) {
        let test = tests[i];
        it(`should show correct suggestions for ${test.ancientSouls} ancient souls and ${test.focusBorb ? "" : "not "}focusing borb`, () => {
            // Make a copy to avoid affecting other tests
            component.savedGame = JSON.parse(JSON.stringify(savedGame));
            component.savedGame.data.ancientSoulsTotal = test.ancientSouls;
            component.focusBorb = test.focusBorb;
            fixture.detectChanges();

            let exponentialPipe = TestBed.get(ExponentialPipe) as ExponentialPipe;

            let table = fixture.debugElement.query(By.css("table"));
            expect(table).not.toBeNull();

            let rows = table.queryAll(By.css("tbody tr"));
            expect(rows.length).toEqual(test.expectedLevels.length);
            expect(rows.length).toEqual(outsiderOrder.length);

            for (let j = 0; j < rows.length; j++) {
                let expectedName = outsiderOrder[j];
                let outsiderId = outsiderIdByName[expectedName];

                let cells = rows[j].children;
                expect(cells.length).toEqual(3);
                expect(getNormalizedTextContent(cells[0])).toEqual(expectedName + ":");
                expect(getNormalizedTextContent(cells[1])).toEqual(exponentialPipe.transform(component.savedGame.data.outsiders.outsiders[outsiderId].level));
                expect(getNormalizedTextContent(cells[2])).toEqual(exponentialPipe.transform(test.expectedLevels[j]));
            }

            let footer = table.queryAll(By.css("tfoot tr"));
            expect(footer.length).toEqual(1);

            let footerCells = footer[0].children;
            expect(footerCells.length).toEqual(3);
            expect(getNormalizedTextContent(footerCells[2])).toEqual(exponentialPipe.transform(test.expectedRemaining));
        });
    }

    function getNormalizedTextContent(element: DebugElement): string {
        let nativeElement: HTMLElement = element.nativeElement;
        return nativeElement.textContent.trim().replace(/\s\s+/g, " ");
    }
});
