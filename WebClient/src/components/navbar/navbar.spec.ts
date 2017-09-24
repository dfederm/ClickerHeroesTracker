import { NO_ERRORS_SCHEMA, Type } from "@angular/core";
import { ComponentFixture, TestBed, async } from "@angular/core/testing";
import { By } from "@angular/platform-browser";
import { NgbModal } from "@ng-bootstrap/ng-bootstrap";
import { BehaviorSubject } from "rxjs/BehaviorSubject";

import { NavbarComponent } from "./navbar";
import { AuthenticationService } from "../../services/authenticationService/authenticationService";

describe("NavbarComponent", () => {
    let component: NavbarComponent;
    let fixture: ComponentFixture<NavbarComponent>;

    beforeEach(async(() => {
        let authenticationService = { isLoggedIn: (): void => void 0, logOut: (): void => void 0 };
        let modalService = { open: (): void => void 0 };

        TestBed.configureTestingModule(
            {
                declarations: [NavbarComponent],
                providers:
                [
                    { provide: AuthenticationService, useValue: authenticationService },
                    { provide: NgbModal, useValue: modalService },
                ],
                schemas: [NO_ERRORS_SCHEMA],
            })
            .compileComponents()
            .then(() => {
                fixture = TestBed.createComponent(NavbarComponent);
                component = fixture.componentInstance;
            });
    }));

    it("should display the anonymous nav bar when the user is not logged in", async(() => {
        let authenticationService = TestBed.get(AuthenticationService) as AuthenticationService;
        spyOn(authenticationService, "isLoggedIn").and.returnValue(new BehaviorSubject(false));

        fixture.detectChanges();
        fixture.whenStable().then(() => {
            fixture.detectChanges();

            let navItems = fixture.debugElement.queryAll(By.css(".nav-item"));
            expect(navItems).not.toBeNull();
            expect(navItems.length).toEqual(5);

            let expectedLinks: { text: string, url?: string, dialog?: Type<{}> }[] =
                [
                    { text: "Upload", dialog: component.UploadDialogComponent },
                    { text: "What's New", url: "/news" },
                    { text: "Feedback" },
                    { text: "Register", dialog: component.RegisterDialogComponent },
                    { text: "Log in", dialog: component.LogInDialogComponent },
                ];
            for (let i = 0; i < navItems.length; i++) {
                let link = navItems[i].query(By.css(".nav-link"));
                expect(link).not.toBeNull();

                let expectations = expectedLinks[i];
                if (expectations.text) {
                    expect(link.nativeElement.innerText).toEqual(expectations.text);
                }

                if (expectations.url) {
                    expect(link.attributes.routerLink).toEqual(expectations.url);
                }

                if (expectations.dialog) {
                    expect(link.properties.openDialog).toEqual(expectations.dialog);
                }
            }
        });
    }));

    it("should display the authenticated nav bar when the user is logged in", async(() => {
        let authenticationService = TestBed.get(AuthenticationService) as AuthenticationService;
        spyOn(authenticationService, "isLoggedIn").and.returnValue(new BehaviorSubject(true));

        fixture.detectChanges();
        fixture.whenStable().then(() => {
            fixture.detectChanges();

            let navItems = fixture.debugElement.queryAll(By.css(".nav-item"));
            expect(navItems).not.toBeNull();
            expect(navItems.length).toEqual(7);

            let expectedLinks: { text: string, url?: string, hasClickHandler?: boolean, dialog?: Type<{}> }[] =
                [
                    { text: "Dashboard", url: "/dashboard" },
                    { text: "Upload", dialog: component.UploadDialogComponent },
                    { text: "Clans", url: "/clans" },
                    { text: "What's New", url: "/news" },
                    { text: "Feedback" },
                    { text: "Hello User!" },
                    { text: "Log off", hasClickHandler: true },
                ];
            for (let i = 0; i < navItems.length; i++) {
                let link = navItems[i].query(By.css(".nav-link"));
                expect(link).not.toBeNull();

                let expectations = expectedLinks[i];
                if (expectations.text) {
                    expect(link.nativeElement.innerText).toEqual(expectations.text);
                }

                if (expectations.url) {
                    expect(link.attributes.routerLink).toEqual(expectations.url);
                }

                if (expectations.hasClickHandler) {
                    expect(link.listeners).not.toBeNull();
                    expect(link.listeners.length).toEqual(1);
                    expect(link.listeners[0].name).toEqual("click");
                }

                if (expectations.dialog) {
                    expect(link.properties.openDialog).toEqual(expectations.dialog);
                }
            }
        });
    }));

    it("should update isLoggedIn when the authenticationService updates", async(() => {
        let isLoggedIn = new BehaviorSubject(false);
        let authenticationService = TestBed.get(AuthenticationService) as AuthenticationService;
        spyOn(authenticationService, "isLoggedIn").and.returnValue(isLoggedIn);

        fixture.detectChanges();
        fixture.whenStable()
            .then(() => {
                expect(component.isLoggedIn).toEqual(false);
                isLoggedIn.next(true);
                return fixture.whenStable();
            })
            .then(() => {
                expect(component.isLoggedIn).toEqual(true);
                isLoggedIn.next(true);
                return fixture.whenStable();
            })
            .then(() => {
                expect(component.isLoggedIn).toEqual(true);
                isLoggedIn.next(false);
                return fixture.whenStable();
            })
            .then(() => {
                expect(component.isLoggedIn).toEqual(false);
            });
    }));

    it("should be able to collape and expand the navbar", () => {
        let authenticationService = TestBed.get(AuthenticationService) as AuthenticationService;
        spyOn(authenticationService, "isLoggedIn").and.returnValue(new BehaviorSubject(false));

        let toggler = fixture.debugElement.query(By.css(".navbar-toggler"));

        // Initial state
        fixture.detectChanges();
        expect(component.isCollapsed).toEqual(true);

        toggler.nativeElement.click();
        fixture.detectChanges();
        expect(component.isCollapsed).toEqual(false);

        toggler.nativeElement.click();
        fixture.detectChanges();
        expect(component.isCollapsed).toEqual(true);

        toggler.nativeElement.click();
        fixture.detectChanges();
        expect(component.isCollapsed).toEqual(false);
    });

    it("should log out after clicking the log out button", () => {
        let authenticationService = TestBed.get(AuthenticationService) as AuthenticationService;
        spyOn(authenticationService, "isLoggedIn").and.returnValue(new BehaviorSubject(true));

        fixture.detectChanges();

        spyOn(authenticationService, "logOut");

        let logOutLink = fixture.debugElement.queryAll(By.css(".nav-link"))[6];
        logOutLink.nativeElement.click();

        expect(authenticationService.logOut).toHaveBeenCalled();
    });
});
