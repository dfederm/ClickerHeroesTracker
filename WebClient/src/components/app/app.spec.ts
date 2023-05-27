import { NO_ERRORS_SCHEMA } from "@angular/core";
import { HttpHeaders } from "@angular/common/http";
import { ComponentFixture, TestBed } from "@angular/core/testing";
import { Subject } from "rxjs";

import { AppComponent } from "./app";
import { LoggingService } from "../../services/loggingService/loggingService";
import { SettingsService, IUserSettings } from "../../services/settingsService/settingsService";
import { AuthenticationService, IUserInfo } from "../../services/authenticationService/authenticationService";

describe("AppComponent", () => {
    let fixture: ComponentFixture<AppComponent>;

    let settingsSubject = new Subject<IUserSettings>();
    let userInfoSubject = new Subject<IUserInfo>();
    let loggingService: LoggingService;

    beforeEach(async () => {
        let settingsService = { settings: () => settingsSubject };
        let authenticationService = {
            getAuthHeaders: () => Promise.resolve(new HttpHeaders()),
            userInfo: () => userInfoSubject,
        };

        loggingService = jasmine.createSpyObj("loggingService", ["setAuthenticatedUserContext", "clearAuthenticatedUserContext"]);

        await TestBed.configureTestingModule(
            {
                declarations: [AppComponent],
                providers: [
                    { provide: SettingsService, useValue: settingsService },
                    { provide: AuthenticationService, useValue: authenticationService },
                    { provide: LoggingService, useValue: loggingService },
                ],
                schemas: [NO_ERRORS_SCHEMA],
            })
            .compileComponents();

        fixture = TestBed.createComponent(AppComponent);

        fixture.detectChanges();
        await fixture.whenStable();
    });

    it("should update the stylesheet when the theme changes", () => {
        let stylesheetElement = document.createElement("link");
        stylesheetElement.href = "";
        stylesheetElement.id = "bootstrapStylesheet";
        document.body.appendChild(stylesheetElement);

        function updateTheme(theme: string): void {
            let settings = JSON.parse(JSON.stringify(SettingsService.defaultSettings));
            settings.theme = theme;
            settingsSubject.next(settings);
            fixture.detectChanges();
        }

        try {
            updateTheme("light");
            expect(stylesheetElement.href).toEqual(AppComponent.themeCssUrls.light);

            updateTheme("dark");
            expect(stylesheetElement.href).toEqual(AppComponent.themeCssUrls.dark);

            updateTheme("notATheme");
            expect(stylesheetElement.href).toEqual(AppComponent.themeCssUrls[AppComponent.defaultTheme]);
        } finally {
            stylesheetElement.remove();
        }
    });

    it("should set the user context when the user info changes", () => {
        let numLogins = 0;
        function logIn(): void {
            updateUserInfo({
                isLoggedIn: true,
                id: "someId" + numLogins,
                username: "someUsername" + numLogins,
                email: "someEmail" + numLogins,
            });
            numLogins++;
        }

        function logOut(): void {
            updateUserInfo({
                isLoggedIn: false,
            });
        }

        function updateUserInfo(userInfo: IUserInfo): void {
            (loggingService.setAuthenticatedUserContext as jasmine.Spy).calls.reset();
            (loggingService.clearAuthenticatedUserContext as jasmine.Spy).calls.reset();

            userInfoSubject.next(userInfo);
            fixture.detectChanges();
        }

        logOut();
        expect(loggingService.setAuthenticatedUserContext).not.toHaveBeenCalled();
        expect(loggingService.clearAuthenticatedUserContext).toHaveBeenCalled();

        logIn();
        expect(loggingService.setAuthenticatedUserContext).toHaveBeenCalledWith("someUsername0");
        expect(loggingService.clearAuthenticatedUserContext).not.toHaveBeenCalled();

        logIn();
        expect(loggingService.setAuthenticatedUserContext).toHaveBeenCalledWith("someUsername1");
        expect(loggingService.clearAuthenticatedUserContext).not.toHaveBeenCalled();

        logOut();
        expect(loggingService.setAuthenticatedUserContext).not.toHaveBeenCalled();
        expect(loggingService.clearAuthenticatedUserContext).toHaveBeenCalled();

        logIn();
        expect(loggingService.setAuthenticatedUserContext).toHaveBeenCalledWith("someUsername2");
        expect(loggingService.clearAuthenticatedUserContext).not.toHaveBeenCalled();
    });
});
