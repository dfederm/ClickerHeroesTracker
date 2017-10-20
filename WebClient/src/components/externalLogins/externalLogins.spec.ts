import { ComponentFixture, TestBed } from "@angular/core/testing";
import { By } from "@angular/platform-browser";
import { FormsModule } from "@angular/forms";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { UserAgentApplication } from "msalx";

import { ExternalLoginsComponent, IErrorResponse } from "./externalLogins";
import { AuthenticationService } from "../../services/authenticationService/authenticationService";
import { DebugElement } from "@angular/core";

// tslint:disable-next-line:no-namespace
declare global {
    var msal: UserAgentApplication;

    // tslint:disable-next-line:interface-name - We don't own this interface name, just extending it
    interface Window {
        gapi: {};
        FB: {};
    }
}

describe("ExternalLoginsComponent", () => {
    let component: ExternalLoginsComponent;
    let fixture: ComponentFixture<ExternalLoginsComponent>;

    beforeEach(done => {
        let authenticationService = {
            logInWithPassword: (): void => void 0,
            logInWithAssertion: (): void => void 0,
        };
        let activeModal = { close: (): void => void 0 };

        // Mock the global variables. We should figure out a better way to both inject this in the product and mock this in tests.
        window.gapi = { load: (): void => void 0 };
        window.FB = { init: (): void => void 0 };

        spyOn(gapi, "load").and.callFake((_apiName: string, callback: gapi.LoadCallback) => {
            // tslint:disable-next-line:no-any
            gapi.auth2 = { init: jasmine.createSpy("init") } as any;

            callback();
        });

        spyOn(FB, "init");

        TestBed.configureTestingModule(
            {
                imports: [FormsModule],
                declarations: [ExternalLoginsComponent],
                providers:
                [
                    { provide: AuthenticationService, useValue: authenticationService },
                    { provide: NgbActiveModal, useValue: activeModal },
                ],
            })
            .compileComponents()
            .then(() => {
                fixture = TestBed.createComponent(ExternalLoginsComponent);
                component = fixture.componentInstance;

                fixture.detectChanges();
            })
            .then(done)
            .catch(done.fail);
    });

    afterEach(() => {
        // tslint:disable-next-line:no-any
        (ExternalLoginsComponent as any).facebookInitialized = false;

        // tslint:disable-next-line:no-any
        (ExternalLoginsComponent as any).microsoftApp = undefined;
        msal = undefined;
    });

    describe("Initialization", () => {
        it("should load the external login sdks", () => {
            // Google
            expect(gapi.load).toHaveBeenCalledWith("auth2", jasmine.any(Function));
            expect(gapi.auth2.init).toHaveBeenCalled();

            // Facebook
            expect(FB.init).toHaveBeenCalled();

            // Microsoft
            expect(msal).toBeDefined();
        });

        it("should only load the external login sdks once", () => {
            let originalMsal = msal;

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
            expect(msal).toBe(originalMsal);
        });
    });

    describe("Logging in with Google", () => {
        it("should close the dialog when successful", done => {
            let authenticationService = TestBed.get(AuthenticationService) as AuthenticationService;
            spyOn(authenticationService, "logInWithAssertion").and.returnValue(Promise.resolve());

            let activeModal = TestBed.get(NgbActiveModal) as NgbActiveModal;
            spyOn(activeModal, "close");

            let authResponse = { id_token: "someIdToken" };
            let googleUser = { getAuthResponse: jasmine.createSpy("getAuthResponse", () => authResponse).and.callThrough() };
            let googleAuth = { signIn: jasmine.createSpy("signIn", () => Promise.resolve(googleUser)).and.callThrough() };
            gapi.auth2.getAuthInstance = jasmine.createSpy("getAuthInstance", () => googleAuth).and.callThrough();

            let buttons = fixture.debugElement.queryAll(By.css("button"));
            expect(buttons.length).toEqual(3);

            let button = buttons[0];
            expect(button).not.toBeNull();
            button.nativeElement.click();

            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    expect(gapi.auth2.getAuthInstance).toHaveBeenCalled();
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

            let googleAuth = { signIn: jasmine.createSpy("signIn", () => Promise.reject("")).and.callThrough() };
            gapi.auth2.getAuthInstance = jasmine.createSpy("getAuthInstance", () => googleAuth).and.callThrough();

            let buttons = fixture.debugElement.queryAll(By.css("button"));
            expect(buttons.length).toEqual(3);

            let button = buttons[0];
            expect(button).not.toBeNull();
            button.nativeElement.click();

            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    expect(gapi.auth2.getAuthInstance).toHaveBeenCalled();
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

            let authResponse = { id_token: "someIdToken" };
            let googleUser = { getAuthResponse: jasmine.createSpy("getAuthResponse", () => authResponse).and.callThrough() };
            let googleAuth = { signIn: jasmine.createSpy("signIn", () => Promise.resolve(googleUser)).and.callThrough() };
            gapi.auth2.getAuthInstance = jasmine.createSpy("getAuthInstance", () => googleAuth).and.callThrough();

            let buttons = fixture.debugElement.queryAll(By.css("button"));
            expect(buttons.length).toEqual(3);

            let button = buttons[0];
            expect(button).not.toBeNull();
            button.nativeElement.click();

            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    expect(gapi.auth2.getAuthInstance).toHaveBeenCalled();
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

            let googleAuth = { signIn: jasmine.createSpy("signIn", () => Promise.reject({ error: "popup_closed_by_user" })).and.callThrough() };
            gapi.auth2.getAuthInstance = jasmine.createSpy("getAuthInstance", () => googleAuth).and.callThrough();

            let buttons = fixture.debugElement.queryAll(By.css("button"));
            expect(buttons.length).toEqual(3);

            let button = buttons[0];
            expect(button).not.toBeNull();
            button.nativeElement.click();

            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    expect(gapi.auth2.getAuthInstance).toHaveBeenCalled();
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
        it("should close the dialog when successful", done => {
            let authenticationService = TestBed.get(AuthenticationService) as AuthenticationService;
            spyOn(authenticationService, "logInWithAssertion").and.returnValue(Promise.resolve());

            let activeModal = TestBed.get(NgbActiveModal) as NgbActiveModal;
            spyOn(activeModal, "close");

            let loginResponse = {
                status: "connected",
                authResponse: { accessToken: "someAccessToken" },
            };
            FB.login = jasmine.createSpy("login", (handler: (response: {}) => void) => handler(loginResponse)).and.callThrough();

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
            };
            FB.login = jasmine.createSpy("login", (handler: (response: {}) => void) => handler(loginResponse)).and.callThrough();

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
            };
            FB.login = jasmine.createSpy("login", (handler: (response: {}) => void) => handler(loginResponse)).and.callThrough();

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

            let loginResponse = {};
            FB.login = jasmine.createSpy("login", (handler: (response: {}) => void) => handler(loginResponse)).and.callThrough();

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
        it("should close the dialog when successful", done => {
            let authenticationService = TestBed.get(AuthenticationService) as AuthenticationService;
            spyOn(authenticationService, "logInWithAssertion").and.returnValue(Promise.resolve());

            let activeModal = TestBed.get(NgbActiveModal) as NgbActiveModal;
            spyOn(activeModal, "close");

            const token = "someToken";
            msal.loginPopup = jasmine.createSpy("loginPopup", () => Promise.resolve(token)).and.callThrough();

            let buttons = fixture.debugElement.queryAll(By.css("button"));
            expect(buttons.length).toEqual(3);

            let button = buttons[2];
            expect(button).not.toBeNull();
            button.nativeElement.click();

            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    expect(msal.loginPopup).toHaveBeenCalledWith(["openid", "email"]);
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

            msal.loginPopup = jasmine.createSpy("loginPopup", () => Promise.reject("")).and.callThrough();

            let buttons = fixture.debugElement.queryAll(By.css("button"));
            expect(buttons.length).toEqual(3);

            let button = buttons[2];
            expect(button).not.toBeNull();
            button.nativeElement.click();

            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    expect(msal.loginPopup).toHaveBeenCalledWith(["openid", "email"]);
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
            msal.loginPopup = jasmine.createSpy("loginPopup", () => Promise.resolve(token)).and.callThrough();

            let buttons = fixture.debugElement.queryAll(By.css("button"));
            expect(buttons.length).toEqual(3);

            let button = buttons[2];
            expect(button).not.toBeNull();
            button.nativeElement.click();

            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    expect(msal.loginPopup).toHaveBeenCalledWith(["openid", "email"]);
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

            msal.loginPopup = jasmine.createSpy("loginPopup", () => Promise.reject("user_cancelled:User closed the popup window window and cancelled the flow")).and.callThrough();

            let buttons = fixture.debugElement.queryAll(By.css("button"));
            expect(buttons.length).toEqual(3);

            let button = buttons[2];
            expect(button).not.toBeNull();
            button.nativeElement.click();

            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    expect(msal.loginPopup).toHaveBeenCalledWith(["openid", "email"]);
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
            authenticationService = TestBed.get(AuthenticationService) as AuthenticationService;

            let errorResponse: IErrorResponse = { error: "account_selection_required" };
            spyOn(authenticationService, "logInWithAssertion").and.returnValue(Promise.reject({ json: () => errorResponse }));

            // Using Microsoft login since it's easy to mock, but they should all apply equally
            msal.loginPopup = () => Promise.resolve("someToken");

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
});
