import { ComponentFixture, TestBed } from "@angular/core/testing";
import { By } from "@angular/platform-browser";
import { FormsModule } from "@angular/forms";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { DebugElement, NO_ERRORS_SCHEMA } from "@angular/core";

import { LogInDialogComponent } from "./logInDialog";
import { AuthenticationService } from "../../services/authenticationService/authenticationService";

describe("LogInDialogComponent", () => {
    let fixture: ComponentFixture<LogInDialogComponent>;

    beforeEach(done => {
        let authenticationService = {
            logInWithPassword: (): void => void 0,
        };
        let activeModal = { close: (): void => void 0 };

        TestBed.configureTestingModule(
            {
                imports: [FormsModule],
                declarations: [LogInDialogComponent],
                providers:
                    [
                        { provide: AuthenticationService, useValue: authenticationService },
                        { provide: NgbActiveModal, useValue: activeModal },
                    ],
                schemas: [NO_ERRORS_SCHEMA],
            })
            .compileComponents()
            .then(() => {
                fixture = TestBed.createComponent(LogInDialogComponent);

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
        expect(title.nativeElement.textContent).toEqual("Log in");
    });

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
