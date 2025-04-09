import { ComponentFixture, TestBed } from "@angular/core/testing";
import { By } from "@angular/platform-browser";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { Component, DebugElement, Input } from "@angular/core";
import { BehaviorSubject } from "rxjs";

import { ChangePasswordDialogComponent } from "./changePasswordDialog";
import { AuthenticationService, IUserInfo } from "../../services/authenticationService/authenticationService";
import { UserService, IUserLogins } from "../../services/userService/userService";
import { NgxSpinnerModule, NgxSpinnerService } from "ngx-spinner";
import { ExternalLoginsComponent } from "../externalLogins/externalLogins";

describe("ChangePasswordDialogComponent", () => {
    let fixture: ComponentFixture<ChangePasswordDialogComponent>;

    @Component({
        selector: "ngx-spinner",
        template: "",
    })
    class MockNgxSpinnerComponent {
        @Input()
        public fullScreen: boolean;
    }

    @Component({
        selector: "externalLogins",
        template: "",
    })
    class MockExternalLoginsComponent {
        @Input()
        public isManageMode: boolean;
    }

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

    beforeEach(async () => {
        let userInfo = new BehaviorSubject(loggedInUser);
        let authenticationService = { userInfo: () => userInfo };
        let userService = {
            getLogins: (): void => void 0,
            setPassword: (): void => void 0,
            changePassword: (): void => void 0,
        };
        let activeModal = { close: (): void => void 0 };
        let spinnerService = {
            show: (): void => void 0,
            hide: (): void => void 0,
        };

        await TestBed.configureTestingModule(
            {
                imports: [
                    ChangePasswordDialogComponent,
                ],
                providers: [
                    { provide: AuthenticationService, useValue: authenticationService },
                    { provide: UserService, useValue: userService },
                    { provide: NgbActiveModal, useValue: activeModal },
                    { provide: NgxSpinnerService, useValue: spinnerService },
                ],
            })
            .compileComponents();
        TestBed.overrideComponent(ChangePasswordDialogComponent, {
            remove: { imports: [NgxSpinnerModule, ExternalLoginsComponent]},
            add: { imports: [MockNgxSpinnerComponent, MockExternalLoginsComponent]},
        });

        fixture = TestBed.createComponent(ChangePasswordDialogComponent);
    });

    it("should show the current password field when the user has a password", async () => {
        await setUserLogins(loginsWithPassword);

        let body = fixture.debugElement.query(By.css(".modal-body"));
        expect(body).not.toBeNull();

        let form = body.query(By.css("form"));
        expect(form).not.toBeNull();

        let element = form.query(By.css("#currentPassword"));
        expect(element).not.toBeNull();
    });

    it("should hide the current password field when the user doesn't have a password", async () => {
        await setUserLogins(loginsWithoutPassword);

        let body = fixture.debugElement.query(By.css(".modal-body"));
        expect(body).not.toBeNull();

        let form = body.query(By.css("form"));
        expect(form).not.toBeNull();

        let element = form.query(By.css("#currentPassword"));
        expect(element).toBeNull();
    });

    describe("Validation", () => {
        it("should disable the submit button initially", async () => {
            await setUserLogins(loginsWithPassword);

            let body = fixture.debugElement.query(By.css(".modal-body"));
            expect(body).not.toBeNull();

            let form = body.query(By.css("form"));
            expect(form).not.toBeNull();

            let button = form.query(By.css("button"));
            expect(button).not.toBeNull();
            expect(button.properties.disabled).toEqual(true);

            let errors = getAllErrors();
            expect(errors.length).toEqual(0);
        });

        it("should enable the submit button when all inputs are valid", async () => {
            await setUserLogins(loginsWithPassword);

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
        });

        describe("Current Password", () => {
            it("should disable the submit button with a missing the current password", async () => {
                await setUserLogins(loginsWithPassword);

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
            });

            it("should disable the submit button with an empty current password", async () => {
                await setUserLogins(loginsWithPassword);

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
            });
        });

        describe("New Password", () => {
            it("should disable the submit button with a missing new password", async () => {
                await setUserLogins(loginsWithPassword);

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
            });

            it("should disable the submit button with an empty new password", async () => {
                await setUserLogins(loginsWithPassword);

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
            });

            it("should disable the submit button with a short new password", async () => {
                await setUserLogins(loginsWithPassword);

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
            });
        });

        describe("New Password Confirmation", () => {
            it("should disable the submit button with a missing new password confirmation", async () => {
                await setUserLogins(loginsWithPassword);

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
            });

            it("should disable the submit button with a non-matching new password confirmation", async () => {
                await setUserLogins(loginsWithPassword);

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
            });

            it("should disable the submit button when the new password is changed to not match the new password confirmation", async () => {
                await setUserLogins(loginsWithPassword);

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
            });
        });
    });

    describe("Form submission", () => {
        it("should close the dialog when changing the password properly", async () => {
            let userService = TestBed.inject(UserService);
            spyOn(userService, "changePassword").and.returnValue(Promise.resolve());

            let activeModal = TestBed.inject(NgbActiveModal);
            spyOn(activeModal, "close");

            await setUserLogins(loginsWithPassword);

            let body = fixture.debugElement.query(By.css(".modal-body"));
            expect(body).not.toBeNull();

            let form = body.query(By.css("form"));
            expect(form).not.toBeNull();

            setInputValue(form, "currentPassword", "someCurrentPassword");
            setInputValue(form, "newPassword", "someNewPassword");
            setInputValue(form, "confirmNewPassword", "someNewPassword");
            await submit(form);

            expect(userService.changePassword).toHaveBeenCalledWith("someUsername", "someCurrentPassword", "someNewPassword");
            expect(activeModal.close).toHaveBeenCalled();

            let errors = getAllErrors();
            expect(errors.length).toEqual(0);
        });

        it("should close the dialog when adding a new password properly", async () => {
            let userService = TestBed.inject(UserService);
            spyOn(userService, "setPassword").and.returnValue(Promise.resolve());

            let activeModal = TestBed.inject(NgbActiveModal);
            spyOn(activeModal, "close");

            await setUserLogins(loginsWithoutPassword);

            let body = fixture.debugElement.query(By.css(".modal-body"));
            expect(body).not.toBeNull();

            let form = body.query(By.css("form"));
            expect(form).not.toBeNull();

            setInputValue(form, "newPassword", "someNewPassword");
            setInputValue(form, "confirmNewPassword", "someNewPassword");
            await submit(form);

            expect(userService.setPassword).toHaveBeenCalledWith("someUsername", "someNewPassword");
            expect(activeModal.close).toHaveBeenCalled();

            let errors = getAllErrors();
            expect(errors.length).toEqual(0);
        });

        it("should show an error when password change fails", async () => {
            let userService = TestBed.inject(UserService);
            spyOn(userService, "changePassword").and.returnValue(Promise.reject(["error0", "error1", "error2"]));

            let activeModal = TestBed.inject(NgbActiveModal);
            spyOn(activeModal, "close");

            await setUserLogins(loginsWithPassword);

            let body = fixture.debugElement.query(By.css(".modal-body"));
            expect(body).not.toBeNull();

            let form = body.query(By.css("form"));
            expect(form).not.toBeNull();

            setInputValue(form, "currentPassword", "someCurrentPassword");
            setInputValue(form, "newPassword", "someNewPassword");
            setInputValue(form, "confirmNewPassword", "someNewPassword");
            await submit(form);

            expect(userService.changePassword).toHaveBeenCalledWith("someUsername", "someCurrentPassword", "someNewPassword");
            expect(activeModal.close).not.toHaveBeenCalled();

            let errors = getAllErrors();
            expect(errors.length).toEqual(3);
            expect(errors[0]).toEqual("error0");
            expect(errors[1]).toEqual("error1");
            expect(errors[2]).toEqual("error2");
        });

        it("should show an error when adding a password fails", async () => {
            let userService = TestBed.inject(UserService);
            spyOn(userService, "setPassword").and.returnValue(Promise.reject(["error0", "error1", "error2"]));

            let activeModal = TestBed.inject(NgbActiveModal);
            spyOn(activeModal, "close");

            await setUserLogins(loginsWithoutPassword);

            let body = fixture.debugElement.query(By.css(".modal-body"));
            expect(body).not.toBeNull();

            let form = body.query(By.css("form"));
            expect(form).not.toBeNull();

            setInputValue(form, "newPassword", "someNewPassword");
            setInputValue(form, "confirmNewPassword", "someNewPassword");
            await submit(form);

            expect(userService.setPassword).toHaveBeenCalledWith("someUsername", "someNewPassword");
            expect(activeModal.close).not.toHaveBeenCalled();

            let errors = getAllErrors();
            expect(errors.length).toEqual(3);
            expect(errors[0]).toEqual("error0");
            expect(errors[1]).toEqual("error1");
            expect(errors[2]).toEqual("error2");
        });

        async function submit(form: DebugElement): Promise<void> {
            fixture.detectChanges();
            let button = form.query(By.css("button"));
            expect(button).not.toBeNull();
            button.nativeElement.click();

            await fixture.whenStable();
            return fixture.detectChanges();
        }
    });

    async function setUserLogins(logins: IUserLogins): Promise<void> {
        let userService = TestBed.inject(UserService);
        spyOn(userService, "getLogins").and.returnValue(Promise.resolve(logins));

        // First allow the getLogins promise to finish
        fixture.detectChanges();
        await fixture.whenStable()

        // Trigger the form to render
        fixture.detectChanges();

        // Wait for stability again since ngModel is async
        await fixture.whenStable();

        // Bind yet again since validation-related bindings depend on the ngModel values.
        fixture.detectChanges();
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
