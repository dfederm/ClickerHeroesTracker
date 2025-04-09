import { ComponentFixture, TestBed } from "@angular/core/testing";
import { By } from "@angular/platform-browser";

import { HomeComponent } from "./home";
import { AuthenticationService, IUserInfo } from "../../services/authenticationService/authenticationService";
import { BehaviorSubject } from "rxjs";
import { provideRouter } from "@angular/router";
import { Component, Input } from "@angular/core";
import { ChangelogComponent } from "../changelog/changelog";
import { UploadsTableComponent } from "../uploadsTable/uploadsTable";
import { OpenDialogDirective } from "src/directives/openDialog/openDialog";

describe("HomeComponent", () => {
    let component: HomeComponent;
    let fixture: ComponentFixture<HomeComponent>;
    let userInfo: BehaviorSubject<IUserInfo>;

    @Component({ selector: "changelog", template: "", standalone: true })
    class MockChangelogComponent {
        @Input()
        public showDates: boolean;

        @Input()
        public maxEntries: number;
    }

    @Component({ selector: "uploadsTable", template: "", standalone: true })
    class MockUploadsTableComponent {
        @Input()
        public userName: string;

        @Input()
        public count: number;
    }

    const loggedInUser: IUserInfo = {
        isLoggedIn: true,
        id: "someId",
        username: "someUsername",
        email: "someEmail",
    };

    const notLoggedInUser: IUserInfo = {
        isLoggedIn: false,
    };

    beforeEach(async () => {
        userInfo = new BehaviorSubject(notLoggedInUser);
        let authenticationService = { userInfo: () => userInfo };

        await TestBed.configureTestingModule(
            {
                imports: [HomeComponent],
                providers: [
                    provideRouter([]),
                    { provide: AuthenticationService, useValue: authenticationService },
                ],
            })
            .compileComponents();
        TestBed.overrideComponent(HomeComponent, {
            remove: { imports: [ChangelogComponent, UploadsTableComponent]},
            add: { imports: [MockChangelogComponent, MockUploadsTableComponent] },
        });

        fixture = TestBed.createComponent(HomeComponent);
        component = fixture.componentInstance;
    });

    it("should display the jumbotron", () => {
        fixture.detectChanges();

        let jumbotron = fixture.debugElement.query(By.css(".row"));
        expect(jumbotron).not.toBeNull();

        let title = jumbotron.query(By.css("h1"));
        expect(title).not.toBeNull();
        expect(title.nativeElement.textContent).toEqual("Clicker Heroes Tracker");

        let uploadLink = jumbotron.query(By.css("a"));
        expect(uploadLink).not.toBeNull();
        let openDialogDirective = uploadLink.injector.get(OpenDialogDirective) as OpenDialogDirective;
        expect(openDialogDirective.openDialog).toEqual(component.UploadDialogComponent);
    });

    it("should display the short changelog", () => {
        fixture.detectChanges();

        let containers = fixture.debugElement.queryAll(By.css(".col-md-6"));
        let changelogContainer = containers[0];

        let changelog = changelogContainer.query(By.css("changelog"))?.componentInstance as MockChangelogComponent;
        expect(changelog).not.toBeNull();
        expect(changelog.showDates).toEqual(false);
        expect(changelog.maxEntries).toEqual(5);

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

        let uploadsTable = uploadsContainer.query(By.css("uploadsTable"))?.componentInstance as MockUploadsTableComponent;
        expect(uploadsTable).not.toBeNull();
        expect(uploadsTable.userName).toEqual("someUsername");
        expect(uploadsTable.count).toEqual(5);

        let allUploadsLink = uploadsContainer.query(By.css("a"));
        expect(allUploadsLink).not.toBeNull();
        expect(allUploadsLink.attributes.href).toEqual("/users/someUsername/uploads");
    });
});
