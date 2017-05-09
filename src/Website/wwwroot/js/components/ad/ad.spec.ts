import { NO_ERRORS_SCHEMA } from "@angular/core";
import { ComponentFixture, TestBed, async } from "@angular/core/testing";
import { By } from "@angular/platform-browser";

import { AdComponent } from "./ad";

describe("AdComponent", () =>
{
    let component: AdComponent;
    let fixture: ComponentFixture<AdComponent>;

    beforeEach(async(() =>
    {
        TestBed.configureTestingModule(
        {
            declarations: [ AdComponent ],
            schemas: [ NO_ERRORS_SCHEMA ],
        })
        .compileComponents()
        .then(() =>
        {
            fixture = TestBed.createComponent(AdComponent);
            component = fixture.componentInstance;
        });
    }));

    it("should display an ad", () =>
    {
        fixture.detectChanges();

        let ad = fixture.debugElement.query(By.css("ins.adsbygoogle"));
        expect(ad).not.toBeNull();
    });
});
