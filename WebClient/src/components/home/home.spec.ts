import { NO_ERRORS_SCHEMA } from "@angular/core";
import { ComponentFixture, TestBed, async } from "@angular/core/testing";
import { By } from "@angular/platform-browser";

import { HomeComponent } from "./home";
import { AuthenticationService, IUserInfo } from "../../services/authenticationService/authenticationService";
import { BehaviorSubject } from "rxjs";

describe("HomeComponent", () => {
    let component: HomeComponent;
    let fixture: ComponentFixture<HomeComponent>;
    let userInfo: BehaviorSubject<IUserInfo>;

    const loggedInUser: IUserInfo = {
        isLoggedIn: true,
        id: "someId",
        username: "someUsername",
        email: "someEmail",
    };

    const notLoggedInUser: IUserInfo = {
        isLoggedIn: false,
    };

    beforeEach(async(() => {
        userInfo = new BehaviorSubject(notLoggedInUser);
        let authenticationService = { userInfo: () => userInfo };

        TestBed.configureTestingModule(
            {
                declarations: [HomeComponent],
                schemas: [NO_ERRORS_SCHEMA],
                providers: [
                    { provide: AuthenticationService, useValue: authenticationService },
                ],
            })
            .compileComponents()
            .then(() => {
                fixture = TestBed.createComponent(HomeComponent);
                component = fixture.componentInstance;
            });
    }));

    it("should display the jumbotron", () => {
        fixture.detectChanges();

        let jumbotron = fixture.debugElement.query(By.css(".jumbotron"));
        expect(jumbotron).not.toBeNull();

        let title = jumbotron.query(By.css("h1"));
        expect(title).not.toBeNull();
        expect(title.nativeElement.textContent).toEqual("Clicker Heroes Tracker");

        let uploadLink = jumbotron.query(By.css("a"));
        expect(uploadLink).not.toBeNull();
        expect(uploadLink.properties.openDialog).toEqual(component.UploadDialogComponent);
    });

    it("should display the short changelog", () => {
        fixture.detectChanges();

        let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));
        let changelogContainer = containers[0];

        let changelog = changelogContainer.query(By.css("changelog"));
        expect(changelog).not.toBeNull();
        expect(changelog.properties.showDates).toEqual(false);
        expect(changelog.properties.maxEntries).toEqual(5);

        let fullChangelogLink = changelogContainer.query(By.css("a"));
        expect(fullChangelogLink).not.toBeNull();
        expect(fullChangelogLink.attributes.routerLink).toEqual("/news");
    });

    it("should not display recent uploads to an anonymous user", () => {
        fixture.detectChanges();

        let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));
        expect(containers.length).toEqual(1);
    });

    it("should display recent uploads to an authenticated user", () => {
        userInfo.next(loggedInUser);
        fixture.detectChanges();

        let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));
        expect(containers.length).toEqual(2);

        let uploadsContainer = containers[1];

        let uploadsTable = uploadsContainer.query(By.css("uploadsTable"));
        expect(uploadsTable).not.toBeNull();
        expect(uploadsTable.properties.userName).toEqual("someUsername");
        expect(uploadsTable.properties.count).toEqual(5);

        let allUploadsLink = uploadsContainer.query(By.css("a"));
        expect(allUploadsLink).not.toBeNull();
        expect(allUploadsLink.properties.routerLink).toEqual("/users/someUsername/uploads");
    });
});
