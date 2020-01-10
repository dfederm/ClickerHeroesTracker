import { ComponentFixture, TestBed } from "@angular/core/testing";
import { By } from "@angular/platform-browser";
import { FormsModule } from "@angular/forms";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { DebugElement, NO_ERRORS_SCHEMA } from "@angular/core";

import { ExternalLoginsComponent, IErrorResponse } from "./externalLogins";
import { AuthenticationService, IUserInfo } from "../../services/authenticationService/authenticationService";
import { UserService, IUserLogins } from "../../services/userService/userService";
import { BehaviorSubject } from "rxjs";
import { AuthResponse } from "msal";

// tslint:disable-next-line:no-namespace
declare global {
    // tslint:disable-next-line:interface-name - We don't own this interface name, just extending it
    interface Window {
        gapi: {};
        FB: {};
    }
}

describe("ExternalLoginsComponent", () => {
    let component: ExternalLoginsComponent;
    let fixture: ComponentFixture<ExternalLoginsComponent>;
    let userInfo: BehaviorSubject<IUserInfo>;

    const loggedInUser: IUserInfo = {
        isLoggedIn: true,
        id: "someId",
        username: "someUsername",
        email: "someEmail",
    };

    let gapiSpy: jasmine.SpyObj<typeof gapi>;
    let fbSpy: jasmine.SpyObj<typeof FB>;

    beforeEach(done => {
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

        gapiSpy = jasmine.createSpyObj<typeof gapi>(["load"]);
        fbSpy = jasmine.createSpyObj<typeof FB>(["init"]);

        gapiSpy.load.and.callFake((_apiName: string, callback: gapi.LoadCallback) => {
            gapi.auth2 = jasmine.createSpyObj(["init"]);
            callback();
        });

        // Mock the global variables. We should figure out a better way to both inject this in the product and mock this in tests.
        window.gapi = gapiSpy;
        window.FB = fbSpy;

        TestBed.configureTestingModule(
            {
                imports: [FormsModule],
                declarations: [ExternalLoginsComponent],
                providers:
                    [
                        { provide: AuthenticationService, useValue: authenticationService },
                        { provide: NgbActiveModal, useValue: activeModal },
                        { provide: UserService, useValue: userService },
                    ],
                schemas: [NO_ERRORS_SCHEMA],
            })
            .compileComponents()
            .then(() => {
                fixture = TestBed.createComponent(ExternalLoginsComponent);
                component = fixture.componentInstance;
            })
            .then(done)
            .catch(done.fail);
    });

    afterEach(() => {
        // tslint:disable-next-line:no-any
        ExternalLoginsComponent.facebookInitialized = false;
    });

    describe("Initialization", () => {
        beforeEach(() => {
            fixture.detectChanges();
        });

        it("should load the external login sdks", () => {
            // Google
            expect(gapi.load).toHaveBeenCalledWith("auth2", jasmine.any(Function));
            expect(gapi.auth2.init).toHaveBeenCalled();

            // Facebook
            expect(FB.init).toHaveBeenCalled();

            // Microsoft
            expect(component.microsoftApp).toBeDefined();
        });

        it("should only load the external login sdks once", () => {
            let originalMicrosoftApp = component.microsoftApp;

            // Create a whole bunch of components to simulate the user opening and closing the dialog a bunch.
            for (let i = 0; i < 100; i++) {
                let dialog = TestBed.createComponent(ExternalLoginsComponent);
                dialog.detectChanges();
                dialog.destroy();
            }

            // Google
            expect(gapi.load).toHaveBeenCalledTimes(1);
            expect(gapi.auth2.init).toHaveBeenCalledTimes(1);

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

        it("should close the dialog when successful", done => {
            let authenticationService = TestBed.get(AuthenticationService) as AuthenticationService;
            spyOn(authenticationService, "logInWithAssertion").and.returnValue(Promise.resolve());

            let activeModal = TestBed.get(NgbActiveModal) as NgbActiveModal;
            spyOn(activeModal, "close");

            let authResponse = { id_token: "someIdToken" } as gapi.auth2.AuthResponse;

            let googleUser = jasmine.createSpyObj<gapi.auth2.GoogleUser>(["getAuthResponse"]);
            googleUser.getAuthResponse.and.returnValue(authResponse);

            let googleAuth = jasmine.createSpyObj<gapi.auth2.GoogleAuth>(["signIn"]);
            googleAuth.signIn.and.returnValue(Promise.resolve(googleUser));

            let auth2 = jasmine.createSpyObj<typeof gapi.auth2>(["getAuthInstance"]);
            auth2.getAuthInstance.and.returnValue(googleAuth);

            gapiSpy.auth2 = auth2;

            let buttons = fixture.debugElement.queryAll(By.css("button"));
            expect(buttons.length).toEqual(3);

            let button = buttons[0];
            expect(button).not.toBeNull();
            button.nativeElement.click();

            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    expect(auth2.getAuthInstance).toHaveBeenCalled();
                    expect(googleAuth.signIn).toHaveBeenCalled();
                    expect(googleUser.getAuthResponse).toHaveBeenCalled();
                    expect(authenticationService.logInWithAssertion).toHaveBeenCalledWith("urn:ietf:params:oauth:grant-type:google_identity_token", authResponse.id_token, undefined);
                    expect(activeModal.close).toHaveBeenCalled();

                    // No error
                    let error = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(error).toBeNull();
                })
                .then(done)
                .catch(done.fail);
        });

        it("should show an error when Google fails", done => {
            let authenticationService = TestBed.get(AuthenticationService) as AuthenticationService;
            spyOn(authenticationService, "logInWithAssertion");

            let activeModal = TestBed.get(NgbActiveModal) as NgbActiveModal;
            spyOn(activeModal, "close");

            let googleAuth = jasmine.createSpyObj<gapi.auth2.GoogleAuth>(["signIn"]);
            googleAuth.signIn.and.returnValue(Promise.reject());

            let auth2 = jasmine.createSpyObj<typeof gapi.auth2>(["getAuthInstance"]);
            auth2.getAuthInstance.and.returnValue(googleAuth);

            gapiSpy.auth2 = auth2;

            let buttons = fixture.debugElement.queryAll(By.css("button"));
            expect(buttons.length).toEqual(3);

            let button = buttons[0];
            expect(button).not.toBeNull();
            button.nativeElement.click();

            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    expect(auth2.getAuthInstance).toHaveBeenCalled();
                    expect(googleAuth.signIn).toHaveBeenCalled();
                    expect(authenticationService.logInWithAssertion).not.toHaveBeenCalled();
                    expect(activeModal.close).not.toHaveBeenCalled();

                    // Error
                    let error = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(error).not.toBeNull();
                })
                .then(done)
                .catch(done.fail);
        });

        it("should show an error when authenticationService fails", done => {
            let authenticationService = TestBed.get(AuthenticationService) as AuthenticationService;
            spyOn(authenticationService, "logInWithAssertion").and.returnValue(Promise.reject(""));

            let activeModal = TestBed.get(NgbActiveModal) as NgbActiveModal;
            spyOn(activeModal, "close");

            let authResponse = { id_token: "someIdToken" } as gapi.auth2.AuthResponse;

            let googleUser = jasmine.createSpyObj<gapi.auth2.GoogleUser>(["getAuthResponse"]);
            googleUser.getAuthResponse.and.returnValue(authResponse);

            let googleAuth = jasmine.createSpyObj<gapi.auth2.GoogleAuth>(["signIn"]);
            googleAuth.signIn.and.returnValue(Promise.resolve(googleUser));

            let auth2 = jasmine.createSpyObj<typeof gapi.auth2>(["getAuthInstance"]);
            auth2.getAuthInstance.and.returnValue(googleAuth);

            gapiSpy.auth2 = auth2;

            let buttons = fixture.debugElement.queryAll(By.css("button"));
            expect(buttons.length).toEqual(3);

            let button = buttons[0];
            expect(button).not.toBeNull();
            button.nativeElement.click();

            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    expect(auth2.getAuthInstance).toHaveBeenCalled();
                    expect(googleAuth.signIn).toHaveBeenCalled();
                    expect(googleUser.getAuthResponse).toHaveBeenCalled();
                    expect(authenticationService.logInWithAssertion).toHaveBeenCalledWith("urn:ietf:params:oauth:grant-type:google_identity_token", authResponse.id_token, undefined);
                    expect(activeModal.close).not.toHaveBeenCalled();

                    // Error
                    let error = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(error).not.toBeNull();
                })
                .then(done)
                .catch(done.fail);
        });

        it("should not show an error when cancelled", done => {
            let authenticationService = TestBed.get(AuthenticationService) as AuthenticationService;
            spyOn(authenticationService, "logInWithAssertion");

            let activeModal = TestBed.get(NgbActiveModal) as NgbActiveModal;
            spyOn(activeModal, "close");

            let googleAuth = jasmine.createSpyObj<gapi.auth2.GoogleAuth>(["signIn"]);
            googleAuth.signIn.and.returnValue(Promise.reject({ error: "popup_closed_by_user" }));

            let auth2 = jasmine.createSpyObj<typeof gapi.auth2>(["getAuthInstance"]);
            auth2.getAuthInstance.and.returnValue(googleAuth);

            gapiSpy.auth2 = auth2;

            let buttons = fixture.debugElement.queryAll(By.css("button"));
            expect(buttons.length).toEqual(3);

            let button = buttons[0];
            expect(button).not.toBeNull();
            button.nativeElement.click();

            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    expect(auth2.getAuthInstance).toHaveBeenCalled();
                    expect(googleAuth.signIn).toHaveBeenCalled();
                    expect(authenticationService.logInWithAssertion).not.toHaveBeenCalled();
                    expect(activeModal.close).not.toHaveBeenCalled();

                    // No error
                    let error = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(error).toBeNull();
                })
                .then(done)
                .catch(done.fail);
        });
    });

    describe("Logging in with Facebook", () => {
        beforeEach(() => {
            fixture.detectChanges();
        });

        it("should close the dialog when successful", done => {
            let authenticationService = TestBed.get(AuthenticationService) as AuthenticationService;
            spyOn(authenticationService, "logInWithAssertion").and.returnValue(Promise.resolve());

            let activeModal = TestBed.get(NgbActiveModal) as NgbActiveModal;
            spyOn(activeModal, "close");

            let loginResponse = {
                status: "connected",
                authResponse: { accessToken: "someAccessToken" },
            } as FB.LoginStatusResponse;
            FB.login = jasmine.createSpy("login", (handler: (response: FB.LoginStatusResponse) => void) => handler(loginResponse)).and.callThrough();

            let buttons = fixture.debugElement.queryAll(By.css("button"));
            expect(buttons.length).toEqual(3);

            let button = buttons[1];
            expect(button).not.toBeNull();
            button.nativeElement.click();

            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    expect(FB.login).toHaveBeenCalled();
                    expect(authenticationService.logInWithAssertion).toHaveBeenCalledWith("urn:ietf:params:oauth:grant-type:facebook_access_token", loginResponse.authResponse.accessToken, undefined);
                    expect(activeModal.close).toHaveBeenCalled();

                    // No error
                    let error = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(error).toBeNull();
                })
                .then(done)
                .catch(done.fail);
        });

        it("should show an error when Facebook fails", done => {
            let authenticationService = TestBed.get(AuthenticationService) as AuthenticationService;
            spyOn(authenticationService, "logInWithAssertion");

            let activeModal = TestBed.get(NgbActiveModal) as NgbActiveModal;
            spyOn(activeModal, "close");

            let loginResponse = {
                status: "somethingInvalid",
                authResponse: {},
            } as unknown as FB.LoginStatusResponse;
            FB.login = jasmine.createSpy("login", (handler: (response: FB.LoginStatusResponse) => void) => handler(loginResponse)).and.callThrough();

            let buttons = fixture.debugElement.queryAll(By.css("button"));
            expect(buttons.length).toEqual(3);

            let button = buttons[1];
            expect(button).not.toBeNull();
            button.nativeElement.click();

            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    expect(FB.login).toHaveBeenCalled();
                    expect(authenticationService.logInWithAssertion).not.toHaveBeenCalled();
                    expect(activeModal.close).not.toHaveBeenCalled();

                    // Error
                    let error = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(error).not.toBeNull();
                })
                .then(done)
                .catch(done.fail);
        });

        it("should show an error when authenticationService fails", done => {
            let authenticationService = TestBed.get(AuthenticationService) as AuthenticationService;
            spyOn(authenticationService, "logInWithAssertion").and.returnValue(Promise.reject(""));

            let activeModal = TestBed.get(NgbActiveModal) as NgbActiveModal;
            spyOn(activeModal, "close");

            let loginResponse = {
                status: "connected",
                authResponse: { accessToken: "someAccessToken" },
            } as FB.LoginStatusResponse;
            FB.login = jasmine.createSpy("login", (handler: (response: FB.LoginStatusResponse) => void) => handler(loginResponse)).and.callThrough();

            let buttons = fixture.debugElement.queryAll(By.css("button"));
            expect(buttons.length).toEqual(3);

            let button = buttons[1];
            expect(button).not.toBeNull();
            button.nativeElement.click();

            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    expect(FB.login).toHaveBeenCalled();
                    expect(authenticationService.logInWithAssertion).toHaveBeenCalledWith("urn:ietf:params:oauth:grant-type:facebook_access_token", loginResponse.authResponse.accessToken, undefined);
                    expect(activeModal.close).not.toHaveBeenCalled();

                    // Error
                    let error = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(error).not.toBeNull();
                })
                .then(done)
                .catch(done.fail);
        });

        it("should not show an error when cancelled", done => {
            let authenticationService = TestBed.get(AuthenticationService) as AuthenticationService;
            spyOn(authenticationService, "logInWithAssertion");

            let activeModal = TestBed.get(NgbActiveModal) as NgbActiveModal;
            spyOn(activeModal, "close");

            let loginResponse = {} as FB.LoginStatusResponse;
            FB.login = jasmine.createSpy("login", (handler: (response: FB.LoginStatusResponse) => void) => handler(loginResponse)).and.callThrough();

            let buttons = fixture.debugElement.queryAll(By.css("button"));
            expect(buttons.length).toEqual(3);

            let button = buttons[1];
            expect(button).not.toBeNull();
            button.nativeElement.click();

            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    expect(FB.login).toHaveBeenCalled();
                    expect(authenticationService.logInWithAssertion).not.toHaveBeenCalled();
                    expect(activeModal.close).not.toHaveBeenCalled();

                    // No error
                    let error = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(error).toBeNull();
                })
                .then(done)
                .catch(done.fail);
        });
    });

    describe("Logging in with Microsoft", () => {
        beforeEach(() => {
            fixture.detectChanges();
        });

        it("should close the dialog when successful", done => {
            let authenticationService = TestBed.get(AuthenticationService) as AuthenticationService;
            spyOn(authenticationService, "logInWithAssertion").and.returnValue(Promise.resolve());

            let activeModal = TestBed.get(NgbActiveModal) as NgbActiveModal;
            spyOn(activeModal, "close");

            const token = "someToken";
            let authResponse = {
                idToken: {
                    rawIdToken: token,
                },
            } as AuthResponse;
            spyOn(component.microsoftApp, "loginPopup").and.returnValue(Promise.resolve(authResponse));

            let buttons = fixture.debugElement.queryAll(By.css("button"));
            expect(buttons.length).toEqual(3);

            let button = buttons[2];
            expect(button).not.toBeNull();
            button.nativeElement.click();

            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    expect(component.microsoftApp.loginPopup).toHaveBeenCalledWith({ scopes: ["openid", "email"] });
                    expect(authenticationService.logInWithAssertion).toHaveBeenCalledWith("urn:ietf:params:oauth:grant-type:microsoft_identity_token", token, undefined);
                    expect(activeModal.close).toHaveBeenCalled();

                    // No error
                    let error = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(error).toBeNull();
                })
                .then(done)
                .catch(done.fail);
        });

        it("should show an error when Microsoft fails", done => {
            let authenticationService = TestBed.get(AuthenticationService) as AuthenticationService;
            spyOn(authenticationService, "logInWithAssertion");

            let activeModal = TestBed.get(NgbActiveModal) as NgbActiveModal;
            spyOn(activeModal, "close");

            spyOn(component.microsoftApp, "loginPopup").and.returnValue(Promise.reject(""));

            let buttons = fixture.debugElement.queryAll(By.css("button"));
            expect(buttons.length).toEqual(3);

            let button = buttons[2];
            expect(button).not.toBeNull();
            button.nativeElement.click();

            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    expect(component.microsoftApp.loginPopup).toHaveBeenCalledWith({ scopes: ["openid", "email"] });
                    expect(authenticationService.logInWithAssertion).not.toHaveBeenCalled();
                    expect(activeModal.close).not.toHaveBeenCalled();

                    // Error
                    let error = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(error).not.toBeNull();
                })
                .then(done)
                .catch(done.fail);
        });

        it("should show an error when authenticationService fails", done => {
            let authenticationService = TestBed.get(AuthenticationService) as AuthenticationService;
            spyOn(authenticationService, "logInWithAssertion").and.returnValue(Promise.reject(""));

            let activeModal = TestBed.get(NgbActiveModal) as NgbActiveModal;
            spyOn(activeModal, "close");

            const token = "someToken";
            let authResponse = {
                idToken: {
                    rawIdToken: token,
                },
            } as AuthResponse;
            spyOn(component.microsoftApp, "loginPopup").and.returnValue(Promise.resolve(authResponse));

            let buttons = fixture.debugElement.queryAll(By.css("button"));
            expect(buttons.length).toEqual(3);

            let button = buttons[2];
            expect(button).not.toBeNull();
            button.nativeElement.click();

            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    expect(component.microsoftApp.loginPopup).toHaveBeenCalledWith({ scopes: ["openid", "email"] });
                    expect(authenticationService.logInWithAssertion).toHaveBeenCalledWith("urn:ietf:params:oauth:grant-type:microsoft_identity_token", token, undefined);
                    expect(activeModal.close).not.toHaveBeenCalled();

                    // Error
                    let error = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(error).not.toBeNull();
                })
                .then(done)
                .catch(done.fail);
        });

        it("should not show an error when cancelled", done => {
            let authenticationService = TestBed.get(AuthenticationService) as AuthenticationService;
            spyOn(authenticationService, "logInWithAssertion");

            let activeModal = TestBed.get(NgbActiveModal) as NgbActiveModal;
            spyOn(activeModal, "close");

            spyOn(component.microsoftApp, "loginPopup").and.returnValue(Promise.reject("user_cancelled:User closed the popup window window and cancelled the flow"));

            let buttons = fixture.debugElement.queryAll(By.css("button"));
            expect(buttons.length).toEqual(3);

            let button = buttons[2];
            expect(button).not.toBeNull();
            button.nativeElement.click();

            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    expect(component.microsoftApp.loginPopup).toHaveBeenCalledWith({ scopes: ["openid", "email"] });
                    expect(authenticationService.logInWithAssertion).not.toHaveBeenCalled();
                    expect(activeModal.close).not.toHaveBeenCalled();

                    // No error
                    let error = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(error).toBeNull();
                })
                .then(done)
                .catch(done.fail);
        });
    });

    describe("Choosing a user name", () => {
        let authenticationService: AuthenticationService;

        beforeEach(done => {
            fixture.detectChanges();

            authenticationService = TestBed.get(AuthenticationService) as AuthenticationService;

            let errorResponse: IErrorResponse = { error: "account_selection_required" };
            spyOn(authenticationService, "logInWithAssertion").and.returnValue(Promise.reject({ json: () => errorResponse }));

            // Using Microsoft login since it's easy to mock, but they should all apply equally
            const token = "someToken";
            let authResponse = {
                idToken: {
                    rawIdToken: token,
                },
            } as AuthResponse;
            spyOn(component.microsoftApp, "loginPopup").and.returnValue(Promise.resolve(authResponse));

            let buttons = fixture.debugElement.queryAll(By.css("button"));
            expect(buttons.length).toEqual(3);

            let button = buttons[2];
            expect(button).not.toBeNull();
            button.nativeElement.click();

            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    // Reset fro the actual tests
                    (authenticationService.logInWithAssertion as jasmine.Spy).calls.reset();

                    // No error
                    let error = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(error).toBeNull();
                })
                .then(done)
                .catch(done.fail);
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
            it("should close the dialog when registering properly", done => {
                (authenticationService.logInWithAssertion as jasmine.Spy).and.returnValue(Promise.resolve());

                let activeModal = TestBed.get(NgbActiveModal) as NgbActiveModal;
                spyOn(activeModal, "close");

                let form = fixture.debugElement.query(By.css("form"));
                expect(form).not.toBeNull();

                submit(form)
                    .then(() => {
                        expect(authenticationService.logInWithAssertion).toHaveBeenCalledWith("urn:ietf:params:oauth:grant-type:microsoft_identity_token", "someToken", "someUsername");
                        expect(activeModal.close).toHaveBeenCalled();

                        let errors = getAllErrors();
                        expect(errors.length).toEqual(0);
                    })
                    .then(done)
                    .catch(done.fail);
            });

            it("should show an error when an http error occurs", done => {
                (authenticationService.logInWithAssertion as jasmine.Spy).and.returnValue(Promise.reject(""));

                let activeModal = TestBed.get(NgbActiveModal) as NgbActiveModal;
                spyOn(activeModal, "close");

                let form = fixture.debugElement.query(By.css("form"));
                expect(form).not.toBeNull();

                submit(form)
                    .then(() => {
                        expect(authenticationService.logInWithAssertion).toHaveBeenCalledWith("urn:ietf:params:oauth:grant-type:microsoft_identity_token", "someToken", "someUsername");
                        expect(activeModal.close).not.toHaveBeenCalled();

                        let errors = getAllErrors();
                        expect(errors.length).toEqual(1);
                        expect(errors[0]).toEqual("There was a problem creating your account");
                    })
                    .then(done)
                    .catch(done.fail);
            });

            it("should show an error when there is a validation error", done => {
                let errorResponse = { error_description: "someErrorDescription" };
                (authenticationService.logInWithAssertion as jasmine.Spy).and.returnValue(Promise.reject({ json: () => errorResponse }));

                let activeModal = TestBed.get(NgbActiveModal) as NgbActiveModal;
                spyOn(activeModal, "close");

                let form = fixture.debugElement.query(By.css("form"));
                expect(form).not.toBeNull();

                submit(form)
                    .then(() => {
                        expect(authenticationService.logInWithAssertion).toHaveBeenCalledWith("urn:ietf:params:oauth:grant-type:microsoft_identity_token", "someToken", "someUsername");
                        expect(activeModal.close).not.toHaveBeenCalled();

                        let errors = getAllErrors();
                        expect(errors.length).toEqual(1);
                        expect(errors[0]).toEqual("someErrorDescription");
                    })
                    .then(done)
                    .catch(done.fail);
            });

            function submit(form: DebugElement): Promise<void> {
                setUsername(form, "someUsername");

                fixture.detectChanges();
                let button = form.query(By.css("button"));
                expect(button).not.toBeNull();
                button.nativeElement.click();

                return fixture.whenStable()
                    .then(() => fixture.detectChanges());
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

        it("should display list of registered logins", done => {
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
            let userService = TestBed.get(UserService) as UserService;
            spyOn(userService, "getLogins").and.returnValue(Promise.resolve(logins));

            fixture.detectChanges();

            expect(userService.getLogins).toHaveBeenCalledWith(loggedInUser.username);

            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    let loginsTable = fixture.debugElement.query(By.css("table"));
                    expect(loginsTable).not.toBeNull();

                    let loginsRows = loginsTable.queryAll(By.css("tr"));
                    expect(loginsRows.length).toEqual(logins.externalLogins.length);

                    // No error
                    let error = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(error).toBeNull();
                })
                .then(done)
                .catch(done.fail);
        });

        it("should show an error when fetching logins fails", done => {
            let userService = TestBed.get(UserService) as UserService;
            spyOn(userService, "getLogins").and.returnValue(Promise.reject(""));

            fixture.detectChanges();

            expect(userService.getLogins).toHaveBeenCalledWith(loggedInUser.username);

            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    let loginsTable = fixture.debugElement.query(By.css("table"));
                    expect(loginsTable).toBeNull();

                    // Error
                    let error = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(error).not.toBeNull();
                })
                .then(done)
                .catch(done.fail);
        });

        it("should remove login", done => {
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
            let userService = TestBed.get(UserService) as UserService;
            spyOn(userService, "getLogins").and.returnValue(Promise.resolve(logins));
            spyOn(userService, "removeLogin").and.returnValue(Promise.resolve());

            fixture.detectChanges();

            expect(userService.getLogins).toHaveBeenCalledWith(loggedInUser.username);
            (userService.getLogins as jasmine.Spy).calls.reset();

            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    let loginsTable = fixture.debugElement.query(By.css("table"));
                    expect(loginsTable).not.toBeNull();

                    let loginsRows = loginsTable.queryAll(By.css("tr"));
                    expect(loginsRows.length).toEqual(logins.externalLogins.length);

                    let removeButton = loginsRows[1].query(By.css("button"));
                    expect(removeButton).not.toBeNull();
                    removeButton.nativeElement.click();

                    expect(userService.removeLogin).toHaveBeenCalledWith(loggedInUser.username, logins.externalLogins[1]);

                    return fixture.whenStable();
                })
                .then(() => {
                    // Refreshes the data
                    expect(userService.getLogins).toHaveBeenCalledWith(loggedInUser.username);

                    // No error
                    let error = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(error).toBeNull();
                })
                .then(done)
                .catch(done.fail);
        });

        it("should show an error when remove login fails", done => {
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
            let userService = TestBed.get(UserService) as UserService;
            spyOn(userService, "getLogins").and.returnValue(Promise.resolve(logins));
            spyOn(userService, "removeLogin").and.returnValue(Promise.reject(""));

            fixture.detectChanges();

            expect(userService.getLogins).toHaveBeenCalledWith(loggedInUser.username);
            (userService.getLogins as jasmine.Spy).calls.reset();

            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    let loginsTable = fixture.debugElement.query(By.css("table"));
                    expect(loginsTable).not.toBeNull();

                    let loginsRows = loginsTable.queryAll(By.css("tr"));
                    expect(loginsRows.length).toEqual(logins.externalLogins.length);

                    let removeButton = loginsRows[1].query(By.css("button"));
                    expect(removeButton).not.toBeNull();
                    removeButton.nativeElement.click();

                    expect(userService.removeLogin).toHaveBeenCalledWith(loggedInUser.username, logins.externalLogins[1]);

                    return fixture.whenStable();
                })
                .then(() => {
                    fixture.detectChanges();

                    expect(userService.getLogins).not.toHaveBeenCalled();

                    // Error
                    let error = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(error).not.toBeNull();
                })
                .then(done)
                .catch(done.fail);
        });

        it("should hide add buttons for registered logins", done => {
            let authResponse = { id_token: "someIdToken" };
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
            let userService = TestBed.get(UserService) as UserService;
            spyOn(userService, "getLogins").and.returnValue(Promise.resolve(logins));

            let authenticationService = TestBed.get(AuthenticationService) as AuthenticationService;
            spyOn(authenticationService, "logInWithAssertion").and.returnValue(Promise.resolve());

            fixture.detectChanges();

            expect(userService.getLogins).toHaveBeenCalledWith(loggedInUser.username);
            (userService.getLogins as jasmine.Spy).calls.reset();

            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    let buttons = fixture.debugElement.queryAll(By.css("button"));
                    expect(buttons.length).not.toEqual(0);

                    let button: DebugElement = null;
                    for (let i = 0; i < buttons.length; i++) {
                        if (buttons[i].nativeElement.textContent.trim() === "Google") {
                            button = buttons[i];
                        }
                    }

                    gapi.auth2.getAuthInstance = () => ({ signIn: () => Promise.resolve({ getAuthResponse: () => authResponse }) }) as {} as gapi.auth2.GoogleAuth;

                    expect(button).toBeDefined();
                    button.nativeElement.click();

                    return fixture.whenStable();
                })
                .then(() => {
                    expect(authenticationService.logInWithAssertion).toHaveBeenCalledWith("urn:ietf:params:oauth:grant-type:google_identity_token", authResponse.id_token, undefined);

                    // Refreshes the data
                    expect(userService.getLogins).toHaveBeenCalledWith(loggedInUser.username);

                    // No error
                    let error = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(error).toBeNull();
                })
                .then(done)
                .catch(done.fail);
        });

        it("should refresh the login data when adding a new login", done => {
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
            let userService = TestBed.get(UserService) as UserService;
            spyOn(userService, "getLogins").and.returnValue(Promise.resolve(logins));

            fixture.detectChanges();

            expect(userService.getLogins).toHaveBeenCalledWith(loggedInUser.username);

            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    let buttons = fixture.debugElement.queryAll(By.css("button"));
                    expect(buttons.length).not.toEqual(0);

                    for (let i = 0; i < buttons.length; i++) {
                        if (buttons[i].nativeElement.textContent.trim() === providerName) {
                            return Promise.reject(`Found add button for provider: ${providerName}`);
                        }
                    }

                    // No error
                    let error = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(error).toBeNull();

                    return Promise.resolve();
                })
                .then(done)
                .catch(done.fail);
        });
    });
});
