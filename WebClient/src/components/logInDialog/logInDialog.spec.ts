import { ComponentFixture, TestBed } from "@angular/core/testing";
import { By } from "@angular/platform-browser";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { Component, DebugElement, Input } from "@angular/core";

import { LogInDialogComponent } from "./logInDialog";
import { AuthenticationService } from "../../services/authenticationService/authenticationService";
import { ExternalLoginsComponent } from "../externalLogins/externalLogins";
import { NgxSpinnerModule } from "ngx-spinner";

describe("LogInDialogComponent", () => {
    let fixture: ComponentFixture<LogInDialogComponent>;

    @Component({ selector: "ngx-spinner", template: "" })
    class MockNgxSpinnerComponent {
        @Input()
        public fullScreen: boolean;
    }
    
    @Component({selector: "externalLogins", template: ""})
    class MockExternalLoginsComponent { }
    
    beforeEach(async () => {
        let authenticationService = {
            logInWithPassword: (): void => void 0,
        };
        let activeModal = { close: (): void => void 0 };

        await TestBed.configureTestingModule(
            {
                imports: [
                    LogInDialogComponent,
                ],
                providers: [
                    { provide: AuthenticationService, useValue: authenticationService },
                    { provide: NgbActiveModal, useValue: activeModal },
                ],
            })
            .compileComponents();
        TestBed.overrideComponent(LogInDialogComponent, {
            remove: { imports: [NgxSpinnerModule, ExternalLoginsComponent]},
            add: { imports: [MockNgxSpinnerComponent, MockExternalLoginsComponent]},
        });

        fixture = TestBed.createComponent(LogInDialogComponent);
        
        fixture.detectChanges();
    });

    it("should display the modal header", () => {
        let header = fixture.debugElement.query(By.css(".modal-header"));
        expect(header).not.toBeNull();

        let title = header.query(By.css(".modal-title"));
        expect(title).not.toBeNull();
        expect(title.nativeElement.textContent).toEqual("Log in");
    });

    it("should close the dialog when using proper credentials", async () => {
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
        await fixture.whenStable();
        fixture.detectChanges();

        expect(authenticationService.logInWithPassword).toHaveBeenCalledWith("someUsername", "somePassword");
        expect(activeModal.close).toHaveBeenCalled();

        // No error
        let error = fixture.debugElement.query(By.css(".alert-danger"));
        expect(error).toBeNull();
    });

    it("should show an error when using incorrect credentials", async () => {
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
        await fixture.whenStable();
        fixture.detectChanges();

        expect(authenticationService.logInWithPassword).toHaveBeenCalledWith("someUsername", "somePassword");

        let error = fixture.debugElement.query(By.css(".alert-danger"));
        expect(error).not.toBeNull();
        expect(activeModal.close).not.toHaveBeenCalled();
    });

    function setInputValue(element: DebugElement, value: string): void {
        element.nativeElement.value = value;

        // Tell Angular
        let evt = document.createEvent("CustomEvent");
        evt.initCustomEvent("input", false, false, null);
        element.nativeElement.dispatchEvent(evt);
    }
});
