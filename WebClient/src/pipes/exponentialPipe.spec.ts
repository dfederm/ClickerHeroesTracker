import { NO_ERRORS_SCHEMA, Component } from "@angular/core";
import { ComponentFixture, TestBed, async } from "@angular/core/testing";
import Decimal from "decimal.js";

import { ExponentialPipe } from "./exponentialPipe";

describe("ExponentialPipe", () => {
    // TODO mock this out
    const scientificNotationThreshold = 100000;

    let fixture: ComponentFixture<MockComponent>;

    @Component({
        template: "{{ value | exponential }}",
    })
    class MockComponent {
        public value: number | decimal.Decimal;
    }

    beforeEach(async(() => {
        fixture = TestBed.configureTestingModule(
            {
                declarations:
                [
                    ExponentialPipe,
                    MockComponent,
                ],
                schemas: [NO_ERRORS_SCHEMA],
            })
            .createComponent(MockComponent);

        // Initial binding
        fixture.detectChanges();
    }));

    it("should display '0' when the value is undefined", () => {
        fixture.componentInstance.value = undefined;
        fixture.detectChanges();

        expect((fixture.nativeElement as HTMLElement).textContent).toEqual("0");
    });

    it("should display the full number when it is below the scientific notation threshold and scientific notation is on", () => {
        fixture.componentInstance.value = scientificNotationThreshold;
        fixture.detectChanges();

        expect((fixture.nativeElement as HTMLElement).textContent).toEqual(fixture.componentInstance.value.toLocaleString());
    });

    it("should display a formatted number when it is above the scientific notation threshold and scientific notation is on", () => {
        fixture.componentInstance.value = scientificNotationThreshold + 1;
        fixture.detectChanges();

        expect((fixture.nativeElement as HTMLElement).textContent).toEqual(fixture.componentInstance.value.toExponential(3));
    });

    it("should display the full Decimal when it is below the scientific notation threshold and scientific notation is on", () => {
        fixture.componentInstance.value = new Decimal(scientificNotationThreshold);
        fixture.detectChanges();

        expect((fixture.nativeElement as HTMLElement).textContent).toEqual(fixture.componentInstance.value.toFormat());
    });

    it("should display a formatted Decimal when it is above the scientific notation threshold and scientific notation is on", () => {
        fixture.componentInstance.value = new Decimal(scientificNotationThreshold + 1);
        fixture.detectChanges();

        expect((fixture.nativeElement as HTMLElement).textContent).toEqual(fixture.componentInstance.value.toExponential(3));
    });

    // TODO add test for when scientific notation is off once user settings are properly fetched
});
