import { ComponentFixture, TestBed } from "@angular/core/testing";
import { By } from "@angular/platform-browser";

import { NewsComponent } from "./news";
import { Component, Input } from "@angular/core";
import { ChangelogComponent } from "../changelog/changelog";

describe("NewsComponent", () => {
    let fixture: ComponentFixture<NewsComponent>;

    @Component({ selector: "changelog", template: "", standalone: true })
    class MockChangelogComponent {
        @Input()
        public showDates: boolean;
    }

    beforeEach(async () => {
        await TestBed.configureTestingModule(
            {
                imports: [
                    NewsComponent,
                ],
            })
            .compileComponents();
        TestBed.overrideComponent(NewsComponent, {
            remove: { imports: [ ChangelogComponent ]},
            add: { imports: [ MockChangelogComponent ] },
        });

        fixture = TestBed.createComponent(NewsComponent);
    });

    it("should display a changelog with dates", () => {
        fixture.detectChanges();

        let changelogContainer = fixture.debugElement.query(By.css(".container"));
        expect(changelogContainer).not.toBeNull();

        let changelog = changelogContainer.query(By.css("changelog"))?.componentInstance as MockChangelogComponent;
        expect(changelog).not.toBeNull();
        expect(changelog.showDates).toEqual(true);
    });
});
