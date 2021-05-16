import { ComponentFixture, TestBed } from "@angular/core/testing";
import { FormsModule } from "@angular/forms";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { ValidateEqualModule } from "ng-validate-equal";

import { RegisterDialogComponent } from "./registerDialog";
import { AuthenticationService } from "../../services/authenticationService/authenticationService";
import { UserService } from "../../services/userService/userService";
import { By } from "@angular/platform-browser";
import { DebugElement, NO_ERRORS_SCHEMA } from "@angular/core";

describe("RegisterDialogComponent", () => {
    let fixture: ComponentFixture<RegisterDialogComponent>;

    beforeEach(done => {
        let authenticationService = {
            logInWithPassword: (): void => void 0,
        };
        let userService = {
            create: (): void => void 0,
        };
        let activeModal = { close: (): void => void 0 };

        TestBed.configureTestingModule(
            {
                imports: [
                    FormsModule,
                    ValidateEqualModule,
                ],
                declarations: [
                    RegisterDialogComponent,
                ],
                providers:
                    [
                        { provide: AuthenticationService, useValue: authenticationService },
                        { provide: UserService, useValue: userService },
                        { provide: NgbActiveModal, useValue: activeModal },
                    ],
                schemas: [NO_ERRORS_SCHEMA],
            })
            .compileComponents()
            .then(() => {
                fixture = TestBed.createComponent(RegisterDialogComponent);

                fixture.detectChanges();
            })
            .then(done)
            .catch(done.fail);
    });

    describe("Validation", () => {
        it("should disable the register button initially", done => {
            // Wait for stability since ngModel is async
            fixture.whenStable()
                .then(() => {
                    let body = fixture.debugElement.query(By.css(".modal-body"));
                    expect(body).not.toBeNull();

                    let form = body.query(By.css("form"));
                    expect(form).not.toBeNull();

                    fixture.detectChanges();
                    let button = form.query(By.css("button"));
                    expect(button).not.toBeNull();
                    expect(button.properties.disabled).toEqual(true);

                    let errors = getAllErrors();
                    expect(errors.length).toEqual(0);
                })
                .then(done)
                .catch(done.fail);
        });

        it("should enable the register button when all inputs are valid", done => {
            // Wait for stability since ngModel is async
            fixture.whenStable()
                .then(() => {
                    let body = fixture.debugElement.query(By.css(".modal-body"));
                    expect(body).not.toBeNull();

                    let form = body.query(By.css("form"));
                    expect(form).not.toBeNull();

                    setInputValue(form, "username", "someUsername");
                    setInputValue(form, "email", "someEmail@someDomain.com");
                    setInputValue(form, "password", "somePassword");
                    setInputValue(form, "confirmPassword", "somePassword");

                    fixture.detectChanges();
                    let button = form.query(By.css("button"));
                    expect(button).not.toBeNull();
                    expect(button.properties.disabled).toEqual(false);

                    let errors = getAllErrors();
                    expect(errors.length).toEqual(0);
                })
                .then(done)
                .catch(done.fail);
        });

        describe("username", () => {
            it("should disable the register button with a missing username", done => {
                // Wait for stability since ngModel is async
                fixture.whenStable()
                    .then(() => {
                        let body = fixture.debugElement.query(By.css(".modal-body"));
                        expect(body).not.toBeNull();

                        let form = body.query(By.css("form"));
                        expect(form).not.toBeNull();

                        setInputValue(form, "email", "someEmail@someDomain.com");
                        setInputValue(form, "password", "somePassword");
                        setInputValue(form, "confirmPassword", "somePassword");

                        fixture.detectChanges();
                        let button = form.query(By.css("button"));
                        expect(button).not.toBeNull();
                        expect(button.properties.disabled).toEqual(true);

                        let errors = getAllErrors();
                        expect(errors.length).toEqual(0);
                    })
                    .then(done)
                    .catch(done.fail);
            });

            it("should disable the register button with an empty username", done => {
                // Wait for stability since ngModel is async
                fixture.whenStable()
                    .then(() => {
                        let body = fixture.debugElement.query(By.css(".modal-body"));
                        expect(body).not.toBeNull();

                        let form = body.query(By.css("form"));
                        expect(form).not.toBeNull();

                        setInputValue(form, "username", "someUsername");
                        setInputValue(form, "username", "");
                        setInputValue(form, "email", "someEmail@someDomain.com");
                        setInputValue(form, "password", "somePassword");
                        setInputValue(form, "confirmPassword", "somePassword");

                        fixture.detectChanges();
                        let button = form.query(By.css("button"));
                        expect(button).not.toBeNull();
                        expect(button.properties.disabled).toEqual(true);

                        let errors = getAllErrors();
                        expect(errors.length).toEqual(1);
                        expect(errors[0]).toEqual("Username is required");
                    })
                    .then(done)
                    .catch(done.fail);
            });

            it("should disable the register button with a short username", done => {
                // Wait for stability since ngModel is async
                fixture.whenStable()
                    .then(() => {
                        let body = fixture.debugElement.query(By.css(".modal-body"));
                        expect(body).not.toBeNull();

                        let form = body.query(By.css("form"));
                        expect(form).not.toBeNull();

                        setInputValue(form, "username", "a");
                        setInputValue(form, "email", "someEmail@someDomain.com");
                        setInputValue(form, "password", "somePassword");
                        setInputValue(form, "confirmPassword", "somePassword");

                        fixture.detectChanges();
                        let button = form.query(By.css("button"));
                        expect(button).not.toBeNull();
                        expect(button.properties.disabled).toEqual(true);

                        let errors = getAllErrors();
                        expect(errors.length).toEqual(1);
                        expect(errors[0]).toEqual("Username must be at least 5 characters long");
                    })
                    .then(done)
                    .catch(done.fail);
            });
        });

        describe("email", () => {
            it("should disable the register button with a missing email", done => {
                // Wait for stability since ngModel is async
                fixture.whenStable()
                    .then(() => {
                        let body = fixture.debugElement.query(By.css(".modal-body"));
                        expect(body).not.toBeNull();

                        let form = body.query(By.css("form"));
                        expect(form).not.toBeNull();

                        setInputValue(form, "username", "someUsername");
                        setInputValue(form, "password", "somePassword");
                        setInputValue(form, "confirmPassword", "somePassword");

                        fixture.detectChanges();
                        let button = form.query(By.css("button"));
                        expect(button).not.toBeNull();
                        expect(button.properties.disabled).toEqual(true);

                        let errors = getAllErrors();
                        expect(errors.length).toEqual(0);
                    })
                    .then(done)
                    .catch(done.fail);
            });

            it("should disable the register button with an empty email", done => {
                // Wait for stability since ngModel is async
                fixture.whenStable()
                    .then(() => {
                        let body = fixture.debugElement.query(By.css(".modal-body"));
                        expect(body).not.toBeNull();

                        let form = body.query(By.css("form"));
                        expect(form).not.toBeNull();

                        setInputValue(form, "username", "someUsername");
                        setInputValue(form, "email", "someEmail@someDomain.com");
                        setInputValue(form, "email", "");
                        setInputValue(form, "password", "somePassword");
                        setInputValue(form, "confirmPassword", "somePassword");

                        fixture.detectChanges();
                        let button = form.query(By.css("button"));
                        expect(button).not.toBeNull();
                        expect(button.properties.disabled).toEqual(true);

                        let errors = getAllErrors();
                        expect(errors.length).toEqual(1);
                        expect(errors[0]).toEqual("Email address is required");
                    })
                    .then(done)
                    .catch(done.fail);
            });

            it("should disable the register button with an invalid email", done => {
                // Wait for stability since ngModel is async
                fixture.whenStable()
                    .then(() => {
                        let body = fixture.debugElement.query(By.css(".modal-body"));
                        expect(body).not.toBeNull();

                        let form = body.query(By.css("form"));
                        expect(form).not.toBeNull();

                        setInputValue(form, "username", "someUsername");
                        setInputValue(form, "email", "notAnEmail");
                        setInputValue(form, "password", "somePassword");
                        setInputValue(form, "confirmPassword", "somePassword");

                        fixture.detectChanges();
                        let button = form.query(By.css("button"));
                        expect(button).not.toBeNull();
                        expect(button.properties.disabled).toEqual(true);

                        let errors = getAllErrors();
                        expect(errors.length).toEqual(1);
                        expect(errors[0]).toEqual("Must be a valid email address");
                    })
                    .then(done)
                    .catch(done.fail);
            });
        });

        describe("password", () => {
            it("should disable the register button with a missing password", done => {
                // Wait for stability since ngModel is async
                fixture.whenStable()
                    .then(() => {
                        let body = fixture.debugElement.query(By.css(".modal-body"));
                        expect(body).not.toBeNull();

                        let form = body.query(By.css("form"));
                        expect(form).not.toBeNull();

                        setInputValue(form, "username", "someUsername");
                        setInputValue(form, "email", "someEmail@someDomain.com");

                        fixture.detectChanges();
                        let button = form.query(By.css("button"));
                        expect(button).not.toBeNull();
                        expect(button.properties.disabled).toEqual(true);

                        let errors = getAllErrors();
                        expect(errors.length).toEqual(0);
                    })
                    .then(done)
                    .catch(done.fail);
            });

            it("should disable the register button with an empty password", done => {
                // Wait for stability since ngModel is async
                fixture.whenStable()
                    .then(() => {
                        let body = fixture.debugElement.query(By.css(".modal-body"));
                        expect(body).not.toBeNull();

                        let form = body.query(By.css("form"));
                        expect(form).not.toBeNull();

                        setInputValue(form, "username", "someUsername");
                        setInputValue(form, "email", "someEmail@someDomain.com");
                        setInputValue(form, "password", "somePassword");
                        setInputValue(form, "password", "");

                        fixture.detectChanges();
                        let button = form.query(By.css("button"));
                        expect(button).not.toBeNull();
                        expect(button.properties.disabled).toEqual(true);

                        let errors = getAllErrors();
                        expect(errors.length).toEqual(1);
                        expect(errors[0]).toEqual("Password is required");
                    })
                    .then(done)
                    .catch(done.fail);
            });

            it("should disable the register button with a short password", done => {
                // Wait for stability since ngModel is async
                fixture.whenStable()
                    .then(() => {
                        let body = fixture.debugElement.query(By.css(".modal-body"));
                        expect(body).not.toBeNull();

                        let form = body.query(By.css("form"));
                        expect(form).not.toBeNull();

                        setInputValue(form, "username", "someUsername");
                        setInputValue(form, "email", "someEmail@someDomain.com");
                        setInputValue(form, "password", "a");

                        fixture.detectChanges();
                        let button = form.query(By.css("button"));
                        expect(button).not.toBeNull();
                        expect(button.properties.disabled).toEqual(true);

                        let errors = getAllErrors();
                        expect(errors.length).toEqual(1);
                        expect(errors[0]).toEqual("Password must be at least 4 characters long");
                    })
                    .then(done)
                    .catch(done.fail);
            });
        });

        describe("password confirmation", () => {
            it("should disable the register button with a missing password confirmation", done => {
                // Wait for stability since ngModel is async
                fixture.whenStable()
                    .then(() => {
                        let body = fixture.debugElement.query(By.css(".modal-body"));
                        expect(body).not.toBeNull();

                        let form = body.query(By.css("form"));
                        expect(form).not.toBeNull();

                        setInputValue(form, "username", "someUsername");
                        setInputValue(form, "email", "someEmail@someDomain.com");
                        setInputValue(form, "password", "somePassword");

                        fixture.detectChanges();
                        let button = form.query(By.css("button"));
                        expect(button).not.toBeNull();
                        expect(button.properties.disabled).toEqual(true);

                        let errors = getAllErrors();
                        expect(errors.length).toEqual(0);
                    })
                    .then(done)
                    .catch(done.fail);
            });

            it("should disable the register button with a non-matching password confirmation", done => {
                // Wait for stability since ngModel is async
                fixture.whenStable()
                    .then(() => {
                        let body = fixture.debugElement.query(By.css(".modal-body"));
                        expect(body).not.toBeNull();

                        let form = body.query(By.css("form"));
                        expect(form).not.toBeNull();

                        setInputValue(form, "username", "someUsername");
                        setInputValue(form, "email", "someEmail@someDomain.com");
                        setInputValue(form, "password", "somePassword");
                        setInputValue(form, "confirmPassword", "someOtherPassword");

                        fixture.detectChanges();
                        let button = form.query(By.css("button"));
                        expect(button).not.toBeNull();
                        expect(button.properties.disabled).toEqual(true);

                        let errors = getAllErrors();
                        expect(errors.length).toEqual(1);
                        expect(errors[0]).toEqual("Passwords don't match");
                    })
                    .then(done)
                    .catch(done.fail);
            });

            it("should disable the register button when the password is changed to not match the password confirmation", done => {
                // Wait for stability since ngModel is async
                fixture.whenStable()
                    .then(() => {
                        let body = fixture.debugElement.query(By.css(".modal-body"));
                        expect(body).not.toBeNull();

                        let form = body.query(By.css("form"));
                        expect(form).not.toBeNull();

                        setInputValue(form, "username", "someUsername");
                        setInputValue(form, "email", "someEmail@someDomain.com");
                        setInputValue(form, "password", "somePassword");
                        setInputValue(form, "confirmPassword", "somePassword");
                        setInputValue(form, "password", "someOtherPassword");

                        fixture.detectChanges();
                        let button = form.query(By.css("button"));
                        expect(button).not.toBeNull();
                        expect(button.properties.disabled).toEqual(true);

                        let errors = getAllErrors();
                        expect(errors.length).toEqual(1);
                        expect(errors[0]).toEqual("Passwords don't match");
                    })
                    .then(done)
                    .catch(done.fail);
            });
        });
    });

    describe("Form submission", () => {
        it("should close the dialog when registering properly", done => {
            let userService = TestBed.get(UserService) as UserService;
            spyOn(userService, "create").and.returnValue(Promise.resolve());

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

                    setInputValue(form, "username", "someUsername");
                    setInputValue(form, "email", "someEmail@someDomain.com");
                    setInputValue(form, "password", "somePassword");
                    setInputValue(form, "confirmPassword", "somePassword");
                    return submit(form);
                })
                .then(() => {
                    expect(userService.create).toHaveBeenCalledWith("someUsername", "someEmail@someDomain.com", "somePassword");
                    expect(authenticationService.logInWithPassword).toHaveBeenCalledWith("someUsername", "somePassword");
                    expect(activeModal.close).toHaveBeenCalled();

                    let errors = getAllErrors();
                    expect(errors.length).toEqual(0);
                })
                .then(done)
                .catch(done.fail);
        });

        it("should show an error when user creation fails", done => {
            let userService = TestBed.get(UserService) as UserService;
            spyOn(userService, "create").and.returnValue(Promise.reject(["error0", "error1", "error2"]));

            let authenticationService = TestBed.get(AuthenticationService) as AuthenticationService;
            spyOn(authenticationService, "logInWithPassword");

            let activeModal = TestBed.get(NgbActiveModal) as NgbActiveModal;
            spyOn(activeModal, "close");

            // Wait for stability since ngModel is async
            fixture.whenStable()
                .then(() => {
                    let body = fixture.debugElement.query(By.css(".modal-body"));
                    expect(body).not.toBeNull();

                    let form = body.query(By.css("form"));
                    expect(form).not.toBeNull();

                    setInputValue(form, "username", "someUsername");
                    setInputValue(form, "email", "someEmail@someDomain.com");
                    setInputValue(form, "password", "somePassword");
                    setInputValue(form, "confirmPassword", "somePassword");
                    return submit(form);
                })
                .then(() => {
                    expect(userService.create).toHaveBeenCalledWith("someUsername", "someEmail@someDomain.com", "somePassword");
                    expect(authenticationService.logInWithPassword).not.toHaveBeenCalled();
                    expect(activeModal.close).not.toHaveBeenCalled();

                    let errors = getAllErrors();
                    expect(errors.length).toEqual(3);
                    expect(errors[0]).toEqual("error0");
                    expect(errors[1]).toEqual("error1");
                    expect(errors[2]).toEqual("error2");
                })
                .then(done)
                .catch(done.fail);
        });

        it("should show an error when login after creation fails", done => {
            let userService = TestBed.get(UserService) as UserService;
            spyOn(userService, "create").and.returnValue(Promise.resolve());

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

                    setInputValue(form, "username", "someUsername");
                    setInputValue(form, "email", "someEmail@someDomain.com");
                    setInputValue(form, "password", "somePassword");
                    setInputValue(form, "confirmPassword", "somePassword");
                    return submit(form);
                })
                .then(() => {
                    expect(userService.create).toHaveBeenCalledWith("someUsername", "someEmail@someDomain.com", "somePassword");
                    expect(authenticationService.logInWithPassword).toHaveBeenCalledWith("someUsername", "somePassword");
                    expect(activeModal.close).not.toHaveBeenCalled();

                    let errors = getAllErrors();
                    expect(errors.length).toEqual(1);
                    expect(errors[0]).toEqual("Something went wrong. Your account was created but but we had trouble logging you in. Please try logging in with your new account.");
                })
                .then(done)
                .catch(done.fail);
        });

        function submit(form: DebugElement): Promise<void> {
            fixture.detectChanges();
            let button = form.query(By.css("button"));
            expect(button).not.toBeNull();
            button.nativeElement.click();

            return fixture.whenStable()
                .then(() => fixture.detectChanges());
        }
    });

    function setInputValue(form: DebugElement, id: string, value: string): void {
        fixture.detectChanges();
        let element = form.query(By.css("#" + id));
        expect(element).not.toBeNull();
        element.nativeElement.value = value;

        // Tell Angular
        let evt = document.createEvent("CustomEvent");
        evt.initCustomEvent("input", false, false, null);
        element.nativeElement.dispatchEvent(evt);
    }

    function getAllErrors(): string[] {
        let errors: string[] = [];
        let errorElements = fixture.debugElement.queryAll(By.css(".alert-danger>div"));
        for (let i = 0; i < errorElements.length; i++) {
            errors.push(errorElements[i].nativeElement.textContent.trim());
        }

        return errors;
    }
});
