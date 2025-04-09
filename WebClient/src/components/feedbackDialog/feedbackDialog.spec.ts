import { ComponentFixture, TestBed } from "@angular/core/testing";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { By } from "@angular/platform-browser";
import { Component, DebugElement, Input } from "@angular/core";
import { BehaviorSubject } from "rxjs";

import { FeedbackDialogComponent } from "./feedbackDialog";
import { AuthenticationService, IUserInfo } from "../../services/authenticationService/authenticationService";
import { FeedbackService } from "../../services/feedbackService/feedbackService";
import { NgxSpinnerModule } from "ngx-spinner";

describe("FeedbackDialogComponent", () => {
    let component: FeedbackDialogComponent;

    @Component({ selector: "ngx-spinner", template: "" })
    class MockNgxSpinnerComponent {
        @Input()
        public fullScreen: boolean;
    }

    let fixture: ComponentFixture<FeedbackDialogComponent>;
    let userInfo: BehaviorSubject<IUserInfo>;

    const loggedInUser: IUserInfo = {
        isLoggedIn: true,
        id: "someId",
        username: "someUsername",
        email: "someEmail",
    };

    const notLoggedInUser: IUserInfo = {
        isLoggedIn: false,
    };

    beforeEach(async () => {
        userInfo = new BehaviorSubject(notLoggedInUser);
        let authenticationService = { userInfo: () => userInfo };
        let feedbackService = {
            send: (): void => void 0,
        };
        let activeModal = { close: (): void => void 0 };

        await TestBed.configureTestingModule(
            {
                imports: [
                    FeedbackDialogComponent,
                ],
                providers: [
                    { provide: AuthenticationService, useValue: authenticationService },
                    { provide: FeedbackService, useValue: feedbackService },
                    { provide: NgbActiveModal, useValue: activeModal },
                ],
            })
            .compileComponents();
        TestBed.overrideComponent(FeedbackDialogComponent, {
            remove: { imports: [ NgxSpinnerModule ]},
            add: { imports: [ MockNgxSpinnerComponent ] },
        });

        fixture = TestBed.createComponent(FeedbackDialogComponent);
        component = fixture.componentInstance;

        fixture.detectChanges();
    });

    it("should display the modal header", () => {
        let header = fixture.debugElement.query(By.css(".modal-header"));
        expect(header).not.toBeNull();

        let title = header.query(By.css(".modal-title"));
        expect(title).not.toBeNull();
        expect(title.nativeElement.textContent).toEqual("Feedback");
    });

    it("should display the email field when the user is not logged in", () => {
        userInfo.next(notLoggedInUser);
        fixture.detectChanges();

        let body = fixture.debugElement.query(By.css(".modal-body"));
        expect(body).not.toBeNull();

        let form = body.query(By.css("form"));
        expect(form).not.toBeNull();

        let email = body.query(By.css("#email"));
        expect(email).not.toBeNull();
    });

    it("should not display the email field when the user is logged in", () => {
        userInfo.next(loggedInUser);
        fixture.detectChanges();

        let body = fixture.debugElement.query(By.css(".modal-body"));
        expect(body).not.toBeNull();

        let form = body.query(By.css("form"));
        expect(form).not.toBeNull();

        let email = body.query(By.css("#email"));
        expect(email).toBeNull();
    });

    describe("form validation", () => {
        it("should disable the submit button initially", async () => {
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

        it("should enable the register button when the user is not logged in and all inputs are valid", async () => {
            userInfo.next(notLoggedInUser);
            fixture.detectChanges();

            // Wait for stability since ngModel is async
            await fixture.whenStable();
            let body = fixture.debugElement.query(By.css(".modal-body"));
            expect(body).not.toBeNull();

            let form = body.query(By.css("form"));
            expect(form).not.toBeNull();

            setInputValue(form, "comments", "someComments");
            setInputValue(form, "email", "someEmail@someDomain.com");

            fixture.detectChanges();
            let button = form.query(By.css("button"));
            expect(button).not.toBeNull();
            expect(button.properties.disabled).toEqual(false);

            let errors = getAllErrors();
            expect(errors.length).toEqual(0);
        });

        it("should enable the register button when the user is logged in and all inputs are valid", async () => {
            userInfo.next(loggedInUser);
            fixture.detectChanges();

            // Wait for stability since ngModel is async
            await fixture.whenStable();

            let body = fixture.debugElement.query(By.css(".modal-body"));
            expect(body).not.toBeNull();

            let form = body.query(By.css("form"));
            expect(form).not.toBeNull();

            setInputValue(form, "comments", "someComments");

            fixture.detectChanges();
            let button = form.query(By.css("button"));
            expect(button).not.toBeNull();
            expect(button.properties.disabled).toEqual(false);

            let errors = getAllErrors();
            expect(errors.length).toEqual(0);
        });

        describe("comments", () => {
            it("should disable the submit button with missing comments", async () => {
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
                expect(button.properties.disabled).toEqual(true);

                let errors = getAllErrors();
                expect(errors.length).toEqual(0);
            });

            it("should disable the submit button with empty comments", async () => {
                // Wait for stability since ngModel is async
                await fixture.whenStable();

                let body = fixture.debugElement.query(By.css(".modal-body"));
                expect(body).not.toBeNull();

                let form = body.query(By.css("form"));
                expect(form).not.toBeNull();

                setInputValue(form, "comments", "someComments");
                setInputValue(form, "comments", "");
                setInputValue(form, "email", "someEmail@someDomain.com");

                fixture.detectChanges();
                let button = form.query(By.css("button"));
                expect(button).not.toBeNull();
                expect(button.properties.disabled).toEqual(true);

                let errors = getAllErrors();
                expect(errors.length).toEqual(1);
                expect(errors[0]).toEqual("Comments are required");
            });
        });

        describe("email", () => {
            it("should disable the submit button with a missing email", async () => {
                // Wait for stability since ngModel is async
                await fixture.whenStable();

                let body = fixture.debugElement.query(By.css(".modal-body"));
                expect(body).not.toBeNull();

                let form = body.query(By.css("form"));
                expect(form).not.toBeNull();

                setInputValue(form, "comments", "someComments");

                fixture.detectChanges();
                let button = form.query(By.css("button"));
                expect(button).not.toBeNull();
                expect(button.properties.disabled).toEqual(true);

                let errors = getAllErrors();
                expect(errors.length).toEqual(0);
            });

            it("should disable the submit button with an empty email", async () => {
                // Wait for stability since ngModel is async
                await fixture.whenStable();

                let body = fixture.debugElement.query(By.css(".modal-body"));
                expect(body).not.toBeNull();

                let form = body.query(By.css("form"));
                expect(form).not.toBeNull();

                setInputValue(form, "comments", "someComments");
                setInputValue(form, "email", "someEmail@someDomain.com");
                setInputValue(form, "email", "");

                fixture.detectChanges();
                let button = form.query(By.css("button"));
                expect(button).not.toBeNull();
                expect(button.properties.disabled).toEqual(true);

                let errors = getAllErrors();
                expect(errors.length).toEqual(1);
                expect(errors[0]).toEqual("Must be a valid email address");
            });

            it("should disable the submit button with an invalid email", async () => {
                // Wait for stability since ngModel is async
                await fixture.whenStable();

                let body = fixture.debugElement.query(By.css(".modal-body"));
                expect(body).not.toBeNull();

                let form = body.query(By.css("form"));
                expect(form).not.toBeNull();

                setInputValue(form, "comments", "someComments");
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
    });

    describe("form submission", () => {
        it("should send correct feedback data when user is not logged in", async () => {
            let feedbackService = TestBed.inject(FeedbackService);
            spyOn(feedbackService, "send").and.returnValue(Promise.resolve());

            let activeModal = TestBed.inject(NgbActiveModal);
            spyOn(activeModal, "close");

            userInfo.next(notLoggedInUser);
            fixture.detectChanges();

            // Wait for stability since ngModel is async
            await fixture.whenStable();

            let body = fixture.debugElement.query(By.css(".modal-body"));
            expect(body).not.toBeNull();

            let form = body.query(By.css("form"));
            expect(form).not.toBeNull();

            setInputValue(form, "comments", "someComments");
            setInputValue(form, "email", "someEmail@someDomain.com");

            fixture.detectChanges();
            let button = form.query(By.css("button"));
            expect(button).not.toBeNull();

            button.nativeElement.click();

            // Wait for stability from the feedbackService promise
            fixture.detectChanges();
            await fixture.whenStable();

            expect(feedbackService.send).toHaveBeenCalledWith("someComments", "someEmail@someDomain.com");
            expect(component.errorMessage).toBeFalsy();
            expect(activeModal.close).toHaveBeenCalled();
        });

        it("should send correct feedback data when user is logged in", async () => {
            let feedbackService = TestBed.inject(FeedbackService);
            spyOn(feedbackService, "send").and.returnValue(Promise.resolve());

            let activeModal = TestBed.inject(NgbActiveModal);
            spyOn(activeModal, "close");

            userInfo.next(loggedInUser);
            fixture.detectChanges();

            // Wait for stability since ngModel is async
            await fixture.whenStable();

            let body = fixture.debugElement.query(By.css(".modal-body"));
            expect(body).not.toBeNull();

            let form = body.query(By.css("form"));
            expect(form).not.toBeNull();

            setInputValue(form, "comments", "someComments");

            fixture.detectChanges();
            let button = form.query(By.css("button"));
            expect(button).not.toBeNull();

            button.nativeElement.click();

            // Wait for stability from the feedbackService promise
            fixture.detectChanges();
            await fixture.whenStable();

            expect(feedbackService.send).toHaveBeenCalledWith("someComments", loggedInUser.email);
            expect(component.errorMessage).toBeFalsy();
            expect(activeModal.close).toHaveBeenCalled();
        });

        it("should show an error when feedbackService fails", async () => {
            let feedbackService = TestBed.inject(FeedbackService);
            spyOn(feedbackService, "send").and.returnValue(Promise.reject(null));

            let activeModal = TestBed.inject(NgbActiveModal);
            spyOn(activeModal, "close");

            userInfo.next(loggedInUser);
            fixture.detectChanges();

            // Wait for stability since ngModel is async
            await fixture.whenStable();

            let body = fixture.debugElement.query(By.css(".modal-body"));
            expect(body).not.toBeNull();

            let form = body.query(By.css("form"));
            expect(form).not.toBeNull();

            setInputValue(form, "comments", "someComments");

            fixture.detectChanges();
            let button = form.query(By.css("button"));
            expect(button).not.toBeNull();

            button.nativeElement.click();

            // Wait for stability from the feedbackService promise
            fixture.detectChanges();
            await fixture.whenStable();

            expect(component.errorMessage).toBeTruthy();
            expect(activeModal.close).not.toHaveBeenCalled();
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

    function getAllErrors(): string[] {
        let errors: string[] = [];
        let errorElements = fixture.debugElement.queryAll(By.css(".alert-danger"));
        for (let i = 0; i < errorElements.length; i++) {
            errors.push(errorElements[i].nativeElement.textContent.trim());
        }

        return errors;
    }
});
