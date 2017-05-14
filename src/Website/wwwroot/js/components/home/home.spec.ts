import { NO_ERRORS_SCHEMA } from "@angular/core";
import { ComponentFixture, TestBed, async } from "@angular/core/testing";
import { By } from "@angular/platform-browser";

import { HomeComponent } from "./home";

describe("HomeComponent", () =>
{
    let component: HomeComponent;
    let fixture: ComponentFixture<HomeComponent>;

    beforeEach(async(() =>
    {
        TestBed.configureTestingModule(
        {
            declarations: [ HomeComponent ],
            schemas:      [ NO_ERRORS_SCHEMA ],
        })
        .compileComponents()
        .then(() =>
        {
            fixture = TestBed.createComponent(HomeComponent);
            component = fixture.componentInstance;
        });
    }));

    it("should display the jumbotron", () =>
    {
        fixture.detectChanges();

        let jumbotron = fixture.debugElement.query(By.css(".jumbotron"));
        expect(jumbotron).not.toBeNull();

        let title = jumbotron.query(By.css("h1"));
        expect(title).not.toBeNull();
        expect(title.nativeElement.textContent).toEqual("Clicker Heroes Tracker");

        let uploadLink = jumbotron.query(By.css("a"));
        expect(uploadLink).not.toBeNull();
        expect(uploadLink.properties["openDialog"]).toEqual(component.UploadDialogComponent);
    });

    it("should display the short changelog", () =>
    {
        fixture.detectChanges();

        let changelogContainer = fixture.debugElement.query(By.css(".col-md-4"));
        expect(changelogContainer).not.toBeNull();

        let changelog = changelogContainer.query(By.css("changelog"));
        expect(changelog).not.toBeNull();
        expect(changelog.properties.isFull).toEqual(false);

        let fullChangelogLink = changelogContainer.query(By.css("a"));
        expect(fullChangelogLink).not.toBeNull();
        expect(fullChangelogLink.attributes.routerLink).toEqual("/news");
    });
});
