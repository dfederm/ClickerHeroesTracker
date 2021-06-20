import { ComponentFixture, TestBed } from "@angular/core/testing";
import { FormsModule } from "@angular/forms";
import { NgbActiveModal, NgbModal } from "@ng-bootstrap/ng-bootstrap";
import { ValidateEqualModule } from "ng-validate-equal";

import { ResetPasswordDialogComponent } from "./resetPasswordDialog";
import { UserService } from "../../services/userService/userService";
import { By } from "@angular/platform-browser";
import { DebugElement, NO_ERRORS_SCHEMA } from "@angular/core";

describe("ResetPasswordDialogComponent", () => {
    let component: ResetPasswordDialogComponent;
    let fixture: ComponentFixture<ResetPasswordDialogComponent>;

    beforeEach(async () => {
        let userService = {
            resetPassword: (): void => void 0,
            resetPasswordConfirmation: (): void => void 0,
        };
        let activeModal = { close: (): void => void 0 };
        let modalService = { open: (): void => void 0 };

        await TestBed.configureTestingModule(
            {
                imports: [
                    FormsModule,
                    ValidateEqualModule,
                ],
                declarations: [
                    ResetPasswordDialogComponent,
                ],
                providers: [
                    { provide: UserService, useValue: userService },
                    { provide: NgbActiveModal, useValue: activeModal },
                    { provide: NgbModal, useValue: modalService },
                ],
                schemas: [NO_ERRORS_SCHEMA],
            })
            .compileComponents();

        fixture = TestBed.createComponent(ResetPasswordDialogComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    describe("Send code form", () => {
        describe("Validation", () => {
            it("should disable the send code button initially", async () => {
                // Wait for stability since ngModel is async
                await fixture.whenStable();

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
            });

            it("should enable the send code button when the email is valid", async () => {
                // Wait for stability since ngModel is async
                await fixture.whenStable();

                let body = fixture.debugElement.query(By.css(".modal-body"));
                expect(body).not.toBeNull();

                let form = body.query(By.css("form"));
                expect(form).not.toBeNull();

                setInputValue(form, "email", "someEmail@someDomain.com");

                fixture.detectChanges();
                let button = form.query(By.css("button"));
                expect(button).not.toBeNull();
                expect(button.properties.disabled).toEqual(false);

                let errors = getAllErrors();
                expect(errors.length).toEqual(0);
            });

            it("should disable the send code button with an empty email", async () => {
                // Wait for stability since ngModel is async
                await fixture.whenStable();

                let body = fixture.debugElement.query(By.css(".modal-body"));
                expect(body).not.toBeNull();

                let form = body.query(By.css("form"));
                expect(form).not.toBeNull();

                setInputValue(form, "email", "someEmail@someDomain.com");
                setInputValue(form, "email", "");

                fixture.detectChanges();
                let button = form.query(By.css("button"));
                expect(button).not.toBeNull();
                expect(button.properties.disabled).toEqual(true);

                let errors = getAllErrors();
                expect(errors.length).toEqual(1);
                expect(errors[0]).toEqual("Email address is required");
            });

            it("should disable the send code button with an invalid email", async () => {
                // Wait for stability since ngModel is async
                await fixture.whenStable();

                let body = fixture.debugElement.query(By.css(".modal-body"));
                expect(body).not.toBeNull();

                let form = body.query(By.css("form"));
                expect(form).not.toBeNull();

                setInputValue(form, "email", "notAnEmail");

                fixture.detectChanges();
                let button = form.query(By.css("button"));
                expect(button).not.toBeNull();
                expect(button.properties.disabled).toEqual(true);

                let errors = getAllErrors();
                expect(errors.length).toEqual(1);
                expect(errors[0]).toEqual("Must be a valid email address");
            });
        });

        describe("Form submission", () => {
            it("should send the code when the form is filled out properly", async () => {
                let userService = TestBed.inject(UserService);
                spyOn(userService, "resetPassword").and.returnValue(Promise.resolve());

                // Wait for stability since ngModel is async
                await fixture.whenStable();

                let body = fixture.debugElement.query(By.css(".modal-body"));
                expect(body).not.toBeNull();

                let form = body.query(By.css("form"));
                expect(form).not.toBeNull();

                setInputValue(form, "email", "someEmail@someDomain.com");

                await submit(form);
                expect(userService.resetPassword).toHaveBeenCalledWith("someEmail@someDomain.com");
                expect(component.codeSent).toEqual(true);

                let errors = getAllErrors();
                expect(errors.length).toEqual(0);
            });

            it("should show an error when sending the code fails", async () => {
                let userService = TestBed.inject(UserService);
                spyOn(userService, "resetPassword").and.returnValue(Promise.reject(["error0", "error1", "error2"]));

                // Wait for stability since ngModel is async
                await fixture.whenStable();

                let body = fixture.debugElement.query(By.css(".modal-body"));
                expect(body).not.toBeNull();

                let form = body.query(By.css("form"));
                expect(form).not.toBeNull();

                setInputValue(form, "email", "someEmail@someDomain.com");

                await submit(form);
                expect(userService.resetPassword).toHaveBeenCalledWith("someEmail@someDomain.com");
                expect(component.codeSent).toEqual(false);

                let errors = getAllErrors();
                expect(errors.length).toEqual(3);
                expect(errors[0]).toEqual("error0");
                expect(errors[1]).toEqual("error1");
                expect(errors[2]).toEqual("error2");
            });
        });
    });

    describe("Reset form", () => {
        beforeEach(() => {
            component.email = "someEmail@someDomain.com";
            component.codeSent = true;
            fixture.detectChanges();
        });

        describe("Validation", () => {
            it("should disable the reset password button initially", async () => {
                // Wait for stability since ngModel is async
                await fixture.whenStable();

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
            });

            it("should enable the reset password button when all inputs are valid", async () => {
                // Wait for stability since ngModel is async
                await fixture.whenStable();

                let body = fixture.debugElement.query(By.css(".modal-body"));
                expect(body).not.toBeNull();

                let form = body.query(By.css("form"));
                expect(form).not.toBeNull();

                setInputValue(form, "code", "someCode");
                setInputValue(form, "password", "somePassword");
                setInputValue(form, "confirmPassword", "somePassword");

                fixture.detectChanges();
                let button = form.query(By.css("button"));
                expect(button).not.toBeNull();
                expect(button.properties.disabled).toEqual(false);

                let errors = getAllErrors();
                expect(errors.length).toEqual(0);
            });

            describe("code", () => {
                it("should disable the register button with a missing code", async () => {
                    // Wait for stability since ngModel is async
                    await fixture.whenStable();

                    let body = fixture.debugElement.query(By.css(".modal-body"));
                    expect(body).not.toBeNull();

                    let form = body.query(By.css("form"));
                    expect(form).not.toBeNull();

                    setInputValue(form, "password", "somePassword");
                    setInputValue(form, "confirmPassword", "somePassword");

                    fixture.detectChanges();
                    let button = form.query(By.css("button"));
                    expect(button).not.toBeNull();
                    expect(button.properties.disabled).toEqual(true);

                    let errors = getAllErrors();
                    expect(errors.length).toEqual(0);
                });

                it("should disable the register button with an empty code", async () => {
                    // Wait for stability since ngModel is async
                    await fixture.whenStable();

                    let body = fixture.debugElement.query(By.css(".modal-body"));
                    expect(body).not.toBeNull();

                    let form = body.query(By.css("form"));
                    expect(form).not.toBeNull();

                    setInputValue(form, "code", "someCode");
                    setInputValue(form, "code", "");
                    setInputValue(form, "password", "somePassword");
                    setInputValue(form, "confirmPassword", "somePassword");

                    fixture.detectChanges();
                    let button = form.query(By.css("button"));
                    expect(button).not.toBeNull();
                    expect(button.properties.disabled).toEqual(true);

                    let errors = getAllErrors();
                    expect(errors.length).toEqual(1);
                    expect(errors[0]).toEqual("Reset code is required");
                });
            });

            describe("password", () => {
                it("should disable the register button with a missing password", async () => {
                    // Wait for stability since ngModel is async
                    await fixture.whenStable();

                    let body = fixture.debugElement.query(By.css(".modal-body"));
                    expect(body).not.toBeNull();

                    let form = body.query(By.css("form"));
                    expect(form).not.toBeNull();

                    setInputValue(form, "code", "someCode");

                    fixture.detectChanges();
                    let button = form.query(By.css("button"));
                    expect(button).not.toBeNull();
                    expect(button.properties.disabled).toEqual(true);

                    let errors = getAllErrors();
                    expect(errors.length).toEqual(0);
                });

                it("should disable the register button with an empty password", async () => {
                    // Wait for stability since ngModel is async
                    await fixture.whenStable();

                    let body = fixture.debugElement.query(By.css(".modal-body"));
                    expect(body).not.toBeNull();

                    let form = body.query(By.css("form"));
                    expect(form).not.toBeNull();

                    setInputValue(form, "code", "someCode");
                    setInputValue(form, "password", "somePassword");
                    setInputValue(form, "password", "");

                    fixture.detectChanges();
                    let button = form.query(By.css("button"));
                    expect(button).not.toBeNull();
                    expect(button.properties.disabled).toEqual(true);

                    let errors = getAllErrors();
                    expect(errors.length).toEqual(1);
                    expect(errors[0]).toEqual("Password is required");
                });

                it("should disable the register button with a short password", async () => {
                    // Wait for stability since ngModel is async
                    await fixture.whenStable();

                    let body = fixture.debugElement.query(By.css(".modal-body"));
                    expect(body).not.toBeNull();

                    let form = body.query(By.css("form"));
                    expect(form).not.toBeNull();

                    setInputValue(form, "code", "someCode");
                    setInputValue(form, "password", "a");

                    fixture.detectChanges();
                    let button = form.query(By.css("button"));
                    expect(button).not.toBeNull();
                    expect(button.properties.disabled).toEqual(true);

                    let errors = getAllErrors();
                    expect(errors.length).toEqual(1);
                    expect(errors[0]).toEqual("Password must be at least 4 characters long");
                });
            });

            describe("password confirmation", () => {
                it("should disable the register button with a missing password confirmation", async () => {
                    // Wait for stability since ngModel is async
                    await fixture.whenStable();

                    let body = fixture.debugElement.query(By.css(".modal-body"));
                    expect(body).not.toBeNull();

                    let form = body.query(By.css("form"));
                    expect(form).not.toBeNull();

                    setInputValue(form, "code", "someCode");
                    setInputValue(form, "password", "somePassword");

                    fixture.detectChanges();
                    let button = form.query(By.css("button"));
                    expect(button).not.toBeNull();
                    expect(button.properties.disabled).toEqual(true);

                    let errors = getAllErrors();
                    expect(errors.length).toEqual(0);
                });

                it("should disable the register button with a non-matching password confirmation", async () => {
                    // Wait for stability since ngModel is async
                    await fixture.whenStable();

                    let body = fixture.debugElement.query(By.css(".modal-body"));
                    expect(body).not.toBeNull();

                    let form = body.query(By.css("form"));
                    expect(form).not.toBeNull();

                    setInputValue(form, "code", "someCode");
                    setInputValue(form, "password", "somePassword");
                    setInputValue(form, "confirmPassword", "someOtherPassword");

                    fixture.detectChanges();
                    let button = form.query(By.css("button"));
                    expect(button).not.toBeNull();
                    expect(button.properties.disabled).toEqual(true);

                    let errors = getAllErrors();
                    expect(errors.length).toEqual(1);
                    expect(errors[0]).toEqual("Passwords don't match");
                });

                it("should disable the register button when the password is changed to not match the password confirmation", async () => {
                    // Wait for stability since ngModel is async
                    await fixture.whenStable();

                    let body = fixture.debugElement.query(By.css(".modal-body"));
                    expect(body).not.toBeNull();

                    let form = body.query(By.css("form"));
                    expect(form).not.toBeNull();

                    setInputValue(form, "code", "someCode");
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
                });
            });
        });

        describe("Form submission", () => {
            it("should reset the password when the form is filled out properly", async () => {
                let userService = TestBed.inject(UserService);
                spyOn(userService, "resetPasswordConfirmation").and.returnValue(Promise.resolve());

                let activeModal = TestBed.inject(NgbActiveModal);
                spyOn(activeModal, "close");

                let modalService = TestBed.inject(NgbModal);
                spyOn(modalService, "open");

                // Wait for stability since ngModel is async
                await fixture.whenStable();

                let body = fixture.debugElement.query(By.css(".modal-body"));
                expect(body).not.toBeNull();

                let form = body.query(By.css("form"));
                expect(form).not.toBeNull();

                setInputValue(form, "code", "someCode");
                setInputValue(form, "password", "somePassword");
                setInputValue(form, "confirmPassword", "somePassword");

                await submit(form);
                expect(userService.resetPasswordConfirmation).toHaveBeenCalledWith("someEmail@someDomain.com", "somePassword", "someCode");
                expect(activeModal.close).toHaveBeenCalled();
                expect(modalService.open).toHaveBeenCalled();

                let errors = getAllErrors();
                expect(errors.length).toEqual(0);
            });

            it("should show an error when reset password fails", async () => {
                let userService = TestBed.inject(UserService);
                spyOn(userService, "resetPasswordConfirmation").and.returnValue(Promise.reject(["error0", "error1", "error2"]));

                let activeModal = TestBed.inject(NgbActiveModal);
                spyOn(activeModal, "close");

                let modalService = TestBed.inject(NgbModal);
                spyOn(modalService, "open");

                // Wait for stability since ngModel is async
                await fixture.whenStable();

                let body = fixture.debugElement.query(By.css(".modal-body"));
                expect(body).not.toBeNull();

                let form = body.query(By.css("form"));
                expect(form).not.toBeNull();

                setInputValue(form, "code", "someCode");
                setInputValue(form, "password", "somePassword");
                setInputValue(form, "confirmPassword", "somePassword");

                await submit(form);
                expect(userService.resetPasswordConfirmation).toHaveBeenCalledWith("someEmail@someDomain.com", "somePassword", "someCode");
                expect(activeModal.close).not.toHaveBeenCalled();
                expect(modalService.open).not.toHaveBeenCalled();

                let errors = getAllErrors();
                expect(errors.length).toEqual(3);
                expect(errors[0]).toEqual("error0");
                expect(errors[1]).toEqual("error1");
                expect(errors[2]).toEqual("error2");
            });
        });
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

    async function submit(form: DebugElement): Promise<void> {
        fixture.detectChanges();
        let button = form.query(By.css("button"));
        expect(button).not.toBeNull();
        button.nativeElement.click();

        await fixture.whenStable();
        return fixture.detectChanges();
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
