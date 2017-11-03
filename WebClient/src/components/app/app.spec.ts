import { NO_ERRORS_SCHEMA } from "@angular/core";
import { ComponentFixture, TestBed } from "@angular/core/testing";
import { Subject } from "rxjs";

import { AppComponent } from "./app";
import { AppInsightsService } from "@markpieszak/ng-application-insights";
import { SettingsService, IUserSettings } from "../../services/settingsService/settingsService";
import { AuthenticationService, IUserInfo } from "../../services/authenticationService/authenticationService";

describe("BannerComponent", () => {
    let fixture: ComponentFixture<AppComponent>;

    let settingsSubject = new Subject<IUserSettings>();
    let userInfoSubject = new Subject<IUserInfo>();
    let appInsights: AppInsightsService;

    beforeEach(done => {
        let settingsService = { settings: () => settingsSubject };
        let authenticationService = { userInfo: () => userInfoSubject };

        appInsights = jasmine.createSpyObj("appInsights", ["setAuthenticatedUserContext", "clearAuthenticatedUserContext"]);

        TestBed.configureTestingModule(
            {
                declarations: [AppComponent],
                providers:
                    [
                        { provide: SettingsService, useValue: settingsService },
                        { provide: AuthenticationService, useValue: authenticationService },
                        { provide: AppInsightsService, useValue: appInsights },
                    ],
                schemas: [NO_ERRORS_SCHEMA],
            })
            .compileComponents()
            .then(() => {
                fixture = TestBed.createComponent(AppComponent);

                fixture.detectChanges();
            })
            .then(done)
            .catch(done.fail);
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
            (appInsights.setAuthenticatedUserContext as jasmine.Spy).calls.reset();
            (appInsights.clearAuthenticatedUserContext as jasmine.Spy).calls.reset();

            userInfoSubject.next(userInfo);
            fixture.detectChanges();
        }

        logOut();
        expect(appInsights.setAuthenticatedUserContext).not.toHaveBeenCalled();
        expect(appInsights.clearAuthenticatedUserContext).toHaveBeenCalled();

        logIn();
        expect(appInsights.setAuthenticatedUserContext).toHaveBeenCalledWith("someUsername0");
        expect(appInsights.clearAuthenticatedUserContext).not.toHaveBeenCalled();

        logIn();
        expect(appInsights.setAuthenticatedUserContext).toHaveBeenCalledWith("someUsername1");
        expect(appInsights.clearAuthenticatedUserContext).not.toHaveBeenCalled();

        logOut();
        expect(appInsights.setAuthenticatedUserContext).not.toHaveBeenCalled();
        expect(appInsights.clearAuthenticatedUserContext).toHaveBeenCalled();

        logIn();
        expect(appInsights.setAuthenticatedUserContext).toHaveBeenCalledWith("someUsername2");
        expect(appInsights.clearAuthenticatedUserContext).not.toHaveBeenCalled();
    });
});
