import { NO_ERRORS_SCHEMA } from "@angular/core";
import { ComponentFixture, TestBed } from "@angular/core/testing";
import { By } from "@angular/platform-browser";

import { NewsComponent } from "./news";

describe("NewsComponent", () => {
    let fixture: ComponentFixture<NewsComponent>;

    beforeEach(async () => {
        await TestBed.configureTestingModule(
            {
                declarations: [NewsComponent],
                schemas: [NO_ERRORS_SCHEMA],
            })
            .compileComponents();

        fixture = TestBed.createComponent(NewsComponent);
    });

    it("should display a changelog with dates", () => {
        fixture.detectChanges();

        let changelogContainer = fixture.debugElement.query(By.css(".container"));
        expect(changelogContainer).not.toBeNull();

        let changelog = changelogContainer.query(By.css("changelog"));
        expect(changelog).not.toBeNull();
        expect(changelog.properties.showDates).toEqual(true);
    });
});
