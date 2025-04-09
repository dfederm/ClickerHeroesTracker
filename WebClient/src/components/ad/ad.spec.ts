import { Component, Input } from "@angular/core";
import { ComponentFixture, TestBed } from "@angular/core/testing";
import { By } from "@angular/platform-browser";
import { Router, Event as NavigationEvent, NavigationEnd, NavigationCancel } from "@angular/router";
import { Subject } from "rxjs";

import { AdComponent } from "./ad";
import { AdsenseModule } from "ng2-adsense";

describe("AdComponent", () => {
    let fixture: ComponentFixture<AdComponent>;
    let navigationEvents: Subject<NavigationEvent>;
    let timesRendered: number;

    @Component({
        selector: "ng-adsense",
        template: "{{ renderNumber }}",
    })
    class MockAdsenseComponent {
        @Input()
        public adClient: string;

        @Input()
        public adSlot: number;

        public renderNumber: number;

        constructor() {
            timesRendered++;
            this.renderNumber = timesRendered;
        }
    }

    beforeEach(async () => {
        timesRendered = 0;
        navigationEvents = new Subject();
        let router = {
            events: {
                // eslint-disable-next-line import/no-deprecated
                subscribe: navigationEvents.subscribe.bind(navigationEvents),
            },
        };

        await TestBed.configureTestingModule(
            {
                imports: [
                    AdComponent,
                ],
                providers: [
                    { provide: Router, useValue: router },
                ],
            })
            .compileComponents();
        TestBed.overrideComponent(AdComponent, {
            remove: { imports: [ AdsenseModule ]},
            add: { imports: [ MockAdsenseComponent ] },
        });
        fixture = TestBed.createComponent(AdComponent);
    });

    it("should display an ad", () => {
        verifyAd(1);
    });

    it("should reload the ad on navigation", () => {
        verifyAd(1);

        let event = new NavigationEnd(1, "someUrl", "someUrlAfterRedirects");
        navigationEvents.next(event);

        verifyAd(2);
    });

    it("should not reload the ad on an unrelated navigation event", () => {
        verifyAd(1);

        let event = new NavigationCancel(1, "someUrl", "someReason");
        navigationEvents.next(event);

        verifyAd(1);
    });

    function verifyAd(expectedTimesRendered: number): void {
        fixture.detectChanges();

        let ad = fixture.debugElement.query(By.css("ng-adsense"));
        expect(ad).not.toBeNull();

        let mockAdsenseComponent = ad.componentInstance as MockAdsenseComponent;
        expect(mockAdsenseComponent.adClient).toEqual("ca-pub-7807152857287265");
        expect(mockAdsenseComponent.adSlot).toEqual(2070554767);
        expect(ad.nativeElement.textContent).toEqual(expectedTimesRendered.toString());
        expect(timesRendered).toEqual(expectedTimesRendered);
    }
});
