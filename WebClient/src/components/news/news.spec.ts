import { NO_ERRORS_SCHEMA } from "@angular/core";
import { ComponentFixture, TestBed, async } from "@angular/core/testing";
import { By } from "@angular/platform-browser";

import { NewsComponent } from "./news";

describe("NewsComponent", () => {
    let component: NewsComponent;
    let fixture: ComponentFixture<NewsComponent>;

    beforeEach(async(() => {
        TestBed.configureTestingModule(
            {
                declarations: [NewsComponent],
                schemas: [NO_ERRORS_SCHEMA],
            })
            .compileComponents()
            .then(() => {
                fixture = TestBed.createComponent(NewsComponent);
                component = fixture.componentInstance;
            });
    }));

    it("should display the full changelog", () => {
        fixture.detectChanges();

        let changelogContainer = fixture.debugElement.query(By.css(".container"));
        expect(changelogContainer).not.toBeNull();

        let changelog = changelogContainer.query(By.css("changelog"));
        expect(changelog).not.toBeNull();
        expect(changelog.properties.isFull).toEqual(true);
    });
});
