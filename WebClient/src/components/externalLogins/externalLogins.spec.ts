import { ComponentFixture, TestBed } from "@angular/core/testing";
import { By } from "@angular/platform-browser";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { Component, DebugElement, Input } from "@angular/core";

import { ExternalLoginsComponent, IErrorResponse } from "./externalLogins";
import { AuthenticationService, IUserInfo } from "../../services/authenticationService/authenticationService";
import { UserService, IUserLogins } from "../../services/userService/userService";
import { BehaviorSubject } from "rxjs";
import { AuthenticationResult } from "@azure/msal-browser";
import { NgxSpinnerModule, NgxSpinnerService } from "ngx-spinner";

describe("ExternalLoginsComponent", () => {
    let component: ExternalLoginsComponent;

    @Component({ selector: "ngx-spinner", template: "", standalone: true })
    class MockNgxSpinnerComponent {
        @Input()
        public fullScreen: boolean;
    }

    let fixture: ComponentFixture<ExternalLoginsComponent>;
    let userInfo: BehaviorSubject<IUserInfo>;

    const loggedInUser: IUserInfo = {
        isLoggedIn: true,
        id: "someId",
        username: "someUsername",
        email: "someEmail",
    };

    let googleAccountsSpy: jasmine.SpyObj<typeof google.accounts.id>;
    let fbSpy: jasmine.SpyObj<typeof FB>;

    beforeEach(async () => {
        userInfo = new BehaviorSubject(loggedInUser);

        let authenticationService = {
            logInWithPassword: (): void => void 0,
            logInWithAssertion: (): void => void 0,
            userInfo: () => userInfo,
        };
        let activeModal = { close: (): void => void 0 };
        let userService = {
            getLogins: (): void => void 0,
            removeLogin: (): void => void 0,
        };
        let spinnerService = {
            show: (): void => void 0,
            hide: (): void => void 0,
        };

        googleAccountsSpy = jasmine.createSpyObj<typeof google.accounts.id>(["initialize"]);
        fbSpy = jasmine.createSpyObj<typeof FB>(["init"]);

        // Mock the global variables. We should figure out a better way to both inject this in the product and mock this in tests.
        window.google = {
            accounts: {
                id: googleAccountsSpy,
                oauth2: null,
            },
        };
        window.FB = fbSpy;

        await TestBed.configureTestingModule(
            {
                imports: [
                    ExternalLoginsComponent,
                ],
                providers: [
                    { provide: AuthenticationService, useValue: authenticationService },
                    { provide: NgbActiveModal, useValue: activeModal },
                    { provide: UserService, useValue: userService },
                    { provide: NgxSpinnerService, useValue: spinnerService },
                ],
            })
            .compileComponents();
        TestBed.overrideComponent(ExternalLoginsComponent, {
            remove: { imports: [ NgxSpinnerModule ]},
            add: { imports: [ MockNgxSpinnerComponent ] },
        });

        fixture = TestBed.createComponent(ExternalLoginsComponent);
        component = fixture.componentInstance;
    });

    afterEach(() => {
        ExternalLoginsComponent.facebookInitialized = false;
    });

    describe("Initialization", () => {
        beforeEach(() => {
            fixture.detectChanges();
        });

        it("should load the external login sdks", () => {
            // Google
            expect(google.accounts.id.initialize).toHaveBeenCalled();

            // Facebook
            expect(FB.init).toHaveBeenCalled();

            // Microsoft
            expect(component.microsoftApp).toBeDefined();
        });

        it("should only load the external login sdks once", () => {
            let originalMicrosoftApp = component.microsoftApp;

            // Destory and create a whole bunch of components to simulate the user opening and closing the dialog a bunch.
            for (let i = 0; i < 100; i++) {
                fixture.destroy();
                fixture = TestBed.createComponent(ExternalLoginsComponent);
                fixture.detectChanges();
            }

            // Google does actually need to be initialized each time since it's given a callback tied to the component.
            // 101 since it's initialized once via beforeEach
            expect(google.accounts.id.initialize).toHaveBeenCalledTimes(101);

            // Facebook
            expect(FB.init).toHaveBeenCalledTimes(1);

            // Microsoft
            expect(component.microsoftApp).toBe(originalMicrosoftApp);
        });
    });

    describe("Logging in with Google", () => {
        beforeEach(() => {
            fixture.detectChanges();
        });

        it("should close the dialog when successful", async () => {
            let authenticationService = TestBed.inject(AuthenticationService);
            spyOn(authenticationService, "logInWithAssertion").and.returnValue(Promise.resolve());

            let activeModal = TestBed.inject(NgbActiveModal);
            spyOn(activeModal, "close");

            let credentialResponse = { credential: "someIdToken" } as google.accounts.id.CredentialResponse;

            // Google makes this into a proper button, so trigger the callback directly.
            expect(googleAccountsSpy.initialize).toHaveBeenCalledTimes(1);
            let config = googleAccountsSpy.initialize.calls.first().args[0] as google.accounts.id.IdConfiguration;
            config.callback(credentialResponse);

            await fixture.whenStable();
            fixture.detectChanges();

            expect(authenticationService.logInWithAssertion).toHaveBeenCalledWith("urn:ietf:params:oauth:grant-type:google_identity_token", credentialResponse.credential, undefined);
            expect(activeModal.close).toHaveBeenCalled();

            // No error
            let error = fixture.debugElement.query(By.css(".alert-danger"));
            expect(error).toBeNull();
        });

        it("should show an error when authenticationService fails", async () => {
            let authenticationService = TestBed.inject(AuthenticationService);
            spyOn(authenticationService, "logInWithAssertion").and.returnValue(Promise.reject(""));

            let activeModal = TestBed.inject(NgbActiveModal);
            spyOn(activeModal, "close");

            let credentialResponse = { credential: "someIdToken" } as google.accounts.id.CredentialResponse;

            // Google makes this into a proper button, so trigger the callback directly.
            expect(googleAccountsSpy.initialize).toHaveBeenCalledTimes(1);
            let config = googleAccountsSpy.initialize.calls.first().args[0] as google.accounts.id.IdConfiguration;
            config.callback(credentialResponse);

            await fixture.whenStable();
            fixture.detectChanges();

            expect(authenticationService.logInWithAssertion).toHaveBeenCalledWith("urn:ietf:params:oauth:grant-type:google_identity_token", credentialResponse.credential, undefined);
            expect(activeModal.close).not.toHaveBeenCalled();

            // Error
            let error = fixture.debugElement.query(By.css(".alert-danger"));
            expect(error).not.toBeNull();
        });
    });

    describe("Logging in with Facebook", () => {
        // Hack to mock FB.login is overloaded. See: https://javascript.plainenglish.io/mocking-ts-method-overloads-with-jest-e9c3d3f1ce0c
        type facebookLogin = (callback: (response: facebook.StatusResponse) => void, options?: facebook.LoginOptions) => void;

        beforeEach(() => {
            fixture.detectChanges();
        });

        it("should close the dialog when successful", async () => {
            let authenticationService = TestBed.inject(AuthenticationService);
            spyOn(authenticationService, "logInWithAssertion").and.returnValue(Promise.resolve());

            let activeModal = TestBed.inject(NgbActiveModal);
            spyOn(activeModal, "close");

            let loginResponse = {
                status: "connected",
                authResponse: { accessToken: "someAccessToken" },
            } as facebook.StatusResponse;
            (FB.login as facebookLogin) = jasmine.createSpy("login", (handler: (response: facebook.StatusResponse) => void) => handler(loginResponse)).and.callThrough();

            let buttons = fixture.debugElement.queryAll(By.css("button"));
            expect(buttons.length).toEqual(3);

            let button = buttons[1];
            expect(button).not.toBeNull();
            button.nativeElement.click();

            await fixture.whenStable();
            fixture.detectChanges();

            expect(FB.login).toHaveBeenCalled();
            expect(authenticationService.logInWithAssertion).toHaveBeenCalledWith("urn:ietf:params:oauth:grant-type:facebook_access_token", loginResponse.authResponse.accessToken, undefined);
            expect(activeModal.close).toHaveBeenCalled();

            // No error
            let error = fixture.debugElement.query(By.css(".alert-danger"));
            expect(error).toBeNull();
        });

        it("should show an error when Facebook fails", async () => {
            let authenticationService = TestBed.inject(AuthenticationService);
            spyOn(authenticationService, "logInWithAssertion");

            let activeModal = TestBed.inject(NgbActiveModal);
            spyOn(activeModal, "close");

            let loginResponse = {
                status: "somethingInvalid",
                authResponse: {},
            } as unknown as facebook.StatusResponse;
            (FB.login as facebookLogin) = jasmine.createSpy("login", (handler: (response: facebook.StatusResponse) => void) => handler(loginResponse)).and.callThrough();

            let buttons = fixture.debugElement.queryAll(By.css("button"));
            expect(buttons.length).toEqual(3);

            let button = buttons[1];
            expect(button).not.toBeNull();
            button.nativeElement.click();

            await fixture.whenStable();
            fixture.detectChanges();

            expect(FB.login).toHaveBeenCalled();
            expect(authenticationService.logInWithAssertion).not.toHaveBeenCalled();
            expect(activeModal.close).not.toHaveBeenCalled();

            // Error
            let error = fixture.debugElement.query(By.css(".alert-danger"));
            expect(error).not.toBeNull();
        });

        it("should show an error when authenticationService fails", async () => {
            let authenticationService = TestBed.inject(AuthenticationService);
            spyOn(authenticationService, "logInWithAssertion").and.returnValue(Promise.reject(""));

            let activeModal = TestBed.inject(NgbActiveModal);
            spyOn(activeModal, "close");

            let loginResponse = {
                status: "connected",
                authResponse: { accessToken: "someAccessToken" },
            } as facebook.StatusResponse;
            (FB.login as facebookLogin) = jasmine.createSpy("login", (handler: (response: facebook.StatusResponse) => void) => handler(loginResponse)).and.callThrough();

            let buttons = fixture.debugElement.queryAll(By.css("button"));
            expect(buttons.length).toEqual(3);

            let button = buttons[1];
            expect(button).not.toBeNull();
            button.nativeElement.click();

            await fixture.whenStable();
            fixture.detectChanges();

            expect(FB.login).toHaveBeenCalled();
            expect(authenticationService.logInWithAssertion).toHaveBeenCalledWith("urn:ietf:params:oauth:grant-type:facebook_access_token", loginResponse.authResponse.accessToken, undefined);
            expect(activeModal.close).not.toHaveBeenCalled();

            // Error
            let error = fixture.debugElement.query(By.css(".alert-danger"));
            expect(error).not.toBeNull();
        });

        it("should not show an error when cancelled", async () => {
            let authenticationService = TestBed.inject(AuthenticationService);
            spyOn(authenticationService, "logInWithAssertion");

            let activeModal = TestBed.inject(NgbActiveModal);
            spyOn(activeModal, "close");

            let loginResponse = {} as facebook.StatusResponse;
            (FB.login as facebookLogin) = jasmine.createSpy("login", (handler: (response: facebook.StatusResponse) => void) => handler(loginResponse)).and.callThrough();

            let buttons = fixture.debugElement.queryAll(By.css("button"));
            expect(buttons.length).toEqual(3);

            let button = buttons[1];
            expect(button).not.toBeNull();
            button.nativeElement.click();

            await fixture.whenStable();
            fixture.detectChanges();

            expect(FB.login).toHaveBeenCalled();
            expect(authenticationService.logInWithAssertion).not.toHaveBeenCalled();
            expect(activeModal.close).not.toHaveBeenCalled();

            // No error
            let error = fixture.debugElement.query(By.css(".alert-danger"));
            expect(error).toBeNull();
        });
    });

    describe("Logging in with Microsoft", () => {
        beforeEach(() => {
            fixture.detectChanges();
        });

        it("should close the dialog when successful", async () => {
            let authenticationService = TestBed.inject(AuthenticationService);
            spyOn(authenticationService, "logInWithAssertion").and.returnValue(Promise.resolve());

            let activeModal = TestBed.inject(NgbActiveModal);
            spyOn(activeModal, "close");

            spyOn(component.microsoftApp, "initialize").and.returnValue(Promise.resolve());

            const token = "someToken";
            let authResult = {
                idToken: token,
            } as AuthenticationResult;
            spyOn(component.microsoftApp, "loginPopup").and.returnValue(Promise.resolve(authResult));

            let buttons = fixture.debugElement.queryAll(By.css("button"));
            expect(buttons.length).toEqual(3);

            let button = buttons[2];
            expect(button).not.toBeNull();
            button.nativeElement.click();

            await fixture.whenStable();
            fixture.detectChanges();

            expect(component.microsoftApp.initialize).toHaveBeenCalled();
            expect(component.microsoftApp.loginPopup).toHaveBeenCalledWith({ scopes: ["openid", "email"] });
            expect(authenticationService.logInWithAssertion).toHaveBeenCalledWith("urn:ietf:params:oauth:grant-type:microsoft_identity_token", token, undefined);
            expect(activeModal.close).toHaveBeenCalled();

            // No error
            let error = fixture.debugElement.query(By.css(".alert-danger"));
            expect(error).toBeNull();
        });

        it("should show an error when Microsoft fails", async () => {
            let authenticationService = TestBed.inject(AuthenticationService);
            spyOn(authenticationService, "logInWithAssertion");

            let activeModal = TestBed.inject(NgbActiveModal);
            spyOn(activeModal, "close");

            spyOn(component.microsoftApp, "initialize").and.returnValue(Promise.resolve());
            spyOn(component.microsoftApp, "loginPopup").and.returnValue(Promise.reject(""));

            let buttons = fixture.debugElement.queryAll(By.css("button"));
            expect(buttons.length).toEqual(3);

            let button = buttons[2];
            expect(button).not.toBeNull();
            button.nativeElement.click();

            await fixture.whenStable();
            fixture.detectChanges();

            expect(component.microsoftApp.initialize).toHaveBeenCalled();
            expect(component.microsoftApp.loginPopup).toHaveBeenCalledWith({ scopes: ["openid", "email"] });
            expect(authenticationService.logInWithAssertion).not.toHaveBeenCalled();
            expect(activeModal.close).not.toHaveBeenCalled();

            // Error
            let error = fixture.debugElement.query(By.css(".alert-danger"));
            expect(error).not.toBeNull();
        });

        it("should show an error when authenticationService fails", async () => {
            let authenticationService = TestBed.inject(AuthenticationService);
            spyOn(authenticationService, "logInWithAssertion").and.returnValue(Promise.reject(""));

            let activeModal = TestBed.inject(NgbActiveModal);
            spyOn(activeModal, "close");

            spyOn(component.microsoftApp, "initialize").and.returnValue(Promise.resolve());

            const token = "someToken";
            let authResult = {
                idToken: token,
            } as AuthenticationResult;
            spyOn(component.microsoftApp, "loginPopup").and.returnValue(Promise.resolve(authResult));

            let buttons = fixture.debugElement.queryAll(By.css("button"));
            expect(buttons.length).toEqual(3);

            let button = buttons[2];
            expect(button).not.toBeNull();
            button.nativeElement.click();

            await fixture.whenStable();
            fixture.detectChanges();

            expect(component.microsoftApp.initialize).toHaveBeenCalled();
            expect(component.microsoftApp.loginPopup).toHaveBeenCalledWith({ scopes: ["openid", "email"] });
            expect(authenticationService.logInWithAssertion).toHaveBeenCalledWith("urn:ietf:params:oauth:grant-type:microsoft_identity_token", token, undefined);
            expect(activeModal.close).not.toHaveBeenCalled();

            // Error
            let error = fixture.debugElement.query(By.css(".alert-danger"));
            expect(error).not.toBeNull();
        });

        it("should not show an error when cancelled", async () => {
            let authenticationService = TestBed.inject(AuthenticationService);
            spyOn(authenticationService, "logInWithAssertion");

            let activeModal = TestBed.inject(NgbActiveModal);
            spyOn(activeModal, "close");

            spyOn(component.microsoftApp, "initialize").and.returnValue(Promise.resolve());
            spyOn(component.microsoftApp, "loginPopup").and.returnValue(Promise.reject("user_cancelled:User closed the popup window window and cancelled the flow"));

            let buttons = fixture.debugElement.queryAll(By.css("button"));
            expect(buttons.length).toEqual(3);

            let button = buttons[2];
            expect(button).not.toBeNull();
            button.nativeElement.click();

            await fixture.whenStable();
            fixture.detectChanges();

            expect(component.microsoftApp.initialize).toHaveBeenCalled();
            expect(component.microsoftApp.loginPopup).toHaveBeenCalledWith({ scopes: ["openid", "email"] });
            expect(authenticationService.logInWithAssertion).not.toHaveBeenCalled();
            expect(activeModal.close).not.toHaveBeenCalled();

            // No error
            let error = fixture.debugElement.query(By.css(".alert-danger"));
            expect(error).toBeNull();
        });
    });

    describe("Choosing a user name", () => {
        let authenticationService: AuthenticationService;

        beforeEach(async () => {
            fixture.detectChanges();

            authenticationService = TestBed.inject(AuthenticationService);

            let errorResponse: IErrorResponse = { error: "account_selection_required" };
            spyOn(authenticationService, "logInWithAssertion").and.returnValue(Promise.reject({ json: () => errorResponse }));

            spyOn(component.microsoftApp, "initialize").and.returnValue(Promise.resolve());

            // Using Microsoft login since it's easy to mock, but they should all apply equally
            const token = "someToken";
            let authResult = {
                idToken: token,
            } as AuthenticationResult;
            spyOn(component.microsoftApp, "loginPopup").and.returnValue(Promise.resolve(authResult));

            let buttons = fixture.debugElement.queryAll(By.css("button"));
            expect(buttons.length).toEqual(3);

            let button = buttons[2];
            expect(button).not.toBeNull();
            button.nativeElement.click();

            await fixture.whenStable();
            fixture.detectChanges();

            // Reset for the actual tests
            (authenticationService.logInWithAssertion as jasmine.Spy).calls.reset();

            // No error
            let error = fixture.debugElement.query(By.css(".alert-danger"));
            expect(error).toBeNull();
        });

        describe("Validation", () => {
            it("should disable the register button initially", () => {
                let form = fixture.debugElement.query(By.css("form"));
                expect(form).not.toBeNull();

                fixture.detectChanges();
                let button = form.query(By.css("button"));
                expect(button).not.toBeNull();
                expect(button.properties.disabled).toEqual(true);

                let errors = getAllErrors();
                expect(errors.length).toEqual(0);
            });

            it("should disable the register button with an empty username", () => {
                let form = fixture.debugElement.query(By.css("form"));
                expect(form).not.toBeNull();

                setUsername(form, "someUsername");
                setUsername(form, "");

                fixture.detectChanges();
                let button = form.query(By.css("button"));
                expect(button).not.toBeNull();
                expect(button.properties.disabled).toEqual(true);

                let errors = getAllErrors();
                expect(errors.length).toEqual(1);
                expect(errors[0]).toEqual("Username is required");
            });

            it("should disable the register button with a short username", () => {
                let form = fixture.debugElement.query(By.css("form"));
                expect(form).not.toBeNull();

                setUsername(form, "a");

                fixture.detectChanges();
                let button = form.query(By.css("button"));
                expect(button).not.toBeNull();
                expect(button.properties.disabled).toEqual(true);

                let errors = getAllErrors();
                expect(errors.length).toEqual(1);
                expect(errors[0]).toEqual("Username must be at least 5 characters long");
            });
        });

        describe("Form submission", () => {
            it("should close the dialog when registering properly", async () => {
                (authenticationService.logInWithAssertion as jasmine.Spy).and.returnValue(Promise.resolve());

                let activeModal = TestBed.inject(NgbActiveModal);
                spyOn(activeModal, "close");

                let form = fixture.debugElement.query(By.css("form"));
                expect(form).not.toBeNull();

                await submit(form);
                expect(authenticationService.logInWithAssertion).toHaveBeenCalledWith("urn:ietf:params:oauth:grant-type:microsoft_identity_token", "someToken", "someUsername");
                expect(activeModal.close).toHaveBeenCalled();

                let errors = getAllErrors();
                expect(errors.length).toEqual(0);
            });

            it("should show an error when an http error occurs", async () => {
                (authenticationService.logInWithAssertion as jasmine.Spy).and.returnValue(Promise.reject(""));

                let activeModal = TestBed.inject(NgbActiveModal);
                spyOn(activeModal, "close");

                let form = fixture.debugElement.query(By.css("form"));
                expect(form).not.toBeNull();

                await submit(form);
                expect(authenticationService.logInWithAssertion).toHaveBeenCalledWith("urn:ietf:params:oauth:grant-type:microsoft_identity_token", "someToken", "someUsername");
                expect(activeModal.close).not.toHaveBeenCalled();

                let errors = getAllErrors();
                expect(errors.length).toEqual(1);
                expect(errors[0]).toEqual("There was a problem creating your account");
            });

            it("should show an error when there is a validation error", async () => {
                let errorResponse = { error_description: "someErrorDescription" };
                (authenticationService.logInWithAssertion as jasmine.Spy).and.returnValue(Promise.reject({ json: () => errorResponse }));

                let activeModal = TestBed.inject(NgbActiveModal);
                spyOn(activeModal, "close");

                let form = fixture.debugElement.query(By.css("form"));
                expect(form).not.toBeNull();

                await submit(form);
                expect(authenticationService.logInWithAssertion).toHaveBeenCalledWith("urn:ietf:params:oauth:grant-type:microsoft_identity_token", "someToken", "someUsername");
                expect(activeModal.close).not.toHaveBeenCalled();

                let errors = getAllErrors();
                expect(errors.length).toEqual(1);
                expect(errors[0]).toEqual("someErrorDescription");
            });

            async function submit(form: DebugElement): Promise<void> {
                setUsername(form, "someUsername");

                fixture.detectChanges();
                let button = form.query(By.css("button"));
                expect(button).not.toBeNull();
                button.nativeElement.click();

                await fixture.whenStable();
                return fixture.detectChanges();
            }
        });

        function setUsername(form: DebugElement, value: string): void {
            fixture.detectChanges();
            let element = form.query(By.css("#username"));
            expect(element).not.toBeNull();
            element.nativeElement.value = value;

            // Tell Angular
            let evt = document.createEvent("CustomEvent");
            evt.initCustomEvent("input", false, false, null);
            element.nativeElement.dispatchEvent(evt);
        }

        function getAllErrors(): string[] {
            let errors: string[] = [];
            let errorElements = fixture.debugElement.queryAll(By.css(".alert-danger"));
            for (let i = 0; i < errorElements.length; i++) {
                let errorChildren = errorElements[i].children;
                if (errorChildren.length > 0) {
                    for (let j = 0; j < errorChildren.length; j++) {
                        errors.push(errorChildren[j].nativeElement.textContent.trim());
                    }
                } else {
                    errors.push(errorElements[i].nativeElement.textContent.trim());
                }
            }

            return errors;
        }
    });

    describe("Manage mode", () => {
        beforeEach(() => {
            component.isManageMode = true;
        });

        it("should display list of registered logins", async () => {
            let logins: IUserLogins = {
                hasPassword: true,
                externalLogins: [
                    {
                        providerName: "someProviderName0",
                        externalUserId: "someExternalUserId0",
                    },
                    {
                        providerName: "someProviderName1",
                        externalUserId: "someExternalUserId1",
                    },
                    {
                        providerName: "someProviderName2",
                        externalUserId: "someExternalUserId2",
                    },
                ],
            };
            let userService = TestBed.inject(UserService);
            spyOn(userService, "getLogins").and.returnValue(Promise.resolve(logins));

            fixture.detectChanges();

            expect(userService.getLogins).toHaveBeenCalledWith(loggedInUser.username);

            await fixture.whenStable();
            fixture.detectChanges();

            let loginsTable = fixture.debugElement.query(By.css("table"));
            expect(loginsTable).not.toBeNull();

            let loginsRows = loginsTable.queryAll(By.css("tr"));
            expect(loginsRows.length).toEqual(logins.externalLogins.length);

            // No error
            let error = fixture.debugElement.query(By.css(".alert-danger"));
            expect(error).toBeNull();
        });

        it("should show an error when fetching logins fails", async () => {
            let userService = TestBed.inject(UserService);
            spyOn(userService, "getLogins").and.returnValue(Promise.reject(""));

            fixture.detectChanges();

            expect(userService.getLogins).toHaveBeenCalledWith(loggedInUser.username);

            await fixture.whenStable();
            fixture.detectChanges();

            let loginsTable = fixture.debugElement.query(By.css("table"));
            expect(loginsTable).toBeNull();

            // Error
            let error = fixture.debugElement.query(By.css(".alert-danger"));
            expect(error).not.toBeNull();
        });

        it("should remove login", async () => {
            let logins: IUserLogins = {
                hasPassword: true,
                externalLogins: [
                    {
                        providerName: "someProviderName0",
                        externalUserId: "someExternalUserId0",
                    },
                    {
                        providerName: "someProviderName1",
                        externalUserId: "someExternalUserId1",
                    },
                    {
                        providerName: "someProviderName2",
                        externalUserId: "someExternalUserId2",
                    },
                ],
            };
            let userService = TestBed.inject(UserService);
            spyOn(userService, "getLogins").and.returnValue(Promise.resolve(logins));
            spyOn(userService, "removeLogin").and.returnValue(Promise.resolve());

            fixture.detectChanges();

            expect(userService.getLogins).toHaveBeenCalledWith(loggedInUser.username);
            (userService.getLogins as jasmine.Spy).calls.reset();

            await fixture.whenStable();
            fixture.detectChanges();

            let loginsTable = fixture.debugElement.query(By.css("table"));
            expect(loginsTable).not.toBeNull();

            let loginsRows = loginsTable.queryAll(By.css("tr"));
            expect(loginsRows.length).toEqual(logins.externalLogins.length);

            let removeButton = loginsRows[1].query(By.css("button"));
            expect(removeButton).not.toBeNull();
            removeButton.nativeElement.click();

            expect(userService.removeLogin).toHaveBeenCalledWith(loggedInUser.username, logins.externalLogins[1]);
            await fixture.whenStable();

            // Refreshes the data
            expect(userService.getLogins).toHaveBeenCalledWith(loggedInUser.username);

            // No error
            let error = fixture.debugElement.query(By.css(".alert-danger"));
            expect(error).toBeNull();
        });

        it("should show an error when remove login fails", async () => {
            let logins: IUserLogins = {
                hasPassword: true,
                externalLogins: [
                    {
                        providerName: "someProviderName0",
                        externalUserId: "someExternalUserId0",
                    },
                    {
                        providerName: "someProviderName1",
                        externalUserId: "someExternalUserId1",
                    },
                    {
                        providerName: "someProviderName2",
                        externalUserId: "someExternalUserId2",
                    },
                ],
            };
            let userService = TestBed.inject(UserService);
            spyOn(userService, "getLogins").and.returnValue(Promise.resolve(logins));
            spyOn(userService, "removeLogin").and.returnValue(Promise.reject(""));

            fixture.detectChanges();

            expect(userService.getLogins).toHaveBeenCalledWith(loggedInUser.username);
            (userService.getLogins as jasmine.Spy).calls.reset();

            await fixture.whenStable();
            fixture.detectChanges();

            let loginsTable = fixture.debugElement.query(By.css("table"));
            expect(loginsTable).not.toBeNull();

            let loginsRows = loginsTable.queryAll(By.css("tr"));
            expect(loginsRows.length).toEqual(logins.externalLogins.length);

            let removeButton = loginsRows[1].query(By.css("button"));
            expect(removeButton).not.toBeNull();
            removeButton.nativeElement.click();

            expect(userService.removeLogin).toHaveBeenCalledWith(loggedInUser.username, logins.externalLogins[1]);

            await fixture.whenStable();
            fixture.detectChanges();

            expect(userService.getLogins).not.toHaveBeenCalled();

            // Error
            let error = fixture.debugElement.query(By.css(".alert-danger"));
            expect(error).not.toBeNull();
        });

        it("should hide add buttons for registered logins", async () => {
            let authResult = { idToken: "someIdToken" } as AuthenticationResult;
            let logins: IUserLogins = {
                hasPassword: true,
                externalLogins: [
                    {
                        providerName: "someProviderName0",
                        externalUserId: "someExternalUserId0",
                    },
                    {
                        providerName: "someProviderName1",
                        externalUserId: "someExternalUserId1",
                    },
                    {
                        providerName: "someProviderName2",
                        externalUserId: "someExternalUserId2",
                    },
                ],
            };
            let userService = TestBed.inject(UserService);
            spyOn(userService, "getLogins").and.returnValue(Promise.resolve(logins));

            let authenticationService = TestBed.inject(AuthenticationService);
            spyOn(authenticationService, "logInWithAssertion").and.returnValue(Promise.resolve());

            fixture.detectChanges();

            expect(userService.getLogins).toHaveBeenCalledWith(loggedInUser.username);
            (userService.getLogins as jasmine.Spy).calls.reset();

            await fixture.whenStable();
            fixture.detectChanges();

            let buttons = fixture.debugElement.queryAll(By.css("button"));
            expect(buttons.length).not.toEqual(0);

            let button: DebugElement = null;
            for (let i = 0; i < buttons.length; i++) {
                let img = buttons[i].query(By.css("img"));
                if (img && img.nativeElement.title === "Sign in with Microsoft") {
                    button = buttons[i];
                }
            }

            spyOn(component.microsoftApp, "initialize").and.returnValue(Promise.resolve());
            spyOn(component.microsoftApp, "loginPopup").and.returnValue(Promise.resolve(authResult));
            expect(button).toBeDefined();
            button.nativeElement.click();

            await fixture.whenStable();

            expect(authenticationService.logInWithAssertion).toHaveBeenCalledWith("urn:ietf:params:oauth:grant-type:microsoft_identity_token", authResult.idToken, undefined);

            // Refreshes the data
            expect(userService.getLogins).toHaveBeenCalledWith(loggedInUser.username);

            // No error
            let error = fixture.debugElement.query(By.css(".alert-danger"));
            expect(error).toBeNull();
        });

        it("should refresh the login data when adding a new login", async () => {
            const providerName = "Google";
            let logins: IUserLogins = {
                hasPassword: true,
                externalLogins: [
                    {
                        providerName,
                        externalUserId: "someExternalUserId",
                    },
                ],
            };
            let userService = TestBed.inject(UserService);
            spyOn(userService, "getLogins").and.returnValue(Promise.resolve(logins));

            fixture.detectChanges();

            expect(userService.getLogins).toHaveBeenCalledWith(loggedInUser.username);

            await fixture.whenStable();
            fixture.detectChanges();

            let buttons = fixture.debugElement.queryAll(By.css("button"));
            expect(buttons.length).not.toEqual(0);
            for (let i = 0; i < buttons.length; i++) {
                if (buttons[i].nativeElement.textContent.trim() === providerName) {
                    throw new Error(`Found add button for provider: ${providerName}`);
                }
            }

            // No error
            let error = fixture.debugElement.query(By.css(".alert-danger"));
            expect(error).toBeNull();
        });
    });
});
