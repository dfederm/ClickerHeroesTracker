import { Component } from "@angular/core";
import { ComponentFixture, TestBed } from "@angular/core/testing";
import { Decimal } from "decimal.js";
import { BehaviorSubject } from "rxjs";

import { ExponentialPipe } from "./exponentialPipe";
import { SettingsService, IUserSettings } from "../services/settingsService/settingsService";

describe("ExponentialPipe", () => {
    let fixture: ComponentFixture<MockComponent>;

    let settingsWithScientificNotation: IUserSettings = {
        playStyle: "hybrid",
        useScientificNotation: true,
        scientificNotationThreshold: 1000000,
        useLogarithmicGraphScale: true,
        logarithmicGraphScaleThreshold: 1000000,
        hybridRatio: 2,
        theme: "light",
        shouldLevelSkillAncients: false,
        skillAncientBaseAncient: 17,
        skillAncientLevelDiff: 0,
        graphSpacingType: "time",
    };

    let settingsWithoutScientificNotation: IUserSettings = {
        playStyle: "hybrid",
        useScientificNotation: false,
        scientificNotationThreshold: 1000000,
        useLogarithmicGraphScale: true,
        logarithmicGraphScaleThreshold: 1000000,
        hybridRatio: 2,
        theme: "light",
        shouldLevelSkillAncients: false,
        skillAncientBaseAncient: 17,
        skillAncientLevelDiff: 0,
        graphSpacingType: "time",
    };

    let settingsSubject = new BehaviorSubject(settingsWithScientificNotation);

    @Component({
    template: "{{ value | exponential }}",
    selector: "mock",
    imports: [
        ExponentialPipe,
    ]
})
    class MockComponent {
        public value: number | Decimal;
    }

    beforeEach(() => {
        let settingsService = { settings: () => settingsSubject };
        fixture = TestBed.configureTestingModule(
            {
                imports: [
                    ExponentialPipe,
                    MockComponent,
                ],
                providers: [
                    { provide: SettingsService, useValue: settingsService },
                ],
            })
            .createComponent(MockComponent);

        // Initial binding
        fixture.detectChanges();
    });

    it("should display '0' when the value is undefined", () => {
        fixture.componentInstance.value = undefined;
        fixture.detectChanges();

        expect((fixture.nativeElement as HTMLElement).textContent).toEqual("0");
    });

    it("should display the full number when it is below the scientific notation threshold and scientific notation is on", () => {
        settingsSubject.next(settingsWithScientificNotation);
        fixture.componentInstance.value = settingsWithScientificNotation.scientificNotationThreshold;
        fixture.detectChanges();

        expect((fixture.nativeElement as HTMLElement).textContent).toEqual(fixture.componentInstance.value.toLocaleString());
    });

    it("should display a formatted number when it is above the scientific notation threshold and scientific notation is on", () => {
        settingsSubject.next(settingsWithScientificNotation);
        fixture.componentInstance.value = settingsWithScientificNotation.scientificNotationThreshold + 1;
        fixture.detectChanges();

        expect((fixture.nativeElement as HTMLElement).textContent).toEqual(fixture.componentInstance.value.toExponential(3));
    });

    it("should display the full number when it is above the scientific notation threshold and scientific notation is off", () => {
        settingsSubject.next(settingsWithoutScientificNotation);
        fixture.componentInstance.value = settingsWithoutScientificNotation.scientificNotationThreshold + 1;
        fixture.detectChanges();

        expect((fixture.nativeElement as HTMLElement).textContent).toEqual(fixture.componentInstance.value.toLocaleString());
    });

    it("should display the full Decimal when it is below the scientific notation threshold and scientific notation is on", () => {
        settingsSubject.next(settingsWithScientificNotation);
        fixture.componentInstance.value = new Decimal(settingsWithScientificNotation.scientificNotationThreshold);
        fixture.detectChanges();

        expect((fixture.nativeElement as HTMLElement).textContent).toEqual(fixture.componentInstance.value.toFormat(0, 0));
    });

    it("should display a formatted Decimal when it is above the scientific notation threshold and scientific notation is on", () => {
        settingsSubject.next(settingsWithScientificNotation);
        fixture.componentInstance.value = new Decimal(settingsWithScientificNotation.scientificNotationThreshold + 1);
        fixture.detectChanges();

        expect((fixture.nativeElement as HTMLElement).textContent).toEqual(fixture.componentInstance.value.toExponential(3));
    });

    it("should display the full Decimal when it is above the scientific notation threshold and scientific notation is off", () => {
        settingsSubject.next(settingsWithoutScientificNotation);
        fixture.componentInstance.value = new Decimal(settingsWithoutScientificNotation.scientificNotationThreshold + 1);
        fixture.detectChanges();

        expect((fixture.nativeElement as HTMLElement).textContent).toEqual(fixture.componentInstance.value.toFormat(0, 0));
    });
});
