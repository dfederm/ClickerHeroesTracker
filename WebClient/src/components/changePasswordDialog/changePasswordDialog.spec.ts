import { ComponentFixture, TestBed } from "@angular/core/testing";
import { By } from "@angular/platform-browser";
import { FormsModule } from "@angular/forms";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { DebugElement, NO_ERRORS_SCHEMA } from "@angular/core";
import { BehaviorSubject } from "rxjs";
import { CompareValidatorModule } from "angular-compare-validator";

import { ChangePasswordDialogComponent } from "./changePasswordDialog";
import { AuthenticationService, IUserInfo } from "../../services/authenticationService/authenticationService";
import { UserService, IUserLogins } from "../../services/userService/userService";

describe("ChangePasswordDialogComponent", () => {
    let fixture: ComponentFixture<ChangePasswordDialogComponent>;

    const loggedInUser: IUserInfo = {
        isLoggedIn: true,
        id: "someId",
        username: "someUsername",
        email: "someEmail",
    };

    const loginsWithPassword: IUserLogins = {
        hasPassword: true,
        externalLogins: [],
    };

    const loginsWithoutPassword: IUserLogins = {
        hasPassword: false,
        externalLogins: [],
    };

    beforeEach(done => {
        let userInfo = new BehaviorSubject(loggedInUser);
        let authenticationService = { userInfo: () => userInfo };
        let userService = {
            getLogins: (): void => void 0,
            setPassword: (): void => void 0,
            changePassword: (): void => void 0,
        };
        let activeModal = { close: (): void => void 0 };

        TestBed.configureTestingModule(
            {
                imports: [
                    FormsModule,
                    CompareValidatorModule,
                ],
                declarations: [ChangePasswordDialogComponent],
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
                fixture = TestBed.createComponent(ChangePasswordDialogComponent);
            })
            .then(done)
            .catch(done.fail);
    });

    it("should show the current password field when the user has a password", done => {
        setUserLogins(loginsWithPassword)
            .then(() => {
                let body = fixture.debugElement.query(By.css(".modal-body"));
                expect(body).not.toBeNull();

                let form = body.query(By.css("form"));
                expect(form).not.toBeNull();

                let element = form.query(By.css("#currentPassword"));
                expect(element).not.toBeNull();
            })
            .then(done)
            .catch(done.fail);
    });

    it("should hide the current password field when the user doesn't have a password", done => {
        setUserLogins(loginsWithoutPassword)
            .then(() => {
                let body = fixture.debugElement.query(By.css(".modal-body"));
                expect(body).not.toBeNull();

                let form = body.query(By.css("form"));
                expect(form).not.toBeNull();

                let element = form.query(By.css("#currentPassword"));
                expect(element).toBeNull();
            })
            .then(done)
            .catch(done.fail);
    });

    describe("Validation", () => {
        it("should disable the submit button initially", done => {
            setUserLogins(loginsWithPassword)
                .then(() => {
                    let body = fixture.debugElement.query(By.css(".modal-body"));
                    expect(body).not.toBeNull();

                    let form = body.query(By.css("form"));
                    expect(form).not.toBeNull();

                    let button = form.query(By.css("button"));
                    expect(button).not.toBeNull();
                    expect(button.properties.disabled).toEqual(true);

                    let errors = getAllErrors();
                    expect(errors.length).toEqual(0);
                })
                .then(done)
                .catch(done.fail);
        });

        it("should enable the submit button when all inputs are valid", done => {
            setUserLogins(loginsWithPassword)
                .then(() => {
                    let body = fixture.debugElement.query(By.css(".modal-body"));
                    expect(body).not.toBeNull();

                    let form = body.query(By.css("form"));
                    expect(form).not.toBeNull();

                    setInputValue(form, "currentPassword", "someCurrentPassword");
                    setInputValue(form, "newPassword", "someNewPassword");
                    setInputValue(form, "confirmNewPassword", "someNewPassword");

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

        describe("Current Password", () => {
            it("should disable the submit button with a missing the current password", done => {
                setUserLogins(loginsWithPassword)
                    .then(() => {
                        let body = fixture.debugElement.query(By.css(".modal-body"));
                        expect(body).not.toBeNull();

                        let form = body.query(By.css("form"));
                        expect(form).not.toBeNull();

                        setInputValue(form, "newPassword", "someNewPassword");
                        setInputValue(form, "confirmNewPassword", "someNewPassword");

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

            it("should disable the submit button with an empty current password", done => {
                setUserLogins(loginsWithPassword)
                    .then(() => {
                        let body = fixture.debugElement.query(By.css(".modal-body"));
                        expect(body).not.toBeNull();

                        let form = body.query(By.css("form"));
                        expect(form).not.toBeNull();

                        setInputValue(form, "currentPassword", "someCurrentPassword");
                        setInputValue(form, "currentPassword", "");
                        setInputValue(form, "newPassword", "someNewPassword");
                        setInputValue(form, "confirmNewPassword", "someNewPassword");

                        fixture.detectChanges();
                        let button = form.query(By.css("button"));
                        expect(button).not.toBeNull();
                        expect(button.properties.disabled).toEqual(true);

                        let errors = getAllErrors();
                        expect(errors.length).toEqual(1);
                        expect(errors[0]).toEqual("Current Password is required");
                    })
                    .then(done)
                    .catch(done.fail);
            });
        });

        describe("New Password", () => {
            it("should disable the submit button with a missing new password", done => {
                setUserLogins(loginsWithPassword)
                    .then(() => {
                        let body = fixture.debugElement.query(By.css(".modal-body"));
                        expect(body).not.toBeNull();

                        let form = body.query(By.css("form"));
                        expect(form).not.toBeNull();

                        setInputValue(form, "currentPassword", "someCurrentPassword");

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

            it("should disable the submit button with an empty new password", done => {
                setUserLogins(loginsWithPassword)
                    .then(() => {
                        let body = fixture.debugElement.query(By.css(".modal-body"));
                        expect(body).not.toBeNull();

                        let form = body.query(By.css("form"));
                        expect(form).not.toBeNull();

                        setInputValue(form, "currentPassword", "someCurrentPassword");
                        setInputValue(form, "newPassword", "someNewPassword");
                        setInputValue(form, "newPassword", "");

                        fixture.detectChanges();
                        let button = form.query(By.css("button"));
                        expect(button).not.toBeNull();
                        expect(button.properties.disabled).toEqual(true);

                        let errors = getAllErrors();
                        expect(errors.length).toEqual(1);
                        expect(errors[0]).toEqual("New Password is required");
                    })
                    .then(done)
                    .catch(done.fail);
            });

            it("should disable the submit button with a short new password", done => {
                setUserLogins(loginsWithPassword)
                    .then(() => {
                        let body = fixture.debugElement.query(By.css(".modal-body"));
                        expect(body).not.toBeNull();

                        let form = body.query(By.css("form"));
                        expect(form).not.toBeNull();

                        setInputValue(form, "currentPassword", "someCurrentPassword");
                        setInputValue(form, "newPassword", "a");

                        fixture.detectChanges();
                        let button = form.query(By.css("button"));
                        expect(button).not.toBeNull();
                        expect(button.properties.disabled).toEqual(true);

                        let errors = getAllErrors();
                        expect(errors.length).toEqual(1);
                        expect(errors[0]).toEqual("New Password must be at least 4 characters long");
                    })
                    .then(done)
                    .catch(done.fail);
            });
        });

        describe("New Password Confirmation", () => {
            it("should disable the submit button with a missing new password confirmation", done => {
                setUserLogins(loginsWithPassword)
                    .then(() => {
                        let body = fixture.debugElement.query(By.css(".modal-body"));
                        expect(body).not.toBeNull();

                        let form = body.query(By.css("form"));
                        expect(form).not.toBeNull();

                        setInputValue(form, "currentPassword", "someCurrentPassword");
                        setInputValue(form, "newPassword", "someNewPassword");

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

            it("should disable the submit button with a non-matching new password confirmation", done => {
                setUserLogins(loginsWithPassword)
                    .then(() => {
                        let body = fixture.debugElement.query(By.css(".modal-body"));
                        expect(body).not.toBeNull();

                        let form = body.query(By.css("form"));
                        expect(form).not.toBeNull();

                        setInputValue(form, "currentPassword", "someCurrentPassword");
                        setInputValue(form, "newPassword", "someNewPassword");
                        setInputValue(form, "confirmNewPassword", "someOtherNewPassword");

                        fixture.detectChanges();
                        let button = form.query(By.css("button"));
                        expect(button).not.toBeNull();
                        expect(button.properties.disabled).toEqual(true);

                        let errors = getAllErrors();
                        expect(errors.length).toEqual(1);
                        expect(errors[0]).toEqual("New Passwords don't match");
                    })
                    .then(done)
                    .catch(done.fail);
            });

            it("should disable the submit button when the new password is changed to not match the new password confirmation", done => {
                setUserLogins(loginsWithPassword)
                    .then(() => {
                        let body = fixture.debugElement.query(By.css(".modal-body"));
                        expect(body).not.toBeNull();

                        let form = body.query(By.css("form"));
                        expect(form).not.toBeNull();

                        setInputValue(form, "currentPassword", "someCurrentPassword");
                        setInputValue(form, "newPassword", "someNewPassword");
                        setInputValue(form, "confirmNewPassword", "someNewPassword");
                        setInputValue(form, "newPassword", "someOtherNewPassword");

                        fixture.detectChanges();
                        let button = form.query(By.css("button"));
                        expect(button).not.toBeNull();
                        expect(button.properties.disabled).toEqual(true);

                        let errors = getAllErrors();
                        expect(errors.length).toEqual(1);
                        expect(errors[0]).toEqual("New Passwords don't match");
                    })
                    .then(done)
                    .catch(done.fail);
            });
        });
    });

    describe("Form submission", () => {
        it("should close the dialog when changing the password properly", done => {
            let userService = TestBed.get(UserService) as UserService;
            spyOn(userService, "changePassword").and.returnValue(Promise.resolve());

            let activeModal = TestBed.get(NgbActiveModal) as NgbActiveModal;
            spyOn(activeModal, "close");

            setUserLogins(loginsWithPassword)
                .then(() => {
                    let body = fixture.debugElement.query(By.css(".modal-body"));
                    expect(body).not.toBeNull();

                    let form = body.query(By.css("form"));
                    expect(form).not.toBeNull();

                    setInputValue(form, "currentPassword", "someCurrentPassword");
                    setInputValue(form, "newPassword", "someNewPassword");
                    setInputValue(form, "confirmNewPassword", "someNewPassword");
                    return submit(form);
                })
                .then(() => {
                    expect(userService.changePassword).toHaveBeenCalledWith("someUsername", "someCurrentPassword", "someNewPassword");
                    expect(activeModal.close).toHaveBeenCalled();

                    let errors = getAllErrors();
                    expect(errors.length).toEqual(0);
                })
                .then(done)
                .catch(done.fail);
        });

        it("should close the dialog when adding a new password properly", done => {
            let userService = TestBed.get(UserService) as UserService;
            spyOn(userService, "setPassword").and.returnValue(Promise.resolve());

            let activeModal = TestBed.get(NgbActiveModal) as NgbActiveModal;
            spyOn(activeModal, "close");

            setUserLogins(loginsWithoutPassword)
                .then(() => {
                    let body = fixture.debugElement.query(By.css(".modal-body"));
                    expect(body).not.toBeNull();

                    let form = body.query(By.css("form"));
                    expect(form).not.toBeNull();

                    setInputValue(form, "newPassword", "someNewPassword");
                    setInputValue(form, "confirmNewPassword", "someNewPassword");
                    return submit(form);
                })
                .then(() => {
                    expect(userService.setPassword).toHaveBeenCalledWith("someUsername", "someNewPassword");
                    expect(activeModal.close).toHaveBeenCalled();

                    let errors = getAllErrors();
                    expect(errors.length).toEqual(0);
                })
                .then(done)
                .catch(done.fail);
        });

        it("should show an error when password change fails", done => {
            let userService = TestBed.get(UserService) as UserService;
            spyOn(userService, "changePassword").and.returnValue(Promise.reject(["error0", "error1", "error2"]));

            let activeModal = TestBed.get(NgbActiveModal) as NgbActiveModal;
            spyOn(activeModal, "close");

            setUserLogins(loginsWithPassword)
                .then(() => {
                    let body = fixture.debugElement.query(By.css(".modal-body"));
                    expect(body).not.toBeNull();

                    let form = body.query(By.css("form"));
                    expect(form).not.toBeNull();

                    setInputValue(form, "currentPassword", "someCurrentPassword");
                    setInputValue(form, "newPassword", "someNewPassword");
                    setInputValue(form, "confirmNewPassword", "someNewPassword");
                    return submit(form);
                })
                .then(() => {
                    expect(userService.changePassword).toHaveBeenCalledWith("someUsername", "someCurrentPassword", "someNewPassword");
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

        it("should show an error when adding a password fails", done => {
            let userService = TestBed.get(UserService) as UserService;
            spyOn(userService, "setPassword").and.returnValue(Promise.reject(["error0", "error1", "error2"]));

            let activeModal = TestBed.get(NgbActiveModal) as NgbActiveModal;
            spyOn(activeModal, "close");

            setUserLogins(loginsWithoutPassword)
                .then(() => {
                    let body = fixture.debugElement.query(By.css(".modal-body"));
                    expect(body).not.toBeNull();

                    let form = body.query(By.css("form"));
                    expect(form).not.toBeNull();

                    setInputValue(form, "newPassword", "someNewPassword");
                    setInputValue(form, "confirmNewPassword", "someNewPassword");
                    return submit(form);
                })
                .then(() => {
                    expect(userService.setPassword).toHaveBeenCalledWith("someUsername", "someNewPassword");
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

        function submit(form: DebugElement): Promise<void> {
            fixture.detectChanges();
            let button = form.query(By.css("button"));
            expect(button).not.toBeNull();
            button.nativeElement.click();

            return fixture.whenStable()
                .then(() => fixture.detectChanges());
        }
    });

    function setUserLogins(logins: IUserLogins): Promise<void> {
        let userService = TestBed.get(UserService) as UserService;
        spyOn(userService, "getLogins").and.returnValue(Promise.resolve(logins));

        // First allow the getLogins promise to finish
        fixture.detectChanges();
        return fixture.whenStable()
            .then(() => {
                // Trigger the form to render
                fixture.detectChanges();

                // Wait for stability again since ngModel is async
                return fixture.whenStable();
            })
            .then(() => {
                // Bind yet again since validation-related bindings depend on the ngModel values.
                fixture.detectChanges();
            });
    }

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
