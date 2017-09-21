import { ComponentFixture, TestBed } from "@angular/core/testing";
import { By } from "@angular/platform-browser";
import { FormsModule } from "@angular/forms";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { DebugElement } from "@angular/core";
import { UserAgentApplication } from "msalx";

import { LogInDialogComponent } from "./logInDialog";
import { AuthenticationService } from "../../services/authenticationService/authenticationService";

// tslint:disable-next-line:no-namespace
declare global {
    var msal: UserAgentApplication;

    // tslint:disable-next-line:interface-name - We don't own this interface name, just extending it
    interface Window {
        gapi: {};
        FB: {};
    }
}

describe("LogInDialogComponent", () => {
    let component: LogInDialogComponent;
    let fixture: ComponentFixture<LogInDialogComponent>;

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
                declarations: [LogInDialogComponent],
                providers:
                [
                    { provide: AuthenticationService, useValue: authenticationService },
                    { provide: NgbActiveModal, useValue: activeModal },
                ],
            })
            .compileComponents()
            .then(() => {
                fixture = TestBed.createComponent(LogInDialogComponent);
                component = fixture.componentInstance;

                fixture.detectChanges();
            })
            .then(done)
            .catch(done.fail);
    });

    afterEach(() => {
        // tslint:disable-next-line:no-any
        (LogInDialogComponent as any).facebookInitialized = false;

        // tslint:disable-next-line:no-any
        (LogInDialogComponent as any).microsoftApp = undefined;
        msal = undefined;
    });

    describe("Initialization", () => {
        it("should display the modal header", () => {
            let header = fixture.debugElement.query(By.css(".modal-header"));
            expect(header).not.toBeNull();

            let title = header.query(By.css(".modal-title"));
            expect(title).not.toBeNull();
            expect(title.nativeElement.textContent).toEqual("Log in");
        });

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
                let dialog = TestBed.createComponent(LogInDialogComponent);
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

    describe("Logging in with a password", () => {
        it("should close the dialog when using proper credentials", done => {
            let authenticationService = TestBed.get(AuthenticationService) as AuthenticationService;
            spyOn(authenticationService, "logInWithPassword").and.returnValue(Promise.resolve());

            let activeModal = TestBed.get(NgbActiveModal) as NgbActiveModal;
            spyOn(activeModal, "close");

            // Wait for stability since ngModel is async
            fixture.whenStable()
                .then(() => {
                    let body = fixture.debugElement.query(By.css(".modal-body"));
                    expect(body).not.toBeNull();

                    let form = body.query(By.css("form"));
                    expect(form).not.toBeNull();

                    let username = form.query(By.css("#username"));
                    expect(username).not.toBeNull();
                    setInputValue(username, "someUsername");

                    let password = form.query(By.css("#password"));
                    expect(password).not.toBeNull();
                    setInputValue(password, "somePassword");

                    let button = form.query(By.css("button"));
                    expect(button).not.toBeNull();
                    button.nativeElement.click();

                    // Wait for stability from the authenticationService call
                    fixture.detectChanges();
                    return fixture.whenStable();
                })
                .then(() => {
                    fixture.detectChanges();

                    expect(authenticationService.logInWithPassword).toHaveBeenCalledWith("someUsername", "somePassword");
                    expect(activeModal.close).toHaveBeenCalled();

                    // No error
                    let error = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(error).toBeNull();
                })
                .then(done)
                .catch(done.fail);
        });

        it("should show an error when using incorrect credentials", done => {
            let authenticationService = TestBed.get(AuthenticationService) as AuthenticationService;
            spyOn(authenticationService, "logInWithPassword").and.returnValue(Promise.reject(""));

            let activeModal = TestBed.get(NgbActiveModal) as NgbActiveModal;
            spyOn(activeModal, "close");

            // Wait for stability since ngModel is async
            fixture.whenStable()
                .then(() => {
                    let body = fixture.debugElement.query(By.css(".modal-body"));
                    expect(body).not.toBeNull();

                    let form = body.query(By.css("form"));
                    expect(form).not.toBeNull();

                    let username = form.query(By.css("#username"));
                    expect(username).not.toBeNull();
                    setInputValue(username, "someUsername");

                    let password = form.query(By.css("#password"));
                    expect(password).not.toBeNull();
                    setInputValue(password, "somePassword");

                    let button = form.query(By.css("button"));
                    expect(button).not.toBeNull();
                    button.nativeElement.click();

                    // Wait for stability from the authenticationService call
                    fixture.detectChanges();
                    return fixture.whenStable();
                })
                .then(() => {
                    fixture.detectChanges();

                    expect(authenticationService.logInWithPassword).toHaveBeenCalledWith("someUsername", "somePassword");

                    let error = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(error).not.toBeNull();
                    expect(activeModal.close).not.toHaveBeenCalled();
                })
                .then(done)
                .catch(done.fail);
        });

        function setInputValue(element: DebugElement, value: string): void {
            element.nativeElement.value = value;

            // Tell Angular
            let evt = document.createEvent("CustomEvent");
            evt.initCustomEvent("input", false, false, null);
            element.nativeElement.dispatchEvent(evt);
        }
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

            let body = fixture.debugElement.query(By.css(".modal-body"));
            expect(body).not.toBeNull();

            let buttons = body.queryAll(By.css("h4+div button"));
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
                    expect(authenticationService.logInWithAssertion).toHaveBeenCalledWith("urn:ietf:params:oauth:grant-type:google_identity_token", authResponse.id_token);
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

            let body = fixture.debugElement.query(By.css(".modal-body"));
            expect(body).not.toBeNull();

            let buttons = body.queryAll(By.css("h4+div button"));
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

            let body = fixture.debugElement.query(By.css(".modal-body"));
            expect(body).not.toBeNull();

            let buttons = body.queryAll(By.css("h4+div button"));
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
                    expect(authenticationService.logInWithAssertion).toHaveBeenCalledWith("urn:ietf:params:oauth:grant-type:google_identity_token", authResponse.id_token);
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

            let body = fixture.debugElement.query(By.css(".modal-body"));
            expect(body).not.toBeNull();

            let buttons = body.queryAll(By.css("h4+div button"));
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

            let body = fixture.debugElement.query(By.css(".modal-body"));
            expect(body).not.toBeNull();

            let buttons = body.queryAll(By.css("h4+div button"));
            expect(buttons.length).toEqual(3);

            let button = buttons[1];
            expect(button).not.toBeNull();
            button.nativeElement.click();

            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    expect(FB.login).toHaveBeenCalled();
                    expect(authenticationService.logInWithAssertion).toHaveBeenCalledWith("urn:ietf:params:oauth:grant-type:facebook_access_token", loginResponse.authResponse.accessToken);
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

            let body = fixture.debugElement.query(By.css(".modal-body"));
            expect(body).not.toBeNull();

            let buttons = body.queryAll(By.css("h4+div button"));
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

            let body = fixture.debugElement.query(By.css(".modal-body"));
            expect(body).not.toBeNull();

            let buttons = body.queryAll(By.css("h4+div button"));
            expect(buttons.length).toEqual(3);

            let button = buttons[1];
            expect(button).not.toBeNull();
            button.nativeElement.click();

            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    expect(FB.login).toHaveBeenCalled();
                    expect(authenticationService.logInWithAssertion).toHaveBeenCalledWith("urn:ietf:params:oauth:grant-type:facebook_access_token", loginResponse.authResponse.accessToken);
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

            let body = fixture.debugElement.query(By.css(".modal-body"));
            expect(body).not.toBeNull();

            let buttons = body.queryAll(By.css("h4+div button"));
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

            let body = fixture.debugElement.query(By.css(".modal-body"));
            expect(body).not.toBeNull();

            let buttons = body.queryAll(By.css("h4+div button"));
            expect(buttons.length).toEqual(3);

            let button = buttons[2];
            expect(button).not.toBeNull();
            button.nativeElement.click();

            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    expect(msal.loginPopup).toHaveBeenCalledWith(["openid"]);
                    expect(authenticationService.logInWithAssertion).toHaveBeenCalledWith("urn:ietf:params:oauth:grant-type:microsoft_identity_token", token);
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

            let body = fixture.debugElement.query(By.css(".modal-body"));
            expect(body).not.toBeNull();

            let buttons = body.queryAll(By.css("h4+div button"));
            expect(buttons.length).toEqual(3);

            let button = buttons[2];
            expect(button).not.toBeNull();
            button.nativeElement.click();

            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    expect(msal.loginPopup).toHaveBeenCalledWith(["openid"]);
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

            let body = fixture.debugElement.query(By.css(".modal-body"));
            expect(body).not.toBeNull();

            let buttons = body.queryAll(By.css("h4+div button"));
            expect(buttons.length).toEqual(3);

            let button = buttons[2];
            expect(button).not.toBeNull();
            button.nativeElement.click();

            fixture.whenStable()
                .then(() => {
                    fixture.detectChanges();

                    expect(msal.loginPopup).toHaveBeenCalledWith(["openid"]);
                    expect(authenticationService.logInWithAssertion).toHaveBeenCalledWith("urn:ietf:params:oauth:grant-type:microsoft_identity_token", token);
                    expect(activeModal.close).not.toHaveBeenCalled();

                    // Error
                    let error = fixture.debugElement.query(By.css(".alert-danger"));
                    expect(error).not.toBeNull();
                })
                .then(done)
                .catch(done.fail);
        });
    });
});
