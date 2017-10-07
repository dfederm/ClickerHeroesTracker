import { ComponentFixture, TestBed } from "@angular/core/testing";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { By } from "@angular/platform-browser";
import { FormsModule } from "@angular/forms";
import { NO_ERRORS_SCHEMA, DebugElement } from "@angular/core";
import { BehaviorSubject } from "rxjs/BehaviorSubject";

import { FeedbackDialogComponent } from "./feedbackDialog";
import { AuthenticationService, IUserInfo } from "../../services/authenticationService/authenticationService";
import { FeedbackService } from "../../services/feedbackService/feedbackService";

describe("FeedbackDialogComponent", () => {
    let component: FeedbackDialogComponent;
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

    beforeEach(done => {
        userInfo = new BehaviorSubject(notLoggedInUser);
        let authenticationService = { userInfo: () => userInfo };
        let feedbackService = {
            send: (): void => void 0,
        };
        let activeModal = { close: (): void => void 0 };

        TestBed.configureTestingModule(
            {
                imports: [FormsModule],
                declarations: [FeedbackDialogComponent],
                providers:
                [
                    { provide: AuthenticationService, useValue: authenticationService },
                    { provide: FeedbackService, useValue: feedbackService },
                    { provide: NgbActiveModal, useValue: activeModal },
                ],
                schemas: [NO_ERRORS_SCHEMA],
            })
            .compileComponents()
            .then(() => {
                fixture = TestBed.createComponent(FeedbackDialogComponent);
                component = fixture.componentInstance;

                fixture.detectChanges();
            })
            .then(done)
            .catch(done.fail);
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
        it("should disable the submit button initially", done => {
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

        it("should enable the register button when the user is not logged in and all inputs are valid", done => {
            userInfo.next(notLoggedInUser);
            fixture.detectChanges();

            // Wait for stability since ngModel is async
            fixture.whenStable()
                .then(() => {
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
                })
                .then(done)
                .catch(done.fail);
        });

        it("should enable the register button when the user is logged in and all inputs are valid", done => {
            userInfo.next(loggedInUser);
            fixture.detectChanges();

            // Wait for stability since ngModel is async
            fixture.whenStable()
                .then(() => {
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
                })
                .then(done)
                .catch(done.fail);
        });

        describe("comments", () => {
            it("should disable the submit button with missing comments", done => {
                // Wait for stability since ngModel is async
                fixture.whenStable()
                    .then(() => {
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
                    })
                    .then(done)
                    .catch(done.fail);
            });

            it("should disable the submit button with empty comments", done => {
                // Wait for stability since ngModel is async
                fixture.whenStable()
                    .then(() => {
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
                    })
                    .then(done)
                    .catch(done.fail);
            });
        });

        describe("email", () => {
            it("should disable the submit button with a missing email", done => {
                // Wait for stability since ngModel is async
                fixture.whenStable()
                    .then(() => {
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
                    })
                    .then(done)
                    .catch(done.fail);
            });

            it("should disable the submit button with an empty email", done => {
                // Wait for stability since ngModel is async
                fixture.whenStable()
                    .then(() => {
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
                    })
                    .then(done)
                    .catch(done.fail);
            });

            it("should disable the submit button with an invalid email", done => {
                // Wait for stability since ngModel is async
                fixture.whenStable()
                    .then(() => {
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
                    })
                    .then(done)
                    .catch(done.fail);
            });
        });
    });

    describe("form submission", () => {
        it("should send correct feedback data when user is not logged in", done => {
            let feedbackService = TestBed.get(FeedbackService) as FeedbackService;
            spyOn(feedbackService, "send").and.returnValue(Promise.resolve());

            let activeModal = TestBed.get(NgbActiveModal) as NgbActiveModal;
            spyOn(activeModal, "close");

            userInfo.next(notLoggedInUser);
            fixture.detectChanges();

            // Wait for stability since ngModel is async
            fixture.whenStable()
                .then(() => {
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
                    return fixture.whenStable();
                })
                .then(() => {
                    expect(feedbackService.send).toHaveBeenCalledWith("someComments", "someEmail@someDomain.com");

                    expect(component.errorMessage).toBeFalsy();
                    expect(activeModal.close).toHaveBeenCalled();
                })
                .then(done)
                .catch(done.fail);
        });

        it("should send correct feedback data when user is logged in", done => {
            let feedbackService = TestBed.get(FeedbackService) as FeedbackService;
            spyOn(feedbackService, "send").and.returnValue(Promise.resolve());

            let activeModal = TestBed.get(NgbActiveModal) as NgbActiveModal;
            spyOn(activeModal, "close");

            userInfo.next(loggedInUser);
            fixture.detectChanges();

            // Wait for stability since ngModel is async
            fixture.whenStable()
                .then(() => {
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
                    return fixture.whenStable();
                })
                .then(() => {
                    expect(feedbackService.send).toHaveBeenCalledWith("someComments", loggedInUser.email);

                    expect(component.errorMessage).toBeFalsy();
                    expect(activeModal.close).toHaveBeenCalled();
                })
                .then(done)
                .catch(done.fail);
        });

        it("should show an error when feedbackService fails", done => {
            let feedbackService = TestBed.get(FeedbackService) as FeedbackService;
            spyOn(feedbackService, "send").and.returnValue(Promise.reject(null));

            let activeModal = TestBed.get(NgbActiveModal) as NgbActiveModal;
            spyOn(activeModal, "close");

            userInfo.next(loggedInUser);
            fixture.detectChanges();

            // Wait for stability since ngModel is async
            fixture.whenStable()
                .then(() => {
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
                    return fixture.whenStable();
                })
                .then(() => {
                    expect(component.errorMessage).toBeTruthy();
                    expect(activeModal.close).not.toHaveBeenCalled();
                })
                .then(done)
                .catch(done.fail);
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
