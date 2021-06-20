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

    beforeEach(async () => {
        let authenticationService = {
            logInWithPassword: (): void => void 0,
        };
        let userService = {
            create: (): void => void 0,
        };
        let activeModal = { close: (): void => void 0 };

        await TestBed.configureTestingModule(
            {
                imports: [
                    FormsModule,
                    ValidateEqualModule,
                ],
                declarations: [
                    RegisterDialogComponent,
                ],
                providers: [
                    { provide: AuthenticationService, useValue: authenticationService },
                    { provide: UserService, useValue: userService },
                    { provide: NgbActiveModal, useValue: activeModal },
                ],
                schemas: [NO_ERRORS_SCHEMA],
            })
            .compileComponents();

        fixture = TestBed.createComponent(RegisterDialogComponent);
        fixture.detectChanges();
    });

    describe("Validation", () => {
        it("should disable the register button initially", async () => {
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

        it("should enable the register button when all inputs are valid", async () => {
            // Wait for stability since ngModel is async
            await fixture.whenStable();

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
        });

        describe("username", () => {
            it("should disable the register button with a missing username", async () => {
                // Wait for stability since ngModel is async
                await fixture.whenStable();

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
            });

            it("should disable the register button with an empty username", async () => {
                // Wait for stability since ngModel is async
                await fixture.whenStable();

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
            });

            it("should disable the register button with a short username", async () => {
                // Wait for stability since ngModel is async
                await fixture.whenStable();

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
            });
        });

        describe("email", () => {
            it("should disable the register button with a missing email", async () => {
                // Wait for stability since ngModel is async
                await fixture.whenStable();

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
            });

            it("should disable the register button with an empty email", async () => {
                // Wait for stability since ngModel is async
                await fixture.whenStable();

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
            });

            it("should disable the register button with an invalid email", async () => {
                // Wait for stability since ngModel is async
                await fixture.whenStable();

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

                setInputValue(form, "username", "someUsername");
                setInputValue(form, "email", "someEmail@someDomain.com");

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
            });

            it("should disable the register button with a short password", async () => {
                // Wait for stability since ngModel is async
                await fixture.whenStable();

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

                setInputValue(form, "username", "someUsername");
                setInputValue(form, "email", "someEmail@someDomain.com");
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
            });

            it("should disable the register button when the password is changed to not match the password confirmation", async () => {
                // Wait for stability since ngModel is async
                await fixture.whenStable();

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
            });
        });
    });

    describe("Form submission", () => {
        it("should close the dialog when registering properly", async () => {
            let userService = TestBed.inject(UserService);
            spyOn(userService, "create").and.returnValue(Promise.resolve());

            let authenticationService = TestBed.inject(AuthenticationService);
            spyOn(authenticationService, "logInWithPassword").and.returnValue(Promise.resolve());

            let activeModal = TestBed.inject(NgbActiveModal);
            spyOn(activeModal, "close");

            // Wait for stability since ngModel is async
            await fixture.whenStable();

            let body = fixture.debugElement.query(By.css(".modal-body"));
            expect(body).not.toBeNull();

            let form = body.query(By.css("form"));
            expect(form).not.toBeNull();

            setInputValue(form, "username", "someUsername");
            setInputValue(form, "email", "someEmail@someDomain.com");
            setInputValue(form, "password", "somePassword");
            setInputValue(form, "confirmPassword", "somePassword");

            await submit(form);

            expect(userService.create).toHaveBeenCalledWith("someUsername", "someEmail@someDomain.com", "somePassword");
            expect(authenticationService.logInWithPassword).toHaveBeenCalledWith("someUsername", "somePassword");
            expect(activeModal.close).toHaveBeenCalled();

            let errors = getAllErrors();
            expect(errors.length).toEqual(0);
        });

        it("should show an error when user creation fails", async () => {
            let userService = TestBed.inject(UserService);
            spyOn(userService, "create").and.returnValue(Promise.reject(["error0", "error1", "error2"]));

            let authenticationService = TestBed.inject(AuthenticationService);
            spyOn(authenticationService, "logInWithPassword");

            let activeModal = TestBed.inject(NgbActiveModal);
            spyOn(activeModal, "close");

            // Wait for stability since ngModel is async
            await fixture.whenStable();

            let body = fixture.debugElement.query(By.css(".modal-body"));
            expect(body).not.toBeNull();

            let form = body.query(By.css("form"));
            expect(form).not.toBeNull();

            setInputValue(form, "username", "someUsername");
            setInputValue(form, "email", "someEmail@someDomain.com");
            setInputValue(form, "password", "somePassword");
            setInputValue(form, "confirmPassword", "somePassword");

            await submit(form);

            expect(userService.create).toHaveBeenCalledWith("someUsername", "someEmail@someDomain.com", "somePassword");
            expect(authenticationService.logInWithPassword).not.toHaveBeenCalled();
            expect(activeModal.close).not.toHaveBeenCalled();

            let errors = getAllErrors();
            expect(errors.length).toEqual(3);
            expect(errors[0]).toEqual("error0");
            expect(errors[1]).toEqual("error1");
            expect(errors[2]).toEqual("error2");
        });

        it("should show an error when login after creation fails", async () => {
            let userService = TestBed.inject(UserService);
            spyOn(userService, "create").and.returnValue(Promise.resolve());

            let authenticationService = TestBed.inject(AuthenticationService);
            spyOn(authenticationService, "logInWithPassword").and.returnValue(Promise.reject(""));

            let activeModal = TestBed.inject(NgbActiveModal);
            spyOn(activeModal, "close");

            // Wait for stability since ngModel is async
            await fixture.whenStable();

            let body = fixture.debugElement.query(By.css(".modal-body"));
            expect(body).not.toBeNull();

            let form = body.query(By.css("form"));
            expect(form).not.toBeNull();

            setInputValue(form, "username", "someUsername");
            setInputValue(form, "email", "someEmail@someDomain.com");
            setInputValue(form, "password", "somePassword");
            setInputValue(form, "confirmPassword", "somePassword");

            await submit(form);

            expect(userService.create).toHaveBeenCalledWith("someUsername", "someEmail@someDomain.com", "somePassword");
            expect(authenticationService.logInWithPassword).toHaveBeenCalledWith("someUsername", "somePassword");
            expect(activeModal.close).not.toHaveBeenCalled();

            let errors = getAllErrors();
            expect(errors.length).toEqual(1);
            expect(errors[0]).toEqual("Something went wrong. Your account was created but but we had trouble logging you in. Please try logging in with your new account.");
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
